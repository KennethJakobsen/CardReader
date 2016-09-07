using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Util;

namespace KSJ.CardReader.Core.Helpers
{
    public class GeometryHelper
    {
        public Point[] ConvertToPoints(PointF[] old)
        {
            var p = new Point[old.Length];
            for (var i = 0; i < old.Length; i++)
            {
                p[i] = new Point(Convert.ToInt32(old[i].X), Convert.ToInt32(old[i].Y));
            }
            return p;
        }

        //Possible performance issue
        public Rectangle GetBoundsForContoursInRectangle(VectorOfVectorOfPoint contours, Rectangle collissionBox)
        {
            var rects = new List<Rectangle>();
            for (var i = 0; i < contours.Size; i++)
            {
                if(IsInRectangle(contours[i], collissionBox))
                    rects.Add(CvInvoke.MinAreaRect(contours[i]).MinAreaRect());
            }
            var minX = rects.OrderBy(r => r.X).First().X;
            var minY = rects.OrderBy(r => r.Y).First().Y;
            var width = rects.Select(r => r.X + r.Width).OrderByDescending(i => i).First()  - minX;
            var height = rects.Select(r => r.Y + r.Height).OrderByDescending(i => i).First() - minY;
            return new Rectangle(minX, minY, width, height);
        }

        public bool IsInRectangle(VectorOfPoint point, Rectangle box)
        {
            return point.ToArray().Any(box.Contains);
        }
    }
}
