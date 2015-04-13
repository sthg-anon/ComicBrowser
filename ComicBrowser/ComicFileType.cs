using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicBrowser
{
    enum ComicFileType
    {
        CBR,
        CBZ
    }

    static class ComicFileTypeExtensions
    {
        public static string ToString(this ComicFileType type)
        {
            switch(type)
            {
                case ComicFileType.CBR:
                    return ".cbr";
                case ComicFileType.CBZ:
                    return ".cbz";
                default:
                    return String.Empty;
            }
        }
    }
}
