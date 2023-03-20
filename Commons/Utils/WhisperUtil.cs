using OpenCCNET;
using SubtitleGenerator.Commons.Sets;
using static SubtitleGenerator.Commons.Sets.EnumSet;
using System.Diagnostics;
using System.Globalization;
using Whisper;
using Whisper.net.Ggml;

namespace SubtitleGenerator.Commons.Utils;

/// <summary>
/// Whisper 工具方法
/// </summary>
public class WhisperUtil
{
    /// <summary>
    /// 共用的 eLoggerFlags
    /// </summary>
    public const eLoggerFlags SharedLoggerFlags =
        eLoggerFlags.UseStandardError |
        eLoggerFlags.SkipFormatMessage;

    /// <summary>
    /// 設定 LogSink
    /// </summary>
    /// <param name="form">FMain</param>
    /// <param name="logLevel">eLogLevel，預設值為 eLogLevel.Debug</param>
    public static void SetLogSink(
        FMain form,
        eLogLevel logLevel = eLogLevel.Debug)
    {
        Library.setLogSink(logLevel, SharedLoggerFlags, GetPFNLogMessage(form));
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
    /// 取得抽樣策略類型
    /// </summary>
    /// <param name="value">字串</param>
    /// <returns>EnumSet.SamplingStrategyType</returns>
    public static SamplingStrategyType GetSamplingStrategyType(string value)
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
    public static GgmlType GetModelType(string value)
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
    public static string GetModelFileName(GgmlType ggmlType)
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
    public static eGpuModelFlags GetGpuModelFlag(string value)
    {
        return value switch
        {
            "Wave32" => eGpuModelFlags.Wave32 | eGpuModelFlags.NoReshapedMatMul,
            "Wave32 (可克隆)" => eGpuModelFlags.Wave32 | eGpuModelFlags.NoReshapedMatMul | eGpuModelFlags.Cloneable,
            "Wave32 (重塑矩陣乘法)" => eGpuModelFlags.Wave32 | eGpuModelFlags.UseReshapedMatMul,
            "Wave32 (重塑矩陣乘法、可克隆)" => eGpuModelFlags.Wave32 | eGpuModelFlags.UseReshapedMatMul | eGpuModelFlags.Cloneable,
            "Wave64" => eGpuModelFlags.Wave64 | eGpuModelFlags.NoReshapedMatMul,
            "Wave64 (可克隆)" => eGpuModelFlags.Wave64 | eGpuModelFlags.NoReshapedMatMul | eGpuModelFlags.Cloneable,
            "Wave64 (重塑矩陣乘法)" => eGpuModelFlags.Wave64 | eGpuModelFlags.UseReshapedMatMul,
            "Wave64 (重塑矩陣乘法、可克隆)" => eGpuModelFlags.Wave64 | eGpuModelFlags.UseReshapedMatMul | eGpuModelFlags.Cloneable,
            _ => eGpuModelFlags.None
        };
    }

    /// <summary>
    /// 取得模型實作
    /// </summary>
    /// <param name="value">字串</param>
    /// <returns>eModelImplementation</returns>
    public static eModelImplementation GetModelImplementation(string value)
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
    /// 取得 GPU 裝置清單
    /// </summary>
    /// <returns>字串陣列</returns>
    public static string[] GetGpuList()
    {
        return Library.listGraphicAdapters();
    }

    /// <summary>
    /// 取得 pfnLogMessage
    /// </summary>
    /// <param name="from">FMain</param>
    /// <returns>pfnLogMessage</returns>
    public static pfnLogMessage GetPFNLogMessage(FMain from)
    {
        return new((level, message) =>
        {
            FMain.WriteLog(from, $"[{level}] {message}");
        });
    }

    /// <summary>
    /// 取得 pfnProgress
    /// </summary>
    /// <param name="from">FMain</param>
    /// <returns>Action&lt;double&gt;</returns>
    public static Action<double> GetPFNProgress(FMain from)
    {
        int tempPercent = 0;

        return new Action<double>(value =>
        {
            double actualPercent = Math.Round(value * 100, 1, MidpointRounding.AwayFromZero);

            // 減速機制。
            int parsePercent = Convert.ToInt32(actualPercent);

            if (parsePercent > tempPercent)
            {
                FMain.WriteLog(from, $"進度：{actualPercent}%");

                tempPercent = parsePercent;
            }
        });
    }

    /// <summary>
    /// 檢查模型檔案
    /// </summary>
    /// <param name="from">FMain</param>
    /// <param name="ggmlType">GgmlType，預設值為 GgmlType.Small</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task&lt;string&gt;，模型檔案的路徑</returns>
    public static async Task<string> CheckModelFile(
        FMain form,
        GgmlType ggmlType = GgmlType.Small,
        CancellationToken cancellationToken = default)
    {
        string modelFilePath = Path.Combine(
                FolderSet.ModelsFolderPath,
                GetModelFileName(ggmlType)),
            modelFileName = Path.GetFileName(modelFilePath);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            // 判斷模型檔案是否存在。
            if (!File.Exists(modelFilePath))
            {
                FMain.WriteLog(form, $"模型檔案 {modelFileName} 不存在，正在開始下載該模型檔案……");

                using Stream stream = await CustomWhisperGgmlDownloader.GetGgmlModelAsync(
                    ggmlType,
                    cancellationToken);
                using FileStream fileStream = File.OpenWrite(modelFilePath);

                await stream.CopyToAsync(fileStream, cancellationToken).ContinueWith(task =>
                {
                    FMain.WriteLog(form, $"已下載模型檔案 {modelFileName}。");
                }, cancellationToken);
            }
            else
            {
                FMain.WriteLog(form, $"已找到模型檔案 {modelFileName}。");
            }
        }
        catch (OperationCanceledException)
        {
            FMain.WriteLog(form, "已取消作業。");
        }
        catch (Exception ex)
        {
            modelFilePath = string.Empty;

            FMain.ShowErrMsg(form, ex.ToString());
        }

        return modelFilePath;
    }

