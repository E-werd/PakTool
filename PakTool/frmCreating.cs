using System;
using System.Windows.Forms;
using System.Threading;

namespace PakTool
{
    public partial class frmCreating : Form
    {
        private static frmCreating mInstance;

        public static void Create()
        {
            var t = new Thread(() =>
            {
                mInstance = new frmCreating();
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

        public frmCreating()
        {
            InitializeComponent();
        }
    }
}
