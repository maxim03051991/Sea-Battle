namespace Sea_Battle.model
{
    public enum CellState //перечисление состояний
    {
        Empty,
        Ship,
        Hit,
        Miss,
        Mine,
        MineHit,
        Forbidden,
        RevealedShip, // НОВОЕ: подсвеченная клетка корабля (от мины)
        MineUsed // НОВОЕ: использованная мина с точкой
    }
}