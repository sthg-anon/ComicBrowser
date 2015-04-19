using System;
using System.Drawing;
using System.Windows.Forms;

namespace ComicBrowser
{
    class ComicView
    {
        public delegate void comicClickDelegate(Comic comic);
        private delegate void gridIteratorDelegate(int x, int y, int index);

        public event comicClickDelegate ComicClicked;

        internal const int THUMBNAIL_WIDTH = 200;
        internal const int THUMBNAIL_HEIGHT = 300;

        private const int SPACER_WIDTH = 40;
        private const int SPACER_HEIGHT = 40;

        private const int SMALL_CHANGE = 50;
        private const int LARGE_CHANGE = 300;

        private const int SCROLLBAR_WIDTH = 20;

        private const int SCROLL_BOTTOM_BACKPEDAL_CONST = 3;

        private readonly ScrollBar scrollbar = new VScrollBar();
        private readonly Panel panel;

        private int width = 0;
        private int height = 0;
        private int rows = 0;
        private int columns = 0;

        private Control[] thumbnailBoxes;
        private CBXml cbxml;

        public ComicView(Panel panel, Panel controlPanel)
        {
            this.panel = panel;

            //--panel--
            //scrollbar
            scrollbar.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Right)));
            scrollbar.Name = "vScrollBar";
            scrollbar.TabIndex = 0;
            scrollbar.Location = new Point(panel.Width - SCROLLBAR_WIDTH, 0);
            scrollbar.Size = new Size(SCROLLBAR_WIDTH, panel.Height);
            scrollbar.SmallChange = SMALL_CHANGE;
            scrollbar.LargeChange = LARGE_CHANGE;
            scrollbar.Scroll += onScroll;

            panel.Controls.Add(scrollbar);

            //panel
            panel.MouseEnter += (sender, e) => scrollbar.Focus();

            //--control panel--
            TrackBar trackbar = new TrackBar();
            trackbar.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            trackbar.Location = new Point(609, 3);
            trackbar.Name = "trackbar";
            trackbar.Size = new Size(230, 45);
            trackbar.TabIndex = 2;

            controlPanel.Controls.Add(trackbar);

            ToolTip trackbarToolTip = new ToolTip();
            trackbarToolTip.AutomaticDelay = 5000;
            trackbarToolTip.InitialDelay = 1000;
            trackbarToolTip.ReshowDelay = 500;
            trackbarToolTip.ShowAlways = true;
            trackbarToolTip.SetToolTip(trackbar, "Change the spacing between the thumbnails");
        }

        public void SetView(CBXml cbxml)
        {
            if(this.cbxml != null)
            {
                foreach(Control c in thumbnailBoxes)
                {
                    panel.Controls.Remove(c);
                    c.Dispose();
                }
            }

            this.cbxml = cbxml;

            if(!cbxml.ThumbnailsGenerated)
            {
                ThumbnailGeneratorProgressWindow tgpw = new ThumbnailGeneratorProgressWindow(cbxml);
                tgpw.Finished += OnPanelResized;
                tgpw.Show();
                tgpw.Start();
            }
            else
            {
                OnPanelResized();
            }
        }

        public void OnPanelResized()
        {
            //columns vertical, rows horizontal
            scrollbar.Value = scrollbar.Minimum;
            cbxml.ThumbnailsGenerated = true;

            this.width = panel.Width - SCROLLBAR_WIDTH;
            this.height = panel.Height;

            this.columns = (int) Math.Floor((double)(this.width - SPACER_WIDTH) / (SPACER_WIDTH + THUMBNAIL_WIDTH));
            this.rows = (int)Math.Ceiling((double)cbxml.Comics.Count / columns);

            int visibleRows = (int)Math.Floor((double)(this.height - SPACER_HEIGHT) / (SPACER_HEIGHT + THUMBNAIL_HEIGHT));
            if (rows <= visibleRows)
            {
                scrollbar.Enabled = false;
            }
            else
            {
                scrollbar.Enabled = true;
                scrollbar.Maximum = (rows * THUMBNAIL_HEIGHT) + ((rows - SCROLL_BOTTOM_BACKPEDAL_CONST) * SPACER_HEIGHT);
            }

            if(thumbnailBoxes != null)
            {
                foreach(Control c in thumbnailBoxes)
                {
                    panel.Controls.Remove(c);
                }
            }

            thumbnailBoxes = new Control[cbxml.Comics.Count];

            iterateAnd((x, y, index) => 
            {
                Image thumbnail = cbxml.Comics[index].Thumbnail;

                PictureBox pictureBox = new PictureBox();
                pictureBox.Width = THUMBNAIL_WIDTH;
                pictureBox.Height = THUMBNAIL_HEIGHT;
                pictureBox.Image = thumbnail;
                pictureBox.Location = new Point(x, y);
                pictureBox.Cursor = Cursors.Hand;
                pictureBox.Click += (sender, e) => ComicClicked(cbxml.Comics[index]);
                thumbnailBoxes[index] = pictureBox;
            });

            panel.Controls.AddRange(thumbnailBoxes);
        }

        private void onScroll(object sender, ScrollEventArgs e)
        {
            int heightOffset = scrollbar.Value;
            iterateAnd((x, y, index) => thumbnailBoxes[index].Location = new Point(x, y - heightOffset));
        }

        private void iterateAnd(gridIteratorDelegate gid)
        {
            int y = SPACER_HEIGHT;
            for (int row = 0; row < rows; row++)
            {
                int x = SPACER_WIDTH;
                for (int column = 0; column < columns; column++)
                {
                    int index = (row * columns) + column;

                    if (index >= cbxml.Comics.Count)
                    {
                        column = columns;//this makes the outer loop false so it can be broken out of
                        break;
                    }

                    gid(x, y, index);
                    x += THUMBNAIL_WIDTH + SPACER_WIDTH;
                }
                y += THUMBNAIL_HEIGHT + SPACER_HEIGHT;
            }
        }
    }
}
