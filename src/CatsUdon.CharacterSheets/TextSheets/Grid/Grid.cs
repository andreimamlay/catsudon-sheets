namespace CatsUdon.CharacterSheets.TextSheets.Grid;

public class Grid(int rows, int columns)
{
    public Cell[,] Cells { get; private set; } = new Cell[rows, columns];
    public bool[] Columns { get; private set; } = new bool[columns];

    public int RowsCount { get; } = rows;
    public int ColumnsCount { get; } = columns;

    public override string ToString()
    {
        return "";
    }

    public void Fill(string[][] data)
    {
        if (data.Length != RowsCount)
        {
            throw new ArgumentException("Invalid number of rows", nameof(data));
        }
        foreach (var (rowIndex, row) in data.Index())
        {
            if (row.Length != ColumnsCount)
            {
                throw new ArgumentException("Invalid number of columns", nameof(data));
            }

            for (var col = 0; col < ColumnsCount; col++)
            {
                ref var cell = ref Cells[rowIndex, col];
                cell.Text = row[col];
                cell.Pushed = false;
                cell.Checked = false;
            }
        }
    }

    public struct Cell
    {
        public string Text { get; set; }
        public bool Pushed { get; set; }
        public bool Checked { get; set; }
    }
}
