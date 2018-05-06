using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace UsbLibrary
{
	#region Custom exception
	/// <summary>
	/// Generic HID device exception
	/// </summary>
    public class HIDDeviceException : ApplicationException
    {
        public HIDDeviceException(string strMessage) : base(strMessage) { }

        public static HIDDeviceException GenerateWithWinError(string strMessage)
        {
            return new HIDDeviceException(string.Format("Msg:{0} WinEr:{1:X8}", strMessage, Marshal.GetLastWin32Error()));
        }

        public static HIDDeviceException GenerateError(string strMessage)
        {
            return new HIDDeviceException(string.Format("Msg:{0}", strMessage));
        }
    }
	#endregion
	/// <summary>
	/// Abstract HID device : Derive your new device controller class from this
	/// </summary>
    public abstract class HIDDevice : Win32Usb, IDisposable
	{
	    private bool _isDisposed;
		#region Privates variables
		/// <summary>Filestream we can use to read/write from</summary>
        private FileStream m_oFile;
		/// <summary>Length of input report : device gives us this</summary>
		private int m_nInputReportLength;
		/// <summary>Length if output report : device gives us this</summary>
		private int m_nOutputReportLength;
		/// <summary>Handle to the device</summary>
		private IntPtr m_hHandle;
        /// <summary>property key of device path</summary>
        private static readonly DevicePropertyKey DevicePathKey = new DevicePropertyKey { propertyCategory = new Guid(0x83da6326, 0x97a6, 0x4088, 0x94, 0x53, 0xa1, 0x92, 0x3f, 0x57, 0x3b, 0x29), propertyId = 10 };
        /// <summary>property key of child device path</summary>
        private static readonly DevicePropertyKey ChildDevicePathKey = new DevicePropertyKey { propertyCategory = new Guid(0x78c34fc8, 0x104a, 0x4aca, 0x9e, 0xa4, 0x52, 0x4d, 0x52, 0x99, 0x6e, 0x57), propertyId = 256 };
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose()
		{
		    _isDisposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }
		/// <summary>
		/// Disposer called by both dispose and finalise
		/// </summary>
		/// <param name="bDisposing">True if disposing</param>
        protected virtual void Dispose(bool bDisposing)
        {
            try
            {
                if (bDisposing)	// if we are disposing, need to close the managed resources
                {
                    if (m_oFile != null)
                    {
                        m_oFile.Close();
                        m_oFile.Dispose();
                        m_oFile = null;
                    }
                }
                if (m_hHandle != IntPtr.Zero)	// Dispose and finalize, get rid of unmanaged resources
                {

                    CloseHandle(m_hHandle);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        #endregion

		#region Privates/protected
		/// <summary>
		/// Initialises the device
		/// </summary>
		/// <param name="strPath">Path to the device</param>
		private void Initialise(string strPath)
		{
			// Create the file from the device path
            m_hHandle = CreateFile(strPath, GENERIC_READ | GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, IntPtr.Zero);
          
            if ( m_hHandle != InvalidHandleValue || m_hHandle == null)	// if the open worked...
			{
				IntPtr lpData;
				if (HidD_GetPreparsedData(m_hHandle, out lpData))	// get windows to read the device data into an internal buffer
				{
                    try
                    {
                        HidCaps oCaps;
                        HidP_GetCaps(lpData, out oCaps);	// extract the device capabilities from the internal buffer
                        m_nInputReportLength = oCaps.InputReportByteLength;	// get the input...
                        m_nOutputReportLength = oCaps.OutputReportByteLength;	// ... and output report lengths

                        //m_oFile = new FileStream(m_hHandle, FileAccess.Read | FileAccess.Write, true, m_nInputReportLength, true);
                        m_oFile = new FileStream(new SafeFileHandle(m_hHandle, false), FileAccess.Read | FileAccess.Write, m_nInputReportLength, true);

                        BeginAsyncRead();	// kick off the first asynchronous read                              
                    }
                    catch (Exception ex)
                    {
                        throw HIDDeviceException.GenerateWithWinError("Failed to get the detailed data from the hid.");
                    }
					finally
					{
						HidD_FreePreparsedData(ref lpData);	// before we quit the funtion, we must free the internal buffer reserved in GetPreparsedData
					}
				}
				else	// GetPreparsedData failed? Chuck an exception
				{
					throw HIDDeviceException.GenerateWithWinError("GetPreparsedData failed");
				}
			}
			else	// File open failed? Chuck an exception
			{
				m_hHandle = IntPtr.Zero;
				throw HIDDeviceException.GenerateWithWinError("Failed to create device file");
			}
		}
		/// <summary>
		/// Kicks off an asynchronous read which completes when data is read or when the device
		/// is disconnected. Uses a callback.
		/// </summary>
        private void BeginAsyncRead()
        {
                byte[] arrInputReport = new byte[m_nInputReportLength];
                // put the buff we used to receive the stuff as the async state then we can get at it when the read completes

                m_oFile.BeginRead(arrInputReport, 0, m_nInputReportLength, new AsyncCallback(ReadCompleted), arrInputReport);
        }

        /// <summary>
        /// Iterates through HID devices, until <paramref name="processDevice"/> returns <see langword="true"/>
        /// </summary>
        /// <param name="processDevice">function to apply to each device path; should return true to break the loop</param>
        private static void IterateThroughDevices(Func<string, string, bool> processDevice)
        {
            Guid gHid = HIDGuid;
            //HidD_GetHidGuid(out gHid);	// next, get the GUID from Windows that it uses to represent the HID USB interface
            IntPtr hInfoSet = SetupDiGetClassDevs(ref gHid, null, IntPtr.Zero, DIGCF_DEVICEINTERFACE | DIGCF_PRESENT);
            // this gets a list of all HID devices currently connected to the computer (InfoSet)
            try
            {
                DeviceInterfaceData oInterface = new DeviceInterfaceData(); // build up a device interface data block
                oInterface.Size = Marshal.SizeOf(oInterface);
                // Now iterate through the InfoSet memory block assigned within Windows in the call to SetupDiGetClassDevs
                // to get device details for each device connected
                int nIndex = 0;
                //while (SetupDiEnumDeviceInterfaces(hInfoSet, 0, ref gHid, (uint)nIndex, ref oInterface))
                while (SetupDiEnumDeviceInterfaces(hInfoSet, 0, ref gHid, (uint)nIndex, ref oInterface))
                // this gets the device interface information for a device at index 'nIndex' in the memory block
                {
                    string strInterfacePath = GetInterfacePath(hInfoSet, ref oInterface);
                    string strDevicePath = GetDevicePath(strInterfacePath);
                    // get the device path (see helper method 'GetDevicePath')
                    if (processDevice(strDevicePath, strInterfacePath)) break;
                    nIndex++; // if we get here, we didn't find our device. So move on to the next one.
                }
            }
            catch (Exception ex)
            {
                throw HIDDeviceException.GenerateError(ex.ToString());
                //Console.WriteLine(ex.ToString());
            }
            finally
            {
                // Before we go, we have to free up the InfoSet memory reserved by SetupDiGetClassDevs
                SetupDiDestroyDeviceInfoList(hInfoSet);
            }
        }

        /// <summary>
        /// Iterates through HID device interfaces, until <paramref name="processDevice"/> returns <see langword="true"/>
        /// </summary>
        /// <param name="processDevice">function to apply to each device path; should return true to break the loop</param>
        private static void IterateThroughInterfaces(Func<string, bool> processDevice)
        {
            Guid gHid = HIDGuid;
            //HidD_GetHidGuid(out gHid);	// next, get the GUID from Windows that it uses to represent the HID USB interface
            IntPtr hInfoSet = SetupDiGetClassDevs(ref gHid, null, IntPtr.Zero, DIGCF_DEVICEINTERFACE | DIGCF_PRESENT);
            // this gets a list of all HID devices currently connected to the computer (InfoSet)
            try
            {
                DeviceInterfaceData oInterface = new DeviceInterfaceData(); // build up a device interface data block
                oInterface.Size = Marshal.SizeOf(oInterface);
                // Now iterate through the InfoSet memory block assigned within Windows in the call to SetupDiGetClassDevs
                // to get device details for each device connected
                int nIndex = 0;
                while (SetupDiEnumDeviceInterfaces(hInfoSet, 0, ref gHid, (uint)nIndex, ref oInterface))
                // this gets the device interface information for a device at index 'nIndex' in the memory block
                {
                    string strDevicePath = GetInterfacePath(hInfoSet, ref oInterface);
                    // get the device path (see helper method 'GetDevicePath')
                    if (processDevice(strDevicePath)) break;
                    nIndex++; // if we get here, we didn't find our device. So move on to the next one.
                }
            }
            catch (Exception ex)
            {
                throw HIDDeviceException.GenerateError(ex.ToString());
                //Console.WriteLine(ex.ToString());
            }
            finally
            {
                // Before we go, we have to free up the InfoSet memory reserved by SetupDiGetClassDevs
                SetupDiDestroyDeviceInfoList(hInfoSet);
            }
        }

        /// <summary>
        /// Callback for above. Care with this as it will be called on the background thread from the async read
        /// </summary>
        /// <param name="iResult">Async result parameter</param>
        protected void ReadCompleted(IAsyncResult iResult)
        {
            byte[] arrBuff = (byte[])iResult.AsyncState;	// retrieve the read buffer
            try
            {
                if(_isDisposed) return;
                var file = m_oFile;
                if(file == null) return;
                file.EndRead(iResult);	// call end read : this throws any exceptions that happened during the read
                try
                {
					InputReport oInRep = CreateInputReport();	// Create the input report for the device
					oInRep.SetData(arrBuff);	// and set the data portion - this processes the data received into a more easily understood format depending upon the report type
                    HandleDataReceived(oInRep);	// pass the new input report on to the higher level handler
                }
                finally
                {
                    BeginAsyncRead();	// when all that is done, kick off another read for the next report
                }                
            }
            catch(IOException ex)	// if we got an IO exception, the device was removed
            {
                HandleDeviceRemoved();
                if (OnDeviceRemoved != null)
                {
                    OnDeviceRemoved(this, new EventArgs());
                }
                Dispose();
            }
        }
		/// <summary>
		/// Write an output report to the device.
		/// </summary>
		/// <param name="oOutRep">Output report to write</param>
        protected void Write(OutputReport oOutRep)
        {
            try
            {
                m_oFile.Write(oOutRep.Buffer, 0, oOutRep.BufferLength);
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.ToString());
                // The device was removed!
                throw new HIDDeviceException("Probbaly the device was removed");
            }
			catch(Exception exx)
			{
                Console.WriteLine(exx.ToString());	
			}
        }
		/// <summary>
		/// virtual handler for any action to be taken when data is received. Override to use.
		/// </summary>
		/// <param name="oInRep">The input report that was received</param>
		protected virtual void HandleDataReceived(InputReport oInRep)
		{
		}
		/// <summary>
		/// Virtual handler for any action to be taken when a device is removed. Override to use.
		/// </summary>
		protected virtual void HandleDeviceRemoved()
		{
		}
		/// <summary>
		/// Helper method to return the device path given a DeviceInterfaceData structure and an InfoSet handle.
		/// Used in 'FindDevice' so check that method out to see how to get an InfoSet handle and a DeviceInterfaceData.
		/// </summary>
		/// <param name="interfacePath">interface path</param>
		/// <returns>The device path or null if there was some problem</returns>
		private static string GetDevicePath(string interfacePath)
		{
			uint nRequiredSize;
		    uint deviceHandle;

		    var match = Regex.Match(interfacePath, @"(...#[^#]*#[^#]*)#");
            if (!match.Success) return null;
		    interfacePath = match.Groups[1].ToString().ToUpper().Replace('#', '\\');
		    IntPtr pointer = Marshal.StringToCoTaskMemAnsi(interfacePath);
		    CM_Locate_DevNode(out deviceHandle, (uint) pointer.ToInt32(), 0);
		    Marshal.FreeCoTaskMem(pointer);
            CM_Get_Parent(out deviceHandle, deviceHandle, 0).ToString();
		    CM_Get_Device_ID_Size(out nRequiredSize, deviceHandle, 0);
		    var deviceID = new byte[++nRequiredSize];
		    CM_Get_Device_ID(deviceHandle, deviceID, nRequiredSize, 0);
		    return System.Text.Encoding.ASCII.GetString(deviceID);
		}

        /// <summary>
		/// Helper method to return the device path given a DeviceInterfaceData structure and an InfoSet handle.
		/// Used in 'FindDevice' so check that method out to see how to get an InfoSet handle and a DeviceInterfaceData.
		/// </summary>
		/// <param name="hInfoSet">Handle to the InfoSet</param>
		/// <param name="oInterface">DeviceInterfaceData structure</param>
		/// <returns>The device path or null if there was some problem</returns>
        private static string GetInterfacePath(IntPtr hInfoSet, ref DeviceInterfaceData oInterface)
        {
            uint nRequiredSize = 0;
            //Get the device interface details
            if (
                !SetupDiGetDeviceInterfaceDetail(hInfoSet, ref oInterface, IntPtr.Zero, 0, ref nRequiredSize,
                    IntPtr.Zero))
            {
                DeviceInterfaceDetailData oDetail = new DeviceInterfaceDetailData();
                if (Win32Usb.Is64Bit())
                    oDetail.Size = 8;
                else
                    oDetail.Size = 5;
                // hardcoded to 5! Sorry, but this works and trying more future proof versions by setting the size to the struct sizeof failed miserably. If you manage to sort it, mail me! Thx
                if (SetupDiGetDeviceInterfaceDetail(hInfoSet, ref oInterface, ref oDetail, nRequiredSize,
                    ref nRequiredSize, IntPtr.Zero))
                {
                    return oDetail.DevicePath;
                }
            }
            return null;
        }
        #endregion

        #region Public static
        /// <summary>
        /// Finds a device given its PID and VID
        /// </summary>
        /// <param name="nVid">Vendor id for device (VID)</param>
        /// <param name="nPid">Product id for device (PID)</param>
        /// <param name="oType">Type of device class to create</param>
        /// <returns>A new device class of the given type or null</returns>
        public static HIDDevice FindDevice(int nVid, int nPid, Type oType)
        {
            HIDDevice oNewDevice = null;
            string strSearch = string.Format("vid_{0:x4}&pid_{1:x4}", nVid, nPid); // first, build the path search string
		    Func<string, bool> processDevice = strDevicePath =>
		    {
		        if (strDevicePath.IndexOf(strSearch) >= 0)
		            // do a string search, if we find the VID/PID string then we found our device!
		        {
		            oNewDevice = (HIDDevice) Activator.CreateInstance(oType);
		            // create an instance of the class for this device
		            oNewDevice.Initialise(strDevicePath); // initialise it with the device path
		            return true; // return oNewDevice;	// and return it
		        }
                return false;
		    };
            IterateThroughInterfaces(processDevice);
		    return oNewDevice;	// oops, didn't find our device
        }

	    /// <summary>
	    /// Finds a device given its PID and VID
	    /// </summary>
	    /// <param name="nVid">Vendor id for device (VID)</param>
	    /// <param name="nPid">Product id for device (PID)</param>
	    /// <param name="restOfPath"></param>
	    /// <param name="oType">Type of device class to create</param>
	    /// <returns>A new device class of the given type or null</returns>
	    public static HIDDevice FindDevice(int nVid, int nPid, string restOfPath, Type oType)
	    {
	        HIDDevice oNewDevice = null;
	        string strSearch = $"vid_{nVid:x4}&pid_{nPid:x4}#{restOfPath}#"; // first, build the path search string
	        Func<string, bool> processInterface = strInterfacePath =>
	        {
	            if (strInterfacePath.IndexOf(strSearch, StringComparison.CurrentCultureIgnoreCase) >= 0)
	            {
	                // do a string search, if we find the VID/PID string then we found our device!
	                oNewDevice = (HIDDevice) Activator.CreateInstance(oType);
	                // create an instance of the class for this device
	                oNewDevice.Initialise(strInterfacePath); // initialise it with the device path
	                return true; // return oNewDevice;	// and return it
	            }
	            return false;
	        };
	        IterateThroughInterfaces(processInterface);
	        return oNewDevice; // oops, didn't find our device
	    }

	    /// <summary>
        /// Gets available HID devices
        /// </summary>
        /// <returns>array of basic information (VID and PID) necessary to select a device with <see cref="FindDevice(int, int, Type)"/></returns>
        public static BasicHidDeviceInfo[] GetAvailableDevices()
        {
            System.Collections.Generic.List<BasicHidDeviceInfo> result =
                new System.Collections.Generic.List<BasicHidDeviceInfo>();
	        Func<string, string, bool> processDevice = (strDevicePath, strInterfacePath) =>
	        {
                var matchDevicePath = Regex.Match(strDevicePath, string.Format(@"vid_([\da-fA-F]{0})&pid_([\da-fA-F]{0}).(.*){1}", @"{4}", '\0'), RegexOptions.IgnoreCase);
	            if (matchDevicePath.Success)
                {
                    int vid;
                    int pid;
                    Match matchInterfacePath;
                    if (int.TryParse(matchDevicePath.Groups[1].Value, NumberStyles.HexNumber, null, out vid) &&
                        int.TryParse(matchDevicePath.Groups[2].Value, NumberStyles.HexNumber, null, out pid) &&
                        (matchInterfacePath = Regex.Match(strInterfacePath, $@"vid_{vid:x4}&pid_{pid:x4}#(.*)#")).Success)
                    {
                        result.Add(new BasicHidDeviceInfo(vid, pid, matchInterfacePath.Groups[1].Value, matchDevicePath.Groups[3].Value));
                    }
                }
                return false;
	        };
            IterateThroughDevices(processDevice);
            return result.ToArray();
        }

        /// <summary>
        /// Gets available HID devices
        /// </summary>
        /// <param name="vid">Vendor id for device (VID)</param>
		/// <param name="pid">Product id for device (PID)</param>
        /// <returns>array of basic information (VID and PID) necessary to select a device with <see cref="FindDevice(int, int, Type)"/></returns>
        public static BasicHidDeviceInfo[] GetAvailableDevices(int vid, int pid)
        {
            System.Collections.Generic.List<BasicHidDeviceInfo> result =
                new System.Collections.Generic.List<BasicHidDeviceInfo>();
            Func<string, string, bool> processDevice = (strDevicePath, strInterfacePath) =>
            {
                var matchDevicePath = Regex.Match(strDevicePath, $@"vid_{vid:x4}&pid_{pid:x4}.(.*){'\0'}", RegexOptions.IgnoreCase);
                Match matchInterfacePath;
                if (matchDevicePath.Success && (matchInterfacePath = Regex.Match(strInterfacePath, $@"vid_{vid:x4}&pid_{pid:x4}#(.*)#")).Success)
                {
                    result.Add(new BasicHidDeviceInfo(vid, pid, matchInterfacePath.Groups[1].Value, matchDevicePath.Groups[1].Value));
                }
                return false;
            };
            IterateThroughDevices(processDevice);
            return result.ToArray();
        }

        /// <summary>
		/// Helper method to return the interface path (last part of it) given device vendor ID, product ID and serial number.
		/// Retrieved interface path can be used as an argument for <see cref="FindDevice(int, int, string, Type)"/>.
		/// </summary>
		/// <param name="serialNumber">device serial number</param>
		/// <returns>The interface path (last part of it) or null if there was some problem</returns>
		public static string GetInterfacePath(ushort vid, ushort pid, string serialNumber)
        {
            uint nRequiredSize;
            uint deviceHandle;

            IntPtr pointer = Marshal.StringToHGlobalAnsi($"USB\\VID_{vid:X4}&PID_{pid:X4}\\{serialNumber}");
            CM_Locate_DevNode(out deviceHandle, (uint)pointer.ToInt32(), 0);
            Marshal.FreeCoTaskMem(pointer);
            CM_Get_Child(out deviceHandle, deviceHandle, 0).ToString();
            CM_Get_Device_ID_Size(out nRequiredSize, deviceHandle, 0);
            var deviceID = new byte[++nRequiredSize];
            CM_Get_Device_ID(deviceHandle, deviceID, nRequiredSize, 0);
            var match = Regex.Match(System.Text.Encoding.ASCII.GetString(deviceID), $@"HID\\.*\\(.*){'\0'}");
            if (!match.Success) return null;
            return match.Groups[1].Value;
        }
        #endregion

        #region Publics
        /// <summary>
        /// Event handler called when device has been removed
        /// </summary>
        public event EventHandler OnDeviceRemoved;
		/// <summary>
		/// Accessor for output report length
		/// </summary>
		public int OutputReportLength
		{
			get
			{
				return m_nOutputReportLength;
			}
		}
		/// <summary>
		/// Accessor for input report length
		/// </summary>
		public int InputReportLength
		{
			get
			{
				return m_nInputReportLength;
			}
		}
		/// <summary>
		/// Virtual method to create an input report for this device. Override to use.
		/// </summary>
		/// <returns>A shiny new input report</returns>
		public virtual InputReport CreateInputReport()
		{
			return null;
		}
		#endregion
    }
}
