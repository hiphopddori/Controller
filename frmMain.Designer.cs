namespace IqaController
{
    partial class frmMain
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnService = new System.Windows.Forms.Button();
            this.btnConvertorRun = new System.Windows.Forms.Button();
            this.btnConvertorPatch = new System.Windows.Forms.Button();
            this.btnWindowFinder = new System.Windows.Forms.Button();
            this.btnTerminal = new System.Windows.Forms.Button();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dgvProcessLog = new IqaController.DataGridViewEx();
            this.dgvProcess = new IqaController.DataGridViewEx();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dgvErrorFile = new IqaController.DataGridViewEx();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.dgvEnv = new IqaController.DataGridViewEx();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.dgvDrmFileSend = new IqaController.DataGridViewEx();
            this.cboFtpServer = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ucLoadingBar = new IqaController.ucLoadingBar();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.btnWorkQueCOnfirm = new System.Windows.Forms.Button();
            this.tabMain.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProcessLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProcess)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvErrorFile)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEnv)).BeginInit();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDrmFileSend)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnService
            // 
            this.btnService.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnService.Location = new System.Drawing.Point(864, 12);
            this.btnService.Name = "btnService";
            this.btnService.Size = new System.Drawing.Size(114, 23);
            this.btnService.TabIndex = 0;
            this.btnService.Text = "서비스 off 상태";
            this.btnService.UseVisualStyleBackColor = true;
            this.btnService.Click += new System.EventHandler(this.btnService_Click);
            // 
            // btnConvertorRun
            // 
            this.btnConvertorRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConvertorRun.Location = new System.Drawing.Point(384, 12);
            this.btnConvertorRun.Name = "btnConvertorRun";
            this.btnConvertorRun.Size = new System.Drawing.Size(114, 23);
            this.btnConvertorRun.TabIndex = 1;
            this.btnConvertorRun.Text = "컨버터 실행";
            this.btnConvertorRun.UseVisualStyleBackColor = true;
            this.btnConvertorRun.Click += new System.EventHandler(this.btnConvertorRun_Click);
            // 
            // btnConvertorPatch
            // 
            this.btnConvertorPatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConvertorPatch.Location = new System.Drawing.Point(504, 12);
            this.btnConvertorPatch.Name = "btnConvertorPatch";
            this.btnConvertorPatch.Size = new System.Drawing.Size(114, 23);
            this.btnConvertorPatch.TabIndex = 2;
            this.btnConvertorPatch.Text = "컨버터 Patch 방법";
            this.btnConvertorPatch.UseVisualStyleBackColor = true;
            // 
            // btnWindowFinder
            // 
            this.btnWindowFinder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnWindowFinder.Location = new System.Drawing.Point(624, 12);
            this.btnWindowFinder.Name = "btnWindowFinder";
            this.btnWindowFinder.Size = new System.Drawing.Size(114, 23);
            this.btnWindowFinder.TabIndex = 3;
            this.btnWindowFinder.Text = "윈도우 탐색기";
            this.btnWindowFinder.UseVisualStyleBackColor = true;
            this.btnWindowFinder.Click += new System.EventHandler(this.btnWindowFinder_Click);
            // 
            // btnTerminal
            // 
            this.btnTerminal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTerminal.Location = new System.Drawing.Point(744, 12);
            this.btnTerminal.Name = "btnTerminal";
            this.btnTerminal.Size = new System.Drawing.Size(114, 23);
            this.btnTerminal.TabIndex = 4;
            this.btnTerminal.Text = "명령프롬프트";
            this.btnTerminal.UseVisualStyleBackColor = true;
            this.btnTerminal.Click += new System.EventHandler(this.btnTerminal_Click);
            // 
            // tabMain
            // 
            this.tabMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabMain.Controls.Add(this.tabPage1);
            this.tabMain.Controls.Add(this.tabPage2);
            this.tabMain.Controls.Add(this.tabPage3);
            this.tabMain.Controls.Add(this.tabPage4);
            this.tabMain.Controls.Add(this.tabPage5);
            this.tabMain.Location = new System.Drawing.Point(4, 42);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(1008, 688);
            this.tabMain.TabIndex = 5;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dgvProcessLog);
            this.tabPage1.Controls.Add(this.dgvProcess);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1000, 662);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "파일처리현황";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dgvProcessLog
            // 
            this.dgvProcessLog.AllowUserToAddRows = false;
            this.dgvProcessLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvProcessLog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProcessLog.Location = new System.Drawing.Point(6, 322);
            this.dgvProcessLog.Name = "dgvProcessLog";
            this.dgvProcessLog.RowTemplate.Height = 23;
            this.dgvProcessLog.Size = new System.Drawing.Size(988, 334);
            this.dgvProcessLog.TabIndex = 3;
            // 
            // dgvProcess
            // 
            this.dgvProcess.AllowUserToAddRows = false;
            this.dgvProcess.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvProcess.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProcess.Location = new System.Drawing.Point(6, 6);
            this.dgvProcess.Name = "dgvProcess";
            this.dgvProcess.RowTemplate.Height = 23;
            this.dgvProcess.Size = new System.Drawing.Size(988, 310);
            this.dgvProcess.TabIndex = 2;
            this.dgvProcess.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvProcess_CellClick);
            this.dgvProcess.Click += new System.EventHandler(this.dgvProcess_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dgvErrorFile);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1000, 662);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Error 파일";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dgvErrorFile
            // 
            this.dgvErrorFile.AllowUserToAddRows = false;
            this.dgvErrorFile.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvErrorFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvErrorFile.Location = new System.Drawing.Point(3, 3);
            this.dgvErrorFile.Name = "dgvErrorFile";
            this.dgvErrorFile.RowTemplate.Height = 23;
            this.dgvErrorFile.Size = new System.Drawing.Size(994, 656);
            this.dgvErrorFile.TabIndex = 0;
            this.dgvErrorFile.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvErrorFile_CellClick);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.dgvEnv);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1000, 662);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "환경설정";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // dgvEnv
            // 
            this.dgvEnv.AllowUserToAddRows = false;
            this.dgvEnv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvEnv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvEnv.Location = new System.Drawing.Point(0, 0);
            this.dgvEnv.Name = "dgvEnv";
            this.dgvEnv.RowTemplate.Height = 23;
            this.dgvEnv.Size = new System.Drawing.Size(1000, 662);
            this.dgvEnv.TabIndex = 0;
            this.dgvEnv.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvEnv_CellClick);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.dgvDrmFileSend);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(1000, 662);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "drm 파일전송";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // dgvDrmFileSend
            // 
            this.dgvDrmFileSend.AllowUserToAddRows = false;
            this.dgvDrmFileSend.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDrmFileSend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDrmFileSend.Location = new System.Drawing.Point(0, 0);
            this.dgvDrmFileSend.Name = "dgvDrmFileSend";
            this.dgvDrmFileSend.RowTemplate.Height = 23;
            this.dgvDrmFileSend.Size = new System.Drawing.Size(1000, 662);
            this.dgvDrmFileSend.TabIndex = 0;
            // 
            // cboFtpServer
            // 
            this.cboFtpServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFtpServer.FormattingEnabled = true;
            this.cboFtpServer.Location = new System.Drawing.Point(42, 13);
            this.cboFtpServer.Name = "cboFtpServer";
            this.cboFtpServer.Size = new System.Drawing.Size(102, 20);
            this.cboFtpServer.TabIndex = 6;
            this.cboFtpServer.SelectedIndexChanged += new System.EventHandler(this.cboFtpServer_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "서버";
            // 
            // ucLoadingBar
            // 
            this.ucLoadingBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ucLoadingBar.Location = new System.Drawing.Point(367, 346);
            this.ucLoadingBar.Name = "ucLoadingBar";
            this.ucLoadingBar.Size = new System.Drawing.Size(337, 43);
            this.ucLoadingBar.TabIndex = 10;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.cboFtpServer);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnConvertorRun);
            this.groupBox1.Controls.Add(this.btnTerminal);
            this.groupBox1.Controls.Add(this.btnService);
            this.groupBox1.Controls.Add(this.btnWindowFinder);
            this.groupBox1.Controls.Add(this.btnConvertorPatch);
            this.groupBox1.Location = new System.Drawing.Point(4, -5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(991, 41);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.btnWorkQueCOnfirm);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(1000, 662);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "개발자모드";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // btnWorkQueCOnfirm
            // 
            this.btnWorkQueCOnfirm.Location = new System.Drawing.Point(7, 6);
            this.btnWorkQueCOnfirm.Name = "btnWorkQueCOnfirm";
            this.btnWorkQueCOnfirm.Size = new System.Drawing.Size(84, 23);
            this.btnWorkQueCOnfirm.TabIndex = 0;
            this.btnWorkQueCOnfirm.Text = "작업큐 확인";
            this.btnWorkQueCOnfirm.UseVisualStyleBackColor = true;
            this.btnWorkQueCOnfirm.Click += new System.EventHandler(this.btnWorkQueCOnfirm_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 730);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.ucLoadingBar);
            this.Controls.Add(this.tabMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmMain";
            this.Text = "IOA 컨트롤러";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.Resize += new System.EventHandler(this.frmMain_Resize);
            this.tabMain.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvProcessLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProcess)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvErrorFile)).EndInit();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvEnv)).EndInit();
            this.tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDrmFileSend)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnService;
        private System.Windows.Forms.Button btnConvertorRun;
        private System.Windows.Forms.Button btnConvertorPatch;
        private System.Windows.Forms.Button btnWindowFinder;
        private System.Windows.Forms.Button btnTerminal;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private DataGridViewEx dgvEnv;
        private DataGridViewEx dgvProcess;
        private System.Windows.Forms.ComboBox cboFtpServer;
        private System.Windows.Forms.Label label1;
        private DataGridViewEx dgvProcessLog;
        private DataGridViewEx dgvErrorFile;
        private System.Windows.Forms.TabPage tabPage4;
        private DataGridViewEx dgvDrmFileSend;
        private ucLoadingBar ucLoadingBar;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.Button btnWorkQueCOnfirm;
    }
}

