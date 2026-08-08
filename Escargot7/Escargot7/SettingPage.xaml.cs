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
using System.Windows.Media.Imaging;
using System.Threading;
using Microsoft.Xna.Framework.GamerServices;
using System.Windows.Navigation;
using MSN_Protocol;

namespace Escargot7
{
    public partial class SettingPage : PhoneApplicationPage
    {
        public SettingPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SettingPage_Loaded);
        }

        TXT_Class txt_c = new TXT_Class();
        
        private string name = "";
        //public string nic_name = "";
        private string psw = "";
        private string host = "143.198.4.104";
        private int port = 1863;

        private bool Is_remember_psw = false;//是否记住密码
        private bool Is_auto_login = false;//是否自动登录
        //public bool Is_remember_messenge = false;//是否记住消息

        private void SettingPage_Loaded(object sender, RoutedEventArgs e)
        {
            load_set();
        }


        private void roundButton3_Click(object sender, RoutedEventArgs e)//取消
        {
            Guide.BeginShowMessageBox(
         "放弃？",
         "你确定要放弃以上配置吗？",
           new[] { "确定", "取消" },
           0,
        MessageBoxIcon.Alert,
        result =>
        {
            int? chosenIndexNullable = Guide.EndShowMessageBox(result);

            if (chosenIndexNullable.HasValue)
            {
                int chosenIndex = chosenIndexNullable.Value;

                if (chosenIndex == 0)
                {
                    //确定
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {

                        if (NavigationService.CanGoBack)
                        {
                            NavigationService.GoBack();
                        }
                        else
                        {
                            this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                        }
                    });
                }
                else if (chosenIndex == 1)
                {
                    //取消
                }
            }
            else
            {
            }
        },
           null
       );

        }

        private void roundButton1_Click(object sender, RoutedEventArgs e)//保存
        {
            Guide.BeginShowMessageBox(
         "保存？",
         "你确定要保存以上配置吗？",
           new[] { "确定", "取消" },
           0,
        MessageBoxIcon.Alert,
        result =>
        {
            int? chosenIndexNullable = Guide.EndShowMessageBox(result);

            if (chosenIndexNullable.HasValue)
            {
                int chosenIndex = chosenIndexNullable.Value;

                if (chosenIndex == 0)
                {
                    //确定
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("保存将在下次登录时生效！");
                        save_set();
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {

                            if (NavigationService.CanGoBack)
                            {
                                NavigationService.GoBack();
                            }
                            else
                            {
                                this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                            }
                        });
                    });
                }
                else if (chosenIndex == 1)
                {
                    //取消
                }
            }
            else
            {
            }
        },
           null
       );
        }

        private void button1_Click(object sender, RoutedEventArgs e)//使用默认设置
        {

        }

        private void button2_Click(object sender, RoutedEventArgs e)//删除聊天记录
        {

        }
        /*
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

        }
         */

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            // 在这里添加你想要执行的代码，例如：
            // 判断是否要退出页面，或者弹窗询问用户[citation:7][citation:11]

            // 如果想让程序不执行默认的“返回”动作（例如，你想先关闭一个弹出的对话框），
            // 可以设置 e.Cancel = true; [citation:2][citation:6][citation:7]

            // 如果不设置 e.Cancel = true，应用程序会继续执行默认的页面返回或退出逻辑[citation:3][citation:5]
            base.OnBackKeyPress(e);
            e.Cancel = true;
            Guide.BeginShowMessageBox(
         "放弃？",
         "你确定要放弃以上配置吗？",
           new[] { "确定", "取消" },
           0,
        MessageBoxIcon.Alert,
        result =>
        {
            int? chosenIndexNullable = Guide.EndShowMessageBox(result);

            if (chosenIndexNullable.HasValue)
            {
                int chosenIndex = chosenIndexNullable.Value;

                if (chosenIndex == 0)
                {
                    //确定
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {

                        if (NavigationService.CanGoBack)
                        {
                            NavigationService.GoBack();
                        }
                        else
                        {
                            this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                        }
                    });
                }
                else if (chosenIndex == 1)
                {
                    //取消
                }
            }
            else
            {
            }
        },
           null
       );
        }

        private void toggleSwitch1_Checked(object sender, RoutedEventArgs e)
        {
            Is_remember_psw = true;
            toggleSwitch2.IsEnabled = true;
        }

        private void toggleSwitch1_Unchecked(object sender, RoutedEventArgs e)
        {
            Is_remember_psw = false;
            toggleSwitch2.IsEnabled = false;
        }

        private void toggleSwitch2_Checked(object sender, RoutedEventArgs e)
        {
            if (Is_remember_psw)
            {
                Is_auto_login = true;
            }
            else
            {
                Is_auto_login = false;
            }
        }

        private void toggleSwitch2_Unchecked(object sender, RoutedEventArgs e)
        {
            Is_auto_login = false;
        }

        private void button1_Click_1(object sender, RoutedEventArgs e)//使用默认配置
        {
            Guide.BeginShowMessageBox(
         "默认配置？",
         "你确定要使用默认配置吗？",
           new[] { "确定", "取消" },
           0,
        MessageBoxIcon.Alert,
        result =>
        {
            int? chosenIndexNullable = Guide.EndShowMessageBox(result);

            if (chosenIndexNullable.HasValue)
            {
                int chosenIndex = chosenIndexNullable.Value;

                if (chosenIndex == 0)
                {
                    //确定
                    normal_set();
                }
                else if (chosenIndex == 1)
                {
                    //取消
                }
            }
            else
            {
            }
        },
           null
       );
        }

        private void save_set()
        {
            string Is_remember_psw_temp = "F";
            string Is_auto_login_temp = "F";
            if (Is_remember_psw)
            {
                Is_remember_psw_temp = "T";
            }
            if (Is_auto_login)
            {
                Is_auto_login_temp = "T";
            }
            txt_c.Txt_File_Writer(name + " " + psw + " " + host + " " + port + " " + Is_remember_psw_temp + " " + Is_auto_login_temp);
        }

        private void load_set()
        {
            string[] read_temp = txt_c.Txt_File_Reader().Split(new[] { " " }, StringSplitOptions.None);
            if (read_temp.Length < 6)
            {
                MessageBox.Show("配置文件有误，将会使用默认设置！");
                normal_set();
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    name = read_temp[0];
                    psw = read_temp[1];

                    host = read_temp[2];
                    port = int.Parse(read_temp[3]);

                    textBox1.Text = host;
                    textBox2.Text = port + "";

                    if (read_temp[4] == "T")
                    {
                        Is_remember_psw = true;
                        toggleSwitch1.IsChecked = true;
                        toggleSwitch2.IsEnabled = true;
                    }
                    else
                    {
                        Is_remember_psw = false;
                        toggleSwitch1.IsChecked = false;
                        toggleSwitch2.IsEnabled = false;
                    }

                    if (read_temp[5] == "T")
                    {
                        Is_auto_login = true;
                        toggleSwitch2.IsChecked = true;
                    }
                    else
                    {
                        Is_auto_login = false;
                        toggleSwitch2.IsChecked = false;
                    }



                });
            }
        }

        private void normal_set()
        {
            //默认配置
            host = "143.198.4.104";
            port = 1863;

            Is_remember_psw = false;//是否记住密码
            Is_auto_login = false;//是否自动登录

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                textBox1.Text = host;
                textBox2.Text = port + "";
                toggleSwitch1.IsChecked = false;
                toggleSwitch2.IsChecked = false;
                toggleSwitch2.IsEnabled = false;

            });
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((textBox1.Text == "")||(textBox2.Text == ""))
            {
                roundButton1.IsEnabled = false;
            }
            else
            {
                host = textBox1.Text;
                roundButton1.IsEnabled = true;
            }
        }

        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            if ((textBox1.Text == "") || (textBox2.Text == ""))
            {
                roundButton1.IsEnabled = false;
            }
            else
            {
                try
                {
                    port = int.Parse(textBox2.Text);
                }
                catch
                {
                    port = 1863;
                }
                roundButton1.IsEnabled = true;
            }
        }


    }
}