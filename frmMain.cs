using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Specialized;
using System.Web;
using System.Timers;

using Renci.SshNet;
using IqaController.entity;
using IqaController.service;




/* Filesystemwatcher
 * https://msdn.microsoft.com/ko-kr/library/system.io.filesystemwatcher(v=vs.110).aspx
 * 
 * // 오라클 클라이언트 접속
 * http://kin33.tistory.com/60
 * 
 * */

namespace IqaController
{
    public partial class frmMain : Form
    {

        private Dictionary<string, FileProcess> m_dicFileProcess = new Dictionary<string, FileProcess>();           //
        private Dictionary<string, FileProcess> m_dicFileProcessErr = new Dictionary<string, FileProcess>();        //Error 파일 따로 관리(로딩시 디렉토리에서도 읽어와야함)

        private string m_ftpId = "14";

        private Boolean m_bServiceOnOffButton = false;  //서비스 On Off 버튼 토글        
        private Boolean m_ServiceOnIng = false;         //서비스 처리 프로세스 진행중 여부 (true:진행중 , false:완료)

        private System.Timers.Timer timer = new System.Timers.Timer();
        //private System.Threading.Timer tmrServiceBtn = null;

        private Boolean m_bServiceColorFlag = false;

        private List<ControllerServerEntity> m_lstServer = null;          //환경설정 수집서버 
        private List<ControllerFileKeepEntity> m_lstFile = null;          //환경설정 파일 보관주기
        private List<ControllerEnvEntity> m_lstEnv = new List<ControllerEnvEntity>();

        private Boolean m_debug = true;
        
        // private FileStream losStrm;
        // private TextWriter log = null;

        delegate void TimerEventFiredDelegate();

        public frmMain()
        {
            InitializeComponent();
        }

        private void startTimer()
        {
            //tmrServiceBtn.Change(0, 2000);           
            
            if (timer != null)
            {
                timer.Enabled = true;
                timer.Start();
            }
        }

