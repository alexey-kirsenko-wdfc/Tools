using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;

namespace DllUpsertionMenu
{
    public partial class MainWindow : Form
    {
        private IEnumerable<global::EnvDTE.Project> allProjects;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(IEnumerable<global::EnvDTE.Project> allProjects)
        {
            this.allProjects = allProjects;
        }
    }
}
