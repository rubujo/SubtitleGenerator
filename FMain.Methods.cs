using System.Diagnostics;
using System.Globalization;
using Whisper.net.Ggml;
using Whisper.net;
using Xabe.FFmpeg.Downloader;
using Xabe.FFmpeg.Events;
using Xabe.FFmpeg;
using SubtitleGenerator.Commons.Extensions;

namespace SubtitleGenerator;

// 阻擋設計工具。
partial class DesignerBlocker { }

partial class FMain
{
    /// <summary>
    /// 自定義初始化
    /// </summary>
    private async void CustomInit()
    {
        CheckFolders();

        await CheckFFmpeg();

        CBModel.Text = "Medium";
        CBLanguage.Text = "zh";

        BtnCancel.Enabled = false;
    }

    /// <summary>
    /// 檢查資料夾
    /// </summary>
    private void CheckFolders()
    {
        if (!Directory.Exists(BinsFolderPath))
        {
            Directory.CreateDirectory(BinsFolderPath);

            WriteLog($"已建立資料夾：{BinsFolderPath}");
        }
        else
        {
            WriteLog($"已找到資料夾：{BinsFolderPath}");
        }

        if (!Directory.Exists(ModelsFolderPath))
        {
            Directory.CreateDirectory(ModelsFolderPath);

            WriteLog($"已建立資料夾：{ModelsFolderPath}");
        }
        else
        {
            WriteLog($"已找到資料夾：{ModelsFolderPath}");
        }

        if (!Directory.Exists(TempFolderPath))
        {
            Directory.CreateDirectory(TempFolderPath);

            WriteLog($"已建立資料夾：{TempFolderPath}");
        }
        else
        {
            WriteLog($"已找到資料夾：{TempFolderPath}");
        }
    }

    /// <summary>
    /// 檢查 FFmpeg
    /// </summary>
    private async Task CheckFFmpeg()
    {
        FFmpeg.SetExecutablesPath(BinsFolderPath);

        string ffpmegExePath = Path.Combine(BinsFolderPath, "ffmpeg.exe"),
            ffprobeExePath = Path.Combine(BinsFolderPath, "ffprobe.exe");

        if (!File.Exists(ffpmegExePath) ||
            !File.Exists(ffprobeExePath))
        {
            WriteLog("FFmpeg 執行檔不存在，正在開始下載 FFmpeg 執行檔……");

            await FFmpegDownloader.GetLatestVersion(
                FFmpegVersion.Official,
                BinsFolderPath);

            WriteLog("已下載 FFmpeg 執行檔。");
        }
        else
        {
            WriteLog("已找到 FFmpeg 執行檔。");
        }
    }

    /// <summary>
    /// 檢查模型檔案
    /// </summary>
    /// <param name="ggmlType">GgmlType，預設值為 GgmlType.Medium</param>
    /// <returns>Task&lt;string&gt;，模型檔案的路徑</returns>
    private async Task<string> CheckModelFile(GgmlType ggmlType = GgmlType.Medium)
    {
        string modelFilePath = Path.Combine(ModelsFolderPath, $"ggml-{ggmlType.ToString().ToLower()}.bin"),
            modelFileName = Path.GetFileName(modelFilePath);

        // 判斷模型檔案是否存在。
        if (!File.Exists(modelFilePath))
        {
            WriteLog($"模型檔案 {modelFileName} 不存在，正在開始下載該模型檔案……");

            using Stream stream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
            using FileStream fileStream = File.OpenWrite(modelFilePath);

            await stream.CopyToAsync(fileStream).ContinueWith(task =>
            {
                WriteLog($"已下載模型檔案 {modelFileName}。");
            });
        }
        else
        {
            WriteLog($"已找到模型檔案 {modelFileName}。");
        }

        return modelFilePath;
    }

    /// <summary>
    /// 轉換成 WAV 檔案
    /// </summary>
    /// <param name="path">字串，檔案的路徑</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task&lt;string&gt;，產生的 WAV 檔案的路徑</returns>
    private async Task<string> ConvertToWavFile(
        string path,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            string fileName = Path.GetFileNameWithoutExtension(path);

            IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(path, cancellationToken);

            IEnumerable<IAudioStream> audioStreams = mediaInfo.AudioStreams;

            if (audioStreams == null)
            {
                WriteLog("發生錯誤：請選擇有效的檔案。");
            }

            string filePath = Path.Combine(TempFolderPath, $"{fileName}_{DateTime.Now:yyyyMMddHHmmssfff}.wav");

            // 轉換成取樣率為 16 kHz 的 WAV 檔案。
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

            IConversionResult conversionResult = await conversion.Start(cancellationToken);

            string result = $"FFmpeg 執行結果：{Environment.NewLine}" +
                $"開始時間：{conversionResult.StartTime}{Environment.NewLine}" +
                $"結束時間：{conversionResult.EndTime}{Environment.NewLine}" +
                $"耗時：{conversionResult.Duration}{Environment.NewLine}" +
                $"參數：{conversionResult.Arguments}";

            WriteLog(result);

            return filePath;
        }
        catch (OperationCanceledException)
        {
            WriteLog("已取消作業。");
        }

