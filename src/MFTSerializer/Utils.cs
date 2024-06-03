using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace MFTSerializer
{
    public static class ErrorType
    {
        public const string InvalidParameters = "Invalid parameters. Usage example: MftReader.exe C C:/temp/ .txt";
        public const string UnknownException = "An unknown exception occurred. Please try again later.";
    }
    public static class Utils
    {

        public static String ExtractExtension(String fileName)
        {
            int index = fileName.LastIndexOf(".", StringComparison.Ordinal);
            if (index == -1) return null; 
            return fileName.Substring(index, fileName.Length - index);
        }

        public static string GetOwnerName(string path)
        {
            //string user = System.IO.File.GetAccessControl(path).GetOwner(typeof(System.Security.Principal.NTAccount)).ToString();

            FileSecurity fileSecurity = new FileInfo(path).GetAccessControl();
            IdentityReference identityReference = fileSecurity.GetOwner(typeof(SecurityIdentifier));
            NTAccount ntAccount = identityReference.Translate(typeof(NTAccount)) as NTAccount;
            return ntAccount.Value;
        }



        public static void Error(string errorType, string message)
        {
            Console.WriteLine(errorType);
            if (message != null) Console.WriteLine(message);

            Console.WriteLine("\nPress enter to continue...");
            Console.ReadLine();

            System.Environment.Exit(0);
        }

        public static FileNameAndParentFrn SearchId(ulong key, Dictionary<ulong, FileNameAndParentFrn> mDict)
        {

            FileNameAndParentFrn file = null;
            if (mDict.ContainsKey(key))
            {
                file = mDict[key];

                Program.nameLst.Add(file.Name);

                while (file != null)
                {
                    file = SearchId(file.ParentFrn, mDict);
                }
            }


            return file;
        }
        
        public static string GetPath(ulong key, Dictionary<ulong, FileNameAndParentFrn> mDict)
        {
            if (!mDict.ContainsKey(key))
            {
                return null;
            }

            var pathStack = new Stack<string>();
            var file = mDict[key];

            while (file != null)
            {
                pathStack.Push(file.Name);
                if (!mDict.ContainsKey(file.ParentFrn))
                {
                    break;
                }
                file = mDict[file.ParentFrn];
            }
            
            return @"C:\" + string.Join(@"\", pathStack);
        }
        public static Dictionary<String, FileDetails> ConvertFileNameAndParentFrnDictionaryToPathAndDetailsDictionary(Dictionary<ulong, FileNameAndParentFrn> mDict, List<FileNameAndParentFrn> fileNameAndParentFrns = null)
        {
            Dictionary<String, FileDetails> files = new Dictionary<string, FileDetails>();
            if (fileNameAndParentFrns == null)
            {
               fileNameAndParentFrns = mDict.Values.ToList();
            }
            foreach (FileNameAndParentFrn file in fileNameAndParentFrns)
            {
                try
                {
                    string path = GetPath(file.ParentFrn, mDict) + @$"\{file.Name}";
                    files.Add(path, new FileDetails(path));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
                
            }

            return files;
        }
        public static string ConvertFileNameAndParentFrnDictionaryToJSON(Dictionary<ulong, FileNameAndParentFrn> mDict, List<FileNameAndParentFrn> fileNameAndParentFrns = null)
        {
            Dictionary<String, FileDetails> files = new Dictionary<string, FileDetails>();
            if (fileNameAndParentFrns == null)
            {
                fileNameAndParentFrns = mDict.Values.ToList();
            }

            StringBuilder sb = new StringBuilder("{\n");
            foreach (FileNameAndParentFrn file in fileNameAndParentFrns)
            {
                try
                {
                    string path = GetPath(file.ParentFrn, mDict) + @$"\{file.Name}";
                    sb.Append("\t" + new FileDetails(path).ToString() + "\n");
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
                
            }

            sb.Append("}");

            return sb.ToString();
        }

        public static void WriteToFile(String line, String fileNamePath)
        {
            try
            {

                line = line + Environment.NewLine;
                if (!File.Exists(fileNamePath))
                {

                    File.WriteAllText(fileNamePath, line);

                } else File.AppendAllText(fileNamePath, line);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static string GetFQDN()
        {
            string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName();

            domainName = "." + domainName;
            if (!hostName.EndsWith(domainName))
            {
                hostName += domainName;
            }

            return hostName;
        }


        public static string FormatBytesLength(long length)
        {
            int chunk = 1024;
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = (double)length;
            int order = 0;
            while (len >= chunk && order < sizes.Length - 1)
            {
                ++order;
                len = len / chunk;
            }
            string result = String.Format("{0:0.##} {1}", len, sizes[order]);

            return result;
        }

        private static void WriteEventLog(string message)
        {
            try
            {
                string sSource = Constants.APP_SIGNATURE;
                string sLog = "Application";

                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);

                EventLog.WriteEntry(sSource, message, EventLogEntryType.Information);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void LogException(Exception e)
        {
            WriteEventLog("Exception: " + e.Message + " [" + e.StackTrace + "]");
        }

        public static string ByteArrayToMd5HashString(byte[] input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(input);

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }

        public static long GetDriveTotalSize(string driveLetter)
        {
            long totalSize = 0;
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                if(d.Name.ToLower().Equals(driveLetter.ToLower()+":\\"))
                {
                    totalSize = d.TotalSize;
                    break;
                }
            }

            return totalSize;
        }
    }
}

