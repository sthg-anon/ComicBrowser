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

        private const int SPACER_RESIZE_STEP = 8;

        private const int SMALL_CHANGE = 50;
        private const int LARGE_CHANGE = 300;

        private const int SCROLLBAR_WIDTH = 20;

        private const int SCROLL_BOTTOM_BACKPEDAL_CONST = 3;

        private const int TOOLTIP_AUTO_POP_DELAY = 5000;
        private const int TOOLTIP_INITIAL_DELAY = 1000;
        private const int TOOLTIP_RESHOW_DELAY = 500;
        private const bool TOOLTIP_SHOW_ALWAYS = true;

        private const int TRACKBAR_WIDTH = 230;
        private const int TRACKBAR_START = 5;

        private readonly ScrollBar scrollbar = new VScrollBar();
        private readonly TrackBar trackbar = new TrackBar();
        private readonly Panel panel;

        private int width = 0;
        private int height = 0;
        private int rows = 0;
        private int columns = 0;

        private int spacerWidth = 40;
        private int spacerHeight = 40;

        private int priorTrackbarValue = TRACKBAR_START;

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
            trackbar.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            trackbar.Size = new Size(TRACKBAR_WIDTH, controlPanel.Height);
            trackbar.Location = new Point(controlPanel.Width - trackbar.Size.Width, 0);
            trackbar.Name = "trackbar";
            trackbar.TabIndex = 2;
            trackbar.MouseUp += onTrackbarMove;
            trackbar.Scroll += onTrackbarMove;
            trackbar.Value = TRACKBAR_START;
            controlPanel.Controls.Add(trackbar);

            //--tooltip--
            ToolTip trackbarToolTip = new ToolTip();
            trackbarToolTip.AutomaticDelay = TOOLTIP_AUTO_POP_DELAY;
            trackbarToolTip.InitialDelay = TOOLTIP_INITIAL_DELAY;
            trackbarToolTip.ReshowDelay = TOOLTIP_RESHOW_DELAY;
            trackbarToolTip.ShowAlways = TOOLTIP_SHOW_ALWAYS;
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

            this.columns = (int) Math.Floor((double)(this.width - spacerWidth) / (spacerWidth + THUMBNAIL_WIDTH));
            this.rows = (int)Math.Ceiling((double)cbxml.Comics.Count / columns);

            int visibleRows = (int)Math.Floor((double)(this.height - spacerHeight) / (spacerHeight + THUMBNAIL_HEIGHT));
            if (rows <= visibleRows)
            {
                scrollbar.Enabled = false;
            }
            else
            {
                scrollbar.Enabled = true;
                scrollbar.Maximum = (rows * THUMBNAIL_HEIGHT) + ((rows - SCROLL_BOTTOM_BACKPEDAL_CONST) * spacerHeight);
            }

            if(thumbnailBoxes != null)
            {
                foreach(Control c in thumbnailBoxes)
                {
                    panel.Controls.Remove(c);
                }
            }

            thumbnailBoxes = new Control[cbxml.Comics.Count];

            ToolTip toolTip = new ToolTip();
            toolTip.AutoPopDelay = TOOLTIP_AUTO_POP_DELAY;
            toolTip.InitialDelay = TOOLTIP_INITIAL_DELAY;
            toolTip.ReshowDelay = TOOLTIP_RESHOW_DELAY;
            toolTip.ShowAlways = TOOLTIP_SHOW_ALWAYS;

            iterateAnd((x, y, index) => 
            {
                Image thumbnail = cbxml.Comics[index].Thumbnail;

                PictureBox pictureBox = new PictureBox();
                pictureBox.Width = THUMBNAIL_WIDTH;
                pictureBox.Height = THUMBNAIL_HEIGHT;
                pictureBox.Image = thumbnail;
                pictureBox.Location = new Point(x, y);
                pictureBox.Cursor = Cursors.Hand;
                toolTip.SetToolTip(pictureBox, cbxml.Comics[index].File);
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
            int y = spacerHeight;
            for (int row = 0; row < rows; row++)
            {
                int x = spacerWidth;
                for (int column = 0; column < columns; column++)
                {
                    int index = (row * columns) + column;

                    if (index >= cbxml.Comics.Count)
                    {
                        column = columns;//this makes the outer loop false so it can be broken out of
                        break;
                    }

                    gid(x, y, index);
                    x += THUMBNAIL_WIDTH + spacerWidth;
                }
                y += THUMBNAIL_HEIGHT + spacerHeight;
            }
        }

        private void onTrackbarMove(object sender, EventArgs e)
        {
            if(trackbar.Value == priorTrackbarValue)
            {
                return;
            }

            priorTrackbarValue = trackbar.Value;
            spacerHeight = trackbar.Value * SPACER_RESIZE_STEP;
            spacerWidth = trackbar.Value * SPACER_RESIZE_STEP;
            OnPanelResized();
        }
    }
}
