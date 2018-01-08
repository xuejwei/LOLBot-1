using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Automation
{
    class ParserImageInWindow : IDisposable
    {
        private readonly Bitmap image;
        public readonly Window window;
        private Rectangle searchZone;

        public Target[] parserResult;

        public ParserImageInWindow(Bitmap image, Window window)
            : this(image, window, window.Rect)
        {
        }

        public ParserImageInWindow(Bitmap image, Window window, Rectangle searchZone)
        {
            this.image = image;
            this.window = window;
            this.searchZone = searchZone;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.image.Dispose();
            }
        }

        /// <summary>
        ///  是否能在窗口中找到这个图片
        /// </summary>
        public bool FindInWindow()
        {
            Bitmap windowScreenshot = this.window.Capture();

            BitmapData parentImageData = windowScreenshot.LockBits(new Rectangle(0, 0, windowScreenshot.Width, windowScreenshot.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData subImageData = this.image.LockBits(new Rectangle(0, 0, this.image.Width, this.image.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            searchZone = ProcessSearchZone(parentImageData.Width, parentImageData.Height);

            Stopwatch sw = new Stopwatch();
            sw.Start(); //计时开始
            Rectangle[] result = new FindImage().Match(parentImageData, subImageData, new Rectangle(searchZone.X, searchZone.Y, searchZone.Width, searchZone.Height));
            sw.Stop();

            windowScreenshot.UnlockBits(parentImageData);
            this.image.UnlockBits(subImageData);
            
            windowScreenshot.Dispose();

            if (result.Length != 0)
            {
                parserResult = new Target[result.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    parserResult[i] = new Target(result[i], 1);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 查找图片过程中，排除一些颜色
        /// </summary>
        /// <param name="similarity">容错值</param>
        /// <param name="excludeColor">排除的颜色</param>
        /// <returns></returns>
        public bool FindInWindow(Color excludeColor, int similarity = 0)
        {
            Bitmap windowScreenshot = this.window.Capture();
            
            BitmapData parentImageData = windowScreenshot.LockBits(new Rectangle(0, 0, windowScreenshot.Width, windowScreenshot.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData subImageData = this.image.LockBits(new Rectangle(0, 0, this.image.Width, this.image.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            searchZone = ProcessSearchZone(parentImageData.Width, parentImageData.Height);

            Rectangle[] result;

            Stopwatch sw = new Stopwatch();
            sw.Start(); //计时开始
            if (excludeColor.IsEmpty)
            {
                result = new FindImage().Match(parentImageData, subImageData, searchZone, similarity);
            }
            else
            {
                result = new FindImage().Match(parentImageData, subImageData, searchZone, excludeColor, similarity);
            }
            sw.Stop();
            
            windowScreenshot.UnlockBits(parentImageData);
            this.image.UnlockBits(subImageData);

            //Graphics g = Graphics.FromImage(windowScreenshot);
            //g.DrawRectangle(new Pen(Color.Red, 1), searchZone.X, searchZone.Y, searchZone.Width, searchZone.Height);
            //foreach (Rectangle rect in result)
            //{
            //    g.DrawRectangle(new Pen(Color.Red, 1), rect.X, rect.Y, rect.Width, rect.Height);
            //}
            //windowScreenshot.Save(@"C:\Users\Injoy\Desktop\draw.png");

            windowScreenshot.Dispose();

            if (result.Length != 0)
            {
                parserResult = new Target[result.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    parserResult[i] = new Target(result[i], similarity);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取一个结果
        /// </summary>
        /// <returns></returns>
        public Target GetATarget()
        {
            if(this.parserResult != null && this.parserResult.Length > 0)
            {
                return this.parserResult[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取所有结果
        /// </summary>
        /// <returns></returns>
        public Target[] GetTargets()
        {
            if (this.parserResult != null)
            {
                return this.parserResult;
            }
            else
            {
                return null;
            }
        }

        private Rectangle ProcessSearchZone(int maxWidth, int maxHeight)
        {
            Rectangle rect = new Rectangle(searchZone.X, searchZone.Y, searchZone.Width, searchZone.Height);
            if (maxWidth < rect.X + rect.Width)
            {
                int newWidth, newX;

                newX = maxWidth < rect.X ? maxWidth : rect.X;
                newWidth = maxWidth - newX;
                rect = new Rectangle(newX, rect.Y, newWidth, rect.Height);
            }

            if (maxHeight < rect.Y + rect.Height)
            {
                int newHeight, newY;

                newY = maxHeight < rect.Y ? maxHeight : rect.Y;
                newHeight = maxHeight - newY;
                rect = new Rectangle(rect.X, newY, rect.Width, newHeight);
            }

            return rect;
        }


        //public bool findInWindow(float similarity)
        //{
        //    var matcher = new ExhaustiveTemplateMatching(similarity);
        //    DateTime date = DateTime.Now;
        //    Bitmap windowScreenshot = window.Capture();
        //    Bitmap screenshot24bpp = new Bitmap(windowScreenshot.Width, windowScreenshot.Height, pixelFormat);
        //    using (Graphics gr = Graphics.FromImage(screenshot24bpp))
        //    {
        //        gr.DrawImage(windowScreenshot, new Rectangle(0, 0, screenshot24bpp.Width, screenshot24bpp.Height));
        //    }
        //    const Int32 divisor = 4;
        //    this.parserResult = matcher.ProcessImage(
        //        new ResizeNearestNeighbor(screenshot24bpp.Width / divisor, screenshot24bpp.Height / divisor).Apply(screenshot24bpp),
        //        new ResizeNearestNeighbor(this.image.Width / divisor, this.image.Height / divisor).Apply(this.image));

        //    if (parserResult.Length == 0)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}

    }
}
