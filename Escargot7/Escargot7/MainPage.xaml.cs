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

using MSN_Protocol;

namespace Escargot7
{
    public partial class MainPage : PhoneApplicationPage
    {
        // 构造函数
        public MainPage()
        {
            InitializeComponent();

            // 将 listbox 控件的数据上下文设置为示例数据
            //DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        App app = Application.Current as App;


        private bool is_double_click = false;
        private int list_update_temp = 0;
        private List<string[]> Friends_list = new List<string[]>();
        // 为 ViewModel 项加载数据
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            /*
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
            */
            app.Change_Is_watching("");
            app.LoginStateChangedEvent += LoginStateChanged;
            app.FriendListChangedEvent += FriendListChanged;
            app.MesChangedEvent += MesChanged;
            app.MesFrmChangedEvent += MesFrmChanged;

            while (NavigationService.CanGoBack)
            {
                NavigationService.RemoveBackEntry();
            }

            if (app.is_Login == 1)
            {
                Log_ed_frm();
                app.is_First = false;
            }
            else
            {
                Log_off_frm();
                if (app.is_First)
                {
                    this.NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
                    app.is_First = false;
                }
                app.is_First = false;
            }

            
            
        }

        public void LoginStateChanged(App sender, LoginStateChangedEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                textBlock1.Text = e.LoginNicName;
                textBlock2.Text = e.LoginName;
                if (e.LoginState == "OUT")
                {
                    this.NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
                }
                else if (e.LoginState == "ING")
                {
                    this.NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
                }
                else
                {
                    switch (e.LoginState)
                    {
                        case "NLN":
                            image1.Source = new BitmapImage(new Uri("images\\chg\\nln.png", UriKind.Relative));
                            break;
                        case "AWY":
                            image1.Source = new BitmapImage(new Uri("images\\chg\\awy.png", UriKind.Relative));
                            break;
                        case "BSY":
                            image1.Source = new BitmapImage(new Uri("images\\chg\\bsy.png", UriKind.Relative));
                            break;
                        default:
                            image1.Source = new BitmapImage(new Uri("images\\chg\\fln.png", UriKind.Relative));
                            break;
                    }
                    Log_ed_frm();
                }
            });
        }

        public void MesChanged(App sender, MesChangedEventArgs e)
        {

            //System.Diagnostics.Debug.WriteLine("[ds_ns_debug: " + e.Message + "]");
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {

                FirstListBox.Items.Clear();

            });

            for (int i = 0; i < Friends_list.Count; i++)
            {
                /*
                System.Diagnostics.Debug.WriteLine("A：" + Friends_list[i].Length + "[end]");
                for (int j = 0; j < Friends_list[i].Length; j++)
                {
                    System.Diagnostics.Debug.WriteLine("[B：" + Friends_list[i][j] + " end]");
                }
                */
                string chg_temp = Friends_list[i][2];
                string name_temp = Friends_list[i][0];
                string nic_name_temp = Friends_list[i][1];
                int num_temp = 0;
                if (app.new_mes_num_list.Count != 0)
                {
                    for (int j = 0; j < app.new_mes_num_list.Count; j++)
                    {
                        if (app.new_mes_num_list[j][0] == name_temp)
                        {
                            num_temp = int.Parse(app.new_mes_num_list[j][1]);
                            break;
                        }
                    }
                }

                //System.Diagnostics.Debug.WriteLine("[UI: " + e.Fri_name + " " + num_temp + "]");

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    ListBoxItem newItem = new ListBoxItem();
                    newItem.Tap += FirstListBox_Tap;
                    newItem.Content = make_list_grid(chg_temp, name_temp, nic_name_temp, num_temp);

                    FirstListBox.Items.Add(newItem);
                });

            }

            is_double_click = false;

        }

        public void MesFrmChanged(App sender, MesFrmChangedEventArgs e)
        {
        }

        public void FriendListChanged(App sender, FriendListChangedEventArgs e)
        {

            Friends_list = e.Friend_list;

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {

                FirstListBox.Items.Clear();
                /*
                FirstListBox.Visibility = Visibility.Collapsed;
                progressOverlay1.Show();
                 */

            });
            Thread th_temp = new Thread(Update_list);
            th_temp.Start();

                for (int i = 0; i < Friends_list.Count; i++)
                {
                    string chg_temp = Friends_list[i][2];
                    string name_temp = Friends_list[i][0];
                    string nic_name_temp = Friends_list[i][1];
                    int num_temp = 0;
                    if (app.new_mes_num_list.Count != 0)
                    {
                        for (int j = 0; j < app.new_mes_num_list.Count; j++)
                        {
                            if (app.new_mes_num_list[j][0] == name_temp)
                            {
                                num_temp = int.Parse(app.new_mes_num_list[j][1]);
                                break;
                            }
                        }
                    }


                    Thread.Sleep(1);

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        ListBoxItem newItem = new ListBoxItem();
                        newItem.Tap += FirstListBox_Tap;
                        newItem.Content = make_list_grid(chg_temp, name_temp, nic_name_temp, num_temp);

                        FirstListBox.Items.Add(newItem);
                    });

                }
            /*
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    progressOverlay1.Hide();
                    FirstListBox.Visibility = Visibility.Visible;

                });
             */
                is_double_click = false;

        }

        private void Update_list()//防闪烁
        {
            if (list_update_temp == 0)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    FirstListBox.Visibility = Visibility.Collapsed;
                    progressOverlay1.Show();

                });
            }
            list_update_temp++;
            Thread.Sleep(1000);
            list_update_temp--;
            if (list_update_temp == 0)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    progressOverlay1.Hide();
                    FirstListBox.Visibility = Visibility.Visible;
                });
            }
        }

        private void Log_off_frm()//加载离线时窗体样貌
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                app.is_Login = 0;
                roundButton1.IsEnabled = false;
                roundButton2.IsEnabled = false;
                roundButton3.IsEnabled = false;
                roundButton7.IsEnabled = false;
                roundButton4.Content = "登录";
                chg_l_p.IsEnabled = false;
                FirstListBox.Items.Clear();
                image1.Source = new BitmapImage(new Uri("images\\chg\\fln.png", UriKind.Relative));
                progressOverlay1.Hide();
            });
        }

        private void Log_ing_frm()//加载登录时窗体样貌
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
            app.is_Login = 2;
            roundButton1.IsEnabled = false;
            roundButton2.IsEnabled = false;
            roundButton3.IsEnabled = false;
            roundButton7.IsEnabled = false;
            roundButton4.Content = "登录";
            chg_l_p.IsEnabled = false;
            FirstListBox.Items.Clear();
            image1.Source = new BitmapImage(new Uri("images\\chg\\fln.png", UriKind.Relative));
            progressOverlay1.Hide();
            });
        }


        private void Log_ed_frm()//加载登录后窗体样貌
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
            app.is_Login = 1;
            if (FirstListBox.SelectedItem == null)
            {
                roundButton1.IsEnabled = false;
                roundButton3.IsEnabled = false;
                
            }
            else
            {
                roundButton1.IsEnabled = true;
                if (Friends_list[FirstListBox.SelectedIndex][2] == "STG")
                {
                    roundButton3.IsEnabled = false;
                }
                else
                {
                    roundButton3.IsEnabled = true;
                }
            }
            roundButton2.IsEnabled = true;
            roundButton7.IsEnabled = true;
            roundButton4.Content = "注销";
            chg_l_p.IsEnabled = true;
            });
        }


        private void button3_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/ChatPage.xaml", UriKind.Relative));
        }

        

        private void roundButton2_Click(object sender, RoutedEventArgs e)//添加好友
        {
            var input = new InputPrompt();
            input.Title = "添加好友";
            input.Message = "请输入好友名称：";
            input.Value = "";
            input.IsCancelVisible = true;
            input.Completed += (s, ev) =>
            {
                if (ev.PopUpResult == PopUpResult.Ok)
                {
                    if (ev.Result != "")
                    {
                        app.Add_friend(ev.Result);
                    }
                    else
                    {
                        MessageBox.Show("不能为空！");
                    }
                }
                else if (ev.PopUpResult == PopUpResult.Cancelled)
                {
                    //MessageBox.Show("操作已取消");
                }
            };
            input.Show();
        }

        private void roundButton1_Click(object sender, RoutedEventArgs e)//发起聊天
        {
            if ((FirstListBox.SelectedItem != null) && (Friends_list.Count != 0))
            {
                this.NavigationService.Navigate(new Uri("/ChatPage.xaml?fri_name=" + Friends_list[FirstListBox.SelectedIndex][0] + "&fri_nic_name=" + Friends_list[FirstListBox.SelectedIndex][1] + "&fri_state=" + Friends_list[FirstListBox.SelectedIndex][2], UriKind.Relative));
            }
        }

        private void roundButton3_Click(object sender, RoutedEventArgs e)//删除好友
        {
            if (FirstListBox.SelectedItem != null)
            {
                app.Rem_friend(Friends_list[FirstListBox.SelectedIndex][0]);
            }
        }

        private void SelectionChanged_CHG(object sender, SelectionChangedEventArgs e)//状态变化
        {
            if (app.is_Login == 1)
            {
                ListPickerItem item = chg_l_p.SelectedItem as ListPickerItem;
                if (item != null)
                {
                    app.ChangeCHG(item.Tag as string);
                }
            }
        }

        private void roundButton4_Click(object sender, RoutedEventArgs e)//注销,登录
        {
            app.Allow_auto_login = false;
            if (app.is_Login == 1)
            {
                app.Logoff();
                this.NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
            }
            else
            {
                this.NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
            }
        }

        private void roundButton5_Click(object sender, RoutedEventArgs e)//设置
        {
            this.NavigationService.Navigate(new Uri("/SettingPage.xaml", UriKind.Relative));
        }

        private void roundButton6_Click(object sender, RoutedEventArgs e)//关于
        {
            this.NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void roundButton7_Click(object sender, RoutedEventArgs e)//编辑昵称
        {
            var input = new InputPrompt();
            input.Title = "更改昵称";
            input.Message = "请输入新昵称：";
            input.Value = app.nic_name;
            input.IsCancelVisible = true;
            input.Completed += (s, ev) =>
            {
                if (ev.PopUpResult == PopUpResult.Ok)
                {
                    if (ev.Result != "")
                    {
                        app.ChangeNicName(ev.Result);
                    }
                    else
                    {
                        MessageBox.Show("昵称不能为空！");
                    }
                }
                else if (ev.PopUpResult == PopUpResult.Cancelled)
                {
                    //MessageBox.Show("操作已取消");
                }
            };
            input.Show();
        }

        private void FirstListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            is_double_click = false;
            Log_ed_frm();
        }

        private void FirstListBox_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!is_double_click)
            {
                is_double_click = true;
            }
            else
            {
                if ((FirstListBox.SelectedItem != null) && (Friends_list.Count != 0))
                {
                    this.NavigationService.Navigate(new Uri("/ChatPage.xaml?fri_name=" + Friends_list[FirstListBox.SelectedIndex][0] + "&fri_nic_name=" + Friends_list[FirstListBox.SelectedIndex][1] + "&fri_state=" + Friends_list[FirstListBox.SelectedIndex][2], UriKind.Relative));
                }

                is_double_click = false;
            }
        }

        private Grid make_list_grid(string list_chg, string list_fri_name, string list_fri_nic_name, int mes_num)
        {
            Grid grid = new Grid();
            grid.Height = 100;
            grid.HorizontalAlignment = HorizontalAlignment.Left;
            grid.Margin = new Thickness(12, 0, 0, 0);
            grid.VerticalAlignment = VerticalAlignment.Top;
            grid.Width = 456;

            Image image = new Image();
            image.Height = 80;
            image.HorizontalAlignment = HorizontalAlignment.Left;
            image.Margin = new Thickness(7, 10, 0, 0);
            image.VerticalAlignment = VerticalAlignment.Top;
            image.Width = 80;
            image.Stretch = Stretch.Fill;

            switch (list_chg)
            {
                case "NLN":
                    image.Source = new BitmapImage(new Uri("images\\chg\\nln.png", UriKind.Relative));
                    break;
                case "AWY":
                    image.Source = new BitmapImage(new Uri("images\\chg\\awy.png", UriKind.Relative));
                    break;
                case "BSY":
                    image.Source = new BitmapImage(new Uri("images\\chg\\bsy.png", UriKind.Relative));
                    break;
                case "STG":
                    image.Source = new BitmapImage(new Uri("images\\chg\\stg.png", UriKind.Relative));
                    break;
                default:
                    image.Source = new BitmapImage(new Uri("images\\chg\\fln.png", UriKind.Relative));
                    break;
            }

            TextBlock NicNameTextBlock = new TextBlock();
            NicNameTextBlock.Height = 47;
            NicNameTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
            NicNameTextBlock.Margin = new Thickness(93, 10, 0, 0);
            NicNameTextBlock.VerticalAlignment = VerticalAlignment.Top;
            NicNameTextBlock.Width = 298;
            NicNameTextBlock.Text = list_fri_nic_name;
            NicNameTextBlock.TextTrimming = TextTrimming.WordEllipsis;
            NicNameTextBlock.FontSize = 32;

            TextBlock NameTextBlock = new TextBlock();
            NameTextBlock.Height = 30;
            NameTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
            NameTextBlock.Margin = new Thickness(93, 60, 0, 0);
            NameTextBlock.VerticalAlignment = VerticalAlignment.Top;
            NameTextBlock.Width = 357;
            NameTextBlock.Text = list_fri_name;
            NameTextBlock.TextTrimming = TextTrimming.WordEllipsis;

            TextBlock NewMesTextBlock = new TextBlock();
            NewMesTextBlock.Height = 30;
            NewMesTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
            NewMesTextBlock.Margin = new Thickness(402, 19, 0, 0);
            NewMesTextBlock.VerticalAlignment = VerticalAlignment.Top;
            NewMesTextBlock.Width = 35;
            NewMesTextBlock.TextAlignment = TextAlignment.Center;
            NewMesTextBlock.Foreground = new SolidColorBrush(Colors.White);

            Rectangle NewMesRectangle = new Rectangle();
            NewMesRectangle.Height = 47;
            NewMesRectangle.HorizontalAlignment = HorizontalAlignment.Left;
            NewMesRectangle.Margin = new Thickness(397, 10, 0, 0);
            NewMesRectangle.VerticalAlignment = VerticalAlignment.Top;
            NewMesRectangle.Width = 47;
            NewMesRectangle.StrokeThickness = 1;

            if (mes_num > 99)
            {
                NewMesTextBlock.Text = "99+";
                NewMesRectangle.Fill = (Brush)Application.Current.Resources["PhoneAccentBrush"];
            }
            else if (mes_num > 0)
            {
                NewMesTextBlock.Text = mes_num + "";
                NewMesRectangle.Fill = (Brush)Application.Current.Resources["PhoneAccentBrush"];
            }


            grid.Children.Add(image);
            grid.Children.Add(NicNameTextBlock);
            grid.Children.Add(NameTextBlock);
            grid.Children.Add(NewMesRectangle);
            grid.Children.Add(NewMesTextBlock);

            return grid;
        }

        


    }
}