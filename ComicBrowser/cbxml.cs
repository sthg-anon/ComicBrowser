using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace ComicBrowser
{
    class CBXml
    {
        internal const string CBXML_EXTENSION = ".cbxml";
        internal const string DEFAULT_CBXML = "comics" + CBXML_EXTENSION;

        private readonly bool isRoot;
        private string file;
        private readonly string directory;
         //file name relative to directory, comic
        private Dictionary<string, Comic> comicMap;
                                  //folder, child xml
        public Dictionary<string, CBXml> ChildXMLs { get; private set; }
        public bool ThumbnailsGenerated { get; set; }
        public readonly bool Valid;
        public List<Comic> Comics { get; private set; }

        //public CBXml(string file) : this(new FileInfo(file).Directory.FullName, true) { }
        /// <summary>
        /// Creates a CBXml file at the expected file location. If it happens that that file exists, cbxml will be valid.
        /// If the xml does not exist, then this instance will be invalid until it is saved.
        /// </summary>
        /// <param name="file">The expected location of the cbxml</param>
        public CBXml(string file, bool create) : this(file, true, create) { }

        /// <summary>
        /// Creates a cbxml file, and will try to intelligently locate the cbxml in the current directory
        /// </summary>
        public CBXml(bool create) : this(findCBXML(Directory.GetCurrentDirectory()), true, create) { }

        private CBXml(string file, bool root, bool create)
        {
            this.isRoot = root;

            if(file.Equals(String.Empty))
            {
                this.Valid = false;
            }
            else
            {
                this.file = file;
                this.directory = new FileInfo(file).Directory.FullName;

                this.Valid = open(create);
            }
        }

        private Dictionary<string, Comic> read(XmlDocument xml)
        {
            Dictionary<string, Comic> dic = new Dictionary<string, Comic>();
            XmlNodeList nodeList = xml.DocumentElement.SelectNodes("/comics/comic");
            foreach(XmlNode node in nodeList)
            {
                string fileName = node.SelectSingleNode("file").InnerText;

                if(!ComicFileTypeExtensions.Matches(fileName) || !File.Exists(fileName))
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

                dic.Add(fileName, new Comic(fileName, directory, issue, tags));
            }
            return dic;
        }

        private void loadNewFiles()
        {
            string[] files = Directory.GetFiles(directory);

            foreach(string f in files)
            {
                string fileName = Path.GetFileName(f);

                if (!ComicFileTypeExtensions.Matches(fileName) || comicMap.ContainsKey(fileName))
                {
                    continue;
                }
                comicMap.Add(fileName, new Comic(fileName, directory));
            }
        }

        public void Save()
        {
            if (file != null && file.Equals(string.Empty))
                return;

            using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                XmlTextWriter writer = new XmlTextWriter(fs, System.Text.Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("comics");
                foreach (Comic comic in comicMap.Values)
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

        /// <summary>
        /// Opens the file. Expects the file member variable to NOT be empty
        /// </summary>
        /// <param name="create">If the file should be created or not.</param>
        /// <returns>The vailidity of the cbxml after opening</returns>
        private bool open(bool create)
        {
            //Console.WriteLine("open!");
            //this.file = inputFile.Equals(String.Empty) ? findCBXML(directory) : inputFile;
            if (!create && !File.Exists(file))
            {
                return false;
            }
            else if (create && !File.Exists(file))
            {
                this.comicMap = new Dictionary<string, Comic>();
                //continues, this.valid = true; set at the end
            }
            else
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(fs);//TODO: catch exception

                    this.comicMap = read(xml);
                }
            }

            // printComics();
            loadNewFiles();

            ChildXMLs = loadChildren();
            //ChildXMLs = new Dictionary<string, CBXml>();
            MakeComicList();
            return true;
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
                string cbxmlFile = getChildCbxmlFileName(childPath);
                if (!cbxmlFile.Equals(String.Empty))
                {
                    CBXml child = new CBXml(cbxmlFile, false, true);
                    if (child.Valid)
                    {
                        children.Add(dir, child);
                    }
                }
            }

            return children;
        }

        private void printComics()
        {
            foreach (KeyValuePair<string, Comic> entry in comicMap)
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

        public TreeNode GetNode()
        {
            TreeNode node = null;
            string dirName = new DirectoryInfo(directory).Name;
            if (ChildXMLs.Count == 0)
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

                node = new TreeNode(dirName, nodes);
            }

            node.AddPairing(this);

            return node;
        }

        public void MakeComicList()
        {
            List<Comic> initialList = new List<Comic>(comicMap.Values);
            List<Comic> comicList = new List<Comic>(initialList.Count);
            List<Comic> indexed_comics = new List<Comic>();

            for (int ii = 0; ii < initialList.Count; ii++)
            {
                if (initialList[ii].Issue >= 0 && initialList[ii].Issue < comicList.Count)
                {
                    indexed_comics.Add(initialList[ii]);
                }
                else
                {
                    comicList.Add(initialList[ii]);
                }
            }
            comicList = comicList.OrderBy(c => c.File).ToList();
            foreach(Comic c in indexed_comics)
            {
                comicList.Insert(c.Issue, c);
            }

            this.Comics = comicList;
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

            return cbXML;
        }

        public static string getFileFilter()
        {
            return String.Format("Comic File Library (*{0})|*{1};", CBXML_EXTENSION, CBXML_EXTENSION);
        }

    }
}
