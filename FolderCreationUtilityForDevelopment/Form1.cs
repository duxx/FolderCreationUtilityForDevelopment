using System;
using System.Windows.Forms;
using System.IO;
using System.Net;
using Ionic.Zip;

namespace Structurer
{
    public partial class Form1 : Form
    {
        private string _basePath;
        private string _currFolder;
        public static string TemplateFolder = "\\templates\\";
        public static string ContentsFolder = "\\contents\\";
        public static string DownloadsFolder = "\\downloads\\";
        public static string ExportFolder = "\\export\\";
        
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;
        private readonly WebClient _webClient = new WebClient();
        private bool _isDownloading = false;

        public static string Template;

        public Form1()
        {
            InitializeComponent();
            textBox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            GetTemplates();
        }

        private void Button1Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void Button2Click(object sender, EventArgs e)
        {
            button2.Enabled = false;

            try
            {
                if (!Directory.Exists(textBox1.Text))
                {
                    Directory.CreateDirectory(textBox1.Text);
                }

                _basePath = _currFolder = textBox1.Text + "\\";

                foreach (var line in textBox2.Lines)
                {
                    HandleLine(line);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The following error occured: " + ex.ToString(), "Folderizer - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (!_isDownloading)
                {
                    button2.Enabled = true;
                    var myPath = textBox1.Text;
                    var prc = new System.Diagnostics.Process {StartInfo = {FileName = myPath}};
                    prc.Start();
                }
            }
        }

        private void HandleLine(string line)
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
                    var parts = line.Split(':');
                    var fileName = parts[0];
                    if (File.Exists(Application.StartupPath + ContentsFolder + parts[1] + ".txt"))
                    {
                        _streamReader = new StreamReader(Application.StartupPath + ContentsFolder + parts[1] + ".txt");
                        var contents = _streamReader.ReadToEnd();
                        _streamReader.Close();

                        _streamWriter = new StreamWriter(_currFolder + parts[0]);
                        _streamWriter.Write(contents);
                        _streamWriter.Close();
                    }
                    else if (File.Exists(Application.StartupPath + DownloadsFolder + parts[1] + ".txt"))
                    {
                        _streamReader = new StreamReader(Application.StartupPath + DownloadsFolder + parts[1] + ".txt");
                        var url = _streamReader.ReadToEnd();
                        _streamReader.Close();

                        _webClient.DownloadFileCompleted += (s, e) => 
                        {
                            _isDownloading = false;
                            button2.Enabled = true;
                            this.Text = "Folderizer - Completed";

                            if (!_webClient.ResponseHeaders["Content-type"].Equals("application/zip")) return;
                            using (ZipFile zipFile = ZipFile.Read(_currFolder + "\\" + url.Substring(url.LastIndexOf('/') + 1)))
                            {
                                foreach (ZipEntry entry in zipFile)
                                {
                                    entry.Extract(_currFolder, ExtractExistingFileAction.OverwriteSilently);
                                }
                            }
                            //Delete the file
                            File.Delete(_currFolder + "\\" + url.Substring(url.LastIndexOf('/') + 1));

                            var myPath = textBox1.Text;
                            var prc = new System.Diagnostics.Process { StartInfo = { FileName = myPath } };
                            prc.Start();
                        };
                        _webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(WebClientDownloadProgressChanged);
                        this.Text = "Downloading 0%";
                        _webClient.DownloadFileAsync(new Uri(url), _currFolder + "\\" + url.Substring(url.LastIndexOf('/') + 1));
                        _isDownloading = true;
                        button2.Enabled = false;
                    }
                }
                else
                {
                    File.Create(_currFolder + line);
                }
            }
            else if (line[line.Length - 1] == '/')
            {
                //Current folder changes, not a file
                _currFolder = _basePath;
                _currFolder += line;
                Directory.CreateDirectory(_currFolder);
            }
            else if (line.IndexOf('/') > 0)
            {
                //Current changes +  a file
                _currFolder = _basePath;
                _currFolder += line.Substring(0, line.LastIndexOf('/'));
                Directory.CreateDirectory(_currFolder);
                var line2 = line.Substring(line.LastIndexOf('/'));
                if (line.IndexOf(':') > 0)
                {
                    var parts = line2.Split(':');
                    var fileName = parts[0];
                    if (File.Exists(Application.StartupPath + ContentsFolder + parts[1] + ".txt"))
                    {
                        _streamReader = new StreamReader(Application.StartupPath + ContentsFolder + parts[1] + ".txt");
                        var contents = _streamReader.ReadToEnd();
                        _streamReader.Close();

                        _streamWriter = new StreamWriter(_currFolder + parts[0]);
                        _streamWriter.Write(contents);
                        _streamWriter.Close();
                    }
                }
                else
                {
                    File.Create(_currFolder + line2);
                }
            }
            else
            {
                //Current folder is basePath
                _currFolder = _basePath;
                //Current folder is currFolder
                if (line.IndexOf(':') > 0)
                {
                    var parts = line.Split(':');
                    var fileName = parts[0];
                    if (File.Exists(Application.StartupPath + ContentsFolder + parts[1] + ".txt"))
                    {
                        _streamReader = new StreamReader(Application.StartupPath + ContentsFolder + parts[1] + ".txt");
                        var contents = _streamReader.ReadToEnd();
                        _streamReader.Close();

                        _streamWriter = new StreamWriter(_currFolder + parts[0]);
                        _streamWriter.Write(contents);
                        _streamWriter.Close();
                    }
                }
                else
                {
                    var path = Path.Combine(_currFolder, line);

                    if (!File.Exists(path))
                        File.Create(path);
                }
            }
        }

        void WebClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.Text = "Downloading " + e.ProgressPercentage + "%";
        }

        private void ComboBox1SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == comboBox1.Items.Count - 1)
            {
                comboBox1.Items.RemoveAt(comboBox1.SelectedIndex);
                //Save a new template
                Template = textBox2.Text;
                var form2 = new Form2();
                if (form2.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    if (! Directory.Exists(Application.StartupPath + TemplateFolder))
                    {
                        Directory.CreateDirectory(Application.StartupPath + TemplateFolder);
                    }
                    if (File.Exists(Application.StartupPath + TemplateFolder + form2.textBox1.Text + ".txt"))
                    {
                        File.Delete(Application.StartupPath + TemplateFolder + form2.textBox1.Text + ".txt");
                    }
                    _streamWriter = new StreamWriter(Application.StartupPath + TemplateFolder + form2.textBox1.Text + ".txt");
                    _streamWriter.Write(form2.textBox2.Text);
                    _streamWriter.Close();
                    GetTemplates();
                }
            }
            else
            {
                _streamReader = new StreamReader(Application.StartupPath + TemplateFolder + (string)comboBox1.Items[comboBox1.SelectedIndex] + ".txt");
                textBox2.Text = _streamReader.ReadToEnd();
                _streamReader.Close();
            }
        }

        private void GetTemplates()
        {
            comboBox1.Items.Clear();

            //Get templates if exists
            if (Directory.Exists(Application.StartupPath + TemplateFolder))
            {
                var files = Directory.GetFiles(Application.StartupPath + TemplateFolder);
                foreach (var file in files)
                {
                    var fileName = file.Substring(file.LastIndexOf('\\') + 1);
                    comboBox1.Items.Add(fileName.Substring(0, fileName.LastIndexOf('.')));
                }
            }
            comboBox1.Items.Add("Save as template...");
        }

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SaveAsANewTemplateToolStripMenuItemClick(object sender, EventArgs e)
        {
            //Save a new template
            Template = textBox2.Text;
            var form2 = new Form2();
            if (form2.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) return;
            if (!Directory.Exists(Application.StartupPath + TemplateFolder))
            {
                Directory.CreateDirectory(Application.StartupPath + TemplateFolder);
            }
            if (File.Exists(Application.StartupPath + TemplateFolder + form2.textBox1.Text + ".txt"))
            {
                File.Delete(Application.StartupPath + TemplateFolder + form2.textBox1.Text + ".txt");
            }
            _streamWriter = new StreamWriter(Application.StartupPath + TemplateFolder + form2.textBox1.Text + ".txt");
            _streamWriter.Write(form2.textBox2.Text);
            _streamWriter.Close();
            GetTemplates();
        }

        private void ManageContentsToolStripMenuItemClick(object sender, EventArgs e)
        {
            var form3 = new Form3();
            if (form3.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

            }
        }

        private void ExportTemplateToolStripMenuItemClick(object sender, EventArgs e)
        {
            using (var zipFile = new ZipFile())
            {
                if (!Directory.Exists(Application.StartupPath + TemplateFolder)
                    ||!Directory.Exists(Application.StartupPath + DownloadsFolder)
                    ||!Directory.Exists(Application.StartupPath + ContentsFolder)) return;

                if (!Directory.Exists(Application.StartupPath + ExportFolder))
                    Directory.CreateDirectory(Application.StartupPath + ExportFolder);

                zipFile.AddDirectory(Application.StartupPath + TemplateFolder, "templates");
                zipFile.AddDirectory(Application.StartupPath + DownloadsFolder, "downloads");
                zipFile.AddDirectory(Application.StartupPath + ContentsFolder, "contents");

                var myPath = Application.StartupPath + ExportFolder + Environment.UserName + "_Folderizer.zip";
                zipFile.Save(myPath);
                myPath = Application.StartupPath + ExportFolder;
                var prc = new System.Diagnostics.Process { StartInfo = { FileName = myPath } };
                prc.Start();
            }
        }

        private void ImportTemplatesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            var file = openFileDialog1.FileName;
            try
            {
                using (var zipFile = ZipFile.Read(file))
                {
                    foreach (var entry in zipFile)
                    {
                        entry.Extract(Application.StartupPath, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error importing templates.\n" + ex.ToString(), "Error - Folderizer",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            MessageBox.Show("Templates imported successfully", "Folderizer", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }
    }
}
