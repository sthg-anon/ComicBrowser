using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using SharpCompress.Archive;

namespace ComicBrowser
{
    class Comic
    {

        public HashSet<string> Tags { get; private set; }
        public string File { get; set; }
        public int Issue { get; set; }
        public Image[] Images { get; set; }

        private volatile Image originalCoverImage = null;
        private volatile Image _thumbnail;
        private volatile bool _valid;

        public Image Thumbnail
        {
            get
            {
                return _thumbnail;
            }
        }

        public bool Valid
        {
            get
            {
                return _valid;
            }
        }

        internal Image OriginalCoverImage
        {
            get
            {
                return originalCoverImage;
            }
        }

        private readonly string dir;

        public Comic(string file, string dir) : this(file, dir, -1, new HashSet<string>()) { }

        public Comic(string file, string dir, int issue, HashSet<string> tags)
        {
            this.File = file;
            this.Issue = issue;
            this.Tags = tags;
            this.dir = dir;
            this._valid = false;
        }

        public string AbsolutePath()
        {
            return System.IO.Path.Combine(dir, File);
        }

        private Image getThumbnail()
        {
            if(originalCoverImage == null)
            {
                using (var archive = ArchiveFactory.Open(AbsolutePath()))
                {
                    List<IArchiveEntry> entries = new List<IArchiveEntry>(archive.Entries);
                    if (entries.Count <= 0)
                    {
                        return null;
                    }

                    entries = entries.OrderBy(e => e.FilePath).ToList();

                    IArchiveEntry thumbnailEntry = null;
                    for (int ii = 0; ii < entries.Count; ii++)
                    {
                        if (ImageFileTypeExtensions.Matches(entries[ii].FilePath))
                        {
                            thumbnailEntry = entries[ii];
                            break;
                        }
                    }

                    if (thumbnailEntry == null)
                    {
                        return null;
                    }

                    using (MemoryStream stream = new MemoryStream())
                    {
                        thumbnailEntry.WriteTo(stream);
                        originalCoverImage = Image.FromStream(stream);
                    }
                    originalCoverImage = scaleImage(originalCoverImage, ComicView.maxThumbnailWidth(), ComicView.maxThumbnailHeight());
                }
            }
            Image thumbnail = scaleImage(originalCoverImage, ComicView.ThumbnailWidth(), ComicView.ThumbnailHeight());
            return thumbnail;
        }

        public void GenerateThumbnail()
        {
            _thumbnail = getThumbnail();
            this._valid = this.Thumbnail != null;
        }

        private static Image scaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            return newImage;
        }
    }
}
