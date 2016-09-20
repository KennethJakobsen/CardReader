using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Emgu.CV;
using EmguCvTests.Annotations;

namespace EmguCvTests.ViewModel
{

    public class TrainingCardViewModel : INotifyPropertyChanged
    {
        private bool show = false;
        public TrainingCardViewModel(Mat image)
        {
            Image = image;
            Show = true;
        }

        public bool Show
        {
            get { return show; }
            set
            {
                if (show == value)
                    return;
                show = value;
                NotifyPropertyChanged();
            }
        }

        public Mat Image { get; set; }
        public BitmapImage BmpImage { set; get; }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
