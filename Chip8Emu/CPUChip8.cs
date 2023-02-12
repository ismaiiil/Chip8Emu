using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.Xna.Framework.Input;

namespace Chip8Emu;

public static class CpuChip8
{
    //Font start adress
    public const ushort FontStartAddr = 0x050;

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

    public const ushort GFX_WIDTH = 64;
    public const ushort GFX_HEIGHT = 32;
    
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

    /*
     * 00E0 - CLS
     * Clear the display.
     */
    public static void OPC_00E0(ushort opcode)
    {
        Array.Clear(GFX, 0, GFX.Length);
    }

    /*
     * 00EE - RET
     * Return from a subroutine.
     * The interpreter sets the program counter to the address at the top of the stack, then subtracts 1 from the stack pointer.
     */
    public static void OPC_00EE(ushort opcode)
    {
        if(SP > 0) --SP;
        PC = STACK[SP];
    }

    /*
     * 1nnn - JP addr
     * Jump to location nnn.
     * The interpreter sets the program counter to nnn.
     */
    public static void OPC_1nnn(ushort opcode)
    {
        PC = (ushort)(opcode & 0x0FFF);
    }

    /*
    * 2nnn - CALL addr
	* Call subroutine at nnn.
	* The interpreter increments the stack pointer, then puts the current PC on the top of the stack. The PC is then set to nnn.
     */
    public static void OPC_2nnn(ushort opcode)
    {
        STACK[SP] = PC;
        SP++;
        PC = (ushort)(opcode & 0x0FFF);
    }


    /*
	* 3xkk - SE Vx, byte
	* Skip next instruction if Vx = kk.
	* The interpreter compares register Vx to kk, and if they are equal, increments the program counter by 2.
     */
    public static void OPC_3xkk(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte kk = (byte)(opcode & 0x00FF);

        if (V[Vx] == kk)
        {
            PC += 2;
        }
    }

    /*
	* 4xkk - SNE Vx, byte
	* Skip next instruction if Vx != kk.
	* The interpreter compares register Vx to kk, and if they are not equal, increments the program counter by 2.
     */
    public static void OPC_4xkk(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte kk = (byte)(opcode & 0x00FF);

        if (V[Vx] != kk)
        {
            PC += 2;
        }
    }

    /*
	* 5xy0 - SE Vx, Vy
	* Skip next instruction if Vx = Vy.
	* The interpreter compares register Vx to register Vy, and if they are equal, increments the program counter by 2.
     */
    public static void OPC_5xy0(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte Vy = (byte)((opcode & 0x00F0) >> 4);

        if (V[Vx] == V[Vy])
        {
            PC += 2;
        }
    }

    /*
	* 6xkk - LD Vx, byte
	* Set Vx = kk.
	* The interpreter puts the value kk into register Vx.
     */
    public static void OPC_6xkk(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte kk = (byte)(opcode & 0x00FF);

        V[Vx] = kk;
    }

    /*
	* 7xkk - ADD Vx, byte
	* Set Vx = Vx + kk.
	* Adds the value kk to the value of register Vx, then stores the result in Vx.
     */
    public static void OPC_7xkk(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte kk = (byte)(opcode & 0x00FF);

        V[Vx] += kk;
    }

    /*
	* 8xy0 - LD Vx, Vy
	* Set Vx = Vy.
	* Stores the value of register Vy in register Vx.
     */
    public static void OPC_8xy0(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte Vy = (byte)((opcode & 0x00F0) >> 4);

        V[Vx] = V[Vy];
    }

    /*
	* 8xy1 - OR Vx, Vy
	* Set Vx = Vx OR Vy.
	* Performs a bitwise OR on the values of Vx and Vy, then stores the result in Vx.
     */
    public static void OPC_8xy1(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte Vy = (byte)((opcode & 0x00F0) >> 4);

        V[Vx] |= V[Vy];
    }
    
