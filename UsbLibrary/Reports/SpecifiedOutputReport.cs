using System;
using System.Collections.Generic;
using System.Text;

namespace UsbLibrary
{
    public class SpecifiedOutputReport : OutputReport
    {
        public SpecifiedOutputReport(HIDDevice oDev) : base(oDev) 
        {

        }

        public bool SendData(byte[] data)
        {
            byte[] arrBuff = Buffer; //new byte[Buffer.Length];
            var len = data.Length < arrBuff.Length ? data.Length : arrBuff.Length;
            for (int i = 0; i < arrBuff.Length; i++)
            {
                arrBuff[i] = (i < len ? data[i] : (byte)0x0);
            }

            //Buffer = arrBuff;

            //returns false if the data does not fit in the buffer. else true
            if (arrBuff.Length <= data.Length)
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
