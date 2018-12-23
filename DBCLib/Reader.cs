using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DBCLib
{
  public class Reader
  {
    public bool AllowErrors
    {
      get;
      private set;
    } = false;

    public bool DisplayEntries
    {
      get;
      private set;
    } = false;

    bool TryParse<T>(
      ref string line,
      ref uint numLines,
      StreamReader streamReader,
      List<object> entries)
      where T : Entry, new()
    {
      T t = new T();
      if (t.TryParse(ref line, ref numLines, streamReader))
      {
        entries.Add(t);
        return true;
      }

      return false;
    }

    public List<object> Read(
      string path
      )
    {
      using (StreamReader streamReader = new StreamReader(path, Encoding.Default, false))
      {
        return Read(streamReader, path);
      }
    }

    public List<object> Read(
      StreamReader streamReader,
      string path
      )
    {
      List<object> entries = new List<object>();

      bool errors = false;
      bool exceptionThrown = false;

      uint numLines = 0;
      if (!streamReader.EndOfStream)
      {
        string line = streamReader.ReadLine();
        numLines++;

        do
        {
          bool parsed = false;
          try
          {
            if (line.Trim().Length > 0)
            {
              parsed =
                TryParse<DBCLib.AttributeValue>(ref line, ref numLines, streamReader, entries) ||
                TryParse<AttributeDefinition>(ref line, ref numLines, streamReader, entries) ||
                TryParse<AttributeDefault>(ref line, ref numLines, streamReader, entries) ||
                TryParse<BitTiming>(ref line, ref numLines, streamReader, entries) ||
                TryParse<Comment>(ref line, ref numLines, streamReader, entries) ||
                TryParse<Message>(ref line, ref numLines, streamReader, entries) ||
                TryParse<MessageTransmitters>(ref line, ref numLines, streamReader, entries) ||
                TryParse<NewSymbols>(ref line, ref numLines, streamReader, entries) ||
                TryParse<Nodes>(ref line, ref numLines, streamReader, entries) ||
                TryParse<Value>(ref line, ref numLines, streamReader, entries) ||
                TryParse<ValueTable>(ref line, ref numLines, streamReader, entries) ||
                TryParse<DBCLib.Version>(ref line, ref numLines, streamReader, entries)
                ;

              if (!parsed)
              {
                errors = true;
                Console.WriteLine("E {0}({1}): {2}",
                  path,
                  numLines,
                  line
                  );
              }
            }
          }
          catch (Exception e)
          {
            exceptionThrown = true;
            Console.WriteLine(e.ToString());
            Console.WriteLine(e.StackTrace);
            Console.WriteLine("X {0}({1}): {2}",
              path,
              numLines,
              line
              );
            break;
          }
          finally
          {
            if (exceptionThrown)
            {
              Console.WriteLine("{0}({1}): {2}",
                path,
                numLines,
                line
                );
            }
          }

          if (!parsed)
          {
            line = null;
            if (!streamReader.EndOfStream)
            {
              line = streamReader.ReadLine();
              numLines++;
            }
          }
        } while (line != null);
      }

      if (exceptionThrown || (errors && !AllowErrors))
      {
        return null;
      }

      if (entries.Count == 0)
      {
        return null;
      }

      if (DisplayEntries)
      {
        foreach (object entry in entries)
        {
          Console.WriteLine("D {0}", entry.ToString());
          if (entry is Message)
          {
            foreach (Message.Signal signal in ((Message)entry).Signals)
            {
              Console.WriteLine("D  {0}", signal.ToString());
            }
          }
        }
      }

      return entries;
    }
  }
}