    /*
	* 8xy2 - AND Vx, Vy
	* Set Vx = Vx AND Vy.
	* Performs a bitwise AND on the values of Vx and Vy, then stores the result in Vx.
     */
    public static void OPC_8xy2(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte Vy = (byte)((opcode & 0x00F0) >> 4);

        V[Vx] &= V[Vy];
    }
    
    /*
	* 8xy3 - XOR Vx, Vy
	* Set Vx = Vx XOR Vy.
	* Performs a bitwise exclusive OR on the values of Vx and Vy, then stores the result in Vx.
     */
    public static void OPC_8xy3(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte Vy = (byte)((opcode & 0x00F0) >> 4);

        V[Vx] ^= V[Vy];
    }
    
    /*
	* 8xy4 - ADD Vx, Vy
	* Set Vx = Vx + Vy, set VF = carry.
	* The values of Vx and Vy are added together. If the result is greater than 8 bits (i.e., > 255,) VF is set to 1, otherwise 0. Only the lowest 8 bits of the result are kept, and stored in Vx.
     */
    public static void OPC_8xy4(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte Vy = (byte)((opcode & 0x00F0) >> 4);

        ushort total = (ushort)(V[Vx] + V[Vy]);

        V[0xF] = total > 255 ? (byte)1 : (byte)0;
        
        V[Vx] = (byte)(total & 0xFFu);
    }
    
    /*
    * 8xy5 - SUB Vx, Vy
    * Set Vx = Vx - Vy, set VF = NOT borrow.
    * If Vx > Vy, then VF is set to 1, otherwise 0. Then Vy is subtracted from Vx, and the results stored in Vx.
     */
    public static void OPC_8xy5(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte Vy = (byte)((opcode & 0x00F0) >> 4);
        
        V[0xF] = V[Vx] > V[Vy] ? (byte)1 : (byte)0;
        
        V[Vx] -= V[Vy];
    }
    
    /*
	* 8xy6 - SHR Vx {, Vy}
	* Set Vx = Vx SHR 1.
	* If the least-significant bit of Vx is 1, then VF is set to 1, otherwise 0. Then Vx is divided by 2.
     */
    public static void OPC_8xy6(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        // byte Vy = (byte)((opcode & 0x00F0) >> 4); unused
        
        V[0xF] = (byte)(V[Vx] & 0x1); //Least significant bit
        
        V[Vx] >>= 1; //the shift divides by 2
    }
    
    /*
	* 8xy7 - SUBN Vx, Vy
	* Set Vx = Vy - Vx, set VF = NOT borrow.
	* If Vy > Vx, then VF is set to 1, otherwise 0. Then Vx is subtracted from Vy, and the results stored in Vx.
     */
    public static void OPC_8xy7(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte Vy = (byte)((opcode & 0x00F0) >> 4);
        
        V[0xF] = V[Vx] < V[Vy] ? (byte)1 : (byte)0;
      
        V[Vx] = (byte)(V[Vy] - V[Vx]);
    }
    
    /*
	* 8xyE - SHL Vx {, Vy}
	* Set Vx = Vx SHL 1.
	* If the most-significant bit of Vx is 1, then VF is set to 1, otherwise to 0. Then Vx is multiplied by 2.
     */
    public static void OPC_8xyE(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        // byte Vy = (byte)((opcode & 0x00F0) >> 4); unused
        
        V[0xF] = (byte)((V[Vx] & 0x80) >> 7); //Most significant bit
        V[Vx] <<= 1; //the shift multiples by 2
    }
    
    /*
	* 9xy0 - SNE Vx, Vy
	* Skip next instruction if Vx != Vy.
	* The values of Vx and Vy are compared, and if they are not equal, the program counter is increased by 2.
     */
    public static void OPC_9xy0(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte Vy = (byte)((opcode & 0x00F0) >> 4);

        if (V[Vx] != V[Vy])
        {
            PC += 2;
        }
    }
    
