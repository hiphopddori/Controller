using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqaController.entity
{
    class ControllerSetInfo
    {
        private List<ControllerFileKeepEntity> file = new List<ControllerFileKeepEntity>();
        internal List<ControllerFileKeepEntity> File { get => file; set => file = value; }
    }
}
