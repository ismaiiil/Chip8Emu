using System;
using System.Collections;
using System.Linq;
using System.Security.Cryptography;

namespace Chip8Emu;

public static class ChipHardware
{
    //Main Memory of the chip
    public static byte[] MEMORY = new byte[4096];
    //Program Counter for keeping track of the current instruction in memory
    public static ushort PC = 0;
    //Index Register for memory addresses
    public static ushort I = 0;
    
    //General purpose register, Note that VF is not to be used by any programs.
    public static byte[] V = new byte[16];

    //A stack consisting of 16 Level of 16 bits Stack Levels
    public static ushort[] STACK = new ushort[16];
    //Stack Pointer to the topmost level of the stack above
    public static byte SP = 0;

    //Graphics Frame Buffer
    public static byte[] GFX = new byte[64 * 32];
    
    //Delay Timer
    public static byte DT = 0;
    //Sound Timer
    public static byte ST = 0;
    
    //Keyboard
    public static byte[] keys = new byte[16];
    
    
    public static byte[] getRandomByte()
    {
        return RandomNumberGenerator.GetBytes(1);
    }
    /*
    OpCodes are 16 bits long ie 4 hexa characters can be represented with ushort, ie a 16 bit value.
    
    nnn or addr - the lowest 12 bit value of an opcode representing an address in the memory (example FFF is address 4095)
    n or nibble - A 4-bit value, the lowest 4 bits of the instruction
    x           - A 4-bit value, the lower 4 bits of the high byte of the instruction
    y           - A 4-bit value, the upper 4 bits of the low byte of the instruction
    kk or byte  - An 8-bit value, the lowest 8 bits of the instruction
    */
    
    //Clear the display.
    public static void OPC_00E0_CLS()
    {
        Array.Clear(GFX, 0, GFX.Length);
    }
    
    //Return from a subroutine.
    public static void OPC_00EE_RET()
    {
        PC = STACK[--SP];
    }
    
    //Jump to an address (NNN)
    public static void OPC_1NNN_JP(ushort opcode)
    {
        PC = (ushort)(opcode & 0x0FFF); //the 0x0FFF is used to get the last 12 bits
    }
    
    //The interpreter increments the stack pointer, then puts the current PC on the top of the stack. The PC is then set to nnn.
    public static void OPC_2NNN_CALL(ushort opcode)
    {
        STACK[SP] = PC;
        SP++;
        PC = (ushort)(opcode & 0x0FFF);
    }
    
    //
    // //The interpreter compares register Vx to kk, and if they are equal, increments the program counter by 2.
    // public static void OPC_3XKK(int x,byte kk)
    // {
    //     if (V[x] == kk)
    //     {
    //         PC += 2;
    //     }
    // }
    

}