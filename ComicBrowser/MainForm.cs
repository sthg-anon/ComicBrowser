
using SharpCompress.Archive;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ComicBrowser
{
    public partial class MainForm : Form
    {
        private SavedItemHistory history;
        private CBXml root = null;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Console.WriteLine("current directory: {0}", Directory.GetCurrentDirectory());
            //set up file history
            this.history = new SavedItemHistory("history.xml", this.CreateGraphics(), this.Font);

            //open file listener
            history.OnFileSelect += this.onFileHistorySelect;
            openFileDialog.Filter = CBXml.getFileFilter();

            //open current cbxml.
            open(getArgFile());

            //update the dropdown menu
            updateHistoryDropdown();

            //populate the tree view
            populateTreeView();
        }

        private void open(string file)
        {
            root = new CBXml(file);
            root.Save();
        }

        private void onFileHistorySelect(object sender, EventArgs e)
        {
            ToolStripButton tsb = (ToolStripButton)sender;
            string file = tsb.Text;
            
            if(!File.Exists(file))
            {
                history.Remove(file);
                MessageBox.Show(String.Format("{0} no longer exists!", file), "File not found!", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                updateHistoryDropdown();
            }
            else
            {
                open(file);
            }
        }

        private void updateHistoryDropdown()
        {
            recentLibrariesToolStripMenuItem.DropDownItems.Clear();
            ToolStripButton[] buttons = history.getEntries();
            if(buttons.Length > 0)
            {
                recentLibrariesToolStripMenuItem.DropDownItems.AddRange(buttons);
                recentLibrariesToolStripMenuItem.Enabled = true;
            }
            else
            {
                recentLibrariesToolStripMenuItem.Enabled = false;
            }
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

        private void populateTreeView()
        {
            if (root == null) return;

            treeView.Nodes.Add(root.GetNode());
            treeView.ExpandAll();
        }

        private void treeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode node = treeView.SelectedNode;
            CBXml cbxml = node.GetCBXml();
            cbxml.PrintTree(0);
        }

        private void treeView_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
