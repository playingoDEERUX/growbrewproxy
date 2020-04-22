using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GrowbrewProxy
{
    public partial class PlayerForm : Form
    {
        public PlayerForm()
        {
            InitializeComponent();
        }

        public struct ControlWithMetaData
        {
            public int netID;
            public Control control;
        }

        public static int updatedHeight = 0;

        public static bool requireClear = false;

        public static List<ControlWithMetaData> controlsToLoad = new List<ControlWithMetaData>();
       
        private void PlayerForm_Load(object sender, EventArgs e)
        {
            updateControls.Start();
        }

        private void timer1_Tick(object sender, EventArgs e) // todo - button handlers knowing which button is which (using netids)
        {
            updatedHeight = this.Height;
            World worldMap = MainForm.messageHandler.worldMap;
            if (worldMap != null) this.Text = "All players in " + worldMap.currentWorld;

            if (requireClear)
            {
                requireClear = false;
                playerBox.Controls.Clear();
            }

            lock (controlsToLoad)
            {
                foreach (ControlWithMetaData ctrl in controlsToLoad)
                    playerBox.Controls.Add(ctrl.control);                
            }
        }
    }
}
