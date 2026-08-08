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
using System.Windows.Navigation;
using System.Windows.Media.Imaging;
using MSN_Protocol;
namespace Escargot7
{
    public partial class ChatPage : PhoneApplicationPage
    {
        public ChatPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ChatPage_Loaded);
        }


        private string fri_name = "";
        private string fri_nic_name = "";
        private string fri_state = "FLN";
        App app = Application.Current as App;

        private void ChatPage_Loaded(object sender, RoutedEventArgs e)
        {
            app.Change_Is_watching(fri_name);
            app.LoginStateChangedEvent += LoginStateChanged;
            app.FriendListChangedEvent += FriendListChanged;
            app.MesChangedEvent += MesChanged;
            app.MesFrmChangedEvent += MesFrmChanged;

            List<string[]> mes_list = app.Get_mes_frm(fri_name);
            if (mes_list.Count != 0)
            {
                for (int i = 0; i < mes_list.Count; i++)
                {
                    int o_s_temp = 1;
                    if (mes_list[i][0] == fri_name)
                    {
                        o_s_temp = 0;
                    }
                    Add_Mes_Box(mes_list[i][1], mes_list[i][2], o_s_temp);
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if ((NavigationContext.QueryString.ContainsKey("fri_name")) && (NavigationContext.QueryString.ContainsKey("fri_nic_name")) && (NavigationContext.QueryString.ContainsKey("fri_state")))
            {
                fri_name = System.Uri.UnescapeDataString(NavigationContext.QueryString["fri_name"]);
                fri_nic_name = System.Uri.UnescapeDataString(NavigationContext.QueryString["fri_nic_name"]);
                fri_state = System.Uri.UnescapeDataString(NavigationContext.QueryString["fri_state"]);
                //MessageBox.Show(fri_name);
                Update_fri_name();
                Update_add_fri_b();
                Update_send_mes_b();
                Check_frm_state();
                /*
                if (fri_name == "")
                {
                    MessageBox.Show("参数错误！");
                }
                 * */
            }
            else
            {
                MessageBox.Show("参数错误！");
            }
        }

        public void LoginStateChanged(App sender, LoginStateChangedEventArgs e)
        {
            Update_send_mes_b();
        }

        public void MesChanged(App sender, MesChangedEventArgs e)
        {
            if ((e.Fri_name == fri_name) && (e.Date != ""))
            {
                Add_Mes_Box(e.Message, e.Date, 0);
            }
        }

        public void MesFrmChanged(App sender, MesFrmChangedEventArgs e)
        {
            Update_send_mes_b();
            Check_frm_state();
        }

        public void FriendListChanged(App sender, FriendListChangedEventArgs e)
        {
            List<string[]> Friends_list = e.Friend_list;

            for (int i = 0; i < Friends_list.Count; i++)
            {
                if(Friends_list[i][0] == fri_name)
                {
                    fri_name = Friends_list[i][0];
                    fri_nic_name = Friends_list[i][1];
                    fri_state = Friends_list[i][2];
                    Update_fri_name();
                    Update_add_fri_b();
                    Update_send_mes_b();
                }
            }
        }

        private void roundButton3_Click(object sender, RoutedEventArgs e)//退出
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

        private void roundButton1_Click(object sender, RoutedEventArgs e)//添加为好友
        {
            if ((fri_state == "STG") && (app.is_Login == 1))
            {
                app.Add_friend(fri_name);
            }
        }

        private void roundButton2_Click(object sender, RoutedEventArgs e)//发送
        {
            int frm_temp = app.Get_mes_frm_state(fri_name);
            if ((fri_state != "FLN") && (frm_temp == 1) && (app.is_Login == 1) && (phoneTextBox1.Text != ""))
            {
                app.Send_mes_frm(fri_name, phoneTextBox1.Text);
                Add_Mes_Box(phoneTextBox1.Text, DateTime.Now.ToString(), 1);
                phoneTextBox1.Text = "";
            }
        }

        private void phoneTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            Update_send_mes_b();
        }

        private void Update_add_fri_b()//更新roundButton1
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if ((fri_state == "STG") && (app.is_Login == 1))
                {
                    roundButton1.IsEnabled = true;
                }
                else
                {
                    roundButton1.IsEnabled = false;
                }
            });
        }

        private void Update_send_mes_b()//更新roundButton2
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                int frm_temp = app.Get_mes_frm_state(fri_name);
                System.Diagnostics.Debug.WriteLine(frm_temp + "");
                if ((fri_state != "FLN") && (frm_temp == 1) && (app.is_Login == 1) && (phoneTextBox1.Text != ""))
                {
                    roundButton2.IsEnabled = true;
                }
                else
                {
                    roundButton2.IsEnabled = false;
                }
            });
        }

        private void Check_frm_state()
        {
            if ((fri_state != "FLN") && (app.is_Login == 1))
            {
                int frm_temp = app.Get_mes_frm_state(fri_name);
                if (frm_temp == 0)
                {
                    app.Create_chat(fri_name);
                }
            }
        }

        private void Update_fri_name()//更新好友状态
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                switch (fri_state)
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
                    case "STG":
                        image1.Source = new BitmapImage(new Uri("images\\chg\\stg.png", UriKind.Relative));
                        break;
                    default:
                        image1.Source = new BitmapImage(new Uri("images\\chg\\fln.png", UriKind.Relative));
                        break;
                }
                textBlock1.Text = fri_nic_name;
                textBlock2.Text = fri_name;
            });
        }

        private void Add_Mes_Box(string mes, string time, int s_or_o)//处理消息框
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (s_or_o == 1)//自己发的
                {
                    ChatBubble bubble = new ChatBubble();
                    bubble.Width = 350;
                    //bubble.Height = 120;
                    bubble.HorizontalAlignment = HorizontalAlignment.Right;
                    bubble.VerticalAlignment = VerticalAlignment.Top;
                    bubble.ChatBubbleDirection = ChatBubbleDirection.LowerRight;
                    bubble.Background = (Brush)Application.Current.Resources["PhoneAccentBrush"];

                    Grid grid = new Grid();
                    grid.Width = 350;

                    RowDefinition row1 = new RowDefinition();
                    row1.Height = GridLength.Auto;
                    grid.RowDefinitions.Add(row1);

                    RowDefinition row2 = new RowDefinition();
                    row2.Height = GridLength.Auto;
                    grid.RowDefinitions.Add(row2);

                    TextBlock mainText = new TextBlock();
                    mainText.Text = mes;//mes
                    mainText.TextWrapping = TextWrapping.Wrap;
                    mainText.FontSize = 22;
                    mainText.Foreground = new SolidColorBrush(Colors.White);
                    mainText.Margin = new Thickness(10, 10, 24, 10);
                    Grid.SetRow(mainText, 0);

                    TextBlock timeText = new TextBlock();
                    timeText.Text = time;//time
                    timeText.HorizontalAlignment = HorizontalAlignment.Right;
                    timeText.FontSize = 18;
                    timeText.Foreground = new SolidColorBrush(Color.FromArgb(170, 255, 255, 255));
                    timeText.Margin = new Thickness(0, 0, 24, 10);
                    timeText.Width = 200;
                    timeText.TextAlignment = TextAlignment.Right;

                    Grid.SetRow(timeText, 1);
                    grid.Children.Add(mainText);
                    grid.Children.Add(timeText);

                    bubble.Content = grid;

                    stackPanel1.Children.Add(bubble);
                    scrollViewer1.UpdateLayout();
                    scrollViewer1.ScrollToVerticalOffset(scrollViewer1.ExtentHeight);

                }
                else//别人发的
                {
                    ChatBubble bubble = new ChatBubble();
                    bubble.Width = 350;
                    //bubble.Height = 120;
                    bubble.HorizontalAlignment = HorizontalAlignment.Left;
                    bubble.VerticalAlignment = VerticalAlignment.Top;
                    bubble.ChatBubbleDirection = ChatBubbleDirection.LowerLeft;
                    bubble.Background = new SolidColorBrush(Colors.DarkGray);

                    Grid grid = new Grid();
                    grid.Width = 350;

                    RowDefinition row1 = new RowDefinition();
                    row1.Height = GridLength.Auto;
                    grid.RowDefinitions.Add(row1);

                    RowDefinition row2 = new RowDefinition();
                    row2.Height = GridLength.Auto;
                    grid.RowDefinitions.Add(row2);

                    TextBlock mainText = new TextBlock();
                    mainText.Text = mes;//mes
                    mainText.TextWrapping = TextWrapping.Wrap;
                    mainText.FontSize = 22;
                    mainText.Foreground = new SolidColorBrush(Colors.Black);
                    mainText.Margin = new Thickness(10, 10, 24, 10);
                    Grid.SetRow(mainText, 0);

                    TextBlock timeText = new TextBlock();
                    timeText.Text = time;//time
                    timeText.HorizontalAlignment = HorizontalAlignment.Right;
                    timeText.FontSize = 18;
                    timeText.Foreground = new SolidColorBrush(Color.FromArgb(170, 0, 0, 0));
                    timeText.Margin = new Thickness(0, 0, 24, 10);
                    timeText.Width = 200;
                    timeText.TextAlignment = TextAlignment.Right;

                    Grid.SetRow(timeText, 1);
                    grid.Children.Add(mainText);
                    grid.Children.Add(timeText);

                    bubble.Content = grid;

                    stackPanel1.Children.Add(bubble);
                    scrollViewer1.UpdateLayout();
                    scrollViewer1.ScrollToVerticalOffset(scrollViewer1.ExtentHeight);
                }
            });
        }

        

    }
}