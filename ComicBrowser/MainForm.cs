
using SharpCompress.Archive;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ComicBrowser
{
    public partial class MainForm : Form
    {
        private FileOpenHistory history;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Console.WriteLine("current directory: {0}", Directory.GetCurrentDirectory());
            this.history = new FileOpenHistory(this.CreateGraphics(), this.Font);

            history.OnFileSelect += (s, ea) =>
                {
                    ToolStripButton tsb = (ToolStripButton)s;
                    Console.WriteLine(tsb.Text);
                };

            openFileDialog.Filter = CBXml.getFileFilter();

            CBXml root = new CBXml(getArgFile());
            root.Save();
            root.PrintTree(0);

            updateHistoryDropdown();
        }

        private void updateHistoryDropdown()
        {
            recentLibrariesToolStripMenuItem.DropDownItems.Clear();
            recentLibrariesToolStripMenuItem.DropDownItems.AddRange(history.getEntries());
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

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if(result == DialogResult.OK)
            {
                string file = openFileDialog.FileName;
                history.OpenFile(file);
                updateHistoryDropdown();
            }
        }

        void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            history.Dispose();
        }
    }
}
