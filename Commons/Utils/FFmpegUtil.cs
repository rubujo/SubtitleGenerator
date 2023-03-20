using SubtitleGenerator.Commons.Sets;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace SubtitleGenerator.Commons.Utils;

/// <summary>
/// FFmpeg 工具方法
/// </summary>
public class FFmpegUtil
{
    /// <summary>
    /// 檢查 FFmpeg
    /// </summary>
    /// <param name="form">FMain</param>
    /// <returns>Task</returns>
    public static async Task CheckFFmpeg(FMain form)
    {
        FFmpeg.SetExecutablesPath(FolderSet.BinsFolderPath);

        string ffpmegExePath = Path.Combine(FolderSet.BinsFolderPath, "ffmpeg.exe"),
            ffprobeExePath = Path.Combine(FolderSet.BinsFolderPath, "ffprobe.exe");

        if (!File.Exists(ffpmegExePath) ||
            !File.Exists(ffprobeExePath))
        {
            FMain.WriteLog(form, "FFmpeg 執行檔不存在，正在開始下載 FFmpeg 執行檔……");

            int tempPercent = 0;

            bool isFinished = false;

            Progress<ProgressInfo> progress = new();

            progress.ProgressChanged += (sender, e) =>
            {
                if (e.DownloadedBytes == 1 && e.TotalBytes == 1)
                {
                    // 避免重複輸出。
                    if (!isFinished)
                    {
                        isFinished = true;

                        FMain.WriteLog(form, "已下載 FFmpeg 執行檔。");
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

                        FMain.WriteLog(form, $"下載進度：{e.DownloadedBytes}/{e.TotalBytes} Bytes ({actulPercent}%)");
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
            FMain.WriteLog(form, "已找到 FFmpeg 執行檔。");
        }
    }

    /// <summary>
    /// 轉換成 WAV 檔案
    /// </summary>
    /// <param name="form">FMain</param>
    /// <param name="inputFilePath">字串，檔案的路徑</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task&lt;string&gt;，產生的 WAV 檔案的路徑</returns>
    public static async Task<string> ConvertToWavFile(
        FMain form,
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
                FMain.WriteLog(form, "發生錯誤：請選擇有效的視訊或音訊檔案。");
            }

            string tempFilePath = Path.Combine(
                FolderSet.TempFolderPath,
                $"{fileName}_{DateTime.Now:yyyyMMddHHmmssfff}.wav");

            // 轉換成取樣率為 16 kHz 的 WAV 檔案。
            IConversion conversion = FFmpeg.Conversions.New()
                .AddStream(audioStreams)
                // 來源：https://github.com/tigros/Whisperer/blob/dcdbcd8c9b01c06016272e4a6784774768b7b316/whisperer/Form1.cs#L220
                // TODO: 2023-03-20 需要再觀察下列參數適不適合。
                .AddParameter("-vn -ar 16000 -ac 1 -ab 32k -af volume=1.75")
                .SetOutputFormat(Format.wav)
                .SetOutput(tempFilePath)
                .SetOverwriteOutput(true);

            string parameter = conversion.Build();

            FMain.WriteLog(form, $"使用的參數：{parameter}");

            conversion.OnDataReceived += (sender, e) =>
            {
                FMain.WriteLog(form, e.Data?.ToString() ?? string.Empty);
            };

            conversion.OnProgress += (sender, args) =>
            {
                FMain.WriteLog(form, args?.ToString() ?? string.Empty);
            };

            IConversionResult conversionResult = await conversion.Start(cancellationToken);

            string result = $"FFmpeg 執行結果：{Environment.NewLine}" +
                $"開始時間：{conversionResult.StartTime}{Environment.NewLine}" +
                $"結束時間：{conversionResult.EndTime}{Environment.NewLine}" +
                $"耗時：{conversionResult.Duration}{Environment.NewLine}" +
                $"參數：{conversionResult.Arguments}";

            FMain.WriteLog(form, result);

            return tempFilePath;
        }
        catch (OperationCanceledException)
        {
            FMain.WriteLog(form, "已取消作業。");
        }
        catch (Exception ex)
        {
            FMain.ShowErrMsg(form, ex.ToString());
        }

        return string.Empty;
    }
}