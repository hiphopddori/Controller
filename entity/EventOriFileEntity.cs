using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqaController.entity
{
    class EventOriFileEntity
    {

        private string zipfile_nm = "";
        private string qmsfile_nm = "";
        private string flagFind = "";
        
        public string Zipfile_nm { get => zipfile_nm; set => zipfile_nm = value; }
        public string Qmsfile_nm { get => qmsfile_nm; set => qmsfile_nm = value; }
        public string FlagFind { get => flagFind; set => flagFind = value; }


        public Dictionary<string, Object> getDomain(Boolean bDetail)
        {
            Dictionary<string, Object> domain = new Dictionary<string, object>();

            domain.Add("zipfileNm", HttpUtility.UrlEncode(this.zipfile_nm));
            domain.Add("qmsfileNm", HttpUtility.UrlEncode(this.qmsfile_nm));
            domain.Add("flagFind", this.flagFind);

            return domain;
        }


    }
}
