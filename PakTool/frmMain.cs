using System;
using System.Collections.Generic;
using System.Windows.Forms;
using PakLib;
using System.IO;
using System.Diagnostics;

namespace PakTool
{
    public partial class frmMain : Form
    {
        private string[] pakfile;
        List<string> selectedFiles = new List<string>();

        public frmMain() : this(null) { }
        public frmMain(string[] args)
        {
            InitializeComponent();

            chkMaintainDir.Enabled = false;
            btnExtractSelected.Enabled = false;
            btnExtractAll.Enabled = false;

            pakfile = args;
            if (pakfile.Length > 0) loadPakTree();

            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(frmMain_DragEnter);
            this.DragDrop += new DragEventHandler(frmMain_DragDrop);
            this.FormClosing += new FormClosingEventHandler(frmMain_FormClosing);
        }

        void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { if (Directory.Exists("temp")) Directory.Delete("temp", true); }
            catch (Exception) { }
        }

        void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            pakfile = files;

            treeView1.Nodes.Clear();

            int f = 0;
            for (int i = 0; i < pakfile.Length; i++)
            {
                if (!pakfile[i].ToLower().EndsWith(".pak"))
                {
                    MessageBox.Show(pakfile[i] + "\n\nThis is not a PAK file.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    pakfile[i] = "";
                }
                else f += 1;
            }

            if (f > 0)
                loadPakTree();
        }

