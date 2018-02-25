using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Audio
{
    public sealed class AudioSession : IDisposable
    {
        private AudioUtilities.IAudioSessionControl2 _ctl;
        private Process _process;

        internal AudioSession(AudioUtilities.IAudioSessionControl2 ctl)
        {
            _ctl = ctl;
        }

        public float Volume
        {
            get
            {
                AudioUtilities.ISimpleAudioVolume volumeControl = _ctl as AudioUtilities.ISimpleAudioVolume;
                volumeControl.GetMasterVolume(out float level);
                return level;
            }
            set
            { 
                AudioUtilities.ISimpleAudioVolume volumeControl = _ctl as AudioUtilities.ISimpleAudioVolume;
                if (value > 100)
                {
                    value = 100;
                } else if (value < 0)
                {
                    value = 0;
                }
                volumeControl.SetMasterVolume(value / 100, Guid.Empty);
            }
        }

        public bool Mute
        {
            get
            {
                AudioUtilities.ISimpleAudioVolume volumeControl = _ctl as AudioUtilities.ISimpleAudioVolume;
                volumeControl.GetMute(out bool mute);
                return mute;
            }
            set
            {
                AudioUtilities.ISimpleAudioVolume volumeControl = _ctl as AudioUtilities.ISimpleAudioVolume;
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
                _ctl.GetProcessId(out i);
                return i;
            }
        }

        public string Identifier
        {
            get
            {
                CheckDisposed();
                string s;
                _ctl.GetSessionIdentifier(out s);
                return s;
            }
        }

        public string InstanceIdentifier
        {
            get
            {
                CheckDisposed();
                string s;
                _ctl.GetSessionInstanceIdentifier(out s);
                return s;
            }
        }

        public AudioSessionState State
        {
            get
            {
                CheckDisposed();
                AudioSessionState s;
                _ctl.GetState(out s);
                return s;
            }
        }

        public Guid GroupingParam
        {
            get
            {
                CheckDisposed();
                Guid g;
                _ctl.GetGroupingParam(out g);
                return g;
            }
            set
            {
                CheckDisposed();
                _ctl.SetGroupingParam(value, Guid.Empty);
            }
        }

        public string DisplayName
        {
            get
            {
                CheckDisposed();
                string s;
                _ctl.GetDisplayName(out s);
                return s;
            }
            set
            {
                CheckDisposed();
                string s;
                _ctl.GetDisplayName(out s);
                if (s != value)
                {
                    _ctl.SetDisplayName(value, Guid.Empty);
                }
            }
        }

        public string IconPath
        {
            get
            {
                CheckDisposed();
                string s;
                _ctl.GetIconPath(out s);
                return s;
            }
            set
            {
                CheckDisposed();
                string s;
                _ctl.GetIconPath(out s);
                if (s != value)
                {
                    _ctl.SetIconPath(value, Guid.Empty);
                }
            }
        }

        private void CheckDisposed()
        {
            if (_ctl == null)
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
            if (_ctl != null)
            {
                Marshal.ReleaseComObject(_ctl);
                _ctl = null;
            }
        }
    }
}
