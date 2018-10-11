using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqaController.entity
{
    class CommonResultEntity
    {
        private string flag = Define.con_FAIL;
        private object result = null;

        public string Flag { get => flag; set => flag = value; }
        public object Result { get => result; set => result = value; }
    }
}
