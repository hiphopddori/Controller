using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

                DirectoryInfo diChk = new DirectoryInfo(".\\log\\");
                if (diChk.Exists == false)
                {
                    diChk.Create();
                }

                string cur = DateTime.Now.ToString("yyyyMMdd");
                System.IO.File.AppendAllText(".\\log\\" + cur +".txt", String.Format("[{0}] : [{1} {2}] : [{3}]", flag, DateTime.Now.ToLongDateString()
                        , DateTime.Now.ToLongTimeString(), logMessage) + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }            
        }

        public static void LogDrm(string flag, string logMessage)
        {
            try
            {
                //w.WriteLine("{0} : {1} {2} : {3}", flag, DateTime.Now.ToLongTimeString(),
                //DateTime.Now.ToLongDateString(), logMessage);

                DirectoryInfo diChk = new DirectoryInfo(".\\log\\drm\\");
                if (diChk.Exists == false)
                {
                    diChk.Create();
                }

                string cur = DateTime.Now.ToString("yyyyMMdd");
                System.IO.File.AppendAllText(".\\log\\drm\\" + cur + ".txt", String.Format("[{0}] : [{1} {2}] : [{3}]", flag, DateTime.Now.ToLongDateString()
                        , DateTime.Now.ToLongTimeString(), logMessage) + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static Boolean ParseDate(string sFileDate , ref DateTime date)
        {
            Boolean bSuccess = false;

            try{
                date = DateTime.ParseExact(sFileDate, "yyyyMMdd", null);
                bSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                bSuccess = false;
            }

            return bSuccess;

        }

        public static void SetIniWriteString(string section, string key, string val, string filePath)
        {
            WritePrivateProfileString(section, key, val, filePath);
        }

        public static void GetIniString(string section, string key, string def, StringBuilder retVal, int size, string filePath)
        {
            GetPrivateProfileString(section, key, def, retVal, size, filePath);
        }

        public static Boolean AllDeleteInDirectory(string dirPath)
        {            
            try
            {
                //읽기 전용 삭제 하기 위해 변경한다.
                DirectoryInfo dir = new DirectoryInfo(dirPath);
                System.IO.FileInfo[] files = dir.GetFiles("*.*", SearchOption.AllDirectories);
                foreach (System.IO.FileInfo file in files)
                {
                    file.Attributes = FileAttributes.Normal;
                }

                //Directory.Delete(dirPath, true);
                //파일 삭제
                string[] filePaths = Directory.GetFiles(dirPath);
                foreach (string filePath in filePaths)
                {
                    FileInfo fi = new FileInfo(filePath);
                    fi.Delete();
                }

                string[] dirs = Directory.GetDirectories(dirPath);

                foreach (string subDirPath in dirs)
                {
                    Directory.Delete(subDirPath, true);
                }

                return true;
            }
            catch (Exception ex)
            {
                util.Log("ERR:directoryDelete", ex.Message);
                Console.WriteLine(ex.Message.ToString());
                return false;
            }
        }
        //취소 되는경우 발생하는듯 함
        public static Boolean Wait2FileAccessible(string fullPath,int maxMin)
        {
            while (true)
            {
                try
                {
                    
                    //복사중일경우 Exception 발생한다.
                    using (var file = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        break;
                    }
                }
                catch (IOException ex)
                {
                    //파일을 찾을수 없을경우 즉 FTP전송되다 취소될경우 
                    if (ex.HResult ==  -2147024894)
                    {
                        return false;
                    }

                    Thread.Sleep(1000);
                }
            }

            //추후 maxMin 처리하자
            return true;
        }
        
        public static long CalculateDirectorySize(DirectoryInfo directory, bool includeSubdirectories)
        {
            long totalSize = 0;

            FileInfo[] files = directory.GetFiles();
            foreach (FileInfo file in files)
            {
                totalSize += file.Length;
            }

            if (includeSubdirectories)
            {
                DirectoryInfo[] dirs = directory.GetDirectories();
                foreach (DirectoryInfo dir in dirs)
                {
                    totalSize += CalculateDirectorySize(dir, true);
                }
            }

            return totalSize;
        }

        public static string GetCurrentDate(string format)
        {
            DateTime dt = DateTime.Now;
            string curDate = String.Format(format, dt);
            return curDate;
        }

        public static Boolean FileSave(string flag, string sourceFileName, string destiFileName)
        {
            int nMax = 2000;
            int nAttemptCnt = 0;
            Boolean bDo = true;
            Boolean bSuccess = false;

            while (bDo)
            {
                try
                {
                    if (flag == "copy")
                    {
                        File.Copy(sourceFileName, destiFileName, true);
                    }
                    else
                    {
                        File.Move(sourceFileName, destiFileName);
                    }

                    bDo = false;
                    bSuccess = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                    if (ex.HResult == -2147024864)
                    {
                        Thread.Sleep(1500);
                        nAttemptCnt++;


                        if (nAttemptCnt > nMax)
                        {
                            bDo = false;
                            util.Log("[ERR]:fileSave", "파일권한 무한 대기 발생");
                        }
                    }
                    else
                    {
                        util.Log("[ERR]:fileSave", ex.Message);
                        bDo = false;
                    }
                }
            }

            return bSuccess;
        }
    }
}
