using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using static Audio.AudioUtilities;

namespace Audio
{
    public sealed class AudioSession : IDisposable
    {
        private Process _process;
        internal IAudioSessionControl2 AudioSessionControl2 { get; private set; }
        ISimpleAudioVolume volumeControl;
        internal AudioSession(IAudioSessionControl2 ctl)
        {
            AudioSessionControl2 = ctl;
            volumeControl = AudioSessionControl2 as ISimpleAudioVolume;
        }

        public float Volume
        {
            get
            {
                volumeControl.GetMasterVolume(out float level);
                return level;
            }
            set
            { 
                if (value > 1)
                {
                    value = 1;
                } else if (value < 0)
                {
                    value = 0;
                }
                volumeControl.SetMasterVolume(value, Guid.Empty);
            }
        }

        public bool Mute
        {
            get
            {
                volumeControl.GetMute(out bool mute);
                return mute;
            }
            set
            {
                volumeControl.SetMute(value, Guid.Empty);
            }
        }

        public Process Process
        {
            get
            {
                if (_process == null && ProcessId != 0)
                {
                    try
                    {
                        _process = Process.GetProcessById(ProcessId);
                    }
                    catch
                    {
                        // do nothing
                    }
                }
                return _process;
            }
        }

        public int ProcessId
        {
            get
            {
                CheckDisposed();
                int i;
                AudioSessionControl2.GetProcessId(out i);
                return i;
            }
        }

        public string Identifier
        {
            get
            {
                CheckDisposed();
                string s;
                AudioSessionControl2.GetSessionIdentifier(out s);
                return s;
            }
        }

        public string InstanceIdentifier
        {
            get
            {
                CheckDisposed();
                string s;
                AudioSessionControl2.GetSessionInstanceIdentifier(out s);
                return s;
            }
        }

        public AudioSessionState State
        {
            get
            {
                CheckDisposed();
                AudioSessionState s;
                AudioSessionControl2.GetState(out s);
                return s;
            }
        }

        public Guid GroupingParam
        {
            get
            {
                CheckDisposed();
                Guid g;
                AudioSessionControl2.GetGroupingParam(out g);
                return g;
            }
            set
            {
                CheckDisposed();
                AudioSessionControl2.SetGroupingParam(value, Guid.Empty);
            }
        }

        public string DisplayName
        {
            get
            {
                CheckDisposed();
                string s;
                AudioSessionControl2.GetDisplayName(out s);
                return s;
            }
            set
            {
                CheckDisposed();
                string s;
                AudioSessionControl2.GetDisplayName(out s);
                if (s != value)
                {
                    AudioSessionControl2.SetDisplayName(value, Guid.Empty);
                }
            }
        }

        public string IconPath
        {
            get
            {
                CheckDisposed();
                string s;
                AudioSessionControl2.GetIconPath(out s);
                return s;
            }
            set
            {
                CheckDisposed();
                string s;
                AudioSessionControl2.GetIconPath(out s);
                if (s != value)
                {
                    AudioSessionControl2.SetIconPath(value, Guid.Empty);
                }
            }
        }


        private void CheckDisposed()
        {
            if (AudioSessionControl2 == null)
                throw new ObjectDisposedException("Control");
        }

        public override string ToString()
        {
            string s = DisplayName;
            if (!string.IsNullOrEmpty(s))
                return "DisplayName: " + s;

            if (Process != null)
                return "Process: " + Process.ProcessName;

            return "Pid: " + ProcessId;
        }

        public void Dispose()
        {
            if (AudioSessionControl2 != null)
            {
                Marshal.ReleaseComObject(AudioSessionControl2);
                AudioSessionControl2 = null;
            }
        }
    }
}
