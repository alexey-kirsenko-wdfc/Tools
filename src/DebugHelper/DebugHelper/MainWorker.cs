using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using EnvDTE;
using System.Management;
using System.Collections.Concurrent;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;
using Microsoft.Build.BuildEngine;
using System.Windows.Forms;

namespace DebugHelper
{
    public class MainWorker
    {
        public List<EnvDTE.Project> Projects;
        public List<ServiceController> Services;
        public List<EnvDTE.Project> SelectedProjects;

        public Dictionary<string, string> SelectedProjectPaths;
        public ConcurrentDictionary<string, string> ServicePaths = new ConcurrentDictionary<string, string>();

        public ServiceController ActiveService;
        public EnvDTE.Project ActiveProject;

        public event MessageEventHandler MessageReceived;
        public delegate void MessageEventHandler(string text, bool newLine = true);

        public event WorkFinishedEventHander WorkFinished;
        public delegate void WorkFinishedEventHander(int? processId, bool success);

        public event StartStopServiceEventHandler ServiceProblem;
        public delegate void StartStopServiceEventHandler(bool start);

        public event ActiveServiceStatusChangedEventHandler ServiceStatusChanged;
        public delegate void ActiveServiceStatusChangedEventHandler(bool started);                

        private EnvDTE.Process _attachedProcess;
        public bool IsAttached { get { return _attachedProcess != null; } }

        private readonly TimeSpan FIRST_WARNING = TimeSpan.FromMinutes(1);
        private readonly TimeSpan BREAK_WARNING = TimeSpan.FromMinutes(5);

        public void StartOperation()
        {
            var projectsBuilt = TryBuildProjects();
            if (!projectsBuilt)
            {
                WorkFinished(null, false);
                return;
            }

            var serviceStopped = TryStopService();
            if (!serviceStopped)
            {
                WorkFinished(null, false);
                return;
            }
            
            var upsertSuccess = TryUpsertDlls();
            if (!upsertSuccess)
            {
                WorkFinished(null, false);
                return;
            }

            var serviceStarted = TryStartService();
            if (!serviceStarted)
            {
                WorkFinished(null, false);
                return;
            }

            var attached = TryAttachToProcess();
            if (!attached)
            {
                WorkFinished(null, false);
                return;
            }            

            WorkFinished(_attachedProcess.ProcessID, true);
        }

