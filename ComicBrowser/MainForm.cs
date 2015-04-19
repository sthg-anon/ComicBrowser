
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

            Console.WriteLine("current directory: {0}", Directory.GetCurrentDirectory());

            //view
            view = new ComicView(viewPanel, viewControlPanel);
            view.ComicClicked += (c) => Console.WriteLine("{0} clicked!", c.File);
            rootSplitContainer.SplitterMoved += (sender, e) => view.OnPanelResized();
            
           // this.ResizeEnd += 

            //set up file history
            this.history = new SavedItemHistory("history.xml", this.CreateGraphics(), this.Font);

            //open file listener
            history.OnFileSelect += this.onFileHistorySelect;
            openFileDialog.Filter = CBXml.getFileFilter();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //open current cbxml.
            openCBXML(getArgFile(), false);

            //update the dropdown menu
            updateHistoryDropdown();

            this.ResizeEnd += (resize_sender, resize_e) => view.OnPanelResized();

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

            //enable tree view
            treeView.Enabled = true;

            //populate the tree view
            populateTreeView();

            view.SetView(root);
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

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            history.Dispose();
            root.Save();
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
            //cbxml.PrintTree(0);

            if (cbxml.Valid)
            {
                //Console.WriteLine("--comics loaded--");
                //CBXReader reader = new CBXReader(cbxml);
                //Comic c = cbxml.Comics[0];
                //for (int ii = 0; ii < cbxml.Comics.Count; ii++)
                //{
                //    Console.WriteLine("Comic: {0} ({1})", cbxml.Comics[ii].AbsolutePath(), cbxml.Comics[ii].File);
                //    reader.GetCover(cbxml.Comics[ii]);
                //}
               // c.GenerateThumbnail();
                //Image i = c.Thumbnail;
                //Image i = Image.FromFile(@"D:\desktop\tails_sonic.jpg");
               // PictureBox box = new PictureBox();
                //pictureBox1.Image = i;
                //pictureBox1.Height = i.Height;
                //pictureBox1.Width = i.Width;
                //box.Show();
                //using(Graphics g = this.CreateGraphics())
                //{
                //    g.DrawImage(c.Thumbnail, new Point(100, 100));
                //}
                view.SetView(cbxml);
            }
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
                    view.OnPanelResized();
                }
            }
        }
    }
}
