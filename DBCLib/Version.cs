using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace DBCLib
{
  [DataContract]
  public class Version : Entry
  {
    public static string Symbol = "VERSION";

    [DataMember(EmitDefaultValue = true, Name = "Symbol")]
    private string SymbolDataMember
    {
      get { return Version.Symbol; }
      set { }
    }

    static Regex regexFirstLine = new Regex(
      string.Format(@"^{0}\s+{1}$",
        Symbol,
        R.C.quotedStringValue
        ),
      RegexOptions.Compiled
      );

    [DataMember(EmitDefaultValue = false)]
    public string Text
    {
      get;
      private set;
    }

    public override string ToString()
    {
      return string.Format("[{0}] {1}",
        GetType().Name,
        Text
        );
    }

    public override bool TryParse(ref string line, ref uint numLines, StreamReader streamReader)
    {
      Match match = Entry.MatchFirstLine(line, Symbol, regexFirstLine);
      if (match != null)
      {
        if (match.Groups.Count != 2)
        {
          throw new DataMisalignedException();
        }

        Text = StringUtility.SimplifyEmptyToNull(StringUtility.DecodeQuotedString(match.Groups[1].Value));

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
      streamWriter.WriteLine(string.Format(@"{0} ""{1}""",
        Symbol,
        Text
        ));
    }
  }
}
