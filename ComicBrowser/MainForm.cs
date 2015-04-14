
using SharpCompress.Archive;
using System;
using System.IO;
using System.Windows.Forms;

namespace ComicBrowser
{
    public partial class MainForm : Form
    {

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //string cbXML = findCBXML();
            //Console.WriteLine("CBXML: {0}", cbXML);
            //label1.Text = cbXML;

            Console.WriteLine("current directory: {0}", Directory.GetCurrentDirectory());

            //if(cbXML.Equals(String.Empty)) //no cbxml found
            //{
            //    cbXML = Path.Combine(Directory.GetCurrentDirectory(), String.Format("comics{0}", CBXml.GetFileExtension()));
            //    Console.WriteLine("cbxml not found, so one was made with {0}", cbXML);
            //}

            //string file = @"D:\desktop\#269.cbz";
            //string file = @"D:\desktop\test.zip";
            //string file = @"D:\desktop\#46.cbr";

            //var archive = ArchiveFactory.Open(file);
            //foreach (var entry in archive.Entries)
            //{
            //    if (!entry.IsDirectory)
            //    {
            //        Console.WriteLine(entry.FilePath);
            //    }
            //}

            CBXml cbxmlFile = new CBXml(getArgFile());
        }


        private static string getArgFile()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length <= 1)
            {
                return String.Empty;
            }

            string file = args[1];
            if (FileUtils.IsDirectory(file))
            {
                MessageBox.Show(String.Format("{0} is a file path!", file),
                    "File Input Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);

                Application.Exit();
            }

            if (!CBXml.FileExtensionMatches(file))
            {
                MessageBox.Show(String.Format("{0} is not a {1} file!", file, CBXml.GetFileExtension()),
                    "File Input Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);

                Application.Exit();
            }

            return file;
        }
    }
}
