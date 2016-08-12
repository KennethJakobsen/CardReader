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
        }

        private void Load_Image_Click(object sender, RoutedEventArgs e)
        {
            if (!UseWebCam)
            {
                var dlg = new OpenFileDialog();
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    _localPath = dlg.FileName;
                ProcessImage();
            }
            else
            {
                ProcessImage(_capture.QueryFrame());
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
            var largestCard = potentialCards.First();

            var cards = potentialCards
                    .Where(c => IsAcceptableSize(c.Size, 10, largestCard.Size))
                    //.Take(52)
                    .OrderBy(c => c.Box.X + c.Box.Y)
                    .ToArray();
            for(var i = 0; i < cards.Length; i++)
            {
                cardCount = i;
                lst.Add(RectifyCardFromWarpedPerspective(cards[i], _img));
                CvInvoke.DrawContours(matContours, contours, cards[i].ListIndex, new MCvScalar(0, 255, 0), 3);
            }
            cardCount = 0;
            BindOriginal(Debug.Bitmap);
            BindList(lst);
            BindModified(matContours);
        }

        private bool IsAcceptableSize(double size, int percentageDiff, double max)
        {
            return ((max/100)*(100 - percentageDiff)) <= size;
        }

        private bool IsRectangle(VectorOfPoint c)
        {
            var approxContour = new VectorOfPoint();
            CvInvoke.ApproxPolyDP(c, approxContour, CvInvoke.ArcLength(c, true)*0.05, true);
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
            
            var card = c.Vector;
            var perimeter = CvInvoke.ArcLength(card, true);
            var polyDp = new VectorOfPointF();
            CvInvoke.ApproxPolyDP(card, polyDp, _perspective*perimeter, true);
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

            
            Debug.Draw(cardCount.ToString(), convertToPoints(old)[3], FontFace.HersheyPlain, 5, new Bgr(0, 0, 0), 8);
            
            Debug.DrawPolyline(convertToPoints(old), true, new Bgr(0,0,0),3);
            
            var transform = CvInvoke.GetPerspectiveTransform(old, points);
            
            var warp = GetMat(img);
            CvInvoke.WarpPerspective(img, warp, transform, new Size(499,750));
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
            var bmp =
                new BitmapImage(
                    new Uri(localPath));
            imgOriginal.Source = bmp;
        }

        private void BindModified(Mat gray)
        {
            var bmpMod = new BitmapImage();
            bmpMod.BeginInit();
            bmpMod.StreamSource = new MemoryStream(ImageToByte(gray.Bitmap));
            bmpMod.EndInit();
            imgModified.Source = bmpMod;
        }
        private void BindOriginal(Mat gray)
        {
            var bmpMod = new BitmapImage();
            bmpMod.BeginInit();
            bmpMod.StreamSource = new MemoryStream(ImageToByte(gray.Bitmap));
            bmpMod.EndInit();
            imgOriginal.Source = bmpMod;
        }
        private void BindOriginal(Bitmap gray)
        {
            var bmpMod = new BitmapImage();
            bmpMod.BeginInit();
            bmpMod.StreamSource = new MemoryStream(ImageToByte(gray));
            bmpMod.EndInit();
            imgOriginal.Source = bmpMod;
        }
        private void BindList(IEnumerable<Mat> materials)
        {
            var lst = new ObservableCollection<ImageSource>();
            foreach (var mat in materials)
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(ImageToByte(mat.Bitmap));
                bmp.EndInit();
                lst.Add(bmp);
            }

            lstCards.ItemsSource = lst;
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
            _capture.Start();
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
