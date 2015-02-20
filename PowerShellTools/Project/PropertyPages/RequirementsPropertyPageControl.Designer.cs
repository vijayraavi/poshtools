namespace PowerShellTools.Project.PropertyPages
{
    partial class RequirementsPropertyPageControl
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
            this.cmoRequiredAssemblies = new System.Windows.Forms.ComboBox();
            this.txtRequiredModules = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.cmoProcessorArchitecture = new System.Windows.Forms.ComboBox();
            this.label17 = new System.Windows.Forms.Label();
            this.cmoPowerShellVersion = new System.Windows.Forms.ComboBox();
            this.label16 = new System.Windows.Forms.Label();
            this.txtPowerShellHostVersion = new System.Windows.Forms.MaskedTextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.cmoCLRVersion = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmoRequiredAssemblies
            // 
            this.cmoRequiredAssemblies.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmoRequiredAssemblies.FormattingEnabled = true;
            this.cmoRequiredAssemblies.Location = new System.Drawing.Point(144, 136);
            this.cmoRequiredAssemblies.Margin = new System.Windows.Forms.Padding(2);
            this.cmoRequiredAssemblies.Name = "cmoRequiredAssemblies";
            this.cmoRequiredAssemblies.Size = new System.Drawing.Size(225, 21);
            this.cmoRequiredAssemblies.TabIndex = 69;
            // 
            // txtRequiredModules
            // 
            this.txtRequiredModules.Location = new System.Drawing.Point(144, 168);
            this.txtRequiredModules.Name = "txtRequiredModules";
            this.txtRequiredModules.Size = new System.Drawing.Size(224, 20);
            this.txtRequiredModules.TabIndex = 68;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(16, 168);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(96, 13);
            this.label19.TabIndex = 67;
            this.label19.Text = "Required Modules:";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(16, 136);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(108, 13);
            this.label18.TabIndex = 66;
            this.label18.Text = "Required Assemblies:";
            // 
            // cmoProcessorArchitecture
            // 
            this.cmoProcessorArchitecture.FormattingEnabled = true;
            this.cmoProcessorArchitecture.Items.AddRange(new object[] {
            "x86",
            "x64"});
            this.cmoProcessorArchitecture.Location = new System.Drawing.Point(144, 8);
            this.cmoProcessorArchitecture.Name = "cmoProcessorArchitecture";
            this.cmoProcessorArchitecture.Size = new System.Drawing.Size(224, 21);
            this.cmoProcessorArchitecture.TabIndex = 65;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(16, 16);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(67, 13);
            this.label17.TabIndex = 64;
            this.label17.Text = "Architecture:";
            // 
            // cmoPowerShellVersion
            // 
            this.cmoPowerShellVersion.FormattingEnabled = true;
            this.cmoPowerShellVersion.Items.AddRange(new object[] {
            "v2",
            "v3",
            "v4"});
            this.cmoPowerShellVersion.Location = new System.Drawing.Point(144, 104);
            this.cmoPowerShellVersion.Name = "cmoPowerShellVersion";
            this.cmoPowerShellVersion.Size = new System.Drawing.Size(224, 21);
            this.cmoPowerShellVersion.TabIndex = 63;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(16, 104);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(101, 13);
            this.label16.TabIndex = 62;
            this.label16.Text = "PowerShell Version:";
            // 
            // txtPowerShellHostVersion
            // 
            this.txtPowerShellHostVersion.Location = new System.Drawing.Point(144, 72);
            this.txtPowerShellHostVersion.Name = "txtPowerShellHostVersion";
            this.txtPowerShellHostVersion.Size = new System.Drawing.Size(224, 20);
            this.txtPowerShellHostVersion.TabIndex = 61;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(16, 72);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(70, 13);
            this.label15.TabIndex = 60;
            this.label15.Text = "Host Version:";
            // 
            // cmoCLRVersion
            // 
            this.cmoCLRVersion.FormattingEnabled = true;
            this.cmoCLRVersion.Items.AddRange(new object[] {
            "v2.0",
            "v3.0",
            "v3.5",
            "v4.0",
            "v4.5"});
            this.cmoCLRVersion.Location = new System.Drawing.Point(144, 40);
            this.cmoCLRVersion.Name = "cmoCLRVersion";
            this.cmoCLRVersion.Size = new System.Drawing.Size(224, 21);
            this.cmoCLRVersion.TabIndex = 59;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 58;
            this.label4.Text = "CLR Version:";
            // 
            // RequirementsPropertyPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmoRequiredAssemblies);
            this.Controls.Add(this.txtRequiredModules);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.cmoProcessorArchitecture);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.cmoPowerShellVersion);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.txtPowerShellHostVersion);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.cmoCLRVersion);
            this.Controls.Add(this.label4);
            this.Name = "RequirementsPropertyPageControl";
            this.Size = new System.Drawing.Size(383, 202);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmoRequiredAssemblies;
        private System.Windows.Forms.TextBox txtRequiredModules;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.ComboBox cmoProcessorArchitecture;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.ComboBox cmoPowerShellVersion;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.MaskedTextBox txtPowerShellHostVersion;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.ComboBox cmoCLRVersion;
        private System.Windows.Forms.Label label4;
    }
}
