using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqaController.entity
{
    class ConvertorResult
    {
        private string unzipfileNm = "";
        private string qmsFileNm = "";
        private string flag = ""; 
        private string desc = "";

        public string UnzipfileNm { get => unzipfileNm; set => unzipfileNm = value; }
        public string QmsFileNm { get => qmsFileNm; set => qmsFileNm = value; }
        public string Flag { get => flag; set => flag = value; }
        public string Desc { get => desc; set => desc = value; }
    }
}
