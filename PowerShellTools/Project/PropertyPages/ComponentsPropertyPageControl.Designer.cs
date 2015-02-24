namespace PowerShellTools.Project.PropertyPages
{
    partial class ComponentsPropertyPageControl
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
            this.cmoScriptsToProcess = new System.Windows.Forms.ComboBox();
            this.txtTypesToProcess = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.txtNestedModules = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtModuleToProcess = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtModuleList = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtFunctionsToProcess = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtFormatsToProcess = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmoScriptsToProcess
            // 
            this.cmoScriptsToProcess.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cmoScriptsToProcess.DropDownHeight = 1;
            this.cmoScriptsToProcess.FormattingEnabled = true;
            this.cmoScriptsToProcess.IntegralHeight = false;
            this.cmoScriptsToProcess.Location = new System.Drawing.Point(151, 168);
            this.cmoScriptsToProcess.Margin = new System.Windows.Forms.Padding(2);
            this.cmoScriptsToProcess.Name = "cmoScriptsToProcess";
            this.cmoScriptsToProcess.Size = new System.Drawing.Size(180, 21);
            this.cmoScriptsToProcess.TabIndex = 72;
            // 
            // txtTypesToProcess
            // 
            this.txtTypesToProcess.Location = new System.Drawing.Point(151, 200);
            this.txtTypesToProcess.Name = "txtTypesToProcess";
            this.txtTypesToProcess.Size = new System.Drawing.Size(180, 20);
            this.txtTypesToProcess.TabIndex = 71;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(16, 200);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(89, 13);
            this.label21.TabIndex = 70;
            this.label21.Text = "Types to Process";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(16, 168);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(92, 13);
            this.label20.TabIndex = 69;
            this.label20.Text = "Scripts to Process";
            // 
            // txtNestedModules
            // 
            this.txtNestedModules.Location = new System.Drawing.Point(151, 136);
            this.txtNestedModules.Name = "txtNestedModules";
            this.txtNestedModules.Size = new System.Drawing.Size(180, 20);
            this.txtNestedModules.TabIndex = 68;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(16, 136);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(84, 13);
            this.label14.TabIndex = 67;
            this.label14.Text = "Nested Modules";
            // 
            // txtModuleToProcess
            // 
            this.txtModuleToProcess.Location = new System.Drawing.Point(151, 104);
            this.txtModuleToProcess.Name = "txtModuleToProcess";
            this.txtModuleToProcess.Size = new System.Drawing.Size(180, 20);
            this.txtModuleToProcess.TabIndex = 66;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(16, 104);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(95, 13);
            this.label13.TabIndex = 65;
            this.label13.Text = "Module to Process";
            // 
            // txtModuleList
            // 
            this.txtModuleList.Location = new System.Drawing.Point(151, 72);
            this.txtModuleList.Name = "txtModuleList";
            this.txtModuleList.Size = new System.Drawing.Size(180, 20);
            this.txtModuleList.TabIndex = 64;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(16, 72);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(61, 13);
            this.label12.TabIndex = 63;
            this.label12.Text = "Module List";
            // 
            // txtFunctionsToProcess
            // 
            this.txtFunctionsToProcess.Location = new System.Drawing.Point(151, 40);
            this.txtFunctionsToProcess.Name = "txtFunctionsToProcess";
            this.txtFunctionsToProcess.Size = new System.Drawing.Size(180, 20);
            this.txtFunctionsToProcess.TabIndex = 62;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(16, 40);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(110, 13);
            this.label10.TabIndex = 61;
            this.label10.Text = "Functions To Process";
            // 
            // txtFormatsToProcess
            // 
            this.txtFormatsToProcess.Location = new System.Drawing.Point(151, 8);
            this.txtFormatsToProcess.Name = "txtFormatsToProcess";
            this.txtFormatsToProcess.Size = new System.Drawing.Size(180, 20);
            this.txtFormatsToProcess.TabIndex = 60;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(16, 8);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(97, 13);
            this.label9.TabIndex = 59;
            this.label9.Text = "Formats to Process";
            // 
            // ComponentsPropertyPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmoScriptsToProcess);
            this.Controls.Add(this.txtTypesToProcess);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.txtNestedModules);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.txtModuleToProcess);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.txtModuleList);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.txtFunctionsToProcess);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txtFormatsToProcess);
            this.Controls.Add(this.label9);
            this.Name = "ComponentsPropertyPageControl";
            this.Size = new System.Drawing.Size(344, 234);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmoScriptsToProcess;
        private System.Windows.Forms.TextBox txtTypesToProcess;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox txtNestedModules;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtModuleToProcess;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtModuleList;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtFunctionsToProcess;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtFormatsToProcess;
        private System.Windows.Forms.Label label9;
    }
}
