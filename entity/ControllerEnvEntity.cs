using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqaController.entity
{
    class ControllerEnvEntity
    {
        private string flag = null;
        private string item = null;
        private string info1 = null;
        private string info2 = null;
        private string info3 = null;

        public string Flag { get => flag; set => flag = value; }
        public string Item { get => item; set => item = value; }
        public string Info1 { get => info1; set => info1 = value; }
        public string Info2 { get => info2; set => info2 = value; }
        public string Info3 { get => info3; set => info3 = value; }
    }
}
