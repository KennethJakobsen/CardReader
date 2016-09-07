using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Util;

namespace KSJ.CardReader.Core.Detection
{
    public class VectorOrderRepository
    {
        public List<VectorOrder> List(VectorOfVectorOfPoint contours)
        {
            var lst = new List<VectorOrder>();
            for (var i = 0; i < contours.Size; i++)
            {

                lst.Add(new VectorOrder
                {
                    Size = CvInvoke.ContourArea(contours[i]),
                    ListIndex = i,
                    Vector = contours[i],
                    Box = CvInvoke.BoundingRectangle(contours[i])
                });
            }
            return lst;
        }
    }
}
