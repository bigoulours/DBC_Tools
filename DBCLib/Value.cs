using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace DBCLib
{
  [DataContract]
  public class Value : Entry
  {
    public static string Symbol = "VAL_";

    [DataMember(EmitDefaultValue = true, Name = "Symbol")]
    private string SymbolDataMember
    {
      get { return Value.Symbol; }
      set { }
    }

    static Regex regexFirstLine = new Regex(
      string.Format(@"^\s*{0}\s+{1}\s+{2}(?:\s+{3}\s+{4})+\s*;?$",
        Symbol,
        R.C.uintValue,
        R.C.signalName,
        R.C.intValue,
        R.C.quotedStringValue
        ),
      RegexOptions.Compiled
      );

    [DataMember(EmitDefaultValue = false)]
    public uint? ContextMessageId
    {
      get;
      private set;
    }

    [DataMember(EmitDefaultValue = false)]
    public string ContextSignalName
    {
      get;
      private set;
    }

    [DataMember(EmitDefaultValue = false)]
    public IEnumerable<KeyValuePair<int, string>> Mapping
    {
      get { return mapping; }
    }
    List<KeyValuePair<int, string>> mapping = new List<KeyValuePair<int, string>>();

    public override string ToString()
    {
      string mappingString = "";
      foreach (KeyValuePair<int, string> pair in Mapping)
      {
        mappingString += string.Format("|{0}|{1}", pair.Key, pair.Value);
      }
      return string.Format("[{0}] {1}|{2}{3}",
        GetType().Name,
        ContextMessageId,
        ContextSignalName,
        mappingString
        );
    }

    public override bool TryParse(ref string line, ref uint numLines, StreamReader streamReader)
    {
      Match match = Entry.MatchFirstLine(line, Symbol, regexFirstLine);
      if (match != null)
      {
        if (match.Groups.Count != 5)
        {
          throw new DataMisalignedException();
        }

        ContextMessageId = uint.Parse(match.Groups[1].Value);
        ContextSignalName = match.Groups[2].Value;

        for (int i = 0; i < match.Groups[3].Captures.Count; i++)
        {
          KeyValuePair<int, string> pair = new KeyValuePair<int, string>(
            int.Parse(match.Groups[3].Captures[i].Value),
            StringUtility.DecodeQuotedString(match.Groups[4].Captures[i].Value)
            );
          mapping.Add(pair);
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
      string mappingString = "";
      foreach (KeyValuePair<int, string> pair in Mapping)
      {
        mappingString += string.Format(@" {0} {1}",
          pair.Key,
          (pair.Value is string) ? StringUtility.EncodeAsQuotedString(pair.Value) : pair.Value
          );
      }

      streamWriter.WriteLine(string.Format("{0} {1} {2}{3}",
        Symbol,
        ContextMessageId,
        ContextSignalName,
        mappingString
        ));
    }
  }
}
