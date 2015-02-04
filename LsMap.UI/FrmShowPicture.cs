using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LsMap.UI
{
    public partial class FrmShowPicture : Form
    {
        public static int count = 0;
        public FrmShowPicture()
        {
            InitializeComponent();
        }
        protected override void OnHandleDestroyed(EventArgs e)
        {
            this.BackgroundImage.Dispose();
            base.OnHandleDestroyed(e);
        }
    }
}
