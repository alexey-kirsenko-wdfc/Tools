using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Management;
using DebugHelper;
using static DebugHelper.MainWorker;
using System.Diagnostics;
using System.Drawing;

namespace DllUpsertionMenu
{
    public partial class MainWindow : Form
    {
        private List<EnvDTE.Project> Projects;
        private List<ServiceController> Services;
        private MainWorker Worker { get; set; }

        private int InitialWindowWidth { get; set; }
        private int InitialWindowHeight { get; set; }

        private const int MinimalWindowWidth = 1150;
        private const int MinimalWindowHeight = 450;

        private Task WorkerProcess;
        private bool _serviceProblem;
        private int? _processId;

        private void SetText(string text, bool newLine = true)
        {
            if (TB_LogConsole.InvokeRequired)
            {
                var handler = new MessageEventHandler(SetText);
                Invoke(handler, new object[] { text, newLine });
            }
            else
            {
                if (newLine)
                {
                    TB_LogConsole.AppendText(Environment.NewLine + text);
                }
                else
                {
                    TB_LogConsole.AppendText(text);
                }                
            }            
        }

        private void WorkFinished(int? processId, bool success)
        {
            if (B_Start.InvokeRequired)
            {
                var handler = new WorkFinishedEventHander(WorkFinished);
                Invoke(handler, new object[] { processId, success });
            }
            else
            {
                SetText("Work Finished");
                if (processId != null)
                {
                    SetText(string.Format("Attached to ProcessId ({0})", processId));
                    _processId = processId;                    
                }
            }

            if (_serviceProblem)
            {
                _serviceProblem = false;
                CheckLogs();
            }

            ChangeControlsState(true);
        }


        private void CheckLogs()
        {
            var dialogResult = MessageBox.Show("There is a problem with starting service, do you want to check logs?", "Service start failure", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                Worker.OpenLogs();                
            }
        }
        
        private void ServiceProblem(bool start)
        {
            _serviceProblem = true;            
        }

        private void ServiceStatusChanged(bool started)
        {
            if (started)
            {
                B_AttachToProcess.Enabled = true;
                _processId = Worker.GetActiveServiceProcessId();                
            }
            else
            {
                B_AttachToProcess.Enabled = false;
                _processId = null;
            }
        }

        public MainWindow(string name, IEnumerable<EnvDTE.Project> allProjects, IEnumerable<ServiceController> services)
        {
            InitializeComponent();
            Worker = new MainWorker();
            Worker.MessageReceived += SetText;
            Worker.WorkFinished += WorkFinished;
            Worker.ServiceProblem += ServiceProblem;
            Worker.ServiceStatusChanged += ServiceStatusChanged;
                        
            Name = name;
            Projects = allProjects.ToList();
            Services = services.ToList();
            Worker.SelectedProjects = new List<EnvDTE.Project>();
                    
            FillListBoxes();
        }
        
        private void FillListBoxes()
        {
            foreach (var project in Projects)
            {
                LB_AllProjects.Items.Add(project.Name);
            }

            Worker.Services = Services.ToList();
                        
            foreach (var service in Worker.Services)
            {
                LB_Services.Items.Add(service.DisplayName);
            }

            var task = new Task(FillListOfServicePaths);
            task.Start();
            task.Wait();

            LB_Services.SelectedIndex = 0;
        }

        private void FillListOfServicePaths()
        {
            Parallel.ForEach(Worker.Services, (service) =>
            {
                GetPathOfService(service.DisplayName);
            });
        }
        
        public void GetPathOfService(string serviceName)
        {
            WqlObjectQuery wqlObjectQuery = new WqlObjectQuery(string.Format("SELECT * FROM Win32_Service WHERE Name = '{0}'", serviceName));
            ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(wqlObjectQuery);
            ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();

            foreach (ManagementObject managementObject in managementObjectCollection)
            {
                var servicePath = managementObject.GetPropertyValue("PathName").ToString();
                var pathArray = servicePath.Split('\\');
                var path = servicePath.Replace(pathArray[pathArray.Length - 1], "").Remove(0, 1);

                Worker.ServicePaths.TryAdd(serviceName, path);
            }           
        }

        private void LB_Services_SelectedIndexChanged(object sender, EventArgs e)
        {
            Worker.ActiveService = Worker.Services[LB_Services.SelectedIndex];            
        }
        
        private void LB_AllProjects_DoubleClick(object sender, EventArgs e)
        {
            Worker.SelectedProjects.Add(Worker.ActiveProject);
            Worker.SelectedProjects = Worker.SelectedProjects.OrderBy(x => x.Name).ToList();

            LB_SelectedProjects.Items.Clear();
            foreach (var project in Worker.SelectedProjects)
            {
                LB_SelectedProjects.Items.Add(project.Name);
            }            

            LB_AllProjects.Items.Remove(Worker.ActiveProject.Name);
            Projects.Remove(Worker.ActiveProject);            
        }

        private void LB_SelectedProjects_DoubleClick(object sender, EventArgs e)
        {
            Projects.Add(Worker.ActiveProject);
            Projects = Projects.OrderBy(x => x.Name).ToList();

            LB_AllProjects.Items.Clear();
            foreach (var project in Projects)
            {
                LB_AllProjects.Items.Add(project.Name);
            }

            LB_SelectedProjects.Items.Remove(Worker.ActiveProject.Name);
            Worker.SelectedProjects.Remove(Worker.ActiveProject);
        }

