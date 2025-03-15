namespace CatsUdon.CharacterSheets.TextSheets;

public class Grid
{

    public Cell[,] Cells { get; private set; } = new Cell[RowsCount, ColumnsCount];
    public bool[] Columns { get; private set; } = new bool[ColumnsCount];

    public const int RowsCount = 12;

    public const int ColumnsCount = 6;

    public override string ToString()
    {
        var writer = new TextSheetWriter(CatsudonTextTypes.Text);
        for (int i = 0; i < ColumnsCount; i++)
        {
            var isSelected = Columns[i] || Columns.ElementAtOrDefault(i - 1);

            writer.Append("LineData", i, isSelected);
        }

        var cellIndex = 0;
        for (int c = 0; c < ColumnsCount; c++)
        {
            for (int r = 0; r < RowsCount; r++)
            {
                ref var cell = ref Cells[r, c];
                writer.Append("Cell", cellIndex, cell.Pushed, cell.Checked, cell.Text);
                cellIndex++;
            }
        }

        return writer.ToString();
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

        public Cell()
        {
            Text = string.Empty;
        }
    }
}