        void frmMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Link;
        }

        void loadPakTree()
        {
            treeView1.BeginUpdate();
            Cursor.Current = Cursors.WaitCursor;

            for (int i = 0; i < pakfile.Length; i++)
            {
                using (Pack pak = new Pack(pakfile[i]))
                {
                    if (pak.IsValid)
                    {
                        TreeNode root = treeView1.Nodes.Add(pak.PackName, pak.PackName);
                        for (int j = 0; j < pak.Files.Length; j++)
                        {
                            PackFile file = pak.Files[j];
                            if (pak.Files[j].PathNodes.Count != 0) populateTree(root, file.PathNodes, file);
                            else root.Nodes.Add(file.FileName, file.FileName);
                        }

                        root.Expand();
                        btnExtractAll.Enabled = true;
                    }
                    else
                    {
                        MessageBox.Show(pakfile[i] + "\n\nThis file cannot be opened or is invalid.\nTip: Make sure the file is not Read-Only.",
                            "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        btnExtractAll.Enabled = false;
                    }
                }
            }

            Cursor.Current = Cursors.Default;

            if (treeView1.Nodes.Count == 0)
            {
                btnExtractSelected.Enabled = false;
                btnExtractAll.Enabled = false;
                chkMaintainDir.Enabled = false;
            }

            treeView1.EndUpdate();
        }

        void populateTree(TreeNode parent, List<string> nodes, PackFile file)
        {
            if (nodes.Count == 0) // Done?
            {
                parent.Nodes.Add(file.FileName, file.FileName); // Add filename to tree
                return;
            }
            else // Not done yet...
            {
                TreeNode tnAdd;
                if (parent.Nodes[nodes[0]] == null) tnAdd = parent.Nodes.Add(nodes[0], nodes[0]);
                else tnAdd = parent.Nodes[nodes[0]];

                List<string> nn = nodes;
                nn.Remove(nodes[0]); // Remove what we just added
                populateTree(tnAdd, nn, file); // Recurse
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "PAK files (*.pak)|*.pak";
            openFile.Multiselect = true;
            DialogResult result = openFile.ShowDialog();
            if (result == DialogResult.OK)
            {
                treeView1.Nodes.Clear();
                pakfile = openFile.FileNames;
                loadPakTree();
            }
        }

        private void btnExtractAll_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                frmExtracting.Create();

                for (int i = 0; i < pakfile.Length; i++)
                {
                    using (Pack pak = new Pack(pakfile[i]))
                    {
                        pak.ExtractAll(dialog.SelectedPath);
                    }
                }

                frmExtracting.Destroy();
            }
        }

        private void btnCreateFromFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dFolder = new FolderBrowserDialog();
            DialogResult dFolderResult = dFolder.ShowDialog();

            if (dFolderResult == DialogResult.OK)
            {
                SaveFileDialog dFile = new SaveFileDialog();
                dFile.Filter = "PAK files (*.pak)|*.pak";
                DialogResult dFileResult = dFile.ShowDialog();
                if (dFileResult == DialogResult.OK)
                {
                    frmCreating.Create();
                    var frmTemp = new frmCreating();

                    using (Pack pak = new Pack(dFile.FileName)) pak.InsertFolder(dFolder.SelectedPath);

                    frmCreating.Destroy();

                    pakfile = new string[1] { dFile.FileName };
                    treeView1.Nodes.Clear();
                    loadPakTree();
                }
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Checked == true)
            {
                if (e.Node.Nodes.Count > 0)
                {
                    for (int i = 0; i < e.Node.Nodes.Count; i++) e.Node.Nodes[i].Checked = true;
                    e.Node.ExpandAll();
                }
            }
            else if (e.Node.Checked == false)
            {
                if (e.Node.Nodes.Count > 0)
                {
                    for (int i = 0; i < e.Node.Nodes.Count; i++) e.Node.Nodes[i].Checked = false;
                }
            }

            selectedFiles.Clear();
            loadChecked(treeView1.Nodes, selectedFiles);
            if (selectedFiles.Count > 0)
            {
                btnExtractSelected.Enabled = true;
                chkMaintainDir.Enabled = true;
            }
            else
            {
                btnExtractSelected.Enabled = false;
                chkMaintainDir.Enabled = false;
            }
        }

        private void loadChecked(TreeNodeCollection nodeList, List<string> list)
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                if (nodeList[i].Checked && nodeList[i].Nodes.Count == 0) list.Add(nodeList[i].FullPath);
                if (nodeList[i].Nodes.Count > 0) loadChecked(nodeList[i].Nodes, list);
            }
        }

        private string[] getPakfileInfo(string fullpath)
        {
            string[] info = new string[2];

            for (int i = 0; i < pakfile.Length; i++)
            {
                if (fullpath.Contains(pakfile[i]))
                {
                    info[0] = pakfile[i];
                    info[1] = fullpath.Replace(info[0] + "/", "");
                    break;
                }
            }

            return info;
        }

        private void btnExtractSelected_Click(object sender, EventArgs e)
        {
            loadChecked(treeView1.Nodes, selectedFiles);

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                frmExtracting.Create();

                List<List<string>> fileList = new List<List<string>>(); // List of Lists

                for (int i = 0; i < selectedFiles.Count; i++)
                {
                    string[] fileInfo = getPakfileInfo(selectedFiles[i]);
                    bool added = false;

                    for (int j = 0; j < fileList.Count; j++)
                    {
                        if (fileList[j][0] == fileInfo[0])
                        {
                            fileList[j].Add(fileInfo[1]);
                            added = true;
                        }
                    }

                    if (!added)
                    {
                        fileList.Add(new List<string>());
                        fileList[fileList.Count - 1].Add(fileInfo[0]);
                        fileList[fileList.Count - 1].Add(fileInfo[1]);
                    }
                }

                for (int i = 0; i < fileList.Count; i++)
                {
                    using (Pack pak = new Pack(fileList[i][0]))
                    {
                        for (int j = 1; j < fileList[i].Count; j++)
                        {
                            if (chkMaintainDir.Checked)
                                pak.ExtractFile(fileList[i][j], dialog.SelectedPath, true);
                            else
                                pak.ExtractFile(fileList[i][j], dialog.SelectedPath, false);
                        }
                    }
                }

                frmExtracting.Destroy();
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {

            if (e.Node.Nodes.Count == 0)
            {
                treeView1.Enabled = false;

                string tempFile = "temp\\" + e.Node.Text;
                
                if (!Directory.Exists("temp"))
                {
                    Directory.CreateDirectory("temp");
                    DirectoryInfo tempDir = new DirectoryInfo("temp");
                    tempDir.Attributes |= FileAttributes.Hidden;
                }

                string[] tempFileInfo = getPakfileInfo(e.Node.FullPath);

                using (Pack pak = new Pack(tempFileInfo[0]))
                {
                    byte[] buffer;
                    buffer = pak.GetFileBytes(tempFileInfo[1]);

                    try 
                    { 
                        File.WriteAllBytes(tempFile, buffer); 
                        Process.Start(tempFile); 
                    }
                    catch (IOException)
                    {
                        MessageBox.Show(e.Node.Text + "\n\nThis file is open or in use. If you opened this file once before, make sure it's not currently open.",
                            "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }

                treeView1.Enabled = true;
            }
            else return;
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            AboutBox1.Create();
        }


    }
}