    /*
	* Annn - LD I, addr
	* Set I = nnn.
	* The value of register I is set to nnn.
     */
    public static void OPC_Annn(ushort opcode)
    {
        I = (ushort)(opcode & 0x0FFF);
    }
    
    /*
	* Bnnn - JP V0, addr
	* Jump to location nnn + V0.
	* The program counter is set to nnn plus the value of V0.
    */
    public static void OPC_Bnnn(ushort opcode)
    {
        PC = (ushort)(V[0] + (opcode & 0x0FFF));
    }
    
    /*
	* Cxkk - RND Vx, byte
	* Set Vx = random byte AND kk.
	* The interpreter generates a random number from 0 to 255, which is then ANDed with the value kk. The results are stored in Vx.
     */
    public static void OPC_Cxkk(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte kk = (byte)(opcode & 0x00FF);

        V[Vx] = (byte)(getRandomByte()[0] & kk);
    }
    
    /*
	* Dxyn - DRW Vx, Vy, nibble
	* Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision.
	* The interpreter reads n bytes from memory, starting at the address stored in I. These bytes are then displayed as sprites on screen at coordinates (Vx, Vy).
    * Sprites are XORed onto the existing screen. If this causes any pixels to be erased, VF is set to 1, otherwise it is set to 0.
     * If the sprite is positioned so part of it is outside the coordinates of the display, it wraps around to the opposite side of the screen.
    */
    public static void OPC_Dxyn(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte Vy = (byte)((opcode & 0x00F0) >> 4);
        ushort spriteHeight =  (byte)(opcode & 0x000F);

        int x = V[Vx];
        int y = V[Vy];
        
        V[0xF] = 0;
        
        byte spriteByte;
        ushort singleSpritePixel;
        int screenPixel;
        
        for (int row = 0; row < spriteHeight; row++)
        {
            spriteByte = MEMORY[I + row];
            for (int column = 0; column < 8; column++)
            {
                singleSpritePixel = (ushort)(spriteByte & (0x80 >> column));
                screenPixel = x + column + (y + row) * 64;
                if (screenPixel >= GFX_WIDTH * GFX_HEIGHT)
                {
                    screenPixel -= GFX_WIDTH * GFX_HEIGHT ;
                }
                if (singleSpritePixel != 0)
                {
                    if (GFX[screenPixel] == 1)
                    {
                        V[0xF] = 1;
                    }
                    GFX[screenPixel] ^= 1;
                }
            }
        }
        
    }

    /*
    * Ex9E - SKP Vx
    * Skip next instruction if key with the value of Vx is pressed.
    * Checks the keyboard, and if the key corresponding to the value of Vx is currently in the down position, PC is increased by 2.
     */
    public static void OPC_Ex9E(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte key = V[Vx];

        if (keys[key] > 0)
        {
            PC += 2;
        }
    }
    
    /*
	* ExA1 - SKNP Vx
	* Skip next instruction if key with the value of Vx is not pressed.
	* Checks the keyboard, and if the key corresponding to the value of Vx is currently in the up position, PC is increased by 2.
    */
    public static void OPC_ExA1(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte key = V[Vx];

        if (keys[key] == 0)
        {
            PC += 2;
        }
    }
    
    /*
	* Fx07 - LD Vx, DT
	* Set Vx = delay timer value.
	* The value of DT is placed into Vx.
     */
    public static void OPC_Fx07(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        V[Vx] = DT;
    }
    
