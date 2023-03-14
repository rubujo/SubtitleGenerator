using SubtitleGenerator.Commons.Extensions;

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
            CBEnableSpeedUp2x_CheckedChanged(this, EventArgs.Empty);
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

    private void CBEnableSpeedUp2x_CheckedChanged(object sender, EventArgs e)
    {
        try
        {
            // 由於 Whisper.dll 會發生例外，故而加入此限制。
            if (CBLanguages.Text == "自動" && CBEnableSpeedUp2x.Checked)
            {
                CBEnableSpeedUp2x.Checked = false;

                ShowWarnMsg(this, "此選項僅於使用對非自動語言時可以使用。");
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
            CBModels,
            CBLanguages,
            CBEnableTranslate,
            CBSamplingStrategies,
            CBEnableSpeedUp2x,
            CBExportWebVTT,
            CBEnableOpenCCS2TWP,
            CBEnableOpenCCTW2SP,
            BtnStart,
            BtnDetectLanguage,
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
                MessageBox.Show(
                    "請先選擇視訊或音訊檔案。",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            DialogResult dialogResult = MessageBox.Show(
                "注意！此功能並不是很穩定，有機大機率會造成應用程式崩潰，若要繼續使用，請按「確定」按鈕以繼續。",
                Text,
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning);

            if (dialogResult == DialogResult.OK)
            {
                ctrlSet1.SetEnabled(false);
                ctrlSet2.SetEnabled(true);

                GlobalCTS = new();
                GlobalCT = GlobalCTS.Token;

                TBLog.Clear();

                PBProgress.Style = ProgressBarStyle.Marquee;

                SetOpenCCVariables();

                // TODO: 2023-03-14 功能表現不是很正常，常態性會回傳 en，有極大機率會發生例外。
                await DoLanguageDetection(
                    TBInputFilePath.Text,
                    GetModelType(CBModels.Text),
                    GlobalCT);
            }
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

    private async void BtnStart_Click(object sender, EventArgs e)
    {
        Control[] ctrlSet1 =
        {
            TBInputFilePath,
            BtnSelectInputFile,
            CBModels,
            CBLanguages,
            CBEnableTranslate,
            CBSamplingStrategies,
            CBEnableSpeedUp2x,
            CBExportWebVTT,
            CBEnableOpenCCS2TWP,
            CBEnableOpenCCTW2SP,
            BtnStart,
            BtnDetectLanguage,
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
                MessageBox.Show(
                    "請先選擇視訊或音訊檔案。",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            ctrlSet1.SetEnabled(false);
            ctrlSet2.SetEnabled(true);

            GlobalCTS = new();
            GlobalCT = GlobalCTS.Token;

            TBLog.Clear();

            PBProgress.Style = ProgressBarStyle.Marquee;

            SetOpenCCVariables();

            await DoFullDetection(
                TBInputFilePath.Text,
                CBLanguages.Text,
                CBEnableTranslate.Checked,
                CBEnableSpeedUp2x.Checked,
                CBExportWebVTT.Checked,
                GetModelType(CBModels.Text),
                GetSamplingStrategyType(CBSamplingStrategies.Text),
                GlobalCT);
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
            EnableOpenCC = false;
            GlobalOCCType = OpenCCType.None;

            TBInputFilePath.Clear();
            CBModels.Text = "中";
            CBLanguages.Text = "zh";
            CBSamplingStrategies.Text = "預設";
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