using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Chip8Emu;

public class BitDebugger
{

    public static String DumpByteArray(byte[] byteArray,int rowSize, int columnSize ,bool showSprites)
    {
        StringBuilder stringBuilder = new StringBuilder("");
        if (byteArray.Length > rowSize*columnSize)
        {
            throw new Exception("Array bigger than rows and columns allocated");
        }
        for (int column = 0; column < byteArray.Length/rowSize; column++)
        {
            for (int rows = 0; rows < byteArray.Length/columnSize; rows++)
            {
                int i = column + rows * byteArray.Length / rowSize;
                if (column%5==0 && rows == 0 && showSprites)
                {
                    stringBuilder.Append('\n');
                }

                var formattedBits = Convert.ToString(byteArray[i], 2).PadLeft(8, '0');
                if (showSprites)
                {
                    formattedBits = formattedBits.Replace("0", " ").Replace("1", "*");
                }
                    
                stringBuilder.Append(String.Format("|| {0} : " + "0x{1:X3} : " + "{2:X2} : " + "{3} ", 
                    Convert.ToString(i).PadLeft(4, paddingChar: '0'),
                    i,
                    byteArray[i], 
                    formattedBits));
                    
            }
            stringBuilder.Append("||");
            stringBuilder.Append('\n');
        }

        return stringBuilder.ToString();
    }
}