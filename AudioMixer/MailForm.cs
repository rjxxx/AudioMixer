using Audio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UsbLibrary;
using System.Management;

namespace AudioMixer
{
    public partial class MailForm : Form
    {
        AudioController controller = new AudioController();
        public static UsbHidPort hidPort = new UsbHidPort();

        
        public MailForm()
        {
           

            hidPort.OnSpecifiedDeviceArrived += new System.EventHandler(HidPort_OnSpecifiedDeviceArrived);
            hidPort.OnSpecifiedDeviceRemoved += new System.EventHandler(HidPort_OnSpecifiedDeviceRemoved);


            hidPort.VendorId = 0x2341;
            hidPort.ProductId = 0x8036;
            hidPort.Open(true);
            InitializeComponent();


        }

        private static void HidPort_OnSpecifiedDeviceRemoved(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void HidPort_OnSpecifiedDeviceArrived(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            hidPort.RegisterHandle(Handle);
        }

        protected override void WndProc(ref Message m)
        {
            hidPort.ParseMessages(m.Msg, m.WParam);
            base.WndProc(ref m);
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                controller.AddProgram("Program1", openFileDialog1.FileName);
                trackBar1.Value = controller.CurrentVolume("Program1");
                label.Text = openFileDialog1.FileName;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            controller.VolumeSet("Program1", trackBar1.Value);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            hidPort.WriteOutputReport(0, new byte[] { 0, 1, 2, 3 });
        }

     
       
    }
}
