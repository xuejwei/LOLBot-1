﻿using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Drawing;
using System.Timers;
using System.Collections.Generic;

using Automation;
using Automation.WinApi;

namespace LOLBot
{
    class Bot
    {
        static Thread inTeamRoomThread;
        static Thread inQueueThread;
        static Thread chooseChampionThread;
        static Thread loadingGameThread;
        static Thread playGameThread;
        static Thread gameOverThread;

        static ClientHandle clientHandle;
        static GameHandle gameHandle;

        /// <summary>
        /// 跟随英雄的时钟
        /// </summary>
        static System.Timers.Timer follow;
        static System.Timers.Timer walkCheck;

        static int notWalkingTime = 0;

        static List<Automation.DD.DDKeys> walkKeys = new List<Automation.DD.DDKeys>();

        public static bool Start()
        {
            walkKeys.Add(Automation.DD.DDKeys.F2);
            walkKeys.Add(Automation.DD.DDKeys.F3);
            walkKeys.Add(Automation.DD.DDKeys.F4);
            walkKeys.Add(Automation.DD.DDKeys.F5);

            IntPtr clientIntPtr = User32.FindWindow("RCLIENT", "League of Legends");
            IntPtr gameIntPtr = User32.FindWindow("RiotWindowClass", "League of Legends (TM) Client");

            bool runing = false;
            if (gameIntPtr != IntPtr.Zero)
            {
                Window gameWindow = new Window(gameIntPtr);
                gameHandle = new GameHandle(gameWindow);

                StartLoadingGameThread();
                runing = true;
            }
            if (clientIntPtr != IntPtr.Zero)
            {
                Window clientWindow = new Window(clientIntPtr);
                clientHandle = new ClientHandle(clientWindow);

                if (!runing)
                {
                    clientHandle.SetWindowTopmost();
                    StartInTeamRoomThreadThread();
                    runing = true;
                }
            }
            return runing;
        }

        /// <summary>
        /// 停止线程
        /// </summary>
        /// <param name="excludeThread">依然运行的线程</param>
        public static void Stop(Thread excludeThread)
        {
            if(excludeThread != inTeamRoomThread || excludeThread != inQueueThread || excludeThread != chooseChampionThread)
            {//如果结束的线程不是处理客户端的线程
                if (clientHandle != null) clientHandle.CancelWindowTopmost();
            }
            else
            {
                if(clientHandle != null) clientHandle.SetWindowTopmost();
            }

            if (inTeamRoomThread != null && excludeThread != inTeamRoomThread)
                inTeamRoomThread.Abort();

            if (inQueueThread != null && excludeThread != inQueueThread)
                inQueueThread.Abort();

            if (chooseChampionThread != null && excludeThread != chooseChampionThread)
                chooseChampionThread.Abort();

            if (loadingGameThread != null && excludeThread != loadingGameThread)
                loadingGameThread.Abort();

            if (playGameThread != null && excludeThread != playGameThread)
                playGameThread.Abort();

            if (follow != null) follow.Close();
            if (walkCheck != null) walkCheck.Close();
        }

        /// <summary>
        /// 处理在队伍房间事件
        /// </summary>
        private static void StartInTeamRoomThreadThread()
        {
            inTeamRoomThread = new Thread(new ThreadStart(InTeamRoom));
            inTeamRoomThread.Start();
        }

        /// <summary>
        /// 处理排队事件
        /// </summary>
        private static void StartInQueueThread()
        {
            inQueueThread = new Thread(new ThreadStart(InQueue));
            inQueueThread.Start();
        }

        /// <summary>
        /// 处理选择英雄事件
        /// </summary>
        private static void StartChooseChampionThread()
        {
            chooseChampionThread = new Thread(new ThreadStart(ChooseChampion));
            chooseChampionThread.Start();
        }

        /// <summary>
        /// 处理加载游戏过程
        /// </summary>
        private static void StartLoadingGameThread()
        {
            loadingGameThread = new Thread(new ThreadStart(LoadingGame));
            loadingGameThread.Start();
        }

