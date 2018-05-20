using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioMixer
{
    public static class USBCommand
    {
        public static byte[] CreateCommandVolume(byte volume, byte numberProgram)
        {
            byte[] command = new byte[64];
            command[0] = 1;
            command[1] = numberProgram;
            command[2] = volume;
            return command;
        }

        public static bool ParseCommandVolume(byte[] command, ref byte numberProgram, ref byte volume)
        {
            if(command[1] == 1)
            {
                numberProgram = command[2];
                volume = command[3];
                return true;
            }
            return false;
        }
        public static bool ParseCommandVolume(byte[] command, ref byte numberProgram, ref byte rotation, ref byte position)
        {
            if (command[1] == 1)
            {
                numberProgram = command[2];
                rotation = command[3];
                position = command[4];
                return true;
            }
            return false;
        }
    }
}
