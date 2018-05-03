using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace UsbLibrary
{
/// <summary>
    /// This class provides an usb component. This can be placed ont to your form.
    /// </summary>
    [ToolboxBitmap(typeof(UsbHidPort), "UsbHidBmp.bmp")]
    public partial class UsbHidPort : Component
    {
        private int product_id;
        private int vendor_id;
        private Guid device_class;
        private IntPtr usb_event_handle;
        private SpecifiedDevice specified_device;
        private IntPtr handle;
        private string device_product = null;

        #region События 
        /// <summary>
        /// Событие, срабатывающее при подключении устройства с указанными PID, VID.
        /// </summary>
        [Description("Событие, срабатывающее при подключении устройства с указанными PID, VID")]
        [Category("Embedded Event")]
        [DisplayName("OnSpecifiedDeviceArrived")]
        public event EventHandler               OnSpecifiedDeviceArrived;

        /// <summary>
        /// Событие, срабатывающее при отключении устройства с указанными PID, VID.
        /// </summary>
        [Description("Событие, срабатывающее при отключении устройства с указанными PID, VID")]
        [Category("Embedded Event")]
        [DisplayName("OnSpecifiedDeviceRemoved")]
        public event EventHandler               OnSpecifiedDeviceRemoved;

        /// <summary>
        /// Событие, срабатывающее при подключении любого USB-устройства.
        /// </summary>
        [Description("Событие, срабатывающее при подключении любого USB-устройства")]
        [Category("Embedded Event")]
        [DisplayName("OnDeviceArrived")]
        public event EventHandler               OnDeviceArrived;

        /// <summary>
        /// Событие, срабатывающее при отключении любого USB-устройства.
        /// </summary>
        [Description("Событие, срабатывающее при отключении любого USB-устройства")]
        [Category("Embedded Event")]
        [DisplayName("OnDeviceRemoved")]
        public event EventHandler               OnDeviceRemoved;

        /// <summary>
        /// Событие, срабатывающее при получении данных от выбранного USB-устройства.
        /// </summary>
        [Description("Событие, срабатывающее при получении данных от выбранного USB-устройства")]
        [Category("Embedded Event")]
        [DisplayName("OnDataRecieved")]
        public event DataRecievedEventHandler   OnDataRecieved;

        /// <summary>
        /// Событие, срабатывающее при отправке данных к выбранному USB-устройству. 
        /// Будет вызванно только при успешной отправке данных.
        /// </summary>
        [Description("Событие, срабатывающее при отправке данных к выбранному USB-устройству")]
        [Category("Embedded Event")]
        [DisplayName("OnDataSend")]
        public event EventHandler               OnDataSend;
        public event EventHandler Disposed;
        #endregion

        #region Конструкторы
        public UsbHidPort()
        {
            product_id = 0;
            vendor_id = 0;
            specified_device = null;
            device_product = null;
            device_class = Win32Usb.HIDGuid;

            InitializeComponent();
        }
        
        public UsbHidPort(IContainer container)
        {
            //initializing in initial state
            product_id = 0;
            vendor_id = 0;
            specified_device = null;
            device_product = null;
            device_class = Win32Usb.HIDGuid;

            container.Add(this);
            InitializeComponent();
        }
        #endregion

        #region Публичные свойства
        [Description("Это Product ID выбранного USB устройства")]
        [DefaultValue("(none)")]
        [Category("Embedded Details")]
        public int ProductId
        {
            get { return this.product_id; }
            set { this.product_id = value; }
        }

       [Description("Это Vendor ID выбранного USB устройства")]
       [DefaultValue("(none)")]
       [Category("Embedded Details")]
        public int VendorId
        {
            get { return this.vendor_id; }
            set { this.vendor_id = value; }
        }

        [Description("Это класс устройства, к которому относится USB-устройства")]
        [DefaultValue("(none)")]
        [Category("Embedded Details")]
        public Guid DeviceClass
        {
            get { return device_class; }
        }

        [Description("Устройство, которое описывает установленный экземпляр")]
        [DefaultValue("(none)")]
        [Category("Embedded Details")]
        public SpecifiedDevice SpecifiedDevice
        {
            get { return this.specified_device; }
        }

        public ISite Site { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion

        #region Базовые функции 
        /// <summary>
        /// Регистрация для получения событий USB-шины.  
        /// </summary>
        /// <param name="Handle">= IntPtr, путь до приложения.</param>
        public void RegisterHandle(IntPtr Handle)
        {
            usb_event_handle = Win32Usb.RegisterForUsbEvents(Handle, device_class);
            this.handle = Handle;
            CheckDevicePresent();
        }

        /// <summary>
        /// Отмена регистрации для получения событий USB-шины, это приложение не будет на них реагировать. 
        /// </summary>
        /// <returns>Возвращает true - если успешно.</returns>
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
        public void ParseMessages(ref Message m)
        {
            ParseMessages(m.Msg, m.WParam);
        }

        /// <summary>
        /// This method will filter the messages that are passed for usb device change messages only. 
        /// And parse them and take the appropriate action 
        /// </summary>
        /// <param name="m">a ref to Messages, The messages that are thrown by windows to the application.</param>
        public void ParseMessages(int Msg, IntPtr WParam)
        { 
            if (Msg == Win32Usb.WM_DEVICECHANGE)	// we got a device change message! A USB device was inserted or removed
            {
                switch (WParam.ToInt32())	// Check the W parameter to see if a device was inserted or removed
                {
                    case Win32Usb.DEVICE_ARRIVAL:	// inserted
                        if (OnDeviceArrived != null)
                        {
                            OnDeviceArrived(this, new EventArgs());
                            CheckDevicePresent();
                        }
                        // Если подключенна обработка Только спец.девайса (иначе, без null события OnDeviceArrived не будет событий OnSpecifiedDeviceArrived)
                        else if (OnSpecifiedDeviceArrived != null)
                        {
                            CheckDevicePresent();
                        }
                        break;
                    case Win32Usb.DEVICE_REMOVECOMPLETE:	// removed
                        if (OnDeviceRemoved != null)
                        {
                            OnDeviceRemoved(this, new EventArgs());
                            CheckDevicePresent();
                        }
                        // Если подключенна обработка Только спец.девайса (аналогично верхнему)
                        // Данный косяк был замечен в оригинальной библиотеке, на которой базируется эта :)
                        else if (OnSpecifiedDeviceRemoved != null)
                        {
                            CheckDevicePresent();
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Проверяет, есть ли на шине USB устройство с указанными PID, VID.
        /// </summary>
        public void CheckDevicePresent()
        {
            try
            {
                //Mind if the specified device existed before.
                bool history = false;

                if(specified_device != null )
                {       
                    history = true;
                }

                specified_device = SpecifiedDevice.FindSpecifiedDevice(this.vendor_id, this.product_id, device_product);
                
                if (specified_device != null)	// нашлось?
                {
                    if (OnSpecifiedDeviceArrived != null)
                    {
                        this.OnSpecifiedDeviceArrived(this, new EventArgs());
                        if (OnDataRecieved != null) specified_device.DataRecieved += new DataRecievedEventHandler(OnDataRecieved);
                        if (OnDataSend != null) specified_device.DataSend += new DataSendEventHandler(OnDataSend);
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
            if(this.OnDataRecieved != null)
            {
                this.OnDataRecieved(sender, args);
            }
        }

        private void DataSend(object sender, DataSendEventArgs args)
        {
            if (this.OnDataSend != null)
            {
                this.OnDataSend(sender, args);
            }
        }
        #endregion 

        #region Подключение\отключение\информация об устройстве
        /// <summary>
        /// Проверяем (мнимую) готовность устройства к работе.
        /// </summary>
        /// <returns>Возвращает True, если устройство готово,иначе False</returns>
        public bool Ready()
        {
            if (SpecifiedDevice != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Закрываем текущее подключение к устройству, если есть таковое.
        /// </summary>
        public void Close()
        {
            if (specified_device != null)
            {
                specified_device.Dispose();
                specified_device = null;
                if (OnSpecifiedDeviceRemoved != null)
                    this.OnSpecifiedDeviceRemoved(this, new EventArgs());
            }
        }

        /// <summary>
        /// Проверка подключения и поддержание в открытом состоянии доступа к устройству.
        /// Подключение к первому подходящему устройству по PID\VID при условии, что
        /// DeviceProduct = null
        /// </summary>
        /// <param name="OpenState">Если надо поддерживать связь, то True</param>
        /// <returns>Возвращает True - если устройство успешно найденно, иначе False</returns>
        public bool Open(bool OpenState)
        {
            bool success = true;
            device_product = null;

            if (OpenState)
            {
                CheckDevicePresent();

                if (SpecifiedDevice == null)
                {
                    success = false;
                }
            }
            else
            {
                CheckDevicePresent();

                if (SpecifiedDevice != null)
                {
                    specified_device.Dispose();
                    specified_device = null;
                    this.OnSpecifiedDeviceRemoved(this, new EventArgs());
                }
                else
                {
                    success = false;
                }
            }
            return success;
        }

        /// <summary>
        /// Проверка подключения и поддержание в открытом состоянии доступа к устройству.
        /// Переподключение к устройству с подходящими PID\VID и соответствующим DeviceProduct
        /// </summary>
        /// <param name="OpenState">Если надо поддерживать связь, то True</param>
        /// <param name="Product">Указываем сюда глобальную константу DeviceProduct, с описанием продукта</param>
        /// <returns>Возвращает True - если устройство успешно найденно, иначе False</returns>
        public bool Open(bool OpenState, string Product)
        {
            bool success = true;

            if (OpenState)
            {
                CheckDevicePresent();

                if (SpecifiedDevice == null)
                {
                    success = false;
                }
            }
            else
            {
                CheckDevicePresent();

                if (SpecifiedDevice != null)
                {
                    specified_device.Dispose();
                    specified_device = null;
                    this.OnSpecifiedDeviceRemoved(this, new EventArgs());
                }
                else
                {
                    success = false;
                }
            }
            return success;
        }

        /// <summary>
        /// Переопределение строки продукта для устройства.
        /// При вызове происходит закрытие текущего подключения (если есть), далее происходит поиск и подключение к устройству с новыми параметрами.
        /// </summary>
        /// <param name="DevPrd">Указываем сюда глобальную константу DeviceProduct, с описанием продукта</param>
        /// <returns>Возвращает True - если устройство успешно найденно, иначе False</returns>
        public bool UpdateDeviceProduct(string Product)
        {
            bool success = true;

            if (specified_device != null)
            {
                specified_device.Dispose();
                specified_device = null;
                if (OnSpecifiedDeviceRemoved != null)
                    this.OnSpecifiedDeviceRemoved(this, new EventArgs());
            }

            device_product = Product;

            CheckDevicePresent();

            if (SpecifiedDevice == null)
            {
                success = false;
            }

            return success;
        }


        /// <summary>
        /// Чтение строк производителя и продукта из девайса.
        /// </summary>
        /// <param name="Manufacturer">Производитель</param>
        /// <param name="Product">Продукт</param>
        /// <returns>Возвращает True в случае успешного чтения обоих строк, иначе False</returns>
        public bool GetInfoStrings(ref string Manufacturer, ref string Product)
        {
            if (specified_device.GetManufacturerString(ref Manufacturer) &&
                specified_device.GetProductString(ref Product))
                return true;
            else
                return false;
        }
        #endregion

        #region Write Reports

        /// <summary>
        /// Записываем Output Report в девайс.
        /// </summary>
        /// <param name="ID">Report ID</param>
        /// <param name="data">Report Data</param>
        /// <returns>Возвращает True, если успешно отправленны данные,иначе False</returns>
        public bool WriteOutputReport(byte ID, byte[] data)
        {
            bool success = false;
            byte[] Report = new byte[specified_device.OutputReportLength];

            // Проверяем, влазит ли пакет данных
            try
            {
                Report[0] = ID;
                Array.Copy(data, Report, data.Length);
                success = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (success)
            {
                try
                {
                    SpecifiedDevice.SendData(Report);
                }
                catch
                {
                    success = false;
                }
            }
            return success;
        }

        /// <summary>
        /// Записываем Feature Report в девайс.
        /// </summary>
        /// <param name="ID">Report ID</param>
        /// <param name="data">Report Data to Write</param>
        /// <param name="rdata">Report Data Received</param>
        /// <returns>Возвращает True, если успешно отправленны и полученны данные,иначе False</returns>
        public bool WriteFeatureReport(byte ID, byte[] data, ref byte[] rdata)
        {
            bool success = true;
            byte[] Report = new byte[specified_device.FeatureReportLength];
            
            // Проверяем, влазит ли пакет данных
            try
            {
                Report[0] = ID;
                Array.Copy(data, 0, Report, 1, data.Length);
            }
            catch
            {
                success = false;
            }
            
            if (success)
            {
                try
                {
                    rdata = new byte[specified_device.FeatureReportLength];
                    rdata = specified_device.SendFeature(Report, ref success);
                }
                catch
                {
                    success = false;
                }
            }
            return success;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
