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
    public partial class EditEntry : Form
    {
        public SRECEntry entry;

        public EditEntry()
        {
            InitializeComponent();
        }

        private void EditEntry_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = Enum.GetNames(typeof(SRECEntry.SRECEntryType));
            comboBox1.SelectedIndex = (int)entry.type;
            switch (entry.GetAddressSize())
            {
                case 2:
                    textBox1.Text = entry.address.ToString("X4");
                    break;
                case 3:
                    textBox1.Text = entry.address.ToString("X6");
                    break;
                case 4:
                    textBox1.Text = entry.address.ToString("X8");
                    break;
            }
            textBox2.Text = "";
            foreach (byte b in entry.data)
                textBox2.Text += b.ToString("X2") + " ";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            End();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                End();
        }

        private void End()
        {
            try
            {
                entry.type = (SRECEntry.SRECEntryType)comboBox1.SelectedIndex;
                entry.address = Convert.ToUInt32(textBox1.Text, 16);
                string s = textBox2.Text.Trim();
                while (s.Contains(" "))
                    s = s.Replace(" ", "");
                MemoryStream m = new MemoryStream();
                for (int i = 0; i < s.Length; i += 2)
                    m.WriteByte(Convert.ToByte(s.Substring(i, 2), 16));
                entry.data = m.ToArray();
                entry.dataSize = (byte)entry.data.Length;
                entry.RefreshCRC();
            }
            catch
            {
                MessageBox.Show("Please check your input!");
                return;
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                End();
        }
    }
}
