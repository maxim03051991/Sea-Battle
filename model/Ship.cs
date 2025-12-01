namespace Sea_Battle.model
{
    public class Ship //класс Ship, представляющий корабль
    {
        public int SizeShip { get; set; } //хранит размер корабля
        public List<Cell> Cells { get; set; } = new List<Cell>(); //список ячеек, которые занимает корабль на игровом поле
        public bool IsSunk => Cells.All(c => c.State == CellState.Hit); // возвращающее true, если корабль потоплен

        public Ship(int sizeShip) //Конструктор класса
        {
            SizeShip = sizeShip;
        }
    }
}