        return string.Empty;
    }

    /// <summary>
    /// 執行任務
    /// </summary>
    /// <param name="inputFilePath">字串，檔案的路徑</param>
    /// <param name="language">字串，語言（兩碼），預設值為 en</param>
    /// <param name="isTranslate">布林直，是否翻譯成英文，預設值為 false</param>
    /// <param name="isWebVtt">布林直，是否使用 WebVTT 格式，預設值為 false</param>
    /// <param name="ggmlType">GgmlType，預設值為 GgmlType.Medium</param>
    /// <returns>Task</returns>
    private async Task DoTask(
        string inputFilePath,
        string language = "en",
        bool isTranslate = false,
        bool isWebVtt = false,
        GgmlType ggmlType = GgmlType.Medium,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            string tempFilePath = await Task.Run(async () =>
            {
                string wavfilePath = await ConvertToWavFile(inputFilePath, cancellationToken),
                    modelFilePath = await CheckModelFile(ggmlType);

                WriteLog("正在開始作業……");
                WriteLog($"使用的語言：{language}");

                using WhisperFactory whisperFactory = WhisperFactory.FromPath(modelFilePath);

                WhisperProcessorBuilder whisperProcessorBuilder = whisperFactory.CreateBuilder()
                    // 來源 1：https://github.com/sandrohanea/whisper.net/blob/0d1f691b3679c4eb2d97dcebafda1dc1d8439215/Whisper.net/WhisperProcessorBuilder.cs#L302
                    // 來源 2：https://github.com/ggerganov/whisper.cpp/blob/09e90680072d8ecdf02eaf21c393218385d2c616/whisper.cpp#L119
                    .WithLanguage(language);

                if (isTranslate)
                {
                    whisperProcessorBuilder.WithTranslate();
                }

                using WhisperProcessor whisperProcessor = whisperProcessorBuilder.Build();
                using FileStream fileStream = File.OpenRead(wavfilePath);

                List<SegmentData> segmentDataSet = new();

                // TODO: 2023-03-13 由於使用 cancellationToken 會發生下列例外：
                // System.AccessViolationException: 'Attempted to read or write protected memory.
                // This is often an indication that other memory is corrupt.'
                // 故先暫時使用 CancellationToken.None。
                await foreach (SegmentData segmentData in whisperProcessor.ProcessAsync(fileStream, CancellationToken.None))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    segmentDataSet.Add(segmentData);

                    WriteLog($"{segmentData.Start} --> {segmentData.End} : {segmentData.Text}");
                }

                CreateSubtitleFile(segmentDataSet, inputFilePath, isWebVtt);

                return wavfilePath;
            }, cancellationToken);

            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);

                WriteLog($"已刪除暫時檔案：{tempFilePath}");
            }
        }
        catch (OperationCanceledException)
        {
            WriteLog("已取消作業。");
            WriteLog($"請自行至「{TempFolderPath}」刪除暫存檔案。");
        }
    }

    /// <summary>
    /// 建立字幕檔案
    /// </summary>
    /// <param name="segmentDataSet">List&lt;SegmentData&gt;</param>
    /// <param name="inputFilePath">字串，檔案的路徑</param>
    /// <param name="isWebVTT">布林值，是否產生 WebVTT 格式，預設值為 false</param>
    private void CreateSubtitleFile(
        List<SegmentData> segmentDataSet,
        string inputFilePath,
        bool isWebVTT)
    {
        string extName = isWebVTT ? ".vtt" : ".srt",
            filePath = Path.ChangeExtension(inputFilePath, extName),
            fileType = isWebVTT ? "WebVTT" : "SubRip Text";

        WriteLog($"開始建立 {fileType} 字幕檔……");

        using StreamWriter streamWriter = File.CreateText(filePath);

        if (isWebVTT)
        {
            streamWriter.WriteLine("WEBVTT ");
            streamWriter.WriteLine();
        }

        for (int i = 0; i < segmentDataSet.Count; i++)
        {
            streamWriter.WriteLine(i + 1);

            SegmentData segmentData = segmentDataSet[i];

            string startTime = isWebVTT ?
                    PrintTime(segmentData.Start) :
                    PrintTimeWithComma(segmentData.Start),
                endTime = isWebVTT ?
                    PrintTime(segmentData.End) :
                    PrintTimeWithComma(segmentData.End);

            streamWriter.WriteLine("{0} --> {1}", startTime, endTime);
            streamWriter.WriteLine(segmentData.Text);
            streamWriter.WriteLine();
        }

        WriteLog($"已建立 {fileType} 字幕檔：{filePath}");
    }

    /// <summary>
    /// 取得模型類型
    /// </summary>
    /// <param name="value">字串</param>
    /// <returns>GgmlType</returns>
    private GgmlType GetModelType(string value)
    {
        return value switch
        {
            "Tiny" => GgmlType.Tiny,
            "TinyEn" => GgmlType.TinyEn,
            "Base" => GgmlType.Base,
            "BaseEn" => GgmlType.BaseEn,
            "Small" => GgmlType.Small,
            "SmallEn" => GgmlType.SmallEn,
            "Medium" => GgmlType.Medium,
            "MediumEn" => GgmlType.MediumEn,
            "LargeV1" => GgmlType.LargeV1,
            "Large" => GgmlType.Large,
            _ => GgmlType.Medium
        };
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