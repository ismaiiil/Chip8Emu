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
    public static void OPC_00E0()
    {
        Array.Clear(GFX, 0, GFX.Length);
    }

    /*
     * 00EE - RET
     * Return from a subroutine.
     * The interpreter sets the program counter to the address at the top of the stack, then subtracts 1 from the stack pointer.
     */
    public static void OPC_00EE()
    {
        PC = STACK[--SP];
    }

    /*
     * 1nnn - JP addr
     * Jump to location nnn.
     * The interpreter sets the program counter to nnn.
     */
    public static void OPC_1NNN(ushort opcode)
    {
        PC = (ushort)(opcode & 0x0FFF); //the 0x0FFF is used to get the last 12 bits
    }

    /*
    * 2nnn - CALL addr
	* Call subroutine at nnn.
	* The interpreter increments the stack pointer, then puts the current PC on the top of the stack. The PC is then set to nnn.
     */
    public static void OPC_2NNN(ushort opcode)
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
    public static void OPC_3XKK(ushort opcode)
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
    public static void OPC_4XKK(ushort opcode)
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
    public static void OPC_5XY0(ushort opcode)
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
    public static void OPC_6XKK(ushort opcode)
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
    public static void OPC_7XKK(ushort opcode)
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
    public static void OPC_8XY0(ushort opcode)
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
    public static void OPC_8XY1(ushort opcode)
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
    public static void OPC_8XY2(ushort opcode)
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
    public static void OPC_8XY3(ushort opcode)
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
    public static void OPC_8XY4(ushort opcode)
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
    public static void OPC_8XY5(ushort opcode)
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
    public static void OPC_8XY6(ushort opcode)
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
    public static void OPC_8XY7(ushort opcode)
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
    public static void OPC_8XYE(ushort opcode)
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
    public static void OPC_9XY0(ushort opcode)
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
    public static void OPC_ANNN(ushort opcode)
    {
        I = (ushort)(opcode & 0x0FFF);
    }
    
    /*
	* Bnnn - JP V0, addr
	* Jump to location nnn + V0.
	* The program counter is set to nnn plus the value of V0.
    */
    public static void OPC_BNNN(ushort opcode)
    {
        PC = (ushort)(V[0] + (opcode & 0x0FFF));
    }
    
    /*
	* Cxkk - RND Vx, byte
	* Set Vx = random byte AND kk.
	* The interpreter generates a random number from 0 to 255, which is then ANDed with the value kk. The results are stored in Vx.
     */
    public static void OPC_CXKK(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte kk = (byte)(opcode & 0x00FF);

        V[Vx] = (byte)(getRandomByte()[0x1] & kk);
    }
    
    /*
	* Dxyn - DRW Vx, Vy, nibble
	* Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision.
	* The interpreter reads n bytes from memory, starting at the address stored in I. These bytes are then displayed as sprites on screen at coordinates (Vx, Vy).
    * Sprites are XORed onto the existing screen. If this causes any pixels to be erased, VF is set to 1, otherwise it is set to 0.
     * If the sprite is positioned so part of it is outside the coordinates of the display, it wraps around to the opposite side of the screen.
 */
    public static void OPC_DXYN(ushort opcode)
    {
        byte Vx = (byte)((opcode & 0x0F00) >> 8);
        byte Vy = (byte)((opcode & 0x00F0) >> 4);
        byte n =  (byte)(opcode & 0x000F);

        byte xAxisPosition = (byte)(V[Vx] % GFX_WIDTH);
        byte yAxisPosition = (byte)(V[Vy] % GFX_HEIGHT);
        
        V[0xF] = 0;
        for (int row = 0; row < n; ++row)
        {
            byte spriteByte = MEMORY[I + row];

            for (int col = 0; col < 8; ++col)
            {
                byte spritePixel = (byte)(spriteByte & (0x80u >> col));
                uint  screenPixel = (uint)((yAxisPosition + row) * GFX_WIDTH + (xAxisPosition + col));

                // Sprite pixel is on
                if ((spritePixel & (0x80 >> xAxisPosition)) != 0)
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
    
    
}