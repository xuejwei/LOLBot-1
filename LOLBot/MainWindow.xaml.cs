﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Flurl.Http;
using System.Globalization;

using MessageBox = System.Windows.MessageBox;

using Automation.WinApi;
using System.Security.Permissions;

namespace LOLBot
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        WindowInteropHelper interopHelper;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            interopHelper = new WindowInteropHelper(this);
            //获得消息源
            HwndSource source = HwndSource.FromHwnd(interopHelper.Handle);
            //判断消息源是否有效
            if (null == source)
            {
                //如果消息源无效
                MessageBox.Show("绑定热键失败，快找我反馈 BUG");
            }
            else
            {
                //挂接事件
                source.AddHook(HotKeyHook);
                bool regF9 = User32.RegisterHotKey(interopHelper.Handle, 233, 0, (int)Keys.F9);
                if (!regF9)
                {
                    this.tipLabel.Content = "F9 热键注册失败";
                }
            }

            if(!InputManager.ShareInstance().Load())
            {
                MessageBox.Show("你还没有安装键盘鼠标模拟器的驱动。请看使用说明");
            }
            InputManager.ShareInstance().OnKeyPressed += MainWindow_OnKeyPressed1;
            //requesAsync();
        }

        private async Task requesAsync()
        {
            string text = await "http://api.mmuaa.com/gettime".GetStringAsync();
            DateTime now = DateTime.ParseExact(text, "#yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            DateTime time = DateTime.ParseExact("#20180116000001", "#yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            if (now.CompareTo(time) > 1)
            {
                stopBot();
                MessageBox.Show("应用有效期截至 2018 年 1 月 16 日，现在无法使用，请更新最新版");
            }
        }

        private IntPtr HotKeyHook(IntPtr hWnd, int msg, IntPtr wideParam, IntPtr longParam, ref bool handled)
        {
            switch (msg)
            {
                //热键消息
                case 0x0312:
                    if (wideParam.ToInt32() == 233)
                    {
                        //按了 F9
                        if(startButton.IsEnabled) startButton_Click(null, null);
                    }

                    break;

            }
            return IntPtr.Zero;
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            return;
            if (InputManager.ShareInstance().deviceId != 0)
            {
                startBot();
            }
            else
            {
                MessageBox.Show("每次启动后，需要按一次键盘后才能开启");
            }
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            stopBot();
        }

        private void startBot()
        {
            if(!InputManager.ShareInstance().IsLoaded)
            {
                InputManager.ShareInstance().Load();
            }

            Bot.Start();
            Bot.championNames = championNamesTextBox.Text.Split('|');

            User32.UnregisterHotKey(interopHelper.Handle, 233);
            
            this.startButton.IsEnabled = false;
            this.stopButton.IsEnabled = true;
        }

        private void MainWindow_OnKeyPressed1(object sender, Interceptor.KeyPressedEventArgs e)
        {
            Dispatcher.Invoke((Action)delegate
            {

                if (e.Key == Interceptor.Keys.F10 && !startButton.IsEnabled)
                {
                    stopBot();
                }
            });
        }

        private void MainWindow_OnKeyPressed(object sender, Interceptor.KeyPressedEventArgs e)
        {
            Console.WriteLine(1);
        }

        private void stopBot()
        {
            InputManager.ShareInstance().Unload();

            Bot.Stop(null);
            User32.RegisterHotKey(interopHelper.Handle, 233, 0, (int)Keys.F9);

            this.startButton.IsEnabled = true;
            this.stopButton.IsEnabled = false;
        }

        
    }
}
