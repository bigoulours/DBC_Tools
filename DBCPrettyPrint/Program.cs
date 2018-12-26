using DBCLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace DBCPrettyPrint
{
  class Program
  {
    static string outputDirectory = "Pretty";
    static bool overwriteExisting = true;

    static void Main(string[] args)
    {
      DBCLib.Reader dbcReader = new Reader();

      if (!Directory.Exists(outputDirectory))
      {
        Directory.CreateDirectory(outputDirectory);
      }

      IEnumerable<string> files = Directory.EnumerateFiles(".", "*.dbc");
      foreach (string fileSrc in files)
      {
        string fileDst = string.Format(@"{0}\{1}", outputDirectory, Path.GetFileName(fileSrc));
        if (overwriteExisting || !File.Exists(fileDst) || (new System.IO.FileInfo(fileDst).Length == 0))
        {
          Console.Title = fileSrc;
          Console.WriteLine("R {0}", fileSrc);
          List<object> entries = dbcReader.Read(fileSrc);

          if (entries != null)
          {
            Console.WriteLine("W {0}", fileDst);
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            try
            {
              using (Stream streamDst = new FileStream(fileDst, FileMode.Create, FileAccess.Write, FileShare.Read))
              {
                using (StreamWriter streamWriter = new StreamWriter(streamDst, Encoding.Default))
                {
                  foreach (object entry in entries)
                  {
                    ((Entry)entry).WriteDBC(streamWriter);
                  }
                }
              }
            }
            finally
            {
              Thread.CurrentThread.CurrentCulture = currentCulture;
            }
          }
        }
      }
    }
  }
}
