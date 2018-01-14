namespace Automation
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using WinApi;
    using WinApi.Structs;
    using Point = WinApi.Structs.Point;

    /// <summary>
    /// The Window class represents a window selected for capture.
    /// </summary>
    public class Window
    {
        public readonly IntPtr windowHandle;
        private Rect rect;

        public Window(IntPtr handle)
        {
            this.windowHandle = handle;
            this.rect = new Rect();
            User32.GetWindowRect(this.windowHandle, ref this.rect);
        }

        public Window(int x, int y) :
            this(User32.WindowFromPoint(new Point(x, y)))
        {
        }

        public Window(System.Drawing.Point pt)
            : this(pt.X, pt.Y)
        {
        }

        public Rect Rect
        {
            get
            {
                Rect rect = new Rect();
                User32.GetWindowRect(this.windowHandle, ref rect);
                return rect;
            }
        }

        public Bitmap Capture()
        {
            if (GetWindowThreadProcessId() == 0) return new Bitmap(1, 1, PixelFormat.Format24bppRgb);

            var deviceSrc = User32.GetWindowDC(this.windowHandle);
            User32.GetWindowRect(this.windowHandle, ref this.rect);
            var deviceDest = Gdi32.CreateCompatibleDC(deviceSrc);
            var bitmapHandle = Gdi32.CreateCompatibleBitmap(
                deviceSrc,
                this.rect.Width,
                this.rect.Height);
            var oldHandle = Gdi32.SelectObject(deviceDest, bitmapHandle);
            Gdi32.BitBlt(deviceDest, 0, 0,  this.rect.Width, this.rect.Height, deviceSrc, 0, 0, 0x00CC0020);
            Gdi32.SelectObject(deviceDest, oldHandle);
            Gdi32.DeleteDC(deviceDest);
            User32.ReleaseDC(this.windowHandle, deviceSrc);

            var img = Image.FromHbitmap(bitmapHandle);
            Gdi32.DeleteObject(bitmapHandle);
          
            return img;
        }

        public void Move(Rectangle rect)
        {
            User32.MoveWindow(windowHandle, rect.X, rect.Y, rect.Width, rect.Height, false);
        }

        /// <summary>
        /// 获取窗口的进程 ID
        /// </summary>
        /// <returns></returns>
        public uint GetWindowThreadProcessId()
        {
            uint pid;
            User32.GetWindowThreadProcessId(windowHandle, out pid);
            return pid;
        }

        public IntPtr GetParentHandle()
        {
            return User32.GetParent(this.windowHandle);
        }

        /// <summary>
        /// 是否是焦点窗口
        /// </summary>
        /// <returns></returns>
        public bool IsForeground()
        {
            if(windowHandle == User32.GetForegroundWindow())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 是否是最小化状态
        /// </summary>
        public bool IsIconic()
        {
            return User32.IsIconic(windowHandle);
        }

        /// <summary>
        /// 设置置顶
        /// </summary>
        public void SetWindowTopmost()
        {
            if (IsIconic())
            {
                User32.ShowWindow(windowHandle, User32.SW_RESTORE);
            }
            User32.SetWindowPos(windowHandle, User32.HWND_TOPMOST, 1, 1, 1, 1, User32.SWP_NOMOVE | User32.SWP_NOSIZE);
        }

        /// <summary>
        /// 取消置顶
        /// </summary>
        public void SetWindowNoTopmost()
        {
            User32.SetWindowPos(windowHandle, User32.HWND_NOTOPMOST, 1, 1, 1, 1, User32.SWP_NOMOVE | User32.SWP_NOSIZE);
        }

        /// <summary>
        /// 设置焦点
        /// </summary>
        public void SetForeground()
        {
            User32.SetForegroundWindow(windowHandle);
        }
    }
}