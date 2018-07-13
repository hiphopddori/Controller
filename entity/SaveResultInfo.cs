using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqaController.entity
{
    class SaveResultInfo
    {
        private string flag = "";
        private string desc = "";

        public string Flag { get => flag; set => flag = value; }
        public string Desc { get => desc; set => desc = value; }
    }
}
