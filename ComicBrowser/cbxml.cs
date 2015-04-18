using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace ComicBrowser
{
    class CBXml
    {
        internal const string CBXML_EXTENSION = ".cbxml";
        internal const string DEFAULT_CBXML = "comics" + CBXML_EXTENSION;
        internal const string ROOT_NODE_NAME = "root";

        private readonly bool isRoot;
        private string file;
        private readonly string directory;
         //file name relative to directory, comic
        private Dictionary<string, Comic> comics;

                                  //folder, child xml
        public Dictionary<string, CBXml> ChildXMLs { get; private set; }
        public bool Valid { get; private set; }

        public CBXml() : this(Directory.GetCurrentDirectory(), true) { }

        public CBXml(string directory, bool root)
        {
            this.isRoot = root;
            //this.file = inputFile;//inputFile.Equals(String.Empty) ? DEFAULT_CBXML : inputFile;
            this.directory = directory;
            this.Valid = false;
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
                    string issueStr = node.SelectSingleNode("issue").InnerText;
                    bool result = int.TryParse(issueStr, out issue);
                    if (!result)
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

            foreach(string f in files)
            {
                string fileName = Path.GetFileName(f);

                if (!ComicFileTypeExtensions.Matches(fileName) || comics.ContainsKey(fileName))
                {
                    continue;
                }
                comics.Add(fileName, new Comic(fileName));
            }
        }

        public void Save()
        {
            using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                XmlTextWriter writer = new XmlTextWriter(fs, System.Text.Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("comics");
                foreach (Comic comic in comics.Values)
                {
                    writer.WriteStartElement("comic");
                    writer.WriteElementString("file", comic.File);

                    if(comic.Issue >= 0)
                    {
                        writer.WriteElementString("issue", comic.Issue.ToString());
                    }

                    if(comic.Tags.Count > 0)
                    {
                        writer.WriteStartElement("tags");
                        foreach(string tag in comic.Tags)
                        {
                            writer.WriteElementString("tag", tag);
                        }
                        writer.WriteEndElement();//"tags"
                    }
                    writer.WriteEndElement();//"comic"
                }
                writer.WriteEndElement();//"comics"
                writer.WriteEndDocument();
                writer.Flush();
            }

            foreach(CBXml child in ChildXMLs.Values)
            {
                child.Save();
            }
        }

        public void Open(string inputFile, bool create)
        {
            //Console.WriteLine("Input file pre: {0}\nEmpty pre: {1}", inputFile, inputFile.Equals(String.Empty));
            this.file = inputFile.Equals(String.Empty) ? findCBXML(directory) : inputFile;
            //Console.WriteLine("!create: {0}\nInput file: {1}\nEmpty:{2}", !create, inputFile, file.Equals(String.Empty));
            if (!create && (file.Equals(String.Empty) || !File.Exists(file)))
            {
                //this.comics = new Dictionary<string, Comic>();
                //throw new FileNotFoundException("File not found!", file);
                //Console.WriteLine("invalid!");
                this.Valid = false;
                return;
            }
            else if (create && !File.Exists(file))
            {
                this.comics = new Dictionary<string, Comic>();
            }
            else
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(fs);//TODO: catch exception

                    this.comics = read(xml);
                }
            }



            // printComics();
            loadNewFiles();

            ChildXMLs = loadChildren();
            //Console.WriteLine("now valid!");
            this.Valid = true;
        }

        public void PrintTree(int level)
        {
            for (int ii = 0; ii < level; ii++)
            {
                Console.Write(" ");
            }
            Console.WriteLine("{0}: ", directory);
            foreach(CBXml xml in ChildXMLs.Values)
            {
                xml.PrintTree(level + 4);
            }
        }

        private Dictionary<string, CBXml> loadChildren()
        {
            Dictionary<string, CBXml> children = new Dictionary<string, CBXml>();
            string[] directories = Directory.GetDirectories(directory);

            foreach (string dir in directories)
            {
                string childPath = Path.Combine(directory, dir);
                Console.WriteLine("Made child path: {0}", childPath);
                string cbxml = getChildCbxmlFileName(childPath);
                if (!cbxml.Equals(String.Empty))
                {
                    string dirName = new DirectoryInfo(dir).Name;
                    Console.WriteLine("Dirname: {0}", dirName);
                    CBXml child = new CBXml(dir, false);
                    child.Open(cbxml, true);
                    if (child.Valid)
                    {
                        children.Add(dirName, child);
                    }
                }
            }

            return children;
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

        private static string getChildCbxmlFileName(string childPath)
        {
            string cbxml = findCBXML(childPath);
            if (!cbxml.Equals(String.Empty))
            {
                return cbxml;
            }

            string[] files = Directory.GetFiles(childPath);
            foreach (string childFile in files)
            {
                if (ComicFileTypeExtensions.Matches(childFile))
                {
                    return Path.Combine(childPath, DEFAULT_CBXML);
                }
            }

            return String.Empty;
        }

        /// <summary>
        /// Locates the .cbxml file in the working direcctory, if there is ONE.
        /// If there is more than one .cbxml file, then neither will be returned, due to the
        /// ambiguity.
        /// 
        /// If the user inputted the CBXML file as the first argument, it will also be returned by this function.
        /// </summary>
        /// <returns>The path to the .cbxml file, if there is one.</returns>
        private static string findCBXML(string currentFolder)
        {
            Console.WriteLine("Current folder: {0}", currentFolder);
            //string currentFolder = Directory.GetCurrentDirectory();
            string[] files = Directory.GetFiles(currentFolder);

            string cbXML = String.Empty;

            for (int ii = 0; ii < files.Length; ii++)
            {
                if (FileUtils.IsDirectory(files[ii]) || !CBXml.FileExtensionMatches(files[ii]))
                {
                    continue;
                }

                if (!cbXML.Equals(String.Empty))
                {
                    return String.Empty;
                }

                cbXML = files[ii];
            }

            //Console.WriteLine("Located cbxml: {0}\nEmpty: {1}", cbXML, cbXML.Equals(String.Empty));
            return cbXML;
        }

        public static string getFileFilter()
        {
            return String.Format("Comic File Library (*{0})|*{1};", CBXML_EXTENSION, CBXML_EXTENSION);
        }

        public TreeNode GetNode()
        {
            TreeNode node = null;
            string dirName = new DirectoryInfo(directory).Name;
            if(ChildXMLs.Count == 0)
            {
                node = new TreeNode(dirName);
            }
            else
            {
                TreeNode[] nodes = new TreeNode[ChildXMLs.Count];
                int index = 0;
                foreach (CBXml child in ChildXMLs.Values)
                {
                    nodes[index] = child.GetNode();
                    index++;
                }

                node = new TreeNode(isRoot ? ROOT_NODE_NAME : dirName, nodes);
            }

            node.AddPairing(this);

            return node;
        }
    }
}
