using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sea_Battle.model
{
    public class Cell : INotifyPropertyChanged
    {
        private CellState _state;
        private bool _isPlayerBoard;

        public int Row { get; set; }
        public int Column { get; set; }
        public Ship Ship { get; set; }

        public bool IsPlayerBoard
        {
            get => _isPlayerBoard;
            set
            {
                _isPlayerBoard = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
            }
        }

        public CellState State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
            }
        }

        public string DisplayText => State switch
        {
            CellState.Ship => IsPlayerBoard ? "■" : "",
            CellState.Hit => "X",
            CellState.Miss => "•",
            _ => ""
        };

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}