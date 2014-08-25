using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace HexViewer
{
    public partial class Form1 : Form
    {
        public List<FileInfo> Files = new List<FileInfo>();

        public Form1()
        {
            InitializeComponent();

            textBox1.Text = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Roaming\Apple Computer\MobileSync\Backup\");
            folderBrowserDialog1.SelectedPath = textBox1.Text;
        }

        private void TestParse(string directory)
        {
            treeView1.Nodes.Clear();
            Files.Clear();
            textBox2.Clear();
            var filePath = directory + "Manifest.mbdb";
            var parser = new BackupParser(filePath);

            var header = parser.ReadString(4);
            textBox2.AppendText(string.Format("Header: {0}, Location: {1}, Size: {2}\r\n", header, parser.Offset, parser.Length));

            var headerBytes = parser.ReadBytes(2);
            textBox2.AppendText(string.Format("{0:x2} {1:x2}\r\n", headerBytes[0], headerBytes[1]));
            
            while (!parser.HasReachedEof())
            {
                var file = new FileInfo(parser);
                Files.Add(file);

                var domainNode = FindNode(null, file.Domain);
                if (domainNode == null)
                {
                    domainNode = new TreeNode(file.Domain);
                    treeView1.Nodes.Add(domainNode);
                }

                if (file.Path.Length == 0)
                    continue;

                //var parsedPath = ParseLocation(file.Path);
                var node = new HexViewerNode(file);
                domainNode.Nodes.Add(node);

                if (file.Size / 1024d > 2000)
                {
                    Console.WriteLine(node.ToString());
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();

            if (result != DialogResult.OK)
            {
                textBox1.Text = "";
                return;
            }

            textBox1.Text = folderBrowserDialog1.SelectedPath + "\\";

            TestParse(textBox1.Text);
        }

        // Finding next node that matches name
        // Check Starting node
        // Get Children of starting node
        // Get Siblings of starting node
        // Check Children
        // Check Siblings
        // Get Parent Node

        // Check Children of starting node
        // Check Siblings of starting node

        private TreeNode FindNextNode(TreeNode startingNode, string name, bool skipStartingNode = false)
        {
            TreeNode foundNode;

            if (startingNode == null)
                return null;

            if (!skipStartingNode && startingNode.Text.Contains(name))
                return startingNode;

            foreach (TreeNode child in startingNode.Nodes)
            {
                foundNode = FindNextNode(child, name);

                if (foundNode != null)
                    return foundNode;
            }

            var nextSiblingNode = startingNode.NextNode;
            foundNode = FindNextNode(nextSiblingNode, name);
            if (foundNode != null)
                return foundNode;

            var parentNode = startingNode.Parent;
            if (parentNode == null)
                return null;

            var parentSibling = parentNode.NextNode;

            foundNode = FindNextNode(parentSibling, name);
            if (foundNode != null)
                return foundNode;

            return null;
        }

        private TreeNode FindNode(TreeNode parentNode, string name, bool contains = false)
        {
            if (parentNode != null && ((!contains && parentNode.Text == name) || (contains && parentNode.Text.Contains(name))))
            {
                return parentNode;
            }

            var childNodes = (parentNode != null) ? parentNode.Nodes : treeView1.Nodes;

            foreach (TreeNode node in childNodes)
            {
                var foundNode = FindNode(node, name, contains);

                if (foundNode != null)
                    return foundNode;
            }

            //TODO: Search siblings as well as children
            //var foundNode = FindNode(parentNode.NextNode)

            return null;
        }

        private static List<string> ParseLocation(string name)
        {
            var parsedPath = new List<string>();

            var directory = Path.GetDirectoryName(name);
            var fileName = Path.GetFileName(name);

            parsedPath.Add(directory);
            parsedPath.Add(fileName);

            return parsedPath;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            textBox2.Clear();
            textBox2.AppendText("File: " + e.Node.Text + "\r\n\r\n");

            var fileNode = e.Node as HexViewerNode;

            if (fileNode == null)
                return;

            textBox2.AppendText(string.Format("FileNameHash:\r\n{0}\r\n", HexToString(8, 4, fileNode.Info.FileNameHash)));
            textBox2.AppendText(string.Format("Domain: {0}\r\n", fileNode.Info.Domain));
            textBox2.AppendText(string.Format("Path: {0}\r\n", fileNode.Info.Path));
            textBox2.AppendText(string.Format("File Size: {0}\r\n", FormattedFileSize(fileNode.Info.Size)));
            textBox2.AppendText(string.Format("\r\nUnknown:\r\n{0}\r\n", HexToString(8, 4, fileNode.Info.Unknown)));
            textBox2.AppendText(string.Format("File Hash:\r\n{0}\r\n", HexToString(8, 4, fileNode.Info.FileHash)));
            textBox2.AppendText(string.Format("Unknown2:\r\n{0}\r\n", HexToString(8, 4, fileNode.Info.Unknown2)));
            textBox2.AppendText(string.Format("Unknown3:\r\n{0}\r\n", HexToString(8, 4, fileNode.Info.Unknown3)));
            textBox2.AppendText(string.Format("Unknown4:\r\n{0}\r\n", HexToString(8, 4, fileNode.Info.Unknown4)));

            foreach (var property in fileNode.Info.Properties)
            {
                textBox2.AppendText(string.Format("Property: {0}\r\n{1}\r\n", property.Name, HexToString(8, 4, property.Value)));
            }

            // Load File Data
            var hashLen = fileNode.Info.FileNameHash.Length;
            if (hashLen != 0)
            {
                var hashText = WriteHexLine(0, hashLen, hashLen, fileNode.Info.FileNameHash);

                if (!File.Exists(textBox1.Text + hashText))
                {
                    textBox2.AppendText("\r\nData:\r\nFile Not Found!\r\n");
                    return;
                }
                else
                {
                    var fileData = File.ReadAllBytes(textBox1.Text + hashText);
                    textBox2.AppendText(string.Format("\r\nData:\r\n{0}\r\n", HexToString(8, 4, fileData)));
                }
            }

            textBox2.Select(0, 0);
            textBox2.ScrollToCaret();
        }

        private static string FormattedFileSize(ulong fileSize)
        {
            var mBytes = fileSize / 1024d / 1024d;
            return string.Format("{0} bytes ({1:0.00} MB)", fileSize, mBytes);
        }

        private static string HexToString(int size, int width, byte[] data)
        {
            var result = new StringBuilder();

            for (var i = 0; i < data.Length; i += size * width)
            {
                result.AppendLine(WriteHexLine(i, size, width, data) + "     " + WriteAsciiLine(i, size, width, data));
            }

            return result.ToString();
        }

        private static string WriteHexLine(int offset, int size, int width, byte[] data)
        {
            var result = new StringBuilder();
            var bytesWritten = 0;

            for (var i = offset; i < data.Length && bytesWritten < size * width; i++, bytesWritten++)
            {
                if (bytesWritten % size == 0 && bytesWritten != 0)
                {
                    result.Append(" ");
                }

                result.Append(String.Format("{0:x2}", data[i]));
            }

            while (bytesWritten < size * width)
            {
                if (bytesWritten % size == 0 && bytesWritten != 0)
                {
                    result.Append(" ");
                }

                result.Append("  ");
                bytesWritten++;
            }

            return result.ToString();
        }

        private static string WriteAsciiLine(int offset, int size, int width, byte[] data)
        {
            var result = new StringBuilder();
            var bytesWritten = 0;

            for (var i = offset; i < data.Length && bytesWritten < size * width; i++, bytesWritten++)
            {
                if (bytesWritten % size == 0 && bytesWritten != 0)
                {
                    result.Append(" ");
                }

                if (data[i] > 31 && data[i] < 128)
                    result.Append(Convert.ToChar(data[i]));
                else
                    result.Append(".");
            }

            return result.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TreeNode foundNode;
            var selectedNode = treeView1.SelectedNode;

            if (selectedNode == null)
            {
                selectedNode = treeView1.TopNode;
                foundNode = FindNextNode(selectedNode, textBox3.Text);
            }
            else
            {
                foundNode = FindNextNode(selectedNode, textBox3.Text, true);
            }

            if (foundNode == null)
                return;

            if (foundNode == selectedNode)
            {
                //if (selectedNode.Nodes.Count > 0)
                //    selectedNode = selectedNode.FirstNode;
                //else 
            }

            treeView1.SelectedNode = foundNode;
        }
    }
}
