namespace RDR_Explorer
{
    partial class RPFviewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RPFviewer));
            this.listView1 = new System.Windows.Forms.ListView();
            this.nameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.typeColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.sizeColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.backButton = new System.Windows.Forms.ToolStripButton();
            this.extrButton = new System.Windows.Forms.ToolStripButton();
            this.extrAllButton = new System.Windows.Forms.ToolStripButton();
            this.uncompressButton = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.pathLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            resources.ApplyResources(this.listView1, "listView1");
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumn,
            this.typeColumn,
            this.sizeColumn});
            this.listView1.FullRowSelect = true;
            this.listView1.Name = "listView1";
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            // 
            // nameColumn
            // 
            resources.ApplyResources(this.nameColumn, "nameColumn");
            // 
            // typeColumn
            // 
            resources.ApplyResources(this.typeColumn, "typeColumn");
            // 
            // sizeColumn
            // 
            resources.ApplyResources(this.sizeColumn, "sizeColumn");
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // toolStrip1
            // 
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.backButton,
            this.extrButton,
            this.extrAllButton,
            this.uncompressButton});
            this.toolStrip1.Name = "toolStrip1";
            // 
            // backButton
            // 
            resources.ApplyResources(this.backButton, "backButton");
            this.backButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.backButton.Name = "backButton";
            this.backButton.Click += new System.EventHandler(this.backButton_Click);
            // 
            // extrButton
            // 
            resources.ApplyResources(this.extrButton, "extrButton");
            this.extrButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.extrButton.Name = "extrButton";
            this.extrButton.Click += new System.EventHandler(this.extrButton_Click);
            // 
            // extrAllButton
            // 
            resources.ApplyResources(this.extrAllButton, "extrAllButton");
            this.extrAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.extrAllButton.Name = "extrAllButton";
            this.extrAllButton.Click += new System.EventHandler(this.extrAllButton_Click);
            // 
            // uncompressButton
            // 
            resources.ApplyResources(this.uncompressButton, "uncompressButton");
            this.uncompressButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.uncompressButton.Name = "uncompressButton";
            this.uncompressButton.Click += new System.EventHandler(this.uncompressButton_Click);
            // 
            // statusStrip1
            // 
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pathLabel});
            this.statusStrip1.Name = "statusStrip1";
            // 
            // pathLabel
            // 
            resources.ApplyResources(this.pathLabel, "pathLabel");
            this.pathLabel.Name = "pathLabel";
            // 
            // RPFviewer
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "RPFviewer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RPFviewer_FormClosed);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ColumnHeader nameColumn;
        private System.Windows.Forms.ColumnHeader typeColumn;
        private System.Windows.Forms.ColumnHeader sizeColumn;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton backButton;
        private System.Windows.Forms.ToolStripButton extrButton;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel pathLabel;
        private System.Windows.Forms.ToolStripButton uncompressButton;
        private System.Windows.Forms.ToolStripButton extrAllButton;
    }
}