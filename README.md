# 字幕檔產生器 (GPGPU)

簡易的字幕產生器，透過 OpenAI 的 Whisper，將選擇的視訊或音訊檔案的內容，轉譯成對應的字幕檔案。

## 作業流程

1. 選擇選項及檔案。
2. 使用 Whisper 轉譯該檔案。
3. 將 Whisper 轉譯的結果產生 SubRip Text 或 WebVTT 格式的字幕檔案。

## 注意事項

1. 本應用程式是基於 [Const-me/Whisper](https://github.com/Const-me/Whisper) 函式庫進行開發，因此只支援 [ggerganov/whisper.cpp](https://github.com/ggerganov/whisper.cpp) 函式庫所採用的 ggml 格式的模型檔案。