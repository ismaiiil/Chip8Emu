using Chip8Emu;

namespace TestsProject;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        //peepeepoopoo
    }

    [Test]
    public void Test_Opcode_Fetch()
    {
        // Arrange
        ushort expected = 0x124E;
        byte[] memory = new byte[2];
        memory[0] = 0x12;
        memory[1] = 0x4E;
        // Act
        ushort actual = (ushort)(memory[0] << 8                 //This shifts the bits to "pad" it with 8 bits 12 00
                                                | memory[1]);   //This combines the bits 12 00 with 00 4E resulting in the expected 124E
        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }
    
    [Test]
    public void Test_Opcode_Decode_OPC_1nnn()
    {
        ushort opcode   = 0x1234;
        byte opKey = (byte)((opcode & 0xF000) >> 12);
        var theObject = (Action<ushort>)ChipHardware.OpcodeTable[opKey];
        theObject(opcode);
    }
    
    [Test]
    public void Test_Opcode_Decode_OPC_8xy0()
    {
        ushort opcode   = 0x8630;
        
        byte opKey1 = (byte)((opcode & 0xF000) >> 12);
        byte opKey2 = (byte)(opcode & 0x000F);
        
        var theObject =  (Action<ushort>)((Dictionary<ushort,object>)ChipHardware.OpcodeTable[opKey1])[opKey2];
        theObject(opcode);
    
    }
    
    [Test]
    public void Test_Opcode_Decode_OPC_00E0()
    {
        ushort opcode   = 0x00E0;
        
        byte opKey1 = (byte)((opcode & 0xF000) >> 12);
        byte opKey2 = (byte)(opcode & 0x000F);
        
        var theObject =  (Action<ushort>)((Dictionary<ushort,object>)ChipHardware.OpcodeTable[opKey1])[opKey2];
        theObject(opcode);
    }
    
    [Test]
    public void Test_Opcode_Decode_OPC_Fx18()
    {
        ushort opcode   = 0xF218;
        
        byte opKey1 = (byte)((opcode & 0xF000) >> 12);
        byte opKey2 = (byte)(opcode & 0x00FF);
        
        var theObject =  (Action<ushort>)((Dictionary<ushort,object>)ChipHardware.OpcodeTable[opKey1])[opKey2];
        theObject(opcode);
    }
}