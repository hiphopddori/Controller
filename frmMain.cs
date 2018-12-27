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
using System.IO.Compression;
using System.Diagnostics;
using System.Linq;

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
 * 
 * //circle loading bar
 * https://github.com/falahati/CircularProgressBar
 * https://www.youtube.com/watch?v=o7MGaf8YW6s
 
 * */

namespace IqaController
{
    public partial class frmMain : Form
    {

        private Queue<FileProcess> m_QueueZipfileParsing = new Queue<FileProcess>();        //시작 ~ 알집해제까지
        private Queue<FileProcess> m_QueueZipfileParsing_2 = new Queue<FileProcess>();      //컨버터 실행 ~ FTP 전송 이후까지(컨버터 처리가 오래걸려서 작업큐 나눈다) 

        private Dictionary<string, FileProcess> m_dicFileProcess = new Dictionary<string, FileProcess>();           //
        private Dictionary<string, FileProcess> m_dicFileProcessErr = new Dictionary<string, FileProcess>();        //Error 파일 따로 관리(로딩시 디렉토리에서도 읽어와야함)

        private FileSystemWatcher m_zipFileWatcher = null;
        private string m_ftpId = "14";

        private Boolean m_formClose = false;
        private Boolean m_bServiceOnOffButton = false;  //서비스 On Off 버튼 토글        
        private Boolean m_ServiceOnIng = false;         //서비스 처리 프로세스 진행중 여부 (true:진행중 , false:완료)

        private System.Timers.Timer timer = new System.Timers.Timer();
        
        private Dictionary<string, ControllerServerEntity> m_dicFtpInfo = new Dictionary<string, ControllerServerEntity>();
        private List<ControllerServerEntity> m_lstServer = null;          //환경설정 수집서버 
        private List<ControllerFileKeepEntity> m_lstFile = null;          //환경설정 파일 보관주기
        private List<ControllerEnvEntity> m_lstEnv = new List<ControllerEnvEntity>();

        private int m_eventOrifileLoopCnt = 0;
        private Boolean m_debug = true;
        private string m_checkedkDay = "";          //일단위 체크날짜
        
        private const string m_version = "1.0.6";

        /* version Log
         * 1.0.1 : 2018-12-11 : 파일명 규칙 사업자는 제외한다
         * 1.0.2 : 2018-12-12 : 파일명 규칙 서비스타입 추가처리
         * 1.0.3 : 2018-12-13 : 모두 중복 drm일시 처리 보완처리(중복 drml삭제해도 나머지 파일이 있어서 파일사이즈로 기존 처리하는부분 보완처리함)
         * 1.0.4 : 2018-12-13 : 화면 및 zipfile key값 orgZipfileName -> zipfileName로 변경  FTP04같은경우 분단위로 같은 파일을 보내서 꼬일경우 발생 ㅠㅠ
         * 1.0.5 : 2018-12-17 : DB 컬럼 UPLOAD_DATE MAIN 과 DETAIL 동일하게 처리요청
         * 1.0.6 : 2018-12-21 : 컨버터 비정상에 인한 파일 복사시 문제 버그 수정
         * 1.0.7 : 2018-12-26 : 취소기능 추가(아직 미배포됨) - 서버 작업 완료되면 배포예정
        */

        delegate void TimerEventFiredDelegate();

        public frmMain()
        {
            InitializeComponent();
        }

        private void StartTmrServiceBtn()
        {
            //tmrServiceBtn.Change(0, 2000);           
            
            if (timer != null)
            {
                if (!timer.Enabled)
                {
                    timer.Enabled = true;
                    timer.Start();
                }
            }
        }

        private void EndTimerServiceBtn()
        {
            //tmrServiceBtn.Change(Timeout.Infinite, Timeout.Infinite);
            if (timer != null)
            {
                timer.Enabled = false;
                timer.Stop();
            }
        }

        private void KillConvertor(Boolean bSti)
        {

            string findProcessName = "";

            if (bSti)
            {
                findProcessName = "QMS Export.exe";
            }
            else
            {
                findProcessName = "QMS-W";
            }

            Process[] allProc = Process.GetProcesses();

            foreach (Process p in allProc)
            {
                if (p.ProcessName.IndexOf("QMS-W") >= 0)
                {
                    p.Kill();                    
                    break;
                }                
            }
        }

        private void TestProc()
        {

            /*
             
            runServyceType2RunConvertor("CSFB", @"D:\FTP16\WORK\본사_도로2조_CSFB_SKT_AUTO_청주시서원십_20180829", false);
            Thread.Sleep(2000);

            runServyceType2RunConvertor("LTED", @"D:\FTP16\WORK\본사_도로501조_LTED_SKT_AUTO_테스트_20180602", false);
            Thread.Sleep(2000);
            runServyceType2RunConvertor("LTED", @"D:\FTP16\WORK\인천본부_인빌딩46조_LTED_SKT_AUTO_소래포구 종합어시장_20180827", false);

            //FileProcess row = GetFilePath2FileProcessInfo(@"D:\FTP16\WORK\충북본부_도로49조_HSDPA__KT_청주대2일차_20181210.zip");
            //FileProcess row = GetFilePath2FileProcessInfo(@"D:\FTP16\WORK\경남본부_도로41조_HDV_LGU_부산지하철2호선aaa_20181210.zip");

            string extractDir = @"D:\FTP14\test2";
            string zipFilefullPath = @"D:\FTP14\7ziptest.zip";

            SevenZipCall("x -o\"" + extractDir + "\" -r -y \"" + zipFilefullPath + "\"");
            return;
            */

            //FileProcess row = GetFilePath2FileProcessInfo(@"D:\FTP16\WORK\강북본부_도로47조_LTEV_SKT_AUTO_동대문구청 보건소_20181214.zip");            
            //ZipFileNamingParsing(row);

            /*
            FileInfo file = new FileInfo(@"D:\FTP16\WORK\본사_도로2조_LTED_SKT_L5_고양시 일산동구_20181115_20181207134958.zip");

            if (file.Exists)
            {
                MessageBox.Show("존재");
            }
            else
            {
                MessageBox.Show("미존재");
            }
            */

            //FileProcess row = GetFilePath2FileProcessInfo(@"D:\FTP16\WORK\강북본부_도로407조_LTEV_SKT_AUTO_롯데_20181205.zip");
            //DeleteWorkZipFile(@"D:\FTP16\WORK\강북본부_도로407조_LTEV_SKT_AUTO_롯데_20181205");

            Boolean bAccessible = util.Wait2FileAccessible(@"D:\FTP16\WORK\본사_도로2조_CSFB_SKT_AUTO_청주시서원십_20180829.zip", 9999);


        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Text = String.Format("IQA 컨트롤러 ver : [{0}]", m_version); ;

            //최소 윈도우 사이즈
            this.MinimumSize = new Size(800, 600);

            iqaService.mainForm = this;
            //TestProc();
            //return;

#if DEBUG
            m_debug = true;
#else
            m_debug = false;
#endif

            if (!DBContollerSetInfo(false))
            {
                MessageBox.Show("컨트롤러 실행에 필요한 데이타를 가져오는데 실패하였습니다.\n관리자에게 문의하십시요.");
                this.Close();
                Application.Exit();
                return;
            }

            setControl();
            UpdateRowEnv();

            //findDirErrorInfo(Define.con_DIR_ZIPDUP);      zip파일 중복체크 안하기로 함
            findDirErrorInfo(Define.con_DIR_NAMING);
            findDirErrorInfo(Define.con_DIR_UPLOAD);
            findDirErrorInfo(Define.con_DIR_CONVERT);

            CreateFolderWatching();

            m_bServiceOnOffButton = false;
            serviceOnOff();

        }

        /* 이벤트 DRM 파일 요청건 FTP 전송 및 DB 처리         
        */
        private void eventOrifileSendProc()
        {

            ControllerServerEntity freeFtpServer = m_dicFtpInfo[Define.con_FTP_DRM];            
            if (freeFtpServer == null)
            {                
                util.LogDrm("ERR:eventOrifileSendProc", "이벤트 DRM FTP 서버 정보 가져오기 실패");
                return;
            }

            //util.Log("INFO", "Drm 요청파일 가져오기");
            string procServer = GetFtpName(m_ftpId);
            CommonResultEntity res = iqaService.getEventOrifileList(procServer);

            if (res.Flag == Define.con_FAIL)
            {
                util.LogDrm("ERR", "Drm 요청파일 가져오기 실패");
                return;
            }

            List<EventOriFileEntity> sendFileInfos = (List<EventOriFileEntity>)res.Result;

            if (sendFileInfos == null || sendFileInfos.Count <=0)
            {
                util.LogDrm("INFO", "Drm 요청건수 0");
                return;
            }

            util.LogDrm("INFO", "EVENT DRM 요청 발생 처리 시작 ##################################START");

            //sendFileInfos 발견여부도 추후 DB에 남기자

            EventOriFileProcResult eventOriFileResult = new EventOriFileProcResult();            
            eventOriFileResult.ProcServer = procServer;
            eventOriFileResult.FtpServer = freeFtpServer.Name;

            //BACKUP 폴도에서 해당 ZIP파일 찾아서 FTP로 전송한다.
            string findZipFileName = sendFileInfos[0].Zipfile_nm;
            eventOriFileResult.ZipfileNm = findZipFileName;

            updateRowEventDrmFileSend(eventOriFileResult);

            //추후 테스트후 적용하자
            //iqaService.updateEventOrifileSendResult(eventOriFileResult, sendFileInfos);

            string defaultPath = GetDefaultFtpPath();
            string backupDir = defaultPath + "\\" + Define.con_DIR_BACK;
            string[] filePaths = Directory.GetFiles(backupDir);
            string orgZipFilePath = "";
            int nfindPos = 0;

            Boolean bFindZipFile = false;
            eventOriFileResult.CurState = Define.con_EVENT_DRM_STATE_DRM;

            foreach (string filePath in filePaths)
            {
                nfindPos = filePath.LastIndexOf('_');
                if (nfindPos >= 0)
                {
                    //orgZipFilePath = filePath.Substring(0, nfindPos);           //DB에 Zip파일 원본이름 줄경우
                    orgZipFilePath = filePath;                                //DB에 해당날짜까지 시스템날짜까지 들어있는 ZIPfileName 정보를 줄경우 

                    if (Path.GetFileNameWithoutExtension(findZipFileName) == Path.GetFileNameWithoutExtension(orgZipFilePath))
                    {
                        util.LogDrm("INFO", "ZIP파일 발견 및 복사: "+ Path.GetFileNameWithoutExtension(findZipFileName));
                        //폴더이동후 필요한 drm파일만 남기고 삭제한후 다시 압축후 전송한다.
                        if (!util.FileSave("copy", filePath, defaultPath + '\\' + Define.con_DIR_EVENT_ORIFILE+'\\'+ findZipFileName))
                        {                                
                            return;
                        }

                        string zipPath = defaultPath + "\\" + Define.con_DIR_EVENT_ORIFILE +"\\"+ findZipFileName;
                        string extractPath = defaultPath + "\\" + Define.con_DIR_EVENT_ORIFILE + "\\" + Path.GetFileNameWithoutExtension(findZipFileName);
                        string eventZipFileName = getEventCompressName();

                        try
                        {

                            //ZipFile.ExtractToDirectory(zipPath, extractPath);                                                       //압축 해제
                            string extractDir = extractPath + @"\";                            
                            SevenZipCall("x -o\"" + extractDir + "\" -r -y \"" + zipPath + "\"");

                            string[] extractFilePaths = Directory.GetFiles(extractPath);
                            string sendFilePath = defaultPath + @"\" + Define.con_DIR_EVENT_ORIFILE + @"\" + eventZipFileName;
                            
                            //FTP 전송 요청한 파일만 담는 작업 폴더 만든다.
                            DirectoryInfo diChk = new DirectoryInfo(sendFilePath);
                            if (diChk.Exists == false)
                            {
                                diChk.Create();
                            }
                            Boolean bMatch = false;
                            int nOriFileCnt = 0;
                            foreach (string extractFilePath in extractFilePaths)
                            {
                                //string extractFileName = Path.GetFileName(extractFilePath);
                                string extractFileName = Path.GetFileNameWithoutExtension(extractFilePath);
                                string extenstion = Path.GetExtension(extractFilePath);

                                if (!(extenstion.ToUpper() == ".DRM" || extenstion.ToUpper() == ".DRMMP" || extenstion.ToUpper() == ".DML"))
                                {
                                    continue;
                                }
                                string remQmsFileNm = "";
                                foreach (EventOriFileEntity sendFileInfo in sendFileInfos)
                                {
                                    //Event 요청 파일명과 같을경우 작업 폴더에 복사한다.
                                    //확장자 drm , drmmp 같이 복사한다
                                    

                                    string sendFileName = Path.GetFileNameWithoutExtension(sendFileInfo.Qmsfile_nm);
                                    if (sendFileName == extractFileName)
                                    {
                                        util.LogDrm("INFO", "DRM 파일 발견 및 복사: " + sendFileName);
                                        bFindZipFile = true;
                                        bMatch = true;
                                        //DRM , DRMMP 같은 이름 유지하기위해 실제 DRMMP는 DB쪽 요청정보에는 없기떄문에 같은이름으로 파일을 관리해야한다
                                        if (sendFileInfo.Qmsfile_nmChg.Length <= 0)
                                        {
                                            //if (remQmsFileNm != sendFileInfo.Qmsfile_nmChg)
                                            //{
                                                nOriFileCnt++;
                                            //    remQmsFileNm = sendFileInfo.Qmsfile_nmChg;
                                            //}

                                            sendFileInfo.Qmsfile_nmChg = eventZipFileName + "_" + nOriFileCnt;
                                        }                                        
                                        //util.FileSave("copy", extractFilePath, sendFilePath + "\\" + extractFileName + extenstion);
                                        util.FileSave("copy", extractFilePath, sendFilePath + "\\" + sendFileInfo.Qmsfile_nmChg + extenstion);
                                        util.LogDrm("INFO", "DRM 파일 발견 및 복사 완료: " + sendFileName);
                                        sendFileInfo.FlagFind = "1";                                        
                                    }
                                }
                            }
                            if (!bMatch)
                            {
                                break;
                            }

                            //요청한 파일 압축한다. ㅠㅠ 실제 FTP서버에서 같은 닷넷프레임워크 버전인데 특정파일에 대해 ZipFIle 처리 안됨 
                            //ZipFile.CreateFromDirectory(sendFilePath, defaultPath + "\\" + Define.con_DIR_EVENT_ORIFILE + "\\" + eventZipFileName + ".zip", CompressionLevel.Fastest, false, Encoding.Default);
                            util.LogDrm("INFO", "DRM 요청파일 압축");
                            string compressZipFile = defaultPath + "\\" + Define.con_DIR_EVENT_ORIFILE + "\\" + eventZipFileName + ".zip";
                            SevenZipCall("a \"" + compressZipFile + "\" \"" + sendFilePath + "\"" + @"\*");        //압축시 해당 파일만 압축하기위해 \* 을 추가함
                            eventOriFileResult.FlagUnzip = Define.con_SUCCESS;

                        }
                        catch (Exception ex)
                        {
                            eventOriFileResult.FlagUnzip = Define.con_FAIL;
                            eventOriFileResult.UnzipErrLog = ex.Message.ToString();
                            util.LogDrm("ERR", ex.Message.ToString());
                        }

                        if (eventOriFileResult.FlagUnzip == Define.con_SUCCESS)
                        {

                            eventOriFileResult.CurState = Define.con_EVENT_DRM_STATE_FTP;
                            updateRowEventDrmFileSend(eventOriFileResult);
                            //FTP로 전송한다.                            
                            //bool bSuccess = sendFtpEventOrifile(freeFtpServer, defaultPath + "\\" + Define.con_DIR_EVENT_ORIFILE + "\\" + eventZipFileName + ".zip");
                            util.LogDrm("INFO", "DRM파일요청 FTP 전송");
                            bool bSuccess = sendSftpEventOrifile(defaultPath + "\\" + Define.con_DIR_EVENT_ORIFILE + "\\" + eventZipFileName + ".zip");                            
                            if (bSuccess)
                            {
                                util.LogDrm("INFO", "DRM파일요청 FTP 전송 성공");
                                eventOriFileResult.FlagSendFtp = Define.con_SUCCESS;
                                eventOriFileResult.ChgCompressFileNm = eventZipFileName + ".zip";
                            }
                            else
                            {
                                util.LogDrm("ERR", "DRM파일요청 FTP 전송 실패");
                                eventOriFileResult.FlagSendFtp = Define.con_FAIL;
                            }
                            updateRowEventDrmFileSend(eventOriFileResult);
                        }

                        //작업파일 모두 삭제 한다.
                        util.AllDeleteInDirectory(defaultPath + "\\" + Define.con_DIR_EVENT_ORIFILE);                       
                        break;
                    }
                }
            }

            eventOriFileResult.CurState = Define.con_EVENT_DRM_STATE_END;

            if (bFindZipFile)
            {                
                eventOriFileResult.IsDrmFind = "1";
            }
            else
            {
                eventOriFileResult.IsDrmFind = "0";
            }

            iqaService.updateEventOrifileSendResult(eventOriFileResult, sendFileInfos);

            updateRowEventDrmFileSend(eventOriFileResult);

            util.Log("INFO", "EVENT DRM 요청 발생 처리 완료 #####################################END");

        }

        private void compressTest()
        {
            // 참조 소스
            // https://docs.microsoft.com/ko-kr/dotnet/api/system.io.compression.zipfile?view=netframework-4.7.2 
            

            try
            {
                //압축 하기
                string startPath = @"D:\FTP14\TEST\본사_도로501조_LTED_SKT_AUTO_압축테스트_20180602";
                string zipPath = @"D:\FTP14\TEST\test.zip";
                ZipFile.CreateFromDirectory(startPath, zipPath ,CompressionLevel.Fastest,false,Encoding.Default);

                /*압축해제 예제
                string zipPath = @"D:\FTP14\TEST\본사_도로2조_CSFB_SKT_AUTO_청주시서원구_20180829.zip";
                string extractPath = @"D:\FTP14\TEST\본사_도로2조_CSFB_SKT_AUTO_청주시서원구_20180829";
                ZipFile.ExtractToDirectory(zipPath, extractPath);
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return;

            //ZipFile.ExtractToDirectory(zipPath, extractPath);

        }

        /* 환경설정 파일 보관주기에 따른 삭제 처리 
         *
         */
        private void FilePeriodChk2Proc()
        {
            string path = GetDefaultFtpPath();
           
            if (!(m_lstFile != null && m_lstFile.Count > 0))
            {
                return;
            }

            string ftpName = GetFtpName(m_ftpId);

            foreach (ControllerFileKeepEntity filePeriodInfo in m_lstFile)
            {
                string dirPath = filePeriodInfo.DirPath;
                if (dirPath.Length <= 0)
                {
                    continue;
                }
                //해당 FTP서버 속하면 삭제 처리한다.
                if (dirPath.IndexOf(ftpName) <= 0)
                {
                    continue;
                }

                if (!Directory.Exists(dirPath)){
                    continue;
                }

                //해당 디렉토리내 파일 삭제
                string[] files = Directory.GetFiles(dirPath);                
                int period = Convert.ToInt16(filePeriodInfo.Period);
                
                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    /*
                    string fileName = Path.GetFileName(fi.Name);
                    if (fileName == "인천본부_도로54조_HSDPA_SKT_AUTO_덕적도_20181109_20181107162054.zip")
                    {
                        Console.WriteLine("dd");
                    }
                    */
                    DateTime dtDate = fi.LastWriteTime;
                    dtDate = dtDate.AddDays(period);                                    //보관 기간

                    int nCompare = DateTime.Compare(dtDate, DateTime.Now);
                    if (nCompare < 0)
                    {
                        fi.Delete();
                    }
                }

                //해당 폴더내 서브 디렉토리 삭제 처리
                string[] dirs = Directory.GetDirectories(dirPath);

                foreach (string dir in dirs)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(dir);
                    DateTime dtDate = dirInfo.LastWriteTime;

                    dtDate = dtDate.AddDays(period);                                    //보관 기간

                    int nCompare = DateTime.Compare(dtDate, DateTime.Now);
                    if (nCompare < 0)
                    {
                        dirInfo.Delete(true);
                    }
                }

            }
        }

        /* 서비스 감시 버튼 깜빡깜빡 처리
        */
        void serviceBtnBlinkProc()
        {
            if (m_bServiceOnOffButton)
            {
                if (btnService.BackColor == Color.Silver)                
                    btnService.BackColor = Color.Red;                
                else                
                    btnService.BackColor = Color.Silver;                
            }
            else
            {
                if (!m_ServiceOnIng)                
                    btnService.BackColor = Color.Silver;                
            }
        }

        void tmr_ServiceBtnBlink(object sender, ElapsedEventArgs e)        
        {          
            if (this.IsHandleCreated)
            {
                BeginInvoke(new TimerEventFiredDelegate(serviceBtnBlinkProc));
            }
        }

        private void SetControlServiceBtnOnOff()
        {

            if (m_bServiceOnOffButton)
            {
                cboFtpServer.Enabled = false;
                btnService.Text = "서비스 On 상태";

                StartTmrServiceBtn();
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
                    EndTimerServiceBtn();
                    btnService.Text = "서비스 Off 상태";
                    btnService.Enabled = true;
                    btnService.BackColor = Color.Silver;                    
                }
            }
        }
        /*
        private void setControlServiceOnOff()
        {

            if (m_bServiceOnOffButton)
            {
                cboFtpServer.Enabled = false;
                btnService.Text = "서비스 On 상태";
                
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
                    EndTimerServiceBtn();
                    btnService.Text = "서비스 Off 상태";
                    btnService.Enabled = true;                    
                    btnService.BackColor = Color.Silver;
                }
            }
        }
        */
        private void SetLog(FileProcess row, string flag , string message)
        {

            AddLogFile(row, flag, message);

            this.Invoke(new Action(delegate ()
            {
                dgvProcessLog.RowCount = dgvProcessLog.RowCount + 1;
                int nRow = dgvProcessLog.RowCount - 1;
                dgvProcessLog.Rows[nRow].Cells[0].Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"); ;

                if (row != null)
                {
                    dgvProcessLog.Rows[nRow].Cells[1].Value = row.MeasuBranch;
                    dgvProcessLog.Rows[nRow].Cells[2].Value = row.MeasuGroup;
                    dgvProcessLog.Rows[nRow].Cells[3].Value = row.ZipFileName;
                    //dgvProcessLog.Rows[nRow].Cells[Define.con_KEY_PROCESSLOG_COLIDX].Value = row.OrgZipFileName;
                    dgvProcessLog.Rows[nRow].Cells[Define.con_KEY_PROCESSLOG_COLIDX].Value = row.ZipFileName;
                }
                else
                {
                    dgvProcessLog.Rows[nRow].Cells[3].Value = "Global";
                }

                dgvProcessLog.Rows[nRow].Cells[4].Value = message;
                
            }));
            
            //Console.WriteLine(message);
        }
        
        private void UpdateRowEnv()
        {

            m_lstEnv.Clear();
            if (m_lstServer == null || m_lstServer.Count <= 0)
            {
                util.Log("ERR:DBContollerSetInfo", "기본설정정보 없음");
                return;
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

#if DEBUG
                if (server.Name == "TEST_APP")
                {
                    m_dicFtpInfo[Define.con_FTP_WAV] = server;
                    m_dicFtpInfo[Define.con_FTP_DRM] = server;
                    m_dicFtpInfo[Define.con_FTP_QMS] = server;
                }
#else
                if (server.Name == "QMS3_WAV")
                {
                    m_dicFtpInfo[Define.con_FTP_WAV] = server;                    
                }
                else if (server.Name == "QM3_DRM")
                {
                    m_dicFtpInfo[Define.con_FTP_DRM] = server;                    
                }
                else if (server.Name == "APP02")
                {
                    m_dicFtpInfo[Define.con_FTP_QMS] = server;                    
                }
#endif
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

            int nRow = 0;
            this.Invoke(new Action(delegate ()
            {
                dgvEnv.Rows.Clear();
                //dgvEnv.RowCount = 0;
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
            string defaultPath = GetDefaultFtpPath();

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
                    row.ZipFileName = zipfileNm;                //zipfile 명 즉 폴더 이름
                    row.OrgZipFileName = files[0];              //zip파일명 시스템 날짜 붙이기전 원래 이름
                    row.ProcessServer = GetFtpName(m_ftpId);    //process서버 명
                    row.ZipfileFullPath = zipfileFullPath;

                    //파일명 규칙 오류
                    if (flagErr == Define.con_DIR_NAMING)
                    {
                        row.FileNameRule = Define.CON_MSG_ERROR;
                    }
                    else if (flagErr == Define.con_DIR_CONVERT)
                    {
                        row.ConversionFlag = Define.CON_MSG_ERROR;
                    }
                    else if (flagErr == Define.con_DIR_UPLOAD)
                    {
                        row.FtpSendFlag = Define.CON_MSG_COMPLETE;
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
                            detail.QmsfileNmChg = GetQmsNameChange(Convert.ToString(nIdx));                            
                            row.ZipfileDetailList.Add(detail); ;
                        }
                    }
                    else if (flagErr == Define.con_DIR_ZIPDUP)
                    {
                        row.ZipDupFlag = Define.CON_MSG_ERROR;
                    }
                    
                    UpdateRowErrorFile(row);
                }                
            }
        }

        //서버쪽 파일명 꺠지는 현상으로 qms파일 변경 처리 한다.
        private string GetQmsNameChange(string seq)
        {
            DateTime curDate = DateTime.Now;

            string qmsNmChg = GetFtpName(m_ftpId) + "_" + seq + "_" + curDate.ToString("yyyyMMddHHmmss.ffff") + ".qms";
            return qmsNmChg;
        }

        private string getEventCompressName()
        {
            DateTime curDate = DateTime.Now;

            string eventZipName = GetFtpName(m_ftpId) + "_" + "Event" + "_" + curDate.ToString("yyyyMMddHHmmss");
            return eventZipName;
        }

       
        private void UpdateRowErrorFile(FileProcess row)
        {
            //string sFileNm = row.OrgZipFileName;
            string sFileNm = row.ZipFileName;

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
                
                if (row.FileNameRule == Define.CON_MSG_ERROR)
                {
                    dgvErrorFile.Rows[nRow].Cells[2].Value = "파일명규칙 Error";
                    dgvErrorFile.setCellButton("수정/파일이동", 5, nRow);
                }
                else if (row.FtpSendFlag == Define.CON_MSG_COMPLETE && row.FtpSuccessFlag == Define.con_FAIL)
                {
                    dgvErrorFile.Rows[nRow].Cells[2].Value = "FTP 전송 Error";
                    dgvErrorFile.setCellButton("FTP 전송", 5, nRow);
                }
                else if (row.ZipDupFlag == Define.CON_MSG_ERROR)
                {
                    dgvErrorFile.Rows[nRow].Cells[2].Value = "ZIP 파일 중복";
                    dgvErrorFile.setCellButton("수정/파일이동", 5, nRow);
                }
                else if (row.ConversionFlag == Define.CON_MSG_ERROR)
                {
                    dgvErrorFile.Rows[nRow].Cells[2].Value = "컨버전 Error";
                    dgvErrorFile.Rows[nRow].Cells[5].Value = "품질분석실-측정기장비업체 확인요망";
                }

                //dgvErrorFile.Rows[nRow].Cells[Define.con_KEY_PROCESSERROR_COLIDX].Value = row.OrgZipFileName;
                dgvErrorFile.Rows[nRow].Cells[Define.con_KEY_PROCESSERROR_COLIDX].Value = row.ZipFileName;

            }));
        }
        /*
         *  drm파일 
        */
        private void updateRowEventDrmFileSend(EventOriFileProcResult eventDrmResult)
        {
            string sFileNm = eventDrmResult.ZipfileNm;
            int nRow = -1;
            this.Invoke(new Action(delegate ()
            {

                //log파일 및 DB상에 기록 다 남기때문에 화면상 크게 기능이 없어서 최대 100건만 화면상 보여준다.
                if (dgvDrmFileSend.RowCount > 100)
                {
                    dgvDrmFileSend.Rows.Clear();
                    //dgvDrmFileSend.RowCount = 0;
                }

                if (dgvDrmFileSend.RowCount <= 0)
                {
                    dgvDrmFileSend.RowCount = dgvDrmFileSend.RowCount + 1;
                    nRow = 0;
                }
                else
                {
                    nRow = dgvDrmFileSend.getFindZipFile2RowIndex(sFileNm, Define.con_KEY_EVENTDRM_COLIDX);
                    if (nRow <= 0)
                    {                        
                        nRow = dgvDrmFileSend.RowCount;
                        dgvDrmFileSend.RowCount = dgvDrmFileSend.RowCount + 1;
                    }                                        
                }

                dgvDrmFileSend.Rows[nRow].Cells[0].Value = sFileNm;

                if (eventDrmResult.CurState == Define.con_EVENT_DRM_STATE_END)
                {
                    if (eventDrmResult.IsDrmFind == "0")
                    {
                        dgvDrmFileSend.Rows[nRow].Cells[1].Value = "요청 Drm 파일 없음";
                    }
                }
                else
                {
                    dgvDrmFileSend.Rows[nRow].Cells[1].Value = Define.CON_MSG_WAIT;
                    dgvDrmFileSend.Rows[nRow].Cells[2].Value = Define.CON_MSG_WAIT;

                    if (eventDrmResult.CurState == Define.con_EVENT_DRM_STATE_DRM || eventDrmResult.CurState == Define.con_EVENT_DRM_STATE_FTP)
                    {
                        if (eventDrmResult.FlagUnzip == Define.con_FAIL)
                        {
                            dgvDrmFileSend.Rows[nRow].Cells[1].Value = "Error";
                        }
                        else if (eventDrmResult.FlagUnzip == Define.con_SUCCESS)
                        {
                            dgvDrmFileSend.Rows[nRow].Cells[1].Value = Define.CON_MSG_COMPLETE;
                        }
                        else
                        {
                            dgvDrmFileSend.Rows[nRow].Cells[1].Value = Define.CON_MSG_ING;
                        }
                    }

                    if (eventDrmResult.CurState == Define.con_EVENT_DRM_STATE_FTP)
                    {
                        if (eventDrmResult.FlagSendFtp == Define.con_FAIL)
                        {
                            dgvDrmFileSend.Rows[nRow].Cells[2].Value = "Error";
                        }
                        else if (eventDrmResult.FlagSendFtp == Define.con_SUCCESS)
                        {
                            dgvDrmFileSend.Rows[nRow].Cells[2].Value = Define.CON_MSG_COMPLETE;
                        }
                        else
                        {
                            dgvDrmFileSend.Rows[nRow].Cells[2].Value = Define.CON_MSG_ING;
                        }
                    }
                }
            }));
        }
        /* 파일처리현황 그리드에 값 반영한다.
         * 
        */
        private void UpdateRowFileProcess(FileProcess row)
        {
            //string sFileNm = row.OrgZipFileName;
            string sFileNm = row.ZipFileName;
            int nRow = -1;
            
            this.Invoke(new Action(delegate ()
            {

                if (m_dicFileProcess.ContainsKey(sFileNm))
                {

                    nRow = dgvProcess.getFindZipFile2RowIndex(sFileNm, Define.con_KEY_PROCESS_COLIDX);

                    if (nRow < 0)
                    {
                        if (dgvProcess.RowCount == 0)
                        {
                            nRow = 0;
                        }
                        else
                        {
                            nRow = dgvProcess.RowCount;
                        }
                        dgvProcess.RowCount = dgvProcess.RowCount + 1;                        
                    }
                    //m_dicFileProcess[sFileNm] = row;                    
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
                dgvProcess.Rows[nRow].Cells[Define.con_KEY_PROCESS_COLIDX].Value = row.ZipFileName;
                dgvProcess.Rows[nRow].Cells[11].Value = row.ZipfileFullPath;

                //선택된 row 활성화 스크롤 되게 
                
                dgvProcess.CurrentCell = dgvProcess.Rows[nRow].Cells[1];

                // row.DefaultCellStyle.BackColor = Color.Red;

                /* 신규일경우 무조건 앞열에 추가하는 방법 - 테스트 필요
                DataGridViewRow row1 = new DataGridViewRow();
                row1.Cells[1].Value = row.ProcessServer;
                dgvProcess.Rows.Insert(0, row1);
                */


            }));

            //Application.DoEvents();

        }
        /* 파일 이동할 이름을 얻는다.
         * 
        * */
        private string GetMoveFileName(string orgFileName)
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

        private string GetFtpName(string serverIdx)
        {
            return String.Format("FTP{0:00}", Convert.ToInt32(serverIdx)); 
        }

        
        private string getDir2FtpName()
        {
            string[] dirNames = Directory.GetDirectories(@"D:\");
            string dirFtpName = "";
            foreach(string dirName in dirNames)
            {
                if (dirName.IndexOf("FTP") >= 0)
                {
                    dirFtpName = Path.GetFileName(dirName);
                    break;
                }
            }

            return dirFtpName;
        }
        
        private void setControlComboFtpServer()
        {
            //FTP Combo Setting
            string[] ftpServers = new string[16];
            string tmpFtpName = "";


            StringBuilder retSelFtp = new StringBuilder();
            util.GetIniString("FTP", "SELECT", "", retSelFtp, 32, Application.StartupPath + "\\controllerSet.Ini");
            string iniSelFtp = retSelFtp.ToString();
            //저장된값이 없을경우 
            if (iniSelFtp.Length <= 0)
            {
                //D:\디렉토리에서 FTP정보 추출한다.
                iniSelFtp = getDir2FtpName();
                if (iniSelFtp.Length <= 0)
                {
                    iniSelFtp = GetFtpName(m_ftpId);
                }
            }
            int nFtpSelIdx = -1;
            for (int idx = 0; idx < 16; idx++)
            {
                tmpFtpName = String.Format("FTP{0:00}", idx + 1);
                ftpServers[idx] = tmpFtpName;

                if (iniSelFtp == tmpFtpName)
                {
                    nFtpSelIdx = idx;
                }
            }

            cboFtpServer.Items.AddRange(ftpServers);
            
            if (nFtpSelIdx < 0)
            {
                cboFtpServer.SelectedIndex = 13;
            }
            else
            {
                cboFtpServer.SelectedIndex = nFtpSelIdx;
            }
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
            StartTmrServiceBtn();

            setControlComboFtpServer();

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
            dgvProcessLog.addColumn("측정본부", 78);
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

            dgvDrmFileSend.addColumn("zip 파일명", 400);
            dgvDrmFileSend.addColumn("drm 파일 추출", 150);            
            dgvDrmFileSend.addColumn("FTP 전송", 100);

        }

        private void btnWindowFinder_Click(object sender, EventArgs e)
        {
            Exec("explorer.exe", "");          
        }
      
        private Boolean isFileValidate(string fileName)
        {
            return true;
        }
        /* Zip 파일 명 체크
         modify log : 2018-12-11 파일명 규칙은 구분자 , 도로/인빌딩 구분 , 날짜형식 및 날짜 유효기간만 체크한다.
        */
        private Boolean ZipFileNamingParsing(FileProcess row )
        {
            //진행중 보여줘야하면 아래 풀자
            //row.CurState = Define.con_STATE_START_NAMING;
            //iqaService.updateZipfileInfo(row, Define.con_ING_START);
            
            //복사할 경로에 해당 파일 유무 체크
            string path = GetDefaultFtpPath();
            //string onlyFileName = Path.GetFileName(fileName);            
            string withoutExtensionName = Path.GetFileNameWithoutExtension(row.ZipFileName);            
            string sMoveDir = Path.GetFileNameWithoutExtension(row.ZipFileName);

            string sWorkFileFullName = row.ZipfileFullPath + "\\" + row.ZipFileName;
            string sMoveFileFullName = path + "\\" + Define.con_DIR_NAMING + "\\" + sMoveDir + "\\" + row.OrgZipFileName;

            string[] arFileName = withoutExtensionName.Split('_');
            string sFileDate = "";
            DateTime dtDate = new DateTime();
            DateTime dtMaxDate;

            string comp = "";
            //오류나더라도 기본정보는 WEB 화면에 보여주게 하기 위해서
            if (arFileName.Length >= 4)
            {
                row.MeasuBranch = arFileName[0];
                row.MeasuGroup = arFileName[1];
                row.ServiceType = arFileName[2];
                comp = arFileName[3];
            }

            //","가 파일명에 있을시 7Zip이 압축해제시 관련 정보를 남기지 못함
            //if (arFileName.Length != 8 || row.ZipFileName.IndexOf(",") >= 0)     //뒤에 시스템 날짜 더 붙임
            /*
            if (arFileName.Length != 8 || row.ZipFileName.IndexOf(",") >= 0)     //뒤에 시스템 날짜 더 붙임
            {   
                if (row.ZipFileName.IndexOf(",") >= 0)
                {
                    SetLog(row,"WARN","Zip 파일명 콤마 오류(콤마 입력 불가)");              
                }
                else
                {
                    SetLog(row, "WARN", "구분자 오류");              
                }              
                return false;
            }
            */


            if (arFileName.Length != 8)     //뒤에 시스템 날짜 더 붙임
            {                
                SetLog(row, "WARN", "구분자 오류");                
                return false;
            }

            //<START> ############# 파일 유효 기간 체크
            sFileDate = arFileName[6];

            //bool bSuccess = DateTime.TryParse(sFileDate, out dtDate);

            Boolean isDate = util.ParseDate(sFileDate, ref dtDate);

            if (!isDate)
            {
                SetLog(row, "WARN", "날짜형식 오류");
                return false;
            }

            //dtDate = DateTime.ParseExact(sFileDate, "yyyyMMdd", null);
            dtMaxDate = dtDate.AddDays(30);                                //최대 30일 유효범위임             
            int nCompare = DateTime.Compare(dtMaxDate, DateTime.Now);
            if (nCompare < 0)
            {                               
                SetLog(row, "WARN", "유효기간 오류");                
                return false;
            }

            string serviceType = row.ServiceType;
            serviceType = serviceType.ToUpper();

            if (!(serviceType == "LTED" || serviceType == "LTEV" || serviceType == "HDV" || serviceType == "HDVML" || serviceType == "CSFB" || serviceType == "CSFBML" || serviceType == "HSDPA"))
            {
                SetLog(row, "WARN", "서비스 유형 오류");
                return false;
            }

            /*
            row.MeasuBranch = arFileName[0];            
            row.MeasuGroup = arFileName[1];
            row.ServiceType = arFileName[2];
            row.Comp = arFileName[3];
            */
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

            if (comp == "SKT")
            {
                row.Comp = "011";
            }
            else if (comp == "KT")
            {
                row.Comp = "016";
            }
            else if (comp == "LGU")
            {
                row.Comp = "019";
            }
            else
            {
                row.Comp = "";
            }

            /* 2018-12-11 체크 하지 않는다.
            if (!(row.Comp == "011" || row.Comp == "016" || row.Comp == "019"))
            {
                row.Comp = "";
                SetLog(row, "WARN", "사업자 오류");
                return false;
            }
            */

            if (row.InoutType == "")
            {
                SetLog(row, "WARN", "Outdoor/Indoor 오류");
                return false;
            }

            row.MeasuDate = sFileDate;
            row.FileNameRule = Define.CON_MSG_COMPLETE;
            row.CurState = Define.con_STATE_COMPLETE_NAMING;

            //iqaService.updateZipfileInfo(row, Define.con_ING_START);
            return true;
        } 

        private void AddLogFile(FileProcess row , string flag , string msg)
        {
            string zipfileName = "";
            if (row != null)
            {
                zipfileName = row.ZipFileName;
            }
            else
            {
                zipfileName = "Global";
            }

            util.Log(flag, zipfileName  + " ==> " +  msg);            
        }


        private Boolean setFilePeriodInfo()
        {
            CommonResultEntity resFile = iqaService.getControllerFilePeriod(false);

            if (resFile == null)
            {
                util.Log("ERR:DBContollerSetInfo", "Web Server 연결 실패");
                return false;
            }

            if (resFile.Flag == Define.con_FAIL)
            {
                util.Log("ERR:DBContollerSetInfo", "파일보관주기 정보 없음");
                m_lstFile = null;
                return false;
            }
            else
            {
                m_lstFile = (List<ControllerFileKeepEntity>)resFile.Result;
            }


            return true;
        }

        private Boolean DBContollerSetInfo(bool bWait)
        {

            CommonResultEntity resFile = iqaService.getControllerFilePeriod(bWait);

            if (resFile == null)
            {
                util.Log("ERR:DBContollerSetInfo", "Web Server 연결 실패");
                return false;
            }


            if (resFile.Flag == Define.con_FAIL)
            {
                util.Log("ERR:DBContollerSetInfo", "파일보관주기 정보 없음");
                return false;                
            }
            else
            {
                m_lstFile = (List<ControllerFileKeepEntity>)resFile.Result;
            }

            CommonResultEntity resServer =  iqaService.getControllerServer();

            if (resServer.Flag == Define.con_FAIL)
            {
                util.Log("ERR:DBContollerSetInfo", "서버정보 없음");
                return false;
            }
            else
            {
                m_lstServer = (List<ControllerServerEntity>)resServer.Result;
            }

            m_lstEnv.Clear();

            if (m_lstServer == null || m_lstServer.Count <= 0)
            {
                util.Log("ERR:DBContollerSetInfo", "기본설정정보 없음");
                return false;
            }
            /*
            foreach (ControllerServerEntity server in m_lstServer)
            {
                ControllerEnvEntity env = new ControllerEnvEntity();
                env.Flag = "server";
                env.Item = server.Name;
                env.Info1 = "IP : " + server.Ip;
                env.Info2 = "ID : " + server.Id;
                env.Info3 = "PW : " + server.Password;
                m_lstEnv.Add(env);

#if DEBUG
                if (server.Name == "TEST_APP")
                {
                    m_dicFtpInfo[Define.con_FTP_WAV] = server;
                    m_dicFtpInfo[Define.con_FTP_DRM] = server;
                    m_dicFtpInfo[Define.con_FTP_QMS] = server;                    
                }                
#else
                if (server.Name == "QMS3_WAV")
                {
                    m_dicFtpInfo[Define.con_FTP_WAV] = server;                    
                }
                else if (server.Name == "QM3_DRM")
                {
                    m_dicFtpInfo[Define.con_FTP_DRM] = server;                    
                }
                else if (server.Name == "APP02")
                {
                    m_dicFtpInfo[Define.con_FTP_QMS] = server;                    
                }
#endif
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
            */
            return true;
        }

        /* DB 중복 ZIP FILE 명 체크 - 
        */
        /*   
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
                util.Log("ERR:dbZipFileDupChk", ex.Message);
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
                }
            }

            CommonResultEntity res = iqaService.setUnzipFileInfoUpdateAndGetDupInfo(row);
            if (res.Flag == Define.con_FAIL)
            {
                return;
            }

            List<CodeNameEntity> dupList = (List<CodeNameEntity>)res.Result;
            if (dupList == null)
            {
                return;
            }

            if (dupList.Count > 0)
            {
                //컨버전에서 제외하기 위해 해당 파일 제거한다.
                foreach (CodeNameEntity tmp in dupList)
                {
                    File.Delete(dupChkDir + "\\" + tmp.Code);
                }
            }

            /*
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

                List<CodeNameEntity> dupList = (List<CodeNameEntity>)Newtonsoft.Json.JsonConvert.DeserializeObject(pagesource, typeof(List<CodeNameEntity>));
                
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
                SetLog(row, "ERR", "[ERR]:[unzipFileDetailProc]:" + ex.Message);
                //util.Log("ERR:unzipFileDetailProc", ex.Message);
                Console.WriteLine(ex.Message.ToString());
            }
            */
        }


        private void Err2FileMoveConversion(FileProcess row)
        {
            string path = GetDefaultFtpPath();
            //string sMoveDir = Path.GetFileNameWithoutExtension(row.ZipFileName);


            //string sWorkFileFullName = row.GetWorkFileFullPathName();

            string sWorkFileFullName = row.GetZipfileFullPathName(path, Define.con_DIR_WORK);
            string sMoveDir = row.GetErr2MovFileDirPath(path, Define.con_DIR_CONVERT);
            string sMoveFileFullPathName = row.GetErr2MovFileFullPathName(path, Define.con_DIR_CONVERT);
            
            DirectoryInfo diChk = new DirectoryInfo(sMoveDir);
            if (diChk.Exists == false)
            {
                diChk.Create();
            }

            row.ConversionFlag = Define.CON_MSG_ERROR;
            util.FileSave("move", sWorkFileFullName, sMoveFileFullPathName);
            row.ZipfileFullPath = sMoveDir;

        }

        /* FTP 전송실패 또는 FTP 전송 못한 QMS파일 백업
         */
        private void Err2FileMoveFtpError(FileProcess row)
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

            //string movePath = GetDefaultFtpPath();
            //movePath += movePath + "\\" + Define.con_DIR_UPLOAD + "\\" + row.ZipFileName;

            string movePath  = row.GetErr2MovFileDirPath(GetDefaultFtpPath(),Define.con_DIR_UPLOAD);

            //UPLOAD ERROR 폴더 생성
            DirectoryInfo diChk = new DirectoryInfo(movePath);
            if (diChk.Exists == false)
            {
                diChk.Create();
            }

            row.ZipfileFullPath = movePath;

            foreach (ZipFileDetailEntity detail in row.ZipfileDetailList)
            {
                if (detail.FlagFtpSend != Define.con_SUCCESS)
                {
                    util.FileSave("move", row.ZipfileFullPath + "\\" + detail.QmsfileNm, movePath + "\\" + detail.QmsfileNm);
                    
                }
            }
        }


        private String ConvertorRunProc(FileProcess row , string extractDir)
        {

            //<START> ############# 컨버터 실행
            SetLog(row, "INFO", "컨버터 실행");

            row.ConversionFlag = Define.CON_MSG_ING;
            row.CurState = Define.con_STATE_START_CONV;
            UpdateRowFileProcess(row);
            iqaService.updateZipfileMainInfo(row, Define.con_STATE_START_CONV);


            string[] dmlFind = Directory.GetFiles(extractDir, "*.dml");
            string sConvertorLogFileName = "";

            //컨버터 실행 비정상일경우 최대 3번 호출한다.
            int nConvertCallCnt = 0;
            Boolean bSti = false;
            while (nConvertCallCnt <= 3 && sConvertorLogFileName.Length <= 0)
            {
                nConvertCallCnt++;

                SetLog(row, "INFO", "컨버터 실행 횟수 : " + nConvertCallCnt.ToString());

                string runConvertorTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                if (dmlFind.Length > 0)
                {
                    bSti = true;
                    runServyceType2RunConvertor(row.ServiceType, extractDir, bSti);
                }
                else
                {
                    runServyceType2RunConvertor(row.ServiceType, extractDir, bSti);
                }

                //생각보다 컨트롤러 처리 오래 걸림
                Thread.Sleep(40 * 1000);
                sConvertorLogFileName = GetConvertLogFileName(runConvertorTime, bSti, 8);
                if (sConvertorLogFileName.Length <= 0)
                {
                    SetLog(row, "INFO", "컨버터 강제 종료");
                    //컨버터 프로세스 죽인다.
                    KillConvertor(bSti);
                    Thread.Sleep(3 * 1000);
                }                
            }

            if (sConvertorLogFileName.Length <= 0)
            {
                row.ConversionFlag = Define.CON_MSG_ERROR;
                row.CurState = Define.con_STATE_ERR_CONV;
                UpdateRowFileProcess(row);
                SetLog(row, "ERR", "Convertor 비정상");

                iqaService.updateZipFileAllInfo(row);
                
                row.Pass = true;
                return "";
            }
            SetLog(row, "INFO", "컨버터 종료 대기중...");
            string sConvertorLogEndFileName = "";
            while (sConvertorLogEndFileName == "")
            {
                sConvertorLogEndFileName = isConvertorRunComplete(sConvertorLogFileName, bSti);
                //Application.DoEvents();
                //SetLog(row, "INFO", "컨버터 종료 대기중...");
                Thread.Sleep(10 * 1000);
            }

            row.ConversionFlag = Define.CON_MSG_COMPLETE;
            //row.FtpSendFlag = Define.CON_MSG_ING;
            SetLog(row, "INFO", "컨버터 실행 완료");
            UpdateRowFileProcess(row);

            return sConvertorLogEndFileName;

        }


        private string[] GetDir2OriFileList(string extractDir)
        {

            List<string> oriFiles = new List<string>();
            string[] extractFile = Directory.GetFiles(extractDir);

            foreach (string extractFilePath in extractFile)
            {
                //string extractFileName = Path.GetFileName(extractFilePath);
                string extractFileName = Path.GetFileNameWithoutExtension(extractFilePath);
                string extenstion = Path.GetExtension(extractFilePath);

                if (extenstion.ToUpper() == ".DRM" || extenstion.ToUpper() == ".DML")
                {
                    oriFiles.Add(extractFilePath);
                }
            }

            return oriFiles.ToArray();
        }

        private Boolean ZipfileCancelCheckProc(FileProcess row , string delFlag)
        {

            string isCancel = iqaService.IsZipfileCancel(row);
            if (isCancel != "1")
            {
                return false;
            }
            string delPath = "";
            if (delFlag == "all")
            {
                delPath = row.ZipfileFullPath;
            }
            else if (delFlag == "zip")
            {
                delPath = row.ZipFileFullFileName;
            }

            row.Pass = true;
            row.CurState = Define.con_STATE_CANCEL;
            SetLog(row, "INFO", "작업 취소");

            DeleteWorkZipFile(delPath);

            return true;
        }

        /* Zip 파일 분석 처리 프로세스
        */        
        private void ZipfileParsing(FileProcess row)
        {           
            string tmpZipfilePath = "";
            string path = GetDefaultFtpPath();                                   
            string withoutExtensionZipFileName = Path.GetFileNameWithoutExtension(row.ZipFileName);

            row.CurState = Define.con_STATE_WORK;
            UpdateRowFileProcess(row);

            try
            {
                //Log 그리드 초기화
                /*
                this.Invoke(new Action(delegate ()
                {
                    dgvProcessLog.RowCount = 0;
                }));
                */

                if (m_QueueZipfileParsing_2.Count <= 0 && m_QueueZipfileParsing.Count <=0)
                {
                    //로그 그리드 초기화
                    this.Invoke(new Action(delegate ()
                    {
                        dgvProcessLog.Rows.Clear();
                    }));
                }

                SetLog(row, "INFO", "#####작업 처리 시작#####");

                string fileName = row.OrgZipFIleFullName;
                FileInfo file = new FileInfo(fileName);

                if (!file.Exists)
                {
                    row.Pass = true;
                    row.BackupFlag = "Zip 파일 미존재";                    
                    row.CurState = Define.con_STATE_ERR_NO_FILE;
                    SetLog(row, "ERR", "해당파일 존재 하지 않음-중간 취소됨");
                    UpdateRowFileProcess(row);
                    iqaService.updateZipfileMainInfo(row, "");
                    return;
                }
                
                SetLog(row, "INFO", "작업폴더 파일이동");
                
                tmpZipfilePath = path + "\\" + Define.con_DIR_WORK;
                //row.ZipfileFullPath = path + "\\" + Define.con_DIR_WORK;
                
                if (!util.FileSave("move", fileName, tmpZipfilePath + "\\" + row.ZipFileName))
                {
                    //파일이동 자체가 실패하였기 떄문에 오류로 오류폴더로 파일을 이동하는게 의미 없음
                    row.Pass = true;
                    SetLog(row, "WARN", "작업폴더 파일이동 실패");                    
                    return;
                }
                row.ZipfileFullPath = tmpZipfilePath;

                FileInfo fInfo = new FileInfo(row.ZipfileFullPath + "\\" + row.ZipFileName);
                row.FileSize = fInfo.Length.ToString();
                SetLog(row, "INFO", "작업폴더 파일이동 완료");

                //<START> ############# 파일 백업 및 작업 디렉토리로 이동                
                row.BackupFlag = Define.CON_MSG_ING;
                UpdateRowFileProcess(row);
                SetLog(row, "INFO", "백업");
                if (!util.FileSave("copy", row.ZipfileFullPath + "\\" + row.ZipFileName, path + "\\"+ Define.con_DIR_BACK + "\\" + row.ZipFileName))
                {
                    row.Pass = true;
                    SetLog(row, "WARN", "백업 실패");
                    row.BackupFlag = Define.CON_MSG_ERROR;
                    UpdateRowFileProcess(row);
                    return;
                }
                SetLog(row, "WARN", "백업 완료");
                row.BackupFlag = Define.CON_MSG_COMPLETE;
                //<END>

                /*
                if (ZipfileCancelCheckProc(row, "zip"))
                {
                    row.FileNameRule = Define.CON_MSG_CANCEL;
                    UpdateRowFileProcess(row);
                    return;
                }
                */

                //<START> ########### 파일명 규칙 분석#######################
                iqaService.updateZipfileMainInfo(row, Define.con_STATE_START_NAMING);                
                SetLog(row, "INFO", "파일명 규칙 분석");
                if (!ZipFileNamingParsing(row))
                {
                    row.Pass = true;
                    row.FileNameRule = Define.CON_MSG_ERROR;
                    row.CurState = Define.con_STATE_ERR_NAMING;
                    
                    UpdateRowFileProcess(row);

                    //string sWorkFileFullName = row.GetWorkFileFullPathName();                    
                    string sWorkFileFullName = row.GetZipfileFullPathName(path,Define.con_DIR_WORK);

                    string sMoveFileFullName = row.GetErr2MovFileFullPathName(path, Define.con_DIR_NAMING);
                    string sMoveDir = row.GetErr2MovFileDirPath(path, Define.con_DIR_NAMING);

                    DirectoryInfo diChk = new DirectoryInfo(sMoveDir);
                    if (diChk.Exists == false)
                    {
                        diChk.Create();
                    }

                    util.FileSave("move", sWorkFileFullName, sMoveFileFullName);
                    row.ZipfileFullPath = sMoveDir;

                    UpdateRowErrorFile(row);

                    iqaService.updateZipfileMainInfo(row,Define.con_STATE_START_NAMING);
                    return;
                }
                
                UpdateRowFileProcess(row);
                SetLog(row, "INFO", "파일명 규칙 완료");
                //<END>

                /*
                if (ZipfileCancelCheckProc(row, "zip"))
                {
                    row.ExtractFlag = Define.CON_MSG_CANCEL;
                    UpdateRowFileProcess(row);
                    return;
                }
                */

                //<START> ############# 압축 해제 처리 ######################
                SetLog(row, "INFO", "압축해제");
                if (!UnZIpProc(row)){
                    row.Pass = true;
                    UpdateRowErrorFile(row);                    
                    iqaService.updateZipfileMainInfo(row, "");
                    return;
                }
                SetLog(row, "INFO", "압축해제 완료");
                UpdateRowFileProcess(row);
                iqaService.updateZipfileMainInfo(row, "");
                //<END> 압축 해제 완료

                Thread.Sleep(1500);

                string extractDir = getUnzipPath(row);

                //string[] filters = new[] { "*.drm", "*.dml" };
                //string[] unzipFile = filters.SelectMany(f => Directory.GetFiles(extractDir, f)).ToArray();
                string[] unzipFile = GetDir2OriFileList(extractDir);

                if (unzipFile.Length <= 0)
                {
                    row.Pass = true;
                    row.CurState = Define.con_STATE_ERR_ZERO_DRM;
                    SetLog(row, "ERR", "Drm/Dml 파일 미존재");
                    //UpdateRowErrorFile(row);
                    iqaService.updateZipfileMainInfo(row, "");
                    DeleteWorkZipFile(extractDir);
                    return;
                }

                //<START> ############# Unzip 파일 정보 DB 업로드및 중복 Drm 제거 처리                
                //이곳에서 unzip 파일 및 dup파일 정보 업데이트 한다.                
                SetLog(row, "INFO", "중복 Drm 체크");
                unzipFileDetailProc(row , extractDir);
                SetLog(row, "INFO", "중복 Drm 체크 완료");
                //#############################################</END>

                //모두 중복 제거일경우 컨버터 호출하지 않는다.
                //string[] extractFile = Directory.GetFiles(extractDir);

                string[] extractFile = GetDir2OriFileList(extractDir);

                //2018-12-07 drm파일이 존제 안할수도있따고 함 위에서 체크 조건 더주기로 함
                if (extractFile.Length <= 0)        //압축 로그정보파일 떄문에 1개파일은 존재한다.
                {
                    row.Pass = true;
                    row.CurState = Define.con_STATE_ERR_DUP_ALL;
                    SetLog(row, "INFO", "모든 파일 중복 Drm");
                    iqaService.updateZipfileMainInfo(row,"");                    
                    DeleteWorkZipFile(extractDir);

                    return;
                }

                Thread.Sleep(2000);

                AddLogFile(row, "INFO", "Que Dequeue2 Enqueue(등록)");
                m_QueueZipfileParsing_2.Enqueue(row);             
            }
            catch (Exception ex)
            {
                SetLog(row, "ERR", "[ERR]:[ZipfileParsing]:" + ex.Message);
                //복사 진행중인 파일
                //파일이 다른 프로세스에서 사용되고 있으므로 프로세스에서 파일에 액세스할 수 없습니다.
                if (ex.HResult == -2147024864)
                {
                    //파일 해당 폴더로 전송중인 상태라 전송 완료시점에 다시 처리 하기 위해
                    row.Pass = false;
                }
                else
                {
                    row.Pass = true;
                }
                Console.WriteLine(ex.Message);
            }
        }

        /*  convertor 부터 나머지 처리 */
        private void ZipfileParsing2(FileProcess row)
        {
            
            /*
            string onlyFileName = Path.GetFileName(fileName);
            string withoutExtensionName = Path.GetFileNameWithoutExtension(fileName);


            //처리 완료또는 패스할 조건일경우
            if (m_dicFileProcess.ContainsKey(onlyFileName))
            {
                if (m_dicFileProcess[onlyFileName].Pass || m_dicFileProcess[onlyFileName].Complete) return;
            }
            */
            
            try
            {
                /*
                //Log 그리드 초기화
                this.Invoke(new Action(delegate ()
                {
                    dgvProcessLog.RowCount = 0;
                }));
                */
                //<START> - 컨버터 실행

                string extractDir = getUnzipPath(row);
                /*
                if (ZipfileCancelCheckProc(row, "all"))
                {
                    row.ConversionFlag = Define.CON_MSG_CANCEL;
                    UpdateRowFileProcess(row);
                    return;
                }
                */

                string sConvertorEndFileName = ConvertorRunProc(row, extractDir);
                if (sConvertorEndFileName.Length <= 0)
                {
                    row.Pass = true;
                    row.CurState = Define.con_STATE_ERR_CONV;
                    Err2FileMoveConversion(row);
                    UpdateRowErrorFile(row);
                    DeleteWorkZipFile(extractDir);
                    return;
                }
                //#############################################</END>

                if (sConvertorEndFileName.Length > 0)
                {
                    if (!ConvertResultProc(row, sConvertorEndFileName))
                    {
                        row.Pass = true;
                        return;
                    }

                    /*
                    if (ZipfileCancelCheckProc(row, "all"))
                    {
                       row.FtpSendFlag = Define.CON_MSG_CANCEL;
                       UpdateRowFileProcess(row);
                       return;
                    }
                    */

                    SetLog(row, "INFO", "FTP 파일전송");
                    //ControllerServerEntity freeFtpServer = iqaService.getFreeFtpServerInfo();
                    ControllerServerEntity freeFtpServer = m_dicFtpInfo[Define.con_FTP_QMS];
                    if (freeFtpServer == null)
                    {
                        row.Pass = true;
                        row.FtpSuccessFlag = Define.con_FAIL;
                        SetLog(row, "WARN", "FTP 서버 정보 가져오기 실패");
                        Err2FileMoveFtpError(row);
                        UpdateRowErrorFile(row);
                        DeleteWorkZipFile(extractDir);
                        return;
                    }
                    Boolean bFtpSuccess = false;
                    //FTP 전송 (모듈이 나뉘고 있어서 상용도 SFTP로 요청하자)

                    string[] qmsFiles = Directory.GetFiles(extractDir, "*.qms");

                    if (qmsFiles.Length > 0)
                    {
                        row.FtpSendFlag = Define.CON_MSG_ING;
                        UpdateRowFileProcess(row);

                        iqaService.updateZipfileMainInfo(row, Define.con_STATE_START_FTP);                        
                        bFtpSuccess = SendFtp(row, freeFtpServer, extractDir, false);
                        
                        if (!bFtpSuccess)
                        {
                            row.Pass = true;
                            row.FtpSendFlag = Define.CON_MSG_ERROR;
                            row.CurState = Define.con_STATE_ERR_FTP;
                            Err2FileMoveFtpError(row);
                            SetLog(row, "INFO", "파일전송 실패");
                        }
                        else
                        {
                            row.CurState = Define.con_STATE_COMPLETE_FTP;
                            row.FtpSendFlag = Define.CON_MSG_COMPLETE;
                            SetLog(row, "INFO", "파일전송 완료");
                        }
                    }
                    else
                    {
                        row.CurState = Define.con_STATE_ERR_ZERO_QMS;
                        SetLog(row, "INFO", "QMS 파일 미존재");
                    }
                }
                //정상 처리 로그 남긴다.                        1                                
                SetLog(row, "INFO", "#####작업 처리 완료#####");

                iqaService.updateZipFileAllInfo(row);

                UpdateRowFileProcess(row);


                //worK 작업 파일 관련 삭제 처리
                if (DeleteWorkZipFile(extractDir))
                {
                    SetLog(row, "INFO", "작업파일 삭제 완료");
                }
                else
                {
                    SetLog(row, "INFO", "작업파일 삭제 실패");
                }
                row.Complete = true;
            }
            catch (Exception ex)
            {
                SetLog(row, "ERR", "[ERR]:[ZipfileParsing]:" + ex.Message);
                //복사 진행중인 파일
                //파일이 다른 프로세스에서 사용되고 있으므로 프로세스에서 파일에 액세스할 수 없습니다.               
                row.Pass = true;               
                Console.WriteLine(ex.Message);
            }
        }

        private Boolean DeleteWorkZipFile(string dirPath)
        {
            
            Thread.Sleep(1000);

            try
            {
                if (dirPath.Length <= 0)
                {
                    return true;
                }

                Boolean bUnzip = true;
                string ext = Path.GetExtension(dirPath);
                if (ext == ".zip" || ext == ".ZIP")
                {
                    bUnzip = false;
                }

                if (bUnzip)
                {
                    DirectoryInfo di = new DirectoryInfo(dirPath);
                    if (di.Exists)
                    {
                        di.Delete(true);
                    }

                    FileInfo file = new FileInfo(dirPath + ".zip");

                    if (file.Exists)
                    {
                        file.Delete();
                    }
                }
                else
                {
                    FileInfo file = new FileInfo(dirPath);
                    if (file.Exists)
                    {
                        file.Delete();
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                //SetLog(row, "[ERROR(deleteDir)]" + ex.Message);
                util.Log("ERR:DeleteWorkZipFile", ex.Message.ToString());                
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

            //FileStream ReadData = new FileStream(unZipLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

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
                        dirTotalSize = GetDirectoryTotalSize(chkDirPath);
                        //Application.DoEvents();
                        Thread.Sleep(2000);
                    }                    
                }
            }
        }

        private long GetDirectoryTotalSize(string sDirPath)
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
            long totalSize = util.CalculateDirectorySize(dir, true);

            return totalSize;
        }

        private Boolean ConvertResultProc(FileProcess row, string filePath)
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
                    if (tmpResult[2].ToUpper() == "END")
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
                        detail.QmsfileNmChg = GetQmsNameChange(Convert.ToString(nQmsFileCnt));
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
            //오류가 발생시 오류항목은 따로 컨버터 오류 폴더에 백업한다.
            if (bConvertError)
            {
                string path = GetDefaultFtpPath();
                string sMoveDir = Path.GetFileNameWithoutExtension(row.OrgZipFileName);

                DirectoryInfo diChk = new DirectoryInfo(path + "\\" + Define.con_DIR_CONVERT + "\\" + sMoveDir);
                if (diChk.Exists == false)
                {
                    diChk.Create();
                }

                row.ConversionFlag = Define.CON_MSG_ERROR;                                
                File.Move(path + "\\WORK\\" + row.ZipFileName, path + "\\" + Define.con_DIR_CONVERT + "\\" + sMoveDir + "\\" + row.OrgZipFileName);
                row.ZipfileFullPath = path + "\\" + Define.con_DIR_CONVERT + "\\"+ sMoveDir;

                UpdateRowErrorFile(row);
            }
            else
            {
                row.CurState = Define.con_STATE_COMPLETE_CONV;
            }

            SaveResultInfo res = iqaService.updateZipFileAllInfo(row);

            if (res.Flag == 0)
            {
                //이럴일 있을까?
                SetLog(row, "ERR", res.Desc);
                AddLogFile(row, "ERR:ConvertResultProc", res.Desc);                
                return false;
            }
            return true;
        }
        /*
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

               

            }
            catch (Exception ex)
            {
                util.Log("ERR:unZipFileChk", ex.Message);
                Console.WriteLine(ex.Message.ToString());
            }

            return true;
        }
        */

        private void serviceOnOff()
        {
            SetControlServiceBtnOnOff();
            if (m_bServiceOnOffButton)
            {
                ServiceOnMain();
            }
            else
            {
                if (m_zipFileWatcher.EnableRaisingEvents)
                {
                    m_zipFileWatcher.EnableRaisingEvents = false;
                }
                m_eventOrifileLoopCnt = 0;
            }
        }

        private void ServiceOnMain()
        {
            Thread thread = new Thread(new ThreadStart(delegate ()
            {
                ServiceOnQueueStartProc();
            }));
            thread.Start();            
        }
        /* Queue 방식으로 변경처리후 아래 실제 사용안함 나중 Queue 방식 문제 없을시 삭제 처리
        
        private void serviceOnMainProc()
        {
            Thread thread = new Thread(new ThreadStart(delegate ()
            {
                serviceOnThreadProc();
            }));
            thread.Start();

        }
         */
        /*
        작업에 필요한 폴더를 생성한다.
        */
        private void AppWorkDirMake(string path)
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

            diChk = new DirectoryInfo(path + "\\" + Define.con_DIR_EVENT_ORIFILE);
            if (diChk.Exists == false)
            {
                diChk.Create();
            }

        }

        private void DeleteVariableFileProcessProc()
        {

            if (m_dicFileProcess.Count <= 0)
            {
                return;
            }

            List<string> lstDelKey = new List<string>();

            foreach (var key in m_dicFileProcess.Keys.ToList())
            {

                if (!(m_dicFileProcess[key].Complete || m_dicFileProcess[key].Pass))
                {
                    continue;
                }

                string zipFileName = m_dicFileProcess[key].ZipFileName;
                string[] arFileInfo = zipFileName.Split('_');
                var fileWorkDate = arFileInfo[arFileInfo.Length - 1];

                DateTime dtFileWirkDate = new DateTime();
                Boolean isDate = util.ParseDate(fileWorkDate, ref dtFileWirkDate);

                if (!isDate)
                {
                    continue;
                }

                fileWorkDate = Path.GetFileNameWithoutExtension(fileWorkDate);
                DateTime dtCurDate = DateTime.Now;
                //DateTime dtFileWirkDate = DateTime.ParseExact(fileWorkDate, "yyyyMMddHHmmss", null);
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
                    this.Invoke(new Action(delegate () {
                        nDeleteRow = dgvProcess.getFindZipFile2RowIndex(key, Define.con_KEY_PROCESS_COLIDX);

                        if (nDeleteRow >= 0)
                        {
                            dgvProcess.Rows.RemoveAt(nDeleteRow);
                        }
                    }));                    
                    m_dicFileProcess.Remove(key);                    
                }
            }            
        }

        private void ServiceOnQueueStartProc()
        {
            //로딩시 해당폴더 기존에 있던 Zip파일 읽어들인다.
            SetControlServiceBtnOnOff();

            Dir2ZipfileQueueAdd();
            //그이후 추가되는 Zip파일 감시
            m_zipFileWatcher.EnableRaisingEvents = true;

            //시작부터~ 압축 해제전까지 작업 큐
            Thread thread = new Thread(new ThreadStart(delegate ()
            {
                RunQueueZIpfileWork();
            }));
            thread.Start();

            //컨버터 부터 ~ FTP 전송까지 작업큐(컨버터 작업이 오래걸려 작업하는동안 압축해제전까지 미리 처리하기 위해 나눈다.)
            Thread thread2 = new Thread(new ThreadStart(delegate ()
            {
                RunQueueZIpfileWork2();
            }));
            thread2.Start();

        }

        private string GetSystemZipfileNm2OrgFIleName(string fileName)
        {
            //fileName = "강북본부_도로407조_LTEV_SKT_AUTO_롯데_20181201_20181205132239.zip";
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            int nPos = fileNameWithoutExtension.LastIndexOf("_");
            fileNameWithoutExtension = fileNameWithoutExtension.Substring(0, nPos);

            return fileNameWithoutExtension + ".zip";
        }

        private FileProcess GetFilePath2FileProcessInfo(string fileName)
        {
            string onlyFileName = Path.GetFileName(fileName);
            string moveFileName = GetMoveFileName(fileName);
            string fullPath = Path.GetDirectoryName(fileName);
            //int systemDatePos = moveFileName.LastIndexOf('_');

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(moveFileName);
            string[] arInfo = fileNameWithoutExtension.Split('_');
            string uploadDate = arInfo[arInfo.Length-1];
            uploadDate = uploadDate.Substring(0, 8);

            FileProcess row = new FileProcess();
            row.ProcessServer = GetFtpName(m_ftpId);
            row.ZipFileName = moveFileName;                     //2018-06-14 : Zip 파일명도 시스템 날짜 추가된거로 관리하는거로 요청옴                                                                    
            row.OrgZipFileName = onlyFileName;            
            row.ZipfileFullPath = fullPath;
            row.CurState = Define.con_STATE_WAIT;
            row.UploadDate = uploadDate;


            return row;
        }

        private string GetUseableZIpFileName(string zipfileName)
        {
            string useableZipfileName = zipfileName;
            useableZipfileName = useableZipfileName.Replace(",", "%%");

            return useableZipfileName;

        }
        private void Dir2ZipfileQueueAdd()
        {
            string path = GetDefaultFtpPath();
            if (Directory.Exists(path))
            {
                m_ServiceOnIng = true;
                AppWorkDirMake(path);

                //요청한 DRM파일을 FTP로 전송한다.
                //eventOrifileSendProc();

                //하루 지난거만 초기화 한다.                
                //DeleteVariableFileProcessProc();

                string[] fileEntries = Directory.GetFiles(path, "*.zip");
                //ZIP파일정보 미리 DB 업뎃하기 위해
                List<FileProcess> dbInsertList = new List<FileProcess>();
                foreach (string fileName in fileEntries)
                {
                    FileProcess row = GetFilePath2FileProcessInfo(fileName);
                    //m_dicFileProcess.Add(row.OrgZipFileName, row);
                    m_dicFileProcess.Add(row.ZipFileName, row);
                    dbInsertList.Add(row);
                }
                //DRM 중복체크는 안하는거로 함 어짜피 drm 중복체크를 하고 또 ZIP파일을 다시 올릴수도있어서 중간 실패시 문제가 될수있어 체크 안함
                if (dbInsertList.Count > 0)
                {
                    SaveResultInfo res = iqaService.insertZipfileInfos(dbInsertList);
                    if (res.Flag == 0)
                    {
                        util.Log("ERR:DirLoad2ZipFileProc", "ZIP File 대기 정보 저장 실패");
                        //DB 저장이 안되어 그이후 행위 의미 없음 
                    }
                    else
                    {
                        foreach (FileProcess row in dbInsertList)
                        {
                            AddLogFile(row, "INFO", "IQA Controller Service Start Zipfile Que Enqueue(등록)");
                            m_QueueZipfileParsing.Enqueue(row);
                        }                     
                    }
                }
                else
                {
                    m_ServiceOnIng = false;
                }                              
            }
            else
            {
                m_ServiceOnIng = false;
                MessageBox.Show("해당 경로의 디렉토리가 존재 하지 않습니다.");                
            }
            
            SetControlServiceBtnOnOff();
        }

        private void CreateFolderWatching()
        {
            m_zipFileWatcher = new System.IO.FileSystemWatcher();
            m_zipFileWatcher.Path = GetDefaultFtpPath();
            m_zipFileWatcher.Filter = "*.zip";
            m_zipFileWatcher.Created += new FileSystemEventHandler(ZipFileCreated);

        }

        private void SetVariableServiceOnIng()
        {
            //Que작업이 완전히 없을경우 
            if (m_QueueZipfileParsing.Count <=0 && m_QueueZipfileParsing_2.Count <= 0)
            {
                m_ServiceOnIng = false;
            }
            else
            {
                m_ServiceOnIng = true;
            }
        }

        private void RunQueueZIpfileWork2()
        {
            try
            {
                while (m_bServiceOnOffButton)
                {
                    //m_ServiceOnIng = true;
                    while (m_QueueZipfileParsing_2.Count > 0)
                    {
                        m_ServiceOnIng = true;
                        FileProcess row = m_QueueZipfileParsing_2.Dequeue();

                        AddLogFile(row, "INFO", "Work_2 Que Dequeue 작업시작");                      
                        ZipfileParsing2(row);
                        Thread.Sleep(1000);
                    }
                    //m_ServiceOnIng = false;

                    SetVariableServiceOnIng();

                    SetControlServiceBtnOnOff();
                    Thread.Sleep(1000 * 1);      //1초마다 순회 체크
                }
            }
            catch (Exception ex)
            {
                m_ServiceOnIng = false;
                StopFtpFoldWatch();
                SetLog(null, "ERR", "RunQueueZIpfileWork2 예외발생_2:" + ex.Message.ToString());
                SetLog(null, "ERR", "FTP 폴더 감시 중지");
                Thread.Sleep(1000 * 2);
            }

        }

        private void RunQueueZIpfileWork()
        {
            try
            {
                while (m_bServiceOnOffButton)
                {
                    try
                    {
                        //DRM 요청파일 전송처리
                        m_eventOrifileLoopCnt++;
                        //5분마다 체크한다.
                        if (m_eventOrifileLoopCnt % (60*5) == 0)
                        {
#if (!DEBUG)
                                m_ServiceOnIng = true;
                                eventOrifileSendProc();
                                SetVariableServiceOnIng();

#endif
                            m_eventOrifileLoopCnt = 0;
                        }

                        //하루에 한번 체크한다.
                        string curDate = util.GetCurrentDate("{0:yyyyMMdd}");
                        if (m_checkedkDay != curDate)
                        {
                            m_ServiceOnIng = true;
                            m_checkedkDay = curDate;
                            //파일 보관주기에 따른 삭제 처리  
                            if (setFilePeriodInfo())
                            {
                                util.Log("INFO", "FilePeriodChk2Proc:파일보관주기 삭제 처리");
                                FilePeriodChk2Proc();
                            }
         
                            //한달 지난 그리드 데이타는 초기화 한다.    
                            DeleteVariableFileProcessProc();
                            SetVariableServiceOnIng();
                        }
                    }
                    catch (Exception ex)
                    {
                        //m_ServiceOnIng = false;
                        SetLog(null, "ERR", "RunQueueZIpfileWork - 기타 작업 오류 발생:" + ex.Message.ToString());
                    }
                    
                    while (m_QueueZipfileParsing.Count > 0)
                    {
                        m_ServiceOnIng = true;  
                        
                        FileProcess row = m_QueueZipfileParsing.Dequeue();

                        //FileProcess row = m_QueueZipfileParsing.Peek();

                        if (!m_dicFileProcess.ContainsKey(row.ZipFileName))
                        {                            
                            m_dicFileProcess.Add(row.ZipFileName, row);
                        }
                        else
                        {
                            //이미 등록 되있을경우 처리 패스 해야하나?  - 이럴일 없음
                            m_dicFileProcess[row.ZipFileName] = row;
                        }

                        AddLogFile(row, "INFO", "Work_1 Que Dequeue 작업시작");
                        //DRM서버로 전송되는 중간에 취소 되는경우 발생함
                        Boolean bAccessible = util.Wait2FileAccessible(row.OrgZipFIleFullName, 9999);
                      
                        Thread.Sleep(1000);
                        ZipfileParsing(row);
                      
                        /* 추후 모든 작업 쓰레드 처리 할경우
                        Thread thread = new Thread(new ThreadStart(delegate ()
                        {
                            ZipfileParsing(row.OrgZipFIleFullName);
                        }));
                        thread.Start();
                        */
                        //ZipfileParsing(row.OrgZipFIleFullName);                        
                        Thread.Sleep(1000);
                    }

                    //작업 2단계까지의 작없이 없다면 작업중 상태 off 한다.                    
                    SetVariableServiceOnIng();
                    SetControlServiceBtnOnOff();
                    Thread.Sleep(1000);      //10초마다 순회 체크
                }
            }
            catch (Exception ex)
            {
                m_ServiceOnIng = false;
                StopFtpFoldWatch();
                SetLog(null, "ERR", "RunQueueZIpfileWork - 예외발생:" + ex.Message.ToString());
                SetLog(null, "ERR", "FTP 폴더 감시 중지");
                //Thread.Sleep(1000 * 5);  
            }            
        }


        private void StopFtpFoldWatch()
        {
            if (m_zipFileWatcher.EnableRaisingEvents)
            {
                m_zipFileWatcher.EnableRaisingEvents = false;
            }
        }

        /* FTP 폴더 감시에 따른 파일 추가시 Callback 함수
        */
        private void ZipFileCreated(Object source, FileSystemEventArgs e)
        {
            string zipfileFullPath = e.FullPath;            
            FileProcess row = GetFilePath2FileProcessInfo(zipfileFullPath);

            List<FileProcess> dbInsertList = new List<FileProcess>();
            dbInsertList.Add(row);
            SaveResultInfo res = iqaService.insertZipfileInfos(dbInsertList);
            if (res.Flag == 0)
            {
                AddLogFile(row, "ERR:ZipFileCreated", "ZIP File 대기 정보 저장 실패");                
                return;
            }
            AddLogFile(row, "INFO", "Zipfile Add Queue Enqueue(등록)");

            //기존 키에 중복되는애가 있을경우 완료나 처리중일경우? -FTP04경우 동일한 zip파일명이 들어옴
            m_QueueZipfileParsing.Enqueue(row);
        }
        /*
        private void serviceOnThreadProc()
        {            
            string path = GetDefaultFtpPath();
            if (Directory.Exists(path))
            {
                m_ServiceOnIng = true;
                AppWorkDirMake(path);
                
                //요청한 DRM파일을 FTP로 전송한다.
                //eventOrifileSendProc();
                
                //하루 지난거만 초기화 한다.                
                //DeleteVariableFileProcessProc();

                string[] fileEntries = Directory.GetFiles(path,"*.zip");
                //ZIP파일정보 미리 DB 업뎃하기 위해
                List<FileProcess> dbInsertList = new List<FileProcess>();
                foreach (string fileName in fileEntries)
                {

                    FileProcess row = GetFilePath2FileProcessInfo(fileName);
                  
                    m_dicFileProcess.Add(row.OrgZipFileName, row);

                    dbInsertList.Add(row);
                }

                //DRM 중복체크는 안하는거로 함 어짜피 drm 중복체크를 하고 또 ZIP파일을 다시 올릴수도있어서 중간 실패시 문제가 될수있어 체크 안함
                if (dbInsertList.Count > 0)
                {
                    SaveResultInfo res = iqaService.insertZipfileInfos(dbInsertList);
                    if (res.Flag == 0)
                    {
                        //DB 저장이 안되어 그이후 행위 의미 없음
                        util.Log("ERR:serviceOnThreadProc", "ZIP File 대기 정보 저장 실패");
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
                    //ZipfileParsing(pair.Value.ZipfileFullPath+"\\"+ pair.Value.OrgZipFileName);
                    ZipfileParsing(m_dicFileProcess[key].ZipfileFullPath + "\\" + m_dicFileProcess[key].OrgZipFileName);

                    //ZipfileParsing(fileName);
                    Thread.Sleep(4000);
                }
                
                m_ServiceOnIng = false;
                Thread.Sleep(500);

                //주기를 주자
                //파일 보관주기에 따른 삭제 처리
                //FilePeriodChk2Proc();

                if (m_bServiceOnOffButton)
                {
                    if (!m_formClose)
                    {
                        //서비스 On 상태면 재귀호출한다.
                        StartTmrServiceBtn();
                        serviceOnThreadProc();
                    }                    
                }
                else
                {
                    //m_bServiceOn = false;
                    EndTimerServiceBtn();
                    setControlServiceOnOff();
                }
            }
            else
            {
                MessageBox.Show("해당 경로의 디렉토리가 존재 하지 않습니다.");
                btnService.PerformClick();
                //서비스 멈춘다.
            }            
        }
        */
        private string GetDefaultFtpPath()
        {
            return @"D:\" + GetFtpName(m_ftpId);
            //return Define.CON_DEFAULT_DIR + m_ftpId;
        }

        private string GetConvertLogFileName(string sRunTime, Boolean bSti, int maxSec)
        {
            //해당 파일을 찾는다.
            string path = "";

            if (bSti)
            {
                path = @"D:\STI\Check List";
            }
            else
            {
                path = @"D:\INNOWHB\Check List";
            }

            DateTime dtConvertorRunDate = DateTime.ParseExact(sRunTime, "yyyyMMddHHmmss", null);
            int nWaitSec = 0;
            string sConvertorLogFile = "";
            while (sConvertorLogFile.Length <= 0 && maxSec >= nWaitSec)
            {

                string[] fileEntries = Directory.GetFiles(path);
                string sDirFileName = "";
                string sWithoutExtensionName = "";

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
                        if (arFileName[2] == "END" || arFileName[2] == "CON" || arFileName[2] == "ERR")
                        {
                            sConvertorLogFile = sWithoutExtensionName;
                            break;
                        }
                    }
                }

                Thread.Sleep(1000);
                nWaitSec++;
            }

            if (sConvertorLogFile.Length <= 0)
            {
                return "";
            }

            SetLog(null, "INFO", "컨버전로그 파일 : " + sConvertorLogFile);

            /*  로그를 가장 나중에 쌓을떄도 있어서 우선 아래 부분 막는다. 
             
            //파일은 찾았으나 해당 파일에 로그가 안찍힐 경우 컨버터 비정상임 - 프로세스 사용중 에러 발생으로 사이즈 체크한다.
            Thread.Sleep(1000);
            Boolean bConvertError = true;
            int nFindCnt = 0;
            while (nFindCnt <=5 && bConvertError)
            {

                string tmpCopyFile = GetDefaultFtpPath();
                tmpCopyFile+= "\\copyLog.txt";

                File.Copy(sConvertorLogFile, tmpCopyFile, true);

                string[] lines = System.IO.File.ReadAllLines(tmpCopyFile, Encoding.Default);
                foreach (string log in lines)
                {
                    // System.Console.WriteLine(log);
                    if (log.IndexOf("dml") > 0 || log.IndexOf("qms") > 0 || log.IndexOf("drm") > 0)
                    {
                        bConvertError = false;
                        break;
                    }
                }

                Thread.Sleep(2000);
                nFindCnt++;
            }

            if (bConvertError)
            {
                sConvertorLogFile = "";
            }
            */
            return sConvertorLogFile;
        }

        /* 컨버터 실행시 컨버터가 처리 완료되었는지 확인 처리
         * 
        */
        private string isConvertorRunComplete(string sLogFiileName , Boolean bSti)
        {

            string path = "";

            if (bSti)
            {
                path = @"D:\STI\Check List";
            }
            else
            {                
                path = @"D:\INNOWHB\Check List";
            }

            string[] arFileName = sLogFiileName.Split('_');
            string  sFindLogFileName = arFileName[0] + arFileName[1];


            //DateTime dtConvertorRunDate = DateTime.ParseExact(sRunTime, "yyyyMMddHHmmss", null);

            string[] fileEntries = Directory.GetFiles(path);
            string sDirFileName = "";
            string sWithoutExtensionName = "";
            string sFindFileName = "";

            foreach (string fileName in fileEntries)
            {
                /*
                sWithoutExtensionName = Path.GetFileNameWithoutExtension(fileName);
                string[] arFileName = sWithoutExtensionName.Split('_');
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
                */
                sWithoutExtensionName = Path.GetFileNameWithoutExtension(fileName);

                string[] arTmpDirFileName = sWithoutExtensionName.Split('_');
                sDirFileName = arTmpDirFileName[0] + arTmpDirFileName[1];

                if (sFindLogFileName == sDirFileName)
                {
                    if (arTmpDirFileName[2] == "END")
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
            string defaultPath = GetDefaultFtpPath();
            string sWithoutExtensionName = Path.GetFileNameWithoutExtension(row.ZipfileFullPath + "\\" + row.ZipFileName);
            string extractDir = defaultPath + "\\WORK\\" + sWithoutExtensionName;
            return extractDir;
        }

        /* 압축 해제 한다. (실패시 어떻게 파악하나? 압축해제 실패시 확인방법 찾기)
         * 
         */
        private Boolean UnZIpProc(FileProcess row)
        {
            row.ExtractFlag = Define.CON_MSG_ING;
            row.CurState = Define.con_STATE_START_UNZIP;
            iqaService.updateZipfileMainInfo(row, Define.con_STATE_START_UNZIP);

            UpdateRowFileProcess(row);
            string extractDir = getUnzipPath(row);
            
            try
            {
                //7Zip 이용방식 - 기존 컨트롤러 처리 방식
                //https://m.blog.naver.com/PostView.nhn?blogId=koromoon&logNo=120208838111&proxyReferer=https%3A%2F%2Fwww.google.co.kr%2F                

                DirectoryInfo di = new DirectoryInfo(extractDir);
                if (di.Exists == false)
                {
                    di.Create();
                }

                extractDir += @"\";
                /*
                execUnzipList("l " + row.ZipfileFullPath + "\\" + row.ZipFileName + " > " + "D:\\FTP14" + "\\unzipInfo.txt");
                Exec(Define.CON_ZIP_EXE_PATH + "7z.exe", "x -o" + extractDir + "\\ -r -y " + row.ZipfileFullPath + "\\" + row.ZipFileName);                                
                unZipCompleteChk(extractDir + "\\" + "unzipInfo.txt", extractDir);
                */

                //string extractDir = @"D:\FTP14\WORK\본사_도로2조_LTED_SKT_L5_고양시 일산동구_20181015_20181017113119\";
                //string zipfileFullPath = @"D:\FTP14\WORK\본사_도로2조_LTED_SKT_L5_고양시 일산동구_20181015_20181017113119.zip";
                string zipFilefullPath = row.ZipfileFullPath + @"\" + row.ZipFileName;
                SevenZipCall("x -o\"" + extractDir + "\" -r -y \"" + zipFilefullPath + "\"");

                /*.Net Framework 이용 ㅠㅠ FTP서버에서 같은 닷넷버전인데 오류발생에 따른 7zip 이용한다.
                string zipPath = row.ZipfileFullPath + "\\" + row.ZipFileName;
                //string extractPath = @"D:\FTP14\TEST\본사_도로2조_CSFB_SKT_AUTO_청주시서원구_20180829";
                ZipFile.ExtractToDirectory(zipPath, extractDir);
                */
            }
            catch (Exception ex)
            {
                row.ExtractFlag = Define.CON_MSG_ERROR;
                row.CurState = Define.con_STATE_ERR_UNZIP;
                SetLog(row,"ERR", "압축 해제 실패 : " + ex.ToString());                
                //AddLogFile(row, "ERR:UnZIpProc", "압축 해제 실패" + ex.ToString());
                return false;
            }

            row.ExtractFlag = Define.CON_MSG_COMPLETE;
            row.CurState = Define.con_STATE_COMPLETE_UNZIP;
            row.ZipfileFullPath = extractDir;
            return true;
                      
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
                util.Log("ERR:MoveFile", ioex.Message);
                throw ioex;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("An Exception occured during move, " + ex.Message);
                //result = ex.HResult;
                util.Log("ERR:MoveFile", ex.Message);
                throw ex;
            }
        }

        private void btnTerminal_Click(object sender, EventArgs e)
        {
            Exec("cmd.exe", "");           
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

        private void runServyceType2RunConvertor(string serviceType,string path,Boolean bSti)
        {
            int nIdx = 0;

            if (bSti)
            {
                nIdx = 4;
            }
            else
            {
                serviceType = serviceType.ToUpper();

                if (serviceType == "LTED")
                {
                    nIdx = 0;
                }
                else if (serviceType == "LTEV" || serviceType == "HDV" || serviceType == "HDVML" || serviceType == "CSFB" || serviceType == "CSFBML")
                {
                    nIdx = 2;
                }
                else if (serviceType == "HSDPA")
                {
                    nIdx = 1;
                }
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
                    //Exec("D:\\INNOWHB\\QMS-W.exe", type + path + " L_Data.cfg ");
                    Exec("D:\\INNOWHB\\QMS-W.exe", type + "\"" + path + "\"" +" L_Data.cfg ");
                    break;
                case 1:     // Inno HSDPA Data                                        
                    Exec("D:\\INNOWHB\\QMS-W.exe", type + "\"" + path + "\"" + " H_Data.cfg");
                    break;
                case 2:     // Inno LTE 음성(M to M) 컨버터                                        
                    Exec("D:\\INNOWHB\\QMS-W.exe", type + "\"" + path + "\"" + " L_Voice.cfg");
                    break;                
                case 4:     // STI 컨버터

                    if (path.Length <= 0)
                    {
                        Exec("D:\\STI\\QMS Export.exe", "");
                    }
                    else
                    {
                        Exec("D:\\STI\\QMSExportConsole.exe", path + " " + @"D:\STI\Check List");
                    }    
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }
        }

        private void reSendFtp(FileProcess row, string localPath)
        {

            //ControllerServerEntity freeFtpServer = iqaService.getFreeFtpServerInfo();
            ControllerServerEntity freeFtpServer = m_dicFtpInfo[Define.con_FTP_QMS];

            if (freeFtpServer == null)
            {
                SetLog(row, "ERR", "FTP 서버 정보 가져오기 실패");
                AddLogFile(row, "ERR:reSendFtp", "FTP 서버 정보 가져오기 실패");                
                return;
            }

            SendFtp(row, freeFtpServer, localPath, true);

            //성공시 폴더 삭제

            //dbUpdate한다.

            row.FtpSendFlag = Define.CON_MSG_COMPLETE;
            row.Complete = true;
            SetLog(row, "INFO", "처리 완료");
            UpdateRowFileProcess(row);
            row.CurState = Define.con_STATE_WORK_COMPLETE;

            //dbUpdateZipfileComplete(row);
            iqaService.updateZipFileAllInfo(row);

        }

        private Boolean SendSFtp(FileProcess row , ControllerServerEntity freeServerInfo , string localPath, Boolean flagResend)
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
                                SetLog(row, "ERR", "[ERR]:[SendSFtp]:" + exFtp.Message);                                
                                //AddLogFile(row, "ERR:SendSFtp", exFtp.Message);
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
                    row.FtpSendFlag = Define.CON_MSG_COMPLETE;

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
                                SetLog(row, "ERR", "[ERR]:[SendSFtp Wav]:" + exFtp.Message);                                
                                //AddLogFile(row, "ERR:SendSFtp Wav", exFtp.Message);
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
                SetLog(row, "ERR", "[ERR]:[SendSFtp]:" + exception.Message);
                //AddLogFile(row, "ERR:SendSFtp", exception.Message);                
                return false;
            }
            return true;
        }

        private Boolean sendSFtpWav(FileProcess row , string localPath)
        {

            ControllerServerEntity freeServerInfo = m_dicFtpInfo[Define.con_FTP_WAV];

            try
            {
                string fileName = "";
                int nFtpFileCnt = 0;
                int nFtpSuccessCnt = 0;

                using (var client = new SftpClient(freeServerInfo.Ip, Convert.ToInt32(freeServerInfo.Port), freeServerInfo.Id, freeServerInfo.Password))
                {
                    client.Connect();
                    client.ChangeDirectory(freeServerInfo.Path);

                    //WAV FILE 전송
                    string[] directories = Directory.GetDirectories(localPath);

                    client.ChangeDirectory(freeServerInfo.Path);
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

                        //WAVE 폴더 Qms내부 이름으로 디렉토리 생성                            //확장자 포함으로 폴더 생성요청
                        waveFolder = Path.GetFileName(detail.QmsfileNmChg);
                        client.CreateDirectory(freeServerInfo.Path + waveFolder);
                        client.ChangeDirectory(freeServerInfo.Path + waveFolder + "/");

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
                                SetLog(row, "ERR", "sendSFtpWav:" + exFtp.Message);
                                //AddLogFile(row, "ERR:SendSFtp Wav", exFtp.Message);
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
            catch (Exception exception)
            {
                SetLog(row, "ERR", "sendSFtpWav:" + exception.Message);                
                return false;
            }

            return true;

        }

        private Boolean SendFtp(FileProcess row , ControllerServerEntity freeServerInfo , string localPath , Boolean flagResend)
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
                            SetLog(row, "ERR", "[ERR]:[SendSFtp]:" + fileName + " FTP 전송 에러발생");
                            detail.FlagFtpSend = Define.con_FAIL;                           
                        }
                    }
                    else
                    {
                        SetLog(row, "ERR", "[ERR]:[SendSFtp]:" + fileName + "QMS파일 매칭 못함");
                    }
                }

                row.FtpSendServer = freeServerInfo.Name;
                row.FtpSendFileCnt = nFtpFileCnt;
                row.FtpSendSuccessCnt = nFtpSuccessCnt;
                row.FtpSendFlag = Define.CON_MSG_COMPLETE;

                //WAV는 SFTP로 보내야 한다고 함 ㅠㅠ

#if DEBUG

#else
               sendSFtpWav(row, localPath);
#endif


                //WAV FILE 전송
                /* FTP 방식 WAV는 SFTP로 보내야 한다고 함 ㅠㅠ
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

                        Boolean bSuccess = ftpClient.upload("MOV/"+ waveFolder + "/" + wavFileName, wavFilePath);
                        if (bSuccess)
                        {
                            nFtpSuccessCnt++;
                            detail.FlagFtpSend = Define.con_SUCCESS;
                            wavInfo.FlagSend = "1";
                        }
                        else
                        {
                            SetLog(row, "ERR", "[ERR]:[SendSFtp Wav]:" + fileName + " FTP 전송 에러발생");
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
                */
            }
            catch (Exception exception)
            {
                SetLog(row, "ERR", "[ERR]:[SendFtp]:" + exception.Message);
                //AddLogFile(row, "ERR:SendFtp", exception.Message);                
                return false;
            }
            return true;

        }

        private Boolean sendSftpEventOrifile(string localPath)
        {
            ControllerServerEntity freeServerInfo = m_dicFtpInfo[Define.con_FTP_DRM];
            
            try
            {                
                using (var client = new SftpClient(freeServerInfo.Ip, Convert.ToInt32(freeServerInfo.Port), freeServerInfo.Id, freeServerInfo.Password))
                {
                    //string curFolder = util.GetCurrentDate("{0:yyyyMMdd}");
                    string remoteFileName = Path.GetFileName(localPath);
                    string curFolder = remoteFileName.Split('_')[2];
                    curFolder = curFolder.Substring(0, 8);

                    client.Connect();
                    client.ChangeDirectory(freeServerInfo.Path + curFolder + "/");
                    
                    try
                    {
                        using (FileStream fs = new FileStream(localPath, FileMode.Open))
                        {
                            long totalLength = fs.Length;
                            client.BufferSize = (uint)totalLength;
                            client.UploadFile(fs, remoteFileName);                            
                        }
                    }
                    catch (Exception exFtp)
                    {
                        SetLog(null, "ERR", "sendSftpEventOrifile:" + exFtp.Message.ToString());                        
                    }
                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {
                SetLog(null, "ERR", "sendSftpEventOrifile:" + ex.Message.ToString());
                return false;
            }
            return true;
        }

        private Boolean sendFtpEventOrifile(ControllerServerEntity freeServerInfo  , string localPath)
        {
            Boolean bSuccess = false;
            string ftpUrl = "ftp://" + freeServerInfo.Ip + ":" + freeServerInfo.Port + "/" + freeServerInfo.Path;
            try
            {
                Ftp ftpClient = new Ftp(ftpUrl, freeServerInfo.Id, freeServerInfo.Password);
                string remoteFileName = Path.GetFileName(localPath);
                bSuccess = ftpClient.upload(remoteFileName , localPath);

                if (bSuccess)
                {
                    
                }
                else
                {
                    //SetLog(row, "[ERR]:[SendSFtp]:" + fileName + " FTP 전송 에러발생");
                    //detail.FlagFtpSend = Define.con_FAIL;
                }
                /*
                row.FtpSendServer = freeServerInfo.Name;
                row.FtpSendFileCnt = nFtpFileCnt;
                row.FtpSendSuccessCnt = nFtpSuccessCnt;
                row.FtpSendFlag = Define.CON_MSG_COMPLETE;
                */
            }
            catch (Exception exception)
            {
                //SetLog(row, "[ERR]:[SendFtp]:" + exception.Message);
                util.Log("ERR:sendFtpEventOrifile", exception.Message);                
            }
            return bSuccess;
        }

        private void onClickMenuConvertor(Object sender, System.EventArgs e)
        {
            int nMenuIdx = ((MenuItem)sender).Index;
            execConvertor(nMenuIdx,"");
        }

        private Boolean SevenZipCall (string arg)
        {
            Boolean bComplete = false;
            try
            {                
                string zPath = Define.CON_ZIP_EXE_PATH + "7z.exe";
                System.Diagnostics.ProcessStartInfo proc = new System.Diagnostics.ProcessStartInfo();
                proc.FileName = zPath;
                proc.Arguments = arg;
                //proc.RedirectStandardInput = true;
                //proc.RedirectStandardOutput = true;
                //proc.RedirectStandardError = true;
                //proc.UseShellExecute = false;
                //proc.CreateNoWindow = true;
                proc.WindowStyle = ProcessWindowStyle.Hidden;
                System.Diagnostics.Process p = System.Diagnostics.Process.Start(proc);
                /*
                while (!p.HasExited)
                {
                    outPut = p.StandardOutput.ReadLine();
                    outPut += "\n" + outPut;
                }            
                p.Close();
                */
                p.WaitForExit();
                bComplete = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                throw ex;                
            }
            return bComplete;

        }

        
        private void Exec(string path , string arg)
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
            util.SetIniWriteString("FTP", "SELECT", GetFtpName(m_ftpId), Application.StartupPath + "\\controllerSet.Ini");
        }

        private void btnService_Click(object sender, EventArgs e)
        {
            
            if (m_ServiceOnIng && btnService.Enabled)
            {
                //우선 새로 들어오는 Zip파일들 감시는 중지 시킨다.

                if (m_zipFileWatcher.EnableRaisingEvents)
                {
                    m_zipFileWatcher.EnableRaisingEvents = false;
                }
            }
            else if (m_ServiceOnIng && btnService.Enabled == false)        //서비스 중지 요청중...
            {
                return;
            }
            
            m_bServiceOnOffButton = !m_bServiceOnOffButton;
            
            serviceOnOff();
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
                Exec("explorer.exe", fileDir);              
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
                    Exec("explorer.exe", fileDir);
                }
            }else if (e.ColumnIndex == 5)
            {             
                if (m_dicFileProcessErr.ContainsKey(zipfileNm))
                {
                    FileProcess selInfo = m_dicFileProcessErr[zipfileNm];
                    if (selInfo.FileNameRule == Define.CON_MSG_ERROR)
                    {
                        PopFileModify(zipfileNm);
                    }else if (selInfo.FtpSendFlag == Define.CON_MSG_COMPLETE && selInfo.FtpSuccessFlag == Define.con_FAIL)
                    {
                        //FTP 재전송 한다.
                        DialogResult result = MessageBox.Show("FTP 재전송 하시겠습니까?", "FTP 재전송", MessageBoxButtons.YesNoCancel);
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
            
            if (m_bServiceOnOffButton && m_ServiceOnIng)
            {
                MessageBox.Show("아직 작업 처리중입니다. 잠시후 다시 종료하여 주십시요.");
                e.Cancel = true;
            }

            m_bServiceOnOffButton = false;
            m_formClose = true;            
            EndTimerServiceBtn();
            timer.Dispose();

            //Application.ExitThread();
            //Environment.Exit(0);

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
                        if (!DBContollerSetInfo(false))
                        {
                            MessageBox.Show("컨트롤러 실행에 필요한 데이타를 가져오는데 실패하였습니다.\n관리자에게 문의하십시요.");
                            this.Close();
                            return;
                        }
                        UpdateRowEnv();
                    }
                }                
            }
        }

        public void loadingOn(string url)
        {
            ucLoadingBar.On(url);
        }

        public void loadingOff()
        {
            ucLoadingBar.Off();
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            
        }

        private void btnWorkQueCOnfirm_Click(object sender, EventArgs e)
        {

        }



        /* not used : 추후 확인후 삭제 
        private void execUnzipList(string path)
        {
            try
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
            catch (Exception ex) {
                
            }            
        }
        */


    }
}
