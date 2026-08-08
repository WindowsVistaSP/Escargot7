using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System.Security.Cryptography;

using System.Net.Sockets;
using System.Text;

using System.IO;
using System.IO.IsolatedStorage;

using System.Threading;
using System.Collections.Generic;

namespace MSN_Protocol
{

    public class SocketClient//TCPClient好像被巨硬阉割了,网上也没找到什么好的替代方案,所以这个类用AI写了(
    {
        /*
        public delegate void ErrorMesChanged(SocketClient sender, ErrorMesEventArgs args);
        public event ErrorMesChanged ErrorMesEvent;
        */

        private Socket socket = null;

        // 接收缓冲区
        private MemoryStream receiveBuffer = new MemoryStream();
        private readonly object bufferLock = new object();

        // 接收状态控制
        private bool isReceiving = false;
        private Queue<string> messageQueue = new Queue<string>();
        private readonly object queueLock = new object();

        // 事件通知
        public event Action<string> LineReceived;
        public event Action<string> ErrorOccurred;
        public event Action ConnectionClosed;

        private const int TIMEOUT_MILLISECONDS = 5000;
        private const int MAX_BUFFER_SIZE = 20480;
        /*
        private void Error_Mes(string mes)//出错了
        {
            if (ErrorMesEvent != null)
            {
                ErrorMesEvent(this, new ErrorMesEventArgs(mes));
            }
        }*/
        private void ReConnect(string host, int port)
        {
            Close();
            Connect(host, port);
        }
        /// <summary>
        /// 连接到服务器
        /// </summary>
        public string Connect(string hostName, int portNumber)
        {
            string result = string.Empty;
            ManualResetEvent connectDone = new ManualResetEvent(false);

            try
            {
                DnsEndPoint hostEntry = new DnsEndPoint(hostName, portNumber);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
                socketEventArg.RemoteEndPoint = hostEntry;
                socketEventArg.Completed += (s, e) =>
                {
                    try
                    {
                        result = e.SocketError.ToString();
                        connectDone.Set();
                    }
                    catch
                    {
                        ReConnect(hostName, portNumber);
                    }
                };

                connectDone.Reset();
                socket.ConnectAsync(socketEventArg);

                if (!connectDone.WaitOne(TIMEOUT_MILLISECONDS))
                {
                    result = "Connection Timeout";
                }

                socketEventArg.Dispose();
            }
            catch (Exception ex)
            {
                result = "Connect Error: " + ex.Message;
            }
            finally
            {
                connectDone.Dispose();
            }

            // 连接成功后立即开始持续接收
            if (result == "Success")
            {
                StartReceiving();
            }

            return result;
        }

        /// <summary>
        /// 启动持续接收
        /// </summary>
        private void StartReceiving()
        {
            lock (bufferLock)
            {
                if (isReceiving || socket == null || !socket.Connected)
                    return;

                isReceiving = true;
                BeginReceive();
            }
        }

        /// <summary>
        /// 开始一次异步接收
        /// </summary>
        private void BeginReceive()
        {
            if (!isReceiving || socket == null || !socket.Connected)
            {
                isReceiving = false;
                return;
            }

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            byte[] buffer = new byte[MAX_BUFFER_SIZE];
            args.SetBuffer(buffer, 0, MAX_BUFFER_SIZE);
            args.Completed += OnReceiveCompleted;

            try
            {
                bool willRaiseEvent = socket.ReceiveAsync(args);
                if (!willRaiseEvent)
                {
                    // 同步完成，直接处理
                    ProcessReceiveResult(args);
                }
            }
            catch (Exception ex)
            {
                args.Dispose();
                HandleError("Receive Error: " + ex.Message);
            }
        }

        /// <summary>
        /// 接收完成回调
        /// </summary>
        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceiveResult(e);
        }

        /// <summary>
        /// 处理接收结果
        /// </summary>
        private void ProcessReceiveResult(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError != SocketError.Success)
                {
                    HandleError("Socket Error: " + e.SocketError.ToString());
                    e.Dispose();
                    return;
                }

                if (e.BytesTransferred == 0)
                {
                    // 连接被远程关闭
                    HandleConnectionClosed();
                    e.Dispose();
                    return;
                }

                // 处理收到的数据
                lock (bufferLock)
                {
                    // 写入缓冲区
                    receiveBuffer.Write(e.Buffer, e.Offset, e.BytesTransferred);

                    // 尝试提取所有完整的行
                    ExtractAllLines();
                }

                e.Dispose();

