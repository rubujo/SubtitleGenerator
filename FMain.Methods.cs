using OpenCCNET;
using SubtitleGenerator.Commons.Extensions;
using SubtitleGenerator.Commons.Sets;
using static SubtitleGenerator.Commons.Sets.EnumSet;
using SubtitleGenerator.Commons.Utils;
using System.Diagnostics;
using System.Reflection;

namespace SubtitleGenerator;

// 阻擋設計工具。
partial class DesignerBlocker { }

/// <summary>
/// FMain 的方法
/// </summary>
partial class FMain
{
    /// <summary>
    /// 自定義初始化
    /// </summary>
    private async void CustomInit()
    {
        // 設定顯示應用程式的版本號。
        LVersion.InvokeIfRequired(() =>
        {
            LVersion.Text = $"版本：v{Assembly.GetExecutingAssembly().GetName().Version?.ToString()}";
        });

        // 設定控制項的工具提示。
        GlobalTT.SetToolTip(CBModelImplementation, "模型實作");
        GlobalTT.SetToolTip(CBGPUs, "GPU 裝置");
        GlobalTT.SetToolTip(CBGpuModelFlags, "GPU 模型旗標");
        GlobalTT.SetToolTip(CBModels, "模型");
        GlobalTT.SetToolTip(CBSamplingStrategies, "抽樣策略");
        GlobalTT.SetToolTip(CBLanguages, "語言");
        GlobalTT.SetToolTip(CBCaptureDevices, "錄音裝置");
        GlobalTT.SetToolTip(CBEnableTranslate, "將轉譯的內容翻譯成英文");
        GlobalTT.SetToolTip(CBConvertToWav, "先透過 FFmpeg 將選擇的檔案轉換成 WAV 格式的暫時檔案後，在進行轉譯");
        GlobalTT.SetToolTip(CBEnableOpenCCS2TWP, "使用 OpenCC 將轉譯的內容，從「簡體中文」轉換成「繁體中文（臺灣）」");
        GlobalTT.SetToolTip(CBEnableOpenCCTW2SP, "使用 OpenCC 將轉譯的內容，從「繁體中文（臺灣）」轉換成「簡體中文」");

        // 設定控制項。
        CBModels.Text = "Medium";
        CBSamplingStrategies.Text = "Default";
        CBLanguages.Text = "en";
        CBModelImplementation.Text = "GPU";

        CBGPUs.DataSource = WhisperUtil.GetGpuList();

        if (CBGPUs.Items.Count > 0)
        {
            CBGPUs.SelectedIndex = 0;
        }

        CBGpuModelFlags.Text = "Wave32";

        CBCaptureDevices.DataSource = WhisperUtil.GetCaptureDeviceList();

        if (CBCaptureDevices.Items.Count > 0)
        {
            CBCaptureDevices.SelectedIndex = 0;
        }

        BtnCancel.Enabled = false;
        LCaptureStatus.Text = string.Empty;

        // 檢查資料夾。
        CheckFolders();

        // 檢查 FFmpeg。
        await FFmpegUtil.CheckFFmpeg(this);

        // 設定 Cnst-me/Whisper 的 LogSink。
        WhisperUtil.SetLogSink(this);

        // 初始化 OpenCC。
        ZhConverter.Initialize();
    }

    /// <summary>
    /// 檢查資料夾
    /// </summary>
    public void CheckFolders()
    {
        if (!Directory.Exists(FolderSet.BinsFolderPath))
        {
            Directory.CreateDirectory(FolderSet.BinsFolderPath);

            WriteLog(this, $"已建立資料夾：{FolderSet.BinsFolderPath}");
        }
        else
        {
            WriteLog(this, $"已找到資料夾：{FolderSet.BinsFolderPath}");
        }

        if (!Directory.Exists(FolderSet.ModelsFolderPath))
        {
            Directory.CreateDirectory(FolderSet.ModelsFolderPath);

            WriteLog(this, $"已建立資料夾：{FolderSet.ModelsFolderPath}");
        }
        else
        {
            WriteLog(this, $"已找到資料夾：{FolderSet.ModelsFolderPath}");
        }

        if (!Directory.Exists(FolderSet.TempFolderPath))
        {
            Directory.CreateDirectory(FolderSet.TempFolderPath);

            WriteLog(this, $"已建立資料夾：{FolderSet.TempFolderPath}");
        }
        else
        {
            WriteLog(this, $"已找到資料夾：{FolderSet.TempFolderPath}");
        }
    }

    /// <summary>
    /// 設定 OpenCC 相關的變數
    /// </summary>
    public void SetOpenCCVariables()
    {
        EnableOpenCC = CBEnableOpenCCS2TWP.Checked | CBEnableOpenCCTW2SP.Checked;

        if (EnableOpenCC)
        {
            if (CBEnableOpenCCS2TWP.Checked)
            {
                GlobalOCCMode = OpenCCMode.S2TWP;
            }
            else if (CBEnableOpenCCTW2SP.Checked)
            {
                GlobalOCCMode = OpenCCMode.TW2SP;
            }
            else
            {
                GlobalOCCMode = OpenCCMode.None;
            }
        }
    }

    /// <summary>
    /// 顯示錯誤訊息
    /// </summary>
    public static readonly Action<Form, string> ShowMsg =
        new((Form form, string message) =>
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            MessageBox.Show(
                message,
                form.Text,
                MessageBoxButtons.OK);
        });

    /// <summary>
    /// 顯示錯誤訊息
    /// </summary>
    public static readonly Action<Form, string> ShowErrMsg =
        new((Form form, string message) =>
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            MessageBox.Show(
                message,
                form.Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        });

    /// <summary>
    /// 顯示警告訊息
    /// </summary>
    public static readonly Action<Form, string> ShowWarnMsg =
        new((Form form, string message) =>
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            MessageBox.Show(
                message,
                form.Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        });

    /// <summary>
    /// 開啟資料夾
    /// </summary>
    /// <param name="form">FMain</param>
    /// <param name="path">字串，路徑</param>
    public static void OpenFolder(FMain form, string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ShowErrMsg(form, ex.ToString());
        }
    }

    /// <summary>
    /// 取得 TBLog
    /// </summary>
    /// <returns>TextBox</returns>
    public TextBox GetTBLog()
    {
        return TBLog;
    }

    /// <summary>
    /// 取得 LCaptureStatus
    /// </summary>
    /// <returns>Label</returns>
    public Label GetLCaptureStatus()
    {
        return LCaptureStatus;
    }

    /// <summary>
    /// 寫紀錄
    /// </summary>
    /// <param name="form">FMain</param>
    /// <param name="message">字串，訊息內容</param>
    public static void WriteLog(FMain form, string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        try
        {
            TextBox textBox = form.GetTBLog();

            textBox.InvokeIfRequired(() =>
            {
                textBox.Text += $"[{DateTime.Now:yyyy/MM/dd HH:mm:ss}] " +
                    $"{message}{Environment.NewLine}";
                textBox.SelectionStart = textBox.TextLength;
                textBox.ScrollToCaret();
            });
        }
        catch (Exception ex)
        {
            ShowErrMsg(form, ex.ToString());
        }
    }
}