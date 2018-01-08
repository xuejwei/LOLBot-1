using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;

namespace Automation.DD
{
    public enum DDKeys
    {
        F1 = 101,
        F2 = 102,
        F3 = 103,
        F4 = 104,
        F5 = 105
    }

    class DDutil
    {
        private static CDD dd;

        public static CDD getInstance()
        {
            // 如果类的实例不存在则创建，否则直接返回
            if (dd == null)
            {
                dd = new CDD();
            }
            return dd;
        }

        /// <summary>
        /// 初始化 dd，如果出错，返回出错原因
        /// </summary>
        /// <returns>出错原因</returns>
        public static string init()
        {
            string dllfile = Environment.CurrentDirectory;
            if (IntPtr.Size == 4)
            {
                // 32-bit 应用
                if(Environment.Is64BitOperatingSystem)
                {
                    // 64位系统
                    dllfile += @"\dd74000x64.32.dll";
                }
                else
                {
                    //32位系统
                    dllfile += @"\dd74000x32.dll";

                }
            }
            else if (IntPtr.Size == 8)
            {
                // 64-bit 应用
                dllfile += @"\dd74000x64.64.dll";
            }

            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            if(!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                return "请使用管理员权限运行";
            }


            System.IO.FileInfo fi = new System.IO.FileInfo(dllfile);
            if (!fi.Exists)
            {
                return "没有正确加载依赖文件，请检查文件是否存在\n" + dllfile;
            }

            int ret = getInstance().Load(dllfile);
            if (ret == -2) { return "装载库时发生错误"; }
            if (ret == -1) { return "取函数地址时发生错误"; }
            if (ret == 0) { return "非增强模块"; }

            return null;
        }
    }
}
