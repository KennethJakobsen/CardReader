using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using KSJ.CardReader.Core.Helpers;
using KSJ.CardReader.Core.Processing;

namespace KSJ.CardReader.Core.Detection
{
    public class CardExtractor
    {
        public Mat OriginalImage { get; set; }
        public Mat ProcessedImage { get; set; }
        public Image<Bgr, byte> ModifiedImage { get; set; }
        public int NumberOfCardsDetected { get; set; }
        public int Blur { get; set; }
        public int MaximumThreshHold { get; set; }
        public int MinimumThreshHold { get; set; }
        public IEnumerable<Mat> Cards { get; set; }
        public event EventHandler ImageProcessed;
        public event EventHandler ImageProcessing;

        private readonly ImagePreProcessor _process;
        private VectorOrderRepository _vORepo;
        private CardHelper _cardHelper;
        private CardRectifier _cardRectifier;
        private GeometryHelper _geoHlp;
        private MaterialHelper _matHlp;

        public CardExtractor()
        {
            _process = new ImagePreProcessor();
            _vORepo = new VectorOrderRepository();
            _cardHelper = new CardHelper();
            _cardRectifier = new CardRectifier();
            _geoHlp = new GeometryHelper();
            _matHlp = new MaterialHelper();
            Blur = 1;
            MaximumThreshHold = 255;
            MinimumThreshHold = 170;
        }

        public void ProcessImage(Mat mat)
        {
            ImageProcessing?.Invoke(this, EventArgs.Empty);

            if (mat == null) return;
            OriginalImage = mat;
            ModifiedImage = new Image<Bgr, byte>(OriginalImage.Bitmap);


            VectorOfVectorOfPoint contours;
            var matContours = _matHlp.GetMat(mat);
            OriginalImage = _process.Process(OriginalImage, out contours, out matContours, Blur, MinimumThreshHold, MaximumThreshHold);
            var lst = new List<Mat>();
            var potentialCards = _cardHelper.GetPotentialCards(_vORepo.List(contours));
            var largestContour = potentialCards.FirstOrDefault();

            var cards = _cardHelper.GetCards(potentialCards, largestContour).ToArray();
            NumberOfCardsDetected = cards.Count();

            for (var index = 0; index < cards.Length; index++)
            {
                var c = cards[index];
                var card = _cardRectifier.RectifyCardFromWarpedPerspective(c, OriginalImage, index.ToString(),modifiedImage: ModifiedImage);


                var contour = new VectorOfVectorOfPoint();
                var matC = new Mat();
                var processed = _process.Process(card, out contour, out matC, Blur, MinimumThreshHold, MaximumThreshHold);
                //var cropped = null;//_process.Crop(matC);
                //if(cropped != null)
                //    lst.Add(cropped.Mat);
                //else
                    lst.Add(card);

                CvInvoke.DrawContours(matContours, contours, c.ListIndex, new MCvScalar(0, 255, 0), 3);
            }
            

            Cards = lst;
            ProcessedImage = matContours;

            ImageProcessed?.Invoke(this, EventArgs.Empty);
        }
        
        
       
    }
}
