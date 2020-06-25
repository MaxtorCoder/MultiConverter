namespace MultiConverter.Updater
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.boxChangelog = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.boxStatus = new System.Windows.Forms.RichTextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // boxChangelog
            // 
            this.boxChangelog.Location = new System.Drawing.Point(12, 27);
            this.boxChangelog.Name = "boxChangelog";
            this.boxChangelog.ReadOnly = true;
            this.boxChangelog.Size = new System.Drawing.Size(268, 411);
            this.boxChangelog.TabIndex = 0;
            this.boxChangelog.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Changelog:";
            // 
            // boxStatus
            // 
            this.boxStatus.Location = new System.Drawing.Point(286, 27);
            this.boxStatus.Name = "boxStatus";
            this.boxStatus.ReadOnly = true;
            this.boxStatus.Size = new System.Drawing.Size(435, 382);
            this.boxStatus.TabIndex = 2;
            this.boxStatus.Text = "";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(286, 415);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(435, 23);
            this.progressBar.TabIndex = 3;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(728, 450);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.boxStatus);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.boxChangelog);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Text = "MultiConverter - Updater";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox boxChangelog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox boxStatus;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}

