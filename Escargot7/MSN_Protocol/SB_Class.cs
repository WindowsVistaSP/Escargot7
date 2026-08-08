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

    public class SB_Class
    {
        public delegate void MesChanged(SB_Class sender, MesChangedEventArgs args);
        public delegate void MesFrmChanged(SB_Class sender, MesFrmChangedEventArgs args);

        public event MesChanged MesChangedEvent;
        public event MesFrmChanged MesFrmChangedEvent;

        private Thread th_bcg;//声明后台线程
        private Thread th_send;//声明后台线程

        private string s_name = "", o_name = "", key = "", key_2 = "", server = "";
        private int port = 0;//默认设置

        private int num = 0;//在列表中的序号

        private int seccess_con = 1;

        public int is_login = 0;//登录状态（0为未登录，1为已登录，2为登录中）

        private byte[] send_temp_;
        private string send_temp = "";
        private string user_temp = "";
        private int zhuangtai_temp = 5;

        private int msg_temp = 0;
        private int msg_temp_2 = 0;

        private string msg_temp_3 = "";

        private SocketClient tcp_client;

        public List<string[]> Message_list = new List<string[]>();//消息列表

        public string s_Name//自己的名称
        {
            set
            {
                s_name = value;
            }
        }

        public string o_Name//对方的名称
        {
            set
            {
                o_name = value;
            }
            get
            {
                return o_name;
            }
        }

        public string Key_1//第一个key
        {
            set
            {
                key = value;
            }
        }

        public string Key_2//第一个key
        {
            set
            {
                key_2 = value;
            }
        }

        public string Host
        {
            set
            {
                server = value;
            }
        }

        public int Port
        {
            set
            {
                port = value;
            }
        }

        public int Num
        {
            set
            {
                num = value;
            }
        }

        private void Log_off_frm()//加载离线时窗体样貌
        {

            //Read_txt();
            is_login = 0;
            if (MesFrmChangedEvent != null)
            {
                MesFrmChangedEvent(this, new MesFrmChangedEventArgs(o_name, 0));
            }
        }

        private void Log_ing_frm()//加载登录时窗体样貌
        {
            //Read_txt();
            is_login = 2;
            if (MesFrmChangedEvent != null)
            {
                MesFrmChangedEvent(this, new MesFrmChangedEventArgs(o_name, 2));
            }
        }


        private void Log_ed_frm()//加载登录后窗体样貌
        {
            //Read_txt();
            is_login = 1;
            if (MesFrmChangedEvent != null)
            {
                MesFrmChangedEvent(this, new MesFrmChangedEventArgs(o_name, 1));
            }
        }

        private void New_Mes(string mes, string date)//新消息
        {
            string[] mes_temp = new string[] {o_name, mes, date };
            Message_list.Add(mes_temp);
            /*
            if (Is_watching)
            {
                num = 0;
            }
            else
            {
                num += 1;
            }
             */
            if (MesChangedEvent != null)
            {
                MesChangedEvent(this, new MesChangedEventArgs(o_name, mes, date));
            }
        }

        public void Send_Mes(string mes)//发消息
        {

            byte[] msgBytes = Encoding.UTF8.GetBytes(mes);
            int mes_leng_temp = msgBytes.Length + 126;

            Sender("MSG " + seccess_con + " N " + mes_leng_temp + "\r\nMIME-Version: 1.0\r\nContent-Type: text/plain; charset=UTF-8\r\nX-MMS-IM-Format: FN=%E5%AE%8B%E4%BD%93; EF=; CO=0; CS=86; PF=0\r\n\r\n");
            Sender(msgBytes);
            string[] mes_temp = new string[] { s_name, mes, DateTime.Now.ToString() };
            Message_list.Add(mes_temp);
        }


        public void Connect()
        {
            th_bcg = new Thread(Connect_to_Server);
            th_bcg.Start();
        }

        public void Dis_Connect()
        {
            Sender("OUT");
            Dis_Connect_to_Server();
        }

        public void Connect_to_Server()
        {
            tcp_client = new SocketClient();
            tcp_client.Connect(server, port);
            user_temp = "1";
            Log_ing_frm();
            if (key_2 == "")
            {
                Sender("USR 1 " + s_name + " " + key + "\r\n");
            }
            else
            {
                Sender("ANS 1 " + s_name + " " + key_2 + " " + key + "\r\n");
            }
            Reader();
        }

        public void Dis_Connect_to_Server()
        {
            tcp_client.Close();
            Log_off_frm();
        }

        private void Reader()
        {
            while (is_login != 0)
            {
                string inf = "";
                string inf_ = "";
                /*
                try
                {
                   */ 
                    inf_ = tcp_client.ReadLine();

                    inf = inf_.TrimEnd('\r', '\n');
                    
                    if (inf != "")
                    {

                        System.Diagnostics.Debug.WriteLine("[SB " + inf_ + " " + inf + "]");
                        string[] inf_split = inf.Split(new[] { " " }, StringSplitOptions.None);
                        string[] inf_split_ = inf_.Split(new[] { " " }, StringSplitOptions.None);

                        if ((inf_split[0] == "USR") && (inf_split[2] == "OK") && (user_temp == "1"))
                        {
                            seccess_con = int.Parse(inf_split[1]) + 1;
                            Sender("CAL " + seccess_con + " " + o_name + "\r\n");
                            user_temp = "";
                        }
                        else if (inf_split[0] == "CAL")
                        {
                            seccess_con = int.Parse(inf_split[1]) + 1;
                        }
                        else if (inf_split[0] == "JOI")//初始化成功
                        {
                            Log_ed_frm();
                        }
                        else if (inf_split[0] == "IRO")//初始化成功
                        {
                            seccess_con = int.Parse(inf_split[1]) + 1;
                            Log_ed_frm();
                        }
                        else if ((inf_split[0] == "X-MMS-IM-Format:") && (msg_temp > 0))
                        {
                            msg_temp_2 = msg_temp;
                            msg_temp = 0;
                        }

                        else if (inf_split_.Length >= 4)//防粘包
                        {
                            if (msg_temp_2 > 0)
                            {
                                //System.Diagnostics.Debug.WriteLine("[debug: " + inf + " A " + msg_temp_3 + " " + msg_temp_2 + "]");
                                if (msg_temp_2 >= Encoding.UTF8.GetByteCount(inf_))
                                {
                                    msg_temp_3 += inf_;
                                    msg_temp_2 -= Encoding.UTF8.GetByteCount(inf_);
                                    if (msg_temp_2 <= 0)
                                    {
                                        New_Mes(msg_temp_3, DateTime.Now.ToString());
                                        msg_temp_3 = "";
                                        msg_temp_2 = 0;
                                    }
                                }
                                else
                                {
                                    msg_temp_3 += SubstringByUtf8Bytes(inf_, msg_temp_2);
                                    New_Mes(msg_temp_3, DateTime.Now.ToString());
                                    msg_temp_3 = "";
                                    msg_temp_2 = 0;
                                }
                            }

                            if ((inf_split_[inf_split_.Length - 3] == o_name) && (inf_split_[inf_split_.Length - 4].EndsWith("MSG")) && (int.Parse(inf_split_[3]) > 127))
                            {
                                msg_temp = int.Parse(inf_split[3]) - 127;
                                //MessageBox.Show("A" + msg_temp);
                            }
                            else
                            {

                            }
                        }
                        else if (msg_temp_2 > 0)
                        {

                            //System.Diagnostics.Debug.WriteLine("[debug: " + inf_ + " B " + msg_temp_3 + " " + msg_temp_2 + "]");
                            msg_temp_3 += inf_;
                            msg_temp_2 -= Encoding.UTF8.GetByteCount(inf_);
                            if ((msg_temp_2 <= 0))
                            {
                                New_Mes(msg_temp_3, DateTime.Now.ToString());
                                msg_temp_3 = "";
                                msg_temp_2 = 0;
                            }
                        }



                    }
                    /*
                }
                catch (Exception ex)
                {

                    System.Diagnostics.Debug.WriteLine("[Error " + ex.Message + "]");
                }
                 */
            }

        }

        private void Sender(string inf)
        {

            send_temp = inf;
            th_send = new Thread(Sender_v);//登录
            th_send.Start();

        }

        private void Sender_v()
        {
            try
            {
                string response = tcp_client.Send(send_temp);
            }
            catch
            {
            }
        }

        private void Sender(byte[] inf)
        {

            send_temp_ = inf;
            th_send = new Thread(Sender_v_);//登录
            th_send.Start();

        }

        private void Sender_v_()
        {
            try
            {
                string response = tcp_client.Send(send_temp_);
            }
            catch
            {
            }
        }

        public static string SubstringByUtf8Bytes(string str, int maxBytes)//这个方法是用ai写的，为了解决汉字截断的问题
        {
            if (string.IsNullOrEmpty(str))
                return str;

            byte[] bytes = Encoding.UTF8.GetBytes(str);

            if (bytes.Length <= maxBytes)
                return str;

            int index = maxBytes;
            while (index > 0 && (bytes[index] & 0xC0) == 0x80)
            {
                index--;
            }
            return Encoding.UTF8.GetString(bytes, 0, index);
        }

    }


}
