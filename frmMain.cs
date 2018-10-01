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
 * rest APi Call example
 * https://stackoverflow.com/questions/9620278/how-do-i-make-calls-to-a-rest-api-using-c
 * 
 * // 오라클 클라이언트 접속
 * http://kin33.tistory.com/60
 * 
 * //Visual studio 자주쓰는 팁
 * https://msdn.microsoft.com/ko-kr/library/dn600331.aspx
 
 * */

namespace IqaController
{
    public partial class frmMain : Form
    {
        
        private Dictionary<string, FileProcess> m_dicFileProcess = new Dictionary<string, FileProcess>();           //
        private Dictionary<string, FileProcess> m_dicFileProcessErr = new Dictionary<string, FileProcess>();        //Error 파일 따로 관리(로딩시 디렉토리에서도 읽어와야함)

        private string m_ftpId = "14";

        private Boolean m_formClose = false;
        private Boolean m_bServiceOnOffButton = false;  //서비스 On Off 버튼 토글        
        private Boolean m_ServiceOnIng = false;         //서비스 처리 프로세스 진행중 여부 (true:진행중 , false:완료)

        private System.Timers.Timer timer = new System.Timers.Timer();
        //private System.Threading.Timer tmrServiceBtn = null;

        private Boolean m_bServiceColorFlag = false;

        private List<ControllerServerEntity> m_lstServer = null;          //환경설정 수집서버 
        private List<ControllerFileKeepEntity> m_lstFile = null;          //환경설정 파일 보관주기
        private List<ControllerEnvEntity> m_lstEnv = new List<ControllerEnvEntity>();

        private Boolean m_debug = false;
        
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
            //TEST 코드 
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
                dgvProcessLog.Rows[nRow].Cells[Define.con_KEY_PROCESSLOG_COLIDX].Value = row.OrgZipFileName;
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

                string zipfileNm = Path.GetFileName(dir+".zip");        //즉 폴더이름에 확장자 zip을 붙인다 - 오류 발생시 zip파일 이름 유지하기 위해


