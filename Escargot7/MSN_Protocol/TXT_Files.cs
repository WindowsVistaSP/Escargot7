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

    #region txt文件操作 TXT_Class
    public class TXT_Class
    {
        public void Txt_File_Writer(string inf, string path)//参考其他文章
        {
            try
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream(path, FileMode.Create, storage))
                    {
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            writer.Write(inf);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public string Txt_File_Reader(string path)//参考其他文章
        {
            try
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (storage.FileExists(path))
                    {
                        using (IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream(path, FileMode.Open, FileAccess.Read, storage))
                        {
                            using (StreamReader reader = new StreamReader(fileStream))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                    else
                    {
                        return "";
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        public void Txt_File_Writer(string inf)//参考其他文章
        {
            try
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream("setting.txt", FileMode.Create, storage))
                    {
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            writer.Write(inf);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public string Txt_File_Reader()//参考其他文章
        {
            try
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (storage.FileExists("setting.txt"))
                    {
                        using (IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream("setting.txt", FileMode.Open, FileAccess.Read, storage))
                        {
                            using (StreamReader reader = new StreamReader(fileStream))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                    else
                    {
                        return "";
                    }
                }
            }
            catch
            {
                return "";
            }
        }
    }
    #endregion

}
