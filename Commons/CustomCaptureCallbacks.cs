using SubtitleGenerator.Commons.Extensions;
using Whisper;
using Whisper.Internal;

namespace SubtitleGenerator.Commons;

/// <summary>
/// 自定義 CaptureCallbacks
/// </summary>
public class CustomCaptureCallbacks : CaptureCallbacks
{
    /// <summary>
    /// FMain
    /// </summary>
    private readonly FMain _FMain;

    /// <summary>
    /// LCaptureStatus
    /// </summary>
    private readonly Label _LCaptureStatus;

    /// <summary>
    /// CancellationToken
    /// </summary>
    private readonly CancellationToken _CancellationToken;

    /// <summary>
    /// 應該取消
    /// </summary>
    private bool ShouldCancel = false;

    /// <summary>
    /// 自定義 Callbacks
    /// </summary>
    /// <param name="form">FMain</param>
    public CustomCaptureCallbacks(FMain form, CancellationToken cancellationToken = default)
    {
        _FMain = form;
        _LCaptureStatus = _FMain.GetLCaptureStatus();
        _CancellationToken = cancellationToken;
    }

    protected override bool shouldCancel(Context sender)
    {
        try
        {
            _CancellationToken.ThrowIfCancellationRequested();

            ShouldCancel = _CancellationToken.IsCancellationRequested;
        }
        catch (Exception ex)
        {
            // 清除控制項。
            UpdateControl();

            NativeLogger.throwForHR(ex.HResult);
        }

        return ShouldCancel;
    }

    protected override void captureStatusChanged(Context sender, eCaptureStatus status)
    {
        UpdateControl($"捕捉狀態：{status}");
    }

    /// <summary>
    /// 更新控制項
    /// </summary>
    /// <param name="text">字串，文字內容，預設值為空白</param>
    private void UpdateControl(string text = "")
    {
        _LCaptureStatus.InvokeIfRequired(() =>
        {
            _LCaptureStatus.Text = text;
        });
    }
}