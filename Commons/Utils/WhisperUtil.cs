﻿using OpenCCNET;
using SubtitleGenerator.Commons.Sets;
using static SubtitleGenerator.Commons.Sets.EnumSet;
using System.Globalization;
using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.Wave;
using System.Diagnostics;

namespace SubtitleGenerator.Commons.Utils;

/// <summary>
/// Whisper 工具
/// </summary>
public class WhisperUtil
{
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
    /// 取得量化類型
    /// </summary>
    /// <param name="value">字串</param>
    /// <returns>QuantizationType</returns>
    public static QuantizationType GetQuantizationType(string value)
    {
        return value switch
        {
            "No Quantization" => QuantizationType.NoQuantization,
            "Q4_0" => QuantizationType.Q4_0,
            "Q4_1" => QuantizationType.Q4_1,
            "Q4_2" => QuantizationType.Q4_2,
            "Q5_0" => QuantizationType.Q5_0,
            "Q5_1" => QuantizationType.Q5_1,
            "Q8_0" => QuantizationType.Q8_0,
            _ => QuantizationType.NoQuantization
        };
    }

    /// <summary>
    /// 取得模型檔案的名稱
    /// </summary>
    /// <param name="ggmlType">GgmlType</param>
    /// <param name="quantizationType">QuantizationType，預設值為 QuantizationType.NoQuantization</param>
    /// <returns>字串</returns>
    public static string GetModelFileName(
        GgmlType ggmlType,
        QuantizationType quantizationType = QuantizationType.NoQuantization)
    {
        string mainFileName = ggmlType switch
        {
            GgmlType.Tiny => "ggml-tiny",
            GgmlType.TinyEn => "ggml-tiny.en",
            GgmlType.Base => "ggml-base",
            GgmlType.BaseEn => "ggml-base.en",
            GgmlType.Small => "ggml-small",
            GgmlType.SmallEn => "ggml-small.en",
            GgmlType.Medium => "ggml-medium",
            GgmlType.MediumEn => "ggml-medium.en",
            GgmlType.LargeV1 => "ggml-large-v1",
            GgmlType.Large => "ggml-large",
            _ => string.Empty
        },
        subFileName = quantizationType switch
        {
            QuantizationType.NoQuantization => string.Empty,
            QuantizationType.Q4_0 => "q4_0",
            QuantizationType.Q4_1 => "q4_1",
            QuantizationType.Q4_2 => "q4_2",
            QuantizationType.Q5_0 => "q5_0",
            QuantizationType.Q5_1 => "q5_1",
            QuantizationType.Q8_0 => "q8_0",
            _ => string.Empty
        },
        extName = ".bin";

        if (string.IsNullOrEmpty(mainFileName))
        {
            return string.Empty;
        }
        else
        {
            if (!string.IsNullOrEmpty(subFileName))
            {
                subFileName = $".{subFileName}";
            }

            return $"{mainFileName}{subFileName}{extName}";
        }
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
    /// 檢查模型檔案
    /// </summary>
    /// <param name="form">FMain</param>
    /// <param name="ggmlType">GgmlType，預設值為 GgmlType.Small</param>
    /// <param name="ggmlType">QuantizationType，預設值為 GgmlType.NoQuantization</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task&lt;string&gt;，模型檔案的路徑</returns>
    public static async Task<string> CheckModelFile(
        FMain form,
        GgmlType ggmlType = GgmlType.Small,
        QuantizationType quantizationType = QuantizationType.NoQuantization,
        CancellationToken cancellationToken = default)
    {
        string modelFilePath = Path.Combine(
                FolderSet.ModelsFolderPath,
                GetModelFileName(ggmlType, quantizationType)),
            modelFileName = Path.GetFileName(modelFilePath);

        try
        {
            // 判斷模型檔案是否存在。
            if (!File.Exists(modelFilePath))
            {
                FMain.WriteLog(form, $"模型檔案 {modelFileName} 不存在，正在開始下載該模型檔案……");

                using Stream stream = await WhisperGgmlDownloader.GetGgmlModelAsync(
                    ggmlType,
                    quantizationType,
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
            FMain.WriteLog(form, "已取消轉譯作業。");
        }
        catch (Exception ex)
        {
            modelFilePath = string.Empty;

            FMain.ShowErrMsg(form, ex.ToString());
        }

        return modelFilePath;
    }

    /// <summary>
    /// 偵測語言
    /// <para>因為會發生 System.AccessViolationException，故 speedUp 需設為 false。</para>
    /// </summary>
    /// <param name="form">FMain</param>
    /// <param name="inputFilePath">字串，檔案的路徑</param>
    /// <param name="language">字串，語言（兩碼），預設值為 "auto"</param>
    /// <param name="enableTranslate">布林值，啟用翻譯成英文，預設值為 false</param>
    /// <param name="enableSpeedUp2x">布林值，啟用 SpeedUp2x，預設值為 false</param>
    /// <param name="speedUp">布林值，是否加速，預設值為 false</param>
    /// <param name="ggmlType">GgmlType，預設值為 GgmlType.Small</param>
    /// <param name="quantizationType">QuantizationType，預設值為 QuantizationType.NoQuantization</param>
    /// <param name="samplingStrategyType">SamplingStrategyType，預設值為 SamplingStrategyType.Default</param>
    /// <param name="beamSize">beamSize，用於 SamplingStrategyType.BeamSearch，預設值為 5</param>
    /// <param name="patience">patience，用於 SamplingStrategyType.BeamSearch，預設值為 -0.1f</param>
    /// <param name="bestOf">bestOf，用於 SamplingStrategyType.Greedy，預設值為 1</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task</returns>
    public static async Task DetectLanguage(
        FMain form,
        string inputFilePath,
        string language = "auto",
        bool enableTranslate = false,
        bool enableSpeedUp2x = false,
        bool speedUp = false,
        GgmlType ggmlType = GgmlType.Small,
        QuantizationType quantizationType = QuantizationType.NoQuantization,
        SamplingStrategyType samplingStrategyType = SamplingStrategyType.Default,
        int beamSize = 5,
        float patience = -0.1f,
        int bestOf = 1,
        CancellationToken cancellationToken = default)
    {
        Stopwatch stopWatch = new();

        stopWatch.Start();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            string tempFilePath = await Task.Run(async () =>
            {
                string wavfilePath = await FFmpegUtil.ConvertToWavFile(
                        form,
                        inputFilePath,
                        cancellationToken),
                    modelFilePath = await CheckModelFile(
                        form,
                        ggmlType,
                        quantizationType,
                        cancellationToken);

                if (string.IsNullOrEmpty(modelFilePath))
                {
                    FMain.WriteLog(form, "發生錯誤：使用的模型檔案不存在或下載失敗。");
                    FMain.WriteLog(form, "已取消偵測語言作業。");
                    FMain.WriteLog(form, $"請自行至「{FolderSet.TempFolderPath}」刪除暫存檔案。");

                    return string.Empty;
                }

                FMain.WriteLog(form, "正在開始偵測語言作業……");
                FMain.WriteLog(form, $"使用的模型：{ggmlType}");
                FMain.WriteLog(form, $"使用的量化：{quantizationType}");

                using WhisperFactory whisperFactory = WhisperFactory.FromPath(modelFilePath);

                WhisperProcessorBuilder whisperProcessorBuilder = whisperFactory.CreateBuilder()
                    .WithEncoderBeginHandler(form.OnEncoderBegin)
                    .WithProgressHandler(form.OnProgress)
                    .WithSegmentEventHandler(form.OnNewSegment);

                if (language == "auto")
                {
                    whisperProcessorBuilder.WithLanguageDetection();
                }
                else
                {
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

                WhisperProcessor whisperProcessor = GetWhisperProcessor(
                    whisperProcessorBuilder: whisperProcessorBuilder,
                    samplingStrategyType: samplingStrategyType,
                    beamSize: beamSize,
                    patience: patience,
                    bestOf: bestOf);

                using FileStream fileStream = File.OpenRead(wavfilePath);

                WaveParser waveParser = new(fileStream);

                bool isTaskCanceled = false;

                try
                {
                    float[] avgSamples = await waveParser.GetAvgSamplesAsync(cancellationToken);

                    (string? detectedLanguage, float? probability) = whisperProcessor
                        .DetectLanguageWithProbability(samples: avgSamples, speedUp: speedUp);

                    string rawResult = string.IsNullOrEmpty(detectedLanguage) ?
                            "識別失敗。" :
                            $"{detectedLanguage}（{probability:P}）",
                        resultMessage = $"偵測語言結果：{rawResult}";

                    FMain.WriteLog(form, resultMessage);

                    FMain.ShowMsg(form, resultMessage);
                }
                catch (OperationCanceledException)
                {
                    isTaskCanceled = true;

                    stopWatch.Stop();

                    FMain.WriteLog(form, "已取消偵測語言作業。");
                    FMain.WriteLog(form, $"總共耗時：{stopWatch.Elapsed.ToFFmpeg()}");
                }

                await whisperProcessor.DisposeAsync();

                if (!isTaskCanceled)
                {
                    stopWatch.Stop();

                    FMain.WriteLog(form, $"總共耗時：{stopWatch.Elapsed.ToFFmpeg()}");
                }

                return wavfilePath;
            }, cancellationToken);

            if (!string.IsNullOrEmpty(tempFilePath) &&
                File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);

                FMain.WriteLog(form, $"已刪除暫時檔案：{tempFilePath}");
            }
        }
        catch (OperationCanceledException)
        {
            stopWatch.Stop();

            FMain.WriteLog(form, "已取消偵測語言作業。");
            FMain.WriteLog(form, $"總共耗時：{stopWatch.Elapsed.ToFFmpeg()}");
            FMain.WriteLog(form, $"請自行至「{FolderSet.TempFolderPath}」刪除暫存檔案。");
        }
        catch (Exception ex)
        {
            stopWatch.Stop();

            FMain.WriteLog(form, "已取消偵測語言作業。");
            FMain.WriteLog(form, $"總共耗時：{stopWatch.Elapsed.ToFFmpeg()}");
            FMain.WriteLog(form, $"請自行至「{FolderSet.TempFolderPath}」刪除暫存檔案。");

            FMain.ShowErrMsg(form, ex.ToString());
        }
    }

    /// <summary>
    /// 轉譯
    /// </summary>
    /// <param name="form">FMain</param>
    /// <param name="inputFilePath">字串，檔案的路徑</param>
    /// <param name="language">字串，語言（兩碼），預設值為 "auto"</param>
    /// <param name="enableTranslate">布林值，啟用翻譯成英文，預設值為 false</param>
    /// <param name="enableSpeedUp2x">布林值，啟用 SpeedUp2x，預設值為 false</param>
    /// <param name="exportWebVtt">布林值，匯出 WebVTT 格式，預設值為 false</param>
    /// <param name="ggmlType">GgmlType，預設值為 GgmlType.Small</param>
    /// <param name="quantizationType">QuantizationType，預設值為 QuantizationType.NoQuantization</param>
    /// <param name="samplingStrategyType">SamplingStrategyType，預設值為 SamplingStrategyType.Default</param>
    /// <param name="beamSize">beamSize，用於 SamplingStrategyType.BeamSearch，預設值為 5</param>
    /// <param name="patience">patience，用於 SamplingStrategyType.BeamSearch，預設值為 -0.1f</param>
    /// <param name="bestOf">bestOf，用於 SamplingStrategyType.Greedy，預設值為 1</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task</returns>
    public static async Task Transcribe(
        FMain form,
        string inputFilePath,
        string language = "auto",
        bool enableTranslate = false,
        bool enableSpeedUp2x = false,
        bool exportWebVtt = false,
        GgmlType ggmlType = GgmlType.Small,
        QuantizationType quantizationType = QuantizationType.NoQuantization,
        SamplingStrategyType samplingStrategyType = SamplingStrategyType.Default,
        int beamSize = 5,
        float patience = -0.1f,
        int bestOf = 1,
        CancellationToken cancellationToken = default)
    {
        Stopwatch stopWatch = new();

        stopWatch.Start();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            string tempFilePath = await Task.Run(async () =>
            {
                List<SegmentData> segmentDataSet = new();

                string wavfilePath = await FFmpegUtil.ConvertToWavFile(
                        form,
                        inputFilePath,
                        cancellationToken),
                    modelFilePath = await CheckModelFile(
                        form,
                        ggmlType,
                        quantizationType,
                        cancellationToken);

                if (string.IsNullOrEmpty(modelFilePath))
                {
                    FMain.WriteLog(form, "發生錯誤：使用的模型檔案不存在或下載失敗。");
                    FMain.WriteLog(form, "已取消轉譯作業。");
                    FMain.WriteLog(form, $"請自行至「{FolderSet.TempFolderPath}」刪除暫存檔案。");

                    return string.Empty;
                }

                FMain.WriteLog(form, "正在開始轉譯作業……");
                FMain.WriteLog(form, $"使用的模型：{ggmlType}");
                FMain.WriteLog(form, $"使用的量化：{quantizationType}");
                FMain.WriteLog(form, $"使用的語言：{language}");
                FMain.WriteLog(form, $"使用的抽樣策略：{samplingStrategyType}");
                FMain.WriteLog(form, $"使用 OpenCC：{(form.EnableOpenCC ? "是" : "否")}");
                FMain.WriteLog(form, $"OpenCC 模式：{form.GlobalOCCMode}");

                using WhisperFactory whisperFactory = WhisperFactory.FromPath(modelFilePath);

                WhisperProcessorBuilder whisperProcessorBuilder = whisperFactory.CreateBuilder()
                    .WithEncoderBeginHandler(form.OnEncoderBegin)
                    .WithProgressHandler(form.OnProgress)
                    .WithSegmentEventHandler(form.OnNewSegment)
                    .WithProbabilities();

                if (language == "auto")
                {
                    whisperProcessorBuilder.WithLanguageDetection();
                }
                else
                {
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

                WhisperProcessor whisperProcessor = GetWhisperProcessor(
                    whisperProcessorBuilder: whisperProcessorBuilder,
                    samplingStrategyType: samplingStrategyType,
                    beamSize: beamSize,
                    patience: patience,
                    bestOf: bestOf);

                using FileStream fileStream = File.OpenRead(wavfilePath);

                FMain.WriteLog(form, "轉譯的內容：");

                bool isTaskCanceled = false;

                try
                {
                    await foreach (SegmentData segmentData in whisperProcessor
                        .ProcessAsync(fileStream, cancellationToken))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        segmentDataSet.Add(segmentData);
                    }
                }
                catch (OperationCanceledException)
                {
                    isTaskCanceled = true;

                    stopWatch.Stop();

                    FMain.WriteLog(form, "已取消轉譯作業。");
                    FMain.WriteLog(form, $"總共耗時：{stopWatch.Elapsed.ToFFmpeg()}");
                }

                await whisperProcessor.DisposeAsync();

                if (!isTaskCanceled)
                {
                    stopWatch.Stop();

                    FMain.WriteLog(form, $"總共耗時：{stopWatch.Elapsed.ToFFmpeg()}");
                    FMain.WriteLog(form, "轉譯完成。");

                    // 建立字幕檔。
                    string subtitleFilePath = CreateSubtitleFile(
                            form,
                            segmentDataSet,
                            inputFilePath,
                            exportWebVtt),
                        subtitleFileName = Path.GetFileName(subtitleFilePath),
                        subtitleFileFolder = Path.GetFullPath(subtitleFilePath)
                            .Replace(subtitleFileName, string.Empty);

                    // 開啟資料夾。
                    FMain.OpenFolder(form, subtitleFileFolder);

                    FMain.ShowMsg(form, "轉譯完成。");
                }

                return wavfilePath;
            }, cancellationToken);

            if (!string.IsNullOrEmpty(tempFilePath) &&
                File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);

                FMain.WriteLog(form, $"已刪除暫時檔案：{tempFilePath}");
            }
        }
        catch (OperationCanceledException)
        {
            stopWatch.Stop();

            FMain.WriteLog(form, "已取消轉譯作業。");
            FMain.WriteLog(form, $"總共耗時：{stopWatch.Elapsed.ToFFmpeg()}");
            FMain.WriteLog(form, $"請自行至「{FolderSet.TempFolderPath}」刪除暫存檔案。");
        }
        catch (Exception ex)
        {
            stopWatch.Stop();

            FMain.WriteLog(form, "已取消轉譯作業。");
            FMain.WriteLog(form, $"總共耗時：{stopWatch.Elapsed.ToFFmpeg()}");
            FMain.WriteLog(form, $"請自行至「{FolderSet.TempFolderPath}」刪除暫存檔案。");

            FMain.ShowErrMsg(form, ex.ToString());
        }
    }

