using System;
using System.Collections.Generic;

namespace ComicBrowser
{
    class Comic
    {
        public HashSet<string> Tags { get; private set; }
        public string File { get; set; }
        public int Issue { get; set; }

        public Comic(string file) : this(file, -1, new HashSet<string>()) { }

        public Comic(string file, int issue, HashSet<string> tags)
        {
            this.File = file;
            this.Issue = issue;
            this.Tags = tags;
        }
    }
}
