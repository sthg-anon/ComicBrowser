using System;
using System.Collections.Generic;

namespace ComicBrowser
{
    class Comic
    {
        public HashSet<string> Tags { get; private set; }
        public string File { get; set; }
        public int Issue { get; set; }

        private readonly string dir;

        public Comic(string file, string dir) : this(file, dir, -1, new HashSet<string>()) { }

        public Comic(string file, string dir, int issue, HashSet<string> tags)
        {
            this.File = file;
            this.Issue = issue;
            this.Tags = tags;
            this.dir = dir;
        }

        public string AbsolutePath()
        {
            return System.IO.Path.Combine(dir, File);
        }
    }
}
