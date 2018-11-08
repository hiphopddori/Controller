namespace IqaController
{
    partial class ucLoadingBar
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

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.picLoading = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picLoading)).BeginInit();
            this.SuspendLayout();
            // 
            // picLoading
            // 
            this.picLoading.Image = global::IqaController.Properties.Resources.ajax_loader_blue_32;
            this.picLoading.Location = new System.Drawing.Point(8, 5);
            this.picLoading.Name = "picLoading";
            this.picLoading.Size = new System.Drawing.Size(32, 32);
            this.picLoading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picLoading.TabIndex = 0;
            this.picLoading.TabStop = false;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.ForeColor = System.Drawing.Color.Blue;
            this.lblTitle.Location = new System.Drawing.Point(49, 16);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(62, 12);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Loading...";
            // 
            // ucLoadingBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.picLoading);
            this.Name = "ucLoadingBar";
            this.Size = new System.Drawing.Size(335, 41);
            ((System.ComponentModel.ISupportInitialize)(this.picLoading)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picLoading;
        private System.Windows.Forms.Label lblTitle;
    }
}