        /// <summary>
        /// 处理游戏中的事件
        /// </summary>
        private static void StartPlayGameThread()
        {
            playGameThread = new Thread(new ThreadStart(PlayGame));
            playGameThread.Start();
        }

        /// <summary>
        /// 处理游戏结束事件
        /// </summary>
        private static void StartGameOverThread()
        {
            gameOverThread = new Thread(new ThreadStart(GameOver));
            gameOverThread.Start();
        }
        /////////////////////////////////////////////////////////

        #region 事件处理

        /// <summary>
        /// 关闭各种提示和弹窗
        /// </summary>
        public static void CloseTipAndAlert()
        {
            if (clientHandle.Running())
            {
                //点击确认按钮
                clientHandle.ClickConfirmButton();
                //点击升级的OK按钮
                clientHandle.ClickOk();
            }
        }

        public static void GameOver()
        {
            Console.WriteLine("GameOver 开始");
            if (clientHandle.Running())
            {
                while (true)
                {
                    Thread.Sleep(2000);
                    if(clientHandle.SkipEvaluate())
                    {//点击跳过评价
                        continue;
                    }
                    else if(clientHandle.PlayAgain())
                    {//点击再来一次
                        StartInTeamRoomThreadThread();
                        break;
                    }
                    else if(clientHandle.InTeamRoom())
                    {//已经在队伍房间中
                        StartInTeamRoomThreadThread();
                        break;
                    }
                    else if(!clientHandle.GetEditRuneBotton().IsEmpty)
                    {//已经在选择英雄
                        StartChooseChampionThread();
                        break;
                    }
                    else if(!clientHandle.CanExecuteUserEvent())
                    {
                        clientHandle.SetWindowTopmost();
                    }
                    else
                    {
                        CloseTipAndAlert();
                    }
                }
            }
            Console.WriteLine("GameOver 结束");
        }

        /// <summary>
        /// 在队伍房间
        /// </summary>
        public static void InTeamRoom()
        {
            Console.WriteLine("inTeamRoom 开始");
            
            if (clientHandle.Running())
            {
                while (true)
                {
                    Thread.Sleep(1000);

                    if (clientHandle.IsInQueue())
                    { //在队列中
                        StartInQueueThread();
                        break;
                    }
                    else
                    { //不在队列中
                        if (clientHandle.InTeamRoom())
                        { //是在房间中
                          //开始游戏
                            clientHandle.QueueUp();

                            StartInQueueThread();
                            break;
                        }
                        else
                        {
                            if(!clientHandle.GetEditRuneBotton().IsEmpty)
                            {//在选择英雄界面
                                StartChooseChampionThread();
                                break;
                            }
                            else if(clientHandle.PlayAgain())
                            {//是否游戏结束了
                                Console.WriteLine("游戏已经结束，再来一次");
                            }
                            else
                            {
                                CloseTipAndAlert();
                            }
                        }
                    }
                }
            }
            else
            {
                //重新开始游戏
            }

            Console.WriteLine("inTeamRoom 结束");
        }

        /// <summary>
        /// 在排队中
        /// </summary>
        public static void InQueue()
        {
            Console.WriteLine("inQueue 开始");

            if (clientHandle.Running())
            {
                while (true)
                {
                    Thread.Sleep(2000);

                    if (clientHandle.IsInQueue())
                    { //在队列中

                        if (clientHandle.Accept())
                        { //点击接受游戏
                            Thread.Sleep(10000);

                            StartChooseChampionThread();
                            break;
                        }
                        Thread.Sleep(4000);
                        continue;
                    }
                    else
                    { //不在队列中
                        if (clientHandle.GetEditRuneBotton().IsEmpty)
                        {//不在选择英雄界面
                         //回到队伍房间事件处理
                            StartInTeamRoomThreadThread();
                            break;
                        }
                        else
                        {
                            StartChooseChampionThread();
                            break;
                        }
                    }
                }
            }
            else
            {
                //重开游戏
            }
            
            Console.WriteLine("inQueue 结束");
        }

