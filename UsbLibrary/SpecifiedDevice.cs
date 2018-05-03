using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace UsbLibrary
{
    public class DataRecievedEventArgs : EventArgs
    {
        public readonly byte[] data;

        public DataRecievedEventArgs(byte[] data)
        {
            this.data = data;
        }
    }

    public class DataSendEventArgs : EventArgs
    {
        public readonly byte[] data;

        public DataSendEventArgs(byte[] data)
        {
            this.data = data;
        }
    }

    public delegate void DataRecievedEventHandler(object sender, DataRecievedEventArgs args);
    public delegate void DataSendEventHandler(object sender, DataSendEventArgs args);

    public class SpecifiedDevice : HIDDevice
    {
        public event DataRecievedEventHandler DataRecieved;
        public event DataSendEventHandler DataSend;

        public override InputReport CreateInputReport()
        {
            return new SpecifiedInputReport(this);
        }

        // Дописываем и для Feature
        public override FeatureReport CreateFeatureInReport()
        {
            return new SpecifiedFeatureReport(this);
        }

        public static SpecifiedDevice FindSpecifiedDevice(int vendor_id, int product_id, string Product)
        {
            return (SpecifiedDevice)FindDevice(vendor_id, product_id, typeof(SpecifiedDevice), Product);
        }

        protected override void HandleDataReceived(InputReport oInRep)
        {
            // Fire the event handler if assigned
            if (DataRecieved != null)
            {
                SpecifiedInputReport report = (SpecifiedInputReport)oInRep;
                DataRecieved(this, new DataRecievedEventArgs(report.Data));
            }
        }

        public void SendData(byte[] data)
        {
            SpecifiedOutputReport oRep = new SpecifiedOutputReport(this);	// create output report
            oRep.SendData(data);	// set the lights states

            Write(oRep); // write the output report
            if (DataSend != null)
            {
                DataSend(this, new DataSendEventArgs(data));
            }
        }

        public byte[] SendFeature(byte[] data, ref bool success)
        {
            SpecifiedFeatureReport oRep = new SpecifiedFeatureReport(this);
            oRep.SendData(data);
            try
            {
                if (!WriteFeature(oRep))
                    success = false;
                if (!ReadFeature(oRep))
                    success = false;
                return oRep.Data;
            }
            catch
            {
                success = false;
            }

            return null;
        }

        public bool GetManufacturerString(ref string data)
        {
            bool success = true;
            byte[] arrBuff = new byte[255];

            success = ReadManufacturerString(ref arrBuff);
            data = "";

            foreach (char b in arrBuff)
            {
                // нет проверки на спец знаки\символы
                if(b != 0)
                    data += b.ToString();
            }

            return success;
        }

        public bool GetProductString(ref string data)
        {
            bool success = true;
            byte[] arrBuff = new byte[255];

            success = ReadProductString(ref arrBuff);
            data = "";

            foreach (char b in arrBuff)
            {
                // нет проверки на спец знаки\символы
                if (b != 0)
                    data += b.ToString();
            }

            return success;
        }

        protected override void Dispose(bool bDisposing)
        {
            if (bDisposing)
            {
                // to do's before exit

               
            }
            base.Dispose(bDisposing);
        }

    }
}
