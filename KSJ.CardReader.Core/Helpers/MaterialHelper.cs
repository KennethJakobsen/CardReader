using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;

namespace KSJ.CardReader.Core.Helpers
{
    public class MaterialHelper
    {
        public Mat GetMat(Mat img)
        {
            return new Mat(img.Size, img.Depth, img.NumberOfChannels);
        }
        public byte[] MatToByte(Mat img)
        {
            if(img.Cols != 0 || img.Rows != 0)
                return img.ToImage<Bgr, byte>().ToJpegData(100);
            return null;
        }
    }
}
