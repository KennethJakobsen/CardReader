using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using KSJ.CardReader.Core.Helpers;

namespace KSJ.CardReader.Core.Processing
{
    public class ImagePreProcessor
    {
        private MaterialHelper _matHlp;

        public ImagePreProcessor()
        {
            _matHlp = new MaterialHelper();
        }
        public Mat Process(Mat img, out VectorOfVectorOfPoint contours, out Mat matContours, int numBlur = 1, int threshMin = 175, int threshMax = 255)
        {

            var gray = _matHlp.GetMat(img);
            CvInvoke.CvtColor(img, gray, ColorConversion.Bgr2Gray);
            var blur = _matHlp.GetMat(img);
            CvInvoke.GaussianBlur(gray, blur, new Size(numBlur, numBlur), 100);
            var threshold = _matHlp.GetMat(img);
            CvInvoke.Threshold(blur, threshold, threshMin, threshMax, ThresholdType.Binary);
            var eqHist = _matHlp.GetMat(img);
            CvInvoke.EqualizeHist(threshold, eqHist);
            contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(threshold, contours, null, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
            matContours = _matHlp.GetMat(img);
            CvInvoke.DrawContours(matContours, contours, -1, new MCvScalar(200, 200, 200));


            return img;
        }

        public Image<Bgr, byte> Crop(Mat matC)
        {
           return Crop(matC, Rectangle.Empty);
        }

        public Image<Bgr, byte> Crop(Mat matC, Rectangle mask)
        {
            if (matC?.Width == 0) return null;
            if(mask == Rectangle.Empty)
                mask = new Rectangle(5, 20, 60, 180);
            var cropped = matC.ToImage<Bgr, byte>();
            cropped.ROI = mask;
            return cropped;
        }
    }
}
