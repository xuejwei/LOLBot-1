using System;
using System.Threading;
using System.Drawing;
using System.Windows;
using Automation;
using Interceptor;

using Window = Automation.Window;
using Point = System.Drawing.Point;

namespace LOLBot
{
    class ClientHandle: BaseHandle
    {
        public ClientHandle(Window window) : base(window)
        {
        }

        /// <summary>
        /// 是否在队列中
        /// </summary>
        public bool IsInQueue()
        {
            Bitmap inQueue = Properties.Resources.InQueue;
            Bitmap InQueue_Dark = Properties.Resources.InQueue_Dark;
            ParserImageInWindow parser = new ParserImageInWindow(inQueue, base.window, new Rectangle(1124, 40, 100, 50));
            ParserImageInWindow parserDark = new ParserImageInWindow(InQueue_Dark, base.window, new Rectangle(1124, 40, 100, 50));

            int count = parser.FindInWindow() + parserDark.FindInWindow();
            parser.Dispose();
            parserDark.Dispose();

            if (count > 0) return true; else return false;
        }

        /// <summary>
        /// 是否在组队房间中
        /// </summary>
        public bool InTeamRoom()
        {
            Bitmap teamRoomDisabled = Properties.Resources.TeamRoom_Disabled;
            ParserImageInWindow parser = new ParserImageInWindow(teamRoomDisabled, base.window,new Rectangle(0, 0, 200, 65));

            bool found = parser.FindInWindow() != 0;
            parser.Dispose();

            if (found) return true; else return false;
        }

