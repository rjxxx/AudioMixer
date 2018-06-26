using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using UsbLibrary;
using Audio;
using System.Configuration;
using System.Collections.Specialized;
using System.IO;

namespace AudioMixer
{
    

    public partial class MainForm: Form
    {
        private static Object lockObjMain = new Object();
        public static UsbHidPort usb = new UsbHidPort(0x8036, 0x2341);
        AudioController controller = new AudioController();
        private bool isClose = false;
        BlogSettings settings = new BlogSettings();


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
        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Hide();
            for (int i = 0; i < Properties.Settings.Default.Paths.Count; i++)
            {
                if (File.Exists(Properties.Settings.Default.Paths[i]))
                {
                    AddProgram(i, Properties.Settings.Default.Paths[i]);
                } else
                {
                    AddProgram(i, null);
                }
                
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isClose)
            {
                for (int i = 1; i <= controller.CountProgram; i++)
                {
                    usb.SpecifiedDevice?.SendData(USBCommand.CreateCommandVolume(0, (byte)i));
                }

                for (int i = 0; i < controller.CountProgram; i++)
                {
                    if (Properties.Settings.Default.Paths.Count > i)
                    {
                        Properties.Settings.Default.Paths[i] = controller.GetAudioProgram(i).ExePath;
                    } else
                    {
                        Properties.Settings.Default.Paths.Add(controller.GetAudioProgram(i).ExePath);
                    }
                }
                Properties.Settings.Default.Save();
                return;
            }

            this.Hide();
            e.Cancel = true;
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
            usb.SpecifiedDevice?.SendData(USBCommand.CreateCommandVolume((byte)trackBar.Value, numberProgram));

        }

        private void AddProgram(int index, string programPath)
        {
            byte numberProgram;
            TrackBar trackBar;
            Label label;
            switch (index)
            {
                case 0:
                    numberProgram = 0;
                    label = label1;
                    trackBar = trackBar1;
                    break;
                case 1:
                    numberProgram = 1;
                    label = label2;
                    trackBar = trackBar2;
                    break;
                case 2:
                    numberProgram = 2;
                    label = label3;
                    trackBar = trackBar3;
                    break;
                default:
                    return;
            }

            controller.AddProgram("program_" + numberProgram, programPath);
            trackBar.Value = controller.GetAudioProgram(numberProgram).Volume;
            usb.SpecifiedDevice?.SendData(USBCommand.CreateCommandVolume((byte)trackBar.Value, numberProgram));
            label.Text = programPath;
        }
        private void Program_Button_Click(object sender, EventArgs e)
        {
            string programPath = OpenDialog();
            if (programPath == "") return;
            int index;
            switch (((Button)sender).Name)
            {
                case "program_1_button":
                    index = 0;
                    break;
                case "program_2_button":
                    index = 1;
                    break;
                case "program_3_button":
                    index = 2;
                    break;
                default:
                    return;
            }
            AddProgram(index, programPath);
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

        private void SendVolumeCommand(byte numberProgram, byte volume)
        {
            byte[] command = new byte[64];
            command[0] = USBCommand.COMMAND_SET_VOLUME;
            command[1] = numberProgram;
            command[2] = volume;
            usb.SpecifiedDevice?.SendData(command);

        }
        private void SendMuteCommand(byte numberProgram, bool isMute)
        {
            byte[] command = new byte[64];
            command[0] = USBCommand.COMMAND_SET_MUTE;
            command[1] = (byte) (isMute ? 1 : 0);
            usb.SpecifiedDevice?.SendData(command);
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isClose = true;
            this.Close();
        }

        
    }
}