    /*
	* Fx0A - LD Vx, K
	* Wait for a key press, store the value of the key in Vx.
	* All execution stops until a key is pressed, then the value of that key is stored in Vx.
     */
    public static void OPC_Fx0A(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        if (keys[0] > 0)
        {
            V[Vx] = 0;
        }
        else if (keys[1] > 0)
        {
            V[Vx] = 1;
        }
        else if (keys[2] > 0)
        {
            V[Vx] = 2;
        }
        else if (keys[3] > 0)
        {
            V[Vx] = 3;
        }
        else if (keys[4] > 0)
        {
            V[Vx] = 4;
        }
        else if (keys[5] > 0)
        {
            V[Vx] = 5;
        }
        else if (keys[6] > 0)
        {
            V[Vx] = 6;
        }
        else if (keys[7] > 0)
        {
            V[Vx] = 7;
        }
        else if (keys[8] > 0)
        {
            V[Vx] = 8;
        }
        else if (keys[9] > 0)
        {
            V[Vx] = 9;
        }
        else if (keys[10] > 0)
        {
            V[Vx] = 10;
        }
        else if (keys[11] > 0)
        {
            V[Vx] = 11;
        }
        else if (keys[12] > 0)
        {
            V[Vx] = 12;
        }
        else if (keys[13] > 0)
        {
            V[Vx] = 13;
        }
        else if (keys[14] > 0)
        {
            V[Vx] = 14;
        }
        else if (keys[15] > 0)
        {
            V[Vx] = 15;
        }
        else
        {
            PC -= 2;
        }
    }
    
    /*
	* Fx15 - LD DT, Vx
	* Set delay timer = Vx.
	* DT is set equal to the value of Vx.
     */
    public static void OPC_Fx15(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        DT = V[Vx];
    }
    
    /*
	* Fx18 - LD ST, Vx
	* Set sound timer = Vx.
	* ST is set equal to the value of Vx.
    */
    public static void OPC_Fx18(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        ST = V[Vx];
    }
    
    /*
	* Fx1E - ADD I, Vx
	* Set I = I + Vx.
	* The values of I and Vx are added, and the results are stored in I.
    */
    public static void OPC_Fx1E(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        I += V[Vx];
    }
    
    /*
	* Fx29 - LD F, Vx
	* Set I = location of sprite for digit Vx.
	* The value of I is set to the location for the hexadecimal sprite corresponding to the value of Vx.
    */
    public static void OPC_Fx29(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte currentFontStart = V[Vx];
        I = (ushort)(FontStartAddr + (5 * currentFontStart));
    }
    
    /*
	* Fx33 - LD B, Vx
	* Store BCD representation of Vx in memory locations I, I+1, and I+2.
	* The interpreter takes the decimal value of Vx, and places the hundreds digit in memory at location in I, the tens digit at location I+1, and the ones digit at location I+2.
    */
    public static void OPC_Fx33(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte number = V[Vx];
        
        // first decimal point
        MEMORY[I + 2] = (byte)(number % 10);
        number /= 10;

        // Tens-place
        MEMORY[I + 1] = (byte)(number % 10);
        number /= 10;

        // Hundreds-place
        MEMORY[I] = (byte)(number % 10);
    }
    
    /*
	* Fx55 - LD [I], Vx
	* Store registers V0 through Vx in memory starting at location I.
	* The interpreter copies the values of registers V0 through Vx into memory, starting at the address in I.
    */
    public static void OPC_Fx55(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);

