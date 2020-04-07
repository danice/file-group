using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using ExifLib;

namespace file_group
{
    public class FileGroupService
    {
        public static void DirSearch(string sDir, string toBasePath)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    var dirName = Path.GetFileName(d);
                    DateTime? dirDate = null;
                    DateTime parseDate;
                    if (DateTime.TryParseExact(dirName, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parseDate))
                        dirDate = parseDate;
                    foreach (string f in Directory.GetFiles(d))
                    {
                        var date = DateTaken(f);
                        if (!date.HasValue && !dirDate.HasValue) {
                            Console.WriteLine("could not move: {0}",f);
                            continue;
                        }

                        var to = GetPathFromDate(toBasePath, date ?? dirDate.Value, date.HasValue);                            
                        var filename = GetFilenameFromDate( date ?? dirDate.Value, f);
                        try
                        {                                                    
                            System.IO.Directory.CreateDirectory(to);
                            var destFile = Path.Combine(to, filename);
                            if (File.Exists(destFile)) {
                                Console.WriteLine("Deleted existing: {0} -> {1}",f, destFile);
                                File.Delete(f);
                            }                                
                            else {
                                Console.WriteLine("move from: {0} -> {1}",f, destFile);
                                File.Move(f, destFile);
                            }
                                
                        }
                        catch (System.Exception)
                        {
                            Console.WriteLine("Error in move from: {0} -> {1} {2}", f, to, filename);
                        }
                    }                    
                    DirSearch(d, toBasePath);
                    if (Directory.GetFiles(d).Length == 0 && Directory.GetDirectories(d).Length == 0) 
                    {
                        Directory.Delete(d);
                    }
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        private static string GetPathFromDate(string basePath, DateTime date, bool isPic)
        {                        
            return Path.Combine(basePath, isPic ? "Pic" : "Mov", date.Year.ToString(), GetMonthStr(date));
        }

        private static string GetFilenameFromDate(DateTime date, string f)
        {
            var filename = Path.GetFileName(f);
            var prefix = date.ToString("yyyy-MM-dd");
            var newFilename = filename.StartsWith(prefix) ? filename : string.Format("{0} {1}", prefix, filename);
            
            return newFilename;
        }

        private static string GetMonthStr(DateTime date)
        {
            string monthName = date.ToString("MMM", CultureInfo.InvariantCulture);
            return string.Format("{0:00} {1}", date.Month, monthName);
        }

        static DateTime? DateTaken(string f)
        {
            try
            {
                using (ExifReader reader = new ExifReader(f))
            {
                // Extract the tag data using the ExifTags enumeration
                DateTime datePictureTaken;
                if (reader.GetTagValue<DateTime>(ExifTags.DateTimeDigitized,
                                                out datePictureTaken))
                {
                    // Do whatever is required with the extracted information
                    return datePictureTaken;
                }

            }
            }
            catch (System.Exception)
            {
                
                return null;
            }
            
            return null;
        }


    }
}
