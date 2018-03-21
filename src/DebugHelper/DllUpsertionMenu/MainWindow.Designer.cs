namespace DllUpsertionMenu
{
    partial class MainWindow
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
            this.LB_AllProjects = new System.Windows.Forms.ListBox();
            this.LB_SelectedProjects = new System.Windows.Forms.ListBox();
            this.LB_Services = new System.Windows.Forms.ListBox();
            this.B_Start = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LB_AllProjects
            // 
            this.LB_AllProjects.FormattingEnabled = true;
            this.LB_AllProjects.ItemHeight = 16;
            this.LB_AllProjects.Location = new System.Drawing.Point(12, 12);
            this.LB_AllProjects.Name = "LB_AllProjects";
            this.LB_AllProjects.Size = new System.Drawing.Size(206, 308);
            this.LB_AllProjects.TabIndex = 0;
            // 
            // LB_SelectedProjects
            // 
            this.LB_SelectedProjects.FormattingEnabled = true;
            this.LB_SelectedProjects.ItemHeight = 16;
            this.LB_SelectedProjects.Location = new System.Drawing.Point(240, 12);
            this.LB_SelectedProjects.Name = "LB_SelectedProjects";
            this.LB_SelectedProjects.Size = new System.Drawing.Size(206, 308);
            this.LB_SelectedProjects.TabIndex = 1;
            // 
            // LB_Services
            // 
            this.LB_Services.FormattingEnabled = true;
            this.LB_Services.ItemHeight = 16;
            this.LB_Services.Location = new System.Drawing.Point(473, 12);
            this.LB_Services.Name = "LB_Services";
            this.LB_Services.Size = new System.Drawing.Size(206, 308);
            this.LB_Services.TabIndex = 2;
            // 
            // B_Start
            // 
            this.B_Start.Location = new System.Drawing.Point(284, 341);
            this.B_Start.Name = "B_Start";
            this.B_Start.Size = new System.Drawing.Size(112, 46);
            this.B_Start.TabIndex = 3;
            this.B_Start.Text = "Start";
            this.B_Start.UseVisualStyleBackColor = true;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(691, 399);
            this.Controls.Add(this.B_Start);
            this.Controls.Add(this.LB_Services);
            this.Controls.Add(this.LB_SelectedProjects);
            this.Controls.Add(this.LB_AllProjects);
            this.Name = "MainWindow";
            this.Text = "Debug Helper";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox LB_AllProjects;
        private System.Windows.Forms.ListBox LB_SelectedProjects;
        private System.Windows.Forms.ListBox LB_Services;
        private System.Windows.Forms.Button B_Start;
    }
}

