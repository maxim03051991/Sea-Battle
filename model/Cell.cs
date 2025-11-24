using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sea_Battle.model
{
    // Класс Cell (ячейка игрового поля) реализует интерфейс INotifyPropertyChanged
    // Это позволяет уведомлять UI об изменениях свойств
    public class Cell : INotifyPropertyChanged
    {
        private CellState _state;

        public int Row { get; set; }
        public int Column { get; set; }
        public Ship Ship { get; set; }

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
        // Свойство для отображения символов в ячейках
        public string DisplayText
        {
            get
            {
                return State switch
                {
                    CellState.Ship => "■",    // Корабль (только на своем поле)
                    CellState.Hit => "X",     // Попадание
                    CellState.Miss => "•",    // Промах
                    _ => ""                   // Пустота
                };
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
