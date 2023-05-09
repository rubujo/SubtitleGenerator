// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Ggml;

/// <summary>
/// 自定義 WhisperGgmlDownloader
/// <para>來源：https://github.com/sandrohanea/whisper.net/blob/main/Whisper.net/Ggml/WhisperGgmlDownloader.cs </para>
/// </summary>
public static class CustomWhisperGgmlDownloader
{
    /// <summary>
    /// HttpClient
    /// </summary>
    private static readonly Lazy<HttpClient> httpClient = new(() => new HttpClient()
    {
        Timeout = Timeout.InfiniteTimeSpan
    });

    /// <summary>
    /// 取得 ggml 模型
    /// </summary>
    /// <param name="type">GgmlType</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task&lt;Stream&gt;</returns>
    /// <exception cref="ArgumentOutOfRangeException">null</exception>
    public static async Task<Stream> GetGgmlModelAsync(
        GgmlType type,
        CancellationToken cancellationToken = default)
    {
        string requestUri = type switch
        {
            GgmlType.Tiny => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-tiny.bin",
            GgmlType.TinyEn => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-tiny.en.bin",
            GgmlType.Base => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-base.bin",
            GgmlType.BaseEn => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-base.en.bin",
            GgmlType.Small => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-small.bin",
            GgmlType.SmallEn => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-small.en.bin",
            GgmlType.Medium => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-medium.bin",
            GgmlType.MediumEn => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-medium.en.bin",
            GgmlType.LargeV1 => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-large-v1.bin",
            GgmlType.Large => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-large.bin",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, requestUri);

        // 來源：https://stackoverflow.com/q/18720435
        HttpResponseMessage httpResponseMessage = await httpClient.Value.SendAsync(
            httpRequestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        httpResponseMessage.EnsureSuccessStatusCode();

#if IOS || MACCATALYST || TVOS || ANDROID || MACOS
        return await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
#else
        return await httpResponseMessage.Content.ReadAsStreamAsync(CancellationToken.None);
#endif
    }
}