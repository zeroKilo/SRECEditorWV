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
using Be.Windows.Forms;

namespace SRECEditorWV
{
    public partial class MemoryDump : Form
    {
        public List<Section> sections;

        public MemoryDump()
        {
            InitializeComponent();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MemoryStream m = new MemoryStream();
                for (long i = 0; i < hb1.ByteProvider.Length; i++)
                    m.WriteByte(hb1.ByteProvider.ReadByte(i));
                File.WriteAllBytes(d.FileName, m.ToArray());
                MessageBox.Show("Done.");
            }
        }

        private void MemoryDump_Load(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            foreach (Section sec in sections)
                listBox1.Items.Add("Section 0x" + sec.start.ToString("X8") + " - 0x" + (sec.end - 1).ToString("X8"));
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            hb1.ByteProvider = new DynamicByteProvider(sections[n].data);
            hb1.LineInfoOffset = sections[n].start;
        }
    }
}
