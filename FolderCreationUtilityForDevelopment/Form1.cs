using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using Ionic.Zip;

namespace Structurer
{
    public partial class Form1 : Form
    {
        private string basePath;
        private string currFolder;
        public static string templateFolder = "\\templates\\";
        public static string contentsFolder = "\\contents\\";
        public static string downloadsFolder = "\\downloads\\";
        
        private StreamReader streamReader;
        private StreamWriter streamWriter;
        private WebClient webClient = new WebClient();

        public static string template;

        public Form1()
        {
            InitializeComponent();
            textBox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            GetTemplates();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists(textBox1.Text))
                {
                    Directory.CreateDirectory(textBox1.Text);
                }

                basePath = currFolder = textBox1.Text + "\\";

                foreach (string line in textBox2.Lines)
                {
                    handleLine(line);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The following error occured: " + ex.ToString(), "Folderizer - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void handleLine(string line)
        {
            //Case 1: Has no /
            //Case 2: Ends with /
            //Case 3: Begins with /
            //Case 4: Contains a /
            if (line[0] == '/')
            {
                //Current folder is currFolder
                if (line.IndexOf(':') > 0)
                {
                    string[] parts = line.Split(':');
                    string fileName = parts[0];
                    if (File.Exists(Application.StartupPath + contentsFolder + parts[1] + ".txt"))
                    {
                        streamReader = new StreamReader(Application.StartupPath + contentsFolder + parts[1] + ".txt");
                        string contents = streamReader.ReadToEnd();
                        streamReader.Close();

                        streamWriter = new StreamWriter(currFolder + parts[0]);
                        streamWriter.Write(contents);
                        streamWriter.Close();
                    }
                    else if (File.Exists(Application.StartupPath + downloadsFolder + parts[1] + ".txt"))
                    {
                        streamReader = new StreamReader(Application.StartupPath + downloadsFolder + parts[1] + ".txt");
                        string url = streamReader.ReadToEnd();
                        streamReader.Close();

                        //webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(webClient_DownloadFileCompleted);
                        webClient.DownloadFileCompleted += (s, e) => 
                        {
                            this.Text = "Folderizer - Completed";
                            if (url.IndexOf(".zip") > 0)
                            {
                                using (ZipFile zipFile = ZipFile.Read(currFolder + "\\" + url.Substring(url.LastIndexOf('/') + 1)))
                                {
                                    foreach (ZipEntry entry in zipFile)
                                    {
                                        entry.Extract(currFolder, ExtractExistingFileAction.OverwriteSilently);
                                    }
                                }
                            }
                        };
                        webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
                        this.Text = "Downloading 0%";
                        webClient.DownloadFileAsync(new Uri(url), currFolder + "\\" + url.Substring(url.LastIndexOf('/') + 1));
                        
                    }
                }
                else
                {
                    File.Create(currFolder + line);
                }
            }
            else if (line[line.Length - 1] == '/')
            {
                //Current folder changes, not a file
                currFolder = basePath;
                currFolder += line;
                Directory.CreateDirectory(currFolder);
            }
            else if (line.IndexOf('/') > 0)
            {
                //Current changes +  a file
                currFolder = basePath;
                currFolder += line.Substring(0, line.LastIndexOf('/'));
                Directory.CreateDirectory(currFolder);
                string line2 = line.Substring(line.LastIndexOf('/'));
                if (line.IndexOf(':') > 0)
                {
                    string[] parts = line2.Split(':');
                    string fileName = parts[0];
                    if (File.Exists(Application.StartupPath + contentsFolder + parts[1] + ".txt"))
                    {
                        streamReader = new StreamReader(Application.StartupPath + contentsFolder + parts[1] + ".txt");
                        string contents = streamReader.ReadToEnd();
                        streamReader.Close();

                        streamWriter = new StreamWriter(currFolder + parts[0]);
                        streamWriter.Write(contents);
                        streamWriter.Close();
                    }
                }
                else
                {
                    File.Create(currFolder + line2);
                }
            }
            else
            {
                //Current folder is basePath
                currFolder = basePath;
                //Current folder is currFolder
                if (line.IndexOf(':') > 0)
                {
                    string[] parts = line.Split(':');
                    string fileName = parts[0];
                    if (File.Exists(Application.StartupPath + contentsFolder + parts[1] + ".txt"))
                    {
                        streamReader = new StreamReader(Application.StartupPath + contentsFolder + parts[1] + ".txt");
                        string contents = streamReader.ReadToEnd();
                        streamReader.Close();

                        streamWriter = new StreamWriter(currFolder + parts[0]);
                        streamWriter.Write(contents);
                        streamWriter.Close();
                    }
                }
                else
                {
                    File.Create(currFolder + line);
                }
            }
        }

        void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.Text = "Downloading " + e.ProgressPercentage + "%";
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == comboBox1.Items.Count - 1)
            {
                comboBox1.Items.RemoveAt(comboBox1.SelectedIndex);
                //Save a new template
                template = textBox2.Text;
                Form2 form2 = new Form2();
                if (form2.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    if (! Directory.Exists(Application.StartupPath + templateFolder))
                    {
                        Directory.CreateDirectory(Application.StartupPath + templateFolder);
                    }
                    if (File.Exists(Application.StartupPath + templateFolder + form2.textBox1.Text + ".txt"))
                    {
                        File.Delete(Application.StartupPath + templateFolder + form2.textBox1.Text + ".txt");
                    }
                    streamWriter = new StreamWriter(Application.StartupPath + templateFolder + form2.textBox1.Text + ".txt");
                    streamWriter.Write(form2.textBox2.Text);
                    streamWriter.Close();
                    GetTemplates();
                }
            }
            else
            {
                streamReader = new StreamReader(Application.StartupPath + templateFolder + (string)comboBox1.Items[comboBox1.SelectedIndex] + ".txt");
                textBox2.Text = streamReader.ReadToEnd();
                streamReader.Close();
            }
        }

        private void GetTemplates()
        {
            comboBox1.Items.Clear();

            //Get templates if exists
            if (Directory.Exists(Application.StartupPath + templateFolder))
            {
                string[] files = Directory.GetFiles(Application.StartupPath + templateFolder);
                foreach (string file in files)
                {
                    string fileName = file.Substring(file.LastIndexOf('\\') + 1);
                    comboBox1.Items.Add(fileName.Substring(0, fileName.LastIndexOf('.')));
                }
            }
            comboBox1.Items.Add("Save as template...");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void saveAsANewTemplateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Save a new template
            template = textBox2.Text;
            Form2 form2 = new Form2();
            if (form2.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                if (!Directory.Exists(Application.StartupPath + templateFolder))
                {
                    Directory.CreateDirectory(Application.StartupPath + templateFolder);
                }
                if (File.Exists(Application.StartupPath + templateFolder + form2.textBox1.Text + ".txt"))
                {
                    File.Delete(Application.StartupPath + templateFolder + form2.textBox1.Text + ".txt");
                }
                streamWriter = new StreamWriter(Application.StartupPath + templateFolder + form2.textBox1.Text + ".txt");
                streamWriter.Write(form2.textBox2.Text);
                streamWriter.Close();
                GetTemplates();
            }
        }

        private void manageContentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3();
            if (form3.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

            }
        }
    }
}
