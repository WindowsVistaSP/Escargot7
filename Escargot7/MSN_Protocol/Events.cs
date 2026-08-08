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

    #region 一些事件
    public class ErrorMesEventArgs : EventArgs//出错了
    {
        public readonly string Error;

        public ErrorMesEventArgs(string mes)
        {
            this.Error = mes;
        }
    }
    public class LoginStateChangedEventArgs : EventArgs//登录状态改变
    {
        public readonly string LoginState;
        public readonly string LoginName;
        public readonly string LoginNicName;

        public LoginStateChangedEventArgs(string name, string nic_name, string loginstate)
        {
            this.LoginState = loginstate;
            this.LoginName = name;
            this.LoginNicName = nic_name;
        }
    }
    public class FriendListChangedEventArgs : EventArgs//好友列表改变
    {

        public readonly List<string[]> Friend_list;

        public FriendListChangedEventArgs(List<string[]> friend_list)
        {
            this.Friend_list = friend_list;
        }
    }
    public class MesChangedEventArgs : EventArgs//新消息
    {

        public readonly string Fri_name;
        public readonly string Message;
        public readonly string Date;

        public MesChangedEventArgs(string fri_name, string new_mes, string date)
        {
            this.Fri_name = fri_name;
            this.Message = new_mes;
            this.Date = date;
        }
    }
    public class MesFrmChangedEventArgs : EventArgs//联系人状态改变
    {

        public readonly string Fri_name;
        public readonly int State;

        public MesFrmChangedEventArgs(string fri_name, int new_state)
        {
            this.Fri_name = fri_name;
            this.State = new_state;
        }
    }
    #endregion

}
