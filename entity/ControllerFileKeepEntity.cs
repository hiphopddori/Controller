using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqaController.entity
{
    class ControllerFileKeepEntity
    {
        private string item = "";
        private string dirPath = "";
        private string period = "";

        public string Period { get => period; set => period = value; }
        public string Item { get => item; set => item = value; }
        public string DirPath { get => dirPath; set => dirPath = value; }
        
    }
}
