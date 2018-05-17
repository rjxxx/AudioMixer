using Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using static Audio.AudioUtilities;

namespace Audio
{
    public class AudioController
    {
        private List<Program> programs = new List<Program>();
        private float stepVolume = 0.01f;

        public float StepVolume {
            get => stepVolume;
            set {
                if (value > 0 && value <= 1f)
                    stepVolume = value;
            }
        }

        public AudioController()
        {
            AudioUtilities.GetAndSubscribeForAllSessions(new AudioNotification(this));

        }

        public void AddProgram(string name, string exePath)
        {
            foreach (var program in programs)
            {
                if (program.Name == name)
                {
                    programs.Remove(program);
                    break;
                }
            }
               
            
            programs.Add(new Program(name, exePath));
            IList<AudioSession> sessions = AudioUtilities.GetAllSessions();
            foreach (var program in programs)
            {
                foreach (var session in sessions)
                {
                    if (session.Process == null)
                        continue;

                    if (session.Process.MainModule.FileName == program.ExePath)
                    {
                        program.AddSession(session);
                    }
                }
            }
            
        }

        public void VolumeUp(string name)
        {
            foreach (var program in programs)
            {
                if (name == program.Name)
                {
                    foreach (var session in program.Sessions)
                    {
                        session.Volume += stepVolume;
                    }
                    return;
                }
            }
        }

        public void VolumeDown(string name)
        {
            foreach (var program in programs)
            {
                if (name == program.Name)
                {
                    foreach (var session in program.Sessions)
                    {
                        session.Volume -= stepVolume;
                    }
                    return;
                }
            }
        }
        public void VolumeSet(string name, int volume)
        {
            foreach (var program in programs)
            {
                if (name == program.Name)
                {
                    foreach (var session in program.Sessions)
                    {
                        session.Volume = volume / 100f;
                    }
                    return;
                }
            }
        }

        public int CurrentVolume(string name)
        {
            foreach (var program in programs)
            {
                if (name == program.Name)
                {
                    float min = 1;
                    foreach (var session in program.Sessions)
                    {
                        if (session.Volume < min)
                        {
                            min = session.Volume;
                        }
                    }
                    return (int)(min * 100);
                }
            }
            return 0;
        }

        public void SetMute(string name, bool mute)
        {
            foreach (var program in programs)
            {
                if (name == program.Name)
                {
                    foreach (var session in program.Sessions)
                    {
                        session.Mute = mute;
                    }
                    return;
                }
            }
        }


        internal class AudioNotification : IAudioNotification
        {
            AudioController parent;
            public AudioNotification(AudioController parent)
            {
                this.parent = parent;
            }

            public void OnSessionCreated(IAudioSessionControl NewSession)
            {
                Console.WriteLine("Connected");
                foreach (var program in parent.programs)
                {
                    AudioSession session = new AudioSession(NewSession as IAudioSessionControl2);
                    if (session.Process == null)
                       continue;

                    if (session.Process.MainModule.FileName == program.ExePath)
                    {
                        program.AddSession(session);
                    }
                   
                }
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
                Console.WriteLine("4");
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
                Console.WriteLine("7");
            }

        }
        class Program
        {
            private string name;
            private string exePath;
            private List<AudioSession> sessions = new List<AudioSession>();

            public string Name { get => name; set => name = value; }
            public string ExePath { get => exePath; set => exePath = value; }
            public List<AudioSession> Sessions { get => sessions; set => sessions = value; }

            public Program(string name, string exePath)
            {
                this.name = name;
                this.exePath = exePath;
            }

            public void AddSession(AudioSession session)
            {
                foreach(var audioSession in Sessions)
                {
                    if (audioSession == null || audioSession.State == AudioSessionState.Expired)
                    {
                        Sessions.Remove(audioSession);
                    }
                }
                Sessions.Add(session);
            }

            public void RemoveSession(AudioSession session)
            {
                Sessions.Remove(session);
            }


        }
    }
}
