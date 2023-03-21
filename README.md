# 字幕檔產生器 (GPGPU)

簡易的字幕產生器，透過 [OpenAI](https://openai.com) 釋出的的 [Whisper](https://openai.com/research/whisper) 自動語音辨識系統，將選擇的視訊或音訊檔案的內容，轉譯成字幕檔案。

## 使用流程

1. 在應用程式介面上選擇設定選項，以及輸入的檔案。
2. 透過 [Const-me/Whisper](https://github.com/Const-me/Whisper) 函式庫啟動 [Whisper](https://openai.com/research/whisper)，以轉譯輸入的檔案。
3. 將轉譯的結果，依據選擇，產生成 [SubRip](https://zh.wikipedia.org/zh-tw/SubRip) Text 或 [WebVTT](https://developer.mozilla.org/zh-TW/docs/Web/API/WebVTT_API) 等格式的字幕檔案。

## 注意事項

1. 本應用程式是基於 [Const-me/Whisper](https://github.com/Const-me/Whisper) 函式庫進行開發，因此只支援 [ggerganov/whisper.cpp](https://github.com/ggerganov/whisper.cpp) 函式庫所採用的 [ggml 格式](https://github.com/ggerganov/whisper.cpp/tree/master/models) 的模型檔案。
2. 因 [Const-me/Whisper](https://github.com/Const-me/Whisper) 函式庫的實作限制，可能會有部分 [ggerganov/whisper.cpp](https://github.com/ggerganov/whisper.cpp) 函式庫的功能不被支援。
3. `錄音轉譯`功能的效能不佳，請謹慎使用。