        private void LB_AllProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (LB_AllProjects.SelectedIndex == -1)
            {
                return;
            }

            Worker.ActiveProject = Projects[LB_AllProjects.SelectedIndex];
            LB_SelectedProjects.SelectedIndex = -1;
        }

        private void LB_SelectedProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (LB_SelectedProjects.SelectedIndex == -1)
            {
                return;
            }

            Worker.ActiveProject = Worker.SelectedProjects[LB_SelectedProjects.SelectedIndex];
            LB_AllProjects.SelectedIndex = -1;
        }

        private void B_Start_Click(object sender, EventArgs e)
        {
            try
            {
                if (Worker.IsAttached)
                {
                    SetText("Debugger is attached, stopping...");
                    Worker.Detach();
                }

                TB_LogConsole.Clear();
                SetText("Starting operation...", false);

                WorkerProcess = new Task(Worker.StartOperation);
                WorkerProcess.Start();
                ChangeControlsState(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error on execution: {ex.Message}", "Exception", MessageBoxButtons.OK);
            }            
        }

        private void ChangeControlsState(bool enabled)
        {
            B_Start.Enabled = enabled;
            LB_AllProjects.Enabled = enabled;
            LB_SelectedProjects.Enabled = enabled;
            LB_Services.Enabled = enabled;
            B_StartStopService.Enabled = enabled;
            B_AttachToProcess.Enabled = enabled;
        }

        private void B_OpenDeployDirectory_Click(object sender, EventArgs e)
        {
            Process.Start(Worker.ServicePaths[Worker.ActiveService.DisplayName]);
        }

        private void B_StartStopService_Click(object sender, EventArgs e)
        {
            try
            {
                Worker.Detach();
                Worker.StartStopService();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error on execution: {ex.Message}", "Exception", MessageBoxButtons.OK);
            }
        }

        private void B_AttachToProcess_Click(object sender, EventArgs e)
        {
            try
            {
                Worker.AttachToProcess(_processId.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error on execution: {ex.Message}", "Exception", MessageBoxButtons.OK);
            }
        }

        private ToolTip ToolTip = new ToolTip();

        private void LB_AllProjects_MouseHover(object sender, EventArgs e)
        {
            if (ToolTip.Active)
            {
                return;
            }
            
            var localPoint = LB_AllProjects.PointToClient(Cursor.Position);
            var index = LB_AllProjects.IndexFromPoint(localPoint);
            if (index != -1)
            {
                var item = LB_AllProjects.Items[index].ToString();
                localPoint.Y += 20;

                ToolTip.Show(item, this, localPoint);
                ToolTip.Active = true;          
            }            
        }

        private void LB_AllProjects_MouseMove(object sender, MouseEventArgs e)
        {
            if (ToolTip != null)
                ToolTip.Hide(this);

            ToolTip.Active = false;
        }

        private void MainWindow_ResizeBegin(object sender, EventArgs e)
        {
            InitialWindowWidth = Width;
            InitialWindowHeight = Height;
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            InitialWindowWidth = MinimalWindowWidth;
            InitialWindowHeight = MinimalWindowHeight;
        }

        private void MainWindow_ResizeEnd(object sender, EventArgs e)
        {
            if (Width < MinimalWindowWidth)
                Width = MinimalWindowWidth;

            if (Height < MinimalWindowHeight)
                Height = MinimalWindowHeight;
            
            var widthDelta = Width - InitialWindowWidth;
            var heightDelta = Height - InitialWindowHeight;

            var widthOffset = widthDelta / 4;
            var heightOffset = heightDelta / 4;
            
            L_Projects.AdjustLocation(widthOffset / 2, 0);
            LB_AllProjects.Width += widthOffset;
            LB_AllProjects.Height += heightDelta;
            B_Start.AdjustLocation(0, heightDelta);
            
            L_SelectedProjects.AdjustLocation(widthOffset + widthOffset / 2, 0);
            LB_SelectedProjects.AdjustLocation(widthOffset, 0);
            LB_SelectedProjects.Width += widthOffset;
            LB_SelectedProjects.Height += heightDelta;
            
            L_Services.AdjustLocation(widthOffset * 2 + widthOffset / 2, 0);
            LB_Services.AdjustLocation(widthOffset * 2, 0);
            LB_Services.Width += widthOffset;
            LB_Services.Height += heightDelta;
            
            L_Console.AdjustLocation(widthOffset * 3 + widthOffset / 2, 0);
            TB_LogConsole.AdjustLocation(widthOffset * 3, 0);
            TB_LogConsole.Width += widthDelta - widthOffset * 3;
            TB_LogConsole.Height += heightDelta;

            B_StartStopService.AdjustLocation(widthDelta, heightDelta);
            B_OpenDeployDirectory.AdjustLocation(widthDelta, heightDelta);
            B_AttachToProcess.AdjustLocation(widthDelta, heightDelta);            
        }       
    }    

    public static class ControlActions
    {
        public static void AdjustLocation<T>(this T element, int widthOffset, int heightOffset) where T : Control
        {
            element.Location = new Point(element.Location.X + widthOffset, element.Location.Y + heightOffset);
        }
    }
}
