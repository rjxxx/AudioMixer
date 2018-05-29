using Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Audio.AudioUtilities;

namespace AudioMixer
{
    public class AudioProgram
    {
        private float stepVolume = 0.01f;
        private AudioNotification audioNotification;
        public string Name { get; set; }
        public string ExePath { get; set; }
        public List<AudioSession> Sessions { get; set; } = new List<AudioSession>();
     
        ~AudioProgram()
        {
            foreach (var session in Sessions)
            {
                session.AudioSessionControl2.UnregisterAudioSessionNotification(audioNotification);
            }
        }
        public float StepVolume
        {
            get => stepVolume;
            set
            {
                if (value > 0 && value <= 1f)
                    stepVolume = value;
            }
        }

        public AudioProgram(string name, string exePath)
        {
            audioNotification = new AudioNotification(this);
            this.Name = name;
            this.ExePath = exePath;
        }

        public void AddSession(AudioSession session)
        {
            foreach (var audioSession in Sessions)
            {
                if (audioSession == null || audioSession.State == AudioSessionState.Expired)
                {
                    Sessions.Remove(audioSession);
                }
            }
            session.AudioSessionControl2.RegisterAudioSessionNotification(audioNotification);
            Sessions.Add(session);
        }

        public void RemoveSession(AudioSession session)
        {
            Sessions.Remove(session);
        }
      
        public void VolumeUp()
        {
            foreach (var session in Sessions)
            {
                session.Volume += stepVolume;
            }
        }

        public void VolumeDown()
        {
            foreach (var session in Sessions)
            {
                session.Volume -= stepVolume;
            }
        }

        public int Volume
        {
            get
            {
                if (Sessions.Count == 0) return 0;

                float min = 1;
                foreach (var session in Sessions)
                {
                    if (session.Volume < min)
                    {
                        min = session.Volume;
                    }
                }
                return (int)(min * 100);
            }
            set
            {
                if (value < 0) value = 0;
                if (value > 100) value = 100;
                foreach (var session in Sessions)
                {
                    session.Volume = value / 100f;
                }
            }
        }

        public void SetMute(bool mute)
        {
            foreach (var session in Sessions)
            {
                session.Mute = mute;
            }
        }

        private class AudioNotification : IAudioSessionEvents
        {
            private AudioProgram audioProgram;

            public AudioNotification(AudioProgram audioProgram)
            {
                this.audioProgram = audioProgram;
            }

            public void OnSessionDisconnected(AudioSessionDisconnectReason DisconnectReason)
            {
                Console.WriteLine("1");
            }

            public void OnDisplayNameChanged([MarshalAs(UnmanagedType.LPWStr)] string NewDisplayName, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext)
            {
                Console.WriteLine("2");
            }

            public void OnIconPathChanged([MarshalAs(UnmanagedType.LPWStr)] string NewIconPath, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext)
            {
                Console.WriteLine("3");
            }

            public void OnSimpleVolumeChanged(float NewVolume, bool NewMute, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext)
            {
                Console.WriteLine(audioProgram?.Name);
            }

            public void OnChannelVolumeChanged(int ChannelCount, IntPtr NewChannelVolumeArray, int ChangedChannel, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext)
            {
                Console.WriteLine("5");
            }

            public void OnGroupingParamChanged([MarshalAs(UnmanagedType.LPStruct)] Guid NewGroupingParam, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext)
            {
                Console.WriteLine("6");
            }

            public void OnStateChanged(AudioSessionState NewState)
            {
                Console.WriteLine(NewState.ToString());
            }

        }
    }
}

