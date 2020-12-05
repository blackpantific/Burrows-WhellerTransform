using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Burrows_WhellerTransform
{
    public static class ByteExtensions
    {
        //public static byte[] ToByteArray(this BitArray bits)
        //{
        //    byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
        //    bits.CopyTo(ret, 0);
        //    return ret;
        //}

        public static BitArray ToBitArray(this List<byte> bytes, int bitCount)
        {
            BitArray ba = new BitArray(bitCount);
            for (int i = 0; i < bytes.Count; i++)
            {
                ba.Set(i, Convert.ToBoolean(bytes[i]));
            }

            var remainderOfTheDivision = ba.Length % 8;
            if(remainderOfTheDivision!= 0)
            {
                for (int i = 0; i < remainderOfTheDivision; i++)
                {
                    ba.Set(ba.Length - 1, true);
                }
            }

            return ba;

        }

        public static byte[] BitArrayToByteArray(this BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }
        //public static BitArray ToBitArray(this byte[] bytes, int bitCount)
        //{
        //    BitArray ba = new BitArray(bitCount);
        //    for (int i = 0; i < bitCount; ++i)
        //    {
        //        ba.Set(i, ((bytes[i / 8] >> (i % 8)) & 0x01) > 0);
        //    }
        //    return ba;
        //}
    }
}
