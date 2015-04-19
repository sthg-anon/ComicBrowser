using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace ComicBrowser
{
    public partial class ThumbnailGeneratorProgressWindow : Form
    {
        public delegate void finishedDelegate();
        public event finishedDelegate Finished;

        private readonly List<Comic> comics;

        private volatile bool done = false;

        internal ThumbnailGeneratorProgressWindow(CBXml cbxml)
        {
            InitializeComponent();

            this.comics = new List<Comic>(cbxml.Comics);
        }

        public void Start()
        {
            Thread thread = new Thread(run);
            progressBar1.Minimum = 0;
            progressBar1.Maximum = comics.Count;
            thread.Start();
        }

        private void run()
        {
            foreach (Comic c in comics)
            {
                if(!c.Valid)
                {
                    c.GenerateThumbnail();
                }

                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Value += 1;
                });

                if(done)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        this.Close();
                    });
                    return;
                }
            }

            if(!done)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    this.Close();
                    Finished();
                });
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            done = true;
        }
    }
}
