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
        public const string CON_WEB_SERVICE = "http://localhost:8080/IQA/";
        // private const string CON_ZIP_EXE_PATH = @"C:\Program Files (x86)\7-Zip\7z.exe";
        public const string CON_ZIP_EXE_PATH = @"C:\Program Files (x86)\7-Zip\";

        public const string CON_COMPLETE = "완료";
        public const string CON_WAIT = "대기";
        public const string CON_ING = "진행중";
        public const string CON_ERROR = "Error";

        //진행단계
        public const string con_ING_START = "START";
        public const string con_ING_UNZIP = "UNZIP";
        public const string con_ING_FTPUPLOAD = "FTPUPLOAD";



        public const string con_DIR_WORK = "WORK";
        public const string con_DIR_NAMING = "NAMING_ERROR";
        public const string con_DIR_PARSING = "PARSING_ERROR";
        public const string con_DIR_UPLOAD = "UPLOAD_ERROR";
        public const string con_DIR_ZIPDUP = "ZIPDUP_ERROR";
        public const string con_DIR_CONVERT = "CONVERT_ERROR";
        public const string con_DIR_BACK = "BACK";




        public const string con_STATE_WAIT = "WAIT";
        public const string con_STATE_WORK = "WORK";
        public const string con_STATE_WORK_COMPLETE = "CP";     //모든 작업 완료
        public const string con_STATE_COMPLETE_NAMING = "NM";   //네이밍 완료
        public const string con_STATE_COMPLETE_UNZIP = "UZ";    //UNZIP 완료
        public const string con_STATE_COMPLETE_CONV = "CONV";   //CONVERTOR 완료
        public const string con_STATE_COMPLETE_FTP = "FTP";   //FTP 완료  //의미 있나 바로 모두 완료 될텐데

        public const string con_STATE_ERR_NAMING = "E_NM";
        public const string con_STATE_ERR_UNZIP = "E_UZ";       
        public const string con_STATE_ERR_DUP_DRM = "E_DD";     //DRP 중복
        public const string con_STATE_ERR_DUP_ZIP = "E_DZ";     //ZIP파일 중복


        public const string con_FOLDER_UPLOAD_ERROR = "UPLOAD_ERROR";


        public const string con_SUCCESS = "1";
        public const string con_FAIL = "0";

    }
}
