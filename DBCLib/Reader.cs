using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace DBCLib
{
  public class Reader
  {
    public bool AllowErrors
    {
      get;
      set;
    } = false;

    public bool DisplayEntries
    {
      get;
      set;
    } = false;

    bool TryParse<T>(
      ref ParseContext parseContext,
      List<object> entries
      )
      where T : Entry, new()
    {
      T t = new T();
      if (t.TryParse(ref parseContext))
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

      var currentCulture = Thread.CurrentThread.CurrentCulture;
      Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

      try
      {
        bool errors = false;
        bool exceptionThrown = false;

        ParseContext parseContext = new ParseContext(streamReader);

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
                  TryParse<DBCLib.AttributeValue>(ref parseContext, entries) ||
                  TryParse<AttributeDefinition>(ref parseContext, entries) ||
                  TryParse<AttributeDefault>(ref parseContext, entries) ||
                  TryParse<BitTiming>(ref parseContext, entries) ||
                  TryParse<Comment>(ref parseContext, entries) ||
                  TryParse<Message>(ref parseContext, entries) ||
                  TryParse<MessageTransmitters>(ref parseContext, entries) ||
                  TryParse<NewSymbols>(ref parseContext, entries) ||
                  TryParse<Nodes>(ref parseContext, entries) ||
                  TryParse<Value>(ref parseContext, entries) ||
                  TryParse<ValueTable>(ref parseContext, entries) ||
                  TryParse<DBCLib.Version>(ref parseContext, entries)
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
      }
      finally
      {
        Thread.CurrentThread.CurrentCulture = currentCulture;
      }

      return entries;
    }
  }
}
