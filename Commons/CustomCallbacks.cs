using Whisper;
using Whisper.Internal;

namespace SubtitleGenerator.Commons;

/// <summary>
/// 自定義 Callbacks
/// </summary>
internal class CustomCallbacks : Callbacks
{
    /// <summary>
    /// FMain
    /// </summary>
    private readonly FMain _FMain;

    /// <summary>
    /// CancellationToken
    /// </summary>
    private readonly CancellationToken? _cancellationToken;

    /// <summary>
    /// 自定義 Callbacks
    /// </summary>
    /// <param name="fMain">FMain</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public CustomCallbacks(FMain fMain, CancellationToken? cancellationToken = default)
    {
        _FMain = fMain;
        _cancellationToken = cancellationToken;
    }

    protected override void onNewSegment(Context sender, int countNew)
    {
        try
        {
            _cancellationToken?.ThrowIfCancellationRequested();

            TranscribeResult transcribeResult = sender.results(eResultFlags.Timestamps);

            int s0 = transcribeResult.segments.Length - countNew;

            if (s0 == 0)
            {
                _FMain.WriteLog("轉譯的內容：");
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

                string text = $"[{FMain.PrintTime(segenmt.time.begin)} --> " +
                    $"{FMain.PrintTime(segenmt.time.end)}]｜" +
                    $"{speaker}：{segenmt.text}";

                _FMain.WriteLog(text);
            }
        }
        catch (Exception ex)
        {
            NativeLogger.throwForHR(ex.HResult);
        }
    }
}