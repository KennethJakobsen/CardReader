using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.ML;
using Emgu.CV.ML.MlEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Size = System.Drawing.Size;

namespace EmguCvTests
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Camera Capture Variables
        private Capture _capture = null; //Camera
        private bool _captureInProgress = false; //Variable to track camera state
        int CameraDevice = 0; //Variable to track camera device selected
        FilterInfoCollection WebCams; //List containing all the camera available
        #endregion

        private string _localPath = string.Empty;
        private int _blur = 1;
        private int _threshMin = 175;
        private int _threshMax = 255;
        private double _perspective = 0.02;
        private int _numcards = 2;
        private Mat _img;
        public bool UseWebCam = false;
        public Image<Bgr, byte> Debug;
        private int cardCount = 0;
        private Tesseract _ocr;
        public class VectorOrder
        {
            public int ListIndex { get; set; }
            public double Size { get; set; }
            public VectorOfPoint Vector { get; set; }
            public Rectangle Box { get; set; }
        }
        public MainWindow()
        {
            InitializeComponent();
            WebCams = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            _capture = new Capture(CameraDevice);
            _capture.ImageGrabbed += ProcessFrame;

        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            //***If you want to access the image data the use the following method call***/
            //Image<Bgr, Byte> frame = new Image<Bgr,byte>(_capture.RetrieveBgrFrame().ToBitmap());
            using (var mat = new Mat())
            {
                _capture.Retrieve(mat);
                ProcessImage(mat);
                //BindOriginal(mat);
            }
        }

        private void Load_Image_Click(object sender, RoutedEventArgs e)
        {
            if (!UseWebCam)
            {
                _capture.Stop();
                var dlg = new OpenFileDialog();
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    _localPath = dlg.FileName;
                ProcessImage();
            }
            else
            {
                _capture.Start();
                using (var mat = new Mat())
                {
                    _capture.Retrieve(mat);
                    ProcessImage(mat);
                }
            }
        }

        private void ProcessImage(Mat mat = null)
        {
            if (_localPath != string.Empty && !UseWebCam)
            {
                _img = CvInvoke.Imread(_localPath, LoadImageType.AnyColor);
                Debug = new Image<Bgr, byte>(_img.Bitmap);
                BindOriginal(_img);
            }
            else
            {
                if (mat != null)
                {
                    _img = mat;
                    Debug = new Image<Bgr, byte>(_img.Bitmap);
                    BindOriginal(mat);
                }
                else
                {
                    return;
                }
            }


            VectorOfVectorOfPoint contours;
            Mat matContours;
            _img = PreProcess(_img, out contours, out matContours);
            var lst = new List<Mat>();
            var potentialCards = listCountours(contours)
                .Where(c => IsRectangle(c.Vector))
                .OrderByDescending(c => c.Size);
            var largestCard = potentialCards.FirstOrDefault();

            var cards = potentialCards
                    .Where(c => IsAcceptableSize(c.Size, 15, largestCard.Size))
                    //.Take(52)
                    .OrderBy(c => c.Box.X)
                    .ThenBy(c => c.Box.Y)
                    .ToArray();
            _numcards = cards.Length;
            this.Dispatcher.Invoke((Action) (() =>
            {
                txtNumCards.Text = _numcards.ToString();
            }));

            for (var i = 0; i < cards.Length; i++) 
            {
                cardCount = i;
                var card = RectifyCardFromWarpedPerspective(cards[i], _img);
                
                
                var contour = new VectorOfVectorOfPoint();
                var matC = new Mat();
                PreProcess(card, out contour, out matC);
                lst.Add(matC);
                //var od = new LicensePlateDetector(@"c:/cardreader/tessdata/");
                //var lstRegions = new List<IInputOutputArray>();
                //var lstFiltered = new List<IInputOutputArray>();
                //var bounds = new List<RotatedRect>();
                //var result =  od.DetectLicensePlate(contour, lstRegions,  lstFiltered, bounds);

                CvInvoke.DrawContours(matContours, contours, cards[i].ListIndex, new MCvScalar(0, 255, 0), 3);

            }

            //var trainCards = new VectorOfVectorOfPoint(cards.Select(c => c.Vector).ToArray());
            //MachineLearningTrain(trainCards);
            cardCount = 0;
            if (Debug != null)
                BindOriginal(Debug.ToJpegData(100));
            else
                BindOriginal(_img);
            BindList(lst);
            BindModified(matContours);
        }

        private bool IsAcceptableSize(double size, int percentageDiff, double max)
        {
            return ((max / 100) * (100 - percentageDiff)) <= size;
        }

        private bool IsRectangle(VectorOfPoint c)
        {
            var approxContour = new VectorOfPoint();
            CvInvoke.ApproxPolyDP(c, approxContour, CvInvoke.ArcLength(c, true) * 0.05, true);
            var area = CvInvoke.ContourArea(approxContour, false);
            if (!(area > 250)) return false;
            return approxContour.Size == 4;
        }

        private Mat PreProcess(Mat img, out VectorOfVectorOfPoint contours, out Mat matContours)
        {

            var gray = GetMat(img);
            CvInvoke.CvtColor(img, gray, ColorConversion.Bgr2Gray);
            var blur = GetMat(img);
            CvInvoke.GaussianBlur(gray, blur, new Size(_blur, _blur), 100);
            var threshold = GetMat(img);
            CvInvoke.Threshold(blur, threshold, _threshMin, _threshMax, ThresholdType.Binary);
            var eqHist = GetMat(img);
            CvInvoke.EqualizeHist(threshold, eqHist);
            contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(threshold, contours, null, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
            matContours = GetMat(img);
            CvInvoke.DrawContours(matContours, contours, -1, new MCvScalar(200, 200, 200));


            return img;
        }

        private Mat RectifyCardFromWarpedPerspective(VectorOrder c, Mat img)
        {
            _ocr = new Tesseract();
            var card = c.Vector;
            var perimeter = CvInvoke.ArcLength(card, true);
            var polyDp = new VectorOfPointF();
            CvInvoke.ApproxPolyDP(card, polyDp, _perspective * perimeter, true);
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

            if (Debug != null)
            {
                Debug.Draw(cardCount.ToString(), convertToPoints(old)[3], FontFace.HersheyPlain, 5, new Bgr(0, 0, 0), 8);
                Debug.DrawPolyline(convertToPoints(old), true, new Bgr(0, 0, 0), 3);
            }

            var transform = CvInvoke.GetPerspectiveTransform(old, points);

            var warp = GetMat(img);
            CvInvoke.WarpPerspective(img, warp, transform, new Size(499, 750));
            return warp;
        }

        private List<VectorOrder> listCountours(VectorOfVectorOfPoint contours)
        {
            var lst = new List<MainWindow.VectorOrder>();
            for (var i = 0; i < contours.Size; i++)
            {

                lst.Add(new MainWindow.VectorOrder
                {
                    Size = CvInvoke.ContourArea(contours[i]),
                    ListIndex = i,
                    Vector = contours[i],
                    Box = CvInvoke.BoundingRectangle(contours[i])
                });
            }
            return lst;
        }

        private System.Drawing.Point[] convertToPoints(PointF[] old)
        {
            var p = new System.Drawing.Point[old.Length];
            for (var i = 0; i < old.Length; i++)
            {
                p[i] = new System.Drawing.Point(Convert.ToInt32(old[i].X), Convert.ToInt32(old[i].Y));
            }
            return p;
        }
        private static Mat GetMat(Mat img)
        {
            return new Mat(img.Size, img.Depth, img.NumberOfChannels);
        }

        private void BindOriginal(string localPath)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                var bmp =
                new BitmapImage(
                    new Uri(localPath));
                imgOriginal.Source = bmp;
            }));
        }


        private void BindModified(Mat gray)
        {
            this.Dispatcher.Invoke((Action)(() =>
           {
               var m = new MemoryStream(ImageToByte(gray));

               var bmpMod = new BitmapImage();
               bmpMod.BeginInit();
               bmpMod.StreamSource = m;
               bmpMod.EndInit();
               imgModified.Source = bmpMod;

           }));
        }

        private void BindOriginal(Mat gray)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {

                var bmpMod = new BitmapImage();
                bmpMod.BeginInit();
                bmpMod.StreamSource = new MemoryStream(ImageToByte(gray));
                bmpMod.EndInit();
                imgOriginal.Source = bmpMod;

            }));
        }

        public class PlayingCard
        {
            public enum CardColor
            {
                Spades,
                Clubs,
                Hearts,
                Diamonds
            }

            public CardColor Color { get; set; }
            public int Number { get; set; }
        }

        private Matrix<int> GetTrainingClasses()
        {
            var mtrx = new Matrix<int>(new Size(1, 54));
            mtrx.GetRow(0).SetValue(1); // Ace of Spades
            mtrx.GetRow(4).SetValue(2); // 2 of Spades
            mtrx.GetRow(8).SetValue(3); // 3 of Spades
            mtrx.GetRow(12).SetValue(4); // 4 of Spades
            mtrx.GetRow(16).SetValue(5); // 5 of Spades
            mtrx.GetRow(23).SetValue(6); // 6 of Spades
            mtrx.GetRow(29).SetValue(7); // 7 of Spades
            mtrx.GetRow(33).SetValue(8); // 8 of Spades
            mtrx.GetRow(37).SetValue(9); // 9 of Spades
            mtrx.GetRow(41).SetValue(10); // 10 of Spades
            mtrx.GetRow(42).SetValue(11); // Jack of Spades
            mtrx.GetRow(46).SetValue(12); // Queen of Spades
            mtrx.GetRow(50).SetValue(13); // king of Spades

            mtrx.GetRow(1).SetValue(14); // 1 of Clubs
            mtrx.GetRow(5).SetValue(15); // 2 of Clubs
            mtrx.GetRow(9).SetValue(16); // 3 of Clubs
            mtrx.GetRow(13).SetValue(17); // 4 of Clubs
            mtrx.GetRow(17).SetValue(18); // 5 of Clubs
            mtrx.GetRow(24).SetValue(19); // 6 of Clubs
            mtrx.GetRow(28).SetValue(20); // 7 of Clubs
            mtrx.GetRow(32).SetValue(21); // 8 of Clubs
            mtrx.GetRow(35).SetValue(22); // 9 of Clubs
            mtrx.GetRow(40).SetValue(23); // 10 of Clubs
            mtrx.GetRow(44).SetValue(24); // 11 of Clubs
            mtrx.GetRow(48).SetValue(25); // 12 of Clubs
            mtrx.GetRow(51).SetValue(26); // 13 of Clubs

            mtrx.GetRow(2).SetValue(27);  // 1 of Diamonds
            mtrx.GetRow(6).SetValue(28);  // 2 of Diamonds
            mtrx.GetRow(10).SetValue(29); // 3 of Diamonds
            mtrx.GetRow(14).SetValue(30); // 4 of Diamonds
            mtrx.GetRow(19).SetValue(31); // 5 of Diamonds
            mtrx.GetRow(21).SetValue(32); // 6 of Diamonds
            mtrx.GetRow(26).SetValue(33); // 7 of Diamonds
            mtrx.GetRow(30).SetValue(34); // 8 of Diamonds
            mtrx.GetRow(36).SetValue(35); // 9 of Diamonds
            mtrx.GetRow(39).SetValue(36); // 10 of Diamonds
            mtrx.GetRow(45).SetValue(37); // 11 of Diamonds
            mtrx.GetRow(47).SetValue(38); // 12 of Diamonds
            mtrx.GetRow(52).SetValue(39); // 13 of Diamonds

            mtrx.GetRow(3).SetValue(40);  // 1 of Hearts
            mtrx.GetRow(7).SetValue(41);  // 2 of Hearts
            mtrx.GetRow(11).SetValue(42); // 3 of Hearts
            mtrx.GetRow(15).SetValue(43); // 4 of Hearts
            mtrx.GetRow(18).SetValue(44); // 5 of Hearts
            mtrx.GetRow(22).SetValue(45); // 6 of Hearts
            mtrx.GetRow(27).SetValue(46); // 7 of Hearts
            mtrx.GetRow(31).SetValue(47); // 8 of Hearts
            mtrx.GetRow(34).SetValue(48); // 9 of Hearts
            mtrx.GetRow(38).SetValue(49); // 10 of Hearts
            mtrx.GetRow(43).SetValue(50); // 11 of Hearts
            mtrx.GetRow(49).SetValue(51); // 12 of Hearts
            mtrx.GetRow(53).SetValue(52); // 13 of Hearts

            mtrx.GetRow(20).SetValue(53); // 12 of Hearts
            mtrx.GetRow(25).SetValue(53); // 13 of Hearts

            return mtrx;
        }
        private void MachineLearningTrain(IInputArray cards)
        {

            var td = new TrainData(cards, DataLayoutType.RowSample, GetTrainingClasses());
            var classifier = new NormalBayesClassifier();
            classifier.Train(td);
        }
        private void BindOriginal(byte[] img)
        {
            this.Dispatcher.Invoke((Action)(() =>
           {

               var bmpMod = new BitmapImage();
               bmpMod.BeginInit();
               bmpMod.StreamSource = new MemoryStream(img);
               bmpMod.EndInit();
               imgOriginal.Source = bmpMod;

           }));
        }

        private void BindList(IEnumerable<Mat> materials)
        {
            this.Dispatcher.Invoke((Action)(() =>
           {
               var lst = new ObservableCollection<ImageSource>();
               foreach (var mat in materials)
               {

                   var bmp = new BitmapImage();
                   bmp.BeginInit();
                   bmp.StreamSource = new MemoryStream(ImageToByte(mat));
                   bmp.EndInit();
                   lst.Add(bmp);

               }

               lstCards.ItemsSource = lst;
           }));
        }
        public static byte[] ImageToByte(Mat img)
        {
            return img.ToImage<Bgr, byte>().ToJpegData(100);
            var converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
        public static byte[] ImageToByte(Image img)
        {
            var converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        private void txtBlur_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (int.TryParse(txtBlur.Text, NumberStyles.Integer, new NumberFormatInfo(), out _blur) && _blur % 2 != 0)
                ProcessImage();

        }

        private void txtTMin_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (int.TryParse(txtTMin.Text, NumberStyles.Integer, new NumberFormatInfo(), out _threshMin))
                ProcessImage();
        }

        private void txtTMax_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (int.TryParse(txtTMax.Text, NumberStyles.Integer, new NumberFormatInfo(), out _threshMax))
                ProcessImage();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtTMax.Text = _threshMax.ToString();
            txtTMin.Text = _threshMin.ToString();
            txtBlur.Text = _blur.ToString();
            txtPerspective.Text = _perspective.ToString();
            txtNumCards.Text = _numcards.ToString();
            
            chkWebCam.DataContext = this;
        }

        private void txtPerspective_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (double.TryParse(txtPerspective.Text, NumberStyles.Integer, new NumberFormatInfo(), out _perspective))
                ProcessImage();
        }

        private void txtNumCards_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (int.TryParse(txtNumCards.Text, NumberStyles.Integer, new NumberFormatInfo(), out _numcards))
                ProcessImage();
        }

        private void chkWebCam_Checked(object sender, RoutedEventArgs e)
        {
            UseWebCam = chkWebCam.IsChecked != null && chkWebCam.IsChecked.Value;
        }

        private void chkWebCam_Unchecked(object sender, RoutedEventArgs e)
        {
            UseWebCam = chkWebCam.IsChecked != null && chkWebCam.IsChecked.Value;
        }
    }
}
