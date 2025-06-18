using System.Drawing;
using System.Windows.Forms;

namespace WinFormUpdate
{
    partial class FmUpdate
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
            this.btnSkipVersion = new System.Windows.Forms.Button();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSkipVersion
            // 
            this.btnSkipVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSkipVersion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSkipVersion.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnSkipVersion.ForeColor = System.Drawing.Color.MediumSlateBlue;
            this.btnSkipVersion.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnSkipVersion.Location = new System.Drawing.Point(130, 515);
            this.btnSkipVersion.Margin = new System.Windows.Forms.Padding(12, 12, 12, 12);
            this.btnSkipVersion.Name = "btnSkipVersion";
            this.btnSkipVersion.Size = new System.Drawing.Size(220, 56);
            this.btnSkipVersion.TabIndex = 2;
            this.btnSkipVersion.Text = "跳过当前版本";
            this.btnSkipVersion.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSkipVersion.UseCompatibleTextRendering = true;
            this.btnSkipVersion.UseVisualStyleBackColor = true;
            this.btnSkipVersion.Click += new System.EventHandler(this.btnSkipVersion_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::WinFormUpdate.Properties.Resources.AttnSoft_Update;
            this.pictureBox2.Location = new System.Drawing.Point(16, 10);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(12, 12, 12, 12);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(96, 96);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 4;
            this.pictureBox2.TabStop = false;
            // 
            // btnUpdate
            // 
            this.btnUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpdate.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.btnUpdate.ForeColor = System.Drawing.Color.BlueViolet;
            this.btnUpdate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnUpdate.Location = new System.Drawing.Point(794, 515);
            this.btnUpdate.Margin = new System.Windows.Forms.Padding(12, 12, 12, 12);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(220, 56);
            this.btnUpdate.TabIndex = 1;
            this.btnUpdate.Text = "立即更新";
            this.btnUpdate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnUpdate.UseCompatibleTextRendering = true;
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Location = new System.Drawing.Point(130, 21);
            this.panel1.Margin = new System.Windows.Forms.Padding(12, 12, 12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(884, 476);
            this.panel1.TabIndex = 7;
            // 
            // FmUpdate2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 27F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(1054, 596);
            this.ControlBox = false;
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.btnSkipVersion);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("宋体", 10F);
            this.Margin = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.Name = "FmUpdate2";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AttnSoft.AutoUpdate-自动更新";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSkipVersion;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button btnUpdate;

        private System.Windows.Forms.Panel panel1;
    }
}