    /// <summary>
    /// 轉譯
    /// </summary>
    /// <param name="form">FMain</param>
    /// <param name="inputFilePath">字串，檔案的路徑</param>
    /// <param name="language">字串，語言（兩碼），預設值為 "en"</param>
    /// <param name="enableTranslate">布林值，啟用翻譯成英文，預設值為 false</param>
    /// <param name="enableSpeedUpAudio">布林值，啟用 SpeedUpAudio，預設值為 false</param>
    /// <param name="exportWebVTT">布林值，匯出 WebVTT 格式，預設值為 false</param>
    /// <param name="enableConvertToWav">布林值，啟用轉換成 WAV 檔案，預設值為 false</param>
    /// <param name="isStereo">布林值，是否為立體聲，預設值為 true</param>
    /// <param name="modelImplementation">eModelImplementation，預設值為 eModelImplementation.GPU</param>
    /// <param name="gpuModelFlags">eGpuModelFlags，預設值為 eGpuModelFlags.None</param>
    /// <param name="adapter">字串，GPU 裝置的名稱，預設值為 null</param>
    /// <param name="ggmlType">GgmlType，預設值為 GgmlType.Small</param>
    /// <param name="samplingStrategyType">SamplingStrategyType，預設值為 SamplingStrategyType.Default</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task</returns>
    public static async Task Transcribe(
        FMain form,
        string inputFilePath,
        string language = "en",
        bool enableTranslate = false,
        bool enableSpeedUpAudio = false,
        bool exportWebVTT = false,
        bool enableConvertToWav = false,
        bool isStereo = true,
        eModelImplementation modelImplementation = eModelImplementation.GPU,
        eGpuModelFlags gpuModelFlags = eGpuModelFlags.None,
        string? adapter = null,
        GgmlType ggmlType = GgmlType.Small,
        SamplingStrategyType samplingStrategyType = SamplingStrategyType.Default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            Stopwatch stopWatch = new();

            stopWatch.Start();

            string tempFilePath = await Task.Run(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                string filePath = enableConvertToWav ?
                    await FFmpegUtil.ConvertToWavFile(form, inputFilePath, cancellationToken) :
                    inputFilePath,
                    modelFilePath = await CheckModelFile(form, ggmlType, cancellationToken),
                    modelFileName = Path.GetFileName(modelFilePath);

                if (string.IsNullOrEmpty(modelFilePath))
                {
                    FMain.WriteLog(form, $"發生錯誤：使用的模型檔案 {modelFileName} 不存在或下載失敗。");
                    FMain.WriteLog(form, "已取消轉譯作業。");

                    if (enableConvertToWav)
                    {
                        FMain.WriteLog(form, $"請自行至「{FolderSet.TempFolderPath}」刪除暫存檔案。");
                    }

                    return string.Empty;
                }

                FMain.WriteLog(form, $"使用的模型實作：{modelImplementation}");
                FMain.WriteLog(form, $"使用的 GPU 裝置：{adapter ?? "預設"}");
                FMain.WriteLog(form, $"使用的 GPU 模型旗標：{gpuModelFlags}");
                FMain.WriteLog(form, $"使用的模型：{ggmlType}");
                FMain.WriteLog(form, $"使用的語言：{language}");
                FMain.WriteLog(form, $"使用的抽樣策略：{samplingStrategyType}");
                FMain.WriteLog(form, $"使用 OpenCC：{(form.EnableOpenCC ? "是" : "否")}");
                FMain.WriteLog(form, $"OpenCC 模式：{form.GlobalOCCMode}");
                FMain.WriteLog(form, $"正在開始載入模型檔案 {modelFileName} ……");

                using iModel model = await Library.loadModelAsync(
                    path: modelFilePath,
                    cancelToken: cancellationToken,
                    flags: gpuModelFlags,
                    adapter: adapter,
                    pfnProgress: GetPFNProgress(form),
                    impl: modelImplementation);
                using Context context = GetContext(
                    model: model,
                    language: language,
                    enableTranslate: enableTranslate,
                    enableSpeedUpAudio: enableSpeedUpAudio,
                    samplingStrategyType: samplingStrategyType);
                using iMediaFoundation mediaFoundation = Library.initMediaFoundation();
                using iAudioBuffer audioBuffer = mediaFoundation.loadAudioFile(
                    path: filePath,
                    stereo: isStereo);

                FMain.WriteLog(form, "正在開始轉譯作業……");

                CustomCallbacks customCallbacks = new(form, cancellationToken);

                context.runFull(
                    buffer: audioBuffer,
                    callbacks: customCallbacks);

                FMain.WriteLog(form, "轉譯作業完成。");

                // 建立字幕檔。
                string subtitleFilePath = CreateSubtitleFile(
                        form,
                        context,
                        inputFilePath,
                        exportWebVTT),
                    subtitleFileName = Path.GetFileName(subtitleFilePath),
                    subtitleFileFolder = Path.GetFullPath(subtitleFilePath)
                        .Replace(subtitleFileName, string.Empty);

                stopWatch.Stop();

                FMain.WriteLog(form, $"總共耗時：{stopWatch.Elapsed.ToFFmpeg()}");

                FMain.ShowMsg(form, "轉譯作業完成。");

                // 開啟字幕檔所位於的資料夾。
                FMain.OpenFolder(form, subtitleFileFolder);

                return filePath;
            }, cancellationToken);

