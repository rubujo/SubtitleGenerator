using NAudio.Wave;
using SubtitleGenerator.Commons.Models;

namespace SubtitleGenerator.Commons.Utils;

/// <summary>
/// NAudio 工具
/// </summary>
public class NAudioUtil
{
    /// <summary>
    /// 支援的 WAVE 格式
    /// </summary>
    public static readonly SupportedWaveFormat SupportedWaveFormats =
        SupportedWaveFormat.WAVE_FORMAT_1M08 |
        SupportedWaveFormat.WAVE_FORMAT_1M16 |
        SupportedWaveFormat.WAVE_FORMAT_1S08 |
        SupportedWaveFormat.WAVE_FORMAT_1S16;

    /// <summary>
    /// 是否為無聲
    /// </summary>
    /// <param name="amplitude">float，振幅</param>
    /// <param name="threshold">sbyte，閾</param>
    /// <returns>布林值</returns>
    public static bool IsSilence(float amplitude, sbyte threshold)
        => GetDecibelsFromAmplitude(amplitude) < threshold;

    /// <summary>
    /// 從振幅中取得分貝值
    /// </summary>
    /// <param name="amplitude">float，振幅</param>
    /// <returns>double</returns>
    public static double GetDecibelsFromAmplitude(float amplitude)
        => 20 * Math.Log10(Math.Abs(amplitude));

    /// <summary>
    /// 取得音訊裝置清單
    /// </summary>
    /// <returns>List&lt;AudioDevice&gt;</returns>
    public static List<AudioDevice> GetAudioDeviceList()
    {
        List<AudioDevice> devices = new();

        int deviceCount = WaveIn.DeviceCount;

        for (int i = 0; i < deviceCount; i++)
        {
            WaveInCapabilities waveInCapabilities = WaveIn.GetCapabilities(i);

            if (waveInCapabilities.SupportsWaveFormat(SupportedWaveFormats))
            {
                devices.Add(new AudioDevice()
                {
                    Number = i,
                    Name = waveInCapabilities.ProductName
                });
            }
        }

        return devices;
    }

    /// <summary>
    /// 取得音訊裝置的號碼
    /// </summary>
    /// <param name="deviceName">字串，音訊裝置的名稱</param>
    /// <returns>數值</returns>
    public static int GetDeviceNumber(string deviceName)
    {
        int deviceCount = WaveIn.DeviceCount;

        for (int i = 0; i < deviceCount; i++)
        {
            WaveInCapabilities waveInCapabilities = WaveIn.GetCapabilities(i);

            if (waveInCapabilities.ProductName == deviceName &&
                waveInCapabilities.SupportsWaveFormat(SupportedWaveFormats))
            {
                return i;
            }
        }

        return -1;
    }
}