using System;
using System.Collections.Generic;
using System.Text;

namespace UsbLibrary
{
    public class SpecifiedFeatureReport : FeatureReport
    {
        private byte[] arrData;

        public SpecifiedFeatureReport(HIDDevice oDev) : base(oDev) 
        {

        }

        /// <summary>
        /// Проверяем на "влезаемость" пакета данных в буфер девайса
        /// </summary>
        /// <param name="data">пакет данных</param>
        /// <returns>False если Data превосходит по размеру Buffer иначе True</returns>
        public bool SendData(byte[] data)
        {
            byte[] arrBuff = Buffer; //new byte[Buffer.Length];
            for (int i = 1; i < arrBuff.Length; i++)
            {
                arrBuff[i] = data[i];
            }

            if (arrBuff.Length <= data.Length) return false;
            else return true;
        }

        public override void ProcessData()
        {
            this.arrData = Buffer;
        }

        public byte[] Data
        {
            get
            {
                return arrData;
            }
        }
    }
}