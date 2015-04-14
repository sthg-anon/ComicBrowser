using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ComicBrowser
{
    class CBXml
    {
        private const string CBXML_EXTENSION = ".xml";

        private readonly string directory;
         //file name relative to directory, comic
        private readonly Dictionary<string, Comic> comics;
                                  //folder, child xml
        private readonly Dictionary<string, CBXml> childXMLs = new Dictionary<string, CBXml>();

        public CBXml(string file)
        {
            bool isNewFile = !File.Exists(file);

            if (!isNewFile && FileUtils.IsDirectory(file))
            {
                throw new FileNotFoundException(String.Format("{0} is not a file!", file));
            }

            directory = new FileInfo(file).Directory.FullName;

            if (isNewFile)
            {
                this.comics = new Dictionary<string, Comic>();
            }
            else
            {
                using(FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(fs);//TODO: catch exception

                    this.comics = read(xml);

                }
            }

            printComics();
            loadNewFiles();
            Save(file);
        }

        private Dictionary<string, Comic> read(XmlDocument xml)
        {
            Dictionary<string, Comic> dic = new Dictionary<string, Comic>();
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
                        //Bad data! (The issue was not an int)
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

                dic.Add(fileName, new Comic(fileName, issue, tags));
            }
            return dic;
        }

        private void loadNewFiles()
        {
            string[] files = Directory.GetFiles(directory);

            foreach(string file in files)
            {
                string fileName = Path.GetFileName(file);

                if (!ComicFileTypeExtensions.Matches(fileName) || comics.ContainsKey(fileName))
                {
                    continue;
                }
                comics.Add(fileName, new Comic(fileName));
            }
        }

        public void Save(string file)
        {
            using(FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                XmlTextWriter writer = new XmlTextWriter(fs, System.Text.Encoding.Default);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("comics");
                foreach(Comic comic in comics.Values)
                {
                    writer.WriteStartElement("comic");
                    writer.WriteElementString("file", comic.File);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
            }
        }

        private void printComics()
        {
            foreach (KeyValuePair<string, Comic> entry in comics)
            {
                Console.WriteLine("Comic: {0}", entry.Key);
                Comic comic = entry.Value;
                Console.WriteLine("  file: {0}\n  issue: {1}\n  tags:", comic.File, comic.Issue);
                HashSet<string> tags = comic.Tags;
                foreach (string tag in tags)
                {
                    Console.WriteLine("    tag: {0}", tag);
                }
            }
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
