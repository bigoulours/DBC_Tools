using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace DBCLib
{
  [DataContract]
  public class BitTiming : Entry
  {
    public static string Symbol = "BS_";

    [DataMember(EmitDefaultValue = true, Name = "Symbol")]
    private string SymbolDataMember
    {
      get { return BitTiming.Symbol; }
      set { }
    }

    static Regex regexFirstLine = new Regex(
      string.Format(@"^{0}:$", Symbol),
      RegexOptions.Compiled
      );

    public override string ToString()
    {
      return string.Format("[{0}]",
        GetType().Name
        );
    }

    public override bool TryParse(ref string line, ref uint numLines, StreamReader streamReader)
    {
      Match match = Entry.MatchFirstLine(line, Symbol, regexFirstLine);
      if (match != null)
      {
        if (match.Groups.Count != 1)
        {
          throw new DataMisalignedException();
        }

        line = null;
        if (!streamReader.EndOfStream)
        {
          line = streamReader.ReadLine();
          numLines++;
        }

        return true;
      }

      return false;
    }

    public override void WriteDBC(StreamWriter streamWriter)
    {
      streamWriter.WriteLine(string.Format("{0}:",
        Symbol
        ));
    }
  }
}
