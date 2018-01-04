namespace Automation.WinApi
{
    using System;
    using System.Runtime.InteropServices;
    using Structs;

    /// <summary>
    /// Contains PInvoke signatures for user32.dll functions.
    /// </summary>
    internal class User32
    {
        public delegate IntPtr HookProc(int code, UIntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(
            int hookId,
            int code,
            UIntPtr wParam,
            IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr windowHandle);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursor(IntPtr instance, int cursor);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint flags, int x, int y, int data, int extraInfo);

        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr dc);

        [DllImport("user32.dll")]
        public static extern IntPtr SetCursor(IntPtr cursorHandle);

        [DllImport("user32.dll")]
        public static extern int SetWindowsHookEx(
            int hookId,
            HookProc function,
            IntPtr instance,
            int threadId);

        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(int hookId);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(Point point);

        [DllImport("user32.dll")]
        public static extern bool PrintWindow( IntPtr hwnd, IntPtr hdcBlt, UInt32 nFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern int MapVirtualKey(uint Ucode, uint uMapType);

        [DllImport("user32.dll")]
        public static extern bool IsZoomed(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint control, int vk);
   
        [DllImport("user32")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}