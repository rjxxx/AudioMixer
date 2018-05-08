using System;
using System.Collections.Generic;
using System.Text;

namespace UsbLibrary
{
    public class SpecifiedOutputReport : OutputReport
    {
        public SpecifiedOutputReport(HIDDevice oDev) : base(oDev) {

        }

        public bool SendData(byte[] data)
        {
            if (data.Length > Buffer.Length - 1) throw new ArgumentException("has invalid length", nameof(data));
            byte[] arrBuff = Buffer; //new byte[Buffer.Length];
            arrBuff[0] = 0;
            int i = 1;
            for (; i <= data.Length; i++)
            {
                arrBuff[i] = data[i - 1];
            }
            if (i < Buffer.Length)
            {
                Array.Clear(Buffer, i, Buffer.Length - i);
            }
            //Buffer = arrBuff;

            //returns false if the data does not fit in the buffer. else true
            if (arrBuff.Length < data.Length)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

}