        public void StartStopService()
        {
            if (ActiveService.Status == ServiceControllerStatus.Stopped)
            {
                var started = TryStartService();
                if (started)
                {
                    ServiceStatusChanged(true);
                }

            }
            else if (ActiveService.Status == ServiceControllerStatus.Running)
            {
                var stopped = TryStopService();
                if (stopped)
                {
                    ServiceStatusChanged(false);
                }
            }
            else
            {
                MessageBox.Show("Service is in start/stop process, please wait", "Service is busy", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void AttachToProcess(int processId)
        {
            var r = Package.GetGlobalService(typeof(SDTE)) as DTE;
            _attachedProcess = r.Debugger.LocalProcesses.Cast<EnvDTE.Process>().FirstOrDefault(process => process.ProcessID == processId);
            _attachedProcess.Attach();
        }

        public int? GetActiveServiceProcessId()
        {
            var r = Package.GetGlobalService(typeof(SDTE)) as DTE;
            ManagementObject service = new ManagementObject(@"Win32_service.Name='" + ActiveService.ServiceName + "'");
            object o = service.GetPropertyValue("ProcessId");

            if (o != null)
            {
                int processId = (int)((UInt32)o);
                return processId;
            }
            else
            {
                return null;
            }            
        }

        private bool TryAttachToProcess()
        {
            try
            {
                var r = Package.GetGlobalService(typeof(SDTE)) as DTE;
                var processId = GetActiveServiceProcessId();
                _attachedProcess = r.Debugger.LocalProcesses.Cast<EnvDTE.Process>().FirstOrDefault(process => process.ProcessID == processId);
                _attachedProcess.Attach();
                return true;
            }
            catch (Exception ex)
            {
                Message(string.Format("Can't attach to service process, exception message - {0}", ex.Message));
                return false;
            }           
        }

        private bool TryStopService()
        {
            if (ActiveService.Status == ServiceControllerStatus.Running)
            {
                ActiveService.Stop();
            }
            
            var firstWarning = false;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            Message(string.Format("Stopping service {0}...   ", ActiveService.DisplayName));
            while (ActiveService.Status != ServiceControllerStatus.Stopped)
            {
                if (stopWatch.Elapsed > FIRST_WARNING && !firstWarning)
                {
                    Message(string.Format("Service {0} is taking too long to stop...", ActiveService.DisplayName));
                    firstWarning = true;
                }

                if (stopWatch.Elapsed > BREAK_WARNING)
                {
                    Message(string.Format("Service {0} can not be stopped in timely fashion.", ActiveService.DisplayName));
                    ServiceProblem(true);
                    return false;
                }

                ActiveService.Refresh();
            }

            Message("Stopped", false);
            return true;
        }

        private bool TryStartService()
        {
            var firstWarning = false;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            ActiveService.Start();

            var statusChanged = false;
            Message(string.Format("Starting service {0}...   ", ActiveService.DisplayName));
            while (ActiveService.Status != ServiceControllerStatus.Running)
            {
                if (!statusChanged && ActiveService.Status == ServiceControllerStatus.StartPending)
                {
                    statusChanged = true;
                }

                if (stopWatch.Elapsed > FIRST_WARNING && !firstWarning)
                {
                    Message(string.Format("Service {0} is taking too long to start...", ActiveService.DisplayName));
                    firstWarning = true;
                }

                if (stopWatch.Elapsed > BREAK_WARNING)
                {
                    Message(string.Format("Service {0} can not be started in timely fashion.", ActiveService.DisplayName));
                    System.Threading.Thread.Sleep(1000);
                    ServiceProblem(false);
                    return false;
                }

                if (statusChanged && ActiveService.Status == ServiceControllerStatus.Stopped)
                {
                    Message(string.Format("Service {0} failed to start.", ActiveService.DisplayName));
                    System.Threading.Thread.Sleep(1000);
                    ServiceProblem(false);
                    return false;
                }

                ActiveService.Refresh();
            }

            Message("Started", false);
            return true;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        
        private const int WM_VSCROLL = 277; // Vertical scroll
        private const int SB_PAGEDOWN = 3; // Scrolls one page down

        [DllImport("User32.Dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        const uint WM_KEYDOWN = 0x100;

        const int WM_a = 0x41;
        const int WM_b = 0x42;
        const int WM_c = 0x43;

        
        uint KEY_DOWN = 0x0100;
        uint KEY_UP = 0x0101;
        IntPtr CTRL_KEY = new IntPtr(0x11);
        IntPtr END_KEY = new IntPtr(0x23);

        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

        public void OpenLogs()
        {
            var target = ServicePaths[ActiveService.DisplayName] + "Logs\\Log.txt";
            var process = new System.Diagnostics.Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Environment.GetEnvironmentVariable("windir").ToString() + "\\system32\\notepad.exe";
            startInfo.Arguments = target;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.StartInfo = startInfo;
            process.Start();
            System.Threading.Thread.Sleep(500);

            var handle = process.MainWindowHandle;

            SetForegroundWindow(handle);
            SendMessage(handle, (int)KEY_DOWN, CTRL_KEY, IntPtr.Zero);
            SendMessage(handle, (int)KEY_DOWN, END_KEY, IntPtr.Zero);
            SendMessage(handle, (int)KEY_UP, END_KEY, IntPtr.Zero);
            SendMessage(handle, (int)KEY_UP, CTRL_KEY, IntPtr.Zero);
            SetForegroundWindow(handle);
            SendKeys.SendWait("^{END}");
        }

        public void Detach()
        {
            if (IsAttached)
            {
                try
                {
                    _attachedProcess.Detach();
                }
                catch
                {
                    _attachedProcess = null;
                }                
            }
        }
        
        public void Message(string text, bool newLine = true)
        {
            MessageReceived(text, newLine);
        }

        private bool TryUpsertDlls()
        {
            var success = false;
            foreach (var path in SelectedProjectPaths)
            {
                success = GetAndCopyDlls(path);
                if (success)
                {
                    Message(string.Format("Upserted DLL & PDB for {0}", path.Key));
                }
                else
                {
                    Message(string.Format("Fatally failed to copy files for {0}", path.Key));
                }
            }

            return success;
        }

        private bool GetAndCopyDlls(KeyValuePair<string, string> path, int? retryCounter = null)
        {
            var sourceDll = path.Value + path.Key + ".dll";
            var sourcePdb = path.Value + path.Key + ".pdb";

            var targetDirectory = ServicePaths[ActiveService.DisplayName];
            var targetDll = targetDirectory + path.Key + ".dll";
            var targetPdb = targetDirectory + path.Key + ".pdb";

            var copied = TryCopyFiles(sourceDll, targetDll, sourcePdb, targetPdb);
            if (copied == null || retryCounter > 10)
            {
                return false;
            }

            if (copied == false)
            {
                if (retryCounter == null)
                {
                    retryCounter = 1;
                }

                System.Threading.Thread.Sleep(1000);
                copied = GetAndCopyDlls(path, retryCounter);
            }
                        
            return true;
        }
        
        private bool? TryCopyFiles(string sourceDll, string targetDll, string sourcePdb, string targetPdb)
        {
            try
            {
                File.Copy(sourceDll, targetDll, true);
                File.Copy(sourcePdb, targetPdb, true);
                return true;
            }
            catch (FileNotFoundException)
            {
                Message(string.Format("Got FileNotFoundException, breaking"));
                return null;
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType();
                
                Message(string.Format("Got exception of type {0}", exceptionType));
                Message(string.Format("File locked, attempting retry..."));
                return false;
            }          
            
        }

        private bool TryBuildProjects()
        {
            SelectedProjectPaths = new Dictionary<string, string>();
            var successfullBuild = true;

            foreach (var project in SelectedProjects)
            {
                successfullBuild = TryBuild(project.FullName, project.Name);

                var paths = project.FullName.Split('\\');
                var debugPath = project.FullName.Replace(paths[paths.Length - 1], "bin\\Debug\\");

                SelectedProjectPaths.Add(project.Name, debugPath);
            };

            if (successfullBuild)
            {
                Message("Project building completed!");
                return true;
            }
            else
            {
                Message("Project building finished with errors!");
                return false;
            }            
        }

        private bool TryBuild(string projectFile, string projectName)
        {
            Message(string.Format("Building project {0}...   ", projectName));
            var pc = new ProjectCollection();

            var parts = projectFile.Split('\\');
            var solution = projectFile.Replace(parts[parts.Length - 1], "").Replace(parts[parts.Length - 2], "");
            solution = solution.Remove(solution.Length - 1);
           
            pc.SetGlobalProperty("Configuration", "Debug");
            pc.SetGlobalProperty("Platform", "Any CPU");
            pc.SetGlobalProperty("EnableNuGetPackageRestore", "true");
            pc.SetGlobalProperty("SolutionDir", solution);
            pc.SetGlobalProperty("OutputPath", @"bin\Debug\");
            var buildParams = new BuildParameters(pc);
            buildParams.Loggers = new List<ILogger>() { new FileLogger() { Verbosity = LoggerVerbosity.Normal, Parameters = @"logfile=C:\deblog.txt" } };
            
            var buildResult = BuildManager.DefaultBuildManager.Build(buildParams, new BuildRequestData(new ProjectInstance(projectFile, pc.GlobalProperties, null), new string[] { "Build" }));
            
            if (buildResult.OverallResult == BuildResultCode.Failure)
            {
                Message(buildResult.OverallResult.ToString(), false);

                if (buildResult.Exception != null)
                {
                    var message = buildResult.Exception.Message;
                    Message(string.Format("Error on building project {0} - {1}", projectName, message));
                }
                else
                {
                    Message(string.Format("Error on building project {0}", projectName));
                }
                
                return false;
            }

            Message(buildResult.OverallResult.ToString(), false);
            return true;
        }        
    }
}
