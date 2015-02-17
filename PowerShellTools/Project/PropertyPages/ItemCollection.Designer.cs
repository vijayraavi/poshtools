namespace PowerShellTools.Project.PropertyPages
{
    partial class ItemCollection
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmoItems = new System.Windows.Forms.ComboBox();
            this.lstItems = new System.Windows.Forms.ListBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmoItems
            // 
            this.cmoItems.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.cmoItems.FormattingEnabled = true;
            this.cmoItems.Location = new System.Drawing.Point(8, 8);
            this.cmoItems.Name = "cmoItems";
            this.cmoItems.Size = new System.Drawing.Size(312, 21);
            this.cmoItems.Sorted = true;
            this.cmoItems.TabIndex = 0;
            // 
            // lstItems
            // 
            this.lstItems.FormattingEnabled = true;
            this.lstItems.Location = new System.Drawing.Point(8, 40);
            this.lstItems.Name = "lstItems";
            this.lstItems.Size = new System.Drawing.Size(312, 134);
            this.lstItems.Sorted = true;
            this.lstItems.TabIndex = 1;
            // 
            // btnAdd
            // 
            this.btnAdd.Enabled = false;
            this.btnAdd.Location = new System.Drawing.Point(328, 8);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(80, 24);
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = "button1";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Enabled = false;
            this.btnRemove.Location = new System.Drawing.Point(328, 40);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(80, 23);
            this.btnRemove.TabIndex = 3;
            this.btnRemove.Text = "button2";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // ItemCollection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.lstItems);
            this.Controls.Add(this.cmoItems);
            this.Name = "ItemCollection";
            this.Size = new System.Drawing.Size(411, 182);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cmoItems;
        private System.Windows.Forms.ListBox lstItems;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
    }
}
