using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqaController.entity
{
    class FileProcess
    {
        private string measuDate = "";              // Zip파일 측정일 정보 Upload_date에 해당 값 넣는다.
        private string filePos = "";                // 파일
        private string processServer = "";          // 처리서버
        private string measuGroup = "";             // 측정조
        private string measuBranch = "";            // 측정본부
        private string zipFileName = "";            // zip 파일 작업가공 이름
        private string orgZipFileName = "";         // zip 파일 이름 원본이름
        private string workZipFileName = "";        // 임시 작업성 zip파일 명
        private string inoutType = "";
        private string comp = "";
        private string serviceType = "";            //서비스 타입

        // private string fileName = "";            // 파일 이름
        private string fileNameRule = "";           // 파일명 규칙
        private string fileNameFlag = "";           // 파일명 규칙 에러 플래그
        private string extractFlag = "";            // 압축해제 에러 플래그
        private string conversionFlag = "";         // 컨버전에러 플래그
        private string ftpSendFlag = "";            // ftp 전송 시도 여부
        private string ftpSuccessFlag = "";         // ftp 전송 성공여부 - qms 파일 하나라도 실패시 "0"

        private string backupFlag = "";             // 백업
        private string zipDupFlag = "";             // zip파일 중복

        private Boolean pass = false;                // 해당파일 패스할지 여부
        private Boolean complete = false;           // 전체 성고 여부
        private string fileSize = "";               //파일 사이즈
        private string unzipfileCnt = "";
        private string dupfileCnt = "";
        private string completefileCnt = "";
        private string parsingCompleteCnt = "";
        private string zipfileFullPath = "";        //Zip 파일경로
        private string curState = Define.con_STATE_WAIT;
        private string ftpSendServer = "";
        private string flagFtpSend = "0";
        private int ftpSendSuccessCnt = 0;
        private int ftpSendFileCnt = 0;

        

        //private List<QmsFileInfo> qmsFileList = new List<QmsFileInfo>();
        private List<ZipFileDetailEntity> zipfileDetailList = new List<ZipFileDetailEntity>();


        public string FilePos { get => filePos; set => filePos = value; }
        public string ProcessServer { get => processServer; set => processServer = value; }
        public string MeasuGroup { get => measuGroup; set => measuGroup = value; }
        public string MeasuBranch { get => measuBranch; set => measuBranch = value; }
        public string ZipFileName { get => zipFileName; set => zipFileName = value; }
        // public string FileName { get => fileName; set => fileName = value; }
        public string FileNameRule { get => fileNameRule; set => fileNameRule = value; }
        public string ExtractFlag { get => extractFlag; set => extractFlag = value; }
        public string ConversionFlag { get => conversionFlag; set => conversionFlag = value; }
        public string FtpSendFlag { get => ftpSendFlag; set => ftpSendFlag = value; }
        public string BackupFlag { get => backupFlag; set => backupFlag = value; }
        public bool Complete { get => complete; set => complete = value; }
        public bool Pass { get => pass; set => pass = value; }
        public string FileSize { get => fileSize; set => fileSize = value; }
        public string FileSize1 { get => fileSize; set => fileSize = value; }
        public string UnzipfileCnt { get => unzipfileCnt; set => unzipfileCnt = value; }
        public string DupfileCnt { get => dupfileCnt; set => dupfileCnt = value; }
        public string CompletefileCnt { get => completefileCnt; set => completefileCnt = value; }
        public string ParsingCompleteCnt { get => parsingCompleteCnt; set => parsingCompleteCnt = value; }
        public string CurState { get => curState; set => curState = value; }
        public string ZipfileFullPath { get => zipfileFullPath; set => zipfileFullPath = value; }
        public string FileNameFlag { get => fileNameFlag; set => fileNameFlag = value; }        
        public string FtpSendServer { get => ftpSendServer; set => ftpSendServer = value; }
        public string FlagFtpSend { get => flagFtpSend; set => flagFtpSend = value; }
        public int FtpSendSuccessCnt { get => ftpSendSuccessCnt; set => ftpSendSuccessCnt = value; }
        public int FtpSendFileCnt { get => ftpSendFileCnt; set => ftpSendFileCnt = value; }
        public string ZipDupFlag { get => zipDupFlag; set => zipDupFlag = value; }
        public string FtpSuccessFlag { get => ftpSuccessFlag; set => ftpSuccessFlag = value; }
        public string InoutType { get => inoutType; set => inoutType = value; }
        public string Comp { get => comp; set => comp = value; }
        public string OrgZipFileName { get => orgZipFileName; set => orgZipFileName = value; }
        public string WorkZipFileName { get => workZipFileName; set => workZipFileName = value; }
        public string MeasuDate { get => measuDate; set => measuDate = value; }
        public string ServiceType { get => serviceType; set => serviceType = value; }

        //internal List<QmsFileInfo> QmsFileList { get => qmsFileList; set => qmsFileList = value; }
        internal List<ZipFileDetailEntity> ZipfileDetailList { get => zipfileDetailList; set => zipfileDetailList = value; }


        public string OrgZipFIleFullName { get => this.zipfileFullPath + "\\" + this.orgZipFileName; }          //Path+orgZipFileName
        public string ZipFileFullFileName { get => this.zipfileFullPath + "\\" + this.zipFileName; }            //Path+zipFileName

        public ZipFileDetailEntity findZipfileDetail(string flag , string key)
        {
            ZipFileDetailEntity find = null;

            foreach(ZipFileDetailEntity detail in this.zipfileDetailList)
            {

                if (flag == "unzipNm")
                {
                    if (detail.UnzipfileNm == key)
                    {
                        find = detail;
                        break;
                    }
                }else if (flag == "qmsNm")
                {
                    if (detail.QmsfileNm == key)
                    {
                        find = detail;
                        break;
                    }
                }
                else if (flag == "onlyUnzipNm")
                {
                    if (Path.GetFileNameWithoutExtension(detail.UnzipfileNm)  == key)
                    {
                        find = detail;
                        break;
                    }
                }

            }
            return find;
        }

        public Dictionary<string, Object> getDomain(Boolean bDetail)
        {
            Dictionary<string, Object> domain = new Dictionary<string, object>();

            Dictionary<string, Object> domainZM = new Dictionary<string, object>();     //ZipFileMain

            domainZM.Add("zipfileNm", HttpUtility.UrlEncode(this.zipFileName));
            domainZM.Add("measuBranch", HttpUtility.UrlEncode(this.measuBranch));
            domainZM.Add("measuGroup", HttpUtility.UrlEncode(this.measuGroup));

            domainZM.Add("inoutType", this.inoutType);
            domainZM.Add("comp", this.Comp);
            domainZM.Add("zipfileSize", this.fileSize);
            domainZM.Add("procServer", this.processServer);
            domainZM.Add("ftpServer", this.ftpSendServer);
            domainZM.Add("unzipfileCnt", this.unzipfileCnt);
            domainZM.Add("dupfileCnt", this.dupfileCnt);
            domainZM.Add("completefileCnt", this.completefileCnt);
            domainZM.Add("parsingCompleteCnt", this.parsingCompleteCnt);
            domainZM.Add("curState", this.curState);

            domain.Add("zipfileMain", domainZM);

            if (zipfileDetailList.Count > 0 && bDetail)
            {
                List <Dictionary<string, Object>> lstDomain = new List<Dictionary<string, object>>();

                foreach (ZipFileDetailEntity zd in this.zipfileDetailList)
                {
                    Dictionary<string, Object> domainPart = zd.getDomain();
                    lstDomain.Add(domainPart);                    
                }

                domain.Add("zipfileDetailList", lstDomain);
            }
            
            return domain;            
        }

    }
}
