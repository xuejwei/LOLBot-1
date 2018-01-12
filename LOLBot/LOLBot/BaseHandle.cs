using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using Automation.WinApi;
using HenoohDeviceEmulator;
using Automation;
using Automation.DD;

namespace LOLBot
{
    class BaseHandle
    {
        protected Window window;

        private MouseController mouseController;

        public BaseHandle(Window window)
        {
            this.window = window;
            mouseController = new MouseController();
        }

        public void MoveMouse(Point point)
        {
            if (Running())
            {
                mouseController.Move(new Point(point.X + this.window.Rect.X, point.Y + this.window.Rect.Y));
            }
        }
        
        public void Click(Point point)
        {
            if (Running())
            {
                User32.SetForegroundWindow(window.windowHandle);

                mouseController.Move(new Point(point.X + this.window.Rect.X, point.Y + this.window.Rect.Y));
                DDutil.getInstance().btn((int)MouseBtn.左下);
                new Random().Next(50, 150);
                DDutil.getInstance().btn((int)MouseBtn.左上);
            }
        }

        public bool Running()
        {
            if (window.GetWindowThreadProcessId() != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 是否可以执行用户事件
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteUserEvent()
        {
            if(Running() && window.IsForeground())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 是非最小化状态
        /// </summary>
        /// <returns></returns>
        public bool IsNotMinimize()
        {
            if(window.IsIconic())
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 设置置顶
        /// </summary>
        public void SetWindowTopmost()
        {
            if(Running())
            {
                window.SetWindowTopmost();
            }
        }

        /// <summary>
        /// 取消置顶
        /// </summary>
        public void CancelWindowTopmost()
        {
            if (Running())
            {
                window.SetWindowNoTopmost();
            }
        }
    }
}
