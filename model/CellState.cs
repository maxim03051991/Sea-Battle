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
        RevealedShip, //  подсвеченная клетка корабля (от мины)
        MineUsed // использованная мина с точкой
    }
}