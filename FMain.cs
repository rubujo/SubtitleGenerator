using SubtitleGenerator.Commons.Extensions;
using SubtitleGenerator.Commons.Utils;
using static SubtitleGenerator.Commons.Sets.EnumSet;

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
            CBModelImplementation,
            CBGPUs,
            CBGpuModelFlags,
            CBModels,
            CBSamplingStrategies,
            CBLanguages,
            CBEnableSpeedUpAudio,
            CBEnableTranslate,
            CBConvertToWav,
            CBEnableOpenCCS2TWP,
            CBEnableOpenCCTW2SP,
            CBExportWebVTT,
            BtnTranscribe,
            BtnReset
        };

        Control[] ctrlSet2 =
        {
            BtnCancel
        };

        try
        {
            if (string.IsNullOrEmpty(TBInputFilePath.Text))
            {
                ShowWarnMsg(this, "請先選擇要轉譯的視訊或音訊檔案。");

                return;
            }

            ctrlSet1.SetEnabled(false);
            ctrlSet2.SetEnabled(true);

            GlobalCTS = new();
            GlobalCT = GlobalCTS.Token;

            TBLog.Clear();

            PBProgress.Style = ProgressBarStyle.Marquee;

            SetOpenCCVariables();

            // 轉譯。
            await WhisperUtil.Transcribe(
                form: this,
                inputFilePath: TBInputFilePath.Text,
                language: CBLanguages.Text,
                enableTranslate: CBEnableTranslate.Checked,
                enableSpeedUpAudio: CBEnableSpeedUpAudio.Checked,
                exportWebVTT: CBExportWebVTT.Checked,
                enableConvertToWav: CBConvertToWav.Checked,
                isStereo: true,
                modelImplementation: WhisperUtil.GetModelImplementation(CBModelImplementation.Text),
                gpuModelFlags: WhisperUtil.GetGpuModelFlag(CBGpuModelFlags.Text),
                adapter: string.IsNullOrEmpty(CBGPUs.Text) ? null : CBGPUs.Text,
                ggmlType: WhisperUtil.GetModelType(CBModels.Text),
                samplingStrategyType: WhisperUtil.GetSamplingStrategyType(CBSamplingStrategies.Text),
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

            // 重設控制項。 
            TBInputFilePath.Clear();
            CBModelImplementation.Text = "GPU";

            if (CBGPUs.Items.Count > 0)
            {
                CBGPUs.SelectedIndex = 0;
            }

            CBGpuModelFlags.Text = "Wave32";
            CBModels.Text = "Small";
            CBSamplingStrategies.Text = "Default";
            CBLanguages.Text = "zh";
            CBEnableSpeedUpAudio.Checked = false;
            CBEnableTranslate.Checked = false;
            CBConvertToWav.Checked = false;
            CBEnableOpenCCS2TWP.Checked = false;
            CBEnableOpenCCTW2SP.Checked = false;
            CBExportWebVTT.Checked = false;
            TBLog.Clear();
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }
}