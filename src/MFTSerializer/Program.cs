using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using System.Linq;


namespace MFTSerializer
{
    public sealed class Settings
    {
        public static string search_volume { get; set; }
        public static string report_folder { get; set; }
        public static string[] search_extensions { get; set; }
        public static bool calc_md5 { get; set; }
    }
    class Program
    {

        public static List<String> nameLst = null;
        public static List<String> finalNameLst = null;
        private static Settings GetSettings()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            return config.GetRequiredSection("Settings").Get<Settings>();
        }
        static void Main(string[] args)
        {
            try
            {
                Settings? settings = GetSettings();

                Console.WriteLine("### "+Constants.APP_SIGNATURE+" ###");
                Console.WriteLine("### " + Constants.APP_URL + " ###\n");
                Console.Write("Processing...\r");

                Int32 unixTimestampInit = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                string driveLetter = Settings.search_volume;
                string fileNamePath = Settings.report_folder;
                List<String> fileExtensions = Settings.search_extensions.ToList();
                string strFileExtensions = String.Join(" ", fileExtensions);
                bool calcMd5 = Settings.calc_md5;

                string fileNamePathRecommendation = "report_folder (" + fileNamePath + ") must be a valid folder, and the current user must have write access in it. The valid slash must be / and NOT \\.";

                if (!Directory.Exists(fileNamePath))
                {
                    Utils.Error(ErrorType.InvalidParameters, fileNamePathRecommendation);
                }
                else if (fileNamePath[fileNamePath.Length - 1] == '/') fileNamePath = fileNamePath.Substring(0, fileNamePath.Length - 1);
                

                if (driveLetter.Length > 1 || driveLetter.Contains(":") || fileNamePath.Contains("\\") || !fileExtensions.Any(extension => extension.Contains(".")) || fileExtensions.Any(extension => extension.Contains("*")))
                {
                    Utils.Error(ErrorType.InvalidParameters, "\n\nCheck the config file:\n\n1. search_volume (" + driveLetter+") must be JUST a NTFS volume/drive letter WITHOUT ':', like C, D, F, G, etc. The current user must have administration rights over this volume.\n\n" +
                        "2. "+fileNamePathRecommendation+"\n\n" +
                        "3. search_extensions (" + strFileExtensions+ ") is the representation of a file extension, like .txt, .pdf, .doc, etc, WITH dot (.) WITHOUT asterisk (*).");
                }


                nameLst = new List<string>();
                Dictionary<ulong, FileNameAndParentFrn> mDict = new Dictionary<ulong, FileNameAndParentFrn>();
                
                EnumerateVolume.PInvokeWin32 mft = new EnumerateVolume.PInvokeWin32();
                mft.Drive = driveLetter;
                mft.Drive = mft.Drive + ":";
                mft.EnumerateVolume(out mDict);
                String jsonFileNamePath = fileNamePath + "/" + driveLetter + "." + unixTimestampInit + ".json";

                Console.Write("Volume: " + driveLetter+"\t\t\n");
                Console.WriteLine("Report folder: " + fileNamePath);
                Console.WriteLine("Extension(s): " + strFileExtensions);

                finalNameLst = new List<string>();
                Console.WriteLine("MFT items: " + mDict.Count);
                List<FileNameAndParentFrn>
                    find = mDict.Values.ToList().FindAll(x => x.Name.ToLower().Contains("Hello".ToLower()));
                string dict = 
                    Utils.ConvertFileNameAndParentFrnDictionaryToJSON(mDict, find);
                Console.WriteLine(dict);

            }
            catch (Exception e)
            {
                Utils.Error(ErrorType.UnknownException, e.Message);
                Utils.LogException(e);
            }

}
    }
        


}
