using Audio;
using System;
using System.Collections.Generic;
using static Audio.AudioUtilities;

namespace AudioMixer
{
    public class AudioController
    {
        private List<AudioProgram> programs = new List<AudioProgram>();

        public AudioController()
        {
            AudioUtilities.GetAndSubscribeForAllSessions(new AudioNotification(this));
        }

        public AudioProgram GetAudioProgram(int index)
        {
            if (index > programs.Count - 1 || index < 0)
                throw new ArgumentOutOfRangeException();
            return programs[index];
        }
        public int CountProgram
        {
            get => programs.Count;
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


            programs.Add(new AudioProgram(name, exePath));
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

        internal class AudioNotification : IAudioSessionNotification
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
                        //program.Volume = (int)(session.Volume * 100);
                    }

                }
            }
    
        }
    } 
}
