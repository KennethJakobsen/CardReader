using System;
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
            try
            {
                if (img.Width != 0 || img.Height != 0)
                    return img.ToImage<Bgr, byte>().ToJpegData(100);
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
