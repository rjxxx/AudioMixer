using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Drawing;
using System.Windows.Forms;


namespace UsbLibrary
{

    public partial class UsbHidPort
    {
        //private memebers
        private int                             ProductID;
        private int                             VendorID;
        private Guid                            deviceClass;
        private IntPtr                          usb_event_handle;
        private SpecifiedDevice                 specifiedDevice;
        private IntPtr                          handle;
        //events
        /// <summary>
        /// This event will be triggered when the device you specified is pluged into your usb port on
        /// the computer. And it is completly enumerated by windows and ready for use.
        /// </summary>
        public event EventHandler               OnSpecifiedDeviceArrived;

        /// <summary>
        /// This event will be triggered when the device you specified is removed from your computer.
        /// </summary>
        public event EventHandler               OnSpecifiedDeviceRemoved;

        /// <summary>
        /// This event will be triggered when a device is pluged into your usb port on
        /// the computer. And it is completly enumerated by windows and ready for use.
        /// </summary>
        public event EventHandler               OnDeviceArrived;

        /// <summary>
        /// This event will be triggered when a device is removed from your computer.
        /// </summary>
        public event EventHandler               OnDeviceRemoved;

        /// <summary>
        /// This event will be triggered when data is recieved from the device specified by you.
        /// </summary>
        public event DataRecievedEventHandler   OnDataRecieved;

        /// <summary>
        /// This event will be triggered when data is send to the device. 
        /// It will only occure when this action wass succesfull.
        /// </summary>
        public event EventHandler               OnDataSend;

        public UsbHidPort(int ProductID, int VendorID)
        {
            //initializing in initial state
            this.ProductID = 0;
            this.VendorID = 0;
            specifiedDevice = null;
            deviceClass = Win32Usb.HIDGuid; 
        }
       
        public int ProductId{
            get { return this.ProductID; }
            set { this.ProductID = value; }
        }

        public int VendorId
        {
            get { return this.VendorID; }
            set { this.VendorID = value; }
        }

        public Guid DeviceClass
        {
            get { return deviceClass; }
        }

        public SpecifiedDevice SpecifiedDevice
        {
            get { return this.specifiedDevice; }
        }

        /// <summary>
        /// Registers this application, so it will be notified for usb events.  
        /// </summary>
        /// <param name="Handle">a IntPtr, that is a handle to the application.</param>
        /// <example> This sample shows how to implement this method in your form.
        /// <code> 
        ///protected override void OnHandleCreated(EventArgs e)
        ///{
        ///    base.OnHandleCreated(e);
        ///    usb.RegisterHandle(Handle);
        ///}
        ///</code>
        ///</example>
        public void RegisterHandle(IntPtr Handle){
            usb_event_handle = Win32Usb.RegisterForUsbEvents(Handle, deviceClass);
            this.handle = Handle;
            //Check if the device is already present.
            CheckDevicePresent();
        }

        /// <summary>
        /// Unregisters this application, so it won't be notified for usb events.  
        /// </summary>
        /// <returns>Returns if it wass succesfull to unregister.</returns>
        public bool UnregisterHandle()
        {
            if (this.handle != null)
            {
                return Win32Usb.UnregisterForUsbEvents(this.handle);
            }
            
            return false;
        }

        /// <summary>
        /// This method will filter the messages that are passed for usb device change messages only. 
        /// And parse them and take the appropriate action 
        /// </summary>
        /// <param name="m">a ref to Messages, The messages that are thrown by windows to the application.</param>
        /// <example> This sample shows how to implement this method in your form.
        /// <code> 
        ///protected override void WndProc(ref Message m)
        ///{
        ///    usb.ParseMessages(ref m);
        ///    base.WndProc(ref m);	    // pass message on to base form
        ///}
        ///</code>
        ///</example>
        public void ParseMessages(int msg, IntPtr wParam)
        {
            if (msg == Win32Usb.WM_DEVICECHANGE)	// we got a device change message! A USB device was inserted or removed
            {
                switch (wParam.ToInt32())	// Check the W parameter to see if a device was inserted or removed
                {
                    case Win32Usb.DEVICE_ARRIVAL:	// inserted
                        if (OnDeviceArrived != null)
                        {
                            OnDeviceArrived(this, new EventArgs());
                            CheckDevicePresent();
                        }
                        break;
                    case Win32Usb.DEVICE_REMOVECOMPLETE:	// removed
                        if (OnDeviceRemoved != null)
                        {
                            OnDeviceRemoved(this, new EventArgs());
                            CheckDevicePresent();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Checks the devices that are present at the moment and checks if one of those
        /// is the device you defined by filling in the product id and vendor id.
        /// </summary>
        public void CheckDevicePresent()
        {
            try
            {
                //Mind if the specified device existed before.
                bool history = false;
                if(specifiedDevice != null ){
                    history = true;
                }

                specifiedDevice = SpecifiedDevice.FindSpecifiedDevice(this.VendorID, this.ProductID);	// look for the device on the USB bus
                if (specifiedDevice != null)	// did we find it?
                {
                    if (OnSpecifiedDeviceArrived != null)
                    {
                        this.OnSpecifiedDeviceArrived(this, new EventArgs());
                        specifiedDevice.DataRecieved += new DataRecievedEventHandler(OnDataRecieved);
                        specifiedDevice.DataSent += new DataSendEventHandler(OnDataSend);
                    }
                }
                else
                {
                    if (OnSpecifiedDeviceRemoved != null && history)
                    {
                        this.OnSpecifiedDeviceRemoved(this, new EventArgs());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void DataRecieved(object sender, DataRecievedEventArgs args)
        {
            this.OnDataRecieved?.Invoke(sender, args);
        }

        private void DataSend(object sender, DataSendEventArgs args)
        {
            this.OnDataSend?.Invoke(sender, args);
        }
    }
}
