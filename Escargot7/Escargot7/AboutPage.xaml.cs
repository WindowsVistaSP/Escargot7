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

using System.Reflection;

namespace Escargot7
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            string build_num = GetAssemblyVersion();
            if (build_num != "E")
            {
                string[] build_num_splited = build_num.Split('.');
                build_t.Text = "版本" + build_num_splited[0] + "." + build_num_splited[1] + "(Build" + build_num_splited[2] + "." + build_num_splited[3] + ")";
            }

        }

        public static string GetAssemblyVersion()//此处参考其他博客
        {
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                string fullName = asm.FullName;
                string[] parts = fullName.Split(',');
                string versionPart = parts[1];
                string[] versionSplit = versionPart.Split('=');
                return versionSplit[1];
            }
            catch
            {
                return "E";
            }

        }

        private void roundButton2_Click(object sender, RoutedEventArgs e)
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
}