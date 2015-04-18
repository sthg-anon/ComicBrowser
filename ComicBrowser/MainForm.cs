
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
            openCBXML(getArgFile(), false);

            //update the dropdown menu
            updateHistoryDropdown();
        }

        private void openCBXML(string file, bool create)
        {
            root = new CBXml();

            if(root.Valid)
            {
                cbxmlPostOpen();
                return;
            }

            if (file.Equals(String.Empty))
            {
                return;
            }

            root.Open(file, create);

            if(root.Valid)
            {
                cbxmlPostOpen();
            }
        }

        private void cbxmlPostOpen()
        {
            root.Save();

            //populate the tree view
            populateTreeView();
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
                //open(file);
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
                MessageBox.Show(String.Format("{0} is not a {1} file!", file, CBXml.CBXML_EXTENSION),
                    "File Input Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);

                Application.Exit();
            }

            return file;
        }

        private string promptOpenCBXMLFile()
        {
            string file = String.Empty;
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                file = openFileDialog.FileName;
            }
            return file;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string file = promptOpenCBXMLFile();
            if(!file.Equals(String.Empty))
            {
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

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = CBXml.DEFAULT_CBXML;
            DialogResult result = saveFileDialog.ShowDialog();
            if(result == DialogResult.OK)
            {
                openCBXML(saveFileDialog.FileName, true);
            }
        }
    }
}