        /// <summary>
        /// 寻找对局
        /// </summary>
        public bool QueueUp()
        {
            Bitmap queueUpNormal = Properties.Resources.QueueUp_Normal;
            Bitmap queueUpHover = Properties.Resources.QueueUp_Hover;

            ParserImageInWindow parserNormal = new ParserImageInWindow(queueUpNormal, base.window, new Rectangle(425, 660, 230, 50));
            ParserImageInWindow parserHover = new ParserImageInWindow(queueUpHover, base.window, new Rectangle(425, 660, 230, 50));
            bool found = parserNormal.FindInWindow() != 0 || parserHover.FindInWindow() != 0;
            parserNormal.Dispose();
            parserHover.Dispose();

            if (found)
            {
                Target normalTarget = parserNormal.GetATarget();
                Target hoverTarget = parserHover.GetATarget();
                Point clickPoint = Point.Empty;

                if (normalTarget != null) clickPoint = normalTarget.randomPoint;
                if (hoverTarget != null) clickPoint = hoverTarget.randomPoint;

                base.Click(clickPoint);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 接受游戏
        /// </summary>
        public bool Accept()
        {
            Bitmap acceptImage = Properties.Resources.Accept;

            ParserImageInWindow parser = new ParserImageInWindow(acceptImage, base.window, new Rectangle(545, 525, 220, 90));
            bool found = parser.FindInWindow() != 0;
            parser.Dispose();
            if (found)
            {
                Target target = parser.GetATarget();
                Point clickPoint = target.randomPoint;

                base.Click(clickPoint);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 搜索英雄
        /// </summary>
        /// <returns></returns>
        public bool SearchChampion(String championName)
        {
            Bitmap searchChampion = Properties.Resources.SearchChampion;

            ParserImageInWindow parser = new ParserImageInWindow(searchChampion, base.window, new Rectangle(728, 90, 60, 30));
            int count = parser.FindInWindow(Color.Empty, 20);
            parser.Dispose();

            if (count > 0)
            {
                Target target = parser.GetATarget();
                Point clickPoint = target.randomPoint;
                //连续点击3次全选
                base.Click(clickPoint);
                Thread.Sleep(new Random().Next(50, 100));
                base.Click(clickPoint);
                Thread.Sleep(new Random().Next(50, 100));
                base.Click(clickPoint);
                Thread.Sleep(new Random().Next(50, 100));

                //按退格键
                Thread.Sleep(new Random().Next(30, 100));
                InputManager.ShareInstance().SendKey(Keys.Delete, KeyState.Down);
                Thread.Sleep(new Random().Next(30, 100));
                InputManager.ShareInstance().SendKey(Keys.Delete, KeyState.Up);

                Thread th = new Thread(new ThreadStart(delegate ()
                {
                    Clipboard.Clear();
                    Clipboard.SetDataObject(championName, true);
                }));
                th.TrySetApartmentState(ApartmentState.STA);
                th.Start();

                Thread.Sleep(1000);
                InputManager.ShareInstance().SendKey(Keys.Control, KeyState.Down);
                Thread.Sleep(new Random().Next(40, 100));
                InputManager.ShareInstance().SendKey(Keys.V, KeyState.Down);
                Thread.Sleep(new Random().Next(40, 100));
                InputManager.ShareInstance().SendKey(Keys.Control, KeyState.Up);
                Thread.Sleep(new Random().Next(40, 100));
                InputManager.ShareInstance().SendKey(Keys.V, KeyState.Up);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取编辑符文按钮的坐标
        /// </summary>
        /// <returns></returns>
        public Point GetEditRuneBotton()
        {
            Bitmap editRune = Properties.Resources.EditRune;

            ParserImageInWindow parser = new ParserImageInWindow(editRune, base.window, new Rectangle(370, 666, 60, 50));
            bool found = parser.FindInWindow(Color.FromArgb(255, 0, 255), 20) != 0;
            parser.Dispose();

            if (found)
            {
                Target target = parser.GetATarget();
                Point clickPoint = target.randomPoint;

                return clickPoint;
            }
            else
            {
                return Point.Empty;
            }
        }

        /// <summary>
        /// 随机选择一个英雄
        /// </summary>
        /// <returns>选择成功</returns>
        public bool RandomlyChooseChampion()
        {
            Bitmap ChampionHeadshotFrame = Properties.Resources.ChampionHeadshotFrame;

            ParserImageInWindow parser = new ParserImageInWindow(ChampionHeadshotFrame, base.window, new Rectangle(340, 130, 610, 470));
            bool found = parser.FindInWindow(Color.White, 0) != 0;
            parser.Dispose();

            if (found)
            {
                Target[] targets = parser.GetTargets();

                Point clickPoint = targets[new Random().Next(0, targets.Length - 1)].randomPoint;

                base.Click(clickPoint);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 锁定英雄
        /// </summary>
        /// <returns></returns>
        public bool LockInChampion()
        {
            Bitmap lockInChampion = Properties.Resources.LockInChampion;

            ParserImageInWindow parser = new ParserImageInWindow(lockInChampion, base.window, new Rectangle(550, 575, 210, 80));
            bool found = parser.FindInWindow(Color.Empty, 30) != 0;
            parser.Dispose();

            if (found)
            {
                Target targets = parser.GetATarget();

                base.Click(targets.randomPoint);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 是否已经选择英雄
        /// </summary>
        /// <returns></returns>
        public bool DidChoosedChampion()
        {
            Bitmap waitingStartGame = Properties.Resources.WaitingStartGame;

            ParserImageInWindow parser = new ParserImageInWindow(waitingStartGame, base.window, new Rectangle(525, 10, 220, 50));

            bool found = parser.FindInWindow(Color.FromArgb(255, 0, 255), 50) != 0;
            parser.Dispose();

            if (found) return true; else return false;
        }

        /// <summary>
        /// 跳过评价
        /// </summary>
        public bool SkipEvaluate()
        {
            Bitmap SkipEvaluate = Properties.Resources.SkipEvaluate;

            ParserImageInWindow parser = new ParserImageInWindow(SkipEvaluate, base.window, new Rectangle(620, 632, 42, 38));
            bool found = parser.FindInWindow() != 0;
            parser.Dispose();

            if (found)
            {
                Target targets = parser.GetATarget();

                base.Click(targets.randomPoint);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 再来一次
        /// </summary>
        /// <returns></returns>
        public bool PlayAgain()
        {
            Bitmap PlayAgain = Properties.Resources.PlayAgain;

            ParserImageInWindow parser = new ParserImageInWindow(PlayAgain, base.window, new Rectangle(470, 666, 160, 40));
            bool found = parser.FindInWindow() != 0;
            parser.Dispose();

            if (found)
            {
                Target targets = parser.GetATarget();

                base.Click(targets.randomPoint);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 点击确认按钮
        /// </summary>
        public void ClickConfirmButton()
        {
            Bitmap Confirm_Normal = Properties.Resources.Confirm_Normal;
            Bitmap Confirm_Hover = Properties.Resources.Confirm_Hover;

            ParserImageInWindow parserNormal = new ParserImageInWindow(Confirm_Normal, base.window, new Rectangle(300, 222, 600, 490));
            ParserImageInWindow parserHover = new ParserImageInWindow(Confirm_Normal, base.window, new Rectangle(300, 222, 600, 490));
            bool found = parserNormal.FindInWindow() != 0 || parserHover.FindInWindow() != 0;
            parserNormal.Dispose();
            parserHover.Dispose();

            if (found)
            {
                Target normalTarget = parserNormal.GetATarget();
                Target hoverTarget = parserHover.GetATarget();
                Point clickPoint = Point.Empty;

                if (normalTarget != null) clickPoint = normalTarget.randomPoint;
                if (hoverTarget != null) clickPoint = hoverTarget.randomPoint;

                base.Click(clickPoint);
            }
        }

        /// <summary>
        /// 领取升级奖励
        /// </summary>
        public void ClickOk()
        {
            Bitmap ok = Properties.Resources.OK_Normal;
            ParserImageInWindow okParser = new ParserImageInWindow(ok, base.window, new Rectangle(300, 222, 600, 490));

            int count = okParser.FindInWindow(Color.White, 30);
            okParser.Dispose();

            if (count > 0)
            {
                Target targets = okParser.GetATarget();
                base.Click(targets.randomPoint);
            }
        }

        /// <summary>
        /// 领取“保持体育精神奖励”
        /// </summary>
        public void ClickCongratulations()
        {
            Bitmap Congratulations_Normal = Properties.Resources.Congratulations_Normal;
            Bitmap Congratulations_Hover = Properties.Resources.Congratulations_Hover;

            ParserImageInWindow parserNormal = new ParserImageInWindow(Congratulations_Normal, base.window, new Rectangle(570, 500, 150, 60));
            ParserImageInWindow parserHover = new ParserImageInWindow(Congratulations_Hover, base.window, new Rectangle(570, 500, 150, 60));
            bool found = parserNormal.FindInWindow(Color.White, 30) != 0 || parserHover.FindInWindow(Color.White, 30) != 0;
            parserNormal.Dispose();
            parserHover.Dispose();

            if (found)
            {
                Target normalTarget = parserNormal.GetATarget();
                Target hoverTarget = parserHover.GetATarget();
                Point clickPoint = Point.Empty;

                if (normalTarget != null) clickPoint = normalTarget.randomPoint;
                if (hoverTarget != null) clickPoint = hoverTarget.randomPoint;

                base.Click(clickPoint);
            }
        }

        /// <summary>
        /// 关闭提示
        /// </summary>
        public void CloseTip()
        {
            Bitmap tipClose = Properties.Resources.Tip_Close;
            ParserImageInWindow tipCloseParser = new ParserImageInWindow(tipClose, base.window, new Rectangle(0, 0, window.Rect.Width, window.Rect.Height));

            int tipCount = tipCloseParser.FindInWindow(Color.White, 30);
            tipCloseParser.Dispose();

            if(tipCount > 0)
            {
                Target targets = tipCloseParser.GetATarget();
                base.Click(targets.randomPoint);
            }
        }
    }
}
