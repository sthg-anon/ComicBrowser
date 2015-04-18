using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ComicBrowser
{
    static class TreeNodeExtensions
    {
        private static Dictionary<TreeNode, CBXml> filePairings = new Dictionary<TreeNode, CBXml>();

        public static void AddPairing(this TreeNode node, CBXml xml)
        {
            filePairings.Add(node, xml);
        }

        public static CBXml GetCBXml(this TreeNode node)
        {
            return filePairings[node];
        }
    }
}
