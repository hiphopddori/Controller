using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqaController.entity
{
    class ControllerServerEntity
    {
        private string name = null;
        private string ip = null;
        private string id = null;
        private string password = null;
        private string path = null;
        private string port = null;

        public string Name { get => name; set => name = value; }
        public string Ip { get => ip; set => ip = value; }
        public string Id { get => id; set => id = value; }
        public string Password { get => password; set => password = value; }
        public string Path { get => path; set => path = value; }
        public string Port { get => port; set => port = value; }
    }
}
