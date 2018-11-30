using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.IO;
using System.Threading;
using IqaController.entity;

namespace IqaController.service
{
    static class iqaService
    {

        public static frmMain mainForm = null;

        /* 서비스 공통 
        */
        public static string serviceCall(Dictionary<string, Object> param, string url)
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

                return result;
            }
            catch (WebException ex)
            {                
                Boolean bConnectFail = false;
                var status = ex.Status;
                if (status != null)
                {
                    //원격서버 죽어있음(배포중) - 재기동까지 대기 모드?
                    if (status == WebExceptionStatus.ConnectFailure)
                    {
                        bConnectFail = true;
                    }else if (status == WebExceptionStatus.ReceiveFailure)
                    {
                        bConnectFail = true;
                    }
                }

                if (bConnectFail)
                {
                    //재호출 한다.
                    return Define.con_SERVICE_CON_FAIL;
                }
                else
                {
                    string errDesc = ex.Message.ToString();
                    util.Log("sendService : error url = ", url);
                    util.Log("sendService : error desc = ", errDesc);
                    Console.WriteLine(errDesc);

                    return "NOK::" + ex.Message.ToString();
                }
            }
            catch (Exception ex)
            {
                string errDesc = ex.Message.ToString();
                util.Log("sendService : error url = ", url);
                util.Log("sendService : error desc = ", errDesc);
                Console.WriteLine(errDesc);
                return "NOK::" + ex.Message.ToString();
            }
        }

        public  static string sendService(Dictionary<string, Object> param ,string url , Boolean bWait = true)
        {
            //톰캣 배포시 끊기는 현상떄문에 오류 막기위해 재전송처리

            mainForm.loadingOn(url);
            Boolean bReSend = false;
            string result = "start";
            while ((result == Define.con_SERVICE_CON_FAIL  && bWait)  || result =="start")
            {
                if (bReSend)
                {                    
                    mainForm.loadingOn("[재시도중..]:" + url);
                }

                result = iqaService.serviceCall(param, url);

                if (result == "ConectFail" && bWait)
                {
                    bReSend = true;
                    util.Log("sendService : error url = ", url);
                    util.Log("sendService : error desc = ", "ConnectFail로 인한 server start wait...");
                    Console.WriteLine("ConnectFail로 인한 server start wait...");
                    Thread.Sleep(2000);
                }
            }

            mainForm.loadingOff();

            return result;
        }
      
        /* 컨트롤러 한 Zip 파일 완료 업데이트
         */
        public static SaveResultInfo updateZipFileAllInfo(FileProcess row)
        {
            SaveResultInfo res = null;

            string uri = Define.CON_WEB_SERVICE + "manage/setUpdateZipFileAllInfo.do";
            Dictionary<string, Object> domain = row.getDomain(true);
            //domain.Add("zipfileInfo", domain);
            String result = iqaService.sendService(domain, uri);

            if (result.IndexOf("NOK") < 0)
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

        public static SaveResultInfo updateEventOrifileSendResult(EventOriFileProcResult eventOrifileProc , List<EventOriFileEntity> drmFileResults)
        {

            string uri = Define.CON_WEB_SERVICE + "manage/setUpdateEventOrifileSendResult.do";
            Dictionary<string, Object> domain = eventOrifileProc.getDomain();
            

            List<Dictionary<string, Object>> lstDomain = new List<Dictionary<string, object>>();
            foreach (EventOriFileEntity drmResult in drmFileResults)
            {
                lstDomain.Add(drmResult.getDomain());
            }

            //domain.Add("drmFileResult", drmFileResults);
            domain.Add("drmFileResult", lstDomain);

            String result = iqaService.sendService(domain, uri);
            SaveResultInfo res = null;
            if (result.IndexOf("NOK") < 0)
            {
                res = (SaveResultInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(result, typeof(SaveResultInfo));
            }
            else
            {
                res = new SaveResultInfo();
                res.Flag = 0;
                res.Desc = "Event Drm File Ftp Send Error";
            }
            return res;


        }

        public static SaveResultInfo updateZipfileMainInfo(FileProcess row, String flag)
        {

            string uri = Define.CON_WEB_SERVICE + "manage/insertZipFileInfo.do";
            Dictionary<string, Object> domain = row.getDomain(false);            
            Dictionary<string, Object> domainZipMain = (Dictionary<string, Object>)domain["zipfileMain"];

            if (flag == Define.con_STATE_START_NAMING)                      
            {
                domainZipMain.Add("startTime", "Y");                             //처리 시작시간
            }
            else if (flag == Define.con_STATE_START_UNZIP)
            {
                domainZipMain.Add("unzipStartTime", "Y");                        //UnZip 시작 시점 업데이트
            }
            else if (flag == Define.con_STATE_START_FTP)
            {
                domainZipMain.Add("uploadStartTime", "Y");                       //업로드 시작 시점 업데이트 - 화면설계상 없어서 사용안함
            }
            else if (flag == Define.con_STATE_START_CONV)
            {
                domainZipMain.Add("convStartTime", "Y");                       //업로드 시작 시점 업데이트 - 화면설계상 없어서 사용안함
            }

            String result = iqaService.sendService(domain, uri);
            SaveResultInfo res = null;
            if (result.IndexOf("NOK") < 0)
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
        /* unzip 파일정보를 업데이트 하고 중복 drm 정보를 얻는다.
         */
        public static CommonResultEntity setUnzipFileInfoUpdateAndGetDupInfo(FileProcess row)
        {

            string uri = Define.CON_WEB_SERVICE + "manage/setUnzipFileInfoUpdateAndGetDupInfo.do";
            
            Dictionary<string, Object> domain = row.getDomain(true);

            CommonResultEntity output = new CommonResultEntity();
            List<CodeNameEntity> result = null;

            String jsonResult = iqaService.sendService(domain, uri);

            if (jsonResult.IndexOf("NOK") < 0)
            {
                result = (List<CodeNameEntity>)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResult, typeof(List<CodeNameEntity>));
                
                output.Flag = Define.con_SUCCESS;
                output.Result = result;
            }
            else
            {
                string errDesc = jsonResult.Split(':')[1];            
                 output.Result = errDesc;
            }

            return output;
        }
        
        /* 전송해야할 Event Drm 파일정보를 가져온다. - FTP 전송을 위해서
         * 
        */
        public static CommonResultEntity getEventOrifileList(string procServer)
        {
            string uri = Define.CON_WEB_SERVICE + "manage/getEventOriFileList.do";
            Dictionary<string, Object> domain = new Dictionary<string, object>();
            
            domain.Add("isSingleZipFile", "1");
            domain.Add("procServer", procServer);

            String jsonResult = iqaService.sendService(domain, uri);

            CommonResultEntity output = new CommonResultEntity();

            List <EventOriFileEntity> result = null;
            if (jsonResult.IndexOf("NOK") < 0)
            {
                result = (List<EventOriFileEntity>)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResult, typeof(List<EventOriFileEntity>));
                output.Flag = Define.con_SUCCESS;
                output.Result = result;
            }
            else
            {                
                string errDesc = jsonResult.Split(':')[1];
                //util.Log("[ERR]:getEventOrifileList", errDesc);
                output.Result = errDesc;
            }

            return output;
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
            if (result.IndexOf("NOK") < 0)
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

        public static CommonResultEntity getControllerFilePeriod(bool bWait)
        {
            
            string uri = Define.CON_WEB_SERVICE + "manage/getControllerFilePeriod.do";
            Dictionary<string, Object> domain = new Dictionary<string, object>();
            String jsonResult = iqaService.sendService(domain, uri, bWait);

            if (jsonResult == Define.con_SERVICE_CON_FAIL)
            {
                return null;
            }

            CommonResultEntity output = new CommonResultEntity();

            List<ControllerFileKeepEntity> result = null;
            if (jsonResult.IndexOf("NOK") < 0)
            {
                result = (List<ControllerFileKeepEntity>)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResult, typeof(List<ControllerFileKeepEntity>));
                output.Flag = Define.con_SUCCESS;
                output.Result = result;
            }
            else
            {
                string errDesc = jsonResult.Split(':')[1];
                //util.Log("[ERR]:getEventOrifileList", errDesc);
                output.Result = errDesc;
            }

            return output;

        }

        public static CommonResultEntity getControllerServer()
        {
           
            string uri = Define.CON_WEB_SERVICE + "manage/getControllerServer.do";
            Dictionary<string, Object> domain = new Dictionary<string, object>();
            String jsonResult = iqaService.sendService(domain, uri);

            CommonResultEntity output = new CommonResultEntity();

            List<ControllerServerEntity> result = null;
            if (jsonResult.IndexOf("NOK") < 0)
            {
                result = (List<ControllerServerEntity>)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResult, typeof(List<ControllerServerEntity>));
                output.Flag = Define.con_SUCCESS;
                output.Result = result;
            }
            else
            {
                string errDesc = jsonResult.Split(':')[1];                
                output.Result = errDesc;
            }
            return output;
        }
    }
}
