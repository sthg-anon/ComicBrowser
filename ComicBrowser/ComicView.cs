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

        #region Constants
        private const int INITIAL_SPACER = 40;
        private const int INITIAL_THUMBNAIL_WIDTH = 200;
        private const int INITIAL_THUMBNAIL_HEIGHT = 300;

        private const int TRACKBAR_WIDTH = 230;
        private const int TRACKBAR_START = 5;

        private const int RESIZE_TRACKBAR_MAX = 10;
        private const int RESIZE_TRACKBAR_MIN = 1;

        private const int SPACER_RESIZE_STEP = (INITIAL_SPACER / TRACKBAR_START);
        private const int THUMBNAIL_WIDTH_RESIZE_STEP = (INITIAL_THUMBNAIL_WIDTH / TRACKBAR_START);
        private const int THUMBNAIL_HEIGHT_RESIZE_STEP = (INITIAL_THUMBNAIL_HEIGHT / TRACKBAR_START);

        private const int SMALL_CHANGE = 50;
        private const int LARGE_CHANGE = 300;

        private const int SCROLLBAR_WIDTH = 20;

        private const int SCROLL_BOTTOM_BACKPEDAL_CONST = 3;

        private const int TOOLTIP_AUTO_POP_DELAY = 5000;
        private const int TOOLTIP_INITIAL_DELAY = 1000;
        private const int TOOLTIP_RESHOW_DELAY = 500;
        private const bool TOOLTIP_SHOW_ALWAYS = true;

        private const int TRACKBARS_SPACER = 15;
        #endregion

        private readonly ScrollBar scrollbar = new VScrollBar();
        private readonly TrackBar spacerTrackbar = new TrackBar();
        private readonly TrackBar sizeTrackbar = new TrackBar();
        private readonly Panel panel;

        private int width = 0;
        private int height = 0;
        private int rows = 0;
        private int columns = 0;

        private int spacerWidth = INITIAL_SPACER;
        private int spacerHeight = INITIAL_SPACER;

        private static int thumbnailWidth = 200;
        private static int thumbnailHeight = 300;

        private int priorSpacerTrackbarValue = TRACKBAR_START;
        private int priorSizeTrackbarValue = TRACKBAR_START;

        private Control[] thumbnailBoxes;
        private CBXml cbxml;

        public ComicView(Panel panel, Panel controlPanel)
        {
            this.panel = panel;

            #region Controls
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
            //spacerTrackbar
            spacerTrackbar.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            spacerTrackbar.Size = new Size(TRACKBAR_WIDTH, controlPanel.Height);
            spacerTrackbar.Location = new Point(controlPanel.Width - spacerTrackbar.Size.Width, 0);
            spacerTrackbar.Name = "spacerTrackbar";
            spacerTrackbar.TabIndex = 2;
            spacerTrackbar.MouseUp += onSpacerTrackbarMove;
            spacerTrackbar.Value = TRACKBAR_START;
            controlPanel.Controls.Add(spacerTrackbar);

            //sizeTrackbar
            sizeTrackbar.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            sizeTrackbar.Size = new Size(TRACKBAR_WIDTH, controlPanel.Height);
            sizeTrackbar.Location = new Point(controlPanel.Width - spacerTrackbar.Size.Width - TRACKBARS_SPACER - sizeTrackbar.Size.Width);
            sizeTrackbar.Name = "sizeTrackbar";
            sizeTrackbar.TabIndex = 3;
            sizeTrackbar.MouseUp += onSizeTrackbarMove;
            sizeTrackbar.Value = TRACKBAR_START;
            controlPanel.Controls.Add(sizeTrackbar);
            sizeTrackbar.Minimum = RESIZE_TRACKBAR_MIN;
            sizeTrackbar.Maximum = RESIZE_TRACKBAR_MAX;

            //--tooltip--
            ToolTip trackbarToolTip = new ToolTip();
            trackbarToolTip.AutomaticDelay = TOOLTIP_AUTO_POP_DELAY;
            trackbarToolTip.InitialDelay = TOOLTIP_INITIAL_DELAY;
            trackbarToolTip.ReshowDelay = TOOLTIP_RESHOW_DELAY;
            trackbarToolTip.ShowAlways = TOOLTIP_SHOW_ALWAYS;
            trackbarToolTip.SetToolTip(spacerTrackbar, "Change the spacing between the thumbnails");
            trackbarToolTip.SetToolTip(sizeTrackbar, "Change the size of the thumbnails");
            #endregion
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
                WorkWindow<Comic> tgpw = new WorkWindow<Comic>(cbxml.Comics, (c) => 
                { 
                    if(!c.Valid)
                    {
                        c.GenerateThumbnail();
                    }
                });
                tgpw.Finished += OnPanelResized;
                tgpw.Text = "Generating thumbnails...";
                tgpw.Show();
                tgpw.Start();
                sizeTrackbar.Enabled = false;
                spacerTrackbar.Enabled = false;
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
            sizeTrackbar.Enabled = true;
            spacerTrackbar.Enabled = true;

            this.width = panel.Width - SCROLLBAR_WIDTH;
            this.height = panel.Height;

            this.columns = (int) Math.Floor((double)(this.width - spacerWidth) / (spacerWidth + thumbnailWidth));
            this.rows = (int)Math.Ceiling((double)cbxml.Comics.Count / columns);

            int visibleRows = (int)Math.Floor((double)(this.height - spacerHeight) / (spacerHeight + thumbnailHeight));
            if (rows <= visibleRows)
            {
                scrollbar.Enabled = false;
            }
            else
            {
                scrollbar.Enabled = true;
                scrollbar.Maximum = (rows * thumbnailHeight) + ((rows - SCROLL_BOTTOM_BACKPEDAL_CONST) * spacerHeight);
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
                pictureBox.Width = thumbnailWidth;
                pictureBox.Height = thumbnailHeight;
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
                    x += thumbnailWidth + spacerWidth;
                }
                y += thumbnailHeight + spacerHeight;
            }
        }

        private void onSpacerTrackbarMove(object sender, EventArgs e)
        {
            if(spacerTrackbar.Value == priorSpacerTrackbarValue)
            {
                return;
            }

            priorSpacerTrackbarValue = spacerTrackbar.Value;
            spacerHeight = spacerTrackbar.Value * SPACER_RESIZE_STEP;
            spacerWidth = spacerTrackbar.Value * SPACER_RESIZE_STEP;
            OnPanelResized();
        }

        private void onSizeTrackbarMove(object sender, EventArgs e)
        {
            if(sizeTrackbar.Value == priorSizeTrackbarValue)
            {
                return;
            }
            priorSizeTrackbarValue = sizeTrackbar.Value;
            thumbnailHeight = sizeTrackbar.Value * THUMBNAIL_HEIGHT_RESIZE_STEP;
            thumbnailWidth = sizeTrackbar.Value * THUMBNAIL_WIDTH_RESIZE_STEP;

            WorkWindow<Comic> tgpw = new WorkWindow<Comic>(cbxml.Comics, (c) =>
            {
                c.GenerateThumbnail();
            });
            tgpw.Text = "Regenerating thumbnails...";
            tgpw.Finished += OnPanelResized;
            tgpw.Show();
            tgpw.Start();
            sizeTrackbar.Enabled = false;
            spacerTrackbar.Enabled = false;
        }

        public static int ThumbnailWidth()
        {
            return thumbnailWidth;
        }

        public static int ThumbnailHeight()
        {
            return thumbnailHeight;
        }

        public static int maxThumbnailWidth()
        {
            return RESIZE_TRACKBAR_MAX * THUMBNAIL_WIDTH_RESIZE_STEP;
        }

        public static int maxThumbnailHeight()
        {
            return RESIZE_TRACKBAR_MAX * THUMBNAIL_HEIGHT_RESIZE_STEP;
        }
    }
}
