using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sea_Battle.model
{ // ячейки
    public enum CellState
    {
        Empty, // Пустая
        Ship, // Корабль
        Hit,  // Подбита
        Miss,  // Промах
        Forbidden // для расстановки кораблей
    }
}
