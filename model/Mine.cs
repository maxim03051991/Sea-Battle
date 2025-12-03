using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sea_Battle.model
{
    public class Mine : INotifyPropertyChanged
    {
        private bool _isActive = true; // состояние мины
        private Cell _cell; //расположение мины
        private bool _isUsed; //использована ли мина
        private Cell _revealedCell; // Клетка, которую показал противник

        public Cell Cell //Свойство для доступа к клетке
        {
            get => _cell;
            set
            {
                _cell = value;
                OnPropertyChanged();
            }
        }

        public bool IsActive //Свойство для доступа к состоянию активности мины
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public bool IsUsed //Свойство для отслеживания, использована ли мина
        {
            get => _isUsed;
            set
            {
                _isUsed = value;
                OnPropertyChanged();
            }
        }
        //Свойство для клетки, которую показывает противник при срабатывании мины
        public Cell RevealedCell
        {
            get => _revealedCell;
            set
            {
                _revealedCell = value;
                OnPropertyChanged();
            }
        }
        //Объявление события требуемого интерфейсом
        public event PropertyChangedEventHandler PropertyChanged;
        //автоматически подставляет имя свойства из которого был вызван метод
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}