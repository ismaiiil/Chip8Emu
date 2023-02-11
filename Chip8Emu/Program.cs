using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

//using doc at http://devernay.free.fr/hacks/chip8/C8TECH10.HTM
namespace Chip8Emu
{
    class Program
    {
        public static bool QuitFlag= false;
        static void Main(string[] args)
        {

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
            
            Array.Copy(FontData,0x000 , CpuChip8.MEMORY,CpuChip8.FontStartAddr,FontData.Length);
            
            // Console.WriteLine(BitDebugger.DumpByteArray(ChipHardware.MEMORY,4,1024,true));
            
            CpuChip8.PC = 0x200;
            CpuChip8.I = 0;
            CpuChip8.SP = 0;
            long cycleDelay = 20000;
            
            byte[] program = File.ReadAllBytes(args[0]);
            Array.Copy(program, 0, CpuChip8.MEMORY, 0x200, program.Length);
            
            ThreadStart displayRef = DisplayThread;
            Thread displayThreadInst = new Thread(displayRef);
            displayThreadInst.Start();
            
            while (!QuitFlag)
            {
                Thread.Sleep(new TimeSpan(cycleDelay));
                CpuChip8.MainLoop();
            }
        }
        
        public static void DisplayThread()
        {
            Console.WriteLine("Display Thread Started");
            var game = new RenderingProject.Game1();
            game.Run();
        }
    }
}
