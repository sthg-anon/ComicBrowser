using System.IO;

namespace ComicBrowser
{
    static class FileUtils
    {
        public static bool IsDirectory(string file)
        {
            FileAttributes attr = File.GetAttributes(file);
            return (attr & FileAttributes.Directory) == FileAttributes.Directory;
        }
    }
}