        /// <summary>
        /// 选择英雄
        /// </summary>
        public static void ChooseChampion()
        {
            Console.WriteLine("chooseChampion 开始");

            if (clientHandle.Running())
            {
                bool isLockInChampion = false;
                while (true)
                {
                    Thread.Sleep(3000);
                    
                    if (clientHandle.GetEditRuneBotton().IsEmpty)
                    {//找不到编辑符文按钮
                        if (Process.GetProcessesByName("League of Legends").ToList().Count == 0)
                        {//不存在游戏进程，返回队列处理事件
                            StartInQueueThread();
                        }
                        break;
                    }
                    else
                    {//在选择英雄界面
                        if (clientHandle.DidChoosedChampion())
                        {//已经选择英雄
                            isLockInChampion = true;
                            break;
                        }
                        else if (clientHandle.RandomlyChooseChampion())
                        {//随机选择一个英雄
                            clientHandle.CloseTip();//关闭提示，防止干扰
                            Thread.Sleep(2000);
                            if (clientHandle.LockInChampion())
                            {//锁定英雄
                                continue;
                            }
                        }
                        else
                        {// 没有英雄可以选择
                         //清空搜索
                        }
                    }
                    Thread.Sleep(3000);
                }
                Console.WriteLine("锁定英雄");
                while (isLockInChampion || Process.GetProcessesByName("League of Legends").ToList().Count > 0)
                {//已经选择英雄，或游戏已经运行起来了
                    Thread.Sleep(3000);

                    if (clientHandle.IsNotMinimize())
                    {//客户端没有最小化
                        if (clientHandle.GetEditRuneBotton().IsEmpty)
                        {//找不到编辑符文按钮
                            Thread.Sleep(3000);
                            if (Process.GetProcessesByName("League of Legends").ToList().Count > 0)
                            {//游戏在运行了
                                StartLoadingGameThread();
                                break;
                            }
                            else
                            {//游戏还没有运行
                                StartInQueueThread();
                                break;
                            }
                        }
                        else
                        {//还在选择英雄界面
                            continue;
                        }
                    }
                    else
                    {//客户端最小化了
                        if (Process.GetProcessesByName("League of Legends").ToList().Count > 0)
                        {//游戏在运行了
                            StartLoadingGameThread();
                            break;
                        }
                        else
                        {//等待游戏运行
                            continue;
                        }
                    }
                }
            }
            else
            {
                //重开游戏
            }
            
            Console.WriteLine("chooseChampion 结束");
        }

        /// <summary>
        /// 处理加载游戏
        /// </summary>
        public static void LoadingGame()
        {
            Console.WriteLine("LoadingGame 开始");

            while (true)
            {
                if (Process.GetProcessesByName("League of Legends").ToList().Count > 0)
                {//游戏在运行
                    IntPtr gameIntPtr = User32.FindWindow("RiotWindowClass", "League of Legends (TM) Client");
                    if (gameIntPtr != IntPtr.Zero)
                    {
                        Window gameWindow = new Window(gameIntPtr);
                        gameHandle = new GameHandle(gameWindow);
                        //窗口出现，跳出循环
                        break;
                    }
                }
                else
                {//游戏没有运行
                    if(clientHandle != null && clientHandle.Running())
                    {//客户端在运行
                        StartInTeamRoomThreadThread();
                        break;
                    }
                    else
                    {//客户端没有运行
                        //重开游戏
                        break;
                    }
                }
            }
            
            if(gameHandle != null)
            {
                while(true)
                {
                    Thread.Sleep(new Random().Next(5000, 8000));
                    //if (gameHandle.InLoadingGame())
                    //{//载入游戏界面
                    //    Console.WriteLine("游戏载入中...");
                    //    Thread.Sleep(10000);
                    //}
                    if (gameHandle.Playing())
                    {//进入游戏
                        StartPlayGameThread();

                        Point point = gameHandle.GetNowWalkMarkPoint();
                        if (!point.IsEmpty) gameHandle.UpdateLastWalkMark(point);
                        break;
                    }
                }
            }

            Console.WriteLine("LoadingGame 结束");
        }

