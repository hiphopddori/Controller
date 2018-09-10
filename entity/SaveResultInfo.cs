using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqaController.entity
{
    class SaveResultInfo
    {
        private int flag = 1;
        private string desc = "";

        
        public string Desc { get => desc; set => desc = value; }
        public int Flag { get => flag; set => flag = value; }
    }
}
