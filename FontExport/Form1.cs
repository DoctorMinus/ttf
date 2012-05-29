using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FontExport
{
    public partial class Form1 : Form
    {
        private Font exFont = new Font("MusetteMusic4b", 100);
        public Form1()
        {
            InitializeComponent();
        }

        private void btnFont_Click(object sender, EventArgs e)
        {
            fontDialog.Font = exFont;

            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                exFont = fontDialog.Font;
            }
        }
        private String GetOutline(char c)
        {
            return WordGraph.GetOutline(GetGB2312Coding(c), exFont );
        }
        private uint GetGB2312Coding(char ch)
        {
            byte[] bts = Encoding.GetEncoding("GB2312").GetBytes(new char[] { ch });
            uint val = bts[0];
            if (bts.Length > 1)
            {
                val = val * 256 + bts[1];
            }
            return val;
        }
        private void btnDo_Click(object sender, EventArgs e)
        {
            if (txtInput.Text.Length > 0)
            {
                char ch = txtInput.Text[0];
                txtOutput.Text = GetOutline(ch);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
