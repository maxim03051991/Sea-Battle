using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sea_Battle.model
{ // таблица выбора кораблей
    public class ShipTemplate : INotifyPropertyChanged //коkичство кораблей для выбора 
    {
        private int _count; // Приватное поле для хранения количества кораблей

        public int SizeShip { get; set; } // Автосвойство для размера корабля (количество клеток)
        public int Count
        {
            get => _count; // Возвращает значение из приватного поля
            set
            {
                _count = value; // Устанавливает новое значение
                OnPropertyChanged(); // Уведомляет об изменении Count
                OnPropertyChanged(nameof(DisplayName));  // Обновляет зависимое свойство
            }
        }
        public string Name { get; set; }  // Название типа корабля
        // Форматированное отображаемое имя с актуальным количеством
        public string DisplayName => $"{Name} ({Count} осталось)";
        // Конструктор класса
        public ShipTemplate(int size, int count, string name)
        {
            SizeShip = size; // Инициализация размера
            _count = count; // Инициализация количества
            Name = name; // Инициализация названия
        }
        // Событие для уведомления об изменениях свойств
        public event PropertyChangedEventHandler PropertyChanged;
        // Метод для вызова события PropertyChanged
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
   
    //перечисление изогнутых кораблей
    public enum CurvedShipType
    {
        Square,     // Квадрат 2x2
        LShape,     // Г-образная
        TShape,     // Т-образная
        ZShape      // Z-образная
    }
}