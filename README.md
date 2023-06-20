# 字幕檔產生器 (CLBlast)

簡易的字幕檔產生器，透過 [sandrohanea/whisper.net](https://github.com/sandrohanea/whisper.net) 函式庫以使用 [OpenAI](https://openai.com) 所釋出的 [Whisper](https://openai.com/research/whisper) 自動語音辨識系統，將選擇的視訊或音訊檔案的聲音內容，轉譯成字幕檔案。

- [CPU 實作](https://github.com/rubujo/SubtitleGenerator/tree/main)
- [cuBLAS 實作](https://github.com/rubujo/SubtitleGenerator/tree/cuBLAS)
- [GPGPU 實作](https://github.com/rubujo/SubtitleGenerator/tree/GPGPU)

## 使用流程

1. 在應用程式介面上選擇設定選項，以及輸入的檔案。
2. 將輸入的檔案，透過 [FFmpeg](https://ffmpeg.org/) 轉換成取樣率為 16 kHz 的 [WAV 格式](https://zh.wikipedia.org/zh-tw/WAV) 的音訊檔案。
3. 透過 [sandrohanea/whisper.net](https://github.com/sandrohanea/whisper.net) 函式庫啟動 [Whisper](https://openai.com/research/whisper)，以轉譯輸入的檔案。
4. 將轉譯的結果，依據選擇，產生成 [SubRip](https://zh.wikipedia.org/zh-tw/SubRip) Text 或 [WebVTT](https://developer.mozilla.org/zh-TW/docs/Web/API/WebVTT_API) 等格式的字幕檔案。
5. 刪除於 "2." 產生的音訊檔案。

### 需要的動態連結程式庫檔案

- clblast.dll

※需要[下載](https://github.com/CNugteren/CLBlast/releases) [CLBlast](https://github.com/CNugteren/CLBlast) 以取得上述動態連結程式庫檔案。  

## 注意事項

1. 本應用程式是基於 [sandrohanea/whisper.net](https://github.com/sandrohanea/whisper.net) 函式庫進行開發，因此只支援 [ggerganov/whisper.cpp](https://github.com/ggerganov/whisper.cpp) 函式庫所採用的 [ggml 格式](https://github.com/ggerganov/whisper.cpp/tree/master/models) 的模型檔案。
2. 因 [sandrohanea/whisper.net](https://github.com/sandrohanea/whisper.net) 函式庫的實作限制，可能會有部分 [ggerganov/whisper.cpp](https://github.com/ggerganov/whisper.cpp) 函式庫的功能不被支援。

## 授權資訊

- [ggerganov/whisper.cpp](https://github.com/ggerganov/whisper.cpp)
   - Copyright (c) 2023  Georgi Gerganov
   - [MIT 授權條款](https://github.com/ggerganov/whisper.cpp/blob/master/LICENSE)
- [sandrohanea/whisper.net](https://github.com/sandrohanea/whisper.net)
   - Copyright (c) 2023 sandrohanea
   - [MIT 授權條款](https://github.com/sandrohanea/whisper.net/blob/main/LICENSE)
- [Xabe.FFmpeg](https://github.com/tomaszzmuda/Xabe.FFmpeg)
   - [授權合約](https://ffmpeg.xabe.net/license.html)
- [FFmpeg](https://ffmpeg.org/)
   - [FFmpeg 授權條款和法律方面的注意事項](https://ffmpeg.org/legal.html)

因 [Xabe.FFmpeg](https://github.com/tomaszzmuda/Xabe.FFmpeg) 函式庫[授權合約](https://ffmpeg.xabe.net/license.html)的限制，此 GitHub 倉庫內，`沒有標註來源`的內容，皆採用 [CC BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) 授權條款釋出，反之皆以其來源之授權條款為準。
