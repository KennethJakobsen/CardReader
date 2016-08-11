using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Size = System.Drawing.Size;

namespace OpenCvTests
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _localPath = string.Empty;
        private int _blur = 1;
        private int _threshMin = 175;
        private int _threshMax = 255;
        private double _perspective = 0.02;
        private int _numcards = 2;
        private Mat _img;
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
        }

        private void Load_Image_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();


            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                _localPath = dlg.FileName;


            ProcessImage();
        }

        private void ProcessImage()
        {
            if (_localPath == string.Empty) return;
            BindOriginal(_localPath);

            VectorOfVectorOfPoint contours;
            Mat matContours;
            _img = PreProcess(_localPath, out contours, out matContours);
            var lst = new List<Mat>();
            foreach (var c in listCountours(contours).OrderByDescending(c => c.Size).Take(_numcards).OrderBy(c => c.Box.X).ThenBy(c => c.Box.Y))
            {
                lst.Add(RectifyCardFromWarpedPerspective(c, _img));
                CvInvoke.DrawContours(matContours, contours, c.ListIndex, new MCvScalar(0, 255, 0), 3);
            }
            BindList(lst);
            BindModified(matContours);
        }

        private Mat PreProcess(string localPath, out VectorOfVectorOfPoint contours, out Mat matContours)
        {
            var img = CvInvoke.Imread(localPath, LoadImageType.AnyColor);
            var gray = GetMat(img);
            CvInvoke.CvtColor(img, gray, ColorConversion.Bgr2Gray);
            var blur = GetMat(img);
            CvInvoke.GaussianBlur(gray, blur, new Size(_blur, _blur), 100);
            var threshold = GetMat(img);
            CvInvoke.Threshold(blur, threshold, _threshMin, _threshMax, ThresholdType.Binary);
            var eqHist = GetMat(img);
            CvInvoke.EqualizeHist(threshold, eqHist);
            var im = eqHist.ToImage<Bgr, byte>();
            im._GammaCorrect(1.8);
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
            points[2] = new PointF(499, 499);
            points[3] = new PointF(0, 499);

            var pArr = polyDp.ToArray();
            var old = new PointF[4];
            old[2] = pArr.OrderBy(p => p.X).First();
            old[0] = pArr.OrderByDescending(p => p.X).First();
            old[3] = pArr.OrderBy(p => p.Y).First();
            old[1] = pArr.OrderByDescending(p => p.Y).First();

            
            var transform = CvInvoke.GetPerspectiveTransform(old, points);
            
            var warp = GetMat(img);
            CvInvoke.WarpPerspective(img, warp, transform, new Size(499,499));
            return warp;
        }

        private List<VectorOrder> listCountours(VectorOfVectorOfPoint contours)
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
    }
}
