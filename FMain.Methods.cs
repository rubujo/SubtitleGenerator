using System.Diagnostics;
using System.Globalization;
using Whisper.net.Ggml;
using Whisper.net;
using Xabe.FFmpeg.Downloader;
using Xabe.FFmpeg.Events;
using Xabe.FFmpeg;
using SubtitleGenerator.Commons.Extensions;
using System.Reflection;
using SubtitleGenerator.Whisper.net.Ggml;
using OpenCCNET;
using Whisper.net.Wave;

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
        LVersion.InvokeIfRequired(() =>
        {
            LVersion.Text = $"版本：v{Assembly.GetExecutingAssembly().GetName().Version?.ToString()}";
        });

        GlobalTT.SetToolTip(CBLanguages, "語言");
        GlobalTT.SetToolTip(CBModels, "模型");
        GlobalTT.SetToolTip(CBSamplingStrategies, "抽樣策略");
        GlobalTT.SetToolTip(CBEnableSpeedUp2x, "可能可以加快辨識處裡的速度，但同時也有可能會造成辨識結果更不精確");
        GlobalTT.SetToolTip(CBEnableTranslate, "將辨識出的內容翻譯成英文");
        GlobalTT.SetToolTip(CBEnableOpenCCS2TWP, "簡體中文 => 繁體中文（臺灣）");
        GlobalTT.SetToolTip(CBEnableOpenCCTW2SP, "繁體中文（臺灣）=> 簡體中文 ");

        CBModels.Text = "中";
        CBLanguages.Text = "zh";
        CBSamplingStrategies.Text = "預設";

        BtnCancel.Enabled = false;

        // 初始化 OpenCC。
        ZhConverter.Initialize();

        CheckFolders();

        await CheckFFmpeg();
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

            int tempPercent = 0;

            bool isFinished = false;

            Progress<ProgressInfo> progress = new();

            progress.ProgressChanged += (object? sender, ProgressInfo e) =>
            {
                if (e.DownloadedBytes == 1 && e.TotalBytes == 1)
                {
                    // 避免重複輸出。
                    if (!isFinished)
                    {
                        isFinished = true;

                        WriteLog("已下載 FFmpeg 執行檔。");
                    }
                }
                else
                {
                    double rawPercent = (double)e.DownloadedBytes / e.TotalBytes * 100.0;
                    double actulPercent = Math.Round(rawPercent, 2, MidpointRounding.AwayFromZero);

                    // 減速機制。
                    int parsePercent = Convert.ToInt32(actulPercent);

                    if (parsePercent > tempPercent)
                    {
                        tempPercent = parsePercent;

                        WriteLog($"下載進度：{e.DownloadedBytes}/{e.TotalBytes} Bytes ({actulPercent}%)");
                    }
                }
            };

            await FFmpegDownloader.GetLatestVersion(
                FFmpegVersion.Official,
                BinsFolderPath,
                progress);
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
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task&lt;string&gt;，模型檔案的路徑</returns>
    private async Task<string> CheckModelFile(
        GgmlType ggmlType = GgmlType.Medium,
        CancellationToken cancellationToken = default)
    {
        string modelFilePath = Path.Combine(
                ModelsFolderPath,
                GetModelFileName(ggmlType)),
            modelFileName = Path.GetFileName(modelFilePath);

        try
        {
            // 判斷模型檔案是否存在。
            if (!File.Exists(modelFilePath))
            {
                WriteLog($"模型檔案 {modelFileName} 不存在，正在開始下載該模型檔案……");

                using Stream stream = await CustomWhisperGgmlDownloader.GetGgmlModelAsync(
                    ggmlType,
                    cancellationToken);
                using FileStream fileStream = File.OpenWrite(modelFilePath);

                await stream.CopyToAsync(fileStream, cancellationToken).ContinueWith(task =>
                {
                    WriteLog($"已下載模型檔案 {modelFileName}。");
                }, cancellationToken);
            }
            else
            {
                WriteLog($"已找到模型檔案 {modelFileName}。");
            }
        }
        catch (OperationCanceledException)
        {
            WriteLog("已取消作業。");
        }
        catch (HttpRequestException hre)
        {
            modelFilePath = string.Empty;

            ShowErrMsg(this, hre.Message);
        }
        catch (Exception ex)
        {
            modelFilePath = string.Empty;

            ShowErrMsg(this, ex.ToString());
        }

        return modelFilePath;
    }

    /// <summary>
    /// 轉換成 WAV 檔案
    /// </summary>
    /// <param name="inputFilePath">字串，檔案的路徑</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task&lt;string&gt;，產生的 WAV 檔案的路徑</returns>
    private async Task<string> ConvertToWavFile(
        string inputFilePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            string fileName = Path.GetFileNameWithoutExtension(inputFilePath);

            IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(inputFilePath, cancellationToken);

            IEnumerable<IAudioStream> audioStreams = mediaInfo.AudioStreams;

            if (audioStreams == null)
            {
                WriteLog("發生錯誤：請選擇有效的視訊或音訊檔案。");
            }

            string tempFilePath = Path.Combine(TempFolderPath, $"{fileName}_{DateTime.Now:yyyyMMddHHmmssfff}.wav");

            // 轉換成取樣率為 16 kHz 的 WAV 檔案。
            IConversion conversion = FFmpeg.Conversions.New()
                .AddStream(audioStreams)
                .AddParameter("-ar 16000")
                .SetOutputFormat(Format.wav)
                .SetOutput(tempFilePath)
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

            return tempFilePath;
        }
        catch (OperationCanceledException)
        {
            WriteLog("已取消作業。");
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }

        return string.Empty;
    }

    /// <summary>
    /// 執行語言偵測
    /// </summary>
    /// <param name="inputFilePath">字串，檔案的路徑</param>
    /// <param name="ggmlType">GgmlType，預設值為 GgmlType.Medium</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task</returns>
    private async Task DoLanguageDetection(
        string inputFilePath,
        GgmlType ggmlType = GgmlType.Medium,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            string tempFilePath = await Task.Run(async () =>
            {
                string wavfilePath = await ConvertToWavFile(inputFilePath, cancellationToken),
                    modelFilePath = await CheckModelFile(ggmlType, cancellationToken);

                if (string.IsNullOrEmpty(modelFilePath))
                {
                    WriteLog("發生錯誤：使用的模型檔案不存在或下載失敗。");
                    WriteLog("已取消作業。");
                    WriteLog($"請自行至「{TempFolderPath}」刪除暫存檔案。");

                    return string.Empty;
                }

                WriteLog("正在開始作業……");
                WriteLog($"使用的模型：{ggmlType}");

                byte[] bufferedModel = File.ReadAllBytes(modelFilePath);

                using WhisperFactory whisperFactory = WhisperFactory.FromBuffer(bufferedModel);
                using WhisperProcessor whisperProcessor = whisperFactory.CreateBuilder().Build();
                using FileStream fileStream = File.OpenRead(wavfilePath);

                WaveParser waveParser = new(fileStream);

                float[] avgSamples = await waveParser.GetAvgSamplesAsync(cancellationToken);

                // TODO: 2023-03-14 使用參數 speedUp 會造成發生例外。
                string? detectedLanguage = whisperProcessor.DetectLanguage(avgSamples),
                    resultMessage = string.IsNullOrEmpty(detectedLanguage) ?
                        "此視訊或音訊檔案的語言識別失敗。" :
                        $"此視訊或音訊檔案的語言是：{detectedLanguage}";

                WriteLog($"偵測語言結果：{resultMessage}");

                return wavfilePath;
            }, cancellationToken);

            if (!string.IsNullOrEmpty(tempFilePath) &&
                File.Exists(tempFilePath))
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
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }

    /// <summary>
    /// 執行完整偵測
    /// </summary>
    /// <param name="inputFilePath">字串，檔案的路徑</param>
    /// <param name="language">字串，語言（兩碼），預設值為 "自動"</param>
    /// <param name="enableTranslate">布林值，啟用翻譯成英文，預設值為 false</param>
    /// <param name="enableSpeedUp2x">布林值，啟用 SpeedUp2x，預設值為 false</param>
    /// <param name="exportWebVtt">布林值，匯出 WebVTT 格式，預設值為 false</param>
    /// <param name="ggmlType">GgmlType，預設值為 GgmlType.Medium</param>
    /// <param name="samplingStrategyType">SamplingStrategyType，預設值為 SamplingStrategyType.Default</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task</returns>
    private async Task DoFullDetection(
        string inputFilePath,
        string language = "自動",
        bool enableTranslate = false,
        bool enableSpeedUp2x = false,
        bool exportWebVtt = false,
        GgmlType ggmlType = GgmlType.Medium,
        SamplingStrategyType samplingStrategyType = SamplingStrategyType.Default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            string tempFilePath = await Task.Run(async () =>
            {
                List<SegmentData> segmentDataSet = new();

                string wavfilePath = await ConvertToWavFile(inputFilePath, cancellationToken),
                    modelFilePath = await CheckModelFile(ggmlType, cancellationToken);

                if (string.IsNullOrEmpty(modelFilePath))
                {
                    WriteLog("發生錯誤：使用的模型檔案不存在或下載失敗。");
                    WriteLog("已取消作業。");
                    WriteLog($"請自行至「{TempFolderPath}」刪除暫存檔案。");

                    return string.Empty;
                }

                WriteLog("正在開始作業……");
                WriteLog($"使用的模型：{ggmlType}");
                WriteLog($"使用的語言：{language}");
                WriteLog($"使用的抽樣策略：{samplingStrategyType}");
                WriteLog($"使用 OpenCC：{(EnableOpenCC ? "是" : "否")}");
                WriteLog($"OpenCC 模式：{GlobalOCCType}");

                using WhisperFactory whisperFactory = WhisperFactory.FromPath(modelFilePath);

                WhisperProcessorBuilder whisperProcessorBuilder = whisperFactory.CreateBuilder()
                    .WithSegmentEventHandler(OnNewSegment);

                if (language == "自動")
                {
                    whisperProcessorBuilder.WithLanguageDetection();
                }
                else
                {
                    // 來源 1：https://github.com/sandrohanea/whisper.net/blob/0d1f691b3679c4eb2d97dcebafda1dc1d8439215/Whisper.net/WhisperProcessorBuilder.cs#L302
                    // 來源 2：https://github.com/ggerganov/whisper.cpp/blob/09e90680072d8ecdf02eaf21c393218385d2c616/whisper.cpp#L119
                    whisperProcessorBuilder.WithLanguage(language);
                }

                if (enableTranslate)
                {
                    whisperProcessorBuilder.WithTranslate();
                }

                if (enableSpeedUp2x)
                {
                    whisperProcessorBuilder.WithSpeedUp2x();
                }

                using WhisperProcessor whisperProcessor = GetWhisperProcessor(
                    whisperProcessorBuilder,
                    samplingStrategyType);
                using FileStream fileStream = File.OpenRead(wavfilePath);

                WriteLog("辨識的內容：");

                // TODO: 2023-03-13 由於使用 cancellationToken 會發生下列例外：
                // System.AccessViolationException: 'Attempted to read or write protected memory.
                // This is often an indication that other memory is corrupt.'
                // 故先暫時使用 CancellationToken.None。
                await foreach (SegmentData segmentData in whisperProcessor.ProcessAsync(fileStream, CancellationToken.None))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    segmentDataSet.Add(segmentData);
                }

                string subtitleFilePath = CreateSubtitleFile(
                        segmentDataSet,
                        inputFilePath,
                        exportWebVtt),
                    subtitleFileName = Path.GetFileName(subtitleFilePath),
                    subtitleFileFolder = Path.GetFullPath(subtitleFilePath)
                        .Replace(subtitleFileName, string.Empty);

                OpenFolder(subtitleFileFolder);

                return wavfilePath;
            }, cancellationToken);

            if (!string.IsNullOrEmpty(tempFilePath) &&
                File.Exists(tempFilePath))
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
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }

    /// <summary>
    /// 取得 WhisperProcessor
    /// </summary>
    /// <param name="whisperProcessorBuilder">WhisperProcessorBuilder</param>
    /// <param name="strategyType">SamplingStrategyType，預設值為 Strategy.Default</param>
    /// <returns>WhisperProcessor</returns>
    private static WhisperProcessor GetWhisperProcessor(
        WhisperProcessorBuilder whisperProcessorBuilder,
        SamplingStrategyType strategyType = SamplingStrategyType.Default)
    {
        WhisperProcessor whisperProcessor;

        switch (strategyType)
        {
            case SamplingStrategyType.Default:
                whisperProcessor = whisperProcessorBuilder.Build();

                break;
            case SamplingStrategyType.Greedy:
                BeamSearchSamplingStrategyBuilder beamSearchSamplingStrategyBuilder =
                    (BeamSearchSamplingStrategyBuilder)whisperProcessorBuilder
                        .WithBeamSearchSamplingStrategy();

                // TODO: 2023-03-14 待看未來是否要開放可以自己設定。

                // 來源 1：https://github.com/sandrohanea/whisper.net/blob/0d1f691b3679c4eb2d97dcebafda1dc1d8439215/Whisper.net/BeamSearchSamplingStrategyBuilder.cs#L31
                // 來源 2：https://github.com/sandrohanea/whisper.net/blob/0d1f691b3679c4eb2d97dcebafda1dc1d8439215/Whisper.net/BeamSearchSamplingStrategyBuilder.cs#L46
                //beamSearchSamplingStrategyBuilder
                //    .WithBeamSize(5)
                //    .WithPatience(-0.1f);

                whisperProcessor = beamSearchSamplingStrategyBuilder
                    .ParentBuilder.Build();

                break;
            case SamplingStrategyType.BeamSearch:
                GreedySamplingStrategyBuilder greedySamplingStrategyBuilder =
                    (GreedySamplingStrategyBuilder)whisperProcessorBuilder
                        .WithGreedySamplingStrategy();

                // TODO: 2023-03-14 待看未來是否要開放可以自己設定。

                // 來源：https://github.com/sandrohanea/whisper.net/blob/0d1f691b3679c4eb2d97dcebafda1dc1d8439215/Whisper.net/GreedySamplingStrategyBuilder.cs#L31
                //greedySamplingStrategyBuilder.WithBestOf(1);

                whisperProcessor = greedySamplingStrategyBuilder
                    .ParentBuilder.Build();

                break;
            default:
                whisperProcessor = whisperProcessorBuilder.Build();

                break;
        }

        return whisperProcessor;
    }

    /// <summary>
    /// 建立字幕檔案
    /// </summary>
    /// <param name="segmentDataSet">List&lt;SegmentData&gt;</param>
    /// <param name="inputFilePath">字串，檔案的路徑</param>
    /// <param name="exportWebVTT">布林值，匯出 WebVTT 格式，預設值為 false</param>
    /// <returns>字串，字幕檔案的路徑</returns>
    private string CreateSubtitleFile(
        List<SegmentData> segmentDataSet,
        string inputFilePath,
        bool exportWebVTT)
    {
        string extName = exportWebVTT ? ".vtt" : ".srt",
            filePath = Path.ChangeExtension(inputFilePath, extName),
            fileType = exportWebVTT ? "WebVTT" : "SubRip Text";

        WriteLog($"開始建立 {fileType} 字幕檔……");

        using StreamWriter streamWriter = File.CreateText(filePath);

        if (exportWebVTT)
        {
            streamWriter.WriteLine("WEBVTT ");
            streamWriter.WriteLine();
        }

        for (int i = 0; i < segmentDataSet.Count; i++)
        {
            streamWriter.WriteLine(i + 1);

            SegmentData segmentData = segmentDataSet[i];

            string startTime = exportWebVTT ?
                    PrintTime(segmentData.Start) :
                    PrintTimeWithComma(segmentData.Start),
                endTime = exportWebVTT ?
                    PrintTime(segmentData.End) :
                    PrintTimeWithComma(segmentData.End);

            streamWriter.WriteLine("{0} --> {1}", startTime, endTime);

            if (EnableOpenCC && GlobalOCCType != OpenCCType.None)
            {
                switch (GlobalOCCType)
                {
                    case OpenCCType.None:
                        streamWriter.WriteLine(segmentData.Text);

                        break;
                    case OpenCCType.S2TWP:
                        streamWriter.WriteLine(ZhConverter.HansToTW(segmentData.Text, true));

                        break;
                    case OpenCCType.TW2SP:
                        streamWriter.WriteLine(ZhConverter.TWToHans(segmentData.Text, true));

                        break;
                    default:
                        streamWriter.WriteLine(segmentData.Text);

                        break;
                }
            }
            else
            {
                streamWriter.WriteLine(segmentData.Text);
            }

            streamWriter.WriteLine();
        }

        WriteLog($"已建立 {fileType} 字幕檔：{filePath}");

        return filePath;
    }

    /// <summary>
    /// 設定 OpenCC 相關的變數
    /// </summary>
    private void SetOpenCCVariables()
    {
        EnableOpenCC = CBEnableOpenCCS2TWP.Checked | CBEnableOpenCCTW2SP.Checked;

        if (EnableOpenCC)
        {
            if (CBEnableOpenCCS2TWP.Checked)
            {
                GlobalOCCType = OpenCCType.S2TWP;
            }
            else if (CBEnableOpenCCTW2SP.Checked)
            {
                GlobalOCCType = OpenCCType.TW2SP;
            }
            else
            {
                GlobalOCCType = OpenCCType.None;
            }
        }
    }

    /// <summary>
    /// 取得抽樣策略類型
    /// </summary>
    /// <param name="value">字串</param>
    /// <returns>SamplingStrategyType</returns>
    private static SamplingStrategyType GetSamplingStrategyType(string value)
    {
        return value switch
        {
            "預設" => SamplingStrategyType.Default,
            "貪婪" => SamplingStrategyType.Greedy,
            "集束搜尋" => SamplingStrategyType.BeamSearch,
            _ => SamplingStrategyType.Default
        };
    }

    /// <summary>
    /// 取得模型類型
    /// </summary>
    /// <param name="value">字串</param>
    /// <returns>GgmlType</returns>
    private static GgmlType GetModelType(string value)
    {
        return value switch
        {
            "微小" => GgmlType.Tiny,
            "微小（英文）" => GgmlType.TinyEn,
            "基礎" => GgmlType.Base,
            "基礎（英文）" => GgmlType.BaseEn,
            "小" => GgmlType.Small,
            "小（英文）" => GgmlType.SmallEn,
            "中" => GgmlType.Medium,
            "中（英文）" => GgmlType.MediumEn,
            "大 V1" => GgmlType.LargeV1,
            "大" => GgmlType.Large,
            _ => GgmlType.Medium
        };
    }

    /// <summary>
    /// 取得模型檔案的名稱
    /// </summary>
    /// <param name="ggmlType">GgmlType</param>
    /// <returns>字串</returns>
    private static string GetModelFileName(GgmlType ggmlType)
    {
        return ggmlType switch
        {
            GgmlType.Tiny => "ggml-tiny.bin",
            GgmlType.TinyEn => "ggml-tiny.en.bin",
            GgmlType.Base => "ggml-base.bin",
            GgmlType.BaseEn => "ggml-base.en.bin",
            GgmlType.Small => "ggml-small.bin",
            GgmlType.SmallEn => "ggml-small.en.bin",
            GgmlType.Medium => "ggml-medium.bin",
            GgmlType.MediumEn => "ggml-medium.en.bin",
            GgmlType.LargeV1 => "ggml-large-v1.bin",
            GgmlType.Large => "ggml-large.bin",
            _ => string.Empty
        };
    }

    /// <summary>
    /// 列印時間（點）
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
    /// 顯示錯誤訊息
    /// </summary>
    private readonly Action<Form, string> ShowErrMsg =
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
    private readonly Action<Form, string> ShowWarnMsg =
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
    /// <param name="path">字串，路徑</param>
    private void OpenFolder(string path)
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
            ShowErrMsg(this, ex.ToString());
        }
    }

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
            ShowErrMsg(this, ex.ToString());
        }
    }
}