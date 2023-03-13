using System.Diagnostics;
using System.Globalization;
using Whisper;
using Whisper.net.Ggml;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using Xabe.FFmpeg.Events;

namespace SubtitleGenerator;

public partial class FMain : Form
{
    private CancellationTokenSource cancellationTokenSource = new();
    private CancellationToken cancellationToken = new();

    public FMain()
    {
        InitializeComponent();
    }

    private void FMain_Load(object sender, EventArgs e)
    {
        try
        {
            CustomInit();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void BtnSelectInputFile_Click(object sender, EventArgs e)
    {
        try
        {
            OpenFileDialog openFileDialog = new();

            DialogResult dialogResult = openFileDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                TBInputFilePath.Text = openFileDialog.FileName;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async void BtnStart_Click(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(TBInputFilePath.Text))
            {
                MessageBox.Show(
                    "請先選擇檔案。",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            TBInputFilePath.Enabled = false;
            BtnSelectInputFile.Enabled = false;
            BtnStart.Enabled = false;
            CBLanguage.Enabled = false;
            BtnCancel.Enabled = true;

            eLanguage language = eLanguage.English;

            language = CBLanguage.Text switch
            {
                "中" => eLanguage.Chinese,
                "英" => eLanguage.English,
                "日" => eLanguage.Japanese,
                "韓" => eLanguage.Korean,
                _ => eLanguage.English
            };

            cancellationTokenSource = new();
            cancellationToken = cancellationTokenSource.Token;

            await DoTaskWithGPU(
                TBInputFilePath.Text,
                language,
                false,
                GgmlType.Medium,
                cancellationToken);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            TBInputFilePath.Enabled = true;
            BtnSelectInputFile.Enabled = true;
            BtnStart.Enabled = true;
            CBLanguage.Enabled = true;
            BtnCancel.Enabled = false;
        }
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
        try
        {
            cancellationTokenSource.Cancel();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 自定義初始化
    /// </summary>
    private async void CustomInit()
    {
        string binPath = Path.Combine(AppContext.BaseDirectory, "Bins"),
            modelPath = Path.Combine(AppContext.BaseDirectory, "Models"),
            tempWavPath = Path.Combine(AppContext.BaseDirectory, "Temp");

        #region 檢查資料夾

        if (!Directory.Exists(binPath))
        {
            Directory.CreateDirectory(binPath);

            WriteLog($"已建立資料夾：{binPath}");
        }
        else
        {
            WriteLog($"已找到資料夾：{binPath}");
        }

        if (!Directory.Exists(modelPath))
        {
            Directory.CreateDirectory(modelPath);

            WriteLog($"已建立資料夾：{modelPath}");
        }
        else
        {
            WriteLog($"已找到資料夾：{modelPath}");
        }

        if (!Directory.Exists(tempWavPath))
        {
            Directory.CreateDirectory(tempWavPath);

            WriteLog($"已建立資料夾：{tempWavPath}");
        }
        else
        {
            WriteLog($"已找到資料夾：{tempWavPath}");
        }

        #endregion

        #region 檢查 FFmpeg

        FFmpeg.SetExecutablesPath(binPath);

        string ffpmegExePath = Path.Combine(binPath, "ffmpeg.exe"),
            ffprobeExePath = Path.Combine(binPath, "ffprobe.exe");

        if (!File.Exists(ffpmegExePath) || !File.Exists(ffprobeExePath))
        {
            WriteLog("FFmpeg 執行檔不存在，正在開始下載 FFmpeg 執行檔……");

            await FFmpegDownloader.GetLatestVersion(
                FFmpegVersion.Official,
                binPath);

            WriteLog("已下載 FFmpeg 執行檔。");
        }
        else
        {
            WriteLog("已找到 FFmpeg 執行檔。");
        }

        #endregion

        // 預設選擇第一個語言。
        CBLanguage.SelectedIndex = 0;

        // 預設停用。
        BtnCancel.Enabled = false;
    }

    /// <summary>
    /// 檢查模型
    /// </summary>
    /// <param name="ggmlType">GgmlType，預設值為 GgmlType.Medium</param>
    /// <returns>字串，模型檔案的路徑</returns>
    private async Task<string> CheckModel(GgmlType ggmlType = GgmlType.Medium)
    {
        string modelPath = Path.Combine(AppContext.BaseDirectory, "Models"),
            modelFilePath = Path.Combine(modelPath, $"ggml-{ggmlType.ToString().ToLower()}.bin"),
            modelFileName = Path.GetFileName(modelFilePath);

        if (!File.Exists(modelFilePath))
        {
            WriteLog($"模型檔案 {modelFileName} 不存在，正在開始下載模型檔案……");

            using Stream stream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
            using FileStream fileStream = File.OpenWrite(modelFilePath);

            await stream.CopyToAsync(fileStream);

            WriteLog($"已下載模型檔案 {modelFileName}。");
        }
        else
        {
            WriteLog($"已找到模型檔案 {modelFileName}。");
        }

        return modelFilePath;
    }

    /// <summary>
    /// 轉換成 WAV 檔
    /// </summary>
    /// <param name="path">字串，檔案的路徑</param>
    private async Task<string> ConvertToWav(string path)
    {
        string fileName = Path.GetFileNameWithoutExtension(path);

        IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(path);

        IEnumerable<IAudioStream> audioStreams = mediaInfo.AudioStreams;

        if (audioStreams == null)
        {
            WriteLog("發生錯誤：請選擇有效的檔案。");
        }

        string tempPath = Path.Combine(AppContext.BaseDirectory, "Temp"),
            filePath = Path.Combine(tempPath, $"{fileName}_{DateTime.Now:yyyyMMddHHmmssfff}.wav");

        IConversion conversion = FFmpeg.Conversions.New()
            .AddStream(audioStreams)
            .AddParameter("-ar 16000")
            .SetOutputFormat(Format.wav)
            .SetOutput(filePath)
            .SetOverwriteOutput(true);

        string parameter = conversion.Build();

        WriteLog($"使用的參數：{parameter}");

        conversion.OnDataReceived += (object sender, DataReceivedEventArgs e) =>
        {
            WriteLog(e.Data?.ToString() ?? string.Empty);
        };
        conversion.OnProgress += (object sender, ConversionProgressEventArgs args) =>
        {
            WriteLog(args?.ToString() ?? string.Empty);
        };

        IConversionResult conversionResult = await conversion.Start();

        string result = $"FFmpeg 執行結果：{Environment.NewLine}" +
            $"開始時間：{conversionResult.StartTime}{Environment.NewLine}" +
            $"結束時間：{conversionResult.EndTime}{Environment.NewLine}" +
            $"耗時：{conversionResult.Duration}{Environment.NewLine}" +
            $"參數：{conversionResult.Arguments}";

        WriteLog(result);

        return filePath;
    }

    /// <summary>
    /// 使用 GPU 執行任務
    /// </summary>
    /// <param name="path">字串，檔案的路徑</param>
    /// <param name="language">eLanguage，預設值為 eLanguage.English</param>
    /// <param name="translate">布林直，是否翻譯成英文，預設值為 false</param>
    /// <param name="ggmlType">GgmlType，預設值為 GgmlType.Medium</param>
    /// <returns>Task</returns>
    private async Task DoTaskWithGPU(
        string path,
        eLanguage language = eLanguage.English,
        bool translate = false,
        GgmlType ggmlType = GgmlType.Medium,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Run(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                string wavfilePath = path,
                    modelFilePath = await CheckModel(ggmlType);

                WriteLog("正在開始作業……");
                WriteLog($"使用的語言：{language}");

                using iModel model = await Library.loadModelAsync(
                    path: modelFilePath,
                    cancelToken: cancellationToken,
                    flags: eGpuModelFlags.Wave64 | eGpuModelFlags.UseReshapedMatMul,
                    impl: eModelImplementation.GPU);
                using Context context = model.createContext();

                context.parameters.setFlag(eFullParamsFlags.PrintRealtime, false);
                context.parameters.setFlag(eFullParamsFlags.PrintProgress, true);
                context.parameters.setFlag(eFullParamsFlags.PrintTimestamps, true);
                context.parameters.setFlag(eFullParamsFlags.PrintSpecial, false);
                context.parameters.setFlag(eFullParamsFlags.Translate, translate);
                context.parameters.language = language;
                context.parameters.cpuThreads = Environment.ProcessorCount;

                using iMediaFoundation mediaFoundation = Library.initMediaFoundation();
                using iAudioBuffer audioBuffer = mediaFoundation.loadAudioFile(wavfilePath, true);

                cancellationToken.ThrowIfCancellationRequested();

                context.runFull(audioBuffer);

                cancellationToken.ThrowIfCancellationRequested();

                WriteLog("正在建立字幕檔案……");

                CreateSubRip(context, path);
                CreateWebVTT(context, path);
            }, cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            MessageBox.Show(
                ex.Message,
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            WriteLog("已取消作業。");
        }
    }

    /// <summary>
    /// 建立 SubRip Text 字幕檔
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="path">字串，檔案的路徑</param>
    private void CreateSubRip(Context context, string path)
    {
        string filePath = Path.ChangeExtension(path, ".srt");

        using StreamWriter streamWriter = File.CreateText(filePath);

        ReadOnlySpan<sSegment> segments = context.results(eResultFlags.Timestamps).segments;

        for (int i = 0; i < segments.Length; i++)
        {
            streamWriter.WriteLine(i + 1);

            sSegment segment = segments[i];

            string begin = PrintTimeWithComma(segment.time.begin),
                end = PrintTimeWithComma(segment.time.end);

            streamWriter.WriteLine("{0} --> {1}", begin, end);
            streamWriter.WriteLine(segment.text);
            streamWriter.WriteLine();
        }

        WriteLog($"已建立 SubRip Text 字幕檔：{filePath}");
    }

    /// <summary>
    /// 建立 WebVTT 字幕檔
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="path">字串，檔案的路徑</param>
    private void CreateWebVTT(Context context, string path)
    {
        string filePath = Path.ChangeExtension(path, ".vtt");

        using StreamWriter streamWriter = File.CreateText(filePath);

        ReadOnlySpan<sSegment> segments = context.results(eResultFlags.Timestamps).segments;

        streamWriter.WriteLine("WEBVTT ");
        streamWriter.WriteLine();

        for (int i = 0; i < segments.Length; i++)
        {
            streamWriter.WriteLine(i + 1);

            sSegment segment = segments[i];

            string begin = PrintTime(segment.time.begin),
                end = PrintTime(segment.time.end);

            streamWriter.WriteLine("{0} --> {1}", begin, end);
            streamWriter.WriteLine(segment.text);
            streamWriter.WriteLine();
        }

        WriteLog($"已建立 WebVTT 字幕檔：{filePath}");
    }

    /// <summary>
    /// 列印時間
    /// </summary>
    /// <param name="timeSpan">TimeSpan</param>
    /// <returns>字串</returns>
    private static string PrintTime(TimeSpan timeSpan) =>
        timeSpan.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture);

    /// <summary>
    /// 列印時間（逗號）
    /// </summary>
    /// <param name="timeSpan">TimeSpan</param>
    /// <returns>字串</returns>
    private static string PrintTimeWithComma(TimeSpan timeSpan) =>
        timeSpan.ToString("hh':'mm':'ss','fff", CultureInfo.InvariantCulture);

    /// <summary>
    /// 寫紀錄
    /// </summary>
    /// <param name="message">字串，訊息內容</param>
    private void WriteLog(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        try
        {
            TBLog.InvokeIfRequired(() =>
            {
                TBLog.Text += $"[{DateTime.Now:yyyy/MM/dd HH:mm:ss}] " +
                    $"{message}{Environment.NewLine}";
                TBLog.SelectionStart = TBLog.TextLength;
                TBLog.ScrollToCaret();
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}