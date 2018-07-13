using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqaController.entity
{
    class QmsFileInfo
    {
        private string fileName = "";
        private string chgFlieName = "";
        private string flagSend = "";

        public string FileName { get => fileName; set => fileName = value; }
        public string FlagSend { get => flagSend; set => flagSend = value; }
        public string ChgFlieName { get => chgFlieName; set => chgFlieName = value; }

        public QmsFileInfo(string fileName , string flagSend, string chgFlieName)
        {
            this.fileName = fileName;
            this.flagSend = flagSend;
            this.chgFlieName = chgFlieName;
        }

    }
}
