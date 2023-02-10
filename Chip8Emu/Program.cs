using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;


//using doc at http://devernay.free.fr/hacks/chip8/C8TECH10.HTM
namespace Chip8Emu
{
    class Program
    {
        static void Main(string[] args)
        {
            
            //
            byte[] FontData = {
                0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
                0x20, 0x60, 0x20, 0x20, 0x70, // 1
                0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
                0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
                0x90, 0x90, 0xF0, 0x10, 0x10, // 4
                0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
                0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
                0xF0, 0x10, 0x20, 0x40, 0x40, // 7
                0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
                0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
                0xF0, 0x90, 0xF0, 0x90, 0x90, // A
                0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
                0xF0, 0x80, 0x80, 0x80, 0xF0, // C
                0xE0, 0x90, 0x90, 0x90, 0xE0, // D
                0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
                0xF0, 0x80, 0xF0, 0x80, 0x80 // F
            };
            
            Array.Copy(FontData,0x000 , ChipHardware.MEMORY,0x050,FontData.Length);

            // Console.WriteLine(BitDebugger.DumpByteArray(ChipHardware.MEMORY,4,1024,true));

            ChipHardware.PC = 0x200;
            ChipHardware.I = 0;
            ChipHardware.SP = 0;
            
            byte[] program = File.ReadAllBytes(args[0]);
            Array.Copy(program, 0, ChipHardware.MEMORY, 0x200, program.Length);
            
            Console.WriteLine(BitDebugger.DumpByteArray(ChipHardware.MEMORY,4,1024,false));

            // ChipHardware.OPC_BNNN(0xB123);

        }
    }
}
