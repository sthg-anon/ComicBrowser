
using SharpCompress.Archive;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ComicBrowser
{
    public partial class MainForm : Form
    {
        private readonly SavedItemHistory history;
        private readonly ComicView view;

        private FormWindowState LastWindowState = FormWindowState.Minimized;
        private CBXml root = null;

        public MainForm()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);

            //Console.WriteLine("current directory: {0}", Directory.GetCurrentDirectory());

            //view
            view = new ComicView(viewPanel, viewControlPanel);
            view.ComicClicked += (c) => System.Diagnostics.Process.Start(c.AbsolutePath());
            rootSplitContainer.SplitterMoved += (sender, e) => view.AdjustView();
            
            //set up file history
            this.history = new SavedItemHistory("history.xml", this.CreateGraphics(), this.Font);

            //open file listener
            history.OnFileSelect += this.onFileHistorySelect;
            openFileDialog.Filter = CBXml.getFileFilter();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //open current cbxml.
            string file = getArgFile();
            openCBXML(file, false);

            //update the dropdown menu
            updateHistoryDropdown();

            this.ResizeEnd += (resize_sender, resize_e) => view.AdjustView();

        }

        private void openCBXML(string file, bool create)
        {
            if(!file.Equals(string.Empty))
            {
                root = new CBXml(file, true);//guranteed creation
            }
            else
            {
                root = new CBXml(false);//look for a cbxml in the working directory. Root will either be valid or not...
            }

            if(root.Valid)
            {
                root.Save();

                //enable tree view
                treeView.Enabled = true;

                //populate the tree view
                populateTreeView();

                view.SetView(root);
            }
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
                Console.WriteLine("file: {0}", file);
                treeView.Nodes.Clear();
                openCBXML(file, false);
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
                treeView.Nodes.Clear();
                openCBXML(file, false);
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            history.Dispose();
            root.Save();
        }

        private void populateTreeView()
        {
            //root.PrintTree(0);
            if (root == null) return;

            treeView.Nodes.Add(root.GetNode());
            treeView.ExpandAll();
        }

        private void treeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode node = treeView.SelectedNode;
            CBXml cbxml = node.GetCBXml();

            if (cbxml.Valid)
            {
                view.SetView(cbxml);
            }
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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
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

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState != LastWindowState)
            {
                LastWindowState = WindowState;

                if (WindowState == FormWindowState.Maximized || WindowState == FormWindowState.Normal)
                {
                    view.AdjustView();
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            root.Save();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new AboutBox()).Show();
        }
    }
}
