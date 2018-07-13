namespace IqaController
{
    partial class popFileInfo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtFileNameB = new System.Windows.Forms.TextBox();
            this.txtFileNameA = new System.Windows.Forms.TextBox();
            this.btnFileNameModify = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtFilePathB = new System.Windows.Forms.TextBox();
            this.txtFilePathA = new System.Windows.Forms.TextBox();
            this.btnFileMove = new System.Windows.Forms.Button();
            this.btnMoveDirSel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnFileNameModify);
            this.groupBox1.Controls.Add(this.txtFileNameA);
            this.groupBox1.Controls.Add(this.txtFileNameB);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(6, 7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(743, 155);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "* 파일명 수정";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(421, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "[참고] 본부명_측정조명_망유형_주파수_주파수대역_Comment_등록일자.zip";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(35, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(341, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "(예시)본사_도로1조_LTED_SKT_1.8_도봉구일대_20180201.zip";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "변경전";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 118);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "변경후";
            // 
            // txtFileNameB
            // 
            this.txtFileNameB.Enabled = false;
            this.txtFileNameB.Location = new System.Drawing.Point(62, 87);
            this.txtFileNameB.Name = "txtFileNameB";
            this.txtFileNameB.Size = new System.Drawing.Size(592, 21);
            this.txtFileNameB.TabIndex = 5;
            // 
            // txtFileNameA
            // 
            this.txtFileNameA.Location = new System.Drawing.Point(62, 114);
            this.txtFileNameA.Name = "txtFileNameA";
            this.txtFileNameA.Size = new System.Drawing.Size(592, 21);
            this.txtFileNameA.TabIndex = 6;
            // 
            // btnFileNameModify
            // 
            this.btnFileNameModify.Location = new System.Drawing.Point(661, 87);
            this.btnFileNameModify.Name = "btnFileNameModify";
            this.btnFileNameModify.Size = new System.Drawing.Size(69, 48);
            this.btnFileNameModify.TabIndex = 7;
            this.btnFileNameModify.Text = "수 정";
            this.btnFileNameModify.UseVisualStyleBackColor = true;
            this.btnFileNameModify.Click += new System.EventHandler(this.btnFileNameModify_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnMoveDirSel);
            this.groupBox2.Controls.Add(this.btnFileMove);
            this.groupBox2.Controls.Add(this.txtFilePathA);
            this.groupBox2.Controls.Add(this.txtFilePathB);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.groupBox2.Location = new System.Drawing.Point(6, 168);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(743, 142);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 17);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 12);
            this.label6.TabIndex = 1;
            this.label6.Text = "* 파일 이동";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 42);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 12);
            this.label7.TabIndex = 4;
            this.label7.Text = "변경전";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(15, 69);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 12);
            this.label8.TabIndex = 5;
            this.label8.Text = "변경후";
            // 
            // txtFilePathB
            // 
            this.txtFilePathB.Enabled = false;
            this.txtFilePathB.Location = new System.Drawing.Point(66, 39);
            this.txtFilePathB.Name = "txtFilePathB";
            this.txtFilePathB.Size = new System.Drawing.Size(591, 21);
            this.txtFilePathB.TabIndex = 6;
            // 
            // txtFilePathA
            // 
            this.txtFilePathA.Enabled = false;
            this.txtFilePathA.Location = new System.Drawing.Point(66, 92);
            this.txtFilePathA.Name = "txtFilePathA";
            this.txtFilePathA.Size = new System.Drawing.Size(590, 21);
            this.txtFilePathA.TabIndex = 7;
            // 
            // btnFileMove
            // 
            this.btnFileMove.Location = new System.Drawing.Point(664, 37);
            this.btnFileMove.Name = "btnFileMove";
            this.btnFileMove.Size = new System.Drawing.Size(69, 47);
            this.btnFileMove.TabIndex = 8;
            this.btnFileMove.Text = "파일 이동";
            this.btnFileMove.UseVisualStyleBackColor = true;
            this.btnFileMove.Click += new System.EventHandler(this.btnFileMove_Click);
            // 
            // btnMoveDirSel
            // 
            this.btnMoveDirSel.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnMoveDirSel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnMoveDirSel.ForeColor = System.Drawing.Color.White;
            this.btnMoveDirSel.Location = new System.Drawing.Point(65, 64);
            this.btnMoveDirSel.Name = "btnMoveDirSel";
            this.btnMoveDirSel.Size = new System.Drawing.Size(164, 26);
            this.btnMoveDirSel.TabIndex = 9;
            this.btnMoveDirSel.Text = "이동 디렉토리 선택";
            this.btnMoveDirSel.UseVisualStyleBackColor = false;
            this.btnMoveDirSel.Click += new System.EventHandler(this.btnMoveDirSel_Click);
            // 
            // popFileInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(758, 325);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "popFileInfo";
            this.Text = "파일 정보 변경";
            this.Load += new System.EventHandler(this.popFileInfo_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnFileNameModify;
        private System.Windows.Forms.TextBox txtFileNameA;
        private System.Windows.Forms.TextBox txtFileNameB;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnFileMove;
        private System.Windows.Forms.TextBox txtFilePathA;
        private System.Windows.Forms.TextBox txtFilePathB;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnMoveDirSel;
    }
}