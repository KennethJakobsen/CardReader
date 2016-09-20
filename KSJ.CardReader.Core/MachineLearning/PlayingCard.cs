using Emgu.CV;

namespace KSJ.CardReader.Core.MachineLearning
{
    public class PlayingCard
    {
        public enum CardColor
        {
            Spades,
            Clubs,
            Hearts,
            Diamonds,
            Other
        }

        public CardColor Color { get; set; }
        public int Number { get; set; }
        public string Label { get; set; }
        public Matrix<int> TrainingData { get; set; }
        public override string ToString()
        {
            return Color != CardColor.Other ? $"{Label} of {Color}" : Label;
        }
    }
}
