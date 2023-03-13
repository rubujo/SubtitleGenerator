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
            MessageBox.Show(
                ex.ToString(),
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
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
            MessageBox.Show(
                ex.ToString(),
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async void BtnStart_Click(object sender, EventArgs e)
    {
        Control[] ctrlSet1 =
        {
            TBInputFilePath,
            BtnSelectInputFile,
            BtnStart,
            CBModel,
            CBLanguage,
            CBTranslate,
            CBWebVTT
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
                    "請先選擇檔案。",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            ctrlSet1.SetEnabled(true);
            ctrlSet2.SetEnabled(false);

            cancellationTokenSource = new();
            cancellationToken = cancellationTokenSource.Token;

            TBLog.Clear();

            await DoTask(
                TBInputFilePath.Text,
                CBLanguage.Text,
                CBTranslate.Checked,
                CBWebVTT.Checked,
                GetModelType(CBModel.Text),
                cancellationToken);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            ctrlSet1.SetEnabled(true);
            ctrlSet2.SetEnabled(false);
        }
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
        try
        {
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}