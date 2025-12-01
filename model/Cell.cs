using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sea_Battle.model
{
    public class Cell : INotifyPropertyChanged // класс клетки с интерфейсом уведомления об изменении свойств
    {
        private CellState _state; // текущее состояние ячейки
        private bool _isPlayerBoard; //является ли доска игрока или компьютера

        public int Row { get; set; } // позиция ячейки
        public int Column { get; set; } // позиция ячейки
        public Ship Ship { get; set; } // является ли ячейка кораблем

        public Mine Mine { get; set; }// является ли ячейка миной


        public bool IsPlayerBoard // ячейка игрока
        {
            get => _isPlayerBoard; //значение приватного поля
            set
            {
                _isPlayerBoard = value; // сохраняет новое значение для поля
                OnPropertyChanged(); // // Уведомляет об IsPlayerBoard
                OnPropertyChanged(nameof(DisplayText)); // Уведомляет об DisplayText
            }
        }

        public CellState State // состояние ячейки
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
            }
        }

        public string DisplayText => State switch // отображение на экране
        {
            CellState.Ship => IsPlayerBoard ? "■" : "", //корабль игрока, если компьютер то не видно
            CellState.Hit => "X", // попадание
            CellState.Miss => "•", // промах
            CellState.Mine => IsPlayerBoard ? "○" : "", // Мина видна только на своем поле 
            CellState.MineHit => "⦿", //попадание по мине видно всем
            _ => "" //пустая клетка
        };

        public event PropertyChangedEventHandler PropertyChanged; // уведомление UI об изменении свойств
        // Защищенный виртуальный метод для уведомления об изменении свойства
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Безопасный вызов события: проверяет наличие подписчиков и уведомляет их
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}