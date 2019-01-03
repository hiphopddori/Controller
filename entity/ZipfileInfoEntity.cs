using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqaController.entity
{
    class ZipfileInfoEntity
    {
        private string zipfile_nm = "";
        private string cur_state = "";

        public string Zipfile_nm { get => zipfile_nm; set => zipfile_nm = value; }
        public string Cur_state { get => cur_state; set => cur_state = value; }
    }
}
