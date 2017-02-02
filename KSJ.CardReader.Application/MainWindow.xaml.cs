using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
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
using KSJ.CardReader.Core.Detection;
using KSJ.CardReader.Core.Helpers;
using Size = System.Drawing.Size;

namespace KSJ.CardReader.Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Camera Capture Variables
        private Capture _capture = null; //Camera
        private bool _captureInProgress = false; //Variable to track camera state
        int CameraDevice = 1; //Variable to track camera device selected
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
        private CardExtractor _extractor;
        private MaterialHelper _matHlp;

        public MainWindow()
        {
            InitializeComponent();
            WebCams = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            _capture = new Capture(CameraDevice);
            _capture.ImageGrabbed += ProcessFrame;
            _extractor = new CardExtractor();
            _matHlp = new MaterialHelper();
            _extractor.ImageProcessed += _extractor_ImageProcessed;
        }

        private void _extractor_ImageProcessed(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                txtNumCards.Text = _extractor.NumberOfCardsDetected.ToString();
                BindAll();
            }));

        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            //***If you want to access the image data the use the following method call***/
            //Image<Bgr, Byte> frame = new Image<Bgr,byte>(_capture.RetrieveBgrFrame().ToBitmap());
            using (var mat = new Mat())
            {
                _capture.Retrieve(mat);
                _extractor.ProcessImage(mat);
            }
        }

        private void Load_Image_Click(object sender, RoutedEventArgs e)
        {
            if (!UseWebCam)
            {
                _capture.Stop();
                var dlg = new OpenFileDialog();
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _localPath = dlg.FileName;
                    _extractor.ProcessImage(CvInvoke.Imread(_localPath, LoadImageType.AnyColor));
                }
            }
            else
            {
                _capture.Start();
                using (var mat = new Mat())
                {
                    var res = _capture.Retrieve(mat);
                    var cMat = new Mat();
                    mat.CopyTo(cMat);
                    if (res)
                        _extractor.ProcessImage(cMat);
                }
            }
        }

        private void BindAll()
        {
            BindModified(_extractor.ProcessedImage);
            BindOriginal(_extractor.ModifiedImage?.ToJpegData(100));
            BindList(_extractor.Cards);
        }


        private void BindModified(Mat gray)
        {
            if (gray == null) return;
            this.Dispatcher.Invoke((Action)(() =>
           {
               var bytes = _matHlp.MatToByte(gray);
               if (bytes == null) return;
               var m = new MemoryStream(bytes);

               var bmpMod = new BitmapImage();
               bmpMod.BeginInit();
               bmpMod.StreamSource = m;
               bmpMod.EndInit();
               imgModified.Source = bmpMod;

           }));
        }

        private void BindOriginal(byte[] img)
        {
            if (img == null) return;
            this.Dispatcher.Invoke((Action)(() =>
            {

                var bmpMod = new BitmapImage();
                bmpMod.BeginInit();
                bmpMod.StreamSource = new MemoryStream(img);
                bmpMod.EndInit();
                imgOriginal.Source = bmpMod;

            }));
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

        private void BindList(IEnumerable<Mat> materials)
        {
            var mats = materials?.ToArray();
            if (mats == null || !mats.Any()) return;
            this.Dispatcher.Invoke((Action)(() =>
           {
               var lst = new ObservableCollection<ImageSource>();
               foreach (var mat in mats)
               {
                   var bytes = _matHlp.MatToByte(mat);
                   if (bytes == null) continue;
                   var bmp = new BitmapImage();
                   bmp.BeginInit();
                   bmp.StreamSource = new MemoryStream(bytes);
                   bmp.EndInit();
                   lst.Add(bmp);

               }

               lstCards.ItemsSource = lst;
           }));
        }



        private void txtBlur_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (int.TryParse(txtBlur.Text, NumberStyles.Integer, new NumberFormatInfo(), out _blur) && _blur % 2 != 0)
            {
                _extractor.Blur = _blur;
                _extractor.ProcessImage(_extractor.OriginalImage);
            }

        }

        private void txtTMin_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (int.TryParse(txtTMin.Text, NumberStyles.Integer, new NumberFormatInfo(), out _threshMin))
            {
                _extractor.MinimumThreshHold = _threshMin;
                _extractor.ProcessImage(_extractor.OriginalImage);
            }
        }

        private void txtTMax_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (int.TryParse(txtTMax.Text, NumberStyles.Integer, new NumberFormatInfo(), out _threshMax))
            {
                _extractor.MaximumThreshHold = _threshMax;
                _extractor.ProcessImage(_extractor.OriginalImage);
            }
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
            {
                _extractor.ProcessImage(_extractor.OriginalImage);
            }
        }

        private void txtNumCards_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (int.TryParse(txtNumCards.Text, NumberStyles.Integer, new NumberFormatInfo(), out _numcards))
            {
                _extractor.ProcessImage(_extractor.OriginalImage);
            }
        }

        private void chkWebCam_Checked(object sender, RoutedEventArgs e)
        {
            UseWebCam = chkWebCam.IsChecked != null && chkWebCam.IsChecked.Value;
        }

        private void chkWebCam_Unchecked(object sender, RoutedEventArgs e)
        {
            UseWebCam = chkWebCam.IsChecked != null && chkWebCam.IsChecked.Value;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var train = new Training();
            train.Show();
        }
    }
}
