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
    /// 在新段
    /// </summary>
    /// <param name="segmentData">SegmentData</param>
    private void OnNewSegment(SegmentData segmentData)
    {
        WriteLog($"{segmentData.Start} --> {segmentData.End} : {segmentData.Text}");
    }
}