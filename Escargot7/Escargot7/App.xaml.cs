using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using MSN_Protocol;

namespace Escargot7
{
    public partial class App : Application
    {
        private static MainViewModel viewModel = null;

        DS_NS_Class ds_ns_c = new DS_NS_Class();
        TXT_Class txt_c = new TXT_Class();
        public int is_Login = 0;

        public string name = "";
        public string nic_name = "";
        public string psw = "";
        public string host = "143.198.4.104";
        public int port = 1863;
        /*
        public bool Is_remember_psw = false;//是否记住密码
        public bool Is_auto_login = false;//是否自动登录
        public bool Is_remember_messenge = false;//是否记住消息
        */
        private string Is_watching = "";//正在查看的用户

        public bool Allow_auto_login = true;

        public bool is_First = true;
        //private string o_name = "";
        private List<string> o_name_list = new List<string>();
        public List<string[]> new_mes_num_list = new List<string[]>();

        public delegate void LoginStateChanged(App sender, LoginStateChangedEventArgs args);
        public delegate void FriendListChanged(App sender, FriendListChangedEventArgs args);
        //public delegate void ErrorMesChanged(App sender, ErrorMesEventArgs args);

        public delegate void MesChanged(App sender, MesChangedEventArgs args);
        public delegate void MesFrmChanged(App sender, MesFrmChangedEventArgs args);

        public event LoginStateChanged LoginStateChangedEvent;
        public event FriendListChanged FriendListChangedEvent;
        //public event ErrorMesChanged ErrorMesEvent;

        public event MesChanged MesChangedEvent;
        public event MesFrmChanged MesFrmChangedEvent;


        /// <summary>
        /// 视图用于进行绑定的静态 ViewModel。
        /// </summary>
        /// <returns>MainViewModel 对象。</returns>
        public static MainViewModel ViewModel
        {
            get
            {
                // 延迟创建视图模型，直至需要时
                if (viewModel == null)
                    viewModel = new MainViewModel();

                return viewModel;
            }
        }

        /// <summary>
        /// 提供对电话应用程序的根框架的轻松访问。
        /// </summary>
        /// <returns>电话应用程序的根框架。</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        //一些事件
        public void MesChanged_(DS_NS_Class sender, MesChangedEventArgs e)//来消息了
        {
            
            if (e.Fri_name != Is_watching)
            {
                if (new_mes_num_list.Count != 0)
                {
                    for (int i = 0; i < new_mes_num_list.Count; i++)
                    {
                        if (new_mes_num_list[i][0] == e.Fri_name)
                        {
                            int num_temp = int.Parse(new_mes_num_list[i][1]) + 1;
                            new_mes_num_list[i][1] = num_temp + "";
                            //System.Diagnostics.Debug.WriteLine("[APPA: " + e.Fri_name + " " + new_mes_num_list[i + 1][1] + "]");
                            break;
                        }
                        if (i == new_mes_num_list.Count - 1)
                        {

                            string[] new_list_temp = new string[] { e.Fri_name, "1" };
                            new_mes_num_list.Add(new_list_temp);
                            //System.Diagnostics.Debug.WriteLine("[APPB: " + e.Fri_name + " " + new_mes_num_list[i + 1][1] + "]");
                        }
                    }
                }
                else
                {
                    string[] new_list_temp = new string[] { e.Fri_name, "1" };
                    new_mes_num_list.Add(new_list_temp);
                    //System.Diagnostics.Debug.WriteLine("[APPC: " + e.Fri_name + " " + new_mes_num_list[0][1] + "]");
                }
            }
            else
            {
                if (new_mes_num_list.Count != 0)
                {
                    for (int i = 0; i < new_mes_num_list.Count; i++)
                    {
                        if (new_mes_num_list[i][0] == e.Fri_name)
                        {
                            new_mes_num_list[i][1] = "0";
                            break;
                        }
                        if (i == new_mes_num_list.Count - 1)
                        {
                            string[] new_list_temp = new string[] { e.Fri_name, "0" };
                            new_mes_num_list.Add(new_list_temp);
                        }
                    }
                }
            }
            if (MesChangedEvent != null)
            {
                MesChangedEvent(this, new MesChangedEventArgs(e.Fri_name, e.Message, e.Date));
            }
        }

