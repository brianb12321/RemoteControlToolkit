using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RemoteControlToolkitCore.Common;

namespace RemoteControlToolkitCoreServerWCF
{
    public class ServerControl : Form
    {
        private Button btn_start;
        private Button btn_stop;
        private ServiceHost _serviceHost;
        public ServerControl(ServiceHost host)
        {
            _serviceHost = host;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.btn_start = new System.Windows.Forms.Button();
            this.btn_stop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_start
            // 
            this.btn_start.Location = new System.Drawing.Point(12, 12);
            this.btn_start.Name = "btn_start";
            this.btn_start.Size = new System.Drawing.Size(354, 23);
            this.btn_start.TabIndex = 0;
            this.btn_start.Text = "Start";
            this.btn_start.UseVisualStyleBackColor = true;
            this.btn_start.Click += new System.EventHandler(this.btn_start_Click);
            // 
            // btn_stop
            // 
            this.btn_stop.Enabled = false;
            this.btn_stop.Location = new System.Drawing.Point(12, 41);
            this.btn_stop.Name = "btn_stop";
            this.btn_stop.Size = new System.Drawing.Size(354, 23);
            this.btn_stop.TabIndex = 1;
            this.btn_stop.Text = "Stop";
            this.btn_stop.UseVisualStyleBackColor = true;
            this.btn_stop.Click += new System.EventHandler(this.btn_stop_Click);
            // 
            // ServerControl
            // 
            this.ClientSize = new System.Drawing.Size(378, 74);
            this.Controls.Add(this.btn_stop);
            this.Controls.Add(this.btn_start);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "ServerControl";
            this.Text = "Server Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ServerControl_FormClosing);
            this.ResumeLayout(false);

        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            _serviceHost.Open();
            btn_start.Enabled = false;
            btn_stop.Enabled = true;
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ServerControl_FormClosing(object sender, FormClosingEventArgs e)
        {
            _serviceHost.Close();
        }
    }
}
