using System;
using System.Drawing;
using System.Windows.Forms;

namespace ComicBrowser
{
    class ComicView
    {
        public delegate void comicClickDelegate(Comic comic);

        public event comicClickDelegate ComicClicked;

        internal const int THUMBNAIL_WIDTH = 100;
        internal const int THUMBNAIL_HEIGHT = 150;

        private const int SCROLLBAR_WIDTH = 20;
        private const int WIDTH_SPACER = 40;
        private const int HEIGHT_SPACER = 40;

        private readonly Panel panel;

        private int width;
        private int height;

        private readonly ScrollBar scrollbar = new VScrollBar();

        private int tabIndex = 0;
        private Control[] thumbnailBoxes;
        private CBXml cbxml;

        public ComicView(Panel panel)
        {
            this.panel = panel;

            //scrollbar
            scrollbar.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Right)));
            scrollbar.Name = "vScrollBar";
            scrollbar.TabIndex = ++tabIndex;
            panel.Controls.Add(scrollbar);
            scrollbar.Location = new Point(panel.Width - SCROLLBAR_WIDTH, 0);
            scrollbar.Size = new Size(SCROLLBAR_WIDTH, panel.Height);
            scrollbar.Minimum = 0;

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

            OnPanelResized();
        }

        public void OnPanelResized()
        {
            Console.WriteLine("Comics count: {0}", cbxml.Comics.Count);
            //columns vertical, rows horizontal
            this.width = panel.Width - SCROLLBAR_WIDTH;
            this.height = panel.Height;

            int columns = (int) Math.Floor((double)(this.width - WIDTH_SPACER) / (WIDTH_SPACER + THUMBNAIL_WIDTH));
            int rows = (int)Math.Ceiling((double)cbxml.Comics.Count / columns);

            Console.WriteLine("Columns: {0}\nRows: {1}", columns, rows);

            int visibleRows = (int)Math.Ceiling((double)(this.height - HEIGHT_SPACER) / (HEIGHT_SPACER + THUMBNAIL_HEIGHT));
            if (rows <= visibleRows)
            {
                scrollbar.Enabled = false;
            }
            else
            {
                scrollbar.Enabled = true;;
            }

            if(thumbnailBoxes != null)
            {
                foreach(Control c in thumbnailBoxes)
                {
                    panel.Controls.Remove(c);
                }
            }

            thumbnailBoxes = new Control[cbxml.Comics.Count];
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

                    Image thumbnail = cbxml.Comics[index].Thumbnail;

                    PictureBox pictureBox = new PictureBox();
                    pictureBox.Width = THUMBNAIL_WIDTH;
                    pictureBox.Height = THUMBNAIL_HEIGHT;
                    pictureBox.Image = thumbnail;
                    pictureBox.Location = new Point(x, y);
                    pictureBox.Cursor = Cursors.Hand;
                    pictureBox.Click += (sender, e) => ComicClicked(cbxml.Comics[index]);
                    thumbnailBoxes[index] = pictureBox;

                    x += THUMBNAIL_WIDTH + WIDTH_SPACER;
                }
                y += THUMBNAIL_HEIGHT + HEIGHT_SPACER;
            }
            panel.Controls.AddRange(thumbnailBoxes);
        }
    }
}
