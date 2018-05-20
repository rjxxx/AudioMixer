﻿using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using UsbLibrary;
using Audio;


namespace AudioMixer
{
    

    public partial class MainForm: Form
    {
        private UInt16 USBDevProductID = 0x8036;
        private UInt16 USBDevVendorID = 0x2341;
        private static Object lockObjMain = new Object();
        public static UsbHidPort usb = new UsbHidPort();
        AudioController controller = new AudioController();

        

        #region События формы
        public MainForm()
        {
            InitializeComponent();
            Connect();
            for (int i = 0; i < controller.CountProgram; i++)
            {
                controller.GetAudioProgram(i).StepVolume = 0.04f;
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            for (int i = 1; i <= controller.CountProgram; i++)
            {
                usb.SpecifiedDevice.SendData(USBCommand.CreateCommandVolume(0, (byte)i));
            }
        }

        #endregion


        private void trackBar_Scroll(object sender, EventArgs e)
        {
           
            TrackBar trackBar;
            byte numberProgram;
           
            switch (((TrackBar)sender).Name)
            {
                case "trackBar1":
                   
                    numberProgram = 0;
                    trackBar = trackBar1;
                    break;
                case "trackBar2":

                    numberProgram = 1;
                    trackBar = trackBar2;
                    break;
                case "trackBar3":
 
                    numberProgram = 2;
                    trackBar = trackBar3;
                    break;
                default:
                    return;
            }

            controller.GetAudioProgram(numberProgram).Volume = trackBar.Value;
            usb.SpecifiedDevice.SendData(USBCommand.CreateCommandVolume((byte)trackBar.Value, numberProgram));

        }

        private void Program_Button_Click(object sender, EventArgs e)
        {

            string programPath = OpenDialog();
            if (programPath == "") return;
            byte numberProgram;
            TrackBar trackBar;
            Label label;
            switch (((Button)sender).Name)
            {
                case "program_1_button":
                    numberProgram = 0;
                    label = label1;
                    trackBar = trackBar1;
                    break;
                case "program_2_button":
                    numberProgram = 1;
                    label = label2;
                    trackBar = trackBar2;
                    break;
                case "program_3_button":
                    numberProgram = 2;
                    label = label3;
                    trackBar = trackBar3;
                    break;
                default:
                    return;
            }

            controller.AddProgram("program_" + numberProgram, programPath);
            trackBar.Value = controller.GetAudioProgram(numberProgram).Volume;
            usb.SpecifiedDevice.SendData(USBCommand.CreateCommandVolume((byte)trackBar.Value, numberProgram));
            label.Text = programPath;

        }

        private void Send_button_Click(object sender, EventArgs e)
        {
            try
            {
                string text = this.textBox1.Text + " ";
                text.Trim();
                string[] arrText = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                byte[] data = new byte[arrText.Length];
                for (int i = 0; i < arrText.Length; i++)
                {
                    if (arrText[i] != "")
                    {
                        int value = Int32.Parse(arrText[i], System.Globalization.NumberStyles.HexNumber);
                        data[i] = (byte)Convert.ToByte(value);
                    }
                }

                if (usb.SpecifiedDevice != null)
                {
                    usb.SpecifiedDevice.SendData(data);
                }
                else
                {
                    MessageBox.Show("Sorry but your device is not present. Plug it in!! ");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Clear_button_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private string OpenDialog()
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return openFileDialog1.FileName;
            }
            return "";
        }


        #region USB

        private void Connect()
        {
            usb.OnSpecifiedDeviceRemoved += new System.EventHandler(usb_OnSpecifiedDeviceRemoved);
            usb.OnSpecifiedDeviceArrived += new System.EventHandler(usb_OnSpecifiedDeviceArrived);
            usb.OnDeviceArrived += new System.EventHandler(usb_OnDeviceArrived);
            usb.OnDeviceRemoved += new System.EventHandler(usb_OnDeviceRemoved);
            usb.OnDataRecieved += new UsbLibrary.DataRecievedEventHandler(usb_OnDataRecieved);
            usb.OnDataSend += new System.EventHandler(usb_OnDataSend);

            usb.ProductId = USBDevProductID;
            usb.VendorId = USBDevVendorID;
           
        }


        #region События USB
        private void usb_OnSpecifiedDeviceArrived(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(usb_OnSpecifiedDeviceArrived), new object[] {sender, e});
            }
            else
            {
                Dev_StatusLabel.Text = "Подключено!";
                Dev_StatusLabel.BackColor = Color.LightBlue;
            }
        }

        private void usb_OnSpecifiedDeviceRemoved(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(usb_OnSpecifiedDeviceRemoved), new object[] {sender, e});
            }
            else
            {
                Dev_StatusLabel.Text = "Нет подключения!";
                Dev_StatusLabel.BackColor = Color.LightYellow;
            }
        }

        private void usb_OnDeviceArrived(object sender, EventArgs e)
        {
            
        }

        private void usb_OnDeviceRemoved(object sender, EventArgs e)
        {
            
        }

        private void usb_OnDataRecieved(object sender, DataRecievedEventArgs args)
        {
            {
                if (InvokeRequired)
                {
                    try
                    {
                        Invoke(new DataRecievedEventHandler(usb_OnDataRecieved), new object[] { sender, args });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                else
                {
                    lock (lockObjMain)
                    {
                        string rec_data = "Data: ";
                        foreach (byte myData in args.data)
                        {
                            rec_data += myData.ToString("X3") + " ";
                        }
                        this.listBox1.Items.Insert(0, rec_data);


                        byte numberProgram = 0;
                        byte rotation = 0;
                        byte position = 0;
                        if (USBCommand.ParseCommandVolume(args.data, ref numberProgram, ref rotation, ref position))
                        {
                            if (rotation == 0)
                            {
                                controller.GetAudioProgram(numberProgram).Volume = controller.GetAudioProgram(numberProgram).Volume + position * 4;
                            } else if (rotation == 1)
                            {
                                controller.GetAudioProgram(numberProgram).Volume = controller.GetAudioProgram(numberProgram).Volume - position * 4;

                            }
                            TrackBar trackBar;
                            switch (numberProgram)
                            {
                                case 0:
                                    trackBar = trackBar1;
                                    break;
                                case 1:
                                    trackBar = trackBar2;
                                    break;
                                case 2:
                                    trackBar = trackBar3;
                                    break;
                                default:
                                    return;
                            }
                            trackBar.Value = controller.GetAudioProgram(numberProgram).Volume;
                            usb.SpecifiedDevice.SendData(USBCommand.CreateCommandVolume((byte)trackBar.Value, numberProgram));
                        }

                    }
                }
            }
        }

        private void usb_OnDataSend(object sender, EventArgs e)
        {
            this.listBox1.Items.Insert(0, "Some data was send");
        }

        #endregion

        #region Обработка системных событий
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            usb.RegisterHandle(Handle);
        }

        protected override void WndProc(ref Message m)
        {
           usb.ParseMessages(m.Msg, m.WParam);
            base.WndProc(ref m);
        }





        #endregion

        #endregion
        
    }
}
