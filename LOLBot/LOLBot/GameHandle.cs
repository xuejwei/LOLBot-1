using System;
using System.Threading;
using Automation;
using System.Drawing;
using Automation.WinApi;
using Automation.DD;
using System.Windows.Forms;

namespace LOLBot
{
    class GameHandle : BaseHandle
    {
        public GameHandle(Window window) : base(window)
        {
        }

        /// <summary>
        /// 是否正在加载游戏
        /// </summary>
        /// <returns></returns>
        public bool InLoadingGame()
        {
            Bitmap playerFrame = Properties.Resources.PlayerFrame;
            ParserImageInWindow parser = new ParserImageInWindow(playerFrame, base.window, new Rectangle(116, 420, 1055, 376));

            bool found = parser.FindInWindow(Color.White, 50);
            parser.Dispose();

            if (found) return true; else return false;
        }

        /// <summary>
        /// 是否正在游戏中
        /// </summary>
        /// <returns></returns>
        public bool Playing()
        {
            Bitmap walkMark = Properties.Resources.WalkMark;
            ParserImageInWindow parser = new ParserImageInWindow(walkMark, base.window, new Rectangle(430, 770, 64, 32));

            bool found = parser.FindInWindow(Color.FromArgb(255, 0, 255), 50);
            parser.Dispose();

            if (found) return true; else return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public void FollowTeammateWithHotkey(DDKeys key)
        {
            DDutil.getInstance().key((int)key, 1);
        }


    }
}
