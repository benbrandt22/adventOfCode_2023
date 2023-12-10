namespace Core.Shared;

public class LinkedGrid<TValue>
{
    public LinkedGrid(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        GridArray = new LinkedGridCell<TValue>[rows, columns];
        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                GridArray[row, column] = new LinkedGridCell<TValue>(this, row, column);
            }
        }
    }

    public int Rows { get; }
    public int Columns { get; }
    private LinkedGridCell<TValue>[,] GridArray { get; }

    public LinkedGridCell<TValue> this[int row, int column] => GridArray[row, column];

    public IEnumerable<LinkedGridCell<TValue>> AllCells
    {
        get
        {
            for (var row = 0; row < Rows; row++)
            {
                for (var column = 0; column < Columns; column++)
                {
                    yield return GridArray[row, column];
                }
            }
        }
    }
}

public class LinkedGridCell<TValue>
{
    private readonly LinkedGrid<TValue> _parentGrid;

    public LinkedGridCell(LinkedGrid<TValue> parentGrid, int row, int column)
    {
        _parentGrid = parentGrid;
        Row = row;
        Column = column;
    }

    public int Row { get; }
    public int Column { get; }
    
    public TValue? Value { get; set; }

    public LinkedGridCell<TValue>? GetNeighbor(LinkedGridDirection direction)
    {
        return direction switch
        {
            LinkedGridDirection.Up => Row > 0 ? _parentGrid[Row - 1, Column] : null,
            LinkedGridDirection.Down => Row < _parentGrid.Rows - 1 ? _parentGrid[Row + 1, Column] : null,
            LinkedGridDirection.Left => Column > 0 ? _parentGrid[Row, Column - 1] : null,
            LinkedGridDirection.Right => Column < _parentGrid.Columns - 1 ? _parentGrid[Row, Column + 1] : null,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
    
    public LinkedGridCell<TValue>? NeighborUp => GetNeighbor(LinkedGridDirection.Up);
    public LinkedGridCell<TValue>? NeighborDown => GetNeighbor(LinkedGridDirection.Down);
    public LinkedGridCell<TValue>? NeighborLeft => GetNeighbor(LinkedGridDirection.Left);
    public LinkedGridCell<TValue>? NeighborRight => GetNeighbor(LinkedGridDirection.Right);
    
}

public enum LinkedGridDirection
{
    Up,
    Down,
    Left,
    Right
}