using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SRECEditorWV
{
    public partial class Form1 : Form
    {
        SRECFile file = new SRECFile();

        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.srec;*.s19|*.srec;*.s19";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                file = new SRECFile(d.FileName);
                Refresh();
            }
        }

        public void Refresh()
        {
            listBox1.Items.Clear();
            foreach (SRECEntry e in file.entries)
                listBox1.Items.Add(e.ToListEntry());
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            file.entries.RemoveAt(n);
            Refresh();
            if (listBox1.Items.Count > n)
                listBox1.SelectedIndex = n;
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Edit();
        }

        private void editToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Edit();
        }

        private void Edit()
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            EditEntry ee = new EditEntry();
            ee.entry = file.entries[n];
            if (ee.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                file.entries[n] = ee.entry;
                Refresh();
                listBox1.SelectedIndex = n;
            }
        }

        private void addToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SRECEntry entry = new SRECEntry();
            entry.type = SRECEntry.SRECEntryType.Header;
            entry.data = new byte[0];
            entry.RefreshCRC();
            file.entries.Add(entry);
            Refresh();
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        private void moveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n < 1)
                return;
            SRECEntry en = file.entries[n];
            file.entries[n] = file.entries[n - 1];
            file.entries[n - 1] = en;
            Refresh();
            listBox1.SelectedIndex = n - 1;
        }

        private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1 || n > listBox1.Items.Count - 2)
                return;
            SRECEntry en = file.entries[n];
            file.entries[n] = file.entries[n + 1];
            file.entries[n + 1] = en;
            Refresh();
            listBox1.SelectedIndex = n + 1;
        }

        private void listBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            Edit();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            file.Save();
            MessageBox.Show("Done.");
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.srec;*.s19|*.srec;*.s19";
            d.FileName = Path.GetFileName(file.myPath);
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                file.myPath = d.FileName;
                file.Save();
                MessageBox.Show("Done.");
            }
        }

        private void showMemoryDumpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (file.entries.Count < 1)
                return;
            List<Section> sections = new List<Section>();
            foreach (SRECEntry entry in file.entries)
            {
                switch (entry.type)
                {
                    case SRECEntry.SRECEntryType.Data16:
                    case SRECEntry.SRECEntryType.Data24:
                    case SRECEntry.SRECEntryType.Data32:
                        Section sec = null;
                        foreach (Section s in sections)
                            if (entry.address >= s.start && entry.address < s.end)
                            {
                                MessageBox.Show("Memory overlap detected!");
                                return;
                            }
                            else if (s.end == entry.address)
                            {
                                sec = s;
                                MemoryStream m = new MemoryStream();
                                m.Write(s.data, 0, (int)(s.end - s.start));
                                m.Write(entry.data, 0, (int)entry.dataSize);
                                s.end += entry.dataSize;
                                s.data = m.ToArray();
                                break;
                            }
                        if (sec == null)
                        {
                            sec = new Section();
                            sec.start = entry.address;
                            sec.end = entry.address + entry.dataSize;
                            sec.data = entry.data;
                            sections.Add(sec);
                        }
                        break;
                }
            }
            MemoryDump d = new MemoryDump();
            d.sections = sections;
            d.ShowDialog();
        }
    }
}
