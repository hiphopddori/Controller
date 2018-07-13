using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using IqaController.entity;

namespace IqaController.service
{
    static class iqaService
    {
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