            // 當有啟用轉換成 WAV 檔案時才需要刪除暫時檔案。
            if (enableConvertToWav)
            {
                if (!string.IsNullOrEmpty(tempFilePath) &&
                    File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);

                    FMain.WriteLog(form, $"已刪除暫時檔案：{tempFilePath}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            FMain.WriteLog(form, "已取消轉譯作業。");

            if (enableConvertToWav)
            {
                FMain.WriteLog(form, $"請自行至「{FolderSet.TempFolderPath}」刪除暫存檔案。");
            }
        }
        catch (ApplicationException ae)
        {
            FMain.WriteLog(form, "已取消轉譯作業。");

            if (enableConvertToWav)
            {
                FMain.WriteLog(form, $"請自行至「{FolderSet.TempFolderPath}」刪除暫存檔案。");
            }

            FMain.ShowErrMsg(form, ae.Message);
        }
        catch (Exception ex)
        {
            FMain.WriteLog(form, "已取消轉譯作業。");

            if (enableConvertToWav)
            {
                FMain.WriteLog(form, $"請自行至「{FolderSet.TempFolderPath}」刪除暫存檔案。");
            }

            FMain.ShowErrMsg(form, ex.ToString());
        }
    }

    /// <summary>
    /// 轉譯
    /// </summary>
    /// <param name="form">FMain</param>
    /// <param name="captureDeviceId">CaptureDeviceId</param>
    /// <param name="language">字串，語言（兩碼），預設值為 "en"</param>
    /// <param name="enableTranslate">布林值，啟用翻譯成英文，預設值為 false</param>
    /// <param name="enableSpeedUpAudio">布林值，啟用 SpeedUpAudio，預設值為 false</param>
    /// <param name="exportWebVTT">布林值，匯出 WebVTT 格式，預設值為 false</param>
    /// <param name="isStereo">布林值，是否為立體聲，預設值為 true</param>
    /// <param name="modelImplementation">eModelImplementation，預設值為 eModelImplementation.GPU</param>
    /// <param name="gpuModelFlags">eGpuModelFlags，預設值為 eGpuModelFlags.None</param>
    /// <param name="adapter">字串，GPU 裝置的名稱，預設值為 null</param>
    /// <param name="ggmlType">GgmlType，預設值為 GgmlType.Small</param>
    /// <param name="samplingStrategyType">SamplingStrategyType，預設值為 SamplingStrategyType.Default</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task</returns>
    public static async Task Transcribe(
        FMain form,
        CaptureDeviceId captureDeviceId,
        string language = "en",
        bool enableTranslate = false,
        bool enableSpeedUpAudio = false,
        bool exportWebVTT = false,
        bool isStereo = true,
        eModelImplementation modelImplementation = eModelImplementation.GPU,
        eGpuModelFlags gpuModelFlags = eGpuModelFlags.None,
        string? adapter = null,
        GgmlType ggmlType = GgmlType.Small,
        SamplingStrategyType samplingStrategyType = SamplingStrategyType.Default,
        CancellationToken cancellationToken = default)
    {
        Context? context = null;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Run(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                string modelFilePath = await CheckModelFile(form, ggmlType, cancellationToken),
                    modelFileName = Path.GetFileName(modelFilePath);

                if (string.IsNullOrEmpty(modelFilePath))
                {
                    FMain.WriteLog(form, $"發生錯誤：使用的模型檔案 {modelFileName} 不存在或下載失敗。");
                    FMain.WriteLog(form, "已取消轉譯作業。");
                }

                FMain.WriteLog(form, $"使用的模型實作：{modelImplementation}");
                FMain.WriteLog(form, $"使用的 GPU 裝置：{adapter ?? "預設"}");
                FMain.WriteLog(form, $"使用的 GPU 模型旗標：{gpuModelFlags}");
                FMain.WriteLog(form, $"使用的模型：{ggmlType}");
                FMain.WriteLog(form, $"使用的語言：{language}");
                FMain.WriteLog(form, $"使用的抽樣策略：{samplingStrategyType}");
                FMain.WriteLog(form, $"使用 OpenCC：{(form.EnableOpenCC ? "是" : "否")}");
                FMain.WriteLog(form, $"OpenCC 模式：{form.GlobalOCCMode}");
                FMain.WriteLog(form, $"正在開始載入模型檔案 {modelFileName} ……");

                using iModel model = await Library.loadModelAsync(
                    path: modelFilePath,
                    cancelToken: cancellationToken,
                    flags: gpuModelFlags,
                    adapter: adapter,
                    pfnProgress: GetPFNProgress(form),
                    impl: modelImplementation);
                using Context context = GetContext(
                    model: model,
                    language: language,
                    enableTranslate: enableTranslate,
                    enableSpeedUpAudio: enableSpeedUpAudio,
                    samplingStrategyType: samplingStrategyType);
                using iMediaFoundation mediaFoundation = Library.initMediaFoundation();

                sCaptureParams captureParams = new();

                if (isStereo)
                {
                    captureParams.flags = eCaptureFlags.Stereo;
                }

                using iAudioCapture audioCapture = mediaFoundation
                    .openCaptureDevice(captureDeviceId, captureParams);

                CustomCallbacks customCallbacks = new(
                    fMain: form,
                    cancellationToken: cancellationToken);

                CustomCaptureCallbacks customCaptureCallbacks = new(
                    fMain: form,
                    cancellationToken: cancellationToken);

                context.runCapture(
                    capture: audioCapture,
                    callbacks: customCallbacks,
                    captureCallbacks: customCaptureCallbacks);
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            FMain.WriteLog(form, "已取消轉譯作業。");

            if (context == null)
            {
                FMain.WriteLog(form, "[Debug] context is null.");
            }

            if (context != null)
            {
                // 建立字幕檔。
                string subtitleFilePath = CreateSubtitleFile(
                        form,
                        context,
                        Path.Combine(FolderSet.TempFolderPath, Path.GetRandomFileName()),
                        exportWebVTT),
                    subtitleFileName = Path.GetFileName(subtitleFilePath),
                    subtitleFileFolder = Path.GetFullPath(subtitleFilePath)
                        .Replace(subtitleFileName, string.Empty);

                // 開啟字幕檔所位於的資料夾。
                FMain.OpenFolder(form, subtitleFileFolder);
            }
        }
        catch (ApplicationException ae)
        {
            FMain.WriteLog(form, "已取消轉譯作業。");

            FMain.ShowErrMsg(form, ae.Message);
        }
        catch (Exception ex)
        {
            FMain.WriteLog(form, "已取消轉譯作業。");

            FMain.ShowErrMsg(form, ex.ToString());
        }
    }

    /// <summary>
    /// 取得 Context
    /// </summary>
    /// <param name="model">iModel</param>
    /// <param name="language">字串，語言（兩碼），預設值為 "en"</param>
    /// <param name="n_past">數值，未知用途，預設值為 0</param>
    /// <param name="n_best">數值，未知用途，預設值為 0</param>
    /// <param name="beam_width">數值，用於 Beam search，預設值為 5</param>
    /// <param name="enableTranslate">布林值，啟用翻譯成英文，預設值為 false</param>
    /// <param name="enableSpeedUpAudio">布林值，啟用 SpeedUpAudio，預設值為 false</param>
    /// <param name="samplingStrategyType">SamplingStrategyType，預設值為 SamplingStrategyType.Default</param>
    /// <returns>Context</returns>
    public static Context GetContext(
        iModel model,
        string language = "en",
        int n_past = 0,
        int n_best = 0,
        int beam_width = 5,
        bool enableTranslate = false,
        bool enableSpeedUpAudio = false,
        SamplingStrategyType samplingStrategyType = SamplingStrategyType.Default)
    {
        Context context = model.createContext();

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
                break;
            case SamplingStrategyType.Greedy:
                context.parameters.strategy = eSamplingStrategy.Greedy;

                context.parameters.greedy = new Parameters.sGreedy()
                {
                    n_past = n_past
                };

                break;
            case SamplingStrategyType.BeamSearch:
                // TODO: 2023-03-16 在 Const-me/Whisper 函式庫內尚未實作此抽樣策略。
                context.parameters.strategy = eSamplingStrategy.BeamSearch;

                context.parameters.beamSearch = new Parameters.sBeamSearch()
                {
                    n_past = n_past,
                    n_best = n_best,
                    beam_width = beam_width
                };

                break;
        }

