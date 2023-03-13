namespace SubtitleGenerator.Commons.Extensions;

/// <summary>
/// Control 的擴充方法
/// </summary>
internal static class ControlExtension
{
    /// <summary>
    /// 非同步委派更新 UI
    /// <para>來源：https://dotblogs.com.tw/shinli/2015/04/16/151076 </para>
    /// </summary>
    /// <param name="control">Control</param>
    /// <param name="action">MethodInvoker</param>
    public static void InvokeIfRequired(this Control control, MethodInvoker action)
    {
        // 在非當前執行緒內，使用委派。
        if (control.InvokeRequired)
        {
            control.Invoke(action);
        }
        else
        {
            action();
        }
    }

    /// <summary>
    /// 啟用控制項
    /// </summary>
    /// <param name="controls">Control</param>
    /// <param name="isEnabled">布林值，是否啟用，預設值為 true</param>
    public static void SetEnabled(this Control[] controls, bool isEnabled = true)
    {
        foreach (Control control in controls)
        {
            control.InvokeIfRequired(() =>
            {
                control.Enabled = isEnabled;
            });
        }
    }
}