                if (files.Length > 0)
                {
                    
                    zipfileFullPath = Path.GetDirectoryName(files[0]);
                    //zipfileNm = Path.GetFileName(files[0]);
                    
                    FileProcess row = new FileProcess();
                    row.ZipFileName = zipfileNm;                //zipfile 명
                    row.OrgZipFileName = files[0];              //zip파일명 시스템 날짜 붙이기전 원래 이름
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
            string sFileNm = row.OrgZipFileName;

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

                dgvErrorFile.Rows[nRow].Cells[6].Value = row.OrgZipFileName;

            }));

        }

        /* 파일처리현황 그리드에 값 반영한다.
         * 
        */
        private void updateRowFileProcess(FileProcess row)
        {
            string sFileNm = row.OrgZipFileName;
            int nRow = -1;
            
            this.Invoke(new Action(delegate ()
            {

                if (m_dicFileProcess.ContainsKey(sFileNm))
                {
                    if (dgvProcess.RowCount == 0)
                    {
                        dgvProcess.RowCount = 1;
                        nRow = 0;
                    }
                    else
                    {
                        nRow = dgvProcess.getFindZipFile2RowIndex(sFileNm, Define.con_KEY_PROCESS_COLIDX);
                    }

                    m_dicFileProcess[sFileNm] = row;
                    
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
                dgvProcess.Rows[nRow].Cells[Define.con_KEY_PROCESS_COLIDX].Value = row.OrgZipFileName;
                dgvProcess.Rows[nRow].Cells[11].Value = row.ZipfileFullPath;

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
            dgvProcess.addColumn("측정본부", 78);
            dgvProcess.addColumn("측정조", 80);
            dgvProcess.addColumn("zip 파일명", 300);
            dgvProcess.addColumn("파일명규칙", 100);
            dgvProcess.addColumn("압축해제", 80);
            dgvProcess.addColumn("컨버전", 80);
            dgvProcess.addColumn("FTP 전송", 95);
            dgvProcess.addColumn("백업", 80);
            dgvProcess.addColumn("원본 zip 파일명", 300);
            dgvProcess.addColumn("최종작업폴더", 300);
            dgvProcess.Columns[Define.con_KEY_PROCESS_COLIDX].Visible = false;
            dgvProcess.Columns[11].Visible = false;


            dgvProcessLog.addColumn("일시", 120);
            dgvProcessLog.addColumn("측정본부", 77);
            dgvProcessLog.addColumn("측정조", 80);
            dgvProcessLog.addColumn("zip 파일명", 400);
            dgvProcessLog.addColumn("로그", 200);
            dgvProcessLog.addColumn("원본 zip 파일명", 300);
            dgvProcessLog.Columns[Define.con_KEY_PROCESSLOG_COLIDX].Visible = false;

            dgvErrorFile.addColumnButton("위치", "파일", 60);
            dgvErrorFile.addColumn("처리서버", 60);
            dgvErrorFile.addColumn("Error 분류", 120);
            dgvErrorFile.addColumn("파일 Path", 200);
            dgvErrorFile.addColumn("zip 파일명", 320);
            dgvErrorFile.addColumn("파일 재처리", 150);
            dgvErrorFile.addColumn("원본 zip 파일명", 300);
            dgvErrorFile.Columns[Define.con_KEY_PROCESSERROR_COLIDX].Visible = false;

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
        /* Zip파일 명 체크
        */
        private Boolean zipFileNamingParsing(FileProcess row )
        {
            //진행중 보여줘야하면 아래 풀자
            //row.CurState = Define.con_STATE_START_NAMING;
            //iqaService.updateZipfileInfo(row, Define.con_ING_START);
            
            //복사할 경로에 해당 파일 유무 체크
            string path = getDefaultFtpPath();
            //string onlyFileName = Path.GetFileName(fileName);            
            string withoutExtensionName = Path.GetFileNameWithoutExtension(row.ZipFileName);            
            string sMoveDir = Path.GetFileNameWithoutExtension(row.ZipFileName);

            string sWorkFileFullName = row.ZipfileFullPath + "\\" + row.ZipFileName;
            string sMoveFileFullName = path + "\\" + Define.con_DIR_NAMING + "\\" + sMoveDir + "\\" + row.OrgZipFileName;

            string[] arFileName = withoutExtensionName.Split('_');
            string sFileDate = "";
            DateTime dtDate;
            DateTime dtMaxDate;

            //","가 파일명에 있을시 7Zip이 압축해제시 관련 정보를 남기지 못함
            if (arFileName.Length != 8 || row.ZipFileName.IndexOf(",") >= 0)     //뒤에 시스템 날짜 더 붙임
            {
                //구분자 오류                                
                row.FileNameRule = Define.CON_ERROR;                       
                updateRowFileProcess(row);

                if (row.ZipFileName.IndexOf(",") >= 0)
                {
                    setLog(row, "Zip 파일명 콤마 오류(콤마 입력 불가)");
                }
                else
                {
                    setLog(row, "구분자 오류");
                }

                DirectoryInfo diChk = new DirectoryInfo(path + "\\" + Define.con_DIR_NAMING + "\\" + sMoveDir);
                if (diChk.Exists == false)
                {
                    diChk.Create();
                }

                row.CurState = Define.con_STATE_ERR_NAMING;
                fileSave("move", sWorkFileFullName, sMoveFileFullName);

                row.ZipfileFullPath = path + "\\" + Define.con_DIR_NAMING + "\\" + sMoveDir;
                return false;
            }

            //<START> ############# 파일 유효 기간 체크
            sFileDate = arFileName[6];
            dtDate = DateTime.ParseExact(sFileDate, "yyyyMMdd", null);
            dtMaxDate = dtDate.AddDays(30);                                //최대 30일 유효범위임             
            int nCompare = DateTime.Compare(dtMaxDate, DateTime.Now);
            if (nCompare < 0)
            {
                
                row.CurState = Define.con_STATE_ERR_NAMING;
                row.FileNameRule = Define.CON_ERROR;                
                updateRowFileProcess(row);
                setLog(row, "유효기간 오류");

                DirectoryInfo diChk = new DirectoryInfo(path + "\\" + Define.con_DIR_NAMING + "\\" + sMoveDir);
                if (diChk.Exists == false)
                {
                    diChk.Create();
                }
                row.CurState = Define.con_STATE_ERR_NAMING;
                fileSave("move", sWorkFileFullName, sMoveFileFullName);

                row.ZipfileFullPath = path + "\\" + Define.con_DIR_NAMING + "\\" + sMoveDir;
                return false;
            }

            row.FileNameRule = Define.CON_COMPLETE;
            row.MeasuBranch = arFileName[0];
            row.ServiceType = arFileName[2];            
            row.MeasuGroup = arFileName[1];
            row.Comp = arFileName[3];

            //아래 부분 null인경우 체크할지는 서버단 필요시 처리
            if (row.MeasuGroup.IndexOf("도로") >= 0)
            {
                row.InoutType = "OUTDOOR";
            }
            else if (row.MeasuGroup.IndexOf("인빌딩") >= 0)
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
            }
            else if (row.Comp == "SKT")
            {
                row.Comp = "016";
            }
            else if (row.Comp == "LGU")
            {
                row.Comp = "019";
            }

            row.MeasuDate = sFileDate;
            row.FileNameRule = Define.CON_COMPLETE;
            row.CurState = Define.con_STATE_COMPLETE_NAMING;

            //iqaService.updateZipfileInfo(row, Define.con_ING_START);
            return true;
        } 

        private Boolean dbContollerSetInfo()
        {
            m_lstFile = iqaService.getControllerFilePeriod();          
            m_lstServer = iqaService.getControllerServer();           
            m_lstEnv.Clear();

            if (m_lstServer == null || m_lstServer.Count <= 0)
            {
                util.Log("[ERROR]", "서버정보 없음");
                return false;
            }


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

        /* DB 중복 ZIP FILE 명 체크 - 
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

                if (result.Flag == 1)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                util.Log("[ERR]:[dbZipFileDupChk]", ex.Message);
                Console.WriteLine(ex.Message.ToString());
                return false;
            }
        }
        /*
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

                if (result.Flag == 1)
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
        */
        /* 사용안함
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
        */
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
            
            NameValueCollection postData = new NameValueCollection();

            postData.Add("zipfileNm", HttpUtility.UrlEncode(row.ZipFileName));
            postData.Add("unzipFileNm", unZipStr);

            string pagesource = Encoding.UTF8.GetString(webClient.UploadValues(uri, postData));
            try
            {

                /*
                 - 중복 drm 파일/dml파일명 의 경우는 해당 디렉토리 내에 별도 디렉토리 생성하여 임시 이동(초반 백업으로 이동하는게 의미 없음)
                 - 중복 drm 파일/dml파일명 정보는 Log 저장
                 - 중복 아닌 경우는 컨버젼 진행 대상으로 처리 
                */

                List<CodeNameEntity> dupList = (List<CodeNameEntity>)Newtonsoft.Json.JsonConvert.DeserializeObject(pagesource, typeof(List<CodeNameEntity>));
                // Console.WriteLine(dupList.Count);
                
                if (dupList.Count > 0)
                {
                    //컨버전에서 제외하기 위해 해당 파일 제거한다.
                    foreach(CodeNameEntity tmp in dupList)
                    {
                        File.Delete(dupChkDir + "\\"+tmp.Code);
                    }     
                }
            }
            catch (Exception ex)
            {
                setLog(row, "[ERR]:[unzipFileDetailProc]:" + ex.Message);
                util.Log("[ERR]:[unzipFileDetailProc]", ex.Message);
                Console.WriteLine(ex.Message.ToString());
            }
        }

        private Boolean fileSave(string flag,string sourceFileName , string destiFileName)
        {
            int nMax = 2000;
            int nAttemptCnt = 0;
            Boolean bDo = true;
            Boolean bSuccess = false;

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
                    bSuccess = true;
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
                            util.Log("[ERR]:fileSave", "파일권한 무한 대기 발생");
                        }
                    }
                    else
                    {
                        util.Log("[ERR]:fileSave", ex.Message);
                        bDo = false;
                    }
                }
            }

            return bSuccess;
        }
        /* FTP 전송실패 또는 FTP 전송 못한 QMS파일 백업
         */
        private void setFtpError2FileMove(FileProcess row)
        {
            if (row.FtpSendFileCnt <= row.FtpSendSuccessCnt)
            {
                return;
            }

            /*
               오류 폴더는 ZIP파일내임은 MOVE내임으로
               안에는 오리지날 파일내임으로 한다.
               추후 폴더구조로 로딩시 Zip 파일내임과 orgZip파일이름 동일시 하기 위해
            */

            string movePath = getDefaultFtpPath();
            movePath += movePath + "\\" + Define.con_DIR_UPLOAD + "\\" + row.ZipFileName;            
            //UPLOAD ERROR 폴더 생성
            DirectoryInfo diChk = new DirectoryInfo(movePath);
            if (diChk.Exists == false)
            {
                diChk.Create();
            }

            //row.ZipfileFullPath = movePath;

            foreach (ZipFileDetailEntity detail in row.ZipfileDetailList)
            {
                if (detail.FlagFtpSend != Define.con_SUCCESS)
                {
                    fileSave("move", row.ZipfileFullPath + "\\" + detail.QmsfileNm, movePath + "\\" + detail.QmsfileNm);
                    
                }
            }
        }
        /* Zip 파일 분석 처리 프로세스
        */
        private void zipfileParsing(string fileName)
        {
            //string sServiceType = "";
            string tmpZipfilePath = "";
            string path = getDefaultFtpPath();                        
            string onlyFileName = Path.GetFileName(fileName);         
            string withoutExtensionName = Path.GetFileNameWithoutExtension(fileName);
            
            //처리 완료또는 패스할 조건일경우
            if (m_dicFileProcess.ContainsKey(onlyFileName))
            {
                if (m_dicFileProcess[onlyFileName].Pass || m_dicFileProcess[onlyFileName].Complete) return;                
            }

            FileProcess row = m_dicFileProcess[onlyFileName]; 
            row.CurState = Define.con_STATE_WORK;
            updateRowFileProcess(row);

            try
            {
                //Log 그리드 초기화
                this.Invoke(new Action(delegate ()
                {
                    dgvProcessLog.RowCount = 0;
                }));

                setLog(row, "작업폴더 파일이동");

                tmpZipfilePath = path + "\\" + Define.con_DIR_WORK;
                row.ZipfileFullPath = path + "\\" + Define.con_DIR_WORK;

                if (!fileSave("move", fileName, tmpZipfilePath + "\\" + row.ZipFileName))
                {
                    //파일이동 자체가 실패하였기 떄문에 오류로 오류폴더로 파일을 이동하는게 의미 없음
                    row.Pass = true;
                    setLog(row, "작업폴더 파일이동 실패");                    
                    return;
                }
                row.ZipfileFullPath = tmpZipfilePath;

                FileInfo fInfo = new FileInfo(row.ZipfileFullPath + "\\" + row.ZipFileName);
                row.FileSize = fInfo.Length.ToString();

                setLog(row, "파일이동 완료");
                setLog(row, "파일명 규칙 분석");
                if (!zipFileNamingParsing(row))
                {
                    row.Pass = true;
                    updateRowErrorFile(row);
                    iqaService.updateZipfileMainInfo(row,Define.con_STATE_START_NAMING);
                    return;
                }
                row.BackupFlag = Define.CON_ING;
                
                updateRowFileProcess(row);
                setLog(row, "파일명 규칙 완료");

                //dbUpdateZipFileInfo(row, "");
                iqaService.updateZipfileMainInfo(row, Define.con_STATE_START_NAMING);

                //<START> ############# 파일 백업 및 작업 디렉토리로 이동                
                setLog(row, "백업");

                if (!fileSave("copy", path + "\\WORK\\" + row.ZipFileName, path + "\\BACK\\" + row.ZipFileName))
                {
                    row.Pass = true;
                    setLog(row, "백업 실패");
                    return;
                }

                setLog(row, "백업 완료");
                row.BackupFlag = Define.CON_COMPLETE;
                //<END>

                //<START> ############# 압축 해제                
                setLog(row, "압축해제");
                if (!UnZIpProc(row)){
                    row.Pass = true;
                    updateRowErrorFile(row);                    
                    iqaService.updateZipfileMainInfo(row, "");
                    return;
                }
                setLog(row, "압축해제 완료");
                updateRowFileProcess(row);
                iqaService.updateZipfileMainInfo(row, "");
                //<END> 압축 해제 완료


                Thread.Sleep(2000);
                setLog(row, "중복 Drm 체크");
                //<START> ############# Unzip 파일 정보 DB 업로드및 중복 Drm 제거 처리                
                string extractDir = getUnzipPath(row);

                //이곳에서 unzip 파일 및 dup파일 정보 업데이트 한다.
                unzipFileDetailProc(row , extractDir);
                setLog(row, "중복 Drm 체크 완료");                
                //#############################################</END>

                //모두 중복 제거일경우 컨버터 호출하지 않는다.
                string[] extractFile = Directory.GetFiles(extractDir);

                if (extractFile.Length <= 1)        //압축 로그정보파일 떄문에 1개파일은 존재한다.
                {
                    row.Pass = true;
                    row.CurState = Define.con_STATE_ERR_DUP_ALL;
                    iqaService.updateZipfileMainInfo(row,"");
                    setLog(row, "모든 파일 중복 Drm");
                    return;
                }
                //<START> ############# 컨버터 실행
                Thread.Sleep(2000);
                setLog(row, "컨버터 실행");

                row.ConversionFlag = Define.CON_ING;
                row.CurState = Define.con_STATE_START_CONV;
                updateRowFileProcess(row);
                iqaService.updateZipfileMainInfo(row, Define.con_STATE_START_CONV);

                string runConvertorTime = DateTime.Now.ToString("yyyyMMddHHmmss");                
                string sdeCompressPath = getUnzipPath(row);
                runServyceType2RunConvertor(row.ServiceType, sdeCompressPath);
                

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
                //#############################################</END>


                if (sResultFindFileName.Length > 0 )
                {
                    if (!convertResultProc(row, sResultFindFileName))
                    {
                        row.Pass = true;
                        return;
                    }

                    setLog(row, "FTP 파일전송");
                    ControllerServerEntity freeFtpServer = iqaService.getFreeFtpServerInfo();
                    if (freeFtpServer == null)
                    {
                        row.Pass = true;
                        row.FtpSuccessFlag = Define.con_FAIL;                        
                        setLog(row, "FTP 서버 정보 가져오기 실패");
                        util.Log("[ERR]:[zipfileParsing]", "FTP 서버 정보 가져오기 실패");
                        setFtpError2FileMove(row);
                        updateRowErrorFile(row);

                        return;
                    }
                    Boolean bFtpSuccess = false;
                    //FTP 전송 (모듈이 나뉘고 있어서 상용도 SFTP로 요청하자)
                    if (m_debug)
                    {
                        bFtpSuccess = sendSFtp(row, freeFtpServer,extractDir, false);
                    }
                    else
                    {
                        bFtpSuccess = sendFtp(row, freeFtpServer,extractDir, false);
                    }

                    if (!bFtpSuccess)
                    {
                        row.Pass = true;
                        setFtpError2FileMove(row);
                    }
                    setLog(row, "파일정송 완료");
                }
                //정상 처리 로그 남긴다.                        1
                row.FtpSendFlag = Define.CON_COMPLETE;
                row.CurState = Define.con_STATE_COMPLETE_FTP;
                row.Complete = true;
                setLog(row, "처리 완료");
                updateRowFileProcess(row);
                //row.CurState = Define.con_STATE_WORK_COMPLETE;      //기현선임 작업이 있어 서버와 협의 필요 서버에서 완료가되야 완료임
                //ZIp파일 완료 DB Update

                iqaService.updateZipFileAllInfo(row);

                //FTP 전송 실패한 qms파일을 백업이동한다. - 위에서 처리 하고 있음
                
                //worK 작업 파일 관련 삭제 처리
                if (deleteWorkZipFile(extractDir))
                {
                    //zip파일 삭제 처리
                    //row.Complete = true;
                    setLog(row, "작업파일 삭제 완료");
                }
                else
                {
                    //row.Pass = true;
                    setLog(row, "작업파일 삭제 실패");
                }
                row.Complete = true;
            }
            catch (Exception ex)
            {
                setLog(row, "[ERR]:[zipfileParsing]:" + ex.Message);
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
                util.Log("[ERR]:[dbUpdateZipfileComplete.do]", ex.Message);                
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

                if (totalSize == "Errors:")
                {
                    Console.WriteLine("ERROR 발생");
                }
                else
                {
                    /* 실제 로그에 찍힌 총 바이트수가 틀릴경우가 발생해서  */
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
        }

        private long getDirectoryTotalSize(string sDirPath)
        {
            // Get array of all file names.
            /*
            string[] a = Directory.GetFiles(sDirPath, "*.*");            
            long b = 0;
            foreach (string name in a)
            {
            
                FileInfo info = new FileInfo(name);
                if (info.Extension == ".drm" || info.Extension == ".dml")
                {
                    b += info.Length;
                }
            }
            
            return b;
            */

            DirectoryInfo dir = new DirectoryInfo(sDirPath);
            long totalSize = CalculateDirectorySize(dir, true);

            return totalSize;
        }

        long CalculateDirectorySize(DirectoryInfo directory, bool includeSubdirectories)
        {
            long totalSize = 0;

            FileInfo[] files = directory.GetFiles();
            foreach (FileInfo file in files)
            {
                totalSize += file.Length;
            }

            if (includeSubdirectories)
            {
                DirectoryInfo[] dirs = directory.GetDirectories();
                foreach (DirectoryInfo dir in dirs)
                {
                    totalSize += CalculateDirectorySize(dir, true);
                }
            }

            return totalSize;
        }

        private Boolean convertResultProc(FileProcess row, string filePath)
        {

            string[] lines = System.IO.File.ReadAllLines(filePath, Encoding.Default);

            // string array
            string flag = "";
            string desc = "";
            string[] tmpResult = null;
          
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

                    ZipFileDetailEntity detail = row.findZipfileDetail("unzipNm", tmpResult[0]);

                    if (detail != null)
                    {
                        nQmsFileCnt++;
                        detail.QmsfileNm = tmpResult[1];
                        detail.FlagConvertorResult = flag;
                        detail.DescConvertorResult = desc;
                        detail.QmsfileNmChg = getQmaNameChange(Convert.ToString(nQmsFileCnt));
                    }
                }
                else
                {
                    ZipFileDetailEntity detail = row.findZipfileDetail("unzipNm", tmpResult[0]);
                    if (detail != null)
                    {
                        //nQmsFileCnt++;
                        detail.QmsfileNm = "";                  //파생 안됨
                        detail.FlagConvertorResult = "0";
                        detail.DescConvertorResult = log;
                        //detail.QmsfileNmChg = getQmaNameChange(Convert.ToString(nQmsFileCnt));
                    }

                }
            }
            //오류가 발생시 오류항먹은 따로 컨버터 오류 폴더에 백업한다.
            if (bConvertError)
            {
                string path = getDefaultFtpPath();
                string sMoveDir = Path.GetFileNameWithoutExtension(row.OrgZipFileName);

                DirectoryInfo diChk = new DirectoryInfo(path + "\\" + Define.con_DIR_CONVERT + "\\" + sMoveDir);
                if (diChk.Exists == false)
                {
                    diChk.Create();
                }

                row.ConversionFlag = Define.CON_ERROR;                                
                File.Move(path + "\\WORK\\" + row.ZipFileName, path + "\\" + Define.con_DIR_CONVERT + "\\" + sMoveDir + "\\" + row.OrgZipFileName);
                row.ZipfileFullPath = path + "\\" + Define.con_DIR_CONVERT + "\\"+ sMoveDir;

                updateRowErrorFile(row);
            }
            else
            {
                row.CurState = Define.con_STATE_COMPLETE_CONV;
            }

            SaveResultInfo res = iqaService.updateZipFileAllInfo(row);

            if (res.Flag == 0)
            {
                setLog(row, res.Desc);
                util.Log("[ERR]:[convertResultProc]", res.Desc);
                return false;
            }
            return true;
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
                util.Log("[ERR]:[unZipFileChk]", ex.Message);
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
        /*
        작업에 필요한 폴더를 생성한다.
        */
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

        private void deleteVariableFileProcessProc()
        {

            if (m_dicFileProcess.Count <= 0)
            {
                return;
            }

            List<string> lstDelKey = new List<string>();

            foreach (var key in m_dicFileProcess.Keys.ToList())
            {

                String zipFileName = m_dicFileProcess[key].ZipFileName;
                string[] arFileInfo = zipFileName.Split('_');
                var fileWorkDate = arFileInfo[arFileInfo.Length - 1];
                fileWorkDate = Path.GetFileNameWithoutExtension(fileWorkDate);

                DateTime dtCurDate = DateTime.Now;
                DateTime dtFileWirkDate = DateTime.ParseExact(fileWorkDate, "yyyyMMddHHmmss", null);
                dtFileWirkDate = dtFileWirkDate.AddDays(30);

                int nCompare = DateTime.Compare(dtFileWirkDate, dtCurDate);
                if (nCompare < 0)
                {
                    //삭제 대상 : 30일 지난 파일 리스트는 삭제 한다.
                    lstDelKey.Add(key);                
                }
            }
            foreach(string key in lstDelKey){
                if (key != null && key != "")
                {

                    //화면 그리드 삭제 한다.
                    int nDeleteRow = 0;                    
                    nDeleteRow = dgvProcess.getFindZipFile2RowIndex(key, Define.con_KEY_PROCESS_COLIDX);

                    if (nDeleteRow >= 0)
                    {
                        dgvProcess.Rows.RemoveAt(nDeleteRow);
                    }
                    m_dicFileProcess.Remove(key);
                    
                }
            }            
        }
        private void serviceOnThreadProc()
        {            
            string path = getDefaultFtpPath();
            if (Directory.Exists(path))
            {
                m_ServiceOnIng = true;
                appWorkDirMake(path);

                //하루 지난거만 초기화 한다.                
                deleteVariableFileProcessProc();

                string[] fileEntries = Directory.GetFiles(path);
                //ZIP파일정보 미리 DB 업뎃하기 위해
                List<FileProcess> dbInsertList = new List<FileProcess>();
                foreach (string fileName in fileEntries)
                {
                    string onlyFileName = Path.GetFileName(fileName);
                    string moveFileName = getMoveFileName(fileName);                    
                    string fullPath = Path.GetDirectoryName(fileName);
                    
                    FileProcess row = new FileProcess();
                    row.ProcessServer = getFtpName(m_ftpId);                        
                    row.ZipFileName = moveFileName;             //2018-06-14 : Zip 파일명도 시스템 날짜 추가된거로 관리하는거로 요청옴                                                                    
                    row.OrgZipFileName = onlyFileName;
                    row.ZipfileFullPath = fullPath;
                    row.CurState = Define.con_STATE_WAIT;                    
                    m_dicFileProcess.Add(onlyFileName,row);

                    dbInsertList.Add(row);

                }
                //DRM 중복체크는 안하는거로 함 어짜피 drm 중복체크를 하고 또 ZIP파일을 다시 올릴수도있어서 중간 실패시 문제가 될수있어 체크 안함
                if (dbInsertList.Count > 0)
                {
                    SaveResultInfo res = iqaService.insertZipfileInfos(dbInsertList);
                    if (res.Flag == 0)
                    {
                        //DB 저장이 안되어 그이후 행위 의미 없음
                        return;
                    }
                }
                //m_dicFileProcess 변수로 처리할지 좀더 고민
                //foreach (string fileName in fileEntries)                
                //foreach (var key in m_dicFileProcess.Keys.ToList())
                //컬렉션이 수정되었습니다. 열거 작업이 실행되지 않을 수도 있습니다.' 아래 참조
                //http://www.jumptovb.net/tag/%EC%97%B4%EA%B1%B0%20%EC%9E%91%EC%97%85%EC%9D%B4%20%EC%8B%A4%ED%96%89%EB%90%98%EC%A7%80%20%EC%95%8A%EC%9D%84%20%EC%88%98%EB%8F%84%20%EC%9E%88%EC%8A%B5%EB%8B%88%EB%8B%A4
                //foreach (KeyValuePair <string, FileProcess> pair in m_dicFileProcess)
                foreach (var key in m_dicFileProcess.Keys.ToList())
                {
                    //중도 중단 시켰을경우
                    if (!m_bServiceOnOffButton) break;
                    //zipfileParsing(pair.Value.ZipfileFullPath+"\\"+ pair.Value.OrgZipFileName);
                    zipfileParsing(m_dicFileProcess[key].ZipfileFullPath + "\\" + m_dicFileProcess[key].OrgZipFileName);

                    //zipfileParsing(fileName);
                    Thread.Sleep(4000);
                }
                
                m_ServiceOnIng = false;
                Thread.Sleep(500);
                
                if (m_bServiceOnOffButton)
                {
                    if (!m_formClose)
                    {
                        //서비스 On 상태면 재귀호출한다.
                        startTimer();
                        serviceOnThreadProc();
                    }                    
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
        
        private string getUnzipPath(FileProcess row)
        {
            string defaultPath = getDefaultFtpPath();
            string sWithoutExtensionName = Path.GetFileNameWithoutExtension(row.ZipfileFullPath + "\\" + row.ZipFileName);
            string extractDir = defaultPath + "\\WORK\\" + sWithoutExtensionName;
            return extractDir;
        }

        /* 압축 해제 한다. (실패시 어떻게 파악하나? 압축해제 실패시 확인방법 찾기)
         * 
         */
        private Boolean UnZIpProc(FileProcess row)
        {
            row.ExtractFlag = Define.CON_ING;
            row.CurState = Define.con_STATE_START_UNZIP;
            iqaService.updateZipfileMainInfo(row, Define.con_STATE_START_UNZIP);


            //row.ZipfileFullPath = row.ZipfileFullPath + "\\WORK";
            //string fileFullPath = row.ZipfileFullPath + "\\";
                                         //이동된 File 디렉토리
            updateRowFileProcess(row);

            string extractDir = getUnzipPath(row);
            /*
            string sWithoutExtensionName = Path.GetFileNameWithoutExtension(fileFullPath);
            string extractDir = m_defaultFolder + m_ftpId + "\\WORK\\"+ sWithoutExtensionName;
            */
            DirectoryInfo di = new DirectoryInfo(extractDir);
            if (di.Exists == false)
            {
                di.Create();
            }

            try
            {
                //string defaultPath = getDefaultFtpPath();
                //defaultPath += "\\WORK\\";
                //https://m.blog.naver.com/PostView.nhn?blogId=koromoon&logNo=120208838111&proxyReferer=https%3A%2F%2Fwww.google.co.kr%2F
                ////process.StandardInput.Write("7z.exe l D:\\FTP14\\WORK\\본사_도로501조_LTED_SKT_AUTO_테스트_20180402_20180417130636.zip > D:\\FTP14\\WORK\test2.txt" + Environment.NewLine);
                execUnzipList("l " + row.ZipfileFullPath + "\\" + row.ZipFileName + " > " + extractDir + "\\unzipInfo.txt");

                //exec(Define.CON_ZIP_EXE_PATH+"7z.exe", "x -o" + extractDir + "\\ -r -y " + fileFullPath);
                //exec(Define.CON_ZIP_EXE_PATH + "7z.exe", "x -o" + extractDir + "\\ -r -y " + row.ZipfileFullPath + "\\" + row.ZipFileName);
                exec(Define.CON_ZIP_EXE_PATH + "7z.exe", "x -o" + extractDir + "\\ -r -y " + row.ZipfileFullPath + "\\" );

                unZipCompleteChk(extractDir + "\\" + "unzipInfo.txt", extractDir);
            }
            catch (Exception ex)
            {
                row.ExtractFlag = Define.CON_ERROR;
                row.CurState = Define.con_STATE_ERR_UNZIP;
                setLog(row, "압축 해제 실패");
                util.Log("ERROR", "압축 해제 실패");
                return false;
            }

            row.ExtractFlag = Define.CON_COMPLETE;
            row.CurState = Define.con_STATE_COMPLETE_UNZIP;

            return true;
           
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
            
            //바로 파싱 들어가기 떄문에 굳이 여기서 업뎃 할필요 없을듯함
            //iqaService.updateZipfileInfo(row, "");
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

            ControllerServerEntity freeFtpServer = iqaService.getFreeFtpServerInfo();

            if (freeFtpServer == null)
            {
                setLog(row, "FTP 서버 정보 가져오기 실패");
                util.Log("ERROR", "FTP 서버 정보 가져오기 실패");
                return;
            }

            //qmnsNm , qmsNmChg ,구성하여 업데이트 해야함
            if (m_debug)
            {
                sendSFtp(row, freeFtpServer,localPath, true);
            }
            else
            {
                sendFtp(row, freeFtpServer ,localPath, true);
            }

            //성공시 폴더 삭제
            

            //dbUpdate한다.

            row.FtpSendFlag = Define.CON_COMPLETE;
            row.Complete = true;
            setLog(row, "처리 완료");
            updateRowFileProcess(row);
            row.CurState = Define.con_STATE_WORK_COMPLETE;

            //dbUpdateZipfileComplete(row);
            iqaService.updateZipFileAllInfo(row);

        }

        private Boolean sendSFtp(FileProcess row , ControllerServerEntity freeServerInfo , string localPath, Boolean flagResend)
        {
        
            WebClient webClient = new WebClient();
            NameValueCollection postData = new NameValueCollection();

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
                        if (extension.ToUpper() != ".QMS")
                        {
                            continue;
                        }

                        fileName = Path.GetFileName(filepath);                        
                        ZipFileDetailEntity detail = row.findZipfileDetail("qmsNm", fileName);

                        if (detail != null)
                        {
                            nFtpFileCnt++;                            
                            try
                            {
                                using (FileStream fs = new FileStream(filepath, FileMode.Open))
                                {
                                    long totalLength = fs.Length;
                                    client.BufferSize = (uint)totalLength;
                                    client.UploadFile(fs, detail.QmsfileNmChg);                                    
                                    nFtpSuccessCnt++;
                                    detail.FlagFtpSend = Define.con_SUCCESS;                                    
                                }
                            }
                            catch (Exception exFtp)
                            {
                                setLog(row, "[ERR]:[sendSFtp]:" + exFtp.Message);
                                util.Log("[ERR]:[sendSFtp]:", exFtp.Message);
                                if (detail != null)
                                {
                                    detail.FlagFtpSend = Define.con_FAIL;
                                }                                
                            }
                        }                        
                    }

                    row.FtpSendServer = freeServerInfo.Name;
                    row.FtpSendFileCnt = nFtpFileCnt;
                    row.FtpSendSuccessCnt = nFtpSuccessCnt;
                    row.FtpSendFlag = Define.CON_COMPLETE;

                    //client.ChangeDirectory(freeServerInfo.Path);
                    //WAV FILE 전송
                    string[] directories = Directory.GetDirectories(localPath);

                    client.ChangeDirectory(freeServerInfo.Path+"/MOV/");
                    String waveFolder = "";
                    string sUnzipFile = "";
                    foreach (string directoryPath in directories)
                    {
                        
                        sUnzipFile = Path.GetFileNameWithoutExtension(directoryPath);                        
                        ZipFileDetailEntity detail = row.findZipfileDetail("onlyUnzipNm", sUnzipFile);                        
                        if (detail == null)
                        {
                            continue;
                        }

                        //WAVE 폴더 Qms내부 이름으로 디렉토리 생성        //확장자 포함으로 폴더 생성요청
                        waveFolder = Path.GetFileName(detail.QmsfileNmChg);
                        client.CreateDirectory(freeServerInfo.Path + "/MOV/" + waveFolder);
                        client.ChangeDirectory(freeServerInfo.Path + "/MOV/"+ waveFolder + "/");

                        List<SendFileInfo> lstWaveFileInfo = new List<SendFileInfo>();

                        string[] wavFiles = Directory.GetFiles(directoryPath);
                        foreach (string wavFilePath in wavFiles)
                        {
                            string extension = Path.GetExtension(wavFilePath);
                            string wavFileName = Path.GetFileName(wavFilePath);

                            if (extension.ToUpper() != ".WAV")
                            {
                                continue;
                            }

                            SendFileInfo wavInfo = new SendFileInfo();
                            wavInfo.FileName = wavFileName;

                            //WAV면 전송한다.
                            try
                            {
                                using (FileStream fs = new FileStream(wavFilePath, FileMode.Open))
                                {
                                    long totalLength = fs.Length;

                                    client.BufferSize = (uint)totalLength;
                                    client.UploadFile(fs, wavFileName);                                    
                                    wavInfo.FlagSend = "1";
                                }
                            }
                            catch (Exception exFtp)
                            {
                                setLog(row, "[ERR]:[sendSFtp Wav]:" + exFtp.Message);
                                util.Log("[ERR]:{sendSFtp Wav]:", exFtp.Message);
                                wavInfo.FlagSend = "0";
                            }

                            lstWaveFileInfo.Add(wavInfo);
                        }

                        if (lstWaveFileInfo.Count > 0)
                        {
                            detail.IsWaveFile = "1";
                            detail.WaveFileInfo = lstWaveFileInfo;
                        }
                        else
                        {
                            detail.IsWaveFile = "0";
                        }                        
                    }
                    client.Disconnect();
                }
            }
            catch(Exception exception)
            {
                setLog(row, "[ERR]:[sendSFtp]:" + exception.Message);
                util.Log("[ERR]:[getControllerFilePeriod.do]", exception.Message);                
                return false;
            }
            return true;
        }

        private Boolean sendFtp(FileProcess row , ControllerServerEntity freeServerInfo , string localPath , Boolean flagResend)
        {

            int nFtpFileCnt = 0;
            int nFtpSuccessCnt = 0;
            string fileName = "";
            string ftpUrl = "ftp://" + freeServerInfo.Ip + ":" + freeServerInfo.Port + "/" + freeServerInfo.Path;

            try
            {

                Ftp ftpClient = new Ftp(ftpUrl, freeServerInfo.Id, freeServerInfo.Password);
                string[] files = Directory.GetFiles(localPath);

                foreach (string filepath in files)
                {
                    string extension = Path.GetExtension(filepath);
                    //qms만 전송한다.
                    if (extension.ToUpper() != ".QMS")
                    {
                        continue;
                    }

                    fileName = Path.GetFileName(filepath);
                    ZipFileDetailEntity detail = row.findZipfileDetail("qmsNm", fileName);

                    if (detail != null)
                    {
                        nFtpFileCnt++;                        
                        Boolean bSuccess = ftpClient.upload(detail.QmsfileNmChg, filepath);
                        if (bSuccess)
                        {
                            nFtpSuccessCnt++;
                            detail.FlagFtpSend = Define.con_SUCCESS;
                        }
                        else
                        {
                            setLog(row, "[ERR]:[sendSFtp]:" + fileName + " FTP 전송 에러발생");
                            detail.FlagFtpSend = Define.con_FAIL;                           
                        }                        
                    }
                }

                row.FtpSendServer = freeServerInfo.Name;
                row.FtpSendFileCnt = nFtpFileCnt;
                row.FtpSendSuccessCnt = nFtpSuccessCnt;
                row.FtpSendFlag = Define.CON_COMPLETE;

                //WAV FILE 전송
                string[] directories = Directory.GetDirectories(localPath);
                String waveFolder = "";
                string sUnzipFile = "";

                foreach (string directoryPath in directories)
                {

                    sUnzipFile = Path.GetFileNameWithoutExtension(directoryPath);
                    ZipFileDetailEntity detail = row.findZipfileDetail("onlyUnzipNm", sUnzipFile);
                    if (detail == null)
                    {
                        continue;
                    }

                    //WAVE 폴더 Qms내부 이름으로 디렉토리 생성        //확장자 포함으로 폴더 생성요청
                    waveFolder = Path.GetFileName(detail.QmsfileNmChg);
                    ftpClient.createDirectory("MOV/" + waveFolder);

                    //client.ChangeDirectory(freeServerInfo.Path + "/MOV/" + waveFolder + "/");

                    List<SendFileInfo> lstWaveFileInfo = new List<SendFileInfo>();

                    string[] wavFiles = Directory.GetFiles(directoryPath);
                    foreach (string wavFilePath in wavFiles)
                    {
                        string extension = Path.GetExtension(wavFilePath);
                        string wavFileName = Path.GetFileName(wavFilePath);

                        if (extension.ToUpper() != ".WAV")
                        {
                            continue;
                        }

                        SendFileInfo wavInfo = new SendFileInfo();
                        wavInfo.FileName = wavFileName;

                        ftpClient.createDirectory("MOV/" + waveFolder);

                        Boolean bSuccess = ftpClient.upload("MOV/"+ waveFolder + "/" + wavFileName, wavFilePath);
                        if (bSuccess)
                        {
                            nFtpSuccessCnt++;
                            detail.FlagFtpSend = Define.con_SUCCESS;
                            wavInfo.FlagSend = "1";
                        }
                        else
                        {
                            setLog(row, "[ERR]:[sendSFtp Wav]:" + fileName + " FTP 전송 에러발생");
                            detail.FlagFtpSend = Define.con_FAIL;
                            wavInfo.FlagSend = "0";
                        }

                        lstWaveFileInfo.Add(wavInfo);
                    }

                    if (lstWaveFileInfo.Count > 0)
                    {
                        detail.IsWaveFile = "1";
                        detail.WaveFileInfo = lstWaveFileInfo;
                    }
                    else
                    {
                        detail.IsWaveFile = "0";
                    }
                }
            }
            catch (Exception exception)
            {
                setLog(row, "[ERR]:[sendFtp]:" + exception.Message);
                util.Log("[ERR]:[getControllerFilePeriod.do]", exception.Message);
                return false;
            }
            return true;


            /*
            // https://docs.microsoft.com/ko-kr/dotnet/api/system.io.filestream.read?f1url=https%3A%2F%2Fmsdn.microsoft.com%2Fquery%2Fdev15.query%3FappId%3DDev15IDEF1%26l%3DKO-KR%26k%3Dk(System.IO.FileStream.Read);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.5);k(DevLang-csharp)%26rd%3Dtrue&view=netframework-4.7.1
            // http://codingcoding.tistory.com/50
            // 추후 아래 두개 모듈참조하여 connetion 한번 맺고 처리하는거로 변환하자
            //https://docs.microsoft.com/ko-kr/dotnet/framework/network-programming/how-to-upload-files-with-ftp
            //https://msdn.microsoft.com/ko-kr/library/system.net.ftpwebrequest.getrequeststream(v=vs.110).aspx

            row.FtpSendServer = freeServerInfo.Name;

            int nFtpFileCnt = 0;
            int nFtpSuccessCnt = 0;

            FtpWebRequest requestFTPUploader = null;
            string[] files = Directory.GetFiles(localPath);
            Boolean bFtpSuccess = true;

            foreach (string filepath in files)
            {
                string extension = Path.GetExtension(filepath);

                //qms만 전송한다.
                if (extension.ToUpper() != ".QMS")
                {
                    continue;
                }

                string fileName = Path.GetFileName(filepath);

                ZipFileDetailEntity detail = row.findZipfileDetail("qmsNm", fileName);

                if (detail != null)
                {
                    string ftpUrl = "ftp://" + freeServerInfo.Ip + ":" + freeServerInfo.Port + "/" + freeServerInfo.Path + detail.QmsfileNmChg;
                    nFtpFileCnt++;

                    detail.FlagFtpSend = Define.con_SUCCESS;

                    requestFTPUploader = (FtpWebRequest)WebRequest.Create(ftpUrl);
                    requestFTPUploader.UsePassive = false;                    
                    requestFTPUploader.Credentials = new NetworkCredential(freeServerInfo.Id, freeServerInfo.Password);
                    requestFTPUploader.Method = WebRequestMethods.Ftp.UploadFile;

                    FileInfo fileInfo = new FileInfo(filepath);
                    FileStream fileStream = fileInfo.OpenRead();

                    double uploadRate = 0;
                    long totalLength = fileStream.Length;
                    int bufferLength = 2048;                    
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
                        }

                        uploadStream.Close();
                        fileStream.Close();
                        //row.QmsFileList.Add(new QmsFileInfo(fileName, Define.con_SUCCESS, fileName));
                        nFtpSuccessCnt++;

                        detail.FlagFtpSend = Define.con_SUCCESS;
                        //detail.QmsfileNmChg = fileName;


                    }
                    catch (Exception exception)
                    {
                        setLog(row, "[ERR]:[sendFtp]:" + exception.Message);
                        util.Log("[ERR]:[sendFtp]", exception.Message);
                        Console.WriteLine(exception.Message.ToString());                        
                        if (detail != null)
                        {
                            detail.FlagFtpSend = Define.con_FAIL;
                        }

                        bFtpSuccess = false;

                        //실패한 QMS 파일 이동 처리
                        //fileInfo.MoveTo(path + "\\" + Define.con_FOLDER_UPLOAD_ERROR + "\\" + fileName);
                    }                    
                }
            }

            row.FtpSendFileCnt = nFtpFileCnt;
            row.FtpSendSuccessCnt = nFtpSuccessCnt;
            row.FtpSendFlag = Define.CON_COMPLETE;

            return bFtpSuccess;
            */
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
            
            setControlServiceOnOff();

        }

        private void dgvProcess_Click(object sender, EventArgs e)
        {
            
        }
        private void dgvProcess_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                DataGridViewTextBoxCell cell = (DataGridViewTextBoxCell)dgvProcess.Rows[e.RowIndex].Cells[11];
                string fileDir = (string)cell.Value;
                exec("explorer.exe", fileDir);              
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
        }

        private void dgvErrorFile_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string zipfileNm = "";
            DataGridViewTextBoxCell cell = (DataGridViewTextBoxCell)dgvErrorFile.Rows[e.RowIndex].Cells[Define.con_KEY_PROCESSERROR_COLIDX];
            zipfileNm = (string)cell.Value;

            if (e.ColumnIndex == 0)
            {
                if (m_dicFileProcessErr.ContainsKey(zipfileNm))
                {
                    string fileDir = m_dicFileProcessErr[zipfileNm].ZipfileFullPath;
                    exec("explorer.exe", fileDir);
                }
            }else if (e.ColumnIndex == 5)
            {             
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
            m_formClose = true;
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
                        if (!dbContollerSetInfo())
                        {
                            MessageBox.Show("컨트롤러 실행에 필요한 데이타를 가져오는데 실패하였습니다.\n관리자에게 문의하십시요.");
                            this.Close();
                            return;
                        }

                        updateRowEnv();
                    }
                }                
            }
        }
        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}
