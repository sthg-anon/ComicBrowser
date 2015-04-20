using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace ComicBrowser
{
    public partial class WorkWindow<T> : Form
    {
        public delegate void finishedDelegate();
        public delegate void DoWork(T item);
        public event finishedDelegate Finished;

        private readonly List<T> items;
        private readonly DoWork worker;
        private readonly Form parent;

        private volatile bool done = false;

        internal WorkWindow(Form parent, List<T> items, DoWork worker)
        {
            InitializeComponent();

            this.worker = worker;
            this.items = new List<T>(items);
            this.parent = parent;
        }

        public void Start()
        {
            Thread thread = new Thread(run);
            progressBar1.Minimum = 0;
            progressBar1.Maximum = items.Count;
            thread.Start();

            parent.Enabled = false;
        }

        private void run()
        {
            foreach (T c in items)
            {
                worker(c);

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

        private void onFormClosed(object sender, FormClosedEventArgs e)
        {
            parent.Enabled = true;
        }
    }
}
