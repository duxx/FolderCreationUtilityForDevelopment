using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Structurer
{
    public partial class Form3 : Form
    {
        private StreamReader streamReader;
        private StreamWriter streamWriter;

        public Form3()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            UpdateSnippetList();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 1)
            {
                textBox3.Enabled = true;
                textBox4.Enabled = false;
            }
            else
            {
                textBox3.Enabled = false;
                textBox4.Enabled = true;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
            {
                button2.Enabled = false;
                return;
            }
            button2.Enabled = true;

            //Check for snippets files
            if (File.Exists(Application.StartupPath + Form1.ContentsFolder + (string)listBox1.Items[listBox1.SelectedIndex] + ".txt"))
            {
                streamReader = new StreamReader(Application.StartupPath + Form1.ContentsFolder + (string)listBox1.Items[listBox1.SelectedIndex] + ".txt");
                textBox2.Text = (string)listBox1.Items[listBox1.SelectedIndex];
                textBox4.Text = streamReader.ReadToEnd();
                textBox3.Text = "";
                streamReader.Close();
                textBox4.Enabled = true;
                textBox3.Enabled = false;
                comboBox1.SelectedIndex = 0;
            }
            //Check for url files
            else if (File.Exists(Application.StartupPath + Form1.DownloadsFolder + (string)listBox1.Items[listBox1.SelectedIndex] + ".txt"))
            {
                streamReader = new StreamReader(Application.StartupPath + Form1.DownloadsFolder + (string)listBox1.Items[listBox1.SelectedIndex] + ".txt");
                textBox2.Text = (string)listBox1.Items[listBox1.SelectedIndex];
                textBox3.Text = streamReader.ReadToEnd();
                textBox4.Text = "";
                streamReader.Close();
                textBox4.Enabled = false;
                textBox3.Enabled = true;
                comboBox1.SelectedIndex = 1;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text.Length > 0)
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;

            //Save code snippet
            if (comboBox1.SelectedIndex == 0)
            {
                if (File.Exists(Application.StartupPath + Form1.ContentsFolder + textBox2.Text + ".txt."))
                {
                    File.Delete(Application.StartupPath + Form1.ContentsFolder + textBox2.Text + ".txt.");
                }

                streamWriter = new StreamWriter(Application.StartupPath + Form1.ContentsFolder + textBox2.Text + ".txt.");
                streamWriter.Write(textBox4.Text);
                streamWriter.Close();
                UpdateSnippetList();
            }
            else
            {
                if (File.Exists(Application.StartupPath + Form1.DownloadsFolder + textBox2.Text + ".txt."))
                {
                    File.Delete(Application.StartupPath + Form1.DownloadsFolder + textBox2.Text + ".txt.");
                }

                streamWriter = new StreamWriter(Application.StartupPath + Form1.DownloadsFolder + textBox2.Text + ".txt.");
                streamWriter.Write(textBox3.Text);
                streamWriter.Close();
                UpdateSnippetList();
            }
        }

        private void UpdateSnippetList()
        {
            listBox1.Items.Clear();

            //Code snippets
            if (! Directory.Exists(Application.StartupPath + Form1.ContentsFolder))
            {
                Directory.CreateDirectory(Application.StartupPath + Form1.ContentsFolder);
            }

            string[] files = Directory.GetFiles(Application.StartupPath + Form1.ContentsFolder);
            foreach (string file in files)
            {
                string fileName = file.Substring(file.LastIndexOf('\\') + 1);
                listBox1.Items.Add(fileName.Substring(0, fileName.LastIndexOf('.')));
            }

            //Urls
            if (!Directory.Exists(Application.StartupPath + Form1.DownloadsFolder))
            {
                Directory.CreateDirectory(Application.StartupPath + Form1.DownloadsFolder);
            }

            files = Directory.GetFiles(Application.StartupPath + Form1.DownloadsFolder);
            foreach (string file in files)
            {
                string fileName = file.Substring(file.LastIndexOf('\\') + 1);
                listBox1.Items.Add(fileName.Substring(0, fileName.LastIndexOf('.')));
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0) return;
            if (File.Exists(Application.StartupPath + Form1.ContentsFolder + (string)listBox1.Items[listBox1.SelectedIndex] + ".txt"))
            {
                File.Delete(Application.StartupPath + Form1.ContentsFolder + (string)listBox1.Items[listBox1.SelectedIndex] + ".txt");
            }
            else if (File.Exists(Application.StartupPath + Form1.DownloadsFolder + (string)listBox1.Items[listBox1.SelectedIndex] + ".txt"))
            {
                File.Delete(Application.StartupPath + Form1.DownloadsFolder + (string)listBox1.Items[listBox1.SelectedIndex] + ".txt");
            }
            UpdateSnippetList();
        }
    }
}
