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
            BtnStart.TabIndex = 2;
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
            TBLog.TabIndex = 4;
            // 
            // CBLanguage
            // 
            CBLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            CBLanguage.FormattingEnabled = true;
            CBLanguage.Items.AddRange(new object[] { "中", "英", "日", "韓" });
            CBLanguage.Location = new Point(12, 41);
            CBLanguage.Name = "CBLanguage";
            CBLanguage.Size = new Size(121, 23);
            CBLanguage.TabIndex = 3;
            // 
            // BtnCancel
            // 
            BtnCancel.Location = new Point(713, 41);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(75, 23);
            BtnCancel.TabIndex = 5;
            BtnCancel.Text = "取消";
            BtnCancel.UseVisualStyleBackColor = true;
            BtnCancel.Click += BtnCancel_Click;
            // 
            // FMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(BtnCancel);
            Controls.Add(CBLanguage);
            Controls.Add(TBLog);
            Controls.Add(BtnStart);
            Controls.Add(BtnSelectInputFile);
            Controls.Add(TBInputFilePath);
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
    }
}