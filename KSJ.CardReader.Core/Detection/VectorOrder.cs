using System.Drawing;
using Emgu.CV.Util;

namespace KSJ.CardReader.Core.Detection
{
    public class VectorOrder
    {
        public int ListIndex { get; set; }
        public double Size { get; set; }
        public VectorOfPoint Vector { get; set; }
        public Rectangle Box { get; set; }
    }
}
