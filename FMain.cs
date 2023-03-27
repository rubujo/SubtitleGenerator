using SubtitleGenerator.Commons.Extensions;
using SubtitleGenerator.Commons.Utils;
using static SubtitleGenerator.Commons.Sets.EnumSet;
using Whisper;

namespace SubtitleGenerator;

public partial class FMain : Form
{
    public FMain()
    {
        InitializeComponent();
    }

    private void FMain_Load(object sender, EventArgs e)
    {
        try
        {
            CustomInit();
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }

    private void BtnSelectInputFile_Click(object sender, EventArgs e)
    {
        try
        {
            OpenFileDialog openFileDialog = new()
            {
                FilterIndex = 1,
                Filter = "視訊檔|*.mp4;*.mkv;*.webm|音訊檔|*.wav;*.m4a;*.webm;*.opus;*.ogg;*.mp3;*.mka;*.flac;",
                Title = "請選擇檔案"
            };

            DialogResult dialogResult = openFileDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                TBInputFilePath.Text = openFileDialog.FileName;
            }
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }

    private void CBLanguages_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            CBEnableTranslate_CheckedChanged(this, EventArgs.Empty);
            CBEnableOpenCCS2TWP_CheckedChanged(this, EventArgs.Empty);
            CBEnableOpenCCTW2SP_CheckedChanged(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }

    private void CBSamplingStrategies_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            // TODO: 2023-03-16 在 Const-me/Whisper 函式庫內尚未實作此策略。。
            if (CBSamplingStrategies.Text == "Beam search")
            {
                ShowErrMsg(this, "Const-me/Whisper 函式庫內尚未實作此抽樣策略。");
            }
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }

    private void CBEnableTranslate_CheckedChanged(object sender, EventArgs e)
    {
        try
        {
            // 因為英文不需要再翻譯成英文，故加入此限制。
            if (CBLanguages.Text == "en" && CBEnableTranslate.Checked)
            {
                CBEnableTranslate.Checked = false;

                ShowWarnMsg(this, "此選項僅於使用對非 en 語言時可以使用。");
            }
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }

    private void CBEnableOpenCCS2TWP_CheckedChanged(object sender, EventArgs e)
    {
        try
        {
            if (CBLanguages.Text != "zh" && CBEnableOpenCCS2TWP.Checked)
            {
                CBEnableOpenCCS2TWP.Checked = false;

                ShowWarnMsg(this, "此選項僅於使用 zh 語言時可以使用。");
            }
            else if (CBEnableOpenCCS2TWP.Checked && CBEnableOpenCCTW2SP.Checked)
            {
                CBEnableOpenCCTW2SP.Checked = false;
            }
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }

    private void CBEnableOpenCCTW2SP_CheckedChanged(object sender, EventArgs e)
    {
        try
        {
            if (CBLanguages.Text != "zh" && CBEnableOpenCCTW2SP.Checked)
            {
                CBEnableOpenCCTW2SP.Checked = false;

                ShowWarnMsg(this, "此選項僅於使用 zh 語言時可以使用。");
            }
            else if (CBEnableOpenCCTW2SP.Checked && CBEnableOpenCCS2TWP.Checked)
            {
                CBEnableOpenCCS2TWP.Checked = false;
            }
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }

    private async void BtnTranscribe_Click(object sender, EventArgs e)
    {
        Control[] ctrlSet1 =
        {
            TBInputFilePath,
            BtnSelectInputFile,
            BtnTranscribe,
            BtnMicTranscribe,
            CBModelImplementation,
            CBGPUs,
            CBGpuModelFlags,
            CBModels,
            CBSamplingStrategies,
            CBLanguages,
            CBCaptureDevices,
            CBEnableSpeedUpAudio,
            CBEnableTranslate,
            CBConvertToWav,
            CBEnableOpenCCS2TWP,
            CBEnableOpenCCTW2SP,
            CBExportWebVTT,
            BtnReset
        };

        Control[] ctrlSet2 =
        {
            BtnCancel
        };

        try
        {
            ctrlSet1.SetEnabled(false);
            ctrlSet2.SetEnabled(true);

            if (string.IsNullOrEmpty(TBInputFilePath.Text))
            {
                ShowWarnMsg(this, "請先選擇要轉譯的視訊或音訊檔案。");

                return;
            }

            GlobalCTS = new();
            GlobalCT = GlobalCTS.Token;
            SegmentDataSet.Clear();

            SetOpenCCVariables();

            TBLog.Clear();

            PBProgress.Style = ProgressBarStyle.Marquee;

            // 轉譯（檔案）。
            await WhisperUtil.Transcribe(
                form: this,
                inputFilePath: TBInputFilePath.Text,
                language: CBLanguages.Text,
                enableTranslate: CBEnableTranslate.Checked,
                enableSpeedUpAudio: CBEnableSpeedUpAudio.Checked,
                exportWebVTT: CBExportWebVTT.Checked,
                enableConvertToWav: CBConvertToWav.Checked,
                isStereo: true,
                useiAudioReader: true,
                useBufferFile: false,
                modelImplementation: WhisperUtil.GetModelImplementation(CBModelImplementation.Text),
                gpuModelFlags: WhisperUtil.GetGpuModelFlag(CBGpuModelFlags.Text),
                adapter: string.IsNullOrEmpty(CBGPUs.Text) ? null : CBGPUs.Text,
                ggmlType: WhisperUtil.GetModelType(CBModels.Text),
                samplingStrategyType: WhisperUtil.GetSamplingStrategyType(CBSamplingStrategies.Text),
                n_past: 0,
                n_best: 0,
                beam_width: 5,
                cancellationToken: GlobalCT);
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
        finally
        {
            ctrlSet1.SetEnabled(true);
            ctrlSet2.SetEnabled(false);

            PBProgress.Style = ProgressBarStyle.Blocks;

            SegmentDataSet.Clear();
        }
    }

    private async void BtnMicTranscribe_Click(object sender, EventArgs e)
    {
        Control[] ctrlSet1 =
        {
            TBInputFilePath,
            BtnSelectInputFile,
            BtnTranscribe,
            BtnMicTranscribe,
            CBModelImplementation,
            CBGPUs,
            CBGpuModelFlags,
            CBModels,
            CBSamplingStrategies,
            CBLanguages,
            CBCaptureDevices,
            CBEnableSpeedUpAudio,
            CBEnableTranslate,
            CBConvertToWav,
            CBEnableOpenCCS2TWP,
            CBEnableOpenCCTW2SP,
            CBExportWebVTT,
            BtnReset
        };

        Control[] ctrlSet2 =
        {
            BtnCancel
        };

        try
        {
            ctrlSet1.SetEnabled(false);
            ctrlSet2.SetEnabled(true);

            if (string.IsNullOrEmpty(CBCaptureDevices.Text))
            {
                ShowWarnMsg(this, "請先選擇您要使用的錄音裝置。");

                return;
            }

            CaptureDeviceId? captureDeviceId = WhisperUtil
                .GetCaptureDeviceId(CBCaptureDevices.Text);

            if (captureDeviceId == null)
            {
                ShowErrMsg(this, "發生錯誤：無法使用您已選擇的錄音裝置，請確認該錄音裝置已安裝完成。");

                return;
            }

            if (CBConvertToWav.Checked)
            {
                CBConvertToWav.Checked = false;

                ShowWarnMsg(this, "注意，此功能不支援此選項。");
            }

            GlobalCTS = new();
            GlobalCT = GlobalCTS.Token;
            SegmentDataSet.Clear();

            SetOpenCCVariables();

            TBLog.Clear();

            PBProgress.Style = ProgressBarStyle.Marquee;

            // TODO: 2023-03-20 待確認如何進行設定，目前已知 minDuration 或 maxDuration 的值為 0 時，會發生例外。

            // 轉譯（檔案）。
            await WhisperUtil.Transcribe(
                form: this,
                captureDeviceId: captureDeviceId.Value,
                language: CBLanguages.Text,
                enableTranslate: CBEnableTranslate.Checked,
                enableSpeedUpAudio: CBEnableSpeedUpAudio.Checked,
                exportWebVTT: CBExportWebVTT.Checked,
                isStereo: true,
                modelImplementation: WhisperUtil.GetModelImplementation(CBModelImplementation.Text),
                gpuModelFlags: WhisperUtil.GetGpuModelFlag(CBGpuModelFlags.Text),
                adapter: string.IsNullOrEmpty(CBGPUs.Text) ? null : CBGPUs.Text,
                ggmlType: WhisperUtil.GetModelType(CBModels.Text),
                dropStartSilence: 0.25f,
                maxDuration: 11f,
                minDuration: 7f,
                pauseDuration: 0.333f,
                samplingStrategyType: WhisperUtil.GetSamplingStrategyType(CBSamplingStrategies.Text),
                n_past: 0,
                n_best: 0,
                beam_width: 5,
                cancellationToken: GlobalCT);
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
        finally
        {
            ctrlSet1.SetEnabled(true);
            ctrlSet2.SetEnabled(false);

            PBProgress.Style = ProgressBarStyle.Blocks;

            SegmentDataSet.Clear();
        }
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
        try
        {
            if (!GlobalCTS.IsCancellationRequested)
            {
                GlobalCTS.Cancel();
            }
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }

    private void BtnReset_Click(object sender, EventArgs e)
    {
        try
        {
            // 重設變數。
            EnableOpenCC = false;
            GlobalOCCMode = OpenCCMode.None;
            SegmentDataSet.Clear();

            // 重設控制項。
            TBInputFilePath.Clear();
            CBModelImplementation.Text = "GPU";

            CBGPUs.DataSource = null;
            CBGPUs.DataSource = WhisperUtil.GetGpuList();

            if (CBGPUs.Items.Count > 0)
            {
                CBGPUs.SelectedIndex = 0;
            }

            CBGpuModelFlags.Text = "Wave32";
            CBModels.Text = "Small";
            CBSamplingStrategies.Text = "Default";
            CBLanguages.Text = "en";

            CBCaptureDevices.DataSource = null;
            CBCaptureDevices.DataSource = WhisperUtil.GetCaptureDeviceList();

            if (CBCaptureDevices.Items.Count > 0)
            {
                CBCaptureDevices.SelectedIndex = 0;
            }

            CBEnableSpeedUpAudio.Checked = false;
            CBEnableTranslate.Checked = false;
            CBConvertToWav.Checked = false;
            CBEnableOpenCCS2TWP.Checked = false;
            CBEnableOpenCCTW2SP.Checked = false;
            CBExportWebVTT.Checked = false;
            TBLog.Clear();
            LCaptureStatus.Text = string.Empty;
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }
}