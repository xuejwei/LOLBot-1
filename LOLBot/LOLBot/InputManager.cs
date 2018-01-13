using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interceptor;

namespace LOLBot
{
    class InputManager
    {
        private static Input input;

        public static Input ShareInstance()
        {
            // 如果类的实例不存在则创建，否则直接返回
            if (input == null)
            {
                input = new Input();
                input.KeyboardFilterMode = KeyboardFilterMode.All;
            }
            return input;
        }

        public void load()
        {
            if(!input.IsLoaded)
            {
                input.Load();
            }
        }

        public void unload()
        {
            input.Unload();
        }
    }
}
