using Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioMixer
{
    enum AudioCommand
    {

    }
    public class AudioProgram
    {
        private string name;
        private string exePath;
        private List<AudioSession> sessions = new List<AudioSession>();
        private float stepVolume = 0.01f;


        public float StepVolume
        {
            get => stepVolume;
            set
            {
                if (value > 0 && value <= 1f)
                    stepVolume = value;
            }
        }

        public string Name { get => name; set => name = value; }
        public string ExePath { get => exePath; set => exePath = value; }
        public List<AudioSession> Sessions { get => sessions; set => sessions = value; }

        public AudioProgram(string name, string exePath)
        {
            this.name = name;
            this.exePath = exePath;
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
            Sessions.Add(session);
        }

        public void RemoveSession(AudioSession session)
        {
            Sessions.Remove(session);
        }
      
        public void VolumeUp()
        {
            foreach (var session in sessions)
            {
                session.Volume += stepVolume;
            }

        }

        public void VolumeDown()
        {
            foreach (var session in sessions)
            {
                session.Volume -= stepVolume;
            }
        }

        public int Volume
        {
            get
            {
                if (sessions.Count == 0) return 0;

                float min = 1;
                foreach (var session in sessions)
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
                foreach (var session in sessions)
                {
                    session.Volume = value / 100f;
                }
            }
        }

        public void SetMute(bool mute)
        {
            foreach (var session in sessions)
            {
                session.Mute = mute;
            }
        }

    }
}

