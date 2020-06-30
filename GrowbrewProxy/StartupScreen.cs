using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GrowbrewProxy
{
    public partial class StartupScreen : Form
    {
        public StartupScreen()
        {
            InitializeComponent();
            Task.Run(() => doStartup());
        }

        private void doStartup()
        {
            Thread.Sleep(2000);
            Invoke(new Action(() =>
            {
                this.Close();
            }));
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // not necessary
        }
        private void StartupScreen_Load(object sender, EventArgs e)
        {
           // using this caused some bugs...
        }
    }
}
