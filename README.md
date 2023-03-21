# 字幕檔產生器

簡易的字幕檔產生器，透過 [OpenAI](https://openai.com) 釋出的的 [Whisper](https://openai.com/research/whisper) 自動語音辨識系統，將選擇的視訊或音訊檔案的內容，轉譯成字幕檔案。

[GPGPU 實作](https://github.com/rubujo/SubtitleGenerator/tree/gpgpu)

## 使用流程

1. 在應用程式介面上選擇設定選項，以及輸入的檔案。
2. 將輸入的檔案，透過 [FFmpeg](https://ffmpeg.org/) 轉換成取樣率為 16 kHz 的 [WAV 格式](https://zh.wikipedia.org/zh-tw/WAV) 的音訊檔案。
3. 透過 [sandrohanea/whisper.net](https://github.com/sandrohanea/whisper.net) 函式庫啟動 [Whisper](https://openai.com/research/whisper)，以轉譯輸入的檔案。
4. 將轉譯的結果，依據選擇，產生成 [SubRip](https://zh.wikipedia.org/zh-tw/SubRip) Text 或 [WebVTT](https://developer.mozilla.org/zh-TW/docs/Web/API/WebVTT_API) 等格式的字幕檔案。
5. 刪除於 "2." 產生的音訊檔案。

## 注意事項

1. 本應用程式是基於 [sandrohanea/whisper.net](https://github.com/sandrohanea/whisper.net) 函式庫進行開發，因此只支援 [ggerganov/whisper.cpp](https://github.com/ggerganov/whisper.cpp) 函式庫所採用的 [ggml 格式](https://github.com/ggerganov/whisper.cpp/tree/master/models) 的模型檔案。
2. 因 [sandrohanea/whisper.net](https://github.com/sandrohanea/whisper.net) 函式庫的實作限制，可能會有部分 [ggerganov/whisper.cpp](https://github.com/ggerganov/whisper.cpp) 函式庫的功能不被支援。