using System;
using System.Collections.Generic;
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



        public string UnzipfileNm { get => unzipfileNm; set => unzipfileNm = value; }
        public string QmsfileNm { get => qmsfileNm; set => qmsfileNm = value; }
        public string FlagConvertorResult { get => flagConvertorResult; set => flagConvertorResult = value; }
        public string DescConvertorResult { get => descConvertorResult; set => descConvertorResult = value; }
        public string QmsfileNmChg { get => qmsfileNmChg; set => qmsfileNmChg = value; }
        public string FlagFtpSend { get => flagFtpSend; set => flagFtpSend = value; }
    }
}
