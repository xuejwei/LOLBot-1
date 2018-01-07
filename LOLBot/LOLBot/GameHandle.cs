using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automation;
using System.Drawing;

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

            bool found = parser.FindInWindow(Color.White, 10);
            parser.Dispose();

            if (found) return true; else return false;
        }
    }
}
