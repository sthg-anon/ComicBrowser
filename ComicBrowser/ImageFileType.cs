using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicBrowser
{
    enum ImageFileType
    {
        BMP,
        GIF,
        JPEG,
        PNG,
        TIFF
    }

    static class ImageFileTypeExtensions
    {
        public static string[] StringValues(this ImageFileType type)
        {
            switch (type)
            {
                case ImageFileType.BMP:
                    return new string[] { ".bmp", ".dib" };
                case ImageFileType.GIF:
                    return new string[] { ".gif" };
                case ImageFileType.JPEG:
                    return new string[] { ".jpg", ".jpeg", ".jpe", ".jif", ".jfif", ".jfi" };
                case ImageFileType.PNG:
                    return new string[] { ".png" };
                case ImageFileType.TIFF:
                    return new string[] { ".tiff", "tif" };
                default:
                    return new string[] { String.Empty };
            }
        }

        public static bool Matches(string file)
        {
            string extension = Path.GetExtension(file).ToLower();

            foreach (ImageFileType type in Enum.GetValues(typeof(ImageFileType)))
            {
                foreach(string fileExtension in type.StringValues())
                {
                    if (extension.ToLower().Equals(fileExtension))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
