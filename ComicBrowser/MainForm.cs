
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
            string cbXML = findCBXML();
            Console.WriteLine("CBXML: {0}", cbXML);
            label1.Text = cbXML;

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

            CBXml cbxmlFile = new CBXml(cbXML);
        }

        /// <summary>
        /// Locates the .cbxml file in the working direcctory, if there is ONE.
        /// If there is more than one .cbxml file, then neither will be returned, due to the
        /// ambiguity.
        /// 
        /// If the user inputted the CBXML file as the first argument, it will also be returned by this function.
        /// </summary>
        /// <returns>The path to the .cbxml file, if there is one.</returns>
        private string findCBXML()
        {
            string argFile = getArgFile();
            if (!argFile.Equals(String.Empty))
            {
                return argFile;
            }

            string currentFolder = Directory.GetCurrentDirectory();
            string[] files = Directory.GetFiles(currentFolder);

            string cbXML = String.Empty;

            for(int ii = 0; ii < files.Length; ii++)
            {
                if(FileUtils.IsDirectory(files[ii]) || !CBXml.FileExtensionMatches(files[ii]))
                {
                    continue;
                }

                if(!cbXML.Equals(String.Empty))
                {
                    return String.Empty;
                }

                cbXML = files[ii];
            }

            return cbXML;
        }

        private string getArgFile()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length <= 1)
            {
                return String.Empty;
            }

            string file = args[1];
            if(FileUtils.IsDirectory(file))
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
