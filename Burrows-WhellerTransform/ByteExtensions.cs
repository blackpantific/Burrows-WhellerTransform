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
        public static BitArray ToBitArray(this List<byte> bytes, int bitCount)
        {
            BitArray ba;
            var remainderOfTheDivision = bytes.Count % 8;
            if (remainderOfTheDivision != 0)
            {
                var nullNumbers = 8 - remainderOfTheDivision;
                ba = new BitArray(bitCount + nullNumbers);
                for (int i = 0; i < bytes.Count; i++)
                {
                    ba.Set(i, Convert.ToBoolean(bytes[i]));
                }
                for (int i = bytes.Count; i < bitCount + nullNumbers; i++)
                {
                    ba.Set(i, false);
                }
            }
            else
            {
                ba = new BitArray(bitCount);
                for (int i = 0; i < bytes.Count; i++)
                {
                    ba.Set(i, Convert.ToBoolean(bytes[i]));
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

        public static List<byte> ByteArrayToBitList(this byte[] byteArray/*, int bitCount*/)
        {
            List<byte> ret = new List<byte>();
            //var comp = bitCount * 8;
            //BitArray ba = new BitArray(comp);
            for (int i = 0; i < byteArray.Length; i++)
            {
                var byteToString = Convert.ToString(byteArray[i], 2).PadLeft(8, '0').ToCharArray().Reverse().ToArray();//
                var reverse = byteToString.Select(x => byte.Parse(x.ToString())).ToList();
                ret.AddRange(reverse);

            }
            return ret;
        }
        public static List<byte> ByteListToBitList(this List<byte> byteArray/*, int bitCount*/)
        {
            List<byte> ret = new List<byte>();
            //var comp = bitCount * 8;
            //BitArray ba = new BitArray(comp);
            for (int i = 0; i < byteArray.Count; i++)
            {
                var byteToString = Convert.ToString(byteArray[i], 2).PadLeft(8, '0').ToCharArray().Reverse().ToArray();//
                var reverse = byteToString.Select(x => byte.Parse(x.ToString())).ToList();
                ret.AddRange(reverse);

            }
            return ret;
        }

        //public static BitArray ByteArrayToBitArray(this byte[] byteArray, int bitCount)
        //{
        //    var comp = bitCount * 8;
        //    BitArray ba = new BitArray(comp);
        //    for (int i = 0; i < comp; i++)
        //    {
        //        ba.Set(i, Convert.ToBoolean(byteArray[i]));
        //    }

        //    var remainderOfTheDivision = ba.Length % 8;
        //    if (remainderOfTheDivision != 0)
        //    {
        //        for (int i = 0; i < remainderOfTheDivision; i++)
        //        {
        //            ba.Set(ba.Length - 1, true);
        //        }
        //    }

        //    return ba;
        //}


        //public static List<byte> BitArrayToSingleBitByteArray(this BitArray bits)
        //{
        //    //byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
        //    //bits.CopyTo(ret, 0);
        //    ////return ret;
        //    //byte[] ret = new byte[bits.Count];
        //    List<byte> ret = new List<byte>();
        //    for (int i = 0; i < bits.Count; i++)
        //    {
        //        ret.Add(Convert.ToByte(bits.Get(i)));
        //    }
        //    return ret;
        //}



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
