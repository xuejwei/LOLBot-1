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

        public void CanHandle()
        {
            User32.ShowWindow(window.windowHandle, User32.SW_RESTORE);
        }

        public void Click(Point point)
        {
            User32.SetForegroundWindow(window.windowHandle);

            mouseController.Move(new Point(point.X + this.window.Rect.X, point.Y + this.window.Rect.Y));
            DDutil.getInstance().btn((int)MouseBtn.左下);
            new Random().Next(50, 150);
            DDutil.getInstance().btn((int)MouseBtn.左上);
        }
    }
}
