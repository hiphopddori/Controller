using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqaController.entity
{
    static class Define
    {

        public const string CON_DEFAULT_DIR = @"D:\FTP";


#if DEBUG
        public const string CON_WEB_SERVICE = "http://localhost:8080/IQA/";
        public const string CON_WEB_SERVICE2 = "http://localhost:8080/IQA/";
        public const string CON_ZIP_EXE_PATH = @"C:\Program Files (x86)\7-Zip\";
#else
        public const string CON_WEB_SERVICE = "http://90.90.30.193:8080/IQA/";
        public const string CON_WEB_SERVICE2 = "http://90.90.30.204:8080/IQA/";
        public const string CON_ZIP_EXE_PATH = @"C:\Program Files\7-Zip\";
#endif



        public const string con_SERVICE_CON_FAIL = "ConnectFail";

        //MSG 상수
        public const string CON_MSG_COMPLETE = "완료";
        public const string CON_MSG_WAIT = "대기";
        public const string CON_MSG_ING = "진행중";
        public const string CON_MSG_ERROR = "Error";
        public const string CON_MSG_CANCEL = "취소";

        //진행단계
        //public const string con_ING_START = "START";
        //public const string con_ING_UNZIP = "UNZIP";
        //public const string con_ING_FTPUPLOAD = "FTPUPLOAD";
        //public const string con_ING_CONV = "CONV";


        public const string con_FTP_QMS = "QMS";
        public const string con_FTP_WAV = "WAV";
        public const string con_FTP_DRM = "DRM";


        public const string con_EVENT_DRM_STATE_DRM = "UZ";         //DRM 파일 추출중        
        public const string con_EVENT_DRM_STATE_FTP = "FTP";        //DRM 파일 추출중
        public const string con_EVENT_DRM_STATE_END = "WORK_END";   //작업 끝

        public const string con_DIR_WORK = "WORK";
        public const string con_DIR_NAMING = "NAMING_ERROR";
        public const string con_DIR_PARSING = "PARSING_ERROR";
        public const string con_DIR_UPLOAD = "UPLOAD_ERROR";
        public const string con_DIR_ZIPDUP = "ZIPDUP_ERROR";
        public const string con_DIR_CONVERT = "CONVERT_ERROR";
        public const string con_DIR_BACK = "BACKUP";
        public const string con_DIR_EVENT_ORIFILE = "EVENT_ORIFILE_WORK";

        public const string con_STATE_START_NAMING = "S_NM";   //네이밍 시작
        public const string con_STATE_START_UNZIP = "S_UZ";    //UNZIP 시작
        public const string con_STATE_START_CONV = "S_CONV";   //CONVERTOR 시작
        public const string con_STATE_START_FTP = "S_FTP";     //FTP 완료  //의미 있나 바로 모두 완료 될텐데


        public const string con_STATE_WAIT = "WAIT";
        public const string con_STATE_WORK = "WORK";
        public const string con_STATE_WORK_COMPLETE = "CP";     //모든 작업 완료
        public const string con_STATE_COMPLETE_NAMING = "NM";   //네이밍 완료
        public const string con_STATE_COMPLETE_UNZIP = "UZ";    //UNZIP 완료
        public const string con_STATE_COMPLETE_CONV = "CONV";   //CONVERTOR 완료
        public const string con_STATE_COMPLETE_FTP = "FTP";   //FTP 완료
        public const string con_STATE_CANCEL = "CANCEL";     //중간 취소

        public const string con_STATE_ERR_NAMING = "E_NM";
        public const string con_STATE_ERR_UNZIP = "E_UZ";
        public const string con_STATE_ERR_CONV = "E_CONV";
        public const string con_STATE_ERR_FTP = "E_FTP";
        public const string con_STATE_ERR_DUP_DRM = "E_DD";     //DRP 중복
        public const string con_STATE_ERR_DUP_ZIP = "E_DZ";     //ZIP파일 중복
        
        
        public const string con_STATE_ERR_DUP_ALL = "E_DD_ALL"; //DRM 모든파일 중복 DRM
        public const string con_STATE_ERR_ZERO_QMS = "E_ZERO_QMS"; //컨버터 실행후 QMS파일 미생성된 오류 
        public const string con_STATE_ERR_ZERO_DRM = "E_ZERO_DRM"; //압축해제후 DRM 파일 미존재
        public const string con_STATE_ERR_NO_FILE = "E_NO_FILE";   //Zip파일 존재 하지 않음(원격 DRM 전송중 최소되는 경우)

        


        public const string con_FOLDER_UPLOAD_ERROR = "UPLOAD_ERROR";


        public const string con_SUCCESS = "1";
        public const string con_FAIL = "0";

        public const int con_KEY_PROCESS_COLIDX = 10;
        public const int con_KEY_PROCESSLOG_COLIDX = 5;
        public const int con_KEY_PROCESSERROR_COLIDX = 6;
        public const int con_KEY_EVENTDRM_COLIDX = 0;

    }
}
