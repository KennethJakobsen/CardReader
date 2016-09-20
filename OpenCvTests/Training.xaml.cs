using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using EmguCvTests.ViewModel;
using KSJ.CardReader.Core.Detection;
using KSJ.CardReader.Core.Helpers;
using KSJ.CardReader.Core.MachineLearning;

namespace EmguCvTests
{
    /// <summary>
    /// Interaction logic for Training.xaml
    /// </summary>
    public partial class Training : Window
    {
        private readonly CardExtractor _extractor;
        private readonly MaterialHelper _matHlp;
        private string _localPath;
        private PlayingCardCollection _cards;

        public Training()
        {
            InitializeComponent();

            _extractor = new CardExtractor();
            _matHlp = new MaterialHelper();
            _extractor.ImageProcessed += _extractor_ImageProcessed;

        }


        private void _extractor_ImageProcessed(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                lblNumCards.Content = _extractor.NumberOfCardsDetected.ToString();
                BindList(_extractor.Cards);
                _cards = new PlayingCardCollection(_extractor.NumberOfCardsDetected);
            }));

            lblCardName.Content = _cards.SelectedCard;
        }
        private void BindList(IEnumerable<Mat> materials)
        {
            var mats = materials?.Select(m => new TrainingCardViewModel(m)).ToArray();
            if (mats == null || !mats.Any()) return;
            this.Dispatcher.Invoke((Action)(() =>
            {
                var lst = new ObservableCollection<ImageSource>();
                foreach (var mat in mats)
                {
                    var bytes = _matHlp.MatToByte(mat.Image);
                    if (bytes == null) continue;
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.StreamSource = new MemoryStream(bytes);
                    bmp.EndInit();
                    mat.BmpImage = bmp;

                }

                lstCards.ItemsSource = mats;
            }));
        }
        private void btnImageLoader_Click(object sender, RoutedEventArgs e)
        {
                var dlg = new OpenFileDialog();
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _localPath = dlg.FileName;
                    _extractor.ProcessImage(CvInvoke.Imread(_localPath, LoadImageType.AnyColor));
                }
        }

        private void lstCards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCards.SelectedIndex == -1) return;
            var matCard = ((TrainingCardViewModel)lstCards.SelectedItem).Image;
            var matrix = new Matrix<int>(matCard.Rows, matCard.Cols);
            matCard.CopyTo(matrix);
            _cards.SelectedCard.TrainingData = matrix;
            ((TrainingCardViewModel)lstCards.SelectedItem).Show = false;
            SetCardInfo();
            
        }

        private void SetCardInfo()
        {
            _cards.SelectNextCard();
            lblCardName.Content = _cards.SelectedCard;

        }

    }
}
