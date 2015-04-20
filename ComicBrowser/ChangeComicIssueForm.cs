using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ComicBrowser
{
    public partial class ChangeComicIssueForm : Form
    {
        public delegate void finishedDelegate();
        public event finishedDelegate Finished;

        private readonly Comic c;

        internal ChangeComicIssueForm(Comic c, int indexEstimate)
        {
            InitializeComponent();

            this.c = c;

            this.numericUpDown.Value = indexEstimate;
            this.Text = c.File;
            this.pictureBox.Image = c.OriginalCoverImage;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            c.Issue = (int)numericUpDown.Value;
            Finished();
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
