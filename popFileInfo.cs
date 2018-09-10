using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using IqaController.entity;

namespace IqaController
{
    public partial class popFileInfo : Form
    {

        private FileProcess m_zipfileInfo = null;

        public popFileInfo()
        {
            InitializeComponent();
            
        }

        internal FileProcess ZipfileInfo { get => m_zipfileInfo; set => m_zipfileInfo = value; }

        public void setControl()
        {
            txtFileNameB.Text = m_zipfileInfo.ZipFileName;
            txtFilePathB.Text = m_zipfileInfo.ZipfileFullPath;
        }

        private void btnFileNameModify_Click(object sender, EventArgs e)
        {
            if (txtFileNameA.Text == "")
            {
                MessageBox.Show("변경후 파일명을 입력하여 주십시요.");
                return;
            }
            m_zipfileInfo.WorkZipFileName = txtFileNameA.Text;

            File.Move(m_zipfileInfo.ZipfileFullPath + "\\" + m_zipfileInfo.ZipFileName, m_zipfileInfo.ZipfileFullPath + "\\" + m_zipfileInfo.WorkZipFileName);
            MessageBox.Show("변경되었습니다.");
            
        }

        private void btnFileMove_Click(object sender, EventArgs e)
        {
            if (txtFilePathA.Text == "")
            {
                MessageBox.Show("파일 이동할 경로를 설정해 주십시요.");
                return;
            }
            
            File.Move(m_zipfileInfo.ZipfileFullPath + "\\" + m_zipfileInfo.ZipFileName, txtFilePathA.Text + "\\" + m_zipfileInfo.WorkZipFileName);
            m_zipfileInfo.ZipfileFullPath = txtFilePathA.Text;
            MessageBox.Show("이동하였습니다.");

        }

        private void btnMoveDirSel_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            txtFilePathA.Text = dialog.SelectedPath;    //선택한 다이얼로그 경로 저장
        }

        private void popFileInfo_Load(object sender, EventArgs e)
        {
            setControl();
            this.Show();
        }
    }
}
