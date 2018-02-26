using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Audio;

namespace AudioMixer
{
    internal static class Program
    {
        [MTAThread]
        static void Main(string[] args)
        {
             foreach (AudioSession session in AudioUtilities.GetAllSessions())
             {
                 if (session.Process != null)
                 {
                     Console.WriteLine("Path: " + session.Process.MainModule.FileName + "| Process Name: " + session.Process.ProcessName);
                 }
             }
         
            AudioController controller = new AudioController();
            controller.AddProgram("WMPlayer", @"C:\Program Files (x86)\Windows Media Player\wmplayer.exe");


            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    controller.VolumeUp("WMPlayer");
                    continue;
                }

                if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    controller.VolumeDown("WMPlayer");
                    continue;
                }



            } while (keyInfo.Key != ConsoleKey.Escape);


        }
    }










}
