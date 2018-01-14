using System;
using System.Threading;
using Automation;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Keys = Interceptor.Keys;
using Interceptor;

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
            Bitmap gameSetting = Properties.Resources.GameSetting;
            ParserImageInWindow parser = new ParserImageInWindow(gameSetting, base.window, new Rectangle(805, 745, 23, 23));

            bool found = parser.FindInWindow() != 0;
            parser.Dispose();

            if (found) return true; else return false;
        }

        /// <summary>
        /// 响应暂离警告
        /// </summary>
        public void AnswerLeaveWarning()
        {
            Bitmap leaveWarning = Properties.Resources.LeaveWarning;
            ParserImageInWindow parser = new ParserImageInWindow(leaveWarning, base.window, new Rectangle(415, 410, 180, 60));

            bool found = parser.FindInWindow(Color.Empty, 15) != 0;
            parser.Dispose();

            if (found)
            {
                Target target = parser.GetATarget();
                Point point = target.randomPoint;

                this.Click(point);
            }
        }

        /// <summary>
        /// 获取当前走动标记图片的位置。
        /// </summary>
        /// <returns></returns>
        public Point GetNowWalkMarkPoint()
        {
            Bitmap gameSetting = Properties.Resources.GameSetting;
            ParserImageInWindow parser = new ParserImageInWindow(gameSetting, base.window, new Rectangle(805, 745, 23, 23));
            
            int count = parser.FindInWindow(true);
            parser.Dispose();
            if(count > 0)
            {
                Target target = parser.GetATarget();
                Point point = target.rect.Location;
                point.Y -= 5;
                return point;
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
            Bitmap gameSetting = Properties.Resources.GameSetting;
            
            Rectangle winRect = window.Rect;
            Rectangle markRect = new Rectangle(winRect.X + markPoint.X, winRect.Y + markPoint.Y, gameSetting.Size.Width, gameSetting.Size.Height);
            gameSetting.Dispose(); //只是为了获取大小

            Bitmap markScreenshot = new Bitmap(markRect.Width, markRect.Height);
            Graphics g = Graphics.FromImage(markScreenshot);
            g.CopyFromScreen(markRect.Location, Point.Empty, markRect.Size);
            g.Dispose();
            lastWalkMark = markScreenshot;
            //markScreenshot.Save(@"C:\Users\Injoy\Desktop\draw.png");
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
        public void FollowTeammateWithHotkey(Keys keyCode)
        {
            InputManager.ShareInstance().SendKey(keyCode, KeyState.Down);
            Thread.Sleep(new Random().Next(10, 40));
        }

        /// <summary>
        /// 取消跟随
        /// </summary>
        /// <param name="key"></param>
        public void CancelFollowTeammateWithHotkey(Keys keyCode)
        {
            InputManager.ShareInstance().SendKey(keyCode, KeyState.Up);
            Thread.Sleep(new Random().Next(5, 20));
        }

        /// <summary>
        /// 鼠标随机在游戏中央移动
        /// </summary>
        public void MouseRandomMove()
        {
            Rectangle rect = window.Rect;
            Point center = new Point(rect.Width / 2, rect.Height / 2);
            this.MoveMouse(new Point(new Random().Next(center.X - 100, center.X + 60), new Random().Next(center.Y - 100, center.Y + 60)));
        }
    }
}
