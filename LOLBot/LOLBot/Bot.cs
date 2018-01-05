using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Automation;
using Automation.WinApi;
using Automation.DD;

namespace LOLBot
{
    class Bot
    {
        static Thread inTeamRoomThread;
        static Thread inQueueThread;
        static Thread chooseChampionThread;

        static IntPtr clientIntPtr;

        static Window clientWindow;

        public static void Start()
        {
            clientIntPtr = User32.FindWindow("RCLIENT", "League of Legends");

            if(clientIntPtr != IntPtr.Zero)
            {
                clientWindow = new Window(clientIntPtr);
                clientWindow.SetWindowTopmost();

                StartInTeamRoomThreadThread();
            }
        }

        public static void Stop()
        {
            clientWindow.SetWindowNoTopmost();

            if (inTeamRoomThread != null)
                inTeamRoomThread.Abort();

            if (inQueueThread != null)
                inQueueThread.Abort();

            if (chooseChampionThread != null)
                chooseChampionThread.Abort();
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

        /////////////////////////////////////////////////////////

        #region 事件处理
        /// <summary>
        /// 在队伍房间
        /// </summary>
        public static void inTeamRoom()
        {
            Console.WriteLine("inTeamRoom 开始");
            ClientHandle clientHandle = new ClientHandle(clientWindow);

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
                        StartChooseChampionThread();
                        break;
                    }
                }
            }

            Console.WriteLine("inTeamRoom 结束");
        }

        /// <summary>
        /// 在排队中
        /// </summary>
        public static void inQueue()
        {
            Console.WriteLine("inQueue 开始");
            ClientHandle clientHandle = new ClientHandle(clientWindow);

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
                }
                else
                { //不在队列中
                    if(clientHandle.GetEditRuneBotton().IsEmpty)
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

            Console.WriteLine("inQueue 结束");
        }

        /// <summary>
        /// 选择英雄
        /// </summary>
        public static void chooseChampion()
        {
            Console.WriteLine("chooseChampion 开始");
            ClientHandle clientHandle = new ClientHandle(clientWindow);

            while (true)
            {
                Thread.Sleep(3000);
                

                if (clientHandle.GetEditRuneBotton().IsEmpty)
                {//找不到编辑符文按钮
                    StartInQueueThread();
                    break;
                }
                else
                {//在选择英雄界面
                    if(clientHandle.DidChoosedChampion())
                    {//已经选择英雄
                        break;
                    }

                    if(clientHandle.RandomlyChooseChampion())
                    {//随机选择一个英雄
                        Thread.Sleep(2000);
                        if(clientHandle.LockInChampion())
                        {
                            //一直判断这个界面
                            StartChooseChampionThread();
                            return;
                        }
                    }
                    else
                    {// 没有英雄可以选择
                        //清空搜索
                    }
                }

                Thread.Sleep(3000);
            }

            Console.WriteLine("chooseChampion 结束");
        }

    }
    #endregion
}
