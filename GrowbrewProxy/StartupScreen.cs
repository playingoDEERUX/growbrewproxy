using System;
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
            Invoke(new Action(() => { Close(); }));
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