        for (byte i = 0; i <= Vx; ++i)
        {
            MEMORY[I + i] = V[i];
        }
    }
    
    /*
	* Fx65 - LD Vx, [I]
	* Read registers V0 through Vx from memory starting at location I.
	* The interpreter reads values from memory starting at location I into registers V0 through Vx.
    */
    public static void OPC_Fx65(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);

        for (byte i = 0; i <= Vx; ++i)
        {
            V[i] = MEMORY[I + i];
        }
    }
    
    /*
    * Any unrcognized opcode will be defaulted to this.
    */
    public static void OPC_NULL(ushort opcode)
    {
        Console.WriteLine("{0:X4} <==== NULL OPCODE OR UNRECOGNISED, Are sure this is a Chip 8 rom? Not SuperChip8,XO chip etc...",opcode);
    }
    
    //create a Function Pointer Table starting from { , OPC_8XY0 }, opcodes of the CHIP 8 using dictionary
    public delegate void OpcodeFunction(ushort opcode);

    [SuppressMessage("Usage", "CS8974", Justification = "Not production code.")]
    public static readonly Dictionary<ushort, object> OpcodeTable = new()
    {
       
        { 0x0, new Dictionary<ushort,object>
                {
                    { 0x0, OPC_00E0 },
                    { 0xE, OPC_00EE }, 
                }},

        { 0x1, OPC_1nnn }, 
        { 0x2, OPC_2nnn }, 
        { 0x3, OPC_3xkk },
        { 0x4, OPC_4xkk }, 
        { 0x5, OPC_5xy0 }, 
        { 0x6, OPC_6xkk }, 
        { 0x7, OPC_7xkk }, 
        { 0x9, OPC_9xy0 }, 
        { 0xA, OPC_Annn }, 
        { 0xB, OPC_Bnnn },
        { 0xC, OPC_Cxkk },
        { 0xD, OPC_Dxyn },

        { 0x8, new Dictionary<ushort,object>
                {
                    { 0x0, OPC_8xy0 },
                    { 0x1, OPC_8xy1 }, 
                    { 0x2, OPC_8xy2 },
                    { 0x3, OPC_8xy3 }, 
                    { 0x4, OPC_8xy4 }, 
                    { 0x5, OPC_8xy5 },
                    { 0x6, OPC_8xy6 }, 
                    { 0x7, OPC_8xy7 },
                    { 0xE, OPC_8xyE }, 
                } },

        { 0xE, new Dictionary<ushort,object>
                {
                    { 0xE, OPC_Ex9E },
                    { 0x1, OPC_ExA1 },
                }},
        
        { 0xF, new Dictionary<ushort,object>
        {
            { 0x07, OPC_Fx07 },
            { 0x0A, OPC_Fx0A },
            { 0x15, OPC_Fx15 },
            { 0x18, OPC_Fx18 },
            { 0x1E, OPC_Fx1E },
            { 0x29, OPC_Fx29 },        
            { 0x33, OPC_Fx33 },
            { 0x55, OPC_Fx55 },
            { 0x65, OPC_Fx65 }
        }},
        
        { 0x404, OPC_NULL },
    };

    public static void SetKeyWithInput(int index, Keys key)
    {
        keys[index] = (byte)(Keyboard.GetState().IsKeyDown(key) ? 1 : 0);
    }
    
    
    public static void CycleCPUOnce()
    {
        // --------------- FETCH 
        //concatenate two bytes from memory to get a 16 bit opcode
        ushort opcode = (ushort)(MEMORY[PC] << 8 | MEMORY[PC + 1]);

        if (opcode == 0xF018)
        {
            Console.WriteLine("score v1");
        }

        //Move PROGRAM CURSOR
        PC += 2;
        
        //Get the index of the Opcode
        
        // --------------- DECODE 
        byte leftKey = (byte)((opcode & 0xF000) >> 12); // The first nibble

        var opCodeMedthod = OpcodeTable[leftKey];
        var opCodeNested = opCodeMedthod as Dictionary<ushort, object>;
        
        if (opCodeNested == null)
        {
            ((Action<ushort>)opCodeMedthod)(opcode); //EXECUTE pattern _nnn
        }
        else
        {
            try
            {
                if (leftKey == 0xF)
                {
                    byte rightKey = (byte)(opcode & 0x00FF);
                    ((Action<ushort>)opCodeNested[rightKey])(opcode); //EXECUTE pattern _n__
                
                }
                else
                {
                    byte rightKey = (byte)(opcode & 0x000F);
                    ((Action<ushort>)opCodeNested[rightKey])(opcode); //EXECUTE pattern _nn_
                }
            }
            catch (KeyNotFoundException e)
            {
                OPC_NULL(opcode);
            }
        }
        
        // Update Delay Timer register
        if (DT > 0)
        {
            --DT;
        }

        // Update Sound Timer register
        if (ST > 0)
        {
            --ST;
        }
    }
}