        context.timingsPrint();

        return context;
    }

    /// <summary>
    /// 取得 sSegment 的文字
    /// </summary>
    /// <param name="form">FMain</param>
    /// <param name="segment">sSegment</param>
    /// <returns>字串，文字內容</returns>
    public static string GetSegmentText(FMain form, sSegment segment)
    {
        if (string.IsNullOrEmpty(segment.text))
        {
            return string.Empty;
        }

        return form.EnableOpenCC ?
            form.GlobalOCCMode switch
            {
                OpenCCMode.None => segment.text,
                OpenCCMode.S2TWP => ZhConverter.HansToTW(segment.text, true),
                OpenCCMode.TW2SP => ZhConverter.TWToHans(segment.text, true),
                _ => segment.text
            } :
            segment.text;
    }

    /// <summary>
    /// 建立字幕檔
    /// </summary>
    /// <param name="form">FMain</param>
    /// <param name="context">Context</param>
    /// <param name="inputFilePath">字串，檔案的路徑</param>
    /// <param name="exportWebVTT">布林值，匯出 WebVTT 格式，預設值為 false</param>
    /// <returns>字串，字幕檔案的路徑</returns>
    public static string CreateSubtitleFile(
        FMain form,
        Context context,
        string inputFilePath,
        bool exportWebVTT)
    {
        string extName = exportWebVTT ? ".vtt" : ".srt",
            filePath = Path.ChangeExtension(inputFilePath, extName),
            fileType = exportWebVTT ? "WebVTT" : "SubRip Text";

        FMain.WriteLog(form, $"開始建立 {fileType} 字幕檔……");

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
            streamWriter.WriteLine(GetSegmentText(form, segment));
            streamWriter.WriteLine();
        }

        FMain.WriteLog(form, $"已建立 {fileType} 字幕檔：{filePath}");

        return filePath;
    }
}