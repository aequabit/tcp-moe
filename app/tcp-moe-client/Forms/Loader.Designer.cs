namespace tcp_moe_client.Forms
{
    partial class Loader
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Loader));
            this.lstCheatList = new System.Windows.Forms.ListBox();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.grpCheatInfo = new System.Windows.Forms.GroupBox();
            this.lblInfo = new System.Windows.Forms.Label();
            this.grpCheatList = new System.Windows.Forms.GroupBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.grpUserInfo = new System.Windows.Forms.GroupBox();
            this.lblUser = new System.Windows.Forms.Label();
            this.tmrLoad = new System.Windows.Forms.Timer(this.components);
            this.lblRank = new System.Windows.Forms.Label();
            this.prgLoad = new tcp_moe_client.Classes.CProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.grpCheatInfo.SuspendLayout();
            this.grpCheatList.SuspendLayout();
            this.grpUserInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstCheatList
            // 
            this.lstCheatList.FormattingEnabled = true;
            this.lstCheatList.ItemHeight = 20;
            this.lstCheatList.Location = new System.Drawing.Point(11, 26);
            this.lstCheatList.Name = "lstCheatList";
            this.lstCheatList.Size = new System.Drawing.Size(252, 144);
            this.lstCheatList.TabIndex = 0;
            this.lstCheatList.SelectedIndexChanged += new System.EventHandler(this.lstCheatList_SelectedIndexChanged);
            // 
            // picLogo
            // 
            this.picLogo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picLogo.BackgroundImage")));
            this.picLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.picLogo.Location = new System.Drawing.Point(12, 14);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(275, 89);
            this.picLogo.TabIndex = 3;
            this.picLogo.TabStop = false;
            // 
            // grpCheatInfo
            // 
            this.grpCheatInfo.Controls.Add(this.lblInfo);
            this.grpCheatInfo.Location = new System.Drawing.Point(293, 109);
            this.grpCheatInfo.Name = "grpCheatInfo";
            this.grpCheatInfo.Size = new System.Drawing.Size(294, 219);
            this.grpCheatInfo.TabIndex = 5;
            this.grpCheatInfo.TabStop = false;
            this.grpCheatInfo.Text = "Cheat Information";
            // 
            // lblInfo
            // 
            this.lblInfo.Location = new System.Drawing.Point(11, 25);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(273, 184);
            this.lblInfo.TabIndex = 0;
            this.lblInfo.Text = "-";
            // 
            // grpCheatList
            // 
            this.grpCheatList.Controls.Add(this.prgLoad);
            this.grpCheatList.Controls.Add(this.btnLoad);
            this.grpCheatList.Controls.Add(this.lstCheatList);
            this.grpCheatList.Location = new System.Drawing.Point(12, 109);
            this.grpCheatList.Name = "grpCheatList";
            this.grpCheatList.Size = new System.Drawing.Size(275, 219);
            this.grpCheatList.TabIndex = 6;
            this.grpCheatList.TabStop = false;
            this.grpCheatList.Text = "Select Cheat";
            // 
            // btnLoad
            // 
            this.btnLoad.Enabled = false;
            this.btnLoad.Location = new System.Drawing.Point(11, 176);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(84, 33);
            this.btnLoad.TabIndex = 7;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // grpUserInfo
            // 
            this.grpUserInfo.Controls.Add(this.lblRank);
            this.grpUserInfo.Controls.Add(this.lblUser);
            this.grpUserInfo.Location = new System.Drawing.Point(293, 14);
            this.grpUserInfo.Name = "grpUserInfo";
            this.grpUserInfo.Size = new System.Drawing.Size(294, 84);
            this.grpUserInfo.TabIndex = 6;
            this.grpUserInfo.TabStop = false;
            this.grpUserInfo.Text = "User Information";
            // 
            // lblUser
            // 
            this.lblUser.AutoSize = true;
            this.lblUser.Location = new System.Drawing.Point(11, 25);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(150, 20);
            this.lblUser.TabIndex = 0;
            this.lblUser.Text = "User: {{ USERNAME }}";
            // 
            // tmrLoad
            // 
            this.tmrLoad.Tick += new System.EventHandler(this.tmrLoad_Tick);
            // 
            // lblRank
            // 
            this.lblRank.AutoSize = true;
            this.lblRank.Location = new System.Drawing.Point(11, 50);
            this.lblRank.Name = "lblRank";
            this.lblRank.Size = new System.Drawing.Size(115, 20);
            this.lblRank.TabIndex = 1;
            this.lblRank.Text = "Rank: {{ RANK }}";
            // 
            // prgLoad
            // 
            this.prgLoad.Location = new System.Drawing.Point(102, 177);
            this.prgLoad.Name = "prgLoad";
            this.prgLoad.Size = new System.Drawing.Size(161, 32);
            this.prgLoad.TabIndex = 8;
            // 
            // Loader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(599, 338);
            this.Controls.Add(this.grpUserInfo);
            this.Controls.Add(this.grpCheatList);
            this.Controls.Add(this.grpCheatInfo);
            this.Controls.Add(this.picLogo);
            this.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Loader";
            this.ShowIcon = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Loader_FormClosing);
            this.Load += new System.EventHandler(this.frmLoader_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.frmLoader_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.grpCheatInfo.ResumeLayout(false);
            this.grpCheatList.ResumeLayout(false);
            this.grpUserInfo.ResumeLayout(false);
            this.grpUserInfo.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lstCheatList;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.GroupBox grpCheatInfo;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.GroupBox grpCheatList;
        private System.Windows.Forms.GroupBox grpUserInfo;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Button btnLoad;
        private tcp_moe_client.Classes.CProgressBar prgLoad;
        private System.Windows.Forms.Timer tmrLoad;
        private System.Windows.Forms.Label lblRank;
    }
}