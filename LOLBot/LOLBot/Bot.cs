using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

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

        static ClientHandle clientHandle;
        static GameHandle gameHandle;

        public static bool Start()
        {
            IntPtr clientIntPtr = User32.FindWindow("RCLIENT", "League of Legends");
            IntPtr gameIntPtr = User32.FindWindow("RiotWindowClass", "League of Legends (TM) Client");

            if (gameIntPtr != IntPtr.Zero)
            {
                Window gameWindow = new Window(gameIntPtr);
                gameHandle = new GameHandle(gameWindow);

                StartLoadingGameThread();
                return true;
            }
            if (clientIntPtr != IntPtr.Zero)
            {
                Window clientWindow = new Window(clientIntPtr);
                clientHandle = new ClientHandle(clientWindow);
                clientHandle.SetWindowTopmost();

                StartInTeamRoomThreadThread();
                return true;
            }
            return false;
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
        }

        /// <summary>
        /// 处理在队伍房间事件
        /// </summary>
        private static void StartInTeamRoomThreadThread()
        {
            inTeamRoomThread = new Thread(new ThreadStart(inTeamRoom));
            inTeamRoomThread.Start();
            //Stop(inTeamRoomThread);
        }

        /// <summary>
        /// 处理排队事件
        /// </summary>
        private static void StartInQueueThread()
        {
            inQueueThread = new Thread(new ThreadStart(inQueue));
            inQueueThread.Start();
            //Stop(inQueueThread);
        }

        /// <summary>
        /// 处理选择英雄事件
        /// </summary>
        private static void StartChooseChampionThread()
        {
            chooseChampionThread = new Thread(new ThreadStart(chooseChampion));
            chooseChampionThread.Start();
            //Stop(chooseChampionThread);
        }

        /// <summary>
        /// 处理加载游戏过程
        /// </summary>
        private static void StartLoadingGameThread()
        {
            loadingGameThread = new Thread(new ThreadStart(LoadingGame));
            loadingGameThread.Start();
            //Stop(loadingGameThread);
        }

        /// <summary>
        /// 处理游戏中的事件
        /// </summary>
        private static void StartPlayGameThread()
        {
            playGameThread = new Thread(new ThreadStart(PlayGame));
            playGameThread.Start();
        }

        /////////////////////////////////////////////////////////

        #region 事件处理
        /// <summary>
        /// 在队伍房间
        /// </summary>
        public static void inTeamRoom()
        {
            Console.WriteLine("inTeamRoom 开始");

            if (clientHandle.Running())
            {
                while (clientHandle.IsNotMinimize())
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
                            StartChooseChampionThread();
                            break;
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
        public static void inQueue()
        {
            Console.WriteLine("inQueue 开始");

            if (clientHandle.Running())
            {
                while (clientHandle.IsNotMinimize())
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

                    Thread.Sleep(4000);
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
        public static void chooseChampion()
        {
            Console.WriteLine("chooseChampion 开始");

            if (clientHandle.Running())
            {
                bool isLockInChampion = false;
                while (clientHandle.IsNotMinimize())
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
                while(gameHandle.IsNotMinimize())
                {
                    Thread.Sleep(5000);

                    if (gameHandle.InLoadingGame())
                    {//载入游戏界面
                        Console.WriteLine("游戏载入中...");
                        Thread.Sleep(10000);
                    }
                    else if(gameHandle.Playing())
                    {//进入游戏
                        StartPlayGameThread();
                        break;
                    }
                    else
                    { //重新连接
                    }
                }
            }

            Console.WriteLine("LoadingGame 结束");
        }

        public static void PlayGame()
        {
            Console.WriteLine("PlayGame 开始");
            if (gameHandle.Running())
            {
                System.Timers.Timer follow = new System.Timers.Timer(20);
                while(true)
                {
                    FollowTeammate();
                    Thread.Sleep(1000);
                }

            }
            Console.WriteLine("PlayGame 结束");
        }

        /// <summary>
        /// 跟随队友
        /// </summary>
        public static void FollowTeammate()
        {
            gameHandle.FollowTeammateWithHotkey(Automation.DD.DDKeys.F1);
            gameHandle.FollowTeammateWithHotkey(Automation.DD.DDKeys.F2);
        }

        public static void IsWalking()
        {

        }

        #endregion
    }
}
