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

        private readonly ImagePreProcessor _process;
        private VectorOrderRepository _vORepo;
        private CardHelper _cardHelper;
        private CardRectifier _cardRectifier;
        private GeometryHelper _geoHlp;

        public CardExtractor()
        {
            _process = new ImagePreProcessor();
            _vORepo = new VectorOrderRepository();
            _cardHelper = new CardHelper();
            _cardRectifier = new CardRectifier();
            _geoHlp = new GeometryHelper();
        }

        public void ProcessImage(Mat mat)
        {
            if (mat == null) return;
            OriginalImage = mat;
            ModifiedImage = new Image<Bgr, byte>(OriginalImage.Bitmap);


            VectorOfVectorOfPoint contours;
            Mat matContours;
            OriginalImage = _process.Process(OriginalImage, out contours, out matContours);
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
                _process.Process(card, out contour, out matC);
                var cropped = _process.Crop(matC);
                lst.Add(cropped.Mat);

                CvInvoke.DrawContours(matContours, contours, c.ListIndex, new MCvScalar(0, 255, 0), 3);
            }

            NumberOfCardsDetected = 0;

            Cards = lst;
            ProcessedImage = matContours;
        }
        
        
       
    }
}