        public static void PlayGame()
        {
            Console.WriteLine("PlayGame 开始");
            notWalkingTime = 0;

            if (gameHandle.Running())
            {
                follow = new System.Timers.Timer(5000);
                follow.Elapsed += new ElapsedEventHandler(CancelFollowTeammate);
                follow.AutoReset = true;
                follow.Start();

                walkCheck = new System.Timers.Timer(8000);
                walkCheck.Elapsed += new ElapsedEventHandler(IsWalking);
                walkCheck.Start();

                gameHandle.MouseRandomMove();
                while (gameHandle.Running())
                {
                    if(gameHandle.CanExecuteUserEvent())
                    {
                        FollowTeammate();
                    }
                }
                //游戏停止运行
                follow.Stop();
                follow.Close();
                walkCheck.Stop();
                walkCheck.Close();
            }
            
            if(clientHandle.Running())
            {
                StartGameOverThread();
            }
            Console.WriteLine("PlayGame 结束");
        }

        /// <summary>
        /// 跟随队友
        /// </summary>
        public static void FollowTeammate()
        {
            gameHandle.FollowTeammateWithHotkey(Automation.DD.DDKeys.F1);
            gameHandle.FollowTeammateWithHotkey(walkKeys[0]);
        }

        /// <summary>
        /// 取消跟随
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void CancelFollowTeammate(object sender, ElapsedEventArgs e)
        {
            ((System.Timers.Timer)sender).Interval = new Random().Next(7000, 10000);
            ((System.Timers.Timer)sender).Start();
            
            gameHandle.CancelFollowTeammateWithHotkey(Automation.DD.DDKeys.F1);
            gameHandle.CancelFollowTeammateWithHotkey(walkKeys[0]);

            gameHandle.MouseRandomMove();
        }

        public static void IsWalking(object sender, ElapsedEventArgs e)
        {
            Point point = gameHandle.GetNowWalkMarkPoint();
            if(point.IsEmpty)
            {
                Console.WriteLine("找不到判断标记");
                notWalkingTime++;
                WindowScreenshot();
                return;
            }
            else
            {
                bool isWalking = gameHandle.IsWalking(point);
                gameHandle.UpdateLastWalkMark(point);

                if(!isWalking)
                {//没走动
                    notWalkingTime++;
                    Console.WriteLine("没走动，已经检测到 " + notWalkingTime + "次");
                    if (notWalkingTime >= 4)
                    {//超过次数
                        gameHandle.CancelFollowTeammateWithHotkey(walkKeys[0]);
                        Automation.DD.DDKeys k1 = walkKeys[0];
                        walkKeys.RemoveAt(0);
                        walkKeys.Add(k1);

                        Console.WriteLine("现在改键为 " + walkKeys[0]);

                        notWalkingTime = 0;
                    }
                }
                else
                {
                    Console.WriteLine("在走动");
                }
            }
        }

        #endregion

        public static void WindowScreenshot()
        {
            //IntPtr gameIntPtr = User32.FindWindow("RCLIENT", "League of Legends");
            IntPtr gameIntPtr = User32.FindWindow("RiotWindowClass", "League of Legends (TM) Client");
            Window gameWindow = new Window(gameIntPtr);
            Rectangle winRect = gameWindow.Rect;

            Bitmap windowScreenshot = new Bitmap(winRect.Width, winRect.Height);
            Graphics g = Graphics.FromImage(windowScreenshot);
            g.CopyFromScreen(winRect.Location, Point.Empty, winRect.Size);
            g.Dispose();
            
            windowScreenshot.Save(@"C:\LOL\" + DateTime.Now.ToString("MM月dd日HH时mm分ss秒") + ".png");
            windowScreenshot.Dispose();
        }
    }
}