    /// <summary>
    /// 取得 WhisperProcessor
    /// </summary>
    /// <param name="whisperProcessorBuilder">WhisperProcessorBuilder</param>
    /// <param name="samplingStrategyType">SamplingStrategyType，預設值為 SamplingStrategyType.Default</param>
    /// <param name="beamSize">beamSize，用於 SamplingStrategyType.BeamSearch，預設值為 5</param>
    /// <param name="patience">patience，用於 SamplingStrategyType.BeamSearch，預設值為 -0.1f</param>
    /// <param name="bestOf">bestOf，用於 SamplingStrategyType.Greedy，預設值為 1</param>
    /// <returns>WhisperProcessor</returns>
    public static WhisperProcessor GetWhisperProcessor(
        WhisperProcessorBuilder whisperProcessorBuilder,
        SamplingStrategyType samplingStrategyType = SamplingStrategyType.Default,
        int beamSize = 5,
        float patience = -0.1f,
        int bestOf = 1)
    {
        WhisperProcessor whisperProcessor;

        switch (samplingStrategyType)
        {
            default:
            case SamplingStrategyType.Default:
                whisperProcessor = whisperProcessorBuilder.Build();

                break;
            case SamplingStrategyType.Greedy:
                GreedySamplingStrategyBuilder greedySamplingStrategyBuilder =
                    (GreedySamplingStrategyBuilder)whisperProcessorBuilder
                        .WithGreedySamplingStrategy();

                greedySamplingStrategyBuilder.WithBestOf(bestOf);

                whisperProcessor = greedySamplingStrategyBuilder
                    .ParentBuilder.Build();

                break;
            case SamplingStrategyType.BeamSearch:
                BeamSearchSamplingStrategyBuilder beamSearchSamplingStrategyBuilder =
                    (BeamSearchSamplingStrategyBuilder)whisperProcessorBuilder
                        .WithBeamSearchSamplingStrategy();

                beamSearchSamplingStrategyBuilder
                    .WithBeamSize(beamSize)
                    .WithPatience(patience);

                whisperProcessor = beamSearchSamplingStrategyBuilder
                    .ParentBuilder.Build();

                break;
        }

        return whisperProcessor;
    }

