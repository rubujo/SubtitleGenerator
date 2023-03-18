using Whisper;
using Whisper.Internal;

namespace SubtitleGenerator.Commons;

/// <summary>
/// 自定義 CaptureCallbacks
/// </summary>
internal class CustomCaptureCallbacks : CaptureCallbacks
{
    /// <summary>
    /// FMain
    /// </summary>
    private readonly FMain _FMain;

    /// <summary>
    /// CancellationToken
    /// </summary>
    private readonly CancellationToken? _cancellationToken;

    private bool ShouldQuit = false;

    /// <summary>
    /// 自定義 Callbacks
    /// </summary>
    /// <param name="fMain">FMain</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public CustomCaptureCallbacks(FMain fMain, CancellationToken? cancellationToken = default)
    {
        _FMain = fMain;
        _cancellationToken = cancellationToken;
    }

    protected override bool shouldCancel(Context sender) => ShouldQuit;

    protected override void captureStatusChanged(Context sender, eCaptureStatus status)
    {
        try
        {
            if (_cancellationToken?.IsCancellationRequested == true)
            {
                ShouldQuit = true;
            }

            _cancellationToken?.ThrowIfCancellationRequested();

            _FMain.WriteLog($"捕捉狀態：{status}");
        }
        catch (Exception ex)
        {
            NativeLogger.throwForHR(ex.HResult);
        }
    }
}