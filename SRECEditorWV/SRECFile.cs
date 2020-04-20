using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRECEditorWV
{
    public class SRECFile
    {
        public string myPath = "";
        public List<SRECEntry> entries = new List<SRECEntry>();

        public SRECFile()
        {
        }

        public SRECFile(string path)
        {
            myPath = path;
            string[] lines = File.ReadAllLines(path);
            entries = new List<SRECEntry>();
            foreach (string line in lines)
                if (line.Trim() != "")
                    entries.Add(new SRECEntry(line));
        }

        public void Save()
        {
            StringBuilder sb = new StringBuilder();
            foreach (SRECEntry e in entries)
                sb.AppendLine(e.Save());
            File.WriteAllText(myPath, sb.ToString());
        }
    }
}
