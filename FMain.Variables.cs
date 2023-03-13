namespace SubtitleGenerator;

// 阻擋設計工具。
partial class DesignerBlocker { }

partial class FMain
{
    private CancellationTokenSource cancellationTokenSource = new();
    private CancellationToken cancellationToken = new();

    private readonly string BinsFolderPath = Path.Combine(AppContext.BaseDirectory, "Bins"),
        ModelsFolderPath = Path.Combine(AppContext.BaseDirectory, "Models"),
        TempFolderPath = Path.Combine(AppContext.BaseDirectory, "Temp");
}