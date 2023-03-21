using SubtitleGenerator.Commons.Utils;
using Whisper;
using Whisper.Internal;

namespace SubtitleGenerator.Commons;

/// <summary>
/// 自定義 Callbacks
/// </summary>
public class CustomCallbacks : Callbacks
{
    /// <summary>
    /// FMain
    /// </summary>
    private readonly FMain _FMain;

    /// <summary>
    /// CancellationToken
    /// </summary>
    private readonly CancellationToken _CancellationToken;

    /// <summary>
    /// 自定義 Callbacks
    /// </summary>
    /// <param name="form">FMain</param>
    public CustomCallbacks(FMain form, CancellationToken cancellationToken = default)
    {
        _FMain = form;
        _CancellationToken = cancellationToken;
    }

    protected override void onNewSegment(Context sender, int countNew)
    {
        try
        {
            _CancellationToken.ThrowIfCancellationRequested();

            TranscribeResult transcribeResult = sender.results(eResultFlags.Timestamps);

            int s0 = transcribeResult.segments.Length - countNew;

            if (s0 == 0)
            {
                FMain.WriteLog(_FMain, string.Empty);
            }

            for (int i = s0; i < transcribeResult.segments.Length; i++)
            {
                sSegment segenmt = transcribeResult.segments[i];

                string speaker = sender.detectSpeaker(segenmt.time) switch
                {
                    eSpeakerChannel.Unsure => "（未知聲道）",
                    eSpeakerChannel.Left => "（左聲道）",
                    eSpeakerChannel.Right => "（右聲道）",
                    eSpeakerChannel.NoStereoData => "（單聲道）",
                    _ => ""
                };

                string text = $"[{WhisperUtil.PrintTime(segenmt.time.begin)} --> " +
                    $"{WhisperUtil.PrintTime(segenmt.time.end)}]｜" +
                    $"{speaker}：{segenmt.text}";

                FMain.WriteLog(_FMain, text);

                _FMain.SegmentDataSet.Add(segenmt);
            }
        }
        catch (Exception ex)
        {
            NativeLogger.throwForHR(ex.HResult);
        }
    }
}