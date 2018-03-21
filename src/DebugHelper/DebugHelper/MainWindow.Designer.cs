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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.LB_SelectedProjects = new System.Windows.Forms.ListBox();
            this.LB_Services = new System.Windows.Forms.ListBox();
            this.B_Start = new System.Windows.Forms.Button();
            this.LB_AllProjects = new System.Windows.Forms.ListBox();
            this.TB_LogConsole = new System.Windows.Forms.TextBox();
            this.L_Projects = new System.Windows.Forms.Label();
            this.L_SelectedProjects = new System.Windows.Forms.Label();
            this.L_Services = new System.Windows.Forms.Label();
            this.L_Console = new System.Windows.Forms.Label();
            this.B_OpenDeployDirectory = new System.Windows.Forms.Button();
            this.B_StartStopService = new System.Windows.Forms.Button();
            this.B_AttachToProcess = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LB_SelectedProjects
            // 
            this.LB_SelectedProjects.FormattingEnabled = true;
            this.LB_SelectedProjects.ItemHeight = 16;
            this.LB_SelectedProjects.Location = new System.Drawing.Point(224, 39);
            this.LB_SelectedProjects.Name = "LB_SelectedProjects";
            this.LB_SelectedProjects.Size = new System.Drawing.Size(206, 308);
            this.LB_SelectedProjects.TabIndex = 1;
            this.LB_SelectedProjects.SelectedIndexChanged += new System.EventHandler(this.LB_SelectedProjects_SelectedIndexChanged);
            this.LB_SelectedProjects.DoubleClick += new System.EventHandler(this.LB_SelectedProjects_DoubleClick);
            // 
            // LB_Services
            // 
            this.LB_Services.FormattingEnabled = true;
            this.LB_Services.ItemHeight = 16;
            this.LB_Services.Location = new System.Drawing.Point(436, 39);
            this.LB_Services.Name = "LB_Services";
            this.LB_Services.Size = new System.Drawing.Size(206, 308);
            this.LB_Services.TabIndex = 2;
            this.LB_Services.SelectedIndexChanged += new System.EventHandler(this.LB_Services_SelectedIndexChanged);
            // 
            // B_Start
            // 
            this.B_Start.Location = new System.Drawing.Point(12, 353);
            this.B_Start.Name = "B_Start";
            this.B_Start.Size = new System.Drawing.Size(112, 46);
            this.B_Start.TabIndex = 3;
            this.B_Start.Text = "Start";
            this.B_Start.UseVisualStyleBackColor = true;
            this.B_Start.Click += new System.EventHandler(this.B_Start_Click);
            // 
            // LB_AllProjects
            // 
            this.LB_AllProjects.FormattingEnabled = true;
            this.LB_AllProjects.ItemHeight = 16;
            this.LB_AllProjects.Location = new System.Drawing.Point(12, 39);
            this.LB_AllProjects.Name = "LB_AllProjects";
            this.LB_AllProjects.Size = new System.Drawing.Size(206, 308);
            this.LB_AllProjects.TabIndex = 4;
            this.LB_AllProjects.SelectedIndexChanged += new System.EventHandler(this.LB_AllProjects_SelectedIndexChanged);
            this.LB_AllProjects.DoubleClick += new System.EventHandler(this.LB_AllProjects_DoubleClick);
            this.LB_AllProjects.MouseHover += new System.EventHandler(this.LB_AllProjects_MouseHover);
            this.LB_AllProjects.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LB_AllProjects_MouseMove);
            // 
            // TB_LogConsole
            // 
            this.TB_LogConsole.Location = new System.Drawing.Point(648, 39);
            this.TB_LogConsole.Multiline = true;
            this.TB_LogConsole.Name = "TB_LogConsole";
            this.TB_LogConsole.Size = new System.Drawing.Size(472, 308);
            this.TB_LogConsole.TabIndex = 5;
            // 
            // L_Projects
            // 
            this.L_Projects.AutoSize = true;
            this.L_Projects.Location = new System.Drawing.Point(55, 9);
            this.L_Projects.Name = "L_Projects";
            this.L_Projects.Size = new System.Drawing.Size(127, 17);
            this.L_Projects.TabIndex = 6;
            this.L_Projects.Text = "Projects in solution";
            // 
            // L_SelectedProjects
            // 
            this.L_SelectedProjects.AutoSize = true;
            this.L_SelectedProjects.Location = new System.Drawing.Point(257, 9);
            this.L_SelectedProjects.Name = "L_SelectedProjects";
            this.L_SelectedProjects.Size = new System.Drawing.Size(109, 17);
            this.L_SelectedProjects.TabIndex = 7;
            this.L_SelectedProjects.Text = "Projects to build";
            // 
            // L_Services
            // 
            this.L_Services.AutoSize = true;
            this.L_Services.Location = new System.Drawing.Point(462, 9);
            this.L_Services.Name = "L_Services";
            this.L_Services.Size = new System.Drawing.Size(136, 17);
            this.L_Services.TabIndex = 8;
            this.L_Services.Text = "Services for solution";
            // 
            // L_Console
            // 
            this.L_Console.AutoSize = true;
            this.L_Console.Location = new System.Drawing.Point(824, 9);
            this.L_Console.Name = "L_Console";
            this.L_Console.Size = new System.Drawing.Size(85, 17);
            this.L_Console.TabIndex = 9;
            this.L_Console.Text = "Log console";
            // 
            // B_OpenDeployDirectory
            // 
            this.B_OpenDeployDirectory.Location = new System.Drawing.Point(840, 354);
            this.B_OpenDeployDirectory.Name = "B_OpenDeployDirectory";
            this.B_OpenDeployDirectory.Size = new System.Drawing.Size(110, 46);
            this.B_OpenDeployDirectory.TabIndex = 10;
            this.B_OpenDeployDirectory.Text = "Open Deploy Directory";
            this.B_OpenDeployDirectory.UseVisualStyleBackColor = true;
            this.B_OpenDeployDirectory.Click += new System.EventHandler(this.B_OpenDeployDirectory_Click);
            // 
            // B_StartStopService
            // 
            this.B_StartStopService.Location = new System.Drawing.Point(722, 354);
            this.B_StartStopService.Name = "B_StartStopService";
            this.B_StartStopService.Size = new System.Drawing.Size(112, 46);
            this.B_StartStopService.TabIndex = 11;
            this.B_StartStopService.Text = "Start/Stop Service";
            this.B_StartStopService.UseVisualStyleBackColor = true;
            this.B_StartStopService.Click += new System.EventHandler(this.B_StartStopService_Click);
            // 
            // B_AttachToProcess
            // 
            this.B_AttachToProcess.Enabled = false;
            this.B_AttachToProcess.Location = new System.Drawing.Point(956, 354);
            this.B_AttachToProcess.Name = "B_AttachToProcess";
            this.B_AttachToProcess.Size = new System.Drawing.Size(112, 46);
            this.B_AttachToProcess.TabIndex = 12;
            this.B_AttachToProcess.Text = "Attach To Process";
            this.B_AttachToProcess.UseVisualStyleBackColor = true;
            this.B_AttachToProcess.Click += new System.EventHandler(this.B_AttachToProcess_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1132, 403);
            this.Controls.Add(this.B_AttachToProcess);
            this.Controls.Add(this.B_StartStopService);
            this.Controls.Add(this.B_OpenDeployDirectory);
            this.Controls.Add(this.L_Console);
            this.Controls.Add(this.L_Services);
            this.Controls.Add(this.L_SelectedProjects);
            this.Controls.Add(this.L_Projects);
            this.Controls.Add(this.TB_LogConsole);
            this.Controls.Add(this.LB_AllProjects);
            this.Controls.Add(this.B_Start);
            this.Controls.Add(this.LB_Services);
            this.Controls.Add(this.LB_SelectedProjects);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWindow";
            this.Text = "Service Debug Helper";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.ResizeBegin += new System.EventHandler(this.MainWindow_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.MainWindow_ResizeEnd);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListBox LB_SelectedProjects;
        private System.Windows.Forms.ListBox LB_Services;
        private System.Windows.Forms.Button B_Start;
        private System.Windows.Forms.ListBox LB_AllProjects;
        private System.Windows.Forms.TextBox TB_LogConsole;
        private System.Windows.Forms.Label L_Projects;
        private System.Windows.Forms.Label L_SelectedProjects;
        private System.Windows.Forms.Label L_Services;
        private System.Windows.Forms.Label L_Console;
        private System.Windows.Forms.Button B_OpenDeployDirectory;
        private System.Windows.Forms.Button B_StartStopService;
        private System.Windows.Forms.Button B_AttachToProcess;
    }
}

