using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqaController.entity
{
    class SendFileInfo
    {
        private string fileName = "";
        private string chgFlieName = "";
        private string flagSend = "";

        public string FileName { get => fileName; set => fileName = value; }
        public string FlagSend { get => flagSend; set => flagSend = value; }
        public string ChgFlieName { get => chgFlieName; set => chgFlieName = value; }

        public SendFileInfo(string fileName, string flagSend, string chgFlieName)
        {
            this.fileName = fileName;
            this.flagSend = flagSend;
            this.chgFlieName = chgFlieName;
        }

        public SendFileInfo()
        {

        }

        public Dictionary<string, Object> getDomain()
        {
            Dictionary<string, Object> domain = new Dictionary<string, object>();
            domain.Add("fileName", HttpUtility.UrlEncode(this.fileName));
            domain.Add("flagSend", this.flagSend);
            return domain;
        }
    }
}