        private void endTimer()
        {
            //tmrServiceBtn.Change(Timeout.Infinite, Timeout.Infinite);
            if (timer != null)
            {
                timer.Enabled = false;
                timer.Stop();
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {            
            // losStrm = new FileStream("./log2.txt", FileMode.OpenOrCreate, FileAccess.Write);
            // log = new StreamWriter(losStrm);

            if (!dbContollerSetInfo())
            {
                MessageBox.Show("컨트롤러 실행에 필요한 데이타를 가져오는데 실패하였습니다.\n관리자에게 문의하십시요.");
                this.Close();
                return;
            }

            //sendSFtp(@"D:\FTP14\WORK\본사_도로501조_LTED_SKT_AUTO_테스트_20180402");
            //sendFtp(@"D:\FTP14\WORK\본사_도로501조_LTED_SKT_AUTO_테스트_20180402");

            setControl();
            updateRowEnv();

            //findDirErrorInfo(Define.con_DIR_ZIPDUP);      zip파일 중복체크 안하기로 함
            findDirErrorInfo(Define.con_DIR_NAMING);
            findDirErrorInfo(Define.con_DIR_UPLOAD);

            m_bServiceOnOffButton = true;            
            setControlServiceOnOff();


            //파일 보관주기에 따른 삭제 처리
            //filePeriodChk2Proc

        }

        /* 환경설정 파일 보관주기에 따른 삭제 처리 
         *
         */
        void filePeriodChk2Proc()
        {
            /*
            string path = getDefaultFtpPath();
            string[] dir = Directory.GetDirectories(path + "\\" + Define.con_DIR_NAMING + "\\");
            string dirOnlyName = "";
            foreach(string dirFullName in dir)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dirFullName);
                dirOnlyName = dirInfo.Name;
                dirInfo.Delete();

            }
            */

            if (!(m_lstFile != null && m_lstFile.Count > 0))
            {
                return;
            }

            foreach(ControllerFileKeepEntity filePeriodInfo in m_lstFile)
            {
                string[] dirs = Directory.GetDirectories(filePeriodInfo.DirPath);
                string dirOnlyName = "";
                int period = Convert.ToInt16(filePeriodInfo.Period);
                

                //디렉토리 삭제
                foreach (string dirFullName in dirs)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(dirFullName);
                    dirOnlyName = dirInfo.Name;

                    //string[] dirSplit = dirOnlyName.Split('_');
                    //string dirDate = dirSplit[dirSplit.Length - 1];

                    //DateTime dtDate = DateTime.ParseExact(dirDate, "yyyyMMddHHmmss", null);
                    DateTime dtDate = dirInfo.CreationTime;
                    dtDate = dtDate.AddDays(period);                                    //보관 기간

                    int nCompare = DateTime.Compare(dtDate, DateTime.Now);
                    if (nCompare < 0)
                    {
                        dirInfo.Delete(true);
                    }
                }

                //파일 삭제 - 실제 없음 (추후 확인후 제거하자)
                string[] files = Directory.GetFiles(filePeriodInfo.DirPath);
                foreach (string fileFullPath in files)
                {
                    FileInfo tmp = new FileInfo(fileFullPath);
                    DateTime dtDate = tmp.CreationTime;
                    dtDate = dtDate.AddDays(period);                                    //보관 기간

                    int nCompare = DateTime.Compare(dtDate, DateTime.Now);
                    if (nCompare < 0)
                    {
                        tmp.Delete();
                    }
                }
            }
        }

        void tmr_ServiceBtnBlink2(object x)
        {
            
            BeginInvoke(new TimerEventFiredDelegate(serviceBtnBlinkProc));

            //btnService.BackColor = backColor;            
        }

        void serviceBtnBlinkProc()
        {
            
            m_bServiceColorFlag = !m_bServiceColorFlag;
          
            if (btnService.BackColor == Color.Silver)
            {
                btnService.BackColor = Color.Red;
            }
            else
            {
                btnService.BackColor = Color.Silver;
            }

            //Console.WriteLine(btnService.BackColor.ToString());
        }

        void tmr_ServiceBtnBlink(object sender, ElapsedEventArgs e)        
        {          
            if (this.IsHandleCreated)
            {
                BeginInvoke(new TimerEventFiredDelegate(serviceBtnBlinkProc));
            }
        }

        private void setControlServiceOnOff()
        {

            if (m_bServiceOnOffButton)
            {
                cboFtpServer.Enabled = false;
                btnService.Text = "서비스 On 상태";
                //startTimer();
                /*
                if (!timer.Enabled)4
                {
                    timer.Start();
                }
                */

                serviceOnMainProc();
            }
            else
            {
                cboFtpServer.Enabled = true;

                if (m_ServiceOnIng)
                {
                    btnService.Text = "서비스 Off 처리중...";
                    btnService.Enabled = false;
                }
                else
                {
                    endTimer();
                    btnService.Text = "서비스 Off 상태";
                    btnService.Enabled = true;                    
                    btnService.BackColor = Color.Silver;
                }
            }
        }

        private void setLog(FileProcess row, string message)
        {

            this.Invoke(new Action(delegate ()
            {
                dgvProcessLog.RowCount = dgvProcessLog.RowCount + 1;
                int nRow = dgvProcessLog.RowCount - 1;
                dgvProcessLog.Rows[nRow].Cells[0].Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"); ;
                dgvProcessLog.Rows[nRow].Cells[1].Value = row.MeasuBranch;
                dgvProcessLog.Rows[nRow].Cells[2].Value = row.MeasuGroup;
                dgvProcessLog.Rows[nRow].Cells[3].Value = row.ZipFileName;
                dgvProcessLog.Rows[nRow].Cells[4].Value = message;
            }));
            
            //Console.WriteLine(message);
        }

        private void updateRowEnv()
        {

            int nRow = 0;
            this.Invoke(new Action(delegate ()
            {
                dgvEnv.RowCount = 0;
                foreach (ControllerEnvEntity env in m_lstEnv)
                {
                    dgvEnv.RowCount = dgvEnv.RowCount + 1;
                    nRow = dgvEnv.RowCount - 1;
                    dgvEnv.Rows[nRow].Cells[1].Value = env.Item;
                    dgvEnv.Rows[nRow].Cells[2].Value = env.Info1;
                    dgvEnv.Rows[nRow].Cells[3].Value = env.Info2;
                    dgvEnv.Rows[nRow].Cells[4].Value = env.Info3;
                }
            }));

        }
        /* 디렉토리 탐색후 Error 파일정보를 가져온다.
         */
        private void findDirErrorInfo(string flagErr)
        {
            string defaultPath = getDefaultFtpPath();

            string[] dirs = Directory.GetDirectories(defaultPath + "\\" + flagErr);

            string findExtention = "";
            if (flagErr == Define.con_DIR_UPLOAD)
            {
                findExtention = "qms";
            }
            else
            {
                findExtention = "zip";
            }


            foreach (string dir in dirs)
            {
                
                string zipfileFullPath = "";
                string[] files = Directory.GetFiles(dir, "*."+ findExtention);

                string zipfileNm = Path.GetFileName(dir+".zip");


                if (files.Length > 0)
                {
                    zipfileFullPath = files[0];
                    //zipfileNm = Path.GetFileName(files[0]);
                    
                    FileProcess row = new FileProcess();
                    row.ZipFileName = zipfileNm;                //zipfile 명
                    row.ProcessServer = getFtpName(m_ftpId);    //process서버 명
                    row.ZipfileFullPath = zipfileFullPath;

                    //파일명 규칙 오류
                    if (flagErr == Define.con_DIR_NAMING)
                    {
                        row.FileNameRule = Define.CON_ERROR;
                    }
                    else if (flagErr == Define.con_DIR_PARSING)
                    {
                        row.ConversionFlag = Define.CON_ERROR;
                    }
                    else if (flagErr == Define.con_DIR_UPLOAD)
                    {
                        row.FtpSendFlag = Define.CON_COMPLETE;
                        row.FtpSuccessFlag = Define.con_FAIL;

                        string qmsFileNm = "";
                        string qmsFileNmChg = "";
                        int nIdx = 0;
                        foreach(string filePath in files)
                        {
                            //FileInfo file = new FileInfo(filePath);
                            nIdx++;
                            qmsFileNm = Path.GetFileName(filePath);
                            ZipFileDetailEntity detail = new ZipFileDetailEntity();
                            detail.QmsfileNm = qmsFileNm;
                            detail.QmsfileNmChg = getQmaNameChange(Convert.ToString(nIdx));                            
                            row.ZipfileDetailList.Add(detail); ;
                        }


                    }
                    else if (flagErr == Define.con_DIR_ZIPDUP)
                    {
                        row.ZipDupFlag = Define.CON_ERROR;
                    }

                    updateRowErrorFile(row);
                }                
            }
        }


        //서버쪽 파일명 꺠지는 현상으로 qms파일 변경 처리 한다.
        private string getQmaNameChange(string seq)
        {
            DateTime curDate = DateTime.Now;

            string qmsNmChg = getFtpName(m_ftpId) + "_" + seq + "_" + curDate.ToString("yyyyMMddHHmmss.ffff") + ".qms";
            return qmsNmChg;
        }
       
        private void updateRowErrorFile(FileProcess row)
        {
            string sFileNm = row.ZipFileName;

            /*
            if (m_dicFileProcess.ContainsKey(sFileNm))
            {
                nRow = dgvErrorFile.getFindZipFile2RowIndex(sFileNm, 4);
            }
            else
            {
                dgvErrorFile.RowCount = dgvErrorFile.RowCount + 1;
                nRow = dgvProcess.RowCount - 1;
                m_dicFileProcess.Add(sFileNm, row);
            }
            */

            this.Invoke(new Action(delegate ()
            {

                if (m_dicFileProcessErr.ContainsKey(sFileNm))
                {
                    m_dicFileProcessErr[sFileNm] = row;                   
                }
                else
                {                    
                    m_dicFileProcessErr.Add(sFileNm, row);
                }

                dgvErrorFile.RowCount = dgvErrorFile.RowCount + 1;
                int nRow = dgvErrorFile.RowCount - 1;

                dgvErrorFile.Rows[nRow].Cells[1].Value = row.ProcessServer;
                dgvErrorFile.Rows[nRow].Cells[3].Value = row.ZipfileFullPath;
                dgvErrorFile.Rows[nRow].Cells[4].Value = row.ZipFileName;

                if (row.FileNameRule == Define.CON_ERROR)
                {
                    dgvErrorFile.Rows[nRow].Cells[2].Value = "파일명규칙 Error";
                    dgvErrorFile.setCellButton("수정/파일이동", 5, nRow);
                }
                else if (row.FtpSendFlag == Define.CON_COMPLETE && row.FtpSuccessFlag == Define.con_FAIL)
                {
                    dgvErrorFile.Rows[nRow].Cells[2].Value = "FTP 전송 Error";
                    dgvErrorFile.setCellButton("FTP 전송", 5, nRow);
                }
                else if (row.ZipDupFlag == Define.CON_ERROR)
                {
                    dgvErrorFile.Rows[nRow].Cells[2].Value = "ZIP 파일 중복";
                    dgvErrorFile.setCellButton("수정/파일이동", 5, nRow);
                }
                else if (row.ConversionFlag == Define.CON_ERROR)
                {
                    dgvErrorFile.Rows[nRow].Cells[2].Value = "컨버전 Error";
                    dgvErrorFile.Rows[nRow].Cells[5].Value = "품질분석실-측정기장비업체 확인요망";
                }
            }));

        }

        /* 파일처리현황 그리드에 값 반영한다.
         * 
         */
        private void updateRowFileProcess(FileProcess row)
        {
            string sFileNm = row.ZipFileName;
            int nRow = -1;
            
            this.Invoke(new Action(delegate ()
            {

                if (m_dicFileProcess.ContainsKey(sFileNm))
                {
                    m_dicFileProcess[sFileNm] = row;
                    nRow = dgvProcess.getFindZipFile2RowIndex(sFileNm, 4);
                }
                else
                {
                    dgvProcess.RowCount = dgvProcess.RowCount + 1;
                    nRow = dgvProcess.RowCount - 1;
                    m_dicFileProcess.Add(sFileNm, row);
                }

                dgvProcess.Rows[nRow].Cells[1].Value = row.ProcessServer;
                dgvProcess.Rows[nRow].Cells[2].Value = row.MeasuBranch;
                dgvProcess.Rows[nRow].Cells[3].Value = row.MeasuGroup;
                dgvProcess.Rows[nRow].Cells[4].Value = row.ZipFileName;
                dgvProcess.Rows[nRow].Cells[5].Value = row.FileNameRule; 
                dgvProcess.Rows[nRow].Cells[6].Value = row.ExtractFlag;
                dgvProcess.Rows[nRow].Cells[7].Value = row.ConversionFlag;
                dgvProcess.Rows[nRow].Cells[8].Value = row.FtpSendFlag;
                dgvProcess.Rows[nRow].Cells[9].Value = row.BackupFlag;
            }));

            //Application.DoEvents();

        }
        /* 파일 이동할 이름을 얻는다.
         * 
        * */
        private string getMoveFileName(string orgFileName)
        {

            string onlyFileName = Path.GetFileNameWithoutExtension(orgFileName);
            string cur = DateTime.Now.ToString("yyyyMMddHHmmss");
            onlyFileName = onlyFileName + "_" + cur;
            onlyFileName = onlyFileName + Path.GetExtension(orgFileName);
            return onlyFileName;
        }

        private void setData()
        {
            /*
            int i = 0;
            lvEnv.BeginUpdate();
            ListViewItem item;
            while (i < 5)
            {
                
                item = new ListViewItem();

                Button btn = new Button();
                btn.Text = "위치";

                //item.SubItems.Add((Control)btn);

                //item = new ListViewItem(i.ToString());
                item.SubItems.Add("test2");
                item.SubItems.Add("test3");
                item.SubItems.Add("test4");
                //em.SubItems.Add("test5");
                lvEnv.Items.Add(item);
                i++;
            }

            lvEnv.EndUpdate();
            */
            
            string[] row = new string[] {"","1"};
            dgvEnv.Rows.Add(row);
            row = new string[] { "","2" };
            dgvEnv.Rows.Add(row);
            row = new string[] { "", "3"};
            dgvEnv.Rows.Add(row);
            row = new string[] { "", "4"};
            dgvEnv.Rows.Add(row);
            //dgvEnv[1, 1].Value = "tes";

            // 동적 값 변경 처리
            foreach (DataGridViewRow r in dgvEnv.Rows)
            {
                r.Cells[0] = new DataGridViewButtonCell();
                ((DataGridViewButtonCell)r.Cells[0]).Value = " ";

                //r.Cells[0] = new DataGridViewTextBoxCell();
                //((DataGridViewTextBoxCell)r.Cells[0]).Value = "Confirm";
            }
        }

        private string getFtpName(string serverIdx)
        {
            return String.Format("FTP{0:00}", Convert.ToInt32(serverIdx)); 
        }

        private void setControl()
        {
            /* column percent example
               https://nickstips.wordpress.com/2010/11/10/c-listview-dynamically-sizing-columns-to-fill-whole-control/
               //데이타 쓰기
               http://blog.naver.com/PostView.nhn?blogId=inho860&logNo=220053153176&beginTime=0&jumpingVid=&from=search&redirect=Log&widgetTypeCall=true
            */

            /*
            Button tmp = new Button();
            tmp.Text = "위치";
            lvFileProcess.Controls.Add(tmp);
            */

            timer.Interval = 1000; // 1 시간
            timer.Elapsed += new ElapsedEventHandler(tmr_ServiceBtnBlink);
            

            //tmrServiceBtn = new System.Threading.Timer(tmr_ServiceBtnBlink2);
            startTimer();
            
            StringBuilder retSelFtp = new StringBuilder();
            util.GetIniString("FTP", "SELECT", "", retSelFtp, 32, Application.StartupPath + "\\controllerSet.Ini");
            string iniSelFtp = retSelFtp.ToString();

            //저장된값이 없을경우 디폴트로 셋팅
            if (iniSelFtp.Length <= 0)
            {
                iniSelFtp = getFtpName(m_ftpId);
            }

            string[] ftpServer = new string[16];
            string tmpFtpName = "";
            int nFtpSelIdx = 13;
            for (int idx = 0; idx < 16; idx++)
            {
                tmpFtpName = String.Format("FTP{0:00}", idx + 1);
                ftpServer[idx] = tmpFtpName;
                if (iniSelFtp == tmpFtpName)
                {
                    nFtpSelIdx = idx;
                }
            }

            cboFtpServer.Items.AddRange(ftpServer);                        
            cboFtpServer.SelectedIndex = nFtpSelIdx;
            
            dgvProcess.addColumnButton("위치", "파일", 60);
            dgvProcess.addColumn("처리서버", 60);
            dgvProcess.addColumn("측정본부", 80);
            dgvProcess.addColumn("측정조", 80);
            dgvProcess.addColumn("zip 파일명", 300);
            dgvProcess.addColumn("파일명규칙", 100);
            dgvProcess.addColumn("압축해제", 80);
            dgvProcess.addColumn("컨버전", 80);
            dgvProcess.addColumn("FTP 전송", 80);
            dgvProcess.addColumn("백업", 80);

            dgvProcessLog.addColumn("일시", 120);
            dgvProcessLog.addColumn("측정본부", 70);
            dgvProcessLog.addColumn("측정조", 100);
            dgvProcessLog.addColumn("zip 파일명", 350);
            dgvProcessLog.addColumn("로그", 200);

            dgvErrorFile.addColumnButton("위치", "파일", 60);
            dgvErrorFile.addColumn("처리서버", 60);
            dgvErrorFile.addColumn("Error 분류", 120);
            dgvErrorFile.addColumn("파일 Path", 200);
            dgvErrorFile.addColumn("zip 파일명", 320);
            dgvErrorFile.addColumn("파일 재처리", 150);

            //dgvEnv.addColumnButton("관리", "위치",60);
            dgvEnv.addColumnButton("관리","수정", 60);
            dgvEnv.addColumn("항목", 200);
            dgvEnv.addColumn("등록정보1", 250);
            dgvEnv.addColumn("등록정보2", 150);
            dgvEnv.addColumn("등록정보3", 150);            
        }
               
        private void btnWindowFinder_Click(object sender, EventArgs e)
        {
            exec("explorer.exe", "");          
        }
      
        private Boolean isFileValidate(string fileName)
        {
            return true;
        }

        private Boolean zipFileNamingChk(FileProcess row , string fileName)
        {
            //복사할 경로에 해당 파일 유무 체크
            string path = getDefaultFtpPath();
            string onlyFileName = Path.GetFileName(fileName);
            // string moveFileName = getMoveFileName(fileName);
            string withoutExtensionName = Path.GetFileNameWithoutExtension(fileName);
            //string sMoveDir = Path.GetFileNameWithoutExtension(Path.GetFileName(moveFileName));
            string sMoveDir = Path.GetFileNameWithoutExtension(onlyFileName);

            string[] arFileName = withoutExtensionName.Split('_');
            string sFileDate = "";
            DateTime dtDate;
            DateTime dtMaxDate;

            //if (arFileName.Length != 7)
            if (arFileName.Length != 8)     //뒤에 시스템 날짜 더 붙임
            {
                //구분자 오류                
                row.ZipfileFullPath = path + "\\" + Define.con_DIR_NAMING + "\\" + sMoveDir;
                row.FileNameRule = Define.CON_ERROR;                       
                updateRowFileProcess(row);
                setLog(row, "구분자 오류");                
                
                DirectoryInfo diChk = new DirectoryInfo(path + "\\" + Define.con_DIR_NAMING + "\\" + sMoveDir);
                if (diChk.Exists == false)
                {
                    diChk.Create();
                }

                fileSave("move", path + "\\" + onlyFileName, path + "\\" + Define.con_DIR_NAMING + "\\" + sMoveDir + "\\" + onlyFileName);
                //MoveFile(path + "\\" + onlyFileName, path + "\\NAMING_ERROR\\" + sMoveDir + "\\" + moveFileName);
                return false;
            }

            //<START> ############# 파일 유효 기간 체크
            sFileDate = arFileName[6];
            dtDate = DateTime.ParseExact(sFileDate, "yyyyMMdd", null);
            dtMaxDate = dtDate.AddDays(30);                                //최대 30일 유효범위임             
            int nCompare = DateTime.Compare(dtMaxDate, DateTime.Now);

            if (nCompare < 0)
            {
                row.ZipfileFullPath = path + "\\" + Define.con_DIR_NAMING + "\\" + sMoveDir;
                row.FileNameRule = Define.CON_ERROR;
                updateRowFileProcess(row);
                setLog(row, "유효기간 오류");

                DirectoryInfo diChk = new DirectoryInfo(path + "\\" + Define.con_DIR_NAMING + "\\" + sMoveDir);
                if (diChk.Exists == false)
                {
                    diChk.Create();
                }
                //MoveFile(path + "\\" + onlyFileName, path + "\\NAMING_ERROR\\" + sMoveDir + "\\" + moveFileName);
                fileSave("move", path + "\\" + onlyFileName, path + "\\" + Define.con_DIR_NAMING + "\\" + sMoveDir + "\\" + onlyFileName);
                return false;
            }

            //#############################################</END>
            return true;
        } 

        private Boolean dbContollerSetInfo()
        {
            m_lstFile = iqaService.getControllerFilePeriod();          
            m_lstServer = iqaService.getControllerServer();           
            m_lstEnv.Clear();

            foreach (ControllerServerEntity server in m_lstServer)
            {
                ControllerEnvEntity env = new ControllerEnvEntity();
                env.Flag = "server";
                env.Item = server.Name;
                env.Info1 = "IP : " + server.Ip;
                env.Info2 = "ID : " + server.Id;
                env.Info3 = "PW : " + server.Password;
                m_lstEnv.Add(env);
            }

            if (m_lstServer == null ||  m_lstServer.Count <= 0)
            {
                util.Log("[ERROR]", "서버정보 없음");
                return false;
            }

            foreach (ControllerFileKeepEntity file in m_lstFile)
            {
                ControllerEnvEntity env = new ControllerEnvEntity();
                env.Flag = "file";
                env.Item = file.Item;
                env.Info1 = "IP : " + file.DirPath;
                env.Info2 = "ID : " + file.Period.ToString() + "일";
                env.Info3 = "";
                m_lstEnv.Add(env);
            }
            return true;
        }

        /* DB 중복 ZIP FILE 명 체크
        */
           
        private Boolean dbZipFileDupChk(string zipFileNm)
        {
            NameValueCollection postData = new NameValueCollection();

            postData.Add("zipfileNm", HttpUtility.UrlEncode(zipFileNm));

            string uri = Define.CON_WEB_SERVICE + "manage/getZipFileDupChk.do";
            WebClient webClient = new WebClient();
            string pagesource = Encoding.UTF8.GetString(webClient.UploadValues(uri, postData));
            try
            {
                SaveResultInfo result = (SaveResultInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(pagesource, typeof(SaveResultInfo));

                if (result.Flag == "1")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                util.Log("[ERROR(getControllerFilePeriod.do)]", ex.Message);
                Console.WriteLine(ex.Message.ToString());
                return false;
            }
        }

        private Boolean dbUpdateZipFileInfo(FileProcess row,string flag)
        {
            NameValueCollection postData = new NameValueCollection();

            postData.Add("zipfileNm", HttpUtility.UrlEncode(row.ZipFileName));
            postData.Add("measuBranch", HttpUtility.UrlEncode(row.MeasuBranch));
            postData.Add("measuGroup", HttpUtility.UrlEncode(row.MeasuGroup));
            postData.Add("inoutType", row.InoutType);
            postData.Add("comp", row.Comp);

            postData.Add("zipfileSize", row.FileSize);
            postData.Add("procServer", row.ProcessServer);

            if (row.FtpSendServer != "")
            {
                postData.Add("ftpServer", row.FtpSendServer);
            }

            if (row.UnzipfileCnt != "")
            {
                postData.Add("unzipfileCnt", row.UnzipfileCnt);
            }
            if (row.DupfileCnt != "")
            {
                postData.Add("dupfileCnt", row.DupfileCnt);
            }
            if (row.CompletefileCnt != "")
            {
                postData.Add("completefileCnt", row.CompletefileCnt);
            }
            if (row.ParsingCompleteCnt != "")
            {
                postData.Add("parsingCompleteCnt", row.ParsingCompleteCnt);
            }
            if (row.CurState != "")
            {
                postData.Add("curState", row.CurState);
            }

            if (row.FlagFtpSend == Define.con_SUCCESS)
            {
                postData.Add("flagFtpSend", Define.con_SUCCESS);
                postData.Add("ftpSendFileCnt", Convert.ToString(row.FtpSendFileCnt));
                postData.Add("ftpSendSuccessCnt", Convert.ToString(row.FtpSendSuccessCnt));                
            }

            if (flag == Define.con_ING_START)
            {
                postData.Add("startTime", "1");                             //처리 시작시간
            }
            else if (flag == Define.con_ING_UNZIP)
            {
                postData.Add("unzipStartTime", "1");                        //UnZip 시작 시점 업데이트
            }
            else if (flag == Define.con_ING_FTPUPLOAD)
            {
                postData.Add("uploadStartTime", "1");                       //업로드 시작 시점 업데이트 - 화면설계상 없어서 사용안함
            }
           

            string uri = Define.CON_WEB_SERVICE + "manage/insertZipFileInfo.do";
            WebClient webClient = new WebClient();
            string pagesource = Encoding.UTF8.GetString(webClient.UploadValues(uri, postData));
            try
            {
                SaveResultInfo result = (SaveResultInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(pagesource, typeof(SaveResultInfo));

                if (result.Flag == "1")
                    return true;                
                else                
                    return false;                
            }
            catch (Exception ex)
            {
                setLog(row, "[ERROR(dbUpdateZipFileInfo)]" + ex.Message);
                util.Log("[ERROR(dbUpdateZipFileInfo.do)]", ex.Message);
                Console.WriteLine(ex.Message.ToString());
                return false;
            }
        }

        private Boolean dbUpdateQmsFile(FileProcess row)
        {
            if (row.QmsFileList.Count <= 0)
            {
                return true;
            }

            List<string> lstQmsfile = new List<string>();
            List<string> lstChgFlie = new List<string>();
            List<string> lstFlagSend = new List<string>();

            foreach (QmsFileInfo qmsInfo in row.QmsFileList)
            {
                lstQmsfile.Add(HttpUtility.UrlEncode(qmsInfo.FileName));
                lstChgFlie.Add(qmsInfo.ChgFlieName);
                lstFlagSend.Add(qmsInfo.FlagSend);
            }

            string qmsStr = getJsonStr(lstQmsfile);
            string chgQmsStr = getJsonStr(lstChgFlie);
            string flagSendStr = getJsonStr(lstFlagSend);

            NameValueCollection postData = new NameValueCollection();
            postData.Add("zipfileNm", HttpUtility.UrlEncode(row.ZipFileName));
            postData.Add("qmsFileNm", qmsStr);
            postData.Add("chgQmsFileNm", chgQmsStr);
            postData.Add("flagFtpSend", flagSendStr);

            return true;

        }

        //unzip 파일정보 업데이트 한다.
        private void unzipFileDetailProc(FileProcess row , string dupChkDir)
        {
            string[] fileEntries = Directory.GetFiles(dupChkDir);
            List<string> unZipFile = new List<string>();

            string extension = "";
            string onlyFileName = "";

            foreach (string fileName in fileEntries)
            {
                extension = Path.GetExtension(fileName);
                onlyFileName = Path.GetFileName(fileName);
                if (extension == ".drm" || extension == ".dml")
                {
                    ZipFileDetailEntity detail = new ZipFileDetailEntity();
                    detail.UnzipfileNm = onlyFileName;
                    row.ZipfileDetailList.Add(detail);
                    unZipFile.Add(HttpUtility.UrlEncode(onlyFileName));
                }
            }

            //string unZipStr = Newtonsoft.Json.JsonConvert.SerializeObject(unZipFile);
            string unZipStr = getJsonStr(unZipFile);
            string uri = Define.CON_WEB_SERVICE + "manage/getUnzipFileUpdateProc.do";
            WebClient webClient = new WebClient();
            //webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
            //webClient.Encoding = UTF8Encoding.UTF8;

            NameValueCollection postData = new NameValueCollection();

            postData.Add("zipfileNm", HttpUtility.UrlEncode(row.ZipFileName));
            postData.Add("unzipFileNm", unZipStr);

            string pagesource = Encoding.UTF8.GetString(webClient.UploadValues(uri, postData));
            try
            {                
                List<CodeNameEntity> dupList = (List<CodeNameEntity>)Newtonsoft.Json.JsonConvert.DeserializeObject(pagesource, typeof(List<CodeNameEntity>));
                // Console.WriteLine(dupList.Count);
                
                if (dupList.Count > 0)
                {
                    //해당 파일 제거한다.
                    foreach(CodeNameEntity tmp in dupList)
                    {
                        File.Delete(dupChkDir + "\\"+tmp.Code);
                    }     
                }

                /*
                 - 중복 drm 파일/dml파일명 의 경우는 해당 디렉토리 내에 별도 디렉토리 생성하여 임시 이동(초반 백업으로 이동하여 의미 없음)
                 - 중복 drm 파일/dml파일명 정보는 Log 저장
                 - 중복 아닌 경우는 컨버젼 진행 대상으로 처리 
                */
            }
            catch (Exception ex)
            {
                setLog(row, "[ERROR(unzipFileDetailProc)]" + ex.Message);
                util.Log("[ERROR(unzipFileDetailProc)]", ex.Message);
                Console.WriteLine(ex.Message.ToString());
            }

            //WebServer 호출해서 dupChk한다.

        }

        private Boolean fileSave(string flag,string sourceFileName , string destiFileName)
        {
            int nMax = 2000;
            int nAttemptCnt = 0;
            Boolean bDo = true;

            while (bDo)
            {
                try
                {
                    if (flag == "copy")
                    {
                        File.Copy(sourceFileName, destiFileName, true);
                    }
                    else
                    {
                        File.Move(sourceFileName, destiFileName);
                    }
                    bDo = false;
                }
                catch (Exception ex)
                {
                    if (ex.HResult == -2147024864)
                    {
                        Thread.Sleep(1500);
                        nAttemptCnt++;


                        if (nAttemptCnt > nMax)
                        {
                            bDo = false;
                            util.Log("[ERROR(fileSave.do)]", "파일권한 무한 대기 발생");
                        }
                    }
                    else
                    {
                        bDo = false;
                    }
                }
            }

            if (bDo)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /* FTP 전송실패 QMS파일 백업
         */
        private void setFtpError2FileMove(FileProcess row)
        {
            if (row.FtpSendFileCnt <= row.FtpSendSuccessCnt)
            {
                return;
            }

            string movePath = getDefaultFtpPath();
            movePath += movePath + "\\" + Define.con_DIR_UPLOAD + "\\" + row.ZipFileName;

            //UPLOAD ERROR 폴더 생성
            DirectoryInfo diChk = new DirectoryInfo(movePath);
            if (diChk.Exists == false)
            {
                diChk.Create();
            }

            foreach (ZipFileDetailEntity detail in row.ZipfileDetailList)
            {
                if (detail.FlagFtpSend == Define.CON_ERROR)
                {
                    fileSave("move", row.ZipfileFullPath + "\\" + detail.QmsfileNm, movePath + "\\" + detail.QmsfileNm);
                }
            }

        }

        private void zipfileParsing(string fileName)
        {
            string sServiceType = "";
            string path = getDefaultFtpPath();                        
            string onlyFileName = Path.GetFileName(fileName);
            string moveFileName = getMoveFileName(fileName);
            string withoutExtensionName = Path.GetFileNameWithoutExtension(fileName);
            string withoutExtensionMoveName = Path.GetFileNameWithoutExtension(moveFileName);

            if (m_dicFileProcess.ContainsKey(onlyFileName))
            {
                if (m_dicFileProcess[onlyFileName].Pass) return;
            }

            FileProcess row = new FileProcess();
            row.ProcessServer = getFtpName(m_ftpId);
            //row.ZipFileName = onlyFileName;
            row.ZipFileName = moveFileName;             //2018-06-14 : Zip 파일명도 시스템 날짜 추가된거로 관리하는거로 요청옴
            row.WorkZipFileName = moveFileName;
            row.CurState = Define.con_STATE_WORK;

            FileInfo fInfo = new FileInfo(fileName);
            row.FileSize = fInfo.Length.ToString();
            row.ZipfileFullPath = path;

            /* 2018-06-14 Zip파일 중복체크는 하지 않는거로 다시 요청옴
            setLog(row, "중복 Zip 파일 체크");
            //<START> ############################ DB 중복 ZipFile 체크
            if (!dbZipFileDupChk(onlyFileName))
            {
                row.CurState = Define.con_STATE_ERR_DUP_ZIP;
                row.ZipDupFlag = Define.CON_ERROR;
                setLog(row, "중복 Zip 파일명");
                
                DirectoryInfo diChk = new DirectoryInfo(path + "\\" + Define.con_DIR_ZIPDUP + "\\" + withoutExtensionName);
                if (diChk.Exists == false)
                {
                    diChk.Create();
                }

                fileSave("move", fileName , path + "\\" + Define.con_DIR_ZIPDUP + "\\" + withoutExtensionName + "\\" + onlyFileName);

                dbUpdateZipFileInfo(row, "");

                return;
            }
            //#############################################</END>
            */

            updateRowFileProcess(row);

            try
            {
                //Log 그리드 초기화

                this.Invoke(new Action(delegate ()
                {
                    dgvProcessLog.RowCount = 0;
                }));

                setLog(row, "파일이동");
                fileSave("move", path + "\\" + onlyFileName, path + "\\" + Define.con_DIR_WORK + "\\" + moveFileName);
                row.ZipfileFullPath = path + "\\" + Define.con_DIR_WORK;

                fInfo = new FileInfo(row.ZipfileFullPath + "\\" + moveFileName);
                row.FileSize = fInfo.Length.ToString();

                setLog(row, "파일이동 완료");
                setLog(row, "파일명 규칙 분석");

                dbUpdateZipFileInfo(row, Define.con_ING_START);

                if (!zipFileNamingChk(row, path + "\\" + Define.con_DIR_WORK +"\\" + moveFileName))
                {
                    row.CurState = Define.con_STATE_ERR_NAMING;
                    updateRowErrorFile(row);
                    dbUpdateZipFileInfo(row,"");                    
                    return;
                }

                string[] arFileName = withoutExtensionName.Split('_');

                row.FileNameRule = Define.CON_COMPLETE;
                row.MeasuBranch = arFileName[0];
                row.MeasuGroup = arFileName[1];
                row.Comp = arFileName[3];

                if (row.MeasuGroup.IndexOf("도로") >= 0)
                {
                    row.InoutType = "OUTDOOR";
                }else if (row.MeasuGroup.IndexOf("인빌딩") >= 0)
                {
                    row.InoutType = "INDOOR";
                }
                else
                {
                    row.InoutType = "";
                }

                if (row.Comp == "SKT")
                {
                    row.Comp = "011";
                }else if (row.Comp == "SKT")
                {
                    row.Comp = "016";
                }
                else if (row.Comp == "LGU")
                {
                    row.Comp = "019";
                }

                row.BackupFlag = Define.CON_ING;
                row.CurState = Define.con_STATE_COMPLETE_NAMING;

                updateRowFileProcess(row);
                setLog(row, "파일명 규칙 완료");

                dbUpdateZipFileInfo(row, "");

                //#############################################</END>

                sServiceType = arFileName[2];

                //<START> ############# 파일 백업 및 작업 디렉토리로 이동
                //moveFileName = getMoveFileName(fileName);
                //MoveFile(path + "\\" + onlyFileName, path + "\\WORK\\" + moveFileName);

                setLog(row, "백업");
                fileSave("copy", path + "\\WORK\\" + moveFileName, path + "\\BACK\\" + moveFileName);
                setLog(row, "백업 완료");


                //완료후 압축 해제
                row.BackupFlag = Define.CON_COMPLETE;
                row.ExtractFlag = Define.CON_ING;
                row.ZipfileFullPath = path + "\\WORK";                                           //이동된 File 디렉토리
                updateRowFileProcess(row);
                //#############################################</END>

                //<START> ############# 압축 해제                
                
                setLog(row, "압축해제");
                UnZIpProc(row.ZipfileFullPath + "\\" + moveFileName);

                row.ExtractFlag = Define.CON_COMPLETE;
                row.ConversionFlag = Define.CON_ING;
                row.CurState = Define.con_STATE_COMPLETE_UNZIP;
                dbUpdateZipFileInfo(row,"");

                setLog(row, "압축해제 완료");
                updateRowFileProcess(row);

                //#############################################</END>
                Thread.Sleep(2000);
                setLog(row, "중복 Drm 체크");
                //<START> ############# Unzip 파일 정보 DB 업로드및 중복 Drm 제거 처리
                string extractDir = getUnzipPath(row.ZipfileFullPath + "\\" + moveFileName);

                //이곳에서 unzip 파일 및 dup파일 정보 업데이트 한다.
                unzipFileDetailProc(row , extractDir);
                setLog(row, "중복 Drm 체크 완료");                
                //#############################################</END>

                //모두 중복 제거일경우 컨버터 호출하지 않는다.
                string[] extractFile = Directory.GetFiles(extractDir);

                if (extractFile.Length <= 0)
                {
                    setLog(row, "모든 파일 중복 Drm");
                    return;
                }
                //<START> ############# 컨버터 실행
                Thread.Sleep(2000);
                setLog(row, "컨버터 실행");                
                string runConvertorTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                string sdeCompressPath = getUnzipPath(path + "\\WORK\\" + moveFileName);
                runServyceType2RunConvertor(sServiceType, sdeCompressPath);
                //#############################################</END>

                //생각보다 컨트롤러 처리 오래 걸림
                Thread.Sleep(40000);
                //최대 횟수 제한하자
                string sResultFindFileName = "";
                while (sResultFindFileName == "")
                {
                    //ZIP파일 생성 이후 시점  마지막 생성된 날짜 로그를 본다._END가 있는지
                    sResultFindFileName = isConvertorRunComplete(runConvertorTime);
                    //Application.DoEvents();
                    Thread.Sleep(10000);
                }

                row.ConversionFlag = Define.CON_COMPLETE;
                row.FtpSendFlag = Define.CON_ING;
                setLog(row, "컨버터 실행 완료");
                updateRowFileProcess(row);
                
                if (sResultFindFileName.Length > 0 )
                {

                    //컨버터 결과를 DB남기고 컨버터 오류시 파일이동한다.
                    convertResultProc(row, sResultFindFileName);

                    //dbUpdateZipFileInfo(row, Define.con_ING_FTPUPLOAD);
                    setLog(row, "파일전송");
                    //FTP 전송
                    if (m_debug)
                    {
                        sendSFtp(row,extractDir,false);
                    }
                    else
                    {
                        sendFtp(row,extractDir,false);
                    }
                    
                    //서버에 파일 전송한다.
                    setLog(row, "파일정송 완료");

                }
                //정상 처리 로그 남긴다.                        1
                row.FtpSendFlag = Define.CON_COMPLETE;
                row.Complete = true;
                setLog(row, "처리 완료");
                updateRowFileProcess(row);
                row.CurState = Define.con_STATE_WORK_COMPLETE;

                //ZIp파일 완료 DB Update
                dbUpdateZipfileComplete(row);

                //FTP 전송 실패한 qms파일을 백업이동한다.
                setFtpError2FileMove(row);

                //worK 작업 파일 관련 삭제 처리
                if (deleteWorkZipFile(extractDir))
                {
                    setLog(row, "작업파일 삭제 완료");
                }
                else
                {
                    setLog(row, "작업파일 삭제 실패");
                }

            }
            catch (Exception ex)
            {
                setLog(row, "[ERROR(zipfileParsing)]" + ex.Message);
                //복사 진행중인 파일
                //파일이 다른 프로세스에서 사용되고 있으므로 프로세스에서 파일에 액세스할 수 없습니다.
                if (ex.HResult == -2147024864)
                {
                    //파일 해당 폴더로 전송중인 상태라 전송 완료시점에 다시 처리 하기 위해
                    row.Pass = false;
                }
                Console.WriteLine(ex.Message);
            }
        }

        private Boolean deleteWorkZipFile(string dirPath)
        {

            try
            {
                DirectoryInfo di = new DirectoryInfo(dirPath);
                di.Delete(true);

                FileInfo file = new FileInfo(dirPath + ".zip");
                file.Delete();

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                //setLog(row, "[ERROR(deleteDir)]" + ex.Message);
                util.Log("[ERROR(dbUpdateZipfileComplete.do)]", ex.Message);                
                return false;
            }

            return true;

        }

        //추후 시간날때 검토해보자
        private string getJsonStr(List<string> data)
        {
            string tmp = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            tmp = tmp.Remove(0, 1);                         //  '[' 제거
            tmp = tmp.Remove(tmp.Length - 1, 1);            //  ']' 제거
            tmp = tmp.Replace(@"""", @"");                  //  string "" 제거
            return tmp;
        }
        /* 압축 해제 완료 체크
        */
        private void unZipCompleteChk(string unZipLogPath,string chkDirPath)
        {

            //파일 새성 여부 체크
            FileInfo fi = null;                            
            Boolean bfileChk = false;
            while (!bfileChk)
            {
                fi = new FileInfo(unZipLogPath);
                if (fi.Exists)
                {
                    bfileChk = true;                    
                }
                Thread.Sleep(1000);
            }

            //파일 생성되었는지 여부 확인
            string[] lines = System.IO.File.ReadAllLines(unZipLogPath, Encoding.Default);
            int lastRow = -1;
            for(int nIdx = lines.Length - 1; nIdx >= 0; nIdx--)
            {
                if (lines[nIdx].Trim() != "")
                {
                    lastRow = nIdx;
                    break;
                }
            }
            string result = "";            
            if (lastRow >= 0)
            {
                Boolean bDataY = false;
                string totalSize = "";
                result = lines[lastRow];
                
                foreach(char tmp in result)
                {
                    if (tmp.ToString() != " ")
                    {
                        totalSize += tmp.ToString();
                        bDataY = true;
                    }
                    else
                    {
                        if (bDataY)
                        {
                            break;
                        }
                    }                    
                }
                Console.WriteLine(totalSize);
                long unzipTotalSize = Convert.ToInt64(totalSize);
                
                Thread.Sleep(4000);
                //최대 횟수 제한하자
                long dirTotalSize = 0;
                while (unzipTotalSize > dirTotalSize)
                {
                    //ZIP파일 생성 이후 시점  마지막 생성된 날짜 로그를 본다._END가 있는지
                    dirTotalSize = getDirectoryTotalSize(chkDirPath);
                    //Application.DoEvents();
                    Thread.Sleep(2000);
                }
            }
        }

        private long getDirectoryTotalSize(string sDirPath)
        {
            // Get array of all file names.
            string[] a = Directory.GetFiles(sDirPath, "*.*");
            // 2
            // Calculate total bytes of all files in a loop.
            long b = 0;
            foreach (string name in a)
            {
                // 3
                // Use FileInfo to get length of each file.
                FileInfo info = new FileInfo(name);
                if (info.Extension == ".drm" || info.Extension == ".dml")
                {
                    b += info.Length;
                }
            }
            // 4
            // Return total size
            return b;            
        }

        private Boolean dbUpdateZipfileComplete(FileProcess row)
        {

            WebClient webClient = new WebClient();
            NameValueCollection postData = new NameValueCollection();

            List<string> arUnzipfileNm = new List<string>();
            List<string> arQmsfileNm = new List<string>();
            List<string> arFlagFtpSend = new List<string>();

            postData.Add("zipfileNm", HttpUtility.UrlEncode(row.ZipFileName));
            if (row.FtpSendFileCnt > 0)
            {
                foreach(ZipFileDetailEntity detail in row.ZipfileDetailList)
                {
                    arUnzipfileNm.Add(HttpUtility.UrlEncode(detail.UnzipfileNm));
                    arQmsfileNm.Add(HttpUtility.UrlEncode(detail.QmsfileNm));
                    arFlagFtpSend.Add(detail.FlagFtpSend);
                }

                string unZipStr = getJsonStr(arUnzipfileNm);
                string qmsStr = getJsonStr(arQmsfileNm);
                string flagFtpSendStr = getJsonStr(arFlagFtpSend);

                postData.Add("unzipfileNm", unZipStr);
                postData.Add("qmsfileNm", qmsStr);
                postData.Add("flagFtpSend", flagFtpSendStr);
            }

            postData.Add("ftpServer", row.FtpSendServer);
            postData.Add("ftpSendFileCnt", Convert.ToString(row.FtpSendFileCnt));
            postData.Add("ftpSendSuccessCnt", Convert.ToString(row.FtpSendSuccessCnt));
            postData.Add("flaFtpSendAction", "1");


            string uri = Define.CON_WEB_SERVICE + "manage/setUpdateZipFileComplete.do";

            string pagesource = Encoding.UTF8.GetString(webClient.UploadValues(uri, postData));
            try
            {
                SaveResultInfo result = (SaveResultInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(pagesource, typeof(SaveResultInfo));

                if (result.Flag == "1")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                setLog(row, "[ERROR(dbUpdateZipfileComplete)]" + ex.Message);
                util.Log("[ERROR(dbUpdateZipfileComplete.do)]", ex.Message);
                Console.WriteLine(ex.Message.ToString());
                return false;
            }
        }

        private Boolean convertResultProc(FileProcess row, string filePath)
        {

            string[] lines = System.IO.File.ReadAllLines(filePath, Encoding.Default);
           
            // string array
            string flag = "";
            string desc = "";
            string[] tmpResult = null;
            //List<ConvertorResult> unZipFileDetailInfo = new List<ConvertorResult>();
            List<string> arUnzipfileNm = new List<string>();
            List<string> arQmsfileNm = new List<string>();
            List<string> arQmsfileNmChg = new List<string>();
            List<string> arFlag = new List<string>();
            List<string> arDesc = new List<string>();
            Boolean bConvertError = false;
            int nQmsFileCnt = 0;
            foreach (string log in lines)
            {
                // System.Console.WriteLine(log);
                tmpResult = log.Split(';');
                if (tmpResult.Length == 3)
                {                    
                    if (tmpResult[2] == "END")
                    {
                        flag = "1";
                        desc = "";
                    }
                    else
                    {
                        flag = "0";
                        desc = tmpResult[2];
                        bConvertError = true;
                    }
                    /*
                    NameValueCollection info = new NameValueCollection()
                       {
                              { "unzipfileNm",  HttpUtility.UrlEncode(tmpResult[0])} ,
                              { "qmsFileNm", HttpUtility.UrlEncode(tmpResult[1])},
                              { "flag", flag},
                              { "desc", desc},
                       };
                    */
                    ZipFileDetailEntity detail = row.findZipfileDetail("unzipNm", tmpResult[0]);

                    if (detail != null)
                    {
                        nQmsFileCnt++;
                        detail.QmsfileNm = tmpResult[1];
                        detail.FlagConvertorResult = flag;
                        detail.DescConvertorResult = desc;
                        detail.QmsfileNmChg = getQmaNameChange(Convert.ToString(nQmsFileCnt));


                        arUnzipfileNm.Add(HttpUtility.UrlEncode(tmpResult[0]));
                        arQmsfileNm.Add(HttpUtility.UrlEncode(detail.QmsfileNm));
                        arQmsfileNmChg.Add(detail.QmsfileNmChg);
                        arFlag.Add(flag);
                        arDesc.Add(desc);
                    }                    
                }
            }
            if (bConvertError)
            {
                string path = getDefaultFtpPath();

                row.ConversionFlag = Define.CON_ERROR;
                row.ZipfileFullPath = path + "\\CONVERT_ERROR\\" + row.ZipFileName;
                updateRowErrorFile(row);
                
                File.Move(path + "\\WORK\\" + row.ZipFileName, path + "\\CONVERT_ERROR\\" + row.ZipFileName);            //WORK 폴더로 이동
            }

            //추후 보완하자 ㅠㅠ 배열 스트링이 WebService에서 잡힘 ㅠㅠ
            string unZipStr = getJsonStr(arUnzipfileNm);
            string qmsStr = getJsonStr(arQmsfileNm);
            string qmsChgStr = getJsonStr(arQmsfileNmChg);
            string flagStr = getJsonStr(arFlag);
            string descStr = getJsonStr(arDesc);
          
            string uri = Define.CON_WEB_SERVICE + "manage/setUpdateConvetorResult.do";
            
            WebClient webClient = new WebClient();
            // webClient.Headers[HttpRequestHeader.ContentType] = "application/json";            
            // webClient.Encoding = UTF8Encoding.UTF8;

            NameValueCollection postData = new NameValueCollection();                          
            postData.Add("zipfileNm", HttpUtility.UrlEncode(row.ZipFileName));
            postData.Add("unzipfileNm", unZipStr);            
            postData.Add("qmsfileNm", qmsStr);
            postData.Add("qmsfileNmChg", qmsChgStr);
            postData.Add("flag", flagStr);
            postData.Add("desc", descStr);


            string pagesource = Encoding.UTF8.GetString(webClient.UploadValues(uri, postData));
            try
            {
                SaveResultInfo result = (SaveResultInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(pagesource, typeof(SaveResultInfo));

                if (result.Flag == "1")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                setLog(row, "[ERROR(convertResultProc)]" + ex.Message);
                util.Log("[ERROR(convertResultProc.do)]", ex.Message);
                Console.WriteLine(ex.Message.ToString());
                return false;
            }
        }

        private Boolean unZipFileChk(string zipFileNm, string unzipPath)
        {
            
            string extension = "";
            string onlyFileName = "";
            string[] fileEntries = Directory.GetFiles(unzipPath);
            
            List<string> unZipFile = new List<string>();

            foreach (string fileName in fileEntries)
            {
                extension = Path.GetExtension(fileName);
                onlyFileName = Path.GetFileNameWithoutExtension(fileName);
                if (extension == ".drm" || extension == ".dml")
                {
                    unZipFile.Add(onlyFileName);                    
                }
            }

            string unZipStr = Newtonsoft.Json.JsonConvert.SerializeObject(unZipFile);
            //string uri = "http://localhost:8080/IQA/map/getTestList.do";
            string uri = Define.CON_WEB_SERVICE + "map/unzipFileDupProc.do";
            WebClient webClient = new WebClient();
            //webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
            //webClient.Encoding = UTF8Encoding.UTF8;
            
            NameValueCollection postData = new NameValueCollection()
               {
                      { "zipFileName",  HttpUtility.UrlEncode(zipFileNm) } ,
                      { "unZipFile", HttpUtility.UrlEncode(unZipStr)}
               };
            string pagesource = Encoding.UTF8.GetString(webClient.UploadValues(uri, postData));
            try
            {
                string sTest = pagesource;

                /*
                List<Test> myDeserializedObjList = (List<Test>)Newtonsoft.Json.JsonConvert.DeserializeObject(pagesource, typeof(List<Test>));
                Console.WriteLine(myDeserializedObjList.Count);
                */

                /*
                 - 중복 drm 파일/dml파일명 의 경우는 해당 디렉토리 내에 별도 디렉토리 생성하여 임시 이동
                 - 중복 drm 파일/dml파일명 정보는 Log 저장
                 - 중복 아닌 경우는 컨버젼 진행 대상으로 처리 
                */

            }
            catch (Exception ex)
            {
                util.Log("[ERROR(getControllerFilePeriod.do)]", ex.Message);
                Console.WriteLine(ex.Message.ToString());
            }

            return true;
        }

        private void serviceOnMainProc()
        {
            Thread thread = new Thread(new ThreadStart(delegate ()
            {
                serviceOnThreadProc();
            }));
            thread.Start();

        }

        private void appWorkDirMake(string path)
        {
            //Work Directory 생성
            DirectoryInfo diChk = new DirectoryInfo(path + "\\" + Define.con_DIR_WORK);
            if (diChk.Exists == false)
            {
                diChk.Create();
            }

            // 중복 backup 백업 폴더
            diChk = new DirectoryInfo(path + "\\" + Define.con_DIR_BACK);
            if (diChk.Exists == false)
            {
                diChk.Create();
            }
            // CONVERT_ERROR
            diChk = new DirectoryInfo(path + "\\" + Define.con_DIR_CONVERT );
            if (diChk.Exists == false)
            {
                diChk.Create();
            }

            // NAMING ERROR
            diChk = new DirectoryInfo(path + "\\" + Define.con_DIR_NAMING);
            if (diChk.Exists == false)
            {
                diChk.Create();
            }

            // ZipfileName 중복 - ZIP파일 중복은 사용안함
            diChk = new DirectoryInfo(path + "\\" + Define.con_DIR_ZIPDUP);
            if (diChk.Exists == false)
            {
                diChk.Create();
            }

            // qms 파일 전송 ERROR
            diChk = new DirectoryInfo(path + "\\" + Define.con_DIR_UPLOAD);
            if (diChk.Exists == false)
            {
                diChk.Create();
            }
        }
        private void serviceOnThreadProc()
        {            
            string path = getDefaultFtpPath();
            if (Directory.Exists(path))
            {
                m_ServiceOnIng = true;

                appWorkDirMake(path);     // WORK 폴도 생성

                string[] fileEntries = Directory.GetFiles(path);
                
                foreach (string fileName in fileEntries)
                {
                    //중도 중단 시켰을경우
                    if (!m_bServiceOnOffButton) break;                    
                    zipfileParsing(fileName);
                    Thread.Sleep(4000);
                }
                
                m_ServiceOnIng = false;
                Thread.Sleep(500);
                
                if (m_bServiceOnOffButton)
                {
                    //서비스 On 상태면 재귀호출한다.
                    startTimer();
                    serviceOnThreadProc();
                }
                else
                {
                    //m_bServiceOn = false;
                    endTimer();
                    setControlServiceOnOff();
                }
            }
            else
            {
                MessageBox.Show("해당 경로의 디렉토리가 존재 하지 않습니다.");
            }            
        }
        private string getDefaultFtpPath()
        {
            return @"D:\" + getFtpName(m_ftpId);
            //return Define.CON_DEFAULT_DIR + m_ftpId;
        }

        /* 컨버터 실행시 컨버터가 처리 완료되었는지 확인 처리
         * 
        */
        private string isConvertorRunComplete(string sRunTime)
        {
            
            string path = @"D:\INNOWHB\Check List";
            DateTime dtConvertorRunDate = DateTime.ParseExact(sRunTime, "yyyyMMddHHmmss", null);

            string[] fileEntries = Directory.GetFiles(path);
            string sDirFileName = "";
            string sWithoutExtensionName = "";
            string sFindFileName = "";
            foreach (string fileName in fileEntries)
            {
                sWithoutExtensionName = Path.GetFileNameWithoutExtension(fileName);
                string[] arFileName = sWithoutExtensionName.Split('_');
                sDirFileName = arFileName[0] + arFileName[1];
                DateTime dtDirFileDate = DateTime.ParseExact(sDirFileName, "yyyyMMddHHmmss", null);
                //컨버터 실행시점보다 로그 파일시점이 이후이고 _END로 되있을경우 완료임
                int nCompare = DateTime.Compare(dtConvertorRunDate, dtDirFileDate);
                if (nCompare < 0)
                {
                    if (arFileName[2] == "END")
                    {            
                        sFindFileName = fileName;
                        break;
                    }
                }
            }
            return sFindFileName;
        }
        
        private string getUnzipPath(string fileFullPath)
        {
            string defaultPath = getDefaultFtpPath();
            string sWithoutExtensionName = Path.GetFileNameWithoutExtension(fileFullPath);
            string extractDir = defaultPath + "\\WORK\\" + sWithoutExtensionName;
            return extractDir;
        }

        /* 압축 해제 한다.
         * 
         */
        private void UnZIpProc(string fileFullPath)
        {

            string extractDir = getUnzipPath(fileFullPath);
            /*
            string sWithoutExtensionName = Path.GetFileNameWithoutExtension(fileFullPath);
            string extractDir = m_defaultFolder + m_ftpId + "\\WORK\\"+ sWithoutExtensionName;
            */
            DirectoryInfo di = new DirectoryInfo(extractDir);
            if (di.Exists == false)
            {
                di.Create();
            }

            string defaultPath = getDefaultFtpPath();
            defaultPath += "\\WORK\\";

            execUnzipList("l " + fileFullPath + " > " + extractDir + "\\unzipInfo.txt");
            // exec(@"C:\Program Files (x86)\7-Zip\7z.exe", "l " + fileFullPath + " > " + defaultPath + "test.txt");
            exec(Define.CON_ZIP_EXE_PATH+"7z.exe", "x -o" + extractDir + "\\ -r -y " + fileFullPath);

            unZipCompleteChk(extractDir+"\\" + "unzipInfo.txt", extractDir); 

            /*
            try
            {
                using (FileStream originalFileStream = fileToDecompress.OpenRead())
                {
                    string currentFileName = fileToDecompress.FullName;
                    string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                    using (FileStream decompressedFileStream = File.Create(newFileName))
                    {
                        using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                        {
                            decompressionStream.CopyTo(decompressedFileStream);
                            Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            } 
            */
        }
        /*
        private void extractFile(string sourceFile, string destinationFile)
        {
            try
            {
                ZipFile.ExtractToDirectory(sourceFile, destinationFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception: {0}", ex.ToString());
            }
        }
        */
        /* 파일 이동 한다.
         * */
        private async void MoveFile(string sourceFile, string destinationFile)
        {
            try
            {
                using (FileStream sourceStream = File.Open(sourceFile, FileMode.Open))
                {
                    using (FileStream destinationStream = File.Create(destinationFile))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                        sourceStream.Close();
                        File.Delete(sourceFile);
                    }
                }
            }
            catch (IOException ioex)
            {
                //MessageBox.Show("An IOException occured during move, " + ioex.Message);
                //result = ioex.HResult;
                util.Log("[ERROR(MoveFile.do)]", ioex.Message);
                throw ioex;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("An Exception occured during move, " + ex.Message);
                //result = ex.HResult;
                util.Log("[ERROR(MoveFile.do)]", ex.Message);
                throw ex;
            }
        }

        private void btnTerminal_Click(object sender, EventArgs e)
        {
            exec("cmd.exe", "");           
        }

        private void btnConvertorRun_Click(object sender, EventArgs e)
        {
            MenuItem[] menuItems = new MenuItem[]{
                    new MenuItem("Inno LTE Data 컨버터",new System.EventHandler(this.onClickMenuConvertor))
                    , new MenuItem("Inno HSDPA Data",new System.EventHandler(this.onClickMenuConvertor))
                    , new MenuItem("Inno LTE 음성(M to M) 컨버터",new System.EventHandler(this.onClickMenuConvertor))
                    , new MenuItem("-")
                    , new MenuItem("STI 컨버터",new System.EventHandler(this.onClickMenuConvertor))
            };

            ContextMenu buttonMenu = new ContextMenu(menuItems);
            buttonMenu.Show(btnConvertorRun, new System.Drawing.Point(0, 20));
        }

        private void runServyceType2RunConvertor(string serviceType,string path)
        {
            int nIdx = 0;
            if (serviceType == "LTED")
            {
                nIdx = 0;
            }else if (serviceType == "LTEV" || serviceType == "HDV" || serviceType == "HDVML" || serviceType == "CSFB" || serviceType == "CSFBML" )
            {
                nIdx = 2;
            }else if (serviceType == "HSDPA")
            {
                nIdx = 1;
            }
            execConvertor(nIdx, path);
        }

        private void execConvertor(int index , string path)
        {

            //추후 정리하자 ㅠㅠ
            string type = "";
            switch (index)
            {
                case 0:                    
                    type = "From/Ax`L ";                    
                    break;
                case 1:                    
                    type = "From/Ax`H ";                    
                    break;
                case 2:            
                    type = "From/Ax`L ";                    
                    break;                                
            }

            if (path.Length > 0)
            {
                type = "";
            }

            switch (index)
            {
                case 0:     // Inno LTE Data 컨버터                    
                    exec("D:\\INNOWHB\\QMS-W.exe", type + path + " L_Data.cfg ");
                    break;
                case 1:     // Inno HSDPA Data                                        
                    exec("D:\\INNOWHB\\QMS-W.exe", type + path + " H_Data.cfg");
                    break;
                case 2:     // Inno LTE 음성(M to M) 컨버터                                        
                    exec("D:\\INNOWHB\\QMS-W.exe", type + path + " L_Voice.cfg");
                    break;                
                case 4:     // STI 컨버터
                    exec("D:\\STI\\QMS Export.exe", "");
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }
        }

        private void reSendFtp(FileProcess row, string localPath)
        {
            //qmnsNm , qmsNmChg ,구성하여 업데이트 해야함
            if (m_debug)
            {
                sendSFtp(row, localPath,true);
            }
            else
            {
                sendFtp(row, localPath,true);
            }

            //성공시 폴더 삭제
            

            //dbUpdate한다.

            row.FtpSendFlag = Define.CON_COMPLETE;
            row.Complete = true;
            setLog(row, "처리 완료");
            updateRowFileProcess(row);
            row.CurState = Define.con_STATE_WORK_COMPLETE;

            dbUpdateZipfileComplete(row);


        }

        private Boolean sendSFtp(FileProcess row , string localPath, Boolean flagResend)
        {
            string uri = Define.CON_WEB_SERVICE + "manage/geFreeServerList.do";
            WebClient webClient = new WebClient();
            NameValueCollection postData = new NameValueCollection();

            ControllerServerEntity freeServerInfo = null;
            
            string pagesource = Encoding.UTF8.GetString(webClient.UploadValues(uri, postData));
            try
            {              
                freeServerInfo = (ControllerServerEntity)Newtonsoft.Json.JsonConvert.DeserializeObject(pagesource, typeof(ControllerServerEntity));            
            }
            catch (Exception ex)
            {
                util.Log("[ERROR(geFreeServerList.do)]", ex.Message);
                Console.WriteLine(ex.Message.ToString());
                return false;
            }

            if (freeServerInfo == null)
            {
                util.Log("[ERROR(sendFtp)]", "유휴서버 리스트 가져오지 못함");
                return false;
                //이럴일이 있나?
            }

            int nFtpFileCnt = 0;
            int nFtpSuccessCnt = 0;
            string fileName = "";
            try
            {                
                using (var client = new SftpClient(freeServerInfo.Ip , Convert.ToInt32(freeServerInfo.Port), freeServerInfo.Id, freeServerInfo.Password))
                {
                    client.Connect();                 
                    client.ChangeDirectory(freeServerInfo.Path);
                    
                    string[] files = Directory.GetFiles(localPath);
                    foreach (string filepath in files)
                    {
                        string extension = Path.GetExtension(filepath);
                        //qms만 전송한다.
                        if (extension != ".qms")
                        {
                            continue;
                        }

                        fileName = Path.GetFileName(filepath);                        
                        ZipFileDetailEntity detail = row.findZipfileDetail("qmsNm", fileName);

                        if (detail != null)
                        {
                            nFtpFileCnt++;
                            detail.FlagFtpSend = Define.con_SUCCESS;
                            //detail.QmsfileNmChg = fileName;

                            try
                            {
                                using (FileStream fs = new FileStream(filepath, FileMode.Open))
                                {
                                    long totalLength = fs.Length;

                                    client.BufferSize = (uint)totalLength;
                                    client.UploadFile(fs, detail.QmsfileNmChg);
                                    nFtpSuccessCnt++;
                                    //row.QmsFileList.Add(new QmsFileInfo(fileName, Define.con_SUCCESS, fileName));
                                }
                            }
                            catch (Exception exFtp)
                            {
                                setLog(row, "[ERROR(sendSFtp)]" + exFtp.Message);
                                util.Log("[ERROR(sendSFtp)]", exFtp.Message);
                                if (detail != null)
                                {
                                    detail.FlagFtpSend = Define.con_FAIL;
                                }
                                //row.QmsFileList.Add(new QmsFileInfo(fileName, Define.con_FAIL, fileName));
                            }
                        }                        
                    }
                    client.Disconnect();

                    row.FtpSendServer = freeServerInfo.Name;
                    row.FtpSendFileCnt = nFtpFileCnt;
                    row.FtpSendSuccessCnt = nFtpSuccessCnt;
                    row.FtpSendFlag = Define.CON_COMPLETE;
                }
            }
            catch(Exception exception)
            {
                setLog(row, "[ERROR(sendSFtp)]" + exception.Message);
                util.Log("[ERROR(getControllerFilePeriod.do)]", exception.Message);
                Console.WriteLine(exception.Message.ToString());
            }
            return true;
        }

        private Boolean sendFtp(FileProcess row ,string localPath , Boolean flagResend)
        {
            // https://docs.microsoft.com/ko-kr/dotnet/api/system.io.filestream.read?f1url=https%3A%2F%2Fmsdn.microsoft.com%2Fquery%2Fdev15.query%3FappId%3DDev15IDEF1%26l%3DKO-KR%26k%3Dk(System.IO.FileStream.Read);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.5);k(DevLang-csharp)%26rd%3Dtrue&view=netframework-4.7.1
            // http://codingcoding.tistory.com/50

            string path = getDefaultFtpPath();
            string uri = Define.CON_WEB_SERVICE + "manage/geFreeServerList.do";
            WebClient webClient = new WebClient();            
            NameValueCollection postData = new NameValueCollection();

            ControllerServerEntity freeServerInfo = null;
            
            string pagesource = Encoding.UTF8.GetString(webClient.UploadValues(uri, postData));
            try
            {              
                freeServerInfo = (ControllerServerEntity)Newtonsoft.Json.JsonConvert.DeserializeObject(pagesource, typeof(ControllerServerEntity));            
            }
            catch (Exception ex)
            {
                util.Log("[ERROR(geFreeServerList.do)]", ex.Message);
                Console.WriteLine(ex.Message.ToString());
                return false;
            }
            /*
            freeServerInfo = new ControllerServerEntity();
            freeServerInfo.Ip = "150.23.27.92";
            freeServerInfo.Port = "9084";
            freeServerInfo.Id = "eva";
            freeServerInfo.Password = "hello.mobigen";
            freeServerInfo.Path = "home/eva/";
            */

            if (freeServerInfo == null)
            {
                util.Log("[ERROR(sendFtp)]", "유휴서버 리스트 가져오지 못함");
                return false;
                //이럴일이 있나?
            }


            int nFtpFileCnt = 0;
            int nFtpSuccessCnt = 0;

            // string localPath = @"C:\Project\IQA\ftpTest\";
            FtpWebRequest requestFTPUploader = null;
            string[] files = Directory.GetFiles(localPath);
            //prgbProcess.Value = 0;

            foreach (string filepath in files)
            {
                string extension = Path.GetExtension(filepath);
                
                //qms만 전송한다.
                if (extension != ".qms")
                {
                    continue;
                }
                nFtpFileCnt++;
                string fileName = Path.GetFileName(filepath);
                string ftpUrl = "ftp://" + freeServerInfo.Ip + ":" + freeServerInfo.Port +  "/" + freeServerInfo.Path + fileName;

                ZipFileDetailEntity detail = row.findZipfileDetail("qmsNm", fileName);

                if (detail != null)
                {
                    detail.FlagFtpSend = Define.con_SUCCESS;
                    detail.QmsfileNmChg = fileName;

                    requestFTPUploader = (FtpWebRequest)WebRequest.Create(ftpUrl);
                    requestFTPUploader.UsePassive = false;
                    //requestFTPUploader.Credentials = new NetworkCredential("mobigen", "hello.mobigen");
                    requestFTPUploader.Credentials = new NetworkCredential(freeServerInfo.Id, freeServerInfo.Password);
                    requestFTPUploader.Method = WebRequestMethods.Ftp.UploadFile;

                    FileInfo fileInfo = new FileInfo(filepath);
                    FileStream fileStream = fileInfo.OpenRead();

                    double uploadRate = 0;
                    long totalLength = fileStream.Length;
                    int bufferLength = 2048;
                    //byte[] buffer = new byte[bufferLength];
                    byte[] buffer = new byte[totalLength];

                    try
                    {
                        int offset = 0;
                        Stream uploadStream = requestFTPUploader.GetRequestStream();

                        int contentLength = fileStream.Read(buffer, 0, bufferLength);

                        while (contentLength != 0)
                        {

                            uploadStream.Write(buffer, offset, contentLength);
                            contentLength = fileStream.Read(buffer, offset, bufferLength);

                            offset += contentLength;

                            uploadRate = ((float)offset / totalLength) * 100;
                            /*
                            Console.WriteLine("offset : " + offset);
                            Console.WriteLine("TotalLength : " + totalLength);
                            Console.WriteLine("Upload Rate : " + Convert.ToInt32(Math.Round(uploadRate, 2, MidpointRounding.AwayFromZero)) + "%");
                            */
                            //Progress                        
                            //prgbProcess.Value = Convert.ToInt32(Math.Round(uploadRate, 2, MidpointRounding.AwayFromZero));

                        }

                        uploadStream.Close();
                        fileStream.Close();
                        //row.QmsFileList.Add(new QmsFileInfo(fileName, Define.con_SUCCESS, fileName));
                        nFtpSuccessCnt++;

                        detail.FlagFtpSend = Define.con_SUCCESS;
                        detail.QmsfileNmChg = fileName;


                    }
                    catch (Exception exception)
                    {
                        setLog(row, "[ERROR(sendFtp)]" + exception.Message);
                        util.Log("[ERROR(sendFtp)]", exception.Message);
                        Console.WriteLine(exception.Message.ToString());
                        //row.QmsFileList.Add(new QmsFileInfo(fileName, Define.con_FAIL, fileName));

                        if (detail != null)
                        {
                            detail.FlagFtpSend = Define.con_FAIL;
                        }

                        //실패한 QMS 파일 이동 처리
                        fileInfo.MoveTo(path + "\\" + Define.con_FOLDER_UPLOAD_ERROR + "\\" + fileName);
                    }
                    Console.WriteLine(fileName + "Upload complete");
                }
            }

            row.FtpSendServer = freeServerInfo.Name;
            row.FtpSendFileCnt = nFtpFileCnt;
            row.FtpSendSuccessCnt = nFtpSuccessCnt;
            row.FtpSendFlag = Define.CON_COMPLETE;

            return true;
        }


        private void onClickMenuConvertor(Object sender, System.EventArgs e)
        {
            int nMenuIdx = ((MenuItem)sender).Index;
            execConvertor(nMenuIdx,"");
        }

        private void execUnzipList(string path)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

            startInfo.FileName = "CMD.exe";
            startInfo.WorkingDirectory = Define.CON_ZIP_EXE_PATH;
            //startInfo.Arguments = "7z.exe l D:\\FTP14\\WORK\\본사_도로501조_LTED_SKT_AUTO_테스트_20180402_20180417130636.zip > D:\\FTP14\\WORK\\test2.txt";

            startInfo.UseShellExecute = false;

            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            process.EnableRaisingEvents = false;
            process.StartInfo = startInfo;
            process.Start();
            //process.StandardInput.Write("7z.exe l D:\\FTP14\\WORK\\본사_도로501조_LTED_SKT_AUTO_테스트_20180402_20180417130636.zip > D:\\FTP14\\WORK\test2.txt" + Environment.NewLine);
            process.StandardInput.Write("7z.exe " + path + Environment.NewLine);
            process.StandardInput.Close();
            process.Close();

        }

        private void exec(string path , string arg)
        {
            try
            {
                if (arg.Length <= 0)
                {
                    System.Diagnostics.Process.Start(path);
                }
                else
                {
                    System.Diagnostics.Process.Start(path, arg);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        private void runConvertor(String flag)
        {
            try
            {
                System.Diagnostics.Process.Start("D:\\INNOWHB\\QMS-W.exe", flag);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void cboFtpServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            //서비스 감시중에는 변경 못하도록 한다.

            m_ftpId = Convert.ToString(cboFtpServer.SelectedIndex + 1);
            util.SetIniWriteString("FTP", "SELECT", getFtpName(m_ftpId), Application.StartupPath + "\\controllerSet.Ini");
        }

        private void btnService_Click(object sender, EventArgs e)
        {
            m_bServiceOnOffButton = !m_bServiceOnOffButton;
            //m_bServiceOn = m_bServiceOnOffButton; 

            setControlServiceOnOff();

        }

        private void dgvProcess_Click(object sender, EventArgs e)
        {

        }

        private void dgvProcess_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // DataGridViewImageCell cell = (DataGridViewImageCell)dgvProcess.Rows[e.RowIndex].Cells[e.ColumnIndex];

            if (e.ColumnIndex == 0)
            {
                DataGridViewTextBoxCell cell = (DataGridViewTextBoxCell)dgvProcess.Rows[e.RowIndex].Cells[4];
                string zipfileNm = (string)cell.Value;

                if (m_dicFileProcess.ContainsKey(zipfileNm))
                {
                    string fileDir = m_dicFileProcess[zipfileNm].ZipfileFullPath;
                    exec("explorer.exe", fileDir);
                }
            }
        }

        private void PopFileModify(string zipfileNm)
        {
            popFileInfo dlg = new popFileInfo();
            dlg.ZipfileInfo = m_dicFileProcessErr[zipfileNm];            
            dlg.StartPosition = FormStartPosition.CenterParent;

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {                
                // Read the contents of testDialog's TextBox.
            }
            else
            {

            }
        }

        private void dgvErrorFile_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string zipfileNm = "";
            if (e.ColumnIndex == 0)
            {
                DataGridViewTextBoxCell cell = (DataGridViewTextBoxCell)dgvErrorFile.Rows[e.RowIndex].Cells[4];
                zipfileNm = (string)cell.Value;

                if (m_dicFileProcessErr.ContainsKey(zipfileNm))
                {
                    string fileDir = m_dicFileProcessErr[zipfileNm].ZipfileFullPath;
                    exec("explorer.exe", fileDir);
                }
            }else if (e.ColumnIndex == 5)
            {
                DataGridViewTextBoxCell cell = (DataGridViewTextBoxCell)dgvErrorFile.Rows[e.RowIndex].Cells[4];
                zipfileNm = (string)cell.Value;

                if (m_dicFileProcessErr.ContainsKey(zipfileNm))
                {

                    FileProcess selInfo = m_dicFileProcessErr[zipfileNm];

                    if (selInfo.FileNameRule == Define.CON_ERROR)
                    {
                        PopFileModify(zipfileNm);
                    }else if (selInfo.FtpSendFlag == Define.CON_COMPLETE && selInfo.FtpSuccessFlag == Define.con_FAIL)
                    {
                        //FTP 재전송 한다.
                        DialogResult result = MessageBox.Show("FTP 재정송 하시겠습니까?", "FTP 재전송", MessageBoxButtons.YesNoCancel);
                        if (result == DialogResult.Yes)
                        {
                            reSendFtp(selInfo, selInfo.ZipfileFullPath);
                        }
                        
                    }
                }
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            endTimer();
            timer.Dispose();
        }

        private void dgvEnv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {

                popEnvSet dlg = new popEnvSet();
                dlg.LstFile = m_lstFile;
                dlg.LstServer = m_lstServer;
                //dlg.Parent = this;
                dlg.StartPosition = FormStartPosition.CenterParent;


                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    // Read the contents of testDialog's TextBox.
                    if (dlg.m_bSaved)
                    {
                        //재조회한다.
                        dbContollerSetInfo();
                        updateRowEnv();
                    }
                }
                else
                {

                }
            }
        }
    }
}
