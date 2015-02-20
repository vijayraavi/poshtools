namespace PowerShellTools.Project.PropertyPages
{
    partial class ExportsPropertyPageControl
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
            this.txtAlisesToExport = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtVariablesToExport = new System.Windows.Forms.TextBox();
            this.txtCmdletsToExport = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtAlisesToExport
            // 
            this.txtAlisesToExport.Location = new System.Drawing.Point(145, 8);
            this.txtAlisesToExport.Name = "txtAlisesToExport";
            this.txtAlisesToExport.Size = new System.Drawing.Size(182, 20);
            this.txtAlisesToExport.TabIndex = 46;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 45;
            this.label2.Text = "Aliases To Export";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 35);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 13);
            this.label5.TabIndex = 47;
            this.label5.Text = "Cmdlets to Export";
            // 
            // txtVariablesToExport
            // 
            this.txtVariablesToExport.Location = new System.Drawing.Point(145, 64);
            this.txtVariablesToExport.Name = "txtVariablesToExport";
            this.txtVariablesToExport.Size = new System.Drawing.Size(182, 20);
            this.txtVariablesToExport.TabIndex = 50;
            // 
            // txtCmdletsToExport
            // 
            this.txtCmdletsToExport.Location = new System.Drawing.Point(145, 35);
            this.txtCmdletsToExport.Name = "txtCmdletsToExport";
            this.txtCmdletsToExport.Size = new System.Drawing.Size(182, 20);
            this.txtCmdletsToExport.TabIndex = 48;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(13, 66);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(95, 13);
            this.label22.TabIndex = 49;
            this.label22.Text = "Variables to Export";
            // 
            // ExportsPropertyPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtAlisesToExport);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtVariablesToExport);
            this.Controls.Add(this.txtCmdletsToExport);
            this.Controls.Add(this.label22);
            this.Name = "ExportsPropertyPageControl";
            this.Size = new System.Drawing.Size(341, 98);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtAlisesToExport;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtVariablesToExport;
        private System.Windows.Forms.TextBox txtCmdletsToExport;
        private System.Windows.Forms.Label label22;
    }
}
