namespace UsbLibrary
{
    /// <summary>
    /// Basic HID device information
    /// </summary>
    public struct BasicHidDeviceInfo
    {
        public BasicHidDeviceInfo(int vid, int pid, string restOfInterfacePath, string restOfDevicePath)
        {
            Vid = vid;
            Pid = pid;
            RestOfInterfacePath = restOfInterfacePath;
            RestOfDevicePath = restOfDevicePath;
        }

        /// <summary>
        /// Vendor ID
        /// </summary>
        public int Vid { get; }
        /// <summary>
        /// Product ID
        /// </summary>
        public int Pid { get; }
        /// <summary>
        /// Remaining part of interface path
        /// </summary>
        public string RestOfInterfacePath { get; }
        /// <summary>
        /// Remaining part of device path
        /// </summary>
        public string RestOfDevicePath { get; }
    }
}
