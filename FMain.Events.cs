using Whisper.net;

namespace SubtitleGenerator;

// 阻擋設計工具。
partial class DesignerBlocker { }

partial class FMain
{
    private void OnNewSegment(SegmentData e)
    {
        WriteLog($"{e.Start} --> {e.End} : {e.Text}");
    }
}