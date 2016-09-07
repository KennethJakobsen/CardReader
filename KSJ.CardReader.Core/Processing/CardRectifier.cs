using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using KSJ.CardReader.Core.Detection;
using KSJ.CardReader.Core.Helpers;

namespace KSJ.CardReader.Core.Processing
{
    public class CardRectifier
    {
        private MaterialHelper _matHlp;
        private GeometryHelper _geoHlp;

        public CardRectifier()
        {
            _matHlp = new MaterialHelper();
            _geoHlp = new GeometryHelper();
        }
        public Mat RectifyCardFromWarpedPerspective(VectorOrder c, Mat img, string idText, double perspective = 0.02, Image<Bgr, byte> modifiedImage = null)
        {
            var card = c.Vector;
            var perimeter = CvInvoke.ArcLength(card, true);
            var polyDp = new VectorOfPointF();
            CvInvoke.ApproxPolyDP(card, polyDp, perspective * perimeter, true);
            var rectangle = CvInvoke.MinAreaRect(card);
            var box = CvInvoke.BoxPoints(rectangle);
            var points = new PointF[4];
            points[0] = new PointF(0, 0);
            points[1] = new PointF(499, 0);
            points[2] = new PointF(499, 750);
            points[3] = new PointF(0, 750);

            var pArr = polyDp.ToArray();
            var old = new PointF[4];
            old[0] = pArr.OrderBy(p => p.X + p.Y).First();
            old[2] = pArr.OrderByDescending(p => p.X + p.Y).First();
            old[3] = pArr.OrderBy(p => p.X - p.Y).First();
            old[1] = pArr.OrderByDescending(p => p.X - p.Y).First();

            if (modifiedImage != null)
            {
                modifiedImage.Draw(idText, _geoHlp.ConvertToPoints(old)[3], FontFace.HersheyPlain, 5, new Bgr(0, 0, 0), 8);
                modifiedImage.DrawPolyline(_geoHlp.ConvertToPoints(old), true, new Bgr(0, 0, 0), 3);
            }

            var transform = CvInvoke.GetPerspectiveTransform(old, points);

            var warp = _matHlp.GetMat(img);
            CvInvoke.WarpPerspective(img, warp, transform, new Size(499, 750));
            return warp;
        }
    }
}