        public void MesFrmChanged_(DS_NS_Class sender, MesFrmChangedEventArgs e)//消息窗口状态改变
        {
            if (MesFrmChangedEvent != null)
            {
                MesFrmChangedEvent(this, new MesFrmChangedEventArgs(e.Fri_name, e.State));
            }
        }

        public void LoginStateChanged_(DS_NS_Class sender, LoginStateChangedEventArgs e)//登录状态改变
        {
            name = e.LoginName;
            nic_name = e.LoginNicName;

            if (LoginStateChangedEvent != null)
            {
                LoginStateChangedEvent(this, new LoginStateChangedEventArgs(e.LoginName, e.LoginNicName, e.LoginState));
            }
        }

        public void FriendListChanged_(DS_NS_Class sender, FriendListChangedEventArgs e)//好友列表改变
        {
            if (FriendListChangedEvent != null)
            {
                FriendListChangedEvent(this, new FriendListChangedEventArgs(e.Friend_list));
            }
        }

        public void ErrorMesChanged_(DS_NS_Class sender, ErrorMesEventArgs e)//错误
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(e.Error);
            });
        }


        //一些方法

        private void load_set()
        {
            string[] read_temp = txt_c.Txt_File_Reader().Split(new[] { " " }, StringSplitOptions.None);
            if (!(read_temp.Length < 6))
            {
                host = read_temp[2];
                port = int.Parse(read_temp[3]);
                /*
                if (read_temp[4] == "T")
                {
                    Is_remember_psw = true;
                }
                else
                {
                    Is_remember_psw = false;
                }

                if (read_temp[5] == "T")
                {
                    Is_auto_login = true;
                }
                else
                {
                    Is_auto_login = false;
                }
                 */
            }
        }

        public void Login(string name_,string psw_)
        {
            if (is_Login == 0)
            {
                load_set();
                is_Login = 2;
                ds_ns_c.Name = name_;
                ds_ns_c.Psw = psw_;
                ds_ns_c.Host = host;
                ds_ns_c.Port = port;
                ds_ns_c.Connect();
            }
        }

        public void Logoff()
        {
            if (is_Login != 0)
            {
                is_Login = 0;
                ds_ns_c.DisConnect();
            }
        }

        public void ChangeNicName(string new_nic_name)
        {

            if (is_Login == 1)
            {
                ds_ns_c.ChangeNicName(new_nic_name);
            }
        }

        public void ChangeCHG(string new_chg)
        {
            if (is_Login == 1)
            {
                ds_ns_c.ChangeCHG(new_chg);
            }
        }

        public void Rem_friend(string o_name_)
        {
            if ((is_Login == 1) && (o_name_ != ""))
            {
                ds_ns_c.Rem_friend(o_name_);
            }
        }

        public void Add_friend(string o_name_)
        {
            if ((is_Login == 1) && (o_name_ != ""))
            {
                ds_ns_c.Add_friend(o_name_);
            }
        }

        public void Create_chat(string o_name_)
        {
            if ((is_Login == 1) && (o_name_ != ""))
            {
                ds_ns_c.Create_chat(o_name_);
            }
        }

        public void Send_mes_frm(string o_name_, string mes_)
        {
            ds_ns_c.Send_mes_frm(o_name_, mes_);
        }

        public List<string[]> Get_mes_frm(string o_name_)
        {
            return ds_ns_c.Get_mes_frm(o_name_);
        }

        public int Get_mes_frm_state(string o_name_)
        {
            return ds_ns_c.Get_mes_frm_state(o_name_);
        }

        public void Change_Is_watching(string fri_name_)
        {
            Is_watching = fri_name_;
            if ((new_mes_num_list.Count != 0) && (fri_name_ != ""))
            {
                for (int i = 0; i < new_mes_num_list.Count; i++)
                {
                    if (new_mes_num_list[i][0] == fri_name_)
                    {
                        new_mes_num_list[i][1] = "0";
                        break;
                    }
                    /*
                    if (i == new_mes_num_list.Count - 1)
                    {
                        string[] new_list_temp = new string[] { fri_name_, "0" };
                        new_mes_num_list.Add(new_list_temp);
                    }
                     */
                }
            }
            if (MesChangedEvent != null)
            {
                MesChangedEvent(this, new MesChangedEventArgs(fri_name_, "", ""));
            }
        }

        /// <summary>
        /// Application 对象的构造函数。
        /// </summary>
        public App()
        {
            // 未捕获的异常的全局处理程序。 
            UnhandledException += Application_UnhandledException;

            // 标准 Silverlight 初始化
            InitializeComponent();

            // 特定于电话的初始化
            InitializePhoneApplication();

            // 调试时显示图形分析信息。
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // 显示当前帧速率计数器
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // 显示在每个帧中重绘的应用程序区域。
                //Application.Current.Host.Settings.EnableRedrawRegions = true；

                // 启用非生产分析可视化模式， 
                // 该模式显示递交给 GPU 的包含彩色重叠区的页面区域。
                //Application.Current.Host.Settings.EnableCacheVisualization = true；

                // 通过将应用程序的 PhoneApplicationService 对象的 UserIdleDetectionMode 属性
                // 设置为 Disabled 来禁用应用程序空闲检测。
                //  注意: 仅在调试模式下使用此设置。禁用用户空闲检测的应用程序在用户不使用电话时将继续运行
                // 并且消耗电池电量。
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }
        }

        // 应用程序启动(例如，从“开始”菜单启动)时执行的代码
        // 此代码在重新激活应用程序时不执行
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            ds_ns_c.LoginStateChangedEvent += LoginStateChanged_;
            ds_ns_c.FriendListChangedEvent += FriendListChanged_;
            ds_ns_c.ErrorMesEvent += ErrorMesChanged_;
            ds_ns_c.MesChangedEvent += MesChanged_;
            ds_ns_c.MesFrmChangedEvent += MesFrmChanged_;
            Allow_auto_login = true;
            //RootFrame.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
        }

        // 激活应用程序(置于前台)时执行的代码
        // 此代码在首次启动应用程序时不执行
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            // 确保正确恢复应用程序状态
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
            Allow_auto_login = true;
            //RootFrame.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
        }

        // 停用应用程序(发送到后台)时执行的代码
        // 此代码在应用程序关闭时不执行
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            Logoff();//目前还没解决后台问题，只能直接注销了
            //RootFrame.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        // 应用程序关闭(例如，用户点击“后退”)时执行的代码
        // 此代码在停用应用程序时不执行
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            Logoff();
            //RootFrame.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        // 导航失败时执行的代码
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // 导航已失败；强行进入调试器
                System.Diagnostics.Debugger.Break();
            }
        }

        // 出现未处理的异常时执行的代码
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // 出现未处理的异常；强行进入调试器
                System.Diagnostics.Debugger.Break();
            }
        }

        #region 电话应用程序初始化

        // 避免双重初始化
        private bool phoneApplicationInitialized = false;

        // 请勿向此方法中添加任何其他代码
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // 创建框架但先不将它设置为 RootVisual；这允许初始
            // 屏幕保持活动状态，直到准备呈现应用程序时。
            RootFrame = new TransitionFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // 处理导航故障
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // 确保我们未再次初始化
            phoneApplicationInitialized = true;
        }

        // 请勿向此方法中添加任何其他代码
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // 设置根视觉效果以允许应用程序呈现
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // 删除此处理程序，因为不再需要它
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}