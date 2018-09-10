using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqaController.entity
{
    class ZipFileDetailEntity
    {
        
        private string unzipfileNm = "";
        private string qmsfileNm = "";
        private string qmsfileNmChg = "";
        private string flagConvertorResult = "";
        private string descConvertorResult = "";
        private string flagFtpSend = "";
        private string isWaveFile = "0";
        private List<SendFileInfo> waveFileInfo = new List<SendFileInfo>();


        public string UnzipfileNm { get => unzipfileNm; set => unzipfileNm = value; }
        public string QmsfileNm { get => qmsfileNm; set => qmsfileNm = value; }
        public string FlagConvertorResult { get => flagConvertorResult; set => flagConvertorResult = value; }
        public string DescConvertorResult { get => descConvertorResult; set => descConvertorResult = value; }
        public string QmsfileNmChg { get => qmsfileNmChg; set => qmsfileNmChg = value; }
        public string FlagFtpSend { get => flagFtpSend; set => flagFtpSend = value; }
        public string IsWaveFile { get => isWaveFile; set => isWaveFile = value; }
        internal List<SendFileInfo> WaveFileInfo { get => waveFileInfo; set => waveFileInfo = value; }

        public Dictionary<string, Object> getDomain()
        {

            Dictionary<string, Object> domain = new Dictionary<string, object>();

            domain.Add("unzipfileNm", HttpUtility.UrlEncode(this.unzipfileNm));
            domain.Add("qmsfileNm", HttpUtility.UrlEncode(this.qmsfileNm));            
            domain.Add("qmsfileNmChg", this.qmsfileNmChg);

            domain.Add("flag", this.flagConvertorResult);
            domain.Add("desc", HttpUtility.UrlEncode(this.descConvertorResult));
            domain.Add("flagFtpSend", this.flagFtpSend);
            domain.Add("isWaveFile", this.isWaveFile);


            if (waveFileInfo.Count > 0)
            {
                List<Dictionary<string, Object>> lstDomain = new List<Dictionary<string, object>>();

                foreach (SendFileInfo wf in this.waveFileInfo)
                {
                    Dictionary<string, Object> domainPart = wf.getDomain();
                    lstDomain.Add(domainPart);
                }

                domain.Add("waveFileInfo", lstDomain);
            }
            return domain;

        }


    }
}
