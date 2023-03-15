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
using SubtitleGenerator.Commons.Sets;
using static SubtitleGenerator.Commons.Sets.EnumSet;

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
        GlobalTT.SetToolTip(CBLanguages, "語言");
        GlobalTT.SetToolTip(CBModels, "模型");
        GlobalTT.SetToolTip(CBSamplingStrategies, "抽樣策略");
        GlobalTT.SetToolTip(CBEnableSpeedUp2x, "可能可以加快轉錄的速度，但同時也有可能會造成轉錄的內容更不精確");
        GlobalTT.SetToolTip(CBEnableTranslate, "將轉錄的內容翻譯成英文");
        GlobalTT.SetToolTip(CBEnableOpenCCS2TWP, "使用 OpenCC 將轉錄的內容，從「簡體中文」轉換成「繁體中文（臺灣）」");
        GlobalTT.SetToolTip(CBEnableOpenCCTW2SP, "使用 OpenCC 將轉錄的內容，從「繁體中文（臺灣）」轉換成「簡體中文」");

        // 設定控制項。
        CBModels.Text = "Small";
        CBLanguages.Text = "zh";
        CBSamplingStrategies.Text = "Default";
        BtnCancel.Enabled = false;

        // 檢查資料夾。
        CheckFolders();

        // 檢查 FFmpeg。
        await CheckFFmpeg();

        // 初始化 OpenCC。
        ZhConverter.Initialize();
    }

    /// <summary>
    /// 檢查資料夾
    /// </summary>
    private void CheckFolders()
    {
        if (!Directory.Exists(FolderSet.BinsFolderPath))
        {
            Directory.CreateDirectory(FolderSet.BinsFolderPath);

            WriteLog($"已建立資料夾：{FolderSet.BinsFolderPath}");
        }
        else
        {
            WriteLog($"已找到資料夾：{FolderSet.BinsFolderPath}");
        }

        if (!Directory.Exists(FolderSet.ModelsFolderPath))
        {
            Directory.CreateDirectory(FolderSet.ModelsFolderPath);

            WriteLog($"已建立資料夾：{FolderSet.ModelsFolderPath}");
        }
        else
        {
            WriteLog($"已找到資料夾：{FolderSet.ModelsFolderPath}");
        }

        if (!Directory.Exists(FolderSet.TempFolderPath))
        {
            Directory.CreateDirectory(FolderSet.TempFolderPath);

            WriteLog($"已建立資料夾：{FolderSet.TempFolderPath}");
        }
        else
        {
            WriteLog($"已找到資料夾：{FolderSet.TempFolderPath}");
        }
    }

    /// <summary>
    /// 檢查 FFmpeg
    /// </summary>
    private async Task CheckFFmpeg()
    {
        FFmpeg.SetExecutablesPath(FolderSet.BinsFolderPath);

        string ffpmegExePath = Path.Combine(FolderSet.BinsFolderPath, "ffmpeg.exe"),
            ffprobeExePath = Path.Combine(FolderSet.BinsFolderPath, "ffprobe.exe");

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
                FolderSet.BinsFolderPath,
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
    /// <param name="ggmlType">GgmlType，預設值為 GgmlType.Small</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task&lt;string&gt;，模型檔案的路徑</returns>
    private async Task<string> CheckModelFile(
        GgmlType ggmlType = GgmlType.Small,
        CancellationToken cancellationToken = default)
    {
        string modelFilePath = Path.Combine(
                FolderSet.ModelsFolderPath,
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

            string tempFilePath = Path.Combine(
                FolderSet.TempFolderPath,
                $"{fileName}_{DateTime.Now:yyyyMMddHHmmssfff}.wav");

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
    /// 偵測語言
    /// <para>因為會發生 System.AccessViolationException，故 speedUp 需設為 false。</para>
    /// </summary>
    /// <param name="inputFilePath">字串，檔案的路徑</param>
    /// <param name="language">字串，語言（兩碼），預設值為 "auto"</param>
    /// <param name="enableTranslate">布林值，啟用翻譯成英文，預設值為 false</param>
    /// <param name="enableSpeedUp2x">布林值，啟用 SpeedUp2x，預設值為 false</param>
    /// <param name="speedUp">布林值，是否加速，預設值為 false</param>
    /// <param name="ggmlType">GgmlType，預設值為 GgmlType.Small</param>
    /// <param name="samplingStrategyType">SamplingStrategyType，預設值為 SamplingStrategyType.Default</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task</returns>
    private async Task DetectLanguage(
        string inputFilePath,
        string language = "auto",
        bool enableTranslate = false,
        bool enableSpeedUp2x = false,
        bool speedUp = false,
        GgmlType ggmlType = GgmlType.Small,
        SamplingStrategyType samplingStrategyType = SamplingStrategyType.Default,
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
                    WriteLog($"請自行至「{FolderSet.TempFolderPath}」刪除暫存檔案。");

                    return string.Empty;
                }

                WriteLog("正在開始作業……");
                WriteLog($"使用的模型：{ggmlType}");

                using WhisperFactory whisperFactory = WhisperFactory.FromPath(modelFilePath);

                WhisperProcessorBuilder whisperProcessorBuilder = whisperFactory.CreateBuilder()
                    .WithSegmentEventHandler(OnNewSegment);

                if (language == "auto")
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

                WaveParser waveParser = new(fileStream);

                float[] avgSamples = await waveParser.GetAvgSamplesAsync(cancellationToken);

                string? detectedLanguage = whisperProcessor.DetectLanguage(avgSamples, speedUp: speedUp),
                    rawResult = string.IsNullOrEmpty(detectedLanguage) ?
                        "識別失敗。" :
                        detectedLanguage,
                    resultMessage = $"偵測語言結果：{rawResult}";

                WriteLog(resultMessage);

                ShowMsg(this, resultMessage);

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
            WriteLog($"請自行至「{FolderSet.TempFolderPath}」刪除暫存檔案。");
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }

    /// <summary>
    /// 轉錄
    /// </summary>
    /// <param name="inputFilePath">字串，檔案的路徑</param>
    /// <param name="language">字串，語言（兩碼），預設值為 "auto"</param>
    /// <param name="enableTranslate">布林值，啟用翻譯成英文，預設值為 false</param>
    /// <param name="enableSpeedUp2x">布林值，啟用 SpeedUp2x，預設值為 false</param>
    /// <param name="exportWebVtt">布林值，匯出 WebVTT 格式，預設值為 false</param>
    /// <param name="ggmlType">GgmlType，預設值為 GgmlType.Small</param>
    /// <param name="samplingStrategyType">SamplingStrategyType，預設值為 SamplingStrategyType.Default</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task</returns>
    private async Task Transcribe(
        string inputFilePath,
        string language = "auto",
        bool enableTranslate = false,
        bool enableSpeedUp2x = false,
        bool exportWebVtt = false,
        GgmlType ggmlType = GgmlType.Small,
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
                    WriteLog($"請自行至「{FolderSet.TempFolderPath}」刪除暫存檔案。");

                    return string.Empty;
                }

                WriteLog("正在開始作業……");
                WriteLog($"使用的模型：{ggmlType}");
                WriteLog($"使用的語言：{language}");
                WriteLog($"使用的抽樣策略：{samplingStrategyType}");
                WriteLog($"使用 OpenCC：{(EnableOpenCC ? "是" : "否")}");
                WriteLog($"OpenCC 模式：{GlobalOCCMode}");

                using WhisperFactory whisperFactory = WhisperFactory.FromPath(modelFilePath);

                WhisperProcessorBuilder whisperProcessorBuilder = whisperFactory.CreateBuilder()
                    .WithSegmentEventHandler(OnNewSegment);

                if (language == "auto")
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

                WriteLog("轉錄的內容：");

                await foreach (SegmentData segmentData in whisperProcessor
                    .ProcessAsync(fileStream, cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    segmentDataSet.Add(segmentData);
                }

                WriteLog("轉錄完成。");

                // 建立字幕檔。
                string subtitleFilePath = CreateSubtitleFile(
                        segmentDataSet,
                        inputFilePath,
                        exportWebVtt),
                    subtitleFileName = Path.GetFileName(subtitleFilePath),
                    subtitleFileFolder = Path.GetFullPath(subtitleFilePath)
                        .Replace(subtitleFileName, string.Empty);

                // 開啟資料夾。
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
            WriteLog($"請自行至「{FolderSet.TempFolderPath}」刪除暫存檔案。");
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
    /// <param name="strategyType">SamplingStrategyType，預設值為 SamplingStrategyType.Default</param>
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
            streamWriter.WriteLine(GetSegmentDataText(segmentData));
            streamWriter.WriteLine();
        }

        WriteLog($"已建立 {fileType} 字幕檔：{filePath}");

        return filePath;
    }

    /// <summary>
    /// 取得 SegmentData 的文字
    /// </summary>
    /// <param name="segmentData">SegmentData</param>
    /// <returns>字串，文字內容</returns>
    private string GetSegmentDataText(SegmentData segmentData)
    {
        return EnableOpenCC ?
            GlobalOCCMode switch
            {
                OpenCCMode.None => segmentData.Text,
                OpenCCMode.S2TWP => ZhConverter.HansToTW(segmentData.Text, true),
                OpenCCMode.TW2SP => ZhConverter.TWToHans(segmentData.Text, true),
                _ => segmentData.Text
            } :
            segmentData.Text;
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
    /// 取得抽樣策略類型
    /// </summary>
    /// <param name="value">字串</param>
    /// <returns>EnumSet.SamplingStrategyType</returns>
    private static SamplingStrategyType GetSamplingStrategyType(string value)
    {
        return value switch
        {
            "Default" => SamplingStrategyType.Default,
            "Greedy" => SamplingStrategyType.Greedy,
            "Beam search" => SamplingStrategyType.BeamSearch,
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
            "Tiny" => GgmlType.Tiny,
            "Tiny En" => GgmlType.TinyEn,
            "Base" => GgmlType.Base,
            "Base En" => GgmlType.BaseEn,
            "Small" => GgmlType.Small,
            "Small En" => GgmlType.SmallEn,
            "Medium" => GgmlType.Medium,
            "Medium En" => GgmlType.MediumEn,
            "Large V1" => GgmlType.LargeV1,
            "Large" => GgmlType.Large,
            _ => GgmlType.Base
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
    private readonly Action<Form, string> ShowMsg =
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