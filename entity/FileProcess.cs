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
        private string uploadDate = "";             // DB 상의 파티션 잡혀있음
        private string measuDate = "";              // Zip파일 측정일 정보 Upload_date에 해당 값 넣는다.
        private string filePos = "";                // 파일
        private string processServer = "";          // 처리서버
        private string measuGroup = "";             // 측정조
        private string measuBranch = "";            // 측정본부
        private string zipFileName = "";            // zip 파일 작업가공 이름 - 뒤에 시스템 날짜 추가됨
        private string orgZipFileName = "";         // zip 파일 이름 원본이름 - 파일 원래 이름 KEY 임(문제되는 이름 가공된 버전) 2018-12-10일 추가 콤마 자동 변환처리 추가        
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
        public string UploadDate { get => uploadDate; set => uploadDate = value; }

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

        public string GetWorkFileFullPathName()
        {
            return this.zipfileFullPath + "\\" + this.zipFileName;
        }

        

        public string GetErr2MovFileDirPath(string path, string dirFlag)
        {
            string fullPathName = "";
            string withoutExtensionZipfileName = Path.GetFileNameWithoutExtension(this.zipFileName);
            fullPathName = path + "\\" + dirFlag + "\\" + withoutExtensionZipfileName;
            return fullPathName;
        }
        /* 시스템 날짜 포함한 ZIP 파일 풀경로
         */
        public string GetZipfileFullPathName(string path, string dirFlag)
        {
            string fullPathName = "";            
            fullPathName = path + "\\" + dirFlag + "\\" + this.ZipFileName;
            return fullPathName;
        }

        public string GetErr2MovFileFullPathName(string path , string dirFlag)
        {
            string fullPathName = "";
            string withoutExtensionZipfileName = Path.GetFileNameWithoutExtension(this.zipFileName);
            fullPathName = path + "\\" + dirFlag + "\\" + withoutExtensionZipfileName + "\\" + this.orgZipFileName;
            return fullPathName;
        }
        /*2018-12-12 작성하다 우선 보류한다.*/
        public string GetConvertorCallServiceType()
        {
            /*              
             O. 서비스망유형 확인하여, 해당 컨버터 명령 실행 시 체크방식
            * DRM파일명을 대문자로 변환하여
            o. CSFB
            1) DRM파일명 상 'CSFB_' 또는 '_CSFB' 또는 ' CSFB ' 또는 '3G음성'
            2) DRM파일명 상 'VOICE_' 또는 '_O_' 또는 '_T_' 또는 ' 발신 ' 또는 ' 착신 ' 이면서, 
            ZIP파일명 상 'CSFB_' 또는 'CSFBML_' 인 경우
            3) 위의 1) 또는 2) 의 해당하지 않는 경우는 ZIP파일명 상의 정보 기준으로 처리 

            o. HDVOICE
            1) DRM파일명 상 'HDV_' 또는 '_HDV' 또는 ' HDV ' 또는 'VOLTE_' 또는 'VOLTE ' 
            2) DRM파일명 상 'VOICE_' 또는 '_O_' 또는 '_T_' 또는 ' 발신 ' 또는 ' 착신 ' 이면서, 
            ZIP파일명 상 'HDV_' 또는 'HDVML_' 인 경우
            3) 위의 1) 또는 2) 의 해당하지 않는 경우는 ZIP파일명 상의 정보 기준으로 처리

            o. HSDPA
            1) DRM파일명 상 'HSDPA_' 또는 ' W DATA ' 
            2) DRM파일명 상 'MC_FTP_' 또는 'FTP_' 이면서, ZIP파일명 상 'HSDPA_' 인 경우
            3) 위의 1) 또는 2) 의 해당하지 않는 경우는 ZIP파일명 상의 정보 기준으로 처리

            o. LTED
            1) DRM파일명 상 'LTED_' 또는 ' LTE DATA '
            2) DRM파일명 상 'MC_FTP_' 또는 'FTP_' 또는 'APP_FTP' 이면서, 
            ZIP파일명 상 'LTED_' 인 경우
            3) 위의 1) 또는 2) 의 해당하지 않는 경우는 ZIP파일명 상의 정보 기준으로 처리 
              
            */
            string zipfileNm = this.orgZipFileName;
            string serviceType = this.serviceType;
            string drmFileNm = "";

            zipfileNm = zipfileNm.ToUpper();


            foreach (ZipFileDetailEntity zd in this.zipfileDetailList)
            {

                drmFileNm = zd.UnzipfileNm;
                drmFileNm = drmFileNm.ToUpper();

                if (drmFileNm.IndexOf("CSFB") >= 0 || drmFileNm.IndexOf("3G음성") >= 0)
                {
                    serviceType = "CSFB";
                }
                else if((zipfileNm.IndexOf("CSFB_") >=0 || zipfileNm.IndexOf("CSFBML_") >=0)  && 
                        (
                            drmFileNm.IndexOf("VOICE_") >=0 || drmFileNm.IndexOf("_O_") >= 0 || drmFileNm.IndexOf("_T_") >= 0) || drmFileNm.IndexOf("발신") >= 0 || drmFileNm.IndexOf("착신") >= 0
                        )
                {
                    serviceType = "CSFB";
                }
            }

            return serviceType;
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
            domainZM.Add("uploadDate", this.uploadDate);                            

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
