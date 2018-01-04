using System;
using System.Drawing;

namespace Automation
{   
    public class Target
    {
        public float similarity;
        public Rectangle rect;

        public Target(Rectangle rect, float similarity)
        {
            this.similarity = similarity;
            this.rect = rect;
        }

        public Point Centre 
        {
            get
            {
                return new Point(rect.X + (rect.Width / 2), rect.Y + (rect.Height / 2));
            }
        }

        public Point randomPoint
        {
            get
            {
                Random rnd = new Random();
                Console.WriteLine(rnd.Next(0, rect.Width));
                return new Point(rect.X + rnd.Next(0, rect.Width), rect.Y + rnd.Next(0, rect.Height));
            }
        }
    }
}