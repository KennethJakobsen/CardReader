using System.Collections;
using System.Collections.Generic;

namespace KSJ.CardReader.Core.MachineLearning
{
    public class PlayingCardCollection : IEnumerable<PlayingCard>
    {
        private readonly List<PlayingCard> _cards;
        private PlayingCard _selectedCard = null;

        public PlayingCard SelectedCard
        {
            get { return _selectedCard; }
            set
            {
                _selectedCard = value;
                if (_selectedCard == null)
                    _selectedIndex = -1;
                else
                    _selectedIndex = _cards.FindIndex(c => c == value);
            }
                
        }
        private int _selectedIndex = -1;
        public  PlayingCardCollection(int numberOfCards)
        {
            _cards = new List<PlayingCard>(numberOfCards);
            CreateCards(numberOfCards);
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                SelectedCard = value == -1 ? null : _cards[value];
            }
        }

        public PlayingCard this[int index] => _cards[index];

        private void CreateCards(int numberOfcards)
        {
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Diamonds, Label = $"A", Number = 1 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Diamonds, Label = $"2", Number = 2 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Diamonds, Label = $"3", Number = 3 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Diamonds, Label = $"4", Number = 4 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Diamonds, Label = $"5", Number = 5 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Diamonds, Label = $"6", Number = 6 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Diamonds, Label = $"7", Number = 7 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Diamonds, Label = $"8", Number = 8 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Diamonds, Label = $"9", Number = 9 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Diamonds, Label = $"10", Number = 10 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Diamonds, Label = $"J", Number = 11 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Diamonds, Label = $"Q", Number = 12 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Diamonds, Label = $"K", Number = 13 });


            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Hearts, Label = $"A", Number = 14 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Hearts, Label = $"2", Number = 15 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Hearts, Label = $"3", Number = 16 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Hearts, Label = $"4", Number = 17 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Hearts, Label = $"5", Number = 18 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Hearts, Label = $"6", Number = 19 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Hearts, Label = $"7", Number = 20 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Hearts, Label = $"8", Number = 21 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Hearts, Label = $"9", Number = 22 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Hearts, Label = $"10",Number = 23 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Hearts, Label = $"J", Number = 24 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Hearts, Label = $"Q", Number = 25 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Hearts, Label = $"K", Number = 26 });


            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Spades, Label = $"A", Number = 27 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Spades, Label = $"2", Number = 28 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Spades, Label = $"3", Number = 29 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Spades, Label = $"4", Number = 30 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Spades, Label = $"5", Number = 31 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Spades, Label = $"6", Number = 32 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Spades, Label = $"7", Number = 33 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Spades, Label = $"8", Number = 34 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Spades, Label = $"9", Number = 35 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Spades, Label = $"10",Number = 36 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Spades, Label = $"J", Number = 37 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Spades, Label = $"Q", Number = 38 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Spades, Label = $"K", Number = 39 });


            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Clubs, Label = $"A", Number = 40 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Clubs, Label = $"2", Number = 41 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Clubs, Label = $"3", Number = 42 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Clubs, Label = $"4", Number = 43 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Clubs, Label = $"5", Number = 44 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Clubs, Label = $"6", Number = 45 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Clubs, Label = $"7", Number = 46 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Clubs, Label = $"8", Number = 47 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Clubs, Label = $"9", Number = 48 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Clubs, Label = $"10",Number = 49 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Clubs, Label = $"J", Number = 50 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Clubs, Label = $"Q", Number = 51 });
            _cards.Add(new PlayingCard() { Color = PlayingCard.CardColor.Clubs, Label = $"K", Number = 52 });

            if (numberOfcards <= 52) return;
            for(var i = 53; i <= numberOfcards; i++ )
                _cards.Add(new PlayingCard() {Color = PlayingCard.CardColor.Other, Label = $"Joker", Number = i});
            SelectedIndex = 0;
        }

        public void SelectNextCard()
        {
            _selectedIndex++;
            if(_selectedIndex < _cards.Count)
                _selectedCard = _cards[_selectedIndex];
            else
                SelectedIndex = -1;

        }

        public IEnumerator<PlayingCard> GetEnumerator()
        {
            return _cards.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
