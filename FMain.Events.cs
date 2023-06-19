using Whisper.net;

namespace SubtitleGenerator;

// 阻擋設計工具。
partial class DesignerBlocker { }

/// <summary>
/// FMain 的事件
/// </summary>
partial class FMain
{
    /// <summary>
    /// 進度
    /// </summary>
    /// <param name="porgress">數值，進度</param>
    public void OnProgress(int porgress)
    {
        WriteLog(this, $"進度：{porgress}%");
    }

    /// <summary>
    /// 新段
    /// </summary>
    /// <param name="segmentData">SegmentData</param>
    public void OnNewSegment(SegmentData segmentData)
    {
        WriteLog(this, $"{segmentData.Start} --> {segmentData.End} : ({segmentData.Language}) {segmentData.Text}");
    }
}