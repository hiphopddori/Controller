using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.IO; 
using IqaController.entity;

namespace IqaController.service
{
    static class iqaService
    {
        public  static string sendService(Dictionary<string, Object> param,string url)
        {
            string result;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(param);
            try
            {
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(jsonStr);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {                
                util.Log("sendService : url : ", url);
                util.Log("sendService : error", ex.Message.ToString());
                Console.WriteLine(ex.Message.ToString());
                return "NOK";
            }

            return result; 
        }
        /* 컨버터 실행 결과 업데이트
        
        public static SaveResultInfo setUpdateConvetorResult(FileProcess row)
        {
            SaveResultInfo res = null;
            string uri = Define.CON_WEB_SERVICE + "manage/setUpdateConvetorResult2.do";

            Dictionary<string, Object> domain = new Dictionary<string, object>();
            List<Dictionary<string, Object>> lstDomain = new List<Dictionary<string, Object>>();
            foreach (ZipFileDetailEntity zipfileDetail in row.ZipfileDetailList)
            {
                Dictionary<string, Object> domainPart = zipfileDetail.getDomain(); ;
                lstDomain.Add(domainPart);
            }
            
            domain.Add("zipfileNm", HttpUtility.UrlEncode(row.ZipFileName));
            domain.Add("curState", row.CurState);
            domain.Add("zipfileDetailList", lstDomain);

            String result = iqaService.sendService(domain, uri);

            if (result != "NOK")
            {
                res = (SaveResultInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(result, typeof(SaveResultInfo));
            }
            else
            {
                res = new SaveResultInfo();
                res.Flag = 0;
                res.Desc = "컨버트 저장 오류 발생";
            }
            return res;
        }
         */


        /* 컨트롤러 한 Zip 파일 완료 업데이트
         */
        public static SaveResultInfo updateZipFileAllInfo(FileProcess row)
        {
            SaveResultInfo res = null;

            string uri = Define.CON_WEB_SERVICE + "manage/setUpdateZipFileAllInfo.do";
            Dictionary<string, Object> domain = row.getDomain(true);
            //domain.Add("zipfileInfo", domain);
            String result = iqaService.sendService(domain, uri);

            if (result != "NOK")
            {
                res = (SaveResultInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(result, typeof(SaveResultInfo));
            }
            else
            {
                res = new SaveResultInfo();
                res.Flag = 0;
                res.Desc = "zip파일 완료 DB 저장오류 발생";

            }
            return res;
        }

        public static SaveResultInfo updateZipfileMainInfo(FileProcess row, String flag)
        {

            string uri = Define.CON_WEB_SERVICE + "manage/insertZipFileInfo.do";
            Dictionary<string, Object> domain = row.getDomain(false);

            if (flag == Define.con_STATE_START_NAMING)                      
            {
                domain.Add("startTime", "1");                             //처리 시작시간
            }
            else if (flag == Define.con_STATE_START_UNZIP)
            {
                domain.Add("unzipStartTime", "1");                        //UnZip 시작 시점 업데이트
            }
            else if (flag == Define.con_STATE_START_FTP)
            {
                domain.Add("uploadStartTime", "1");                       //업로드 시작 시점 업데이트 - 화면설계상 없어서 사용안함
            }
            else if (flag == Define.con_STATE_START_CONV)
            {
                domain.Add("convStartTime", "1");                       //업로드 시작 시점 업데이트 - 화면설계상 없어서 사용안함
            }

            String result = iqaService.sendService(domain, uri);
            SaveResultInfo res = null;
            if (result != "NOK")
            {
                res = (SaveResultInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(result, typeof(SaveResultInfo));
            }
            else
            {
                res = new SaveResultInfo();
                res.Flag = 0;
                res.Desc = "zIP파일 정보 DB 업데이트 오류";
            }
            return res;
        }

        /*
         */
        public static SaveResultInfo insertZipfileInfos(List<FileProcess> dbInsertList)
        {
            string uri = Define.CON_WEB_SERVICE + "manage/insertZipFileInfos.do";

            Dictionary<string, Object> domain = new Dictionary<string, object>();
            List<Dictionary<string, Object>> lstDomain = new List <Dictionary<string, Object>>();
            foreach (FileProcess row in dbInsertList)
            {
                Dictionary<string, Object> domainPart = row.getDomain(false); ;
                lstDomain.Add(domainPart);
            }

            domain.Add("zipfileInfos", lstDomain);

            String result = iqaService.sendService(domain, uri);
            SaveResultInfo res = null;
            if (result != "NOK")
            {
                res = (SaveResultInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(result, typeof(SaveResultInfo));
            }
            else
            {
                res = new SaveResultInfo();
                res.Flag = 0;
                res.Desc = "통신오류 발생";
            }
            return res;
        }

        public static ControllerServerEntity getFreeFtpServerInfo()
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
            }
            return freeServerInfo;
        }


        public static List<ControllerFileKeepEntity> serviceTest()
        {
            
            string URL = Define.CON_WEB_SERVICE + "manage/csharpTest.do";
            
            List<CodeNameEntity> codeNames = new List<CodeNameEntity>();
            CodeNameEntity test1 = new CodeNameEntity();
            test1.Code = "1";
            test1.Name = "김인철";

            CodeNameEntity test2 = new CodeNameEntity();
            test2.Code = "2";
            test2.Name = "김소연";

            codeNames.Add(test1);
            codeNames.Add(test2);

            Dictionary<string, Object> param = new Dictionary<string, Object>();
            param.Add("zipfileNm", "test");
            param.Add("codes", codeNames);


            


            string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(param);

            List<ControllerFileKeepEntity> lstFile = null;
          
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {              
                streamWriter.Write(jsonStr);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                lstFile = (List<ControllerFileKeepEntity>)Newtonsoft.Json.JsonConvert.DeserializeObject(result, typeof(List<ControllerFileKeepEntity>));
            }

            return lstFile;
        }

        public static List<ControllerFileKeepEntity> getControllerFilePeriod()
        {

            NameValueCollection postData = new NameValueCollection();
            //postData.Add("test", "test");

            List < ControllerFileKeepEntity>lstFile  = null;

            string uri = Define.CON_WEB_SERVICE + "manage/getControllerFilePeriod.do";
            WebClient webClient = new WebClient();
            string pagesource = "";
            try
            {
                pagesource = Encoding.UTF8.GetString(webClient.UploadValues(uri, postData));
                lstFile = (List<ControllerFileKeepEntity>)Newtonsoft.Json.JsonConvert.DeserializeObject(pagesource, typeof(List<ControllerFileKeepEntity>));
            }
            catch (Exception ex)
            {
                util.Log("[ERROR(getControllerFilePeriod.do)]", ex.Message);
                //return false;
            }
            return lstFile;
        }

        public static List<ControllerServerEntity>  getControllerServer()
        {
            string uri = Define.CON_WEB_SERVICE + "manage/getControllerServer.do";
            string pagesource = "";
            WebClient webClient = new WebClient();
            NameValueCollection postData = new NameValueCollection();


            List<ControllerServerEntity> lstServer = null;

            try
            {
                pagesource = Encoding.UTF8.GetString(webClient.UploadValues(uri, postData));
                lstServer = (List<ControllerServerEntity>)Newtonsoft.Json.JsonConvert.DeserializeObject(pagesource, typeof(List<ControllerServerEntity>));
            }
            catch (Exception ex)
            {
                util.Log("[ERROR(getControllerServer.do)]", ex.Message);                
            }
            return lstServer;
        }
    }
}
