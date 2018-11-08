using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;

namespace IqaController.entity
{
    class EventOriFileProcResult
    {
        private string procServer = "";
        private string zipfileNm = "";
        private string ftpServer = "";
        private string flagUnzip = "";
        private string unzipErrLog = "";
        private string flagSendFtp = "";
        private string ftpErrLog = "";
        private string chgCompressFileNm = "";        
        private string isDrmFind = "";
        private string curState = Define.CON_MSG_WAIT;


        public string ProcServer { get => procServer; set => procServer = value; }
        public string ZipfileNm { get => zipfileNm; set => zipfileNm = value; }
        public string FlagUnzip { get => flagUnzip; set => flagUnzip = value; }
        public string UnzipErrLog { get => unzipErrLog; set => unzipErrLog = value; }
        public string FlagSendFtp { get => flagSendFtp; set => flagSendFtp = value; }
        public string FtpErrLog { get => ftpErrLog; set => ftpErrLog = value; }
        public string FtpServer { get => ftpServer; set => ftpServer = value; }
        public string ChgCompressFileNm { get => chgCompressFileNm; set => chgCompressFileNm = value; }
        public string CurState { get => curState; set => curState = value; }
        public string IsDrmFind { get => isDrmFind; set => isDrmFind = value; }

        public Dictionary<string, Object> getDomain()
        {

            Dictionary<string, Object> domain = new Dictionary<string, object>();

            domain.Add("procServer", this.procServer);
            domain.Add("zipfileNm", HttpUtility.UrlEncode(this.zipfileNm));
            domain.Add("ftpServer", this.ftpServer);
            domain.Add("flagUnzip", this.flagUnzip);
            domain.Add("unzipErrLog", HttpUtility.UrlEncode(this.unzipErrLog));
            domain.Add("flagSendFtp", this.flagSendFtp);
            domain.Add("ftpErrLog", HttpUtility.UrlEncode(this.ftpErrLog));
            domain.Add("chgCompressFileNm", this.chgCompressFileNm);
            domain.Add("flagFind", this.isDrmFind);

            return domain;
        }

    }
}
