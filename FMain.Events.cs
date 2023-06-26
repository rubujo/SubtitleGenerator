using SubtitleGenerator.Commons.Extensions;
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
    /// 編碼器開始
    /// </summary>
    /// <param name="encoderBeginData">EncoderBeginData</param>
    public bool OnEncoderBegin(EncoderBeginData encoderBeginData)
    {
        _ = encoderBeginData;

        if (GlobalCTS.IsCancellationRequested)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// 進度
    /// </summary>
    /// <param name="porgress">數值，進度</param>
    public void OnProgress(int porgress)
    {
        PBProgress.InvokeIfRequired(() =>
        {
            PBProgress.Value = porgress;
        });
    }

    /// <summary>
    /// 新段
    /// </summary>
    /// <param name="segmentData">SegmentData</param>
    public void OnNewSegment(SegmentData segmentData)
    {
        string segment = $"{segmentData.Start} --> {segmentData.End}：" +
                $"[ {segmentData.Language} ({segmentData.Probability:P}) ] " +
                $"{segmentData.Text}";

        WriteLog(this, segment);
    }
}