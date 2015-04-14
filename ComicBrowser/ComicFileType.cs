using System;
using System.Collections.Generic;
using System.IO;
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
        public static string StringValue(this ComicFileType type)
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

        public static bool Matches(string file)
        {
            string extension = Path.GetExtension(file).ToLower();

            foreach(ComicFileType type in Enum.GetValues(typeof(ComicFileType)))
            {
                if (extension.Equals(type.StringValue()))
                {
                    return true;
                }
            }

            return false;
        }

        //public static string GetFilter()
        //{
        //    StringBuilder sb = new StringBuilder("Comic Files (");
        //    foreach (ComicFileType type in Enum.GetValues(typeof(ComicFileType)))
        //    {
        //        string extension = type.StringValue();
        //        sb.Append("*").Append(extension).Append(", ");
        //    }
        //    sb.Length -= 2;
        //    sb.Append(")|");
        //    foreach (ComicFileType type in Enum.GetValues(typeof(ComicFileType)))
        //    {
        //        string extension = type.StringValue();
        //        sb.Append("*").Append(extension).Append(";");
        //    }
        //    sb.Length -= 1;
        //    return sb.ToString();
        //}
    }
}
