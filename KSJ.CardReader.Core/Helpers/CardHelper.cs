using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using KSJ.CardReader.Core.Detection;

namespace KSJ.CardReader.Core.Helpers
{
    public class CardHelper
    {

        public bool IsAcceptableSize(double size, int percentageDiff, double max)
        {
            return ((max / 100) * (100 - percentageDiff)) <= size;
        }

        public bool IsRectangle(VectorOfPoint c)
        {
            var approxContour = new VectorOfPoint();
            CvInvoke.ApproxPolyDP(c, approxContour, CvInvoke.ArcLength(c, true) * 0.05, true);
            var area = CvInvoke.ContourArea(approxContour, false);
            if (!(area > 250)) return false;
            if(approxContour.Size != 4) return false;
            var isRectangle = true;
            var pts = approxContour.ToArray();
            var edges = PointCollection.PolyLine(pts, true);
            //using edges i found coordinates.
            for (int i = 0; i < edges.Length; i++)
            {
                double angle = Math.Abs(
                    edges[(i + 1) % edges.Length].GetExteriorAngleDegree(edges[i]));
                if (angle < 75 || angle > 105)
                {
                    isRectangle = false;
                    break;
                }
                
            }
            return isRectangle;
        }

        public IOrderedEnumerable<VectorOrder> GetPotentialCards(List<VectorOrder> contours)
        {
            return contours
               .Where(c => IsRectangle(c.Vector))
               .OrderByDescending(c => c.Size);
        }

        public IEnumerable<VectorOrder> GetCards(IEnumerable<VectorOrder> potentialCards, VectorOrder largestContour, int bufferPercentage = 15)
        {
            return potentialCards
                    .Where(c => IsAcceptableSize(c.Size, bufferPercentage, largestContour.Size))
                    .OrderBy(c => c.Box.X)
                    .ThenBy(c => c.Box.Y)
                    .ToArray();
        } 
    }
}