                // 继续接收下一批数据
                BeginReceive();
            }
            catch (Exception ex)
            {
                e.Dispose();
                HandleError("Process Error: " + ex.Message);
            }
        }

        /// <summary>
        /// 从缓冲区提取所有完整的行
        /// </summary>
        private void ExtractAllLines()
        {
            byte[] allData = receiveBuffer.ToArray();
            int processedIndex = 0;
            bool foundLine = false;

            for (int i = 0; i < allData.Length - 1; i++)
            {
                if (allData[i] == '\r' && allData[i + 1] == '\n')
                {
                    // 找到一行
                    int lineLength = i - processedIndex;
                    string line = Encoding.UTF8.GetString(allData, processedIndex, lineLength);

                    // 加入消息队列
                    lock (queueLock)
                    {
                        messageQueue.Enqueue(line);
                    }

                    // 触发事件
                    OnLineReceived(line);

                    // 移动处理位置（跳过 \r\n）
                    processedIndex = i + 2;
                    i = processedIndex - 1; // 继续循环
                    foundLine = true;
                }
            }

            // 如果处理了数据，更新缓冲区
            if (processedIndex > 0)
            {
                byte[] remaining = new byte[allData.Length - processedIndex];
                Array.Copy(allData, processedIndex, remaining, 0, remaining.Length);
                receiveBuffer.SetLength(0);
                receiveBuffer.Write(remaining, 0, remaining.Length);
            }

            // 如果缓冲区太大，考虑清理
            if (receiveBuffer.Length > MAX_BUFFER_SIZE * 4)
            {
                // 如果超过一定大小，可能有问题，清空缓冲区
                receiveBuffer.SetLength(0);
                HandleError("Buffer overflow - cleared");
            }
        }

        /// <summary>
        /// 触发行接收事件
        /// </summary>
        private void OnLineReceived(string line)
        {
            if (LineReceived != null)
            {
                try
                {
                    LineReceived(line);
                }
                catch { /* 忽略事件处理中的异常 */ }
            }
        }

        /// <summary>
        /// 处理错误
        /// </summary>
        private void HandleError(string error)
        {
            isReceiving = false;
            if (ErrorOccurred != null)
            {
                try
                {
                    ErrorOccurred(error);
                }
                catch { /* 忽略事件处理中的异常 */ }
            }
        }

        /// <summary>
        /// 处理连接关闭
        /// </summary>
        private void HandleConnectionClosed()
        {
            isReceiving = false;
            if (ConnectionClosed != null)
            {
                try
                {
                    ConnectionClosed();
                }
                catch { /* 忽略事件处理中的异常 */ }
            }
        }

        /// <summary>
        /// 同步发送数据（自动添加 \r\n）
        /// </summary>
        public string SendLine(string data)
        {
            return Send(data + "\r\n");
        }

        /// <summary>
        /// 同步发送原始数据
        /// </summary>
        public string Send(string data)
        {
            string response = "Operation Timeout";

            if (socket == null)
            {
                return "Socket is not initialized";
            }

            if (!socket.Connected)
            {
                return "Socket is not connected";
            }

            ManualResetEvent sendDone = new ManualResetEvent(false);

            try
            {
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
                socketEventArg.Completed += (s, e) =>
                {
                    response = e.SocketError.ToString();
                    sendDone.Set();
                };

                byte[] payload = GetASCIIBytes(data);
                socketEventArg.SetBuffer(payload, 0, payload.Length);

                sendDone.Reset();
                socket.SendAsync(socketEventArg);

                if (!sendDone.WaitOne(TIMEOUT_MILLISECONDS))
                {
                    response = "Send Timeout";
                }

                socketEventArg.Dispose();
            }
            catch (Exception ex)
            {
                response = "Send Error: " + ex.Message;
            }
            finally
            {
                sendDone.Dispose();
            }

            return response;
        }

        public string Send(byte[] inf)
        {
            string response = "Operation Timeout";

            if (socket == null)
            {
                return "Socket is not initialized";
            }

            if (!socket.Connected)
            {
                return "Socket is not connected";
            }

            ManualResetEvent sendDone = new ManualResetEvent(false);

            try
            {
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
                socketEventArg.Completed += (s, e) =>
                {
                    response = e.SocketError.ToString();
                    sendDone.Set();
                };

                //byte[] payload = GetASCIIBytes(data);
                socketEventArg.SetBuffer(inf, 0, inf.Length);

                sendDone.Reset();
                socket.SendAsync(socketEventArg);

                if (!sendDone.WaitOne(TIMEOUT_MILLISECONDS))
                {
                    response = "Send Timeout";
                }

                socketEventArg.Dispose();
            }
            catch (Exception ex)
            {
                response = "Send Error: " + ex.Message;
            }
            finally
            {
                sendDone.Dispose();
            }

            return response;
        }

        /// <summary>
        /// 同步读取一行（阻塞，直到收到一行或超时）
        /// 注意：这个方法会阻塞当前线程
        /// </summary>
        public string ReadLine()
        {
            if (socket == null)
            {
                return "Socket is not initialized";
            }

            if (!socket.Connected)
            {
                return "Socket is not connected";
            }

            DateTime startTime = DateTime.Now;

            // 先检查队列中是否有已经完整接收的行
            lock (queueLock)
            {
                if (messageQueue.Count > 0)
                {
                    return messageQueue.Dequeue() +"\r\n";
                }
            }

            // 循环等待新数据
            while ((DateTime.Now - startTime).TotalMilliseconds < TIMEOUT_MILLISECONDS)
            {
                lock (queueLock)
                {
                    if (messageQueue.Count > 0)
                    {
                        return messageQueue.Dequeue() +"\r\n";
                    }
                }

                // 等待一小段时间，让接收循环有机会处理数据
                System.Threading.Thread.Sleep(10);
            }

            // ========== 超时处理：检查缓冲区中是否有未完成的数据 ==========
            lock (bufferLock)
            {
                // 检查缓冲区中是否有数据
                if (receiveBuffer != null)
                {
                    if (receiveBuffer.Length > 0)
                    {
                        // 获取缓冲区中的所有数据
                        byte[] allData = receiveBuffer.ToArray();

                        // 将数据转换为字符串
                        string partialData = Encoding.UTF8.GetString(allData, 0, allData.Length);

                        // 清空缓冲区，避免重复读取
                        receiveBuffer.SetLength(0);

                        // 返回部分数据（调用者可以通过返回值是否以 \r\n 结尾来判断是否完整）
                        return partialData;
                    }
                }
            }

            return "";//超时了
        }

        public bool HasPartialData()
        {
            lock (bufferLock)
            {
                return receiveBuffer != null && receiveBuffer.Length > 0;
            }
        }

        public string GetPartialData()
        {
            lock (bufferLock)
            {
                if (receiveBuffer == null || receiveBuffer.Length == 0)
                {
                    return null;
                }

                byte[] allData = receiveBuffer.ToArray();
                string data = Encoding.UTF8.GetString(allData, 0, allData.Length);
                receiveBuffer.SetLength(0);
                return data;
            }
        }

        /// <summary>
        /// 非阻塞读取：尝试从队列获取一行
        /// </summary>
        public string TryReadLine()
        {
            lock (queueLock)
            {
                if (messageQueue.Count > 0)
                {
                    return messageQueue.Dequeue();
                }
                return null;
            }
        }

        /// <summary>
        /// 检查是否有等待处理的数据
        /// </summary>
        public bool HasData()
        {
            lock (queueLock)
            {
                return messageQueue.Count > 0;
            }
        }

        /// <summary>
        /// 同步接收原始数据（用于调试）
        /// </summary>
        public string Receive()
        {
            if (socket == null)
            {
                return "Socket is not initialized";
            }

            if (!socket.Connected)
            {
                return "Socket is not connected";
            }

            ManualResetEvent localDone = new ManualResetEvent(false);
            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            socketEventArg.SetBuffer(new byte[MAX_BUFFER_SIZE], 0, MAX_BUFFER_SIZE);

            string response = "";//超时了

            socketEventArg.Completed += (s, e) =>
            {
                if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                {
                    response = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                    response = response.Trim('\0');
                }
                else if (e.SocketError != SocketError.Success)
                {
                    response = e.SocketError.ToString();
                }
                else if (e.BytesTransferred == 0)
                {
                    response = "Connection closed by remote host";
                }
                localDone.Set();
            };

            bool willRaiseEvent = socket.ReceiveAsync(socketEventArg);

            if (!willRaiseEvent)
            {
                if (socketEventArg.SocketError == SocketError.Success && socketEventArg.BytesTransferred > 0)
                {
                    response = Encoding.UTF8.GetString(socketEventArg.Buffer, socketEventArg.Offset, socketEventArg.BytesTransferred);
                    response = response.Trim('\0');
                }
                else if (socketEventArg.SocketError != SocketError.Success)
                {
                    response = socketEventArg.SocketError.ToString();
                }
                else if (socketEventArg.BytesTransferred == 0)
                {
                    response = "Connection closed by remote host";
                }
                localDone.Set();
            }
            else
            {
                if (!localDone.WaitOne(TIMEOUT_MILLISECONDS))
                {
                    response = "";//超时了
                }
            }

            socketEventArg.Dispose();
            localDone.Dispose();

            return response;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            isReceiving = false;

            // 处理缓冲区中未完成的数据
            string partialData = GetPartialData();
            if (!string.IsNullOrEmpty(partialData))
            {
                // 如果还有未处理的数据，可以通过事件通知调用者
                OnLineReceived(partialData);
            }

            lock (bufferLock)
            {
                if (receiveBuffer != null)
                {
                    receiveBuffer.Dispose();
                }
            }

            lock (queueLock)
            {
                messageQueue.Clear();
            }

            if (socket != null)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch { }
                socket = null;
            }
        }

        /// <summary>
        /// 检查连接状态
        /// </summary>
        public bool IsConnected()
        {
            return socket != null && socket.Connected && isReceiving;
        }

        /// <summary>
        /// 获取消息队列大小
        /// </summary>
        public int QueueSize()
        {
            lock (queueLock)
            {
                return messageQueue.Count;
            }
        }

        /// <summary>
        /// 将字符串转换为ASCII字节数组
        /// </summary>
        public static byte[] GetASCIIBytes(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new byte[0];
            }

            byte[] result = new byte[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                result[i] = (byte)(c <= 0x7f ? c : '?');
            }
            return result;
        }
    }

}