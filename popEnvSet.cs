using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IqaController.entity;
using System.IO;
using System.Net;
using System.Web;
using System.Collections.Specialized;

namespace IqaController
{
    public partial class popEnvSet : Form
    {
        public Boolean m_bSaved = false;
        private List<ControllerServerEntity> m_lstServer = null;          //환경설정 수집서버 
        private List<ControllerFileKeepEntity> m_lstFile = null;          //환경설정 파일 보관주기

        internal List<ControllerServerEntity> LstServer { get => m_lstServer; set => m_lstServer = value; }
        internal List<ControllerFileKeepEntity> LstFile { get => m_lstFile; set => m_lstFile = value; }


        public popEnvSet()
        {
            InitializeComponent();
        }

        private void popEnvSet_Load(object sender, EventArgs e)
        {
            setControl();
            setData();
        }

        private void DBControlsetServer()
        {

            NameValueCollection postData = new NameValueCollection();

            string uri = Define.CON_WEB_SERVICE + "manage/getControllerServer.do";
            WebClient webClient = new WebClient();
            string pagesource = Encoding.UTF8.GetString(webClient.UploadValues(uri, postData));
            try
            {
                m_lstServer = (List<ControllerServerEntity>)Newtonsoft.Json.JsonConvert.DeserializeObject(pagesource, typeof(List<ControllerServerEntity>));
            }
            catch (Exception ex)
            {

            }
        }

        private void DBControlsetFile()
        {

            NameValueCollection postData = new NameValueCollection();
            
            string uri = Define.CON_WEB_SERVICE + "manage/getControllerFilePeriod.do";
            WebClient webClient = new WebClient();
            string pagesource = Encoding.UTF8.GetString(webClient.UploadValues(uri, postData));
            try
            {
                m_lstFile = (List<ControllerFileKeepEntity>)Newtonsoft.Json.JsonConvert.DeserializeObject(pagesource, typeof(List<ControllerFileKeepEntity>));
            }
            catch (Exception ex)
            {

            }
        }

        private void setControl()
        {
            dgvServer.addColumn("수집서버 No", 150);
            dgvServer.addColumn("서버 IP", 150);
            dgvServer.addColumn("ID", 150);
            dgvServer.addColumn("PW", 150);
            dgvServer.addColumn("Path", 150);

            dgvFile.addColumn("항목", 150);
            dgvFile.addColumn("디렉토리 Path", 150);
            dgvFile.addColumn("보관주기", 150);            
        }

        private void SetDataServer()
        {
            foreach (ControllerServerEntity server in m_lstServer)
            {
                updateRowServer(server);
            }
        }

        private void setDataFile()
        {
            foreach (ControllerFileKeepEntity file in m_lstFile)
            {
                updateRowFile(file); ;
            }
        }

        private void setData()
        {
            SetDataServer();
            setDataFile();
        }

        private void updateRowFile(ControllerFileKeepEntity row)
        {
            int nRow = 0;            
            dgvFile.RowCount = dgvFile.RowCount + 1;
            nRow = dgvFile.RowCount - 1;
            dgvFile.Rows[nRow].Cells[0].Value = row.Item;
            dgvFile.Rows[nRow].Cells[1].Value = row.DirPath;
            dgvFile.Rows[nRow].Cells[2].Value = row.Period + "일";
        }

        private void updateRowServer(ControllerServerEntity row)
        {
            int nRow = 0;
            dgvServer.RowCount = dgvServer.RowCount + 1;
            nRow = dgvServer.RowCount - 1;
            dgvServer.Rows[nRow].Cells[0].Value = row.Name;
            dgvServer.Rows[nRow].Cells[1].Value = row.Ip;
            dgvServer.Rows[nRow].Cells[2].Value = row.Id;
            dgvServer.Rows[nRow].Cells[3].Value = row.Password;
            dgvServer.Rows[nRow].Cells[3].Value = row.Path;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (txtServerName.Text == "")
            {
                MessageBox.Show("서버명을 입력하여 주십시요.");
                return;
            }
            if (txtServerIp.Text == "")
            {
                MessageBox.Show("IP를 입력하여 주십시요.");
                return;
            }
            if (txtServerId.Text == "")
            {
                MessageBox.Show("ID를 입력하여 주십시요.");
                return;
            }
            if (txtServerPw.Text == "")
            {
                MessageBox.Show("PW를 입력하여 주십시요.");
                return;
            }

            string uri = Define.CON_WEB_SERVICE + "manage/saveContollerServerSet.do";

            WebClient webClient = new WebClient();
            
            NameValueCollection postData = new NameValueCollection();
            postData.Add("name", HttpUtility.UrlEncode(txtServerName.Text));
            postData.Add("ip", txtServerIp.Text);
            postData.Add("id", txtServerId.Text);
            postData.Add("password", txtServerPw.Text);
            
            string pagesource = Encoding.UTF8.GetString(webClient.UploadValues(uri, postData));
            try
            {
                SaveResultInfo result = (SaveResultInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(pagesource, typeof(SaveResultInfo));
                if (result.Flag == 1)
                {
                    //다시 조회한다.
                    DBControlsetServer();
                    SetDataServer();

                    txtServerIp.Text = "";
                    txtServerId.Text = "";
                    txtServerPw.Text = "";

                    MessageBox.Show("저장하였습니다.");
                    m_bSaved = true;
                }
                else
                {
                    MessageBox.Show(result.Desc);
                }                    
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        private void btnSaveFile_Click(object sender, EventArgs e)
        {
            if (txtItem.Text == "")
            {
                MessageBox.Show("항목을 입력하여 주십시요.");
                return;
            }

            if (txtDirPath.Text == "")
            {
                MessageBox.Show("디렉토리를 설정해 주십시요.");
                return;
            }

            if (txtPeriod.Text == "")
            {
                MessageBox.Show("보관주기를 입력해주십시요.");
                return;
            }

            string uri = Define.CON_WEB_SERVICE + "manage/saveContollerFileSet.do";

            WebClient webClient = new WebClient();
            
            NameValueCollection postData = new NameValueCollection();
            postData.Add("item", HttpUtility.UrlEncode(txtItem.Text));
            postData.Add("dirPath", txtDirPath.Text);
            postData.Add("period", txtPeriod.Text);
            
            string pagesource = Encoding.UTF8.GetString(webClient.UploadValues(uri, postData));
            try
            {
                SaveResultInfo result = (SaveResultInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(pagesource, typeof(SaveResultInfo));
                if (result.Flag == 1)
                {
                    //다시 조회한다.
                    DBControlsetFile();
                    setDataFile();

                    txtItem.Text = "";
                    txtDirPath.Text = "";
                    txtPeriod.Text = "";

                    MessageBox.Show("저장하였습니다.");
                    m_bSaved = true;
                }
                else
                {
                    MessageBox.Show(result.Desc);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }


        }

        private void popEnvSet_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void btnDirSel_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            txtDirPath.Text = dialog.SelectedPath;    //선택한 다이얼로그 경로 저장
        }
    }
}
