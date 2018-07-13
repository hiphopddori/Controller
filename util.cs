using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace IqaController
{
    class util
    {

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal,
                                                        int size, string filePath);

        public static void Log(string flag , string logMessage)
        {
            try{
                //w.WriteLine("{0} : {1} {2} : {3}", flag, DateTime.Now.ToLongTimeString(),
                //DateTime.Now.ToLongDateString(), logMessage);


                string cur = DateTime.Now.ToString("yyyyMMddHH");

                DirectoryInfo diChk = new DirectoryInfo(".\\log\\");
                if (diChk.Exists == false)
                {
                    diChk.Create();
                }

                System.IO.File.AppendAllText(".\\log\\" + cur +".txt", String.Format("\n{0} : {1} {2} : {3}", flag, DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString(), logMessage));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }            
        }

        public static void SetIniWriteString(string section, string key, string val, string filePath)
        {
            WritePrivateProfileString(section, key, val, filePath);
        }

        public static void GetIniString(string section, string key, string def, StringBuilder retVal, int size, string filePath)
        {
            GetPrivateProfileString(section, key, def, retVal, size, filePath);
        }
    }
}
