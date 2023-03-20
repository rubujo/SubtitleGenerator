using Whisper;
using static SubtitleGenerator.Commons.Sets.EnumSet;

namespace SubtitleGenerator;

// 阻擋設計工具。
partial class DesignerBlocker { }

/// <summary>
/// FMain 的變數
/// </summary>
partial class FMain
{
    /// <summary>
    /// 全域 ToolTip
    /// </summary>
    private readonly ToolTip GlobalTT = new();

    /// <summary>
    /// 全域 CancellationTokenSource
    /// </summary>
    private CancellationTokenSource GlobalCTS = new();

    /// <summary>
    /// 全域 CancellationToken
    /// </summary>
    private CancellationToken GlobalCT = new();

    /// <summary>
    /// 是否啟用 OpenCC
    /// </summary>
    public bool EnableOpenCC = false;

    /// <summary>
    /// OpenCC 的模式
    /// </summary>
    public OpenCCMode GlobalOCCMode = OpenCCMode.None;

    /// <summary>
    /// 段列表
    /// </summary>
    public List<sSegment> SegmentDataSet = new();
}