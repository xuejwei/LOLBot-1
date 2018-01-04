using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;

using Automation;

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
            Bitmap inQueue = Resource.InQueue;
            ParserImageInWindow parser = new ParserImageInWindow(inQueue, base.window, new Rectangle(1124, 40, 100, 50));
            if (parser.FindInWindow())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 是否在组队房间中
        /// </summary>
        public bool InTeamRoom()
        {
            Bitmap teamRoomDisabled = Resource.TeamRoom_Disabled;
            ParserImageInWindow parser = new ParserImageInWindow(teamRoomDisabled, base.window,new Rectangle(0, 0, 200, 65));
            if (parser.FindInWindow())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 寻找对局
        /// </summary>
        public bool QueueUp()
        {
            Bitmap queueUpNormal = Resource.QueueUp_Normal;
            Bitmap queueUpHover = Resource.QueueUp_Hover;

            ParserImageInWindow parserNormal = new ParserImageInWindow(queueUpNormal, base.window, new Rectangle(425, 660, 230, 50));
            ParserImageInWindow parserHover = new ParserImageInWindow(queueUpHover, base.window, new Rectangle(425, 660, 230, 50));
            if (parserNormal.FindInWindow() || parserHover.FindInWindow())
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
            Bitmap acceptImage = Resource.Accept;

            ParserImageInWindow parser = new ParserImageInWindow(acceptImage, base.window, new Rectangle(545, 525, 220, 90));
            if(parser.FindInWindow())
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
        public bool SearchChampion()
        {
            Bitmap searchChampion = Resource.SearchChampion;

            ParserImageInWindow parser = new ParserImageInWindow(searchChampion, base.window, new Rectangle(728, 90, 60, 60));
            if (parser.FindInWindow())
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
        /// 获取编辑符文按钮的坐标
        /// </summary>
        /// <returns></returns>
        public Point GetEditRuneBotton()
        {
            Bitmap editRune = Resource.EditRune;

            ParserImageInWindow parser = new ParserImageInWindow(editRune, base.window, new Rectangle(370, 666, 60, 50));
            if (parser.FindInWindow(Color.FromArgb(255, 0, 255), 20))
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
            Bitmap ChampionHeadshotFrame = Resource.ChampionHeadshotFrame;

            ParserImageInWindow parser = new ParserImageInWindow(ChampionHeadshotFrame, base.window, new Rectangle(340, 130, 610, 470));
            if (parser.FindInWindow(Color.White, 0))
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
            Bitmap lockInChampion = Resource.LockInChampion;

            ParserImageInWindow parser = new ParserImageInWindow(lockInChampion, base.window, new Rectangle(550, 575, 210, 80));
            if (parser.FindInWindow())
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
            Bitmap waitingStartGame = Resource.WaitingStartGame;

            ParserImageInWindow parser = new ParserImageInWindow(waitingStartGame, base.window, new Rectangle(525, 10, 220, 50));
            if (parser.FindInWindow(Color.FromArgb(255, 0, 255), 50))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
