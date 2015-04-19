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

        private const int SCROLLBAR_WIDTH = 20;
        private const int WIDTH_SPACER = 40;
        private const int HEIGHT_SPACER = 40;

        private const int SMALL_CHANGE = 50;
        private const int LARGE_CHANGE = 300;

        private readonly ScrollBar scrollbar = new VScrollBar();
        private readonly Panel panel;

        private int width = 0;
        private int height = 0;
        private int rows = 0;
        private int columns = 0;

        private Control[] thumbnailBoxes;
        private CBXml cbxml;

        public ComicView(Panel panel)
        {
            this.panel = panel;

            //scrollbar
            scrollbar.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Right)));
            scrollbar.Name = "vScrollBar";
            scrollbar.TabIndex = 0;
            panel.Controls.Add(scrollbar);
            scrollbar.Location = new Point(panel.Width - SCROLLBAR_WIDTH, 0);
            scrollbar.Size = new Size(SCROLLBAR_WIDTH, panel.Height);
            scrollbar.SmallChange = SMALL_CHANGE;
            scrollbar.LargeChange = LARGE_CHANGE;
            scrollbar.Scroll += (sender, e) => onScroll();

            //panel
            panel.MouseEnter += (sender, e) => scrollbar.Focus();
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

            this.columns = (int) Math.Floor((double)(this.width - WIDTH_SPACER) / (WIDTH_SPACER + THUMBNAIL_WIDTH));
            this.rows = (int)Math.Ceiling((double)cbxml.Comics.Count / columns);

            int visibleRows = (int)Math.Floor((double)(this.height - HEIGHT_SPACER) / (HEIGHT_SPACER + THUMBNAIL_HEIGHT));
            if (rows <= visibleRows)
            {
                scrollbar.Enabled = false;
            }
            else
            {
                scrollbar.Enabled = true;
                scrollbar.Maximum = (rows * THUMBNAIL_HEIGHT) + (rows * HEIGHT_SPACER) - (3 * HEIGHT_SPACER);
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

        private void onScroll()
        {
            int heightOffset = scrollbar.Value;
            iterateAnd((x, y, index) => thumbnailBoxes[index].Location = new Point(x, y - heightOffset));
        }

        private void iterateAnd(gridIteratorDelegate gid)
        {
            int y = HEIGHT_SPACER;
            for (int row = 0; row < rows; row++)
            {
                int x = WIDTH_SPACER;
                for (int column = 0; column < columns; column++)
                {
                    int index = (row * columns) + column;

                    if (index >= cbxml.Comics.Count)
                    {
                        column = columns;//this makes the outer loop false so it can be broken out of
                        break;
                    }

                    gid(x, y, index);
                    x += THUMBNAIL_WIDTH + WIDTH_SPACER;
                }
                y += THUMBNAIL_HEIGHT + HEIGHT_SPACER;
            }
        }
    }
}
