namespace MultiConverter.GUI
{
    partial class ConverterForm
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
            this.fix_btn = new System.Windows.Forms.Button();
            this.lb = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.progress = new System.Windows.Forms.ProgressBar();
            this.cb_wod = new System.Windows.Forms.CheckBox();
            this.adt_water = new System.Windows.Forms.CheckBox();
            this.adt_models = new System.Windows.Forms.CheckBox();
            this.adt_group = new System.Windows.Forms.GroupBox();
            this.wmo_group = new System.Windows.Forms.GroupBox();
            this.m2_group = new System.Windows.Forms.GroupBox();
            this.helm_fix_cb = new System.Windows.Forms.CheckBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adt_group.SuspendLayout();
            this.wmo_group.SuspendLayout();
            this.m2_group.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // fix_btn
            // 
            this.fix_btn.Location = new System.Drawing.Point(371, 39);
            this.fix_btn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.fix_btn.Name = "fix_btn";
            this.fix_btn.Size = new System.Drawing.Size(180, 45);
            this.fix_btn.TabIndex = 0;
            this.fix_btn.Text = "Fix";
            this.fix_btn.UseVisualStyleBackColor = true;
            this.fix_btn.Click += new System.EventHandler(this.fix_btn_Click);
            // 
            // lb
            // 
            this.lb.AllowDrop = true;
            this.lb.FormattingEnabled = true;
            this.lb.ItemHeight = 15;
            this.lb.Location = new System.Drawing.Point(15, 40);
            this.lb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lb.Name = "lb";
            this.lb.Size = new System.Drawing.Size(348, 244);
            this.lb.TabIndex = 2;
            this.lb.DragDrop += new System.Windows.Forms.DragEventHandler(this.filepath_OnDrop);
            this.lb.DragEnter += new System.Windows.Forms.DragEventHandler(this.filepath_DragEnter);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(371, 91);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(180, 45);
            this.button1.TabIndex = 3;
            this.button1.Text = "Clear";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // progress
            // 
            this.progress.Location = new System.Drawing.Point(15, 288);
            this.progress.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(349, 27);
            this.progress.TabIndex = 4;
            // 
            // cb_wod
            // 
            this.cb_wod.AutoSize = true;
            this.cb_wod.Location = new System.Drawing.Point(6, 16);
            this.cb_wod.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cb_wod.Name = "cb_wod";
            this.cb_wod.Size = new System.Drawing.Size(102, 19);
            this.cb_wod.TabIndex = 5;
            this.cb_wod.Text = "Legion > WoD";
            this.cb_wod.UseVisualStyleBackColor = true;
            // 
            // adt_water
            // 
            this.adt_water.AutoSize = true;
            this.adt_water.Checked = true;
            this.adt_water.CheckState = System.Windows.Forms.CheckState.Checked;
            this.adt_water.Location = new System.Drawing.Point(7, 22);
            this.adt_water.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.adt_water.Name = "adt_water";
            this.adt_water.Size = new System.Drawing.Size(136, 19);
            this.adt_water.TabIndex = 6;
            this.adt_water.Text = "Liquids (Water, Lava)";
            this.adt_water.UseVisualStyleBackColor = true;
            // 
            // adt_models
            // 
            this.adt_models.AutoSize = true;
            this.adt_models.Checked = true;
            this.adt_models.CheckState = System.Windows.Forms.CheckState.Checked;
            this.adt_models.Location = new System.Drawing.Point(7, 46);
            this.adt_models.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.adt_models.Name = "adt_models";
            this.adt_models.Size = new System.Drawing.Size(130, 19);
            this.adt_models.TabIndex = 7;
            this.adt_models.Text = "Models (WMO, M2)";
            this.adt_models.UseVisualStyleBackColor = true;
            // 
            // adt_group
            // 
            this.adt_group.Controls.Add(this.adt_models);
            this.adt_group.Controls.Add(this.adt_water);
            this.adt_group.Location = new System.Drawing.Point(371, 241);
            this.adt_group.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.adt_group.Name = "adt_group";
            this.adt_group.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.adt_group.Size = new System.Drawing.Size(180, 74);
            this.adt_group.TabIndex = 8;
            this.adt_group.TabStop = false;
            this.adt_group.Text = "ADT";
            // 
            // wmo_group
            // 
            this.wmo_group.Controls.Add(this.cb_wod);
            this.wmo_group.Location = new System.Drawing.Point(371, 143);
            this.wmo_group.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.wmo_group.Name = "wmo_group";
            this.wmo_group.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.wmo_group.Size = new System.Drawing.Size(180, 43);
            this.wmo_group.TabIndex = 9;
            this.wmo_group.TabStop = false;
            this.wmo_group.Text = "WMO";
            // 
            // m2_group
            // 
            this.m2_group.Controls.Add(this.helm_fix_cb);
            this.m2_group.Location = new System.Drawing.Point(371, 193);
            this.m2_group.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.m2_group.Name = "m2_group";
            this.m2_group.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.m2_group.Size = new System.Drawing.Size(180, 42);
            this.m2_group.TabIndex = 10;
            this.m2_group.TabStop = false;
            this.m2_group.Text = "M2";
            // 
            // helm_fix_cb
            // 
            this.helm_fix_cb.AutoSize = true;
            this.helm_fix_cb.Checked = true;
            this.helm_fix_cb.CheckState = System.Windows.Forms.CheckState.Checked;
            this.helm_fix_cb.Location = new System.Drawing.Point(9, 16);
            this.helm_fix_cb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.helm_fix_cb.Name = "helm_fix_cb";
            this.helm_fix_cb.Size = new System.Drawing.Size(108, 19);
            this.helm_fix_cb.TabIndex = 0;
            this.helm_fix_cb.Text = "Helm Offset Fix";
            this.helm_fix_cb.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(565, 24);
            this.menuStrip1.TabIndex = 11;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // ConverterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 330);
            this.Controls.Add(this.m2_group);
            this.Controls.Add(this.wmo_group);
            this.Controls.Add(this.adt_group);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lb);
            this.Controls.Add(this.fix_btn);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximumSize = new System.Drawing.Size(581, 369);
            this.MinimumSize = new System.Drawing.Size(581, 369);
            this.Name = "ConverterForm";
            this.Text = "Multi-Converter";
            this.adt_group.ResumeLayout(false);
            this.adt_group.PerformLayout();
            this.wmo_group.ResumeLayout(false);
            this.wmo_group.PerformLayout();
            this.m2_group.ResumeLayout(false);
            this.m2_group.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button fix_btn;
        private System.Windows.Forms.ListBox lb;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ProgressBar progress;
        private System.Windows.Forms.CheckBox cb_wod;
        private System.Windows.Forms.CheckBox adt_water;
        private System.Windows.Forms.CheckBox adt_models;
        private System.Windows.Forms.GroupBox adt_group;
        private System.Windows.Forms.GroupBox wmo_group;
        private System.Windows.Forms.GroupBox m2_group;
        private System.Windows.Forms.CheckBox helm_fix_cb;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    }
}

