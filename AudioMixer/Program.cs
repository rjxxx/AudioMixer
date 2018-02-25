using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Audio;

namespace AudioMixer
{
  internal class Program
    {
        [MTAThread]
        static void Main(string[] args)
        {

           /*    
            foreach (AudioSession session in AudioUtilities.GetAllSessions())
            {
                if (session.Process != null)
                {
                    Console.WriteLine("Path: " + session.Process.MainModule.FileName + "| Process Name: " + session.Process.ProcessName);
                    string fileName = Path.GetFileName(session.Process.MainModule.FileName);
                    if (fileName == "firefox.exe")
                    {
                        session.Volume = 100;
                    }
                }
            }
        */
            AudioController controller = new AudioController();

            


            Console.ReadKey();
            /*
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow: 
                        se
                }
            }
            */
        }

        
       
    }










}
