using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Sea_Battle.model
{
    // Объявление класса "Корабль"
   public class Ship
    {
        // Свойство для хранения размера корабля (количество палуб)
        public int Size {get; set; }
        // Список ячеек (клеток на игровом поле), которые занимает этот корабль
        // Инициализируется пустым списком при создании объекта
        public List<Cell> Cells { get; set; } = new List<Cell>();
        // Вычисляемое свойство, проверяющее, потоплен ли корабль
        // Возвращает true, если ВСЕ ячейки корабля имеют статус "Подбит"
        public bool IsSunk => Cells.All(c => c.State == CellState.Hit);

            // Конструктор класса, принимающий размер корабля
            public Ship(int size)
        {
            Size = size;    
        }
    }
}
