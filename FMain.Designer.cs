namespace SubtitleGenerator
{
    partial class FMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TBInputFilePath = new TextBox();
            BtnSelectInputFile = new Button();
            BtnTranscribe = new Button();
            TBLog = new TextBox();
            CBLanguages = new ComboBox();
            BtnCancel = new Button();
            CBEnableTranslate = new CheckBox();
            CBExportWebVTT = new CheckBox();
            CBModels = new ComboBox();
            LVersion = new Label();
            CBSamplingStrategies = new ComboBox();
            BtnReset = new Button();
            PBProgress = new ProgressBar();
            CBEnableOpenCCS2TWP = new CheckBox();
            CBEnableOpenCCTW2SP = new CheckBox();
            CBGPUs = new ComboBox();
            CBConvertToWav = new CheckBox();
            CBGpuModelFlags = new ComboBox();
            CBModelImplementation = new ComboBox();
            CBEnableSpeedUpAudio = new CheckBox();
            SuspendLayout();
            // 
            // TBInputFilePath
            // 
            TBInputFilePath.Location = new Point(12, 12);
            TBInputFilePath.Name = "TBInputFilePath";
            TBInputFilePath.Size = new Size(647, 23);
            TBInputFilePath.TabIndex = 0;
            // 
            // BtnSelectInputFile
            // 
            BtnSelectInputFile.Location = new Point(665, 12);
            BtnSelectInputFile.Name = "BtnSelectInputFile";
            BtnSelectInputFile.Size = new Size(75, 23);
            BtnSelectInputFile.TabIndex = 1;
            BtnSelectInputFile.Text = "選擇檔案";
            BtnSelectInputFile.UseVisualStyleBackColor = true;
            BtnSelectInputFile.Click += BtnSelectInputFile_Click;
            // 
            // BtnTranscribe
            // 
            BtnTranscribe.Location = new Point(746, 12);
            BtnTranscribe.Name = "BtnTranscribe";
            BtnTranscribe.Size = new Size(75, 23);
            BtnTranscribe.TabIndex = 13;
            BtnTranscribe.Text = "轉錄字幕檔";
            BtnTranscribe.UseVisualStyleBackColor = true;
            BtnTranscribe.Click += BtnTranscribe_Click;
            // 
            // TBLog
            // 
            TBLog.Location = new Point(12, 125);
            TBLog.Multiline = true;
            TBLog.Name = "TBLog";
            TBLog.ScrollBars = ScrollBars.Both;
            TBLog.Size = new Size(809, 313);
            TBLog.TabIndex = 16;
            // 
            // CBLanguages
            // 
            CBLanguages.DropDownStyle = ComboBoxStyle.DropDownList;
            CBLanguages.FormattingEnabled = true;
            CBLanguages.Items.AddRange(new object[] { "en", "zh", "de", "es", "ru", "ko", "fr", "ja", "pt", "tr", "pl", "ca", "nl", "ar", "sv", "it", "id", "hi", "fi", "vi", "iw", "uk", "el", "ms", "cs", "ro", "da", "hu", "ta", "no", "th", "ur", "hr", "bg", "lt", "la", "mi", "ml", "cy", "sk", "te", "fa", "lv", "bn", "sr", "az", "sl", "kn", "et", "mk", "br", "eu", "is", "hy", "ne", "mn", "bs", "kk", "sq", "sw", "gl", "mr", "pa", "si", "km", "sn", "yo", "so", "af", "oc", "ka", "be", "tg", "sd", "gu", "am", "yi", "lo", "uz", "fo", "ht", "ps", "tk", "nn", "mt", "sa", "lb", "my", "bo", "tl", "mg", "as", "tt", "haw", "ln", "ha", "ba", "jw", "su" });
            CBLanguages.Location = new Point(266, 71);
            CBLanguages.Name = "CBLanguages";
            CBLanguages.Size = new Size(121, 23);
            CBLanguages.TabIndex = 6;
            CBLanguages.SelectedIndexChanged += CBLanguages_SelectedIndexChanged;
            // 
            // BtnCancel
            // 
            BtnCancel.Location = new Point(746, 41);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(75, 23);
            BtnCancel.TabIndex = 14;
            BtnCancel.Text = "取消";
            BtnCancel.UseVisualStyleBackColor = true;
            BtnCancel.Click += BtnCancel_Click;
            // 
            // CBEnableTranslate
            // 
            CBEnableTranslate.AutoSize = true;
            CBEnableTranslate.Location = new Point(540, 73);
            CBEnableTranslate.Name = "CBEnableTranslate";
            CBEnableTranslate.Size = new Size(86, 19);
            CBEnableTranslate.TabIndex = 8;
            CBEnableTranslate.Text = "翻譯成英文";
            CBEnableTranslate.UseVisualStyleBackColor = true;
            CBEnableTranslate.CheckedChanged += CBEnableTranslate_CheckedChanged;
            // 
            // CBExportWebVTT
            // 
            CBExportWebVTT.AutoSize = true;
            CBExportWebVTT.Location = new Point(411, 98);
            CBExportWebVTT.Name = "CBExportWebVTT";
            CBExportWebVTT.Size = new Size(129, 19);
            CBExportWebVTT.TabIndex = 12;
            CBExportWebVTT.Text = "匯出 WebVTT 格式";
            CBExportWebVTT.UseVisualStyleBackColor = true;
            // 
            // CBModels
            // 
            CBModels.DropDownStyle = ComboBoxStyle.DropDownList;
            CBModels.FormattingEnabled = true;
            CBModels.Items.AddRange(new object[] { "Tiny", "Tiny En", "Base", "Base En", "Small", "Small En", "Medium", "Medium En", "Large V1", "Large" });
            CBModels.Location = new Point(12, 71);
            CBModels.Name = "CBModels";
            CBModels.Size = new Size(121, 23);
            CBModels.TabIndex = 4;
            // 
            // LVersion
            // 
            LVersion.AutoSize = true;
            LVersion.Location = new Point(12, 448);
            LVersion.Name = "LVersion";
            LVersion.Size = new Size(43, 15);
            LVersion.TabIndex = 17;
            LVersion.Text = "版本：";
            // 
            // CBSamplingStrategies
            // 
            CBSamplingStrategies.DropDownStyle = ComboBoxStyle.DropDownList;
            CBSamplingStrategies.FormattingEnabled = true;
            CBSamplingStrategies.Items.AddRange(new object[] { "Default", "Greedy", "Beam search" });
            CBSamplingStrategies.Location = new Point(139, 70);
            CBSamplingStrategies.Name = "CBSamplingStrategies";
            CBSamplingStrategies.Size = new Size(121, 23);
            CBSamplingStrategies.TabIndex = 5;
            CBSamplingStrategies.SelectedIndexChanged += CBSamplingStrategies_SelectedIndexChanged;
            // 
            // BtnReset
            // 
            BtnReset.Location = new Point(641, 444);
            BtnReset.Name = "BtnReset";
            BtnReset.Size = new Size(75, 23);
            BtnReset.TabIndex = 15;
            BtnReset.Text = "重設";
            BtnReset.UseVisualStyleBackColor = true;
            BtnReset.Click += BtnReset_Click;
            // 
            // PBProgress
            // 
            PBProgress.Location = new Point(722, 444);
            PBProgress.Name = "PBProgress";
            PBProgress.Size = new Size(100, 23);
            PBProgress.TabIndex = 18;
            // 
            // CBEnableOpenCCS2TWP
            // 
            CBEnableOpenCCS2TWP.AutoSize = true;
            CBEnableOpenCCS2TWP.Location = new Point(111, 99);
            CBEnableOpenCCS2TWP.Name = "CBEnableOpenCCS2TWP";
            CBEnableOpenCCS2TWP.Size = new Size(144, 19);
            CBEnableOpenCCS2TWP.TabIndex = 10;
            CBEnableOpenCCS2TWP.Text = "啟用 OpenCC S2TWP";
            CBEnableOpenCCS2TWP.UseVisualStyleBackColor = true;
            CBEnableOpenCCS2TWP.CheckedChanged += CBEnableOpenCCS2TWP_CheckedChanged;
            // 
            // CBEnableOpenCCTW2SP
            // 
            CBEnableOpenCCTW2SP.AutoSize = true;
            CBEnableOpenCCTW2SP.Location = new Point(261, 99);
            CBEnableOpenCCTW2SP.Name = "CBEnableOpenCCTW2SP";
            CBEnableOpenCCTW2SP.Size = new Size(144, 19);
            CBEnableOpenCCTW2SP.TabIndex = 11;
            CBEnableOpenCCTW2SP.Text = "啟用 OpenCC TW2SP";
            CBEnableOpenCCTW2SP.UseVisualStyleBackColor = true;
            CBEnableOpenCCTW2SP.CheckedChanged += CBEnableOpenCCTW2SP_CheckedChanged;
            // 
            // CBGPUs
            // 
            CBGPUs.DropDownStyle = ComboBoxStyle.DropDownList;
            CBGPUs.FormattingEnabled = true;
            CBGPUs.Location = new Point(139, 41);
            CBGPUs.Name = "CBGPUs";
            CBGPUs.Size = new Size(358, 23);
            CBGPUs.TabIndex = 2;
            // 
            // CBConvertToWav
            // 
            CBConvertToWav.AutoSize = true;
            CBConvertToWav.Location = new Point(12, 100);
            CBConvertToWav.Name = "CBConvertToWav";
            CBConvertToWav.Size = new Size(93, 19);
            CBConvertToWav.TabIndex = 9;
            CBConvertToWav.Text = "轉換成 WAV";
            CBConvertToWav.UseVisualStyleBackColor = true;
            // 
            // CBGpuModelFlags
            // 
            CBGpuModelFlags.DropDownStyle = ComboBoxStyle.DropDownList;
            CBGpuModelFlags.FormattingEnabled = true;
            CBGpuModelFlags.Items.AddRange(new object[] { "Wave32", "Wave64", "Wave32 (重塑矩陣乘法)", "Wave64 (重塑矩陣乘法)" });
            CBGpuModelFlags.Location = new Point(503, 41);
            CBGpuModelFlags.Name = "CBGpuModelFlags";
            CBGpuModelFlags.Size = new Size(156, 23);
            CBGpuModelFlags.TabIndex = 3;
            // 
            // CBModelImplementation
            // 
            CBModelImplementation.DropDownStyle = ComboBoxStyle.DropDownList;
            CBModelImplementation.FormattingEnabled = true;
            CBModelImplementation.Items.AddRange(new object[] { "GPU", "Hybrid", "Reference" });
            CBModelImplementation.Location = new Point(12, 41);
            CBModelImplementation.Name = "CBModelImplementation";
            CBModelImplementation.Size = new Size(121, 23);
            CBModelImplementation.TabIndex = 1;
            // 
            // CBSpeedUpAudio
            // 
            CBEnableSpeedUpAudio.AutoSize = true;
            CBEnableSpeedUpAudio.Location = new Point(393, 73);
            CBEnableSpeedUpAudio.Name = "CBSpeedUpAudio";
            CBEnableSpeedUpAudio.Size = new Size(141, 19);
            CBEnableSpeedUpAudio.TabIndex = 7;
            CBEnableSpeedUpAudio.Text = "使用 SpeedUpAudio";
            CBEnableSpeedUpAudio.UseVisualStyleBackColor = true;
            // 
            // FMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(834, 476);
            Controls.Add(CBEnableSpeedUpAudio);
            Controls.Add(CBModelImplementation);
            Controls.Add(CBGpuModelFlags);
            Controls.Add(CBConvertToWav);
            Controls.Add(CBGPUs);
            Controls.Add(CBEnableOpenCCTW2SP);
            Controls.Add(CBEnableOpenCCS2TWP);
            Controls.Add(PBProgress);
            Controls.Add(BtnReset);
            Controls.Add(CBSamplingStrategies);
            Controls.Add(LVersion);
            Controls.Add(CBModels);
            Controls.Add(CBExportWebVTT);
            Controls.Add(CBEnableTranslate);
            Controls.Add(BtnCancel);
            Controls.Add(CBLanguages);
            Controls.Add(TBLog);
            Controls.Add(BtnTranscribe);
            Controls.Add(BtnSelectInputFile);
            Controls.Add(TBInputFilePath);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "FMain";
            Text = "字幕檔產生器";
            Load += FMain_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox TBInputFilePath;
        private Button BtnSelectInputFile;
        private Button BtnTranscribe;
        private TextBox TBLog;
        private ComboBox CBLanguages;
        private Button BtnCancel;
        private CheckBox CBEnableTranslate;
        private CheckBox CBExportWebVTT;
        private ComboBox CBModels;
        private Label LVersion;
        private ComboBox CBSamplingStrategies;
        private Button BtnReset;
        private ProgressBar PBProgress;
        private CheckBox CBEnableOpenCCS2TWP;
        private CheckBox CBEnableOpenCCTW2SP;
        private ComboBox CBGPUs;
        private CheckBox CBConvertToWav;
        private ComboBox CBGpuModelFlags;
        private ComboBox CBModelImplementation;
        private CheckBox CBEnableSpeedUpAudio;
    }
}