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

    #region DS/NS服务器连接 DS_NS_Class

    public class DS_NS_Class
    {
        private Thread th_bcg;//声明后台线程
        private Thread th_send;//声明后台线程
        private Thread th_xfr;//声明后台线程
        private Thread th_atd;//声明后台线程

        public delegate void LoginStateChanged(DS_NS_Class sender, LoginStateChangedEventArgs args);
        public delegate void FriendListChanged(DS_NS_Class sender, FriendListChangedEventArgs args);
        public delegate void ErrorMesChanged(DS_NS_Class sender, ErrorMesEventArgs args);

        public delegate void MesChanged(DS_NS_Class sender, MesChangedEventArgs args);
        public delegate void MesFrmChanged(DS_NS_Class sender, MesFrmChangedEventArgs args);

        public event LoginStateChanged LoginStateChangedEvent;
        public event FriendListChanged FriendListChangedEvent;
        public event ErrorMesChanged ErrorMesEvent;

        public event MesChanged MesChangedEvent;
        public event MesFrmChanged MesFrmChangedEvent;

        private int is_login = 0;//登录状态（0为未登录，1为已登录，2为登录中）
        private int seccess_con = 1;

        private string name_path = "name.txt";
        private string psw_path = "psw.txt";

        private string server_path = "server.txt";
        private string port_path = "port.txt";
        private string friend_path = "friend.txt";

        private string name = "", nic_name = "", server = "ds.escargot.nina.chat", psw = "", chg = "NLN";//默认设置
        private int port = 1863;


        private string send_temp = "";
        private string chg_temp = "";
        private string add_temp = "";
        private string rem_temp = "";
        private string xfr_temp = "";
        private string rea_temp = "";


        private string xfr_s_name_temp = "", xfr_o_name_temp = "", xfr_key_temp = "", xfr_key_2_temp = "", xfr_server_temp = "";
        private int xfr_port_temp = 0;
        private int mes_frm_show_i_temp = 0;


        private int self_c_num = 0;

        private List<string[]> Friends_list = new List<string[]>();//好友列表

        /*
        private static TcpClient tcp_client;
        private static NetworkStream tcp_stream;
        private static StreamReader tcp_reader;
        private static StreamWriter tcp_writer;
         */
        private SocketClient tcp_client;


        public List<SB_Class> SB_Chat_list = new List<SB_Class>();//好友列表

        public void MesChanged_(SB_Class sender, MesChangedEventArgs e)
        {
            if (MesChangedEvent != null)
            {
                MesChangedEvent(this, new MesChangedEventArgs(e.Fri_name, e.Message, e.Date));
            }

        }

        public void MesFrmChanged_(SB_Class sender, MesFrmChangedEventArgs e)
        {

            if (MesFrmChangedEvent != null)
            {
                MesFrmChangedEvent(this, new MesFrmChangedEventArgs(e.Fri_name, e.State));
            }
        }

        public string Name
        {
            set
            {
                name = value;
            }
        }

        public string Psw
        {
            set
            {
                psw = value;
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

        public void Connect()
        {
            th_bcg = new Thread(Connect_to_Server);//登录
            th_bcg.Start();
            th_atd = new Thread(Anti_dead);//抗假死
            th_atd.Start();
        }

        public void DisConnect()
        {
            Sender("OUT");//注销
            Dis_Connect_to_Server();
        }

        private void Log_off_frm()//加载离线时窗体样貌
        {

            //Read_txt();
            is_login = 0;
            if (LoginStateChangedEvent != null)
            {
                LoginStateChangedEvent(this, new LoginStateChangedEventArgs(name, nic_name, "OUT"));
            }
        }

        private void Log_ing_frm(int step)//加载登录时窗体样貌
        {
            //Read_txt();
            is_login = 2;
            if (LoginStateChangedEvent != null)
            {
                LoginStateChangedEvent(this, new LoginStateChangedEventArgs(name, nic_name, "ING"));
            }
        }


        private void Log_ed_frm()//加载登录后窗体样貌
        {
            //Read_txt();
            is_login = 1;
            if (LoginStateChangedEvent != null)
            {
                LoginStateChangedEvent(this, new LoginStateChangedEventArgs(name, nic_name, chg));
            }
        }

        private void Error_Mes(string mes)//出错了
        {
            if (ErrorMesEvent != null)
            {
                ErrorMesEvent(this, new ErrorMesEventArgs(mes));
            }
        }

        public void Connect_to_Server()
        {
            Log_ing_frm(0);
            tcp_client = new SocketClient();
            tcp_client.Connect(server, port);
            Sender("VER 1 MSNP4 MSNP3 CVR0\r\n");
            /*
            tcp_stream = tcp_client.GetStream();
            tcp_reader = new StreamReader(tcp_stream, Encoding.UTF8);
            tcp_writer = new StreamWriter(tcp_stream, Encoding.ASCII) { AutoFlush = true };
             */
            Reader();
        }

        public void Dis_Connect_to_Server()
        {
            if (SB_Chat_list.Count != 0)
            {
                for (int i = 0; i < SB_Chat_list.Count; i++)
                {
                    try
                    {
                        SB_Chat_list[i].Dis_Connect();
                    }
                    catch
                    {
                    }
                }
            }

            Log_off_frm();
            tcp_client.Close();
        }

        public void ChangeNicName(string new_nic_name)//更改昵称
        {
            rea_temp = "REA";
            Sender("REA " + seccess_con + " " + name + " " + new_nic_name + "\r\n");
        }
        public void ChangeCHG(string new_chg)//更改状态
        {
            chg_temp = new_chg;
            Sender("CHG " + seccess_con + " " + new_chg + "\r\n");
        }
        public void Add_friend(string fri_name)//添加好友
        {
            add_temp = fri_name;
            Sender("ADD " + seccess_con + " FL " + fri_name + " " + fri_name + "\r\n");
        }
        public void Rem_friend(string fri_name)//删除好友
        {
            rem_temp = fri_name;
            Sender("REM " + seccess_con + " FL " + fri_name + "\r\n");
        }
        public void Create_chat(string fri_name)//发起聊天
        {
            xfr_temp = fri_name;
            Sender("XFR " + seccess_con + " SB\r\n");
        }

        private void Load_Friend_List(string fri_name, string fri_nic_name, int c_num, int t_num)
        {
            if (fri_name != name)
            {
                if (c_num == 1)
                {
                    Friends_list.Clear();
                }
                string[] new_fri_list_temp = new string[] { fri_name, fri_nic_name, "FLN" };
                Friends_list.Add(new_fri_list_temp);
                if ((c_num == t_num) && (FriendListChangedEvent != null))
                {
                    FriendListChangedEvent(this, new FriendListChangedEventArgs(Friends_list));
                }
            }
        }

        private void Update_Friend_List(string fri_chg, string fri_name, string fri_nic_name)
        {
            if (fri_name != name)
            {
                switch (fri_chg)
                {
                    case "NLN":
                        fri_chg = "NLN";
                        break;
                    case "BSY":
                    case "PHN":
                        fri_chg = "BSY";
                        break;
                    case "BRB":
                    case "IDL":
                    case "AWY":
                    case "LUN":
                        fri_chg = "AWY";
                        break;
                    default:
                        fri_chg = "FLN";
                        break;
                }


                if (fri_nic_name == "")
                {
                    fri_nic_name = fri_name;
                }
                if (Friends_list.Count == 0)
                {
                    string[] new_fri_list_temp = new string[] { fri_name, fri_nic_name, fri_chg };
                    Friends_list.Add(new_fri_list_temp);
                }
                else
                {
                    for (int i = 0; i < Friends_list.Count; i++)
                    {

                        if (Friends_list[i][0] == fri_name)
                        {
                            Friends_list[i][0] = fri_name;
                            Friends_list[i][1] = fri_nic_name;
                            Friends_list[i][2] = fri_chg;
                            break;
                        }
                        else if (i == Friends_list.Count - 1)
                        {
                            string[] new_fri_list_temp = new string[] { fri_name, fri_nic_name, fri_chg };
                            Friends_list.Add(new_fri_list_temp);
                        }

                    }
                }


                for (int i = 0; i < Friends_list.Count; i++)
                {
                    if (Friends_list[i][2] != "FLN")
                    {
                        string[] item = Friends_list[i];
                        Friends_list.RemoveAt(i);
                        Friends_list.Insert(0, item);
                    }
                }


                if (FriendListChangedEvent != null)
                {
                    FriendListChangedEvent(this, new FriendListChangedEventArgs(Friends_list));
                }
            }
        }

        private void Remove_Friend_List(string fri_name)
        {
            if (Friends_list.Count != 0)
            {
                for (int i = 0; i < Friends_list.Count; i++)
                {

                    if (Friends_list[i][0] == fri_name)
                    {
                        Friends_list.RemoveAt(i);
                        break;
                    }

                }
            }
            if (FriendListChangedEvent != null)
            {
                FriendListChangedEvent(this, new FriendListChangedEventArgs(Friends_list));
            }
        }

        private void Add_mes_frm(string o_name_, string key_, string key_2_, string server_, int port_)
        {
            for (int i = 0; i < Friends_list.Count; i++)
            {

                if (Friends_list[i][0] == o_name_)
                {
                    break;
                }
                if (i == Friends_list.Count - 1)
                {
                    Update_Friend_List("STG", o_name_, o_name_);
                }

            }

            int Count_temp = SB_Chat_list.Count;

            if (Count_temp == 0)
            {
                SB_Class SB_Chat_list_temp = new SB_Class();
                SB_Chat_list_temp.s_Name = name;
                SB_Chat_list_temp.o_Name = o_name_;
                SB_Chat_list_temp.Key_1 = key_;
                SB_Chat_list_temp.Key_2 = key_2_;
                SB_Chat_list_temp.Host = server_;
                SB_Chat_list_temp.Port = port_;
                SB_Chat_list_temp.Num = 0;
                SB_Chat_list_temp.MesChangedEvent += MesChanged_;
                SB_Chat_list_temp.MesFrmChangedEvent += MesFrmChanged_;
                SB_Chat_list_temp.Connect();
                SB_Chat_list.Add(SB_Chat_list_temp);

                //System.Diagnostics.Debug.WriteLine("H1");
            }
            else
            {
                for (int i = 0; i < Count_temp; i++)
                {
                    if (SB_Chat_list[i].o_Name == o_name_)
                    {

                        //System.Diagnostics.Debug.WriteLine("H21 " + i + " " + SB_Chat_list[i].o_Name + " " + o_name_ + " " + SB_Chat_list.Count + " " + Count_temp);
                        SB_Chat_list[i].Dis_Connect();
                        SB_Chat_list[i] = new SB_Class();
                        SB_Chat_list[i].s_Name = name;
                        SB_Chat_list[i].o_Name = o_name_;
                        SB_Chat_list[i].Key_1 = key_;
                        SB_Chat_list[i].Key_2 = key_2_;
                        SB_Chat_list[i].Host = server_;
                        SB_Chat_list[i].Port = port_;
                        SB_Chat_list[i].Num = i;
                        SB_Chat_list[i].MesChangedEvent += MesChanged_;
                        SB_Chat_list[i].MesFrmChangedEvent += MesFrmChanged_;
                        SB_Chat_list[i].Connect();

                        //System.Diagnostics.Debug.WriteLine("H22 " + i + " " + SB_Chat_list[i].o_Name + " " + o_name_ + " " + SB_Chat_list.Count + " " + Count_temp);
                        break;
                    }
                    else if (i == Count_temp - 1)
                    {
                        //System.Diagnostics.Debug.WriteLine("H31 " + i + " " + SB_Chat_list[i].o_Name + " " + o_name_ + " " + SB_Chat_list.Count + " " + Count_temp);
                        SB_Class SB_Chat_list_temp = new SB_Class();
                        SB_Chat_list_temp.s_Name = name;
                        SB_Chat_list_temp.o_Name = o_name_;
                        SB_Chat_list_temp.Key_1 = key_;
                        SB_Chat_list_temp.Key_2 = key_2_;
                        SB_Chat_list_temp.Host = server_;
                        SB_Chat_list_temp.Port = port_;
                        SB_Chat_list_temp.Num = i + 1;
                        SB_Chat_list_temp.MesChangedEvent += MesChanged_;
                        SB_Chat_list_temp.MesFrmChangedEvent += MesFrmChanged_;
                        SB_Chat_list_temp.Connect();
                        SB_Chat_list.Add(SB_Chat_list_temp);
                        //System.Diagnostics.Debug.WriteLine("H32 " + i + " " + SB_Chat_list[i].o_Name + " " + o_name_ + " " + SB_Chat_list.Count + " " + Count_temp);
                        break;
                    }
                }
            }
        }
        private void Rem_mes_frm(string o_name_)
        {
            for (int i = 0; i < SB_Chat_list.Count; i++)
            {
                if (SB_Chat_list[i].o_Name == o_name_)
                {
                    SB_Chat_list[i].Dis_Connect();
                    //SB_Chat_list.RemoveAt(i);
                }
            }


        }
        public List<string[]> Get_mes_frm(string o_name_)
        {
            for (int i = 0; i < SB_Chat_list.Count; i++)
            {
                if (SB_Chat_list[i].o_Name == o_name_)
                {
                    return SB_Chat_list[i].Message_list;
                }
            }
            return new List<string[]>();

        }

        public int Get_mes_frm_state(string o_name_)
        {
            for (int i = 0; i < SB_Chat_list.Count; i++)
            {
                if (SB_Chat_list[i].o_Name == o_name_)
                {
                    return SB_Chat_list[i].is_login;
                }
            }
            return 0;

        }

        public void Send_mes_frm(string o_name_,string mes)
        {
            for (int i = 0; i < SB_Chat_list.Count; i++)
            {
                if (SB_Chat_list[i].o_Name == o_name_)
                {
                    SB_Chat_list[i].Send_Mes(mes);
                }
            }

        }

        private void Reader()
        {
            while (is_login != 0)
            {
                string inf = "";
                try
                {
                    inf = tcp_client.ReadLine().TrimEnd('\r', '\n');
                    //inf = tcp_client.Receive();


                    if (inf != "")
                    {
                        System.Diagnostics.Debug.WriteLine("[ds_ns_debug: " + inf + "]");
                        //Log_ing_frm(inf);
                        string[] inf_split = inf.Split(new[] { " " }, StringSplitOptions.None);

                        //Log_ing_frm(inf_split[2]);


                        if (is_login == 2)//MSNP 4 登录协议
                        {
                            try
                            {
                                if ((inf_split[0] == "VER") && (inf_split[2] == "MSNP4"))
                                {
                                    seccess_con = int.Parse(inf_split[1]) + 1;
                                    Sender("INF " + seccess_con + "\r\n");
                                    Log_ing_frm(1);
                                }
                                else if ((inf_split[0] == "INF") && (inf_split[2] == "MD5"))
                                {
                                    seccess_con = int.Parse(inf_split[1]) + 1;
                                    Sender("USR " + seccess_con + " MD5 I " + name + "\r\n");
                                    Log_ing_frm(2);
                                }
                                else if ((inf_split[0] == "USR") && (inf_split[2] == "MD5") && (inf_split[3] == "S"))
                                {
                                    seccess_con = int.Parse(inf_split[1]) + 1;
                                    Sender("USR " + seccess_con + " MD5 S " + GetMD5Hash(inf_split[4] + psw + "") + "\r\n");
                                    Log_ing_frm(3);
                                }
                                else if ((inf_split[0] == "USR") && (inf_split[2] == "OK") && (inf_split[3] == name))
                                {
                                    seccess_con = int.Parse(inf_split[1]) + 1;
                                    Sender("SYN " + seccess_con + " 1\r\n");
                                    nic_name = inf_split[4];
                                    Log_ing_frm(4);
                                }
                                else if ((inf_split[0] == "SYN") && (inf_split[2] == "2"))
                                {
                                    seccess_con = int.Parse(inf_split[1]) + 1;
                                    Sender("CHG " + seccess_con + " NLN\r\n");
                                    Log_ed_frm();
                                }
                                else if (inf_split[0] == "911")
                                {
                                    Dis_Connect_to_Server();
                                    Error_Mes("密码错误！");
                                }
                            }
                            catch
                            {
                            }
                        }
                        else if (is_login == 1)//MSNP 4 其它协议
                        {
                            try
                            {
                                if ((inf_split[0] == "OUT") && (inf_split[1] == "OTH"))//已在其它位置登录
                                {
                                    Dis_Connect_to_Server();
                                    Error_Mes("你的账号已在别处登录！");
                                }
                                else if ((inf_split[0] == "LST") && (inf_split[2] == "FL"))
                                {
                                    //if (inf_split[6] != name)
                                    //{
                                    seccess_con = int.Parse(inf_split[1]) + 1;
                                    Load_Friend_List(inf_split[6], inf_split[7], int.Parse(inf_split[4]), int.Parse(inf_split[5]));
                                    //}
                                }
                                else if ((inf_split[0] == "ILN"))
                                {
                                    seccess_con = int.Parse(inf_split[1]) + 1;
                                    Update_Friend_List(inf_split[2], inf_split[3], inf_split[4]);
                                }
                                else if ((inf_split[0] == "NLN"))
                                {
                                    Update_Friend_List(inf_split[1], inf_split[2], inf_split[3]);
                                }
                                else if ((inf_split[0] == "FLN"))
                                {
                                    //string nic_name_temp = Txt_File_Reader(juedui_path() + name + "\\" + inf_split[1] + "\\fri_nic_name.txt");
                                    Rem_mes_frm(inf_split[1]);
                                    Update_Friend_List("FLN", inf_split[1], "");
                                }
                                else if ((inf_split[0] == "CHG") && (chg_temp != "") && (inf_split[2] == chg_temp))
                                {
                                    seccess_con = int.Parse(inf_split[1]) + 1;
                                    chg_temp = "";
                                    chg = inf_split[2];
                                    Log_ed_frm();
                                }
                                else if ((inf_split[0] == "REA") && (rea_temp != "") && (inf_split[3] == name))
                                {
                                    seccess_con = int.Parse(inf_split[1]) + 1;
                                    rea_temp = "";
                                    nic_name = inf_split[4];
                                    Log_ed_frm();
                                }
                                else if ((inf_split[0] == "ADD"))
                                {
                                    seccess_con = int.Parse(inf_split[1]) + 1;
                                    if ((add_temp != "") && (inf_split[4] == add_temp))
                                    {
                                        add_temp = "";
                                    }
                                    Update_Friend_List("FLN", inf_split[4], inf_split[5]);
                                }
                                else if (((inf_split[0] == "201") || (inf_split[0] == "205")) && (add_temp != ""))
                                {
                                    seccess_con = int.Parse(inf_split[1]) + 1;
                                    add_temp = "";
                                    Error_Mes("此联系人不存在！");
                                }
                                else if ((inf_split[0] == "REM") && (rem_temp != "") && (inf_split[4] == rem_temp))
                                {
                                    seccess_con = int.Parse(inf_split[1]) + 1;
                                    rem_temp = "";
                                    Remove_Friend_List(inf_split[4]);
                                }
                                else if ((inf_split[0] == "XFR") && (xfr_temp != ""))
                                {
                                    seccess_con = int.Parse(inf_split[1]) + 1;

                                    string[] server_inf_split = inf_split[3].Split(new[] { ":" }, StringSplitOptions.None);

                                    Add_mes_frm(xfr_temp, inf_split[5], "", server_inf_split[0], int.Parse(server_inf_split[1]));

                                    xfr_temp = "";

                                }
                                else if (inf_split[0] == "RNG")
                                {
                                    string[] server_inf_split = inf_split[2].Split(new[] { ":" }, StringSplitOptions.None);

                                    Add_mes_frm(inf_split[5], inf_split[1], inf_split[4], server_inf_split[0], int.Parse(server_inf_split[1]));

                                }
                            }
                            catch
                            {
                            }
                        }

                    }
                }
                catch
                {

                }
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



        private void Anti_dead()//防假死
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(60000);
                    //System.Diagnostics.Debug.WriteLine("ring");
                    if (is_login == 1)
                    {
                        ChangeCHG(chg);
                        //System.Diagnostics.Debug.WriteLine("ring2"+chg);
                    }
                }
            }
            catch
            {
            }
        }

        static string GetMD5Hash(string input)
        {
            return MD5Core.GetHashString(input).ToLower();
        }


    }

    #endregion

}
