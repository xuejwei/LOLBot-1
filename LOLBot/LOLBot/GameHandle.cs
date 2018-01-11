using System;
using System.Threading;
using Automation;
using System.Drawing;
using Automation.WinApi;
using Automation.DD;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace LOLBot
{
    class GameHandle : BaseHandle
    {
        private Bitmap lastWalkMark;

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

            bool found = parser.FindInWindow(Color.White, 50) != 0;
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

            bool found = parser.FindInWindow(Color.FromArgb(255, 0, 255), 25) != 0;
            parser.Dispose();

            if (found) return true; else return false;
        }

        /// <summary>
        /// 获取当前走动标记图片的位置。
        /// </summary>
        /// <returns></returns>
        public Point GetNowWalkMarkPoint()
        {
            Bitmap walkMark = Properties.Resources.WalkMark;
            ParserImageInWindow parser = new ParserImageInWindow(walkMark, base.window, new Rectangle(430, 770, 64, 32));

            int count = parser.FindInWindow(Color.FromArgb(255, 0, 255), 25, true);
            parser.Dispose();
            if(count > 0)
            {
                Target target = parser.GetATarget();
                
                return target.rect.Location ;
            }
            else
            {
                return Point.Empty;
            }
        }

        /// <summary>
        /// 更新标记
        /// </summary>
        /// <param name="markPoint">标记坐标</param>
        public void UpdateLastWalkMark(Point markPoint)
        {
            Bitmap walkMark = Properties.Resources.WalkMark;
            
            Rectangle winRect = window.Rect;
            Rectangle markRect = new Rectangle(winRect.X + markPoint.X, winRect.Y + markPoint.Y, walkMark.Size.Width, walkMark.Size.Height);
            walkMark.Dispose(); //只是为了获取大小

            Bitmap markScreenshot = new Bitmap(markRect.Width, markRect.Height);
            Graphics g = Graphics.FromImage(markScreenshot);
            g.CopyFromScreen(markRect.Location, Point.Empty, markRect.Size);
            g.Dispose();
            lastWalkMark = markScreenshot;
        }

        public bool IsWalking(Point markPoint)
        {
            if (lastWalkMark == null) return false;

            Rectangle winRect = window.Rect;
            Rectangle markRect = new Rectangle(winRect.X + markPoint.X, winRect.Y + markPoint.Y, lastWalkMark.Size.Width, lastWalkMark.Size.Height);

            Bitmap nowWalkMark = new Bitmap(markRect.Width, markRect.Height);
            Graphics g = Graphics.FromImage(nowWalkMark);
            g.CopyFromScreen(markRect.Location, Point.Empty, markRect.Size);
            g.Dispose();

            BitmapData lastImageData = lastWalkMark.LockBits(new Rectangle(0, 0, lastWalkMark.Width, lastWalkMark.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData nowImageData = nowWalkMark.LockBits(new Rectangle(0, 0, nowWalkMark.Width, nowWalkMark.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            FindImage findImage = new FindImage();
            int count = findImage.Match(lastImageData, nowImageData, new Rectangle(0, 0, lastWalkMark.Width, lastWalkMark.Height)).Length;

            nowWalkMark.UnlockBits(nowImageData);
            nowWalkMark.Dispose();

            if (count == 0)
            {
                return true;
            }
            else
            {//找到，说明没在走动
                return false;
            }
        }

        /// <summary>
        /// 跟随
        /// </summary>
        /// <param name="key"></param>
        public void FollowTeammateWithHotkey(DDKeys key)
        {
            DDutil.getInstance().key((int)key, 1);
            Thread.Sleep(new Random().Next(10, 40));
        }

        /// <summary>
        /// 取消跟随
        /// </summary>
        /// <param name="key"></param>
        public void CancelFollowTeammateWithHotkey(DDKeys key)
        {
            DDutil.getInstance().key((int)key, 2);
            Thread.Sleep(new Random().Next(5, 20));
        }

        /// <summary>
        /// 鼠标随机在游戏中央移动
        /// </summary>
        public void MouseMove()
        {
            Rectangle rect = window.Rect;
            Point center = new Point(rect.Width / 2, rect.Height / 2);
            this.Click(new Point(new Random().Next(center.X - 100, center.X + 60), new Random().Next(center.Y - 100, center.Y + 60)));
        }
    }
}
