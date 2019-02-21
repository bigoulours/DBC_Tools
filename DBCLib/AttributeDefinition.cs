using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace DBCLib
{
  [DataContract]
  public class AttributeDefinition : Entry
  {
    public static string Symbol = "BA_DEF_";

    [DataMember(EmitDefaultValue = true, Name = "Symbol")]
    private string SymbolDataMember
    {
      get { return AttributeDefinition.Symbol; }
      set { }
    }

    static string intRegexSubstring = @"(INT)\s+" + R.C.uintValue + @"\s+" + R.C.uintValue;
    static string stringRegexSubstring = @"(STRING)";

    static Regex regexFirstLine = new Regex(
      string.Format(@"^{0}\s+(?:(BO_|BU_|SG_)\s+)?{1}\s+(?:{2}|{3})\s*;?$",
        Symbol,
        R.C.quotedStringValue,
        intRegexSubstring,
        stringRegexSubstring
        ),
      RegexOptions.Compiled
      );

    [DataMember(EmitDefaultValue = false)]
    public string ContextNode
    {
      get;
      private set;
    }

    public enum DataTypeEnum
    {
      INT,
      STRING
    }
    [DataMember(EmitDefaultValue = false)]
    public DataTypeEnum DataType
    {
      get;
      private set;
    }

    [DataMember(EmitDefaultValue = false)]
    public object Maximum
    {
      get;
      private set;
    }

    [DataMember(EmitDefaultValue = false)]
    public object Minimum
    {
      get;
      private set;
    }

    [DataMember(EmitDefaultValue = false)]
    public string Name
    {
      get;
      private set;
    }

    [DataMember(EmitDefaultValue = false)]
    IEnumerable<string> Values
    {
      get { return values;}
    }
    List<string> values = null;

    public override string ToString()
    {
      string contextString = "";
      if (ContextNode != null)
      {
        contextString = ContextNode + "|";
      }

      string dataString = "";
      switch (DataType)
      {
        case DataTypeEnum.INT:
          dataString = string.Format("|{0}|{1}", Minimum, Maximum);
          break;
        case DataTypeEnum.STRING:
        default:
          break;
      }

      return string.Format("[{0}] {1}{2}|{3}{4}",
        GetType().Name,
        contextString,
        Name,
        DataType,
        dataString
        );
    }

    public override bool TryParse(ref ParseContext parseContext)
    {
      Match match = Entry.MatchFirstLine(parseContext.line, Symbol, regexFirstLine);
      if (match != null)
      {
        if (match.Groups.Count != 7)
        {
          throw new DataMisalignedException();
        }

        ContextNode = null;
        if (match.Groups[1].Value.Length > 0)
        {
          ContextNode = match.Groups[1].Value;
        }

        Name = StringUtility.DecodeQuotedString(match.Groups[2].Value);

        if (match.Groups[3].Value == "INT")
        {
          DataType = DataTypeEnum.INT;
          Minimum = int.Parse(match.Groups[4].Value);
          Maximum = int.Parse(match.Groups[5].Value);
        }
        else if (match.Groups[6].Value == "STRING")
        {
          DataType = DataTypeEnum.STRING;
        }

        parseContext.line = null;
        if (!parseContext.streamReader.EndOfStream)
        {
          parseContext.line = parseContext.streamReader.ReadLine();
          parseContext.numLines++;
        }

        return true;
      }

      return false;
    }

    public override void WriteDBC(StreamWriter streamWriter)
    {
      string contextString = "";
      if (ContextNode != null)
      {
        contextString = ContextNode + " ";
      }

      string dataString = "";
      switch (DataType)
      {
        case DataTypeEnum.INT:
          dataString = string.Format(" {0} {1}", Minimum, Maximum);
          break;
        case DataTypeEnum.STRING:
        default:
          break;
      }

      streamWriter.WriteLine(string.Format("{0} {1}{2} {3}{4}",
        Symbol,
        contextString,
        StringUtility.EncodeAsQuotedString(Name),
        DataType,
        dataString
        ));
    }
  }
}
