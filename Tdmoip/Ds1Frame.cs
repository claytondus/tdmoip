using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Tdmoip
{
    //Octet aligned DS1 frame.  
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Ds1Frame
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public byte[] Subframes;
        public byte FramingBit;
    }

    //Octet aligned DS1 extended superframe.  
    //Odd numbered framing bits are FDL
    //2,6,10,14,18,22: CRC x^6+x+1
    //4,8,12,16,20,24: Framing pattern 001011
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Ds1ExtendedSuperframe
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public Ds1Frame[] Frames;
    }

    public static class Ds1FrameExtensions {

        public static Ds1ExtendedSuperframe CreateExtendedSuperframe()
        {
            var esf = new Ds1ExtendedSuperframe {Frames = new Ds1Frame[24]};
            for (var i = 0; i < 24; i++)
            {
                esf.Frames[i].Subframes = new byte[24];
            }
            //Setup framing pattern
            esf.Frames[3].FramingBit = 0;
            esf.Frames[7].FramingBit = 0;
            esf.Frames[11].FramingBit = 1;
            esf.Frames[15].FramingBit = 0;
            esf.Frames[19].FramingBit = 1;
            esf.Frames[23].FramingBit = 1;
            return esf;
        }


    }
}
