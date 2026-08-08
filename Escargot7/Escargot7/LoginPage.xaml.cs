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
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Coding4Fun.Phone.Controls;

using MSN_Protocol;

namespace Escargot7
{
    public partial class LoginPage : PhoneApplicationPage
    {
        public LoginPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        App app = Application.Current as App;
        TXT_Class txt_c = new TXT_Class();
        private int is_login = 0;//防闪烁

        private string name = "";
        //public string nic_name = "";
        private string psw = "";
        private string host = "143.198.4.104";
        private int port = 1863;

        private bool Is_remember_psw = false;//是否记住密码
        private bool Is_auto_login = false;//是否自动登录

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            app.Change_Is_watching("");
            app.LoginStateChangedEvent += LoginStateChanged;
            load_set();

            if (app.is_Login == 1)
            {
                Log_ed_frm();
            }
            else if (app.is_Login == 0)
            {
                Log_off_frm();
            }
            else
            {
                Log_ing_frm();
            }
        }

        public void LoginStateChanged(App sender, LoginStateChangedEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (e.LoginState == "OUT")
                {
                    Log_off_frm();
                }
                else if (e.LoginState == "ING")
                {
                    Log_ing_frm();
                }
                else
                {
                    Log_ed_frm();
                }
            });
        }

        private void load_set()
        {
            string[] read_temp = txt_c.Txt_File_Reader().Split(new[] { " " }, StringSplitOptions.None);
            if (!(read_temp.Length < 6))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    name = read_temp[0];
                    psw = read_temp[1];

                    host = read_temp[2];
                    port = int.Parse(read_temp[3]);

                    phoneTextBox1.Text = name;

                    if (read_temp[4] == "T")
                    {
                        Is_remember_psw = true;
                        checkBox2.IsChecked = true;
                        checkBox1.IsEnabled = true;
                        passwordBox1.Password = psw;
                    }
                    else
                    {
                        Is_remember_psw = false;
                        checkBox2.IsChecked = false;
                        checkBox1.IsEnabled = false;
                        passwordBox1.Password = "";
                    }

                    if (read_temp[5] == "T")
                    {
                        Is_auto_login = true;
                        checkBox1.IsChecked = true;
                        if ((name != "")&&(checkBox1.IsEnabled == true)&&(app.Allow_auto_login))
                        {
                            app.Login(name, psw);
                        }
                    }
                    else
                    {
                        Is_auto_login = false;
                        checkBox1.IsChecked = false;
                    }

                
                });
            }
        }

        private void save_set()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                string Is_remember_psw_temp = "F";
                string Is_auto_login_temp = "F";
                if (checkBox2.IsChecked == true)
                {
                    Is_remember_psw_temp = "T";
                }
                if (checkBox1.IsChecked == true)
                {
                    Is_auto_login_temp = "T";
                }
                txt_c.Txt_File_Writer(name + " " + psw + " " + host + " " + port + " " + Is_remember_psw_temp + " " + Is_auto_login_temp);
            });
        }

        private void Log_off_frm()//加载离线时窗体样貌
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                app.is_Login = 0;
                is_login = 0;
                progressOverlay1.Hide();
                roundButton2.IsEnabled = true;
                roundButton1.IsEnabled = true;
            });
        }

        private void Log_ing_frm()//加载登录时窗体样貌
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                app.is_Login = 2;
                if (is_login != 2)
                {
                    progressOverlay1.Show();
                }
                is_login = 2;
                roundButton2.IsEnabled = false;
                roundButton1.IsEnabled = false;
            });
        }


        private void Log_ed_frm()//加载登录后窗体样貌
        {
            app.is_Login = 1;
            is_login = 1;
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }


        private void roundButton3_Click(object sender, RoutedEventArgs e)//取消
        {
            if (app.is_Login == 2)
            {
                app.Logoff();
            }
            else
            {
                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
                else
                {
                    this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                }
            }
        }

        private void roundButton1_Click(object sender, RoutedEventArgs e)//设置
        {
            this.NavigationService.Navigate(new Uri("/SettingPage.xaml", UriKind.Relative));
        }

        private void roundButton2_Click(object sender, RoutedEventArgs e)//登录
        {
            if (phoneTextBox1.Text == "")
            {
                MessageBox.Show("用户名不能为空");
            }
            else
            {
                app.Login(phoneTextBox1.Text, passwordBox1.Password);
                save_set();
            }
        }

        private void checkBox2_Checked(object sender, RoutedEventArgs e)
        {
            checkBox1.IsEnabled = true;
        }

        private void checkBox2_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBox1.IsEnabled = false;
        }

        private void phoneTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            name = phoneTextBox1.Text;
        }

        private void passwordBox1_PasswordChanged(object sender, RoutedEventArgs e)
        {
            psw = passwordBox1.Password;
        }

        
    }
}