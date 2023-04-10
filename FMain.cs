using SubtitleGenerator.Commons.Extensions;
using SubtitleGenerator.Commons.Sets;
using SubtitleGenerator.Commons.Utils;

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

    private async void BtnDetectLanguage_Click(object sender, EventArgs e)
    {
        Control[] ctrlSet1 =
        {
            TBInputFilePath,
            BtnSelectInputFile,
            BtnDetectLanguage,
            BtnTranscribe,
            CBModels,
            CBLanguages,
            CBEnableTranslate,
            CBSamplingStrategies,
            CBEnableSpeedUp2x,
            CBExportWebVTT,
            CBEnableOpenCCS2TWP,
            CBEnableOpenCCTW2SP,
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
                ShowWarnMsg(this, "請先選擇視訊或音訊檔案。");

                return;
            }

            ctrlSet1.SetEnabled(false);
            ctrlSet2.SetEnabled(true);

            GlobalCTS = new();
            GlobalCT = GlobalCTS.Token;

            TBLog.Clear();

            PBProgress.Style = ProgressBarStyle.Marquee;

            SetOpenCCVariables();

            // TODO: 2023-03-27 看未來是否開放 beamSize、patience 及 bestOf 等參數的設定。
            await WhisperUtil.DetectLanguage(
                form: this,
                inputFilePath: TBInputFilePath.Text,
                language: CBLanguages.Text,
                enableTranslate: CBEnableTranslate.Checked,
                enableSpeedUp2x: CBEnableSpeedUp2x.Checked,
                speedUp: false,
                ggmlType: WhisperUtil.GetModelType(CBModels.Text),
                samplingStrategyType: WhisperUtil.GetSamplingStrategyType(CBSamplingStrategies.Text),
                beamSize: 5,
                patience: -0.1f,
                bestOf: 1,
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

    private async void BtnTranscribe_Click(object sender, EventArgs e)
    {
        Control[] ctrlSet1 =
        {
            TBInputFilePath,
            BtnSelectInputFile,
            BtnDetectLanguage,
            BtnTranscribe,
            CBModels,
            CBLanguages,
            CBEnableTranslate,
            CBSamplingStrategies,
            CBEnableSpeedUp2x,
            CBExportWebVTT,
            CBEnableOpenCCS2TWP,
            CBEnableOpenCCTW2SP,
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
                ShowWarnMsg(this, "請先選擇視訊或音訊檔案。");

                return;
            }

            ctrlSet1.SetEnabled(false);
            ctrlSet2.SetEnabled(true);

            GlobalCTS = new();
            GlobalCT = GlobalCTS.Token;

            TBLog.Clear();

            PBProgress.Style = ProgressBarStyle.Marquee;

            SetOpenCCVariables();

            // TODO: 2023-03-27 看未來是否開放 beamSize、patience 及 bestOf 等參數的設定。
            await WhisperUtil.Transcribe(
                form: this,
                inputFilePath: TBInputFilePath.Text,
                language: CBLanguages.Text,
                enableTranslate: CBEnableTranslate.Checked,
                enableSpeedUp2x: CBEnableSpeedUp2x.Checked,
                exportWebVtt: CBExportWebVTT.Checked,
                ggmlType: WhisperUtil.GetModelType(CBModels.Text),
                samplingStrategyType: WhisperUtil.GetSamplingStrategyType(CBSamplingStrategies.Text),
                beamSize: 5,
                patience: -0.1f,
                bestOf: 1,
                cancellationToken: GlobalCT);

            // TODO: 2023-04-10 NAudio test code, not worked well.
            //await WhisperUtil.AudioTranscribe(
            //    form: this,
            //    deviceName: "",
            //    language: CBLanguages.Text,
            //    enableTranslate: CBEnableTranslate.Checked,
            //    enableSpeedUp2x: CBEnableSpeedUp2x.Checked,
            //    exportWebVtt: CBExportWebVTT.Checked,
            //    ggmlType: WhisperUtil.GetModelType(CBModels.Text),
            //    samplingStrategyType: WhisperUtil.GetSamplingStrategyType(CBSamplingStrategies.Text),
            //    beamSize: 5,
            //    patience: -0.1f,
            //    bestOf: 1,
            //    cancellationToken: GlobalCT);
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
            GlobalOCCMode = EnumSet.OpenCCMode.None;

            // 重設控制項。 
            TBInputFilePath.Clear();
            CBModels.Text = "Small";
            CBLanguages.Text = "auto";
            CBSamplingStrategies.Text = "Default";
            CBEnableSpeedUp2x.Checked = false;
            CBEnableTranslate.Checked = false;
            CBExportWebVTT.Checked = false;
            CBEnableOpenCCS2TWP.Checked = false;
            CBEnableOpenCCTW2SP.Checked = false;
            TBLog.Clear();
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }
}