using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Automation
{
    class FindImage
    {
        /// <summary>
        /// 全匹配找图
        /// </summary>
        /// <param name="parentImage">大图数据</param>
        /// <param name="subImage">小图数据</param>
        /// <returns></returns>
        public unsafe Rectangle[] Match(BitmapData parentImage, BitmapData subImage, Rectangle searchRect)
        {
            List<Rectangle> list = new List<Rectangle>();
            int parentStride = parentImage.Stride;
            int subStride = subImage.Stride;
            IntPtr parentIptr = parentImage.Scan0;
            IntPtr subIptr = subImage.Scan0;
            byte* parentPtr;
            byte* subPtr;
            bool isOk = false;
            int breakW = searchRect.X + searchRect.Width - subImage.Width + 1;
            int breakH = searchRect.Y + searchRect.Height - subImage.Height + 1;
            for (int h = searchRect.Y; h < breakH; h++)
            {
                for (int w = searchRect.X; w < breakW; w++)
                {
                    subPtr = (byte*)(subIptr);
                    for (int y = 0; y < subImage.Height; y++)
                    {
                        for (int x = 0; x < subImage.Width; x++)
                        {
                            parentPtr = (byte*)((IntPtr)parentIptr + parentStride * (h + y) + (w + x) * 3);
                            subPtr = (byte*)((IntPtr)subIptr + subStride * y + x * 3);
                            try
                            {
                                if (parentPtr[0] == subPtr[0] && parentPtr[1] == subPtr[1] && parentPtr[2] == subPtr[2])
                                {
                                    isOk = true;
                                }
                                else
                                {
                                    isOk = false;
                                    break;
                                }
                            }
                            catch(Exception)
                            {
                                return list.ToArray();
                            }
                        }
                        if (isOk == false) { break; }
                    }
                    if (isOk)
                    {
                        list.Add(new Rectangle(w, h, subImage.Width, subImage.Height));
                    }
                    isOk = false;
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// 相似找图
        /// </summary>
        /// <param name="parentImage">大图</param>
        /// <param name="subImage">小图</param>
        /// <param name="searchRect">搜索区域</param>
        /// <param name="similar">相识度</param>
        /// <returns></returns>
        public unsafe Rectangle[] Match(BitmapData parentImage, BitmapData subImage, Rectangle searchRect, int similar)
        {
            List<Rectangle> list = new List<Rectangle>();
            int parentStride = parentImage.Stride;
            int subStride = subImage.Stride;
            IntPtr parentIptr = parentImage.Scan0;
            IntPtr subIptr = subImage.Scan0;
            byte* parentPtr;
            byte* subPtr;
            bool isOk = false;
            int breakW = searchRect.X + searchRect.Width - subImage.Width + 1;
            int breakH = searchRect.Y + searchRect.Height - subImage.Height + 1;
            for (int h = searchRect.Y; h < breakH; h++)
            {
                for (int w = searchRect.X; w < breakW; w++)
                {
                    subPtr = (byte*)(subIptr);
                    for (int y = 0; y < subImage.Height; y++)
                    {
                        for (int x = 0; x < subImage.Width; x++)
                        {
                            parentPtr = (byte*)((IntPtr)parentIptr + parentStride * (h + y) + (w + x) * 3);
                            subPtr = (byte*)((IntPtr)subIptr + subStride * y + x * 3);
                            try
                            {
                                if (ColorEqual(parentPtr[0], parentPtr[1], parentPtr[2], subPtr[0], subPtr[1], subPtr[2], similar))  //比较颜色
                                {
                                    isOk = true;
                                }
                                else
                                {
                                    isOk = false; break;
                                }
                            }
                            catch (Exception)
                            {
                                return list.ToArray();
                            }

                            
                        }
                        if (isOk == false) { break; }
                    }
                    if (isOk) { list.Add(new Rectangle(w, h, subImage.Width, subImage.Height)); }
                    isOk = false;
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// 透明找图
        /// </summary>
        /// <param name="S_Data">大图数据</param>
        /// <param name="P_Data">小图数据</param>
        /// <param name="excludeColor">排除的颜色</param>
        /// <param name="similar">误差值</param>
        /// <returns></returns>
        public unsafe Rectangle[] Match(BitmapData parentImage, BitmapData subImage, Rectangle searchRect, Color excludeColor, int similar)
        {
            List<Rectangle> list = new List<Rectangle>();
            int[,] pixelData = GetPixelData(subImage, excludeColor);
            int len = pixelData.GetLength(0);
            int parentStride = parentImage.Stride;
            int subStride = subImage.Stride;
            IntPtr parentIptr = parentImage.Scan0;
            IntPtr subIptr = subImage.Scan0;
            byte* parentPtr;
            byte* subPtr;
            bool isOk = false;
            int breakW = searchRect.X + searchRect.Width - subImage.Width + 1;
            int breakH = searchRect.Y + searchRect.Height - subImage.Height + 1;

            for (int h = searchRect.Y; h < breakH; h++)
            {
                for (int w = searchRect.X; w < breakW; w++)
                {
                    for (int i = 0; i < len; i++)
                    {
                        parentPtr = (byte*)((IntPtr)parentIptr + parentStride * (h + pixelData[i, 1]) + (w + pixelData[i, 0]) * 3);
                        subPtr = (byte*)((IntPtr)subIptr + subStride * pixelData[i, 1] + pixelData[i, 0] * 3);
                        try
                        {
                            if (ColorEqual(parentPtr[0], parentPtr[1], parentPtr[2], subPtr[0], subPtr[1], subPtr[2], similar))  //比较颜色
                            {
                                isOk = true;
                            }
                            else
                            {
                                isOk = false; break;
                            }
                        }
                        catch (Exception)
                        {
                            return list.ToArray();
                        }

                        
                    }
                    if (isOk) { list.Add(new Rectangle(w, h, subImage.Width, subImage.Height)); }
                    isOk = false;
                }
            }
            return list.ToArray();
        }

        private static unsafe int[,] GetPixelData(BitmapData P_Data, Color BackColor)
        {
            byte B = BackColor.B, G = BackColor.G, R = BackColor.R;
            int Width = P_Data.Width, Height = P_Data.Height;
            int P_stride = P_Data.Stride;
            IntPtr P_Iptr = P_Data.Scan0;
            byte* P_ptr;
            int[,] PixelData = new int[Width * Height, 2];
            int i = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    P_ptr = (byte*)((IntPtr)P_Iptr + P_stride * y + x * 3);
                    if (B == P_ptr[0] & G == P_ptr[1] & R == P_ptr[2])
                    {

                    }
                    else
                    {
                        PixelData[i, 0] = x;
                        PixelData[i, 1] = y;
                        i++;
                    }
                }
            }
            int[,] PixelData2 = new int[i, 2];
            Array.Copy(PixelData, PixelData2, i * 2);
            return PixelData2;
        }

        //找图BGR比较
        private static unsafe bool ColorEqual(byte b1, byte g1, byte r1, byte b2, byte g2, byte r2, int similar)
        {
            if ((Math.Abs(b1 - b2)) > similar) { return false; } //B
            if ((Math.Abs(g1 - g2)) > similar) { return false; } //G
            if ((Math.Abs(r1 - r2)) > similar) { return false; } //R
            return true;
        }
    }
}
