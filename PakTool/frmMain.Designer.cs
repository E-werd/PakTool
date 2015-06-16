namespace PakTool
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnExtractAll = new System.Windows.Forms.Button();
            this.btnCreateFromFolder = new System.Windows.Forms.Button();
            this.btnExtractSelected = new System.Windows.Forms.Button();
            this.chkMaintainDir = new System.Windows.Forms.CheckBox();
            this.btnAbout = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView1.CheckBoxes = true;
            this.treeView1.Location = new System.Drawing.Point(12, 41);
            this.treeView1.Name = "treeView1";
            this.treeView1.PathSeparator = "/";
            this.treeView1.Size = new System.Drawing.Size(310, 247);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterCheck);
            this.treeView1.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseDoubleClick);
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(12, 12);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 1;
            this.btnOpen.Text = "Open PAK...";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnExtractAll
            // 
            this.btnExtractAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExtractAll.Location = new System.Drawing.Point(247, 294);
            this.btnExtractAll.Name = "btnExtractAll";
            this.btnExtractAll.Size = new System.Drawing.Size(75, 23);
            this.btnExtractAll.TabIndex = 2;
            this.btnExtractAll.Text = "Extract All...";
            this.btnExtractAll.UseVisualStyleBackColor = true;
            this.btnExtractAll.Click += new System.EventHandler(this.btnExtractAll_Click);
            // 
            // btnCreateFromFolder
            // 
            this.btnCreateFromFolder.Location = new System.Drawing.Point(93, 12);
            this.btnCreateFromFolder.Name = "btnCreateFromFolder";
            this.btnCreateFromFolder.Size = new System.Drawing.Size(134, 23);
            this.btnCreateFromFolder.TabIndex = 3;
            this.btnCreateFromFolder.Text = "Create PAK from folder...";
            this.btnCreateFromFolder.UseVisualStyleBackColor = true;
            this.btnCreateFromFolder.Click += new System.EventHandler(this.btnCreateFromFolder_Click);
            // 
            // btnExtractSelected
            // 
            this.btnExtractSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExtractSelected.Location = new System.Drawing.Point(139, 294);
            this.btnExtractSelected.Name = "btnExtractSelected";
            this.btnExtractSelected.Size = new System.Drawing.Size(102, 23);
            this.btnExtractSelected.TabIndex = 6;
            this.btnExtractSelected.Text = "Extract Selected...";
            this.btnExtractSelected.UseVisualStyleBackColor = true;
            this.btnExtractSelected.Click += new System.EventHandler(this.btnExtractSelected_Click);
            // 
            // chkMaintainDir
            // 
            this.chkMaintainDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkMaintainDir.AutoSize = true;
            this.chkMaintainDir.Checked = true;
            this.chkMaintainDir.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMaintainDir.Location = new System.Drawing.Point(139, 323);
            this.chkMaintainDir.Name = "chkMaintainDir";
            this.chkMaintainDir.Size = new System.Drawing.Size(153, 17);
            this.chkMaintainDir.TabIndex = 7;
            this.chkMaintainDir.Text = "Maintain directory structure";
            this.chkMaintainDir.UseVisualStyleBackColor = true;
            // 
            // btnAbout
            // 
            this.btnAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAbout.AutoSize = true;
            this.btnAbout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAbout.Location = new System.Drawing.Point(277, 12);
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.Size = new System.Drawing.Size(45, 23);
            this.btnAbout.TabIndex = 8;
            this.btnAbout.Text = "About";
            this.btnAbout.UseVisualStyleBackColor = true;
            this.btnAbout.Click += new System.EventHandler(this.btnAbout_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 352);
            this.Controls.Add(this.btnAbout);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.btnCreateFromFolder);
            this.Controls.Add(this.chkMaintainDir);
            this.Controls.Add(this.btnExtractSelected);
            this.Controls.Add(this.btnExtractAll);
            this.Controls.Add(this.treeView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(350, 390);
            this.Name = "frmMain";
            this.Text = "PakTool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnExtractAll;
        private System.Windows.Forms.Button btnCreateFromFolder;
        private System.Windows.Forms.Button btnExtractSelected;
        private System.Windows.Forms.CheckBox chkMaintainDir;
        private System.Windows.Forms.Button btnAbout;


    }
}

