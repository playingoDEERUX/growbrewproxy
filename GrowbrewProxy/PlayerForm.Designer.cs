namespace GrowbrewProxy
{
    partial class PlayerForm
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
            this.playerBox = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            // 
            // playerBox
            // 
            this.playerBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.playerBox.Location = new System.Drawing.Point(0, 0);
            this.playerBox.Name = "playerBox";
            this.playerBox.Size = new System.Drawing.Size(605, 333);
            this.playerBox.TabIndex = 0;
            this.playerBox.TabStop = false;
            this.playerBox.Text = "Player Box";
            // 
            // PlayerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(605, 333);
            this.Controls.Add(this.playerBox);
            this.MinimumSize = new System.Drawing.Size(621, 372);
            this.Name = "PlayerForm";
            this.ShowIcon = false;
            this.Text = "All players in EXIT";
            this.Load += new System.EventHandler(this.PlayerForm_Load);
            
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.GroupBox playerBox;
    }
}