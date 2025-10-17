using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing;
using MetroFramework.Controls;

namespace SmartDNSProxy_VPN_Client
{
    public class RoundButton : Button
    {

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(new Rectangle(2, 2, this.Width - 5, this.Height - 5));
                this.Region = new Region(path);
            }
            base.OnPaint(e);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // RoundButton
            // 
            this.TabStop = false;
            this.ResumeLayout(false);

        }
    }
}