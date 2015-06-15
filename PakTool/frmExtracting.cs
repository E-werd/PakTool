using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace PakTool
{
    public partial class frmExtracting : Form
    {
        private static frmExtracting mInstance;

        public static void Create()
        {
            var t = new Thread(() =>
            {
                mInstance = new frmExtracting();
                mInstance.FormClosed += (s, e) => mInstance = null;
                Application.Run(mInstance);
            });
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();
        }

        public static void Destroy()
        {
            try { if (mInstance != null) mInstance.Invoke(new Action(() => mInstance.Close())); }
            catch (Exception) { }
        }

        public frmExtracting()
        {
            InitializeComponent();
        }

        private void frmExtracting_Load(object sender, EventArgs e)
        {

        }
    }
}
