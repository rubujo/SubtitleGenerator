namespace SubtitleGenerator;

// 阻擋設計工具。
partial class DesignerBlocker { }

partial class FMain
{
    /// <summary>
    /// 抽樣策略類型
    /// </summary>
    private enum SamplingStrategyType
    {
        /// <summary>
        /// 預設
        /// </summary>
        Default,
        /// <summary>
        /// 貪婪
        /// </summary>
        Greedy,
        /// <summary>
        /// 集束搜尋
        /// </summary>
        BeamSearch
    }

    /// <summary>
    /// OpenCC 類型
    /// </summary>
    private enum OpenCCType
    {
        None,
        S2TWP,
        TW2SP
    }

    private CancellationTokenSource GlobalCTS = new();
    private CancellationToken GlobalCT = new();

    private readonly ToolTip GlobalTT = new();
    private readonly string BinsFolderPath = Path.Combine(AppContext.BaseDirectory, "Bins"),
        ModelsFolderPath = Path.Combine(AppContext.BaseDirectory, "Models"),
        TempFolderPath = Path.Combine(AppContext.BaseDirectory, "Temp");

    private bool EnableOpenCC = false;
    private OpenCCType GlobalOCCType = OpenCCType.None;
}