﻿namespace AudioMixer
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.Dev_StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.clear_button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.program_1_button = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.trackBar2 = new System.Windows.Forms.TrackBar();
            this.program_2_button = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.trackBar3 = new System.Windows.Forms.TrackBar();
            this.program_3_button = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar3)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Dev_StatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 258);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1104, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // Dev_StatusLabel
            // 
            this.Dev_StatusLabel.Name = "Dev_StatusLabel";
            this.Dev_StatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.HorizontalScrollbar = true;
            this.listBox1.ItemHeight = 16;
            this.listBox1.Location = new System.Drawing.Point(557, 26);
            this.listBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(530, 180);
            this.listBox1.TabIndex = 12;
            // 
            // clear_button
            // 
            this.clear_button.Location = new System.Drawing.Point(557, 220);
            this.clear_button.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.clear_button.Name = "clear_button";
            this.clear_button.Size = new System.Drawing.Size(113, 28);
            this.clear_button.TabIndex = 23;
            this.clear_button.Text = "clear";
            this.clear_button.UseVisualStyleBackColor = true;
            this.clear_button.Click += new System.EventHandler(this.Clear_button_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 17);
            this.label1.TabIndex = 26;
            this.label1.Text = "None";
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(15, 26);
            this.trackBar1.Maximum = 100;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(435, 56);
            this.trackBar1.TabIndex = 25;
            this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar_Scroll);
            // 
            // program_1_button
            // 
            this.program_1_button.Location = new System.Drawing.Point(456, 26);
            this.program_1_button.Name = "program_1_button";
            this.program_1_button.Size = new System.Drawing.Size(81, 29);
            this.program_1_button.TabIndex = 24;
            this.program_1_button.Text = "Open";
            this.program_1_button.UseVisualStyleBackColor = true;
            this.program_1_button.Click += new System.EventHandler(this.Program_Button_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 17);
            this.label2.TabIndex = 29;
            this.label2.Text = "None";
            // 
            // trackBar2
            // 
            this.trackBar2.Location = new System.Drawing.Point(15, 97);
            this.trackBar2.Maximum = 100;
            this.trackBar2.Name = "trackBar2";
            this.trackBar2.Size = new System.Drawing.Size(435, 56);
            this.trackBar2.TabIndex = 28;
            this.trackBar2.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar2.Scroll += new System.EventHandler(this.trackBar_Scroll);
            // 
            // program_2_button
            // 
            this.program_2_button.Location = new System.Drawing.Point(456, 97);
            this.program_2_button.Name = "program_2_button";
            this.program_2_button.Size = new System.Drawing.Size(81, 29);
            this.program_2_button.TabIndex = 27;
            this.program_2_button.Text = "Open";
            this.program_2_button.UseVisualStyleBackColor = true;
            this.program_2_button.Click += new System.EventHandler(this.Program_Button_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 148);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 17);
            this.label3.TabIndex = 32;
            this.label3.Text = "None";
            // 
            // trackBar3
            // 
            this.trackBar3.Location = new System.Drawing.Point(15, 168);
            this.trackBar3.Maximum = 100;
            this.trackBar3.Name = "trackBar3";
            this.trackBar3.Size = new System.Drawing.Size(435, 56);
            this.trackBar3.TabIndex = 31;
            this.trackBar3.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar3.Scroll += new System.EventHandler(this.trackBar_Scroll);
            // 
            // program_3_button
            // 
            this.program_3_button.Location = new System.Drawing.Point(456, 168);
            this.program_3_button.Name = "program_3_button";
            this.program_3_button.Size = new System.Drawing.Size(81, 29);
            this.program_3_button.TabIndex = 30;
            this.program_3_button.Text = "Open";
            this.program_3_button.UseVisualStyleBackColor = true;
            this.program_3_button.Click += new System.EventHandler(this.Program_Button_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Executable files (*.exe)|*.exe";
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "AudioMixer";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(103, 28);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(102, 24);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1104, 280);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.trackBar3);
            this.Controls.Add(this.program_3_button);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.trackBar2);
            this.Controls.Add(this.program_2_button);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.program_1_button);
            this.Controls.Add(this.clear_button);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Audio Mixer";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar3)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel Dev_StatusLabel;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button clear_button;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Button program_1_button;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar trackBar2;
        private System.Windows.Forms.Button program_2_button;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar trackBar3;
        private System.Windows.Forms.Button program_3_button;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    }
}

