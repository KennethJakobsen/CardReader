using System.Collections.Generic;
using System.Linq;
using Emgu.CV;
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
            return approxContour.Size == 4;
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
