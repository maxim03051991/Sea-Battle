using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sea_Battle.model
{
    public class Mine : INotifyPropertyChanged
    {
        private bool _isActive = true;
        private Cell _cell;
        private bool _isUsed;
        private Cell _revealedCell; // Клетка, которую показал противник

        public Cell Cell
        {
            get => _cell;
            set
            {
                _cell = value;
                OnPropertyChanged();
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public bool IsUsed
        {
            get => _isUsed;
            set
            {
                _isUsed = value;
                OnPropertyChanged();
            }
        }

        public Cell RevealedCell
        {
            get => _revealedCell;
            set
            {
                _revealedCell = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}