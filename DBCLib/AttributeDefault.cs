using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace DBCLib
{
  [DataContract]
  public class AttributeDefault : Entry
  {
    public static string Symbol = "BA_DEF_DEF_";

    [DataMember(EmitDefaultValue = true, Name = "Symbol")]
    private string SymbolDataMember
    {
      get { return AttributeDefault.Symbol; }
      set { }
    }

    static Regex regexFirstLine = new Regex(
      string.Format(@"^{0}\s+{1}\s+(?:{2}|{3});?$",
        Symbol,
        R.C.quotedStringValue,
        R.C.intValue,
        R.C.quotedStringValue
        ),
      RegexOptions.Compiled
      );

    [DataMember(EmitDefaultValue = false)]
    public string Name
    {
      get;
      private set;
    }

    [DataMember(EmitDefaultValue = false)]
    public object Value
    {
      get;
      private set;
    }

    public override string ToString()
    {
      return string.Format("[{0}] {1}|{2}",
        GetType().Name,
        Name,
        Value
        );
    }

    public override bool TryParse(ref string line, ref uint numLines, StreamReader streamReader)
    {
      Match match = Entry.MatchFirstLine(line, Symbol, regexFirstLine);
      if (match != null)
      {
        if (match.Groups.Count != 4)
        {
          throw new DataMisalignedException();
        }

        Name = StringUtility.DecodeQuotedString(match.Groups[1].Value);

        if (match.Groups[2].Value.Length > 0)
        {
          Value = int.Parse(match.Groups[2].Value);
        }
        if (match.Groups[3].Value.Length > 0)
        {
          Value = StringUtility.DecodeQuotedString(match.Groups[3].Value);
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
      streamWriter.WriteLine(string.Format("{0} {1} {2}",
        Symbol,
        StringUtility.EncodeAsQuotedString(Name),
        (Value is string) ? StringUtility.EncodeAsQuotedString(Value as string) : Value
        ));
    }
  }
}
