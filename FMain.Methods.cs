using OpenCCNET;
using SubtitleGenerator.Commons;
using SubtitleGenerator.Commons.Extensions;
using SubtitleGenerator.Commons.Sets;
using static SubtitleGenerator.Commons.Sets.EnumSet;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Whisper;
using Whisper.net.Ggml;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using Xabe.FFmpeg.Events;

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
        GlobalTT.SetToolTip(CBEnableTranslate, "將轉錄的內容翻譯成英文");
        GlobalTT.SetToolTip(CBConvertToWav, "先透過 FFmpeg 將選擇的檔案轉換成 WAV 格式的暫時檔案後，在進行轉錄");
        GlobalTT.SetToolTip(CBEnableOpenCCS2TWP, "使用 OpenCC 將轉錄的內容，從「簡體中文」轉換成「繁體中文（臺灣）」");
        GlobalTT.SetToolTip(CBEnableOpenCCTW2SP, "使用 OpenCC 將轉錄的內容，從「繁體中文（臺灣）」轉換成「簡體中文」");

        // 設定控制項。
        CBModels.Text = "Small";
        CBSamplingStrategies.Text = "Default";
        CBLanguages.Text = "zh";
        CBModelImplementation.Text = "GPU";

        CBGPUs.DataSource = GetGpuList();

        if (CBGPUs.Items.Count > 0)
        {
            CBGPUs.SelectedIndex = 0;
        }

        CBGpuModelFlags.Text = "Wave32";
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
    /// 轉錄
    /// </summary>
    /// <param name="inputFilePath">字串，檔案的路徑</param>
    /// <param name="language">字串，語言（兩碼），預設值為 "en"</param>
    /// <param name="enableTranslate">布林值，啟用翻譯成英文，預設值為 false</param>
    /// <param name="enableSpeedUpAudio">布林值，啟用 SpeedUpAudio，預設值為 false</param>
    /// <param name="exportWebVtt">布林值，匯出 WebVTT 格式，預設值為 false</param>
    /// <param name="enableConvertToWav">布林值，啟用轉換成 WAV 檔案，預設值為 false</param>
    /// <param name="modelImplementation">eModelImplementation，預設值為 eModelImplementation.GPU</param>
    /// <param name="gpuModelFlags">eGpuModelFlags，預設值為 eGpuModelFlags.None</param>
    /// <param name="adapter">字串，GPU 裝置的名稱，預設值為 null</param>
    /// <param name="ggmlType">GgmlType，預設值為 GgmlType.Small</param>
    /// <param name="samplingStrategyType">SamplingStrategyType，預設值為 SamplingStrategyType.Default</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task</returns>
    private async Task Transcribe(
        string inputFilePath,
        string language = "en",
        bool enableTranslate = false,
        bool enableSpeedUpAudio = false,
        bool exportWebVtt = false,
        bool enableConvertToWav = false,
        eModelImplementation modelImplementation = eModelImplementation.GPU,
        eGpuModelFlags gpuModelFlags = eGpuModelFlags.None,
        string? adapter = null,
        GgmlType ggmlType = GgmlType.Small,
        SamplingStrategyType samplingStrategyType = SamplingStrategyType.Default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Stopwatch stopWatch = new();

            stopWatch.Start();

            cancellationToken.ThrowIfCancellationRequested();

            string tempFilePath = await Task.Run(async () =>
            {
                string filePath = enableConvertToWav ?
                    await ConvertToWavFile(inputFilePath, cancellationToken) :
                    inputFilePath,
                    modelFilePath = await CheckModelFile(ggmlType, cancellationToken),
                    modelFileName = Path.GetFileName(modelFilePath);

                if (string.IsNullOrEmpty(modelFilePath))
                {
                    WriteLog($"發生錯誤：使用的模型檔案 {modelFileName} 不存在或下載失敗。");
                    WriteLog("已取消轉錄作業。");

                    if (enableConvertToWav)
                    {
                        WriteLog($"請自行至「{FolderSet.TempFolderPath}」刪除暫存檔案。");
                    }

                    return string.Empty;
                }

                WriteLog($"使用的模型實作：{modelImplementation}");
                WriteLog($"使用的 GPU 裝置：{adapter ?? "預設"}");
                WriteLog($"使用的 GPU 模型旗標：{gpuModelFlags}");
                WriteLog($"使用的模型：{ggmlType}");
                WriteLog($"使用的語言：{language}");
                WriteLog($"使用的抽樣策略：{samplingStrategyType}");
                WriteLog($"使用 OpenCC：{(EnableOpenCC ? "是" : "否")}");
                WriteLog($"OpenCC 模式：{GlobalOCCMode}");

                int tempPercent = 0;

                bool isFinished = false;

                Action<double> pfnProgress = new((value) =>
                {
                    double actualPercent = Math.Round(value * 100, 1, MidpointRounding.AwayFromZero);

                    // 減速機制。
                    int parsePercent = Convert.ToInt32(actualPercent);

                    if (parsePercent > tempPercent)
                    {
                        WriteLog($"模型檔案載入進度：{actualPercent}%");

                        tempPercent = parsePercent;
                    }

                    if (actualPercent >= 100 && !isFinished)
                    {
                        isFinished = true;

                        WriteLog($"模型檔案 {modelFileName} 載入完成。");
                    }
                });

                pfnLogMessage pfnLogMessage = new((eLogLevel level, string message) =>
                {
                    WriteLog($"[{level}] {message}");
                });

                const eLoggerFlags loggerFlags = eLoggerFlags.UseStandardError | eLoggerFlags.SkipFormatMessage;

                Library.setLogSink(eLogLevel.Debug, loggerFlags, pfnLogMessage);

                WriteLog($"正在開始載入模型檔案 {modelFileName} ……");

                using iModel model = await Library.loadModelAsync(
                    path: modelFilePath,
                    cancelToken: cancellationToken,
                    flags: gpuModelFlags,
                    adapter: adapter,
                    pfnProgress: pfnProgress,
                    impl: modelImplementation);
                using Context context = model.createContext();

                context.parameters.setFlag(eFullParamsFlags.PrintRealtime, false);
                context.parameters.setFlag(eFullParamsFlags.PrintProgress, true);
                context.parameters.setFlag(eFullParamsFlags.PrintTimestamps, true);
                context.parameters.setFlag(eFullParamsFlags.PrintSpecial, false);
                context.parameters.setFlag(eFullParamsFlags.Translate, enableTranslate);
                // TODO: 2023-03-16 在 Const-me/Whisper 函式庫內為實驗性質，在 GPU 模型沒有實作此功能，使用時會發生例外。
                context.parameters.setFlag(eFullParamsFlags.SpeedupAudio, enableSpeedUpAudio);
                // 反查時失敗時則使用 eLanguage.English。
                context.parameters.language = Library.languageFromCode(language) ?? eLanguage.English;
                context.parameters.cpuThreads = Environment.ProcessorCount;

                switch (samplingStrategyType)
                {
                    default:
                    case SamplingStrategyType.Default:
                        // 不進行任何處裡。
                        break;
                    case SamplingStrategyType.Greedy:
                        context.parameters.strategy = eSamplingStrategy.Greedy;

                        // TODO: 2023-03-16 待看未來是否要開放可以自己設定。
                        //context.parameters.greedy = new Parameters.sGreedy()
                        //{
                        //    n_past = 0
                        //};

                        break;
                    case SamplingStrategyType.BeamSearch:
                        // TODO: 2023-03-16 在 Const-me/Whisper 函式庫內尚未實作此抽樣策略。。
                        context.parameters.strategy = eSamplingStrategy.BeamSearch;

                        // TODO: 2023-03-16 待看未來是否要開放可以自己設定。
                        //context.parameters.beamSearch = new Parameters.sBeamSearch() 
                        //{ 
                        //    beam_width = 0,
                        //    n_best = 0,
                        //    n_past = 0 
                        //};

                        break;
                }

                using iMediaFoundation mediaFoundation = Library.initMediaFoundation();
                using iAudioBuffer audioBuffer = mediaFoundation.loadAudioFile(path: filePath, stereo: true);

                cancellationToken.ThrowIfCancellationRequested();

                WriteLog("正在開始轉錄作業……");

                CustomCallbacks customCallbacks = new(this, cancellationToken);

                context.runFull(buffer: audioBuffer, callbacks: customCallbacks);

                WriteLog("轉錄作業完成。");

                cancellationToken.ThrowIfCancellationRequested();

                // 建立字幕檔。
                string subtitleFilePath = CreateSubtitleFile(
                        context,
                        inputFilePath,
                        exportWebVtt),
                    subtitleFileName = Path.GetFileName(subtitleFilePath),
                    subtitleFileFolder = Path.GetFullPath(subtitleFilePath)
                        .Replace(subtitleFileName, string.Empty);

                stopWatch.Stop();

                WriteLog($"總共耗時：{stopWatch.Elapsed.ToFFmpeg()}");

                ShowMsg(this, "轉錄作業完成。");

                // 開啟字幕檔所位於的資料夾。
                OpenFolder(subtitleFileFolder);

                return filePath;
            }, cancellationToken);

            // 當有啟用轉換成 WAV 檔案時才需要刪除暫時檔案。
            if (enableConvertToWav)
            {
                if (!string.IsNullOrEmpty(tempFilePath) &&
                    File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);

                    WriteLog($"已刪除暫時檔案：{tempFilePath}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            WriteLog("已取消轉錄作業。");

            if (enableConvertToWav)
            {
                WriteLog($"請自行至「{FolderSet.TempFolderPath}」刪除暫存檔案。");
            }
        }
        catch (ApplicationException ae)
        {
            WriteLog("已取消轉錄作業。");

            if (enableConvertToWav)
            {
                WriteLog($"請自行至「{FolderSet.TempFolderPath}」刪除暫存檔案。");
            }

            ShowErrMsg(this, ae.Message);
        }
        catch (Exception ex)
        {
            WriteLog("已取消轉錄作業。");

            if (enableConvertToWav)
            {
                WriteLog($"請自行至「{FolderSet.TempFolderPath}」刪除暫存檔案。");
            }

            ShowErrMsg(this, ex.ToString());
        }
    }

    /// <summary>
    /// 建立字幕檔
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="inputFilePath">字串，檔案的路徑</param>
    /// <param name="exportWebVTT">布林值，匯出 WebVTT 格式，預設值為 false</param>
    /// <returns>字串，字幕檔案的路徑</returns>
    private string CreateSubtitleFile(Context context,
        string inputFilePath,
        bool exportWebVTT)
    {
        string extName = exportWebVTT ? ".vtt" : ".srt",
            filePath = Path.ChangeExtension(inputFilePath, extName),
            fileType = exportWebVTT ? "WebVTT" : "SubRip Text";

        WriteLog($"開始建立 {fileType} 字幕檔……");

        using StreamWriter streamWriter = File.CreateText(filePath);

        ReadOnlySpan<sSegment> segments = context.results(eResultFlags.Timestamps).segments;

        if (exportWebVTT)
        {
            streamWriter.WriteLine("WEBVTT ");
            streamWriter.WriteLine();
        }

        for (int i = 0; i < segments.Length; i++)
        {
            streamWriter.WriteLine(i + 1);

            sSegment segment = segments[i];

            string startTime = exportWebVTT ?
                    PrintTime(segment.time.begin) :
                    PrintTimeWithComma(segment.time.begin),
                endTime = exportWebVTT ?
                    PrintTime(segment.time.end) :
                    PrintTimeWithComma(segment.time.end);

            streamWriter.WriteLine("{0} --> {1}", startTime, endTime);
            streamWriter.WriteLine(GetSegmentText(segment));
            streamWriter.WriteLine();
        }

        WriteLog($"已建立 {fileType} 字幕檔：{filePath}");

        return filePath;
    }

    /// <summary>
    /// 取得 sSegment 的文字
    /// </summary>
    /// <param name="segment">sSegment</param>
    /// <returns>字串，文字內容</returns>
    private string GetSegmentText(sSegment segment)
    {
        if (string.IsNullOrEmpty(segment.text))
        {
            return string.Empty;
        }

        return EnableOpenCC ?
            GlobalOCCMode switch
            {
                OpenCCMode.None => segment.text,
                OpenCCMode.S2TWP => ZhConverter.HansToTW(segment.text, true),
                OpenCCMode.TW2SP => ZhConverter.TWToHans(segment.text, true),
                _ => segment.text
            } :
            segment.text;
    }

    /// <summary>
    /// 取得 GPU 裝置清單
    /// </summary>
    /// <returns>字串陣列</returns>
    private static string[] GetGpuList()
    {
        return Library.listGraphicAdapters();
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
    /// 取得 GPU 模型旗標
    /// </summary>
    /// <param name="value">字串</param>
    /// <returns>eGpuModelFlags</returns>
    private static eGpuModelFlags GetGpuModelFlag(string value)
    {
        return value switch
        {
            "Wave32" => eGpuModelFlags.Wave32 | eGpuModelFlags.NoReshapedMatMul,
            "Wave64" => eGpuModelFlags.Wave64 | eGpuModelFlags.NoReshapedMatMul,
            "Wave32 (重塑矩陣乘法)" => eGpuModelFlags.Wave32 | eGpuModelFlags.UseReshapedMatMul,
            "Wave64 (重塑矩陣乘法)" => eGpuModelFlags.Wave64 | eGpuModelFlags.UseReshapedMatMul,
            _ => eGpuModelFlags.None
        };
    }

    /// <summary>
    /// 取得模型實作
    /// </summary>
    /// <param name="value">字串</param>
    /// <returns>eModelImplementation</returns>
    private static eModelImplementation GetModelImplementation(string value)
    {
        return value switch
        {
            "GPU" => eModelImplementation.GPU,
            "Hybrid" => eModelImplementation.Hybrid,
            "Reference" => eModelImplementation.Reference,
            _ => eModelImplementation.GPU
        };
    }

    /// <summary>
    /// 列印時間（點）
    /// </summary>
    /// <param name="timeSpan">TimeSpan</param>
    /// <returns>字串</returns>
    public static string PrintTime(TimeSpan timeSpan) =>
        timeSpan.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture);

    /// <summary>
    /// 列印時間（逗號）
    /// </summary>
    /// <param name="timeSpan">TimeSpan</param>
    /// <returns>字串</returns>
    public static string PrintTimeWithComma(TimeSpan timeSpan) =>
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
    public void WriteLog(string message)
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