using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace 斗图UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class About : Page
    {
        public About()
        {
            this.InitializeComponent();
            this.Loaded += About_Loaded;
        }

        private void About_Loaded(object sender, RoutedEventArgs e)
        {
            setStateBar();
            setBackKey();
        }

        private async void setStateBar()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var stateBar = StatusBar.GetForCurrentView();
                await stateBar.ShowAsync();
                stateBar.BackgroundOpacity = 1;
                stateBar.BackgroundColor = Colors.RoyalBlue;
                stateBar.ForegroundColor = Colors.WhiteSmoke;
            }
            else
            {

            }
        }

        private void setBackKey()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += (s, t) =>
                {
                    t.Handled = true;
                    Frame.Navigate(typeof(MainPage));
                };
            }
            //那就是在电脑上运行
            else
            {
                //更改标题栏颜色
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                titleBar.BackgroundColor = Colors.DodgerBlue;
                SystemNavigationManager.GetForCurrentView().BackRequested += BingWallpaperDetailsPage_BackRequested;
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
        }
        private void BingWallpaperDetailsPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
