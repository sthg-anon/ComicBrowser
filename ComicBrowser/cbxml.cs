using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ComicBrowser
{
    class CBXml
    {
        private const string CBXML_EXTENSION = ".cbxml";

        private readonly string directory;
        private readonly XmlDocument xml = new XmlDocument();
         //file name relative to directory, comic
        private readonly Dictionary<string, Comic> comics = new Dictionary<string, Comic>();

        public CBXml(string file)
        {
            if (FileUtils.IsDirectory(file))
            {
                throw new FileNotFoundException(String.Format("{0} is not a file!", file));
            }

            directory = new FileInfo(file).Directory.FullName;

            bool fileExists = File.Exists(file);

            FileStream fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Read);
            xml.Load(fs);//TODO: catch exception
            

            if(fileExists)
            {
                read();
            }

            foreach(KeyValuePair<string, Comic> entry in comics)
            {
                Console.WriteLine("Comic: {0}", entry.Key);
                Comic comic = entry.Value;
                Console.WriteLine("  file: {0}\n  issue: {1}\n  tags:", comic.File, comic.Issue);
                HashSet<string> tags = comic.Tags;
                foreach(string tag in tags)
                {
                    Console.WriteLine("    tag: {0}", tag);
                }
            }

            loadNewFiles();

            Save();
        }

        private void read()
        {
            XmlNodeList nodeList = xml.DocumentElement.SelectNodes("/comics/comic");
            foreach(XmlNode node in nodeList)
            {
                string fileName = node.SelectSingleNode("file").InnerText;

                if(!ComicFileTypeExtensions.Matches(fileName))
                {
                    //invalid entry! This entry won't get put in the map, so when the stuff is saved back,
                    //this data won't be included. In other words, this 'comic' entry gets deleted,
                    //so it will be re-written when the the update() method is called.
                    continue;
                }

                int issue = -1;
                if (node.SelectSingleNode("issue") != null)
                {
                    bool result = int.TryParse(node.SelectSingleNode("issue").InnerText, out issue);
                    if(!result)
                    {
                        //Bad data!
                        continue;
                    }
                }

                HashSet<string> tags = new HashSet<string>();
                XmlNodeList tagNodeList = node.SelectNodes("tags");
                if(nodeList != null)
                {
                    foreach(XmlNode tagNode in tagNodeList)
                    {
                        foreach(var child in tagNode.ChildNodes)
                        {
                            XmlElement element = (XmlElement)child;
                            tags.Add(element.InnerXml);
                        }
                    }
                }

                comics.Add(fileName, new Comic(fileName, issue, tags));
            }
        }

        private void loadNewFiles()
        {
            string[] files = Directory.GetFiles(directory);

            foreach(string file in files)
            {

            }
        }

        public void Save()
        {

        }

        public static bool FileExtensionMatches(string file)
        {
            string extention = Path.GetExtension(file);
            return extention.ToLower().Equals(CBXML_EXTENSION);
        }

        public static string GetFileExtension()
        {
            return CBXML_EXTENSION;
        }
    }
}
