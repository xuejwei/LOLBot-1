using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;

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
        
        static ClientHandle clientHandle;
        static GameHandle gameHandle;

        public static bool Start()
        {
            IntPtr clientIntPtr = User32.FindWindow("RCLIENT", "League of Legends");

            if(clientIntPtr != IntPtr.Zero)
            {
                Window clientWindow = new Window(clientIntPtr);
                clientHandle = new ClientHandle(clientWindow);
                clientHandle.SetWindowTopmost();

                StartInTeamRoomThreadThread();
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void Stop()
        {
            if(clientHandle != null)
            {
                clientHandle.CancelWindowTopmost();
            }

            if (inTeamRoomThread != null)
                inTeamRoomThread.Abort();

            if (inQueueThread != null)
                inQueueThread.Abort();

            if (chooseChampionThread != null)
                chooseChampionThread.Abort();

            if (loadingGameThread != null)
            {
                loadingGameThread.Abort();
            }
        }

        /// <summary>
        /// 处理在队伍房间事件
        /// </summary>
        private static void StartInTeamRoomThreadThread()
        {
            inTeamRoomThread = new Thread(new ThreadStart(inTeamRoom));
            inTeamRoomThread.Start();
        }

        /// <summary>
        /// 处理排队事件
        /// </summary>
        private static void StartInQueueThread()
        {
            inQueueThread = new Thread(new ThreadStart(inQueue));
            inQueueThread.Start();
        }

        /// <summary>
        /// 处理选择英雄事件
        /// </summary>
        private static void StartChooseChampionThread()
        {
            chooseChampionThread = new Thread(new ThreadStart(chooseChampion));
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
                while (clientHandle.IsNotMinimize())
                {
                    Thread.Sleep(3000);

                    if (clientHandle.GetEditRuneBotton().IsEmpty)
                    {//找不到编辑符文按钮
                        if (Process.GetProcessesByName("League of Legends").ToList().Count > 0)
                        {
                            //存在
                            IntPtr gameIntPtr = User32.FindWindow("RiotWindowClass", "League of Legends (TM) Client");
                            Window gameWindow = new Window(gameIntPtr);
                            gameHandle = new GameHandle(gameWindow);
                            StartLoadingGameThread();
                        }
                        else
                        {
                            //不存在游戏进程，返回队列处理事件
                            StartInQueueThread();
                        }
                        break;
                    }
                    else
                    {//在选择英雄界面
                        if (clientHandle.DidChoosedChampion())
                        {//已经选择英雄
                            continue;
                        }

                        if (clientHandle.RandomlyChooseChampion())
                        {//随机选择一个英雄
                            Thread.Sleep(2000);
                            if (clientHandle.LockInChampion())
                            {//处理下一步
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


            Console.WriteLine("LoadingGame 结束");
        }
    }
    #endregion
}
