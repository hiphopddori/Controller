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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnService = new System.Windows.Forms.Button();
            this.btnConvertorRun = new System.Windows.Forms.Button();
            this.btnConvertorPatch = new System.Windows.Forms.Button();
            this.btnWindowFinder = new System.Windows.Forms.Button();
            this.btnTerminal = new System.Windows.Forms.Button();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.btnConvertorError = new System.Windows.Forms.Button();
            this.btnWorkDirDel = new System.Windows.Forms.Button();
            this.btnAbnormalApply = new System.Windows.Forms.Button();
            this.btnAbnormalSearch = new System.Windows.Forms.Button();
            this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.btnWorkQueCOnfirm = new System.Windows.Forms.Button();
            this.cboFtpServer = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ucLoadingBar = new IqaController.ucLoadingBar();
            this.dgvProcessLog = new IqaController.DataGridViewEx();
            this.dgvProcess = new IqaController.DataGridViewEx();
            this.dgvErrorFile = new IqaController.DataGridViewEx();
            this.dgvEnv = new IqaController.DataGridViewEx();
            this.dgvDrmFileSend = new IqaController.DataGridViewEx();
            this.dgvAbnormalZipfile = new IqaController.DataGridViewEx();
            this.tabMain.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProcessLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProcess)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvErrorFile)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEnv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDrmFileSend)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAbnormalZipfile)).BeginInit();
            this.SuspendLayout();
            // 
            // btnService
            // 
            this.btnService.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnService.BackColor = System.Drawing.SystemColors.Control;
            this.btnService.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnService.ForeColor = System.Drawing.Color.Black;
            this.btnService.Location = new System.Drawing.Point(871, 11);
            this.btnService.Name = "btnService";
            this.btnService.Size = new System.Drawing.Size(116, 24);
            this.btnService.TabIndex = 0;
            this.btnService.Text = "서비스 off 상태";
            this.btnService.UseVisualStyleBackColor = false;
            this.btnService.Click += new System.EventHandler(this.btnService_Click);
            // 
            // btnConvertorRun
            // 
            this.btnConvertorRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConvertorRun.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.btnConvertorRun.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConvertorRun.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnConvertorRun.ForeColor = System.Drawing.SystemColors.Window;
            this.btnConvertorRun.Location = new System.Drawing.Point(362, 12);
            this.btnConvertorRun.Name = "btnConvertorRun";
            this.btnConvertorRun.Size = new System.Drawing.Size(104, 23);
            this.btnConvertorRun.TabIndex = 1;
            this.btnConvertorRun.Text = "컨버터 실행";
            this.btnConvertorRun.UseVisualStyleBackColor = false;
            this.btnConvertorRun.Click += new System.EventHandler(this.btnConvertorRun_Click);
            // 
            // btnConvertorPatch
            // 
            this.btnConvertorPatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConvertorPatch.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.btnConvertorPatch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConvertorPatch.ForeColor = System.Drawing.Color.White;
            this.btnConvertorPatch.Location = new System.Drawing.Point(476, 12);
            this.btnConvertorPatch.Name = "btnConvertorPatch";
            this.btnConvertorPatch.Size = new System.Drawing.Size(117, 23);
            this.btnConvertorPatch.TabIndex = 2;
            this.btnConvertorPatch.Text = "컨버터 Patch 방법";
            this.btnConvertorPatch.UseVisualStyleBackColor = false;
            // 
            // btnWindowFinder
            // 
            this.btnWindowFinder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnWindowFinder.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.btnWindowFinder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWindowFinder.ForeColor = System.Drawing.Color.White;
            this.btnWindowFinder.Location = new System.Drawing.Point(601, 12);
            this.btnWindowFinder.Name = "btnWindowFinder";
            this.btnWindowFinder.Size = new System.Drawing.Size(114, 23);
            this.btnWindowFinder.TabIndex = 3;
            this.btnWindowFinder.Text = "윈도우 탐색기";
            this.btnWindowFinder.UseVisualStyleBackColor = false;
            this.btnWindowFinder.Click += new System.EventHandler(this.btnWindowFinder_Click);
            // 
            // btnTerminal
            // 
            this.btnTerminal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTerminal.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.btnTerminal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTerminal.ForeColor = System.Drawing.Color.White;
            this.btnTerminal.Location = new System.Drawing.Point(721, 12);
            this.btnTerminal.Name = "btnTerminal";
            this.btnTerminal.Size = new System.Drawing.Size(114, 23);
            this.btnTerminal.TabIndex = 4;
            this.btnTerminal.Text = "명령프롬프트";
            this.btnTerminal.UseVisualStyleBackColor = false;
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
            this.tabMain.Controls.Add(this.tabPage6);
            this.tabMain.Controls.Add(this.tabPage5);
            this.tabMain.Location = new System.Drawing.Point(4, 42);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(1008, 688);
            this.tabMain.TabIndex = 6;
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
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.btnConvertorError);
            this.tabPage6.Controls.Add(this.btnWorkDirDel);
            this.tabPage6.Controls.Add(this.btnAbnormalApply);
            this.tabPage6.Controls.Add(this.btnAbnormalSearch);
            this.tabPage6.Controls.Add(this.dtpEndDate);
            this.tabPage6.Controls.Add(this.label3);
            this.tabPage6.Controls.Add(this.label2);
            this.tabPage6.Controls.Add(this.dtpStartDate);
            this.tabPage6.Controls.Add(this.dgvAbnormalZipfile);
            this.tabPage6.Location = new System.Drawing.Point(4, 22);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Size = new System.Drawing.Size(1000, 662);
            this.tabPage6.TabIndex = 5;
            this.tabPage6.Text = "비정상 정지 zip파일 처리";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // btnConvertorError
            // 
            this.btnConvertorError.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.btnConvertorError.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConvertorError.ForeColor = System.Drawing.Color.White;
            this.btnConvertorError.Location = new System.Drawing.Point(639, 10);
            this.btnConvertorError.Name = "btnConvertorError";
            this.btnConvertorError.Size = new System.Drawing.Size(105, 23);
            this.btnConvertorError.TabIndex = 9;
            this.btnConvertorError.Text = "컨버터 오류 처리";
            this.btnConvertorError.UseVisualStyleBackColor = false;
            this.btnConvertorError.Click += new System.EventHandler(this.btnConvertorError_Click);
            // 
            // btnWorkDirDel
            // 
            this.btnWorkDirDel.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.btnWorkDirDel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWorkDirDel.ForeColor = System.Drawing.Color.White;
            this.btnWorkDirDel.Location = new System.Drawing.Point(873, 10);
            this.btnWorkDirDel.Name = "btnWorkDirDel";
            this.btnWorkDirDel.Size = new System.Drawing.Size(118, 23);
            this.btnWorkDirDel.TabIndex = 8;
            this.btnWorkDirDel.Text = "WORK 폴더 정리";
            this.btnWorkDirDel.UseVisualStyleBackColor = false;
            this.btnWorkDirDel.Click += new System.EventHandler(this.btnWorkDirDel_Click);
            // 
            // btnAbnormalApply
            // 
            this.btnAbnormalApply.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.btnAbnormalApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAbnormalApply.ForeColor = System.Drawing.Color.White;
            this.btnAbnormalApply.Location = new System.Drawing.Point(482, 10);
            this.btnAbnormalApply.Name = "btnAbnormalApply";
            this.btnAbnormalApply.Size = new System.Drawing.Size(152, 23);
            this.btnAbnormalApply.TabIndex = 7;
            this.btnAbnormalApply.Text = "DB 삭제 및 Zip파일 이동";
            this.btnAbnormalApply.UseVisualStyleBackColor = false;
            this.btnAbnormalApply.Click += new System.EventHandler(this.btnAbnormalApply_Click);
            // 
            // btnAbnormalSearch
            // 
            this.btnAbnormalSearch.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.btnAbnormalSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAbnormalSearch.ForeColor = System.Drawing.Color.White;
            this.btnAbnormalSearch.Location = new System.Drawing.Point(401, 10);
            this.btnAbnormalSearch.Name = "btnAbnormalSearch";
            this.btnAbnormalSearch.Size = new System.Drawing.Size(75, 23);
            this.btnAbnormalSearch.TabIndex = 6;
            this.btnAbnormalSearch.Text = "조 회";
            this.btnAbnormalSearch.UseVisualStyleBackColor = false;
            this.btnAbnormalSearch.Click += new System.EventHandler(this.btnAbnormalSearch_Click);
            // 
            // dtpEndDate
            // 
            this.dtpEndDate.Location = new System.Drawing.Point(241, 11);
            this.dtpEndDate.Name = "dtpEndDate";
            this.dtpEndDate.Size = new System.Drawing.Size(150, 21);
            this.dtpEndDate.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(222, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "~";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "검색기간";
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.Location = new System.Drawing.Point(68, 10);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new System.Drawing.Size(150, 21);
            this.dtpStartDate.TabIndex = 1;
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
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox1.Controls.Add(this.cboFtpServer);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnConvertorRun);
            this.groupBox1.Controls.Add(this.btnTerminal);
            this.groupBox1.Controls.Add(this.btnService);
            this.groupBox1.Controls.Add(this.btnWindowFinder);
            this.groupBox1.Controls.Add(this.btnConvertorPatch);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Location = new System.Drawing.Point(4, -3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(999, 41);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            // 
            // ucLoadingBar
            // 
            this.ucLoadingBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ucLoadingBar.Location = new System.Drawing.Point(367, 346);
            this.ucLoadingBar.Name = "ucLoadingBar";
            this.ucLoadingBar.Size = new System.Drawing.Size(337, 43);
            this.ucLoadingBar.TabIndex = 10;
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
            // dgvEnv
            // 
            this.dgvEnv.AllowUserToAddRows = false;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.Info;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvEnv.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dgvEnv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvEnv.GridColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.dgvEnv.Location = new System.Drawing.Point(0, 0);
            this.dgvEnv.Name = "dgvEnv";
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.AppWorkspace;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvEnv.RowHeadersDefaultCellStyle = dataGridViewCellStyle8;
            this.dgvEnv.RowTemplate.Height = 23;
            this.dgvEnv.Size = new System.Drawing.Size(1000, 662);
            this.dgvEnv.TabIndex = 0;
            this.dgvEnv.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvEnv_CellClick);
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
            // dgvAbnormalZipfile
            // 
            this.dgvAbnormalZipfile.AllowUserToAddRows = false;
            this.dgvAbnormalZipfile.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAbnormalZipfile.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgvAbnormalZipfile.Location = new System.Drawing.Point(0, 42);
            this.dgvAbnormalZipfile.Name = "dgvAbnormalZipfile";
            this.dgvAbnormalZipfile.RowTemplate.Height = 23;
            this.dgvAbnormalZipfile.Size = new System.Drawing.Size(1000, 620);
            this.dgvAbnormalZipfile.TabIndex = 5;
            this.dgvAbnormalZipfile.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgvAbnormalZipfile_CellPainting);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
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
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage6.ResumeLayout(false);
            this.tabPage6.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProcessLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProcess)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvErrorFile)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEnv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDrmFileSend)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAbnormalZipfile)).EndInit();
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
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private DataGridViewEx dgvAbnormalZipfile;
        private System.Windows.Forms.Button btnAbnormalSearch;
        private System.Windows.Forms.Button btnAbnormalApply;
        private System.Windows.Forms.Button btnWorkDirDel;
        private System.Windows.Forms.Button btnConvertorError;
    }
}

