﻿using Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using static Audio.AudioUtilities;

namespace AudioMixer
{
    class AudioController
    {
        private List<Program> programs = new List<Program>();


        public AudioController()
        {
            IList<AudioSession> sessions = AudioUtilities.GetAndSubscribeForAllSessions(new AudioNotification(this));
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
        public void AddProgram(string name, string exePath)
        {
            programs.Add(new Program(name, exePath));
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

            }

            public void OnDisplayNameChanged([MarshalAs(UnmanagedType.LPWStr)] string NewDisplayName, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext)
            {

            }

            public void OnIconPathChanged([MarshalAs(UnmanagedType.LPWStr)] string NewIconPath, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext)
            {

            }

            public void OnSimpleVolumeChanged(float NewVolume, bool NewMute, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext)
            {
                
            }

            public void OnChannelVolumeChanged(int ChannelCount, IntPtr NewChannelVolumeArray, int ChangedChannel, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext)
            {
                
            }

            public void OnGroupingParamChanged([MarshalAs(UnmanagedType.LPStruct)] Guid NewGroupingParam, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext)
            {
               
            }

            public void OnStateChanged(AudioSessionState NewState)
            {
              
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
                Sessions.Add(session);
            }

            public void RemoveSession(AudioSession session)
            {
                Sessions.Remove(session);
            }


        }
    }
}