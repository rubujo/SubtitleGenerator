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
            BtnStart = new Button();
            TBLog = new TextBox();
            CBLanguage = new ComboBox();
            BtnCancel = new Button();
            CBTranslate = new CheckBox();
            CBWebVTT = new CheckBox();
            CBModel = new ComboBox();
            SuspendLayout();
            // 
            // TBInputFilePath
            // 
            TBInputFilePath.Location = new Point(12, 12);
            TBInputFilePath.Name = "TBInputFilePath";
            TBInputFilePath.Size = new Size(614, 23);
            TBInputFilePath.TabIndex = 0;
            // 
            // BtnSelectInputFile
            // 
            BtnSelectInputFile.Location = new Point(632, 12);
            BtnSelectInputFile.Name = "BtnSelectInputFile";
            BtnSelectInputFile.Size = new Size(75, 23);
            BtnSelectInputFile.TabIndex = 1;
            BtnSelectInputFile.Text = "選擇檔案";
            BtnSelectInputFile.UseVisualStyleBackColor = true;
            BtnSelectInputFile.Click += BtnSelectInputFile_Click;
            // 
            // BtnStart
            // 
            BtnStart.Location = new Point(713, 12);
            BtnStart.Name = "BtnStart";
            BtnStart.Size = new Size(75, 23);
            BtnStart.TabIndex = 6;
            BtnStart.Text = "執行";
            BtnStart.UseVisualStyleBackColor = true;
            BtnStart.Click += BtnStart_Click;
            // 
            // TBLog
            // 
            TBLog.Location = new Point(12, 70);
            TBLog.Multiline = true;
            TBLog.Name = "TBLog";
            TBLog.ScrollBars = ScrollBars.Both;
            TBLog.Size = new Size(776, 368);
            TBLog.TabIndex = 8;
            // 
            // CBLanguage
            // 
            CBLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            CBLanguage.FormattingEnabled = true;
            CBLanguage.Items.AddRange(new object[] { "en", "zh", "de", "es", "ru", "ko", "fr", "ja", "pt", "tr", "pl", "ca", "nl", "ar", "sv", "it", "id", "hi", "fi", "vi", "iw", "uk", "el", "ms", "cs", "ro", "da", "hu", "ta", "no", "th", "ur", "hr", "bg", "lt", "la", "mi", "ml", "cy", "sk", "te", "fa", "lv", "bn", "sr", "az", "sl", "kn", "et", "mk", "br", "eu", "is", "hy", "ne", "mn", "bs", "kk", "sq", "sw", "gl", "mr", "pa", "si", "km", "sn", "yo", "so", "af", "oc", "ka", "be", "tg", "sd", "gu", "am", "yi", "lo", "uz", "fo", "ht", "ps", "tk", "nn", "mt", "sa", "lb", "my", "bo", "tl", "mg", "as", "tt", "haw", "ln", "ha", "ba", "jw", "su" });
            CBLanguage.Location = new Point(139, 41);
            CBLanguage.Name = "CBLanguage";
            CBLanguage.Size = new Size(121, 23);
            CBLanguage.TabIndex = 3;
            // 
            // BtnCancel
            // 
            BtnCancel.Location = new Point(713, 41);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(75, 23);
            BtnCancel.TabIndex = 7;
            BtnCancel.Text = "取消";
            BtnCancel.UseVisualStyleBackColor = true;
            BtnCancel.Click += BtnCancel_Click;
            // 
            // CBTranslate
            // 
            CBTranslate.AutoSize = true;
            CBTranslate.Location = new Point(266, 43);
            CBTranslate.Name = "CBTranslate";
            CBTranslate.Size = new Size(86, 19);
            CBTranslate.TabIndex = 4;
            CBTranslate.Text = "翻譯成英文";
            CBTranslate.UseVisualStyleBackColor = true;
            // 
            // CBWebVTT
            // 
            CBWebVTT.AutoSize = true;
            CBWebVTT.Location = new Point(358, 43);
            CBWebVTT.Name = "CBWebVTT";
            CBWebVTT.Size = new Size(102, 19);
            CBWebVTT.TabIndex = 5;
            CBWebVTT.Text = "產生 WebVTT";
            CBWebVTT.UseVisualStyleBackColor = true;
            // 
            // CBModel
            // 
            CBModel.DropDownStyle = ComboBoxStyle.DropDownList;
            CBModel.FormattingEnabled = true;
            CBModel.Items.AddRange(new object[] { "Tiny", "TinyEn", "Base", "BaseEn", "Small", "SmallEn", "Medium", "MediumEn", "LargeV1", "Large" });
            CBModel.Location = new Point(12, 41);
            CBModel.Name = "CBModel";
            CBModel.Size = new Size(121, 23);
            CBModel.TabIndex = 2;
            // 
            // FMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(CBModel);
            Controls.Add(CBWebVTT);
            Controls.Add(CBTranslate);
            Controls.Add(BtnCancel);
            Controls.Add(CBLanguage);
            Controls.Add(TBLog);
            Controls.Add(BtnStart);
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
        private Button BtnStart;
        private TextBox TBLog;
        private ComboBox CBLanguage;
        private Button BtnCancel;
        private CheckBox CBTranslate;
        private CheckBox CBWebVTT;
        private ComboBox CBModel;
    }
}