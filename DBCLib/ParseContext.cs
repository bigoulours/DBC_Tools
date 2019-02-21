using System.Collections.Generic;
using System.IO;

namespace DBCLib
{
  public class ParseContext
  {
    public StreamReader streamReader;
    public string line;
    public uint numLines;

    public ParseContext(StreamReader streamReader)
    {
      this.streamReader = streamReader;
      line = null;
      numLines = 0;
    }
  }
}
