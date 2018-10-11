using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqaController.entity
{
    class EventOriFileEntity
    {

        private string zipfile_nm = "";
        private string qmsfile_nm = "";
        
        public string Zipfile_nm { get => zipfile_nm; set => zipfile_nm = value; }
        public string Qmsfile_nm { get => qmsfile_nm; set => qmsfile_nm = value; }
    }
}
