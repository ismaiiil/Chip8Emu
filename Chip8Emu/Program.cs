using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

//using doc at http://devernay.free.fr/hacks/chip8/C8TECH10.HTM
namespace Chip8Emu
{
    class Program
    {
        public static bool QuitFlag = false;
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
            long cycleDelay = 50000;
            
            byte[] program = File.ReadAllBytes(args[0]);
            Array.Copy(program, 0, CpuChip8.MEMORY, 0x200, program.Length);
            
            ThreadStart displayRef = DisplayThread;
            Thread displayThreadInst = new Thread(displayRef);
            displayThreadInst.Start();
            
            SoundEffect beep;
            SoundEffectInstance beepInstance;
            
            beep = new SoundEffect(new byte[] { 0x52, 0x49, 0x46, 0x46, 0x24, 0x0, 0x0, 0x0, 0x57, 0x41, 
                0x56, 0x45, 0x66, 0x6D, 0x74, 0x20, 0x10, 0x0, 0x0, 0x0, 0x1, 0x0, 0x1, 0x0, 0x44, 0xAC, 0x0, 0x0, 0x88, 
                0x58, 0x1, 0x0, 0x2, 0x0, 0x10, 0x0, 0x64, 0x61, 0x74, 0x61, 0x8, 0x0, 0x0, 0x0, 0xFC, 0xFF }, 8016, AudioChannels.Mono);
            beepInstance = beep.CreateInstance();
            // beepInstance.IsLooped = true;
            
            while (!QuitFlag)
            {
                
                // +-+-+-+-+    +-+-+-+-+
                // |1|2|3|C|    |1|2|3|4|
                // +-+-+-+-+    +-+-+-+-+
                // |4|5|6|D|    |Q|W|E|R|
                // +-+-+-+-+ => +-+-+-+-+
                // |7|8|9|E|    |A|S|D|F|
                // +-+-+-+-+    +-+-+-+-+
                // |A|0|B|F|    |Z|X|C|V|
                // +-+-+-+-+    +-+-+-+-+
                CpuChip8.SetKeyWithInput(1, Keys.D1);
                CpuChip8.SetKeyWithInput(2, Keys.D2);
                CpuChip8.SetKeyWithInput(3, Keys.D3);
                CpuChip8.SetKeyWithInput(0xC, Keys.D4);
                CpuChip8.SetKeyWithInput(4, Keys.Q);
                CpuChip8.SetKeyWithInput(5, Keys.W);
                CpuChip8.SetKeyWithInput(6, Keys.E);
                CpuChip8.SetKeyWithInput(0xD, Keys.R);
                CpuChip8.SetKeyWithInput(7, Keys.A);
                CpuChip8.SetKeyWithInput(8, Keys.S);
                CpuChip8.SetKeyWithInput(9, Keys.D);
                CpuChip8.SetKeyWithInput(0xE, Keys.F);
                CpuChip8.SetKeyWithInput(0xA, Keys.Z);
                CpuChip8.SetKeyWithInput(0, Keys.C);
                CpuChip8.SetKeyWithInput(0xF, Keys.V);
                
                if (CpuChip8.ST > 0 && beepInstance.State is SoundState.Stopped or SoundState.Paused)
                {
                    beepInstance.Play();
                }
                else
                {
                    beepInstance.Stop();
                }
                
                Thread.Sleep(new TimeSpan(cycleDelay));
                CpuChip8.CycleCPUOnce();
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