    /// <summary>
    /// 建立字幕檔案
    /// </summary>
    /// <param name="form">FMain</param>
    /// <param name="segmentDataSet">List&lt;SegmentData&gt;</param>
    /// <param name="inputFilePath">字串，檔案的路徑</param>
    /// <param name="exportWebVTT">布林值，匯出 WebVTT 格式，預設值為 false</param>
    /// <returns>字串，字幕檔案的路徑</returns>
    public static string CreateSubtitleFile(
        FMain form,
        List<SegmentData> segmentDataSet,
        string inputFilePath,
        bool exportWebVTT)
    {
        string filePath1 = Path.ChangeExtension(inputFilePath, ".srt");

        FMain.WriteLog(form, $"開始建立 SubRip Text 字幕檔……");

        using StreamWriter streamWriter1 = File.CreateText(filePath1);

        for (int i = 0; i < segmentDataSet.Count; i++)
        {
            streamWriter1.WriteLine(i + 1);

            SegmentData segmentData = segmentDataSet[i];

            string startTime = PrintTimeWithComma(segmentData.Start),
                endTime = PrintTimeWithComma(segmentData.End);

            streamWriter1.WriteLine("{0} --> {1}", startTime, endTime);
            streamWriter1.WriteLine(GetSegmentDataText(form, segmentData));
            streamWriter1.WriteLine();
        }

        FMain.WriteLog(form, $"已建立 SubRip Text 字幕檔：{filePath1}");

        #region WebVTT

        if (exportWebVTT)
        {
            string filePath2 = Path.ChangeExtension(inputFilePath, ".vtt");

            FMain.WriteLog(form, $"開始建立 WebVTT 字幕檔……");

            using StreamWriter streamWriter2 = File.CreateText(filePath2);

            streamWriter2.WriteLine("WEBVTT ");
            streamWriter2.WriteLine();

            for (int i = 0; i < segmentDataSet.Count; i++)
            {
                streamWriter2.WriteLine(i + 1);

                SegmentData segmentData = segmentDataSet[i];

                string startTime = PrintTime(segmentData.Start),
                    endTime = PrintTime(segmentData.End);

                streamWriter2.WriteLine("{0} --> {1}", startTime, endTime);
                streamWriter2.WriteLine(GetSegmentDataText(form, segmentData));
                streamWriter2.WriteLine();
            }

            FMain.WriteLog(form, $"已建立 WebVTT 字幕檔：{filePath2}");
        }

        #endregion

        return filePath1;
    }

    /// <summary>
    /// 取得 SegmentData 的文字
    /// </summary>
    /// <param name="form">FMain</param>
    /// <param name="segmentData">SegmentData</param>
    /// <returns>字串，文字內容</returns>
    public static string GetSegmentDataText(FMain form, SegmentData segmentData)
    {
        return form.EnableOpenCC ?
            form.GlobalOCCMode switch
            {
                OpenCCMode.None => segmentData.Text.TrimStart(),
                OpenCCMode.S2TWP => ZhConverter.HansToTW(segmentData.Text, true).TrimStart(),
                OpenCCMode.TW2SP => ZhConverter.TWToHans(segmentData.Text, true).TrimStart(),
                _ => segmentData.Text.TrimStart()
            } :
            segmentData.Text;
    }
}