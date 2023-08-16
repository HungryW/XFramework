using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace XFrameworkBase
{
    public static class BinaryExtension
    {
        private static readonly byte[] ms_arrCacheBytes = new byte[byte.MaxValue + 1];

        public static int Read7BitEncodedInt32(this BinaryReader binaryReader)
        {
            int value = 0;
            int shift = 0;
            byte b;
            do
            {
                Debug.Assert(shift < 35);
                b = binaryReader.ReadByte();
                value |= (b & 0x7f) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);

            return value;
        }

        public static void Write7BitEncodedInt32(this BinaryWriter binaryWriter, int value)
        {
            uint num = (uint)value;
            while (num >= 0x80)
            {
                binaryWriter.Write((byte)(num | 0x80));
                num >>= 7;
            }

            binaryWriter.Write((byte)num);
        }

        public static uint Read7BitEncodedUInt32(this BinaryReader binaryReader)
        {
            return (uint)Read7BitEncodedInt32(binaryReader);
        }

        public static void Write7BitEncodedUInt32(this BinaryWriter binaryWriter, uint value)
        {
            Write7BitEncodedInt32(binaryWriter, (int)value);
        }

        public static long Read7BitEncodedInt64(this BinaryReader binaryReader)
        {
            long value = 0L;
            int shift = 0;
            byte b;
            do
            {
                Debug.Assert(shift < 70);

                b = binaryReader.ReadByte();
                value |= (b & 0x7fL) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);

            return value;
        }

        public static void Write7BitEncodedInt64(this BinaryWriter binaryWriter, long value)
        {
            ulong num = (ulong)value;
            while (num >= 0x80)
            {
                binaryWriter.Write((byte)(num | 0x80));
                num >>= 7;
            }

            binaryWriter.Write((byte)num);
        }


        public static ulong Read7BitEncodedUInt64(this BinaryReader binaryReader)
        {
            return (ulong)Read7BitEncodedInt64(binaryReader);
        }

        public static void Write7BitEncodedUInt64(this BinaryWriter binaryWriter, ulong value)
        {
            Write7BitEncodedInt64(binaryWriter, (long)value);
        }
        public static string ReadEncryptedString(this BinaryReader binaryReader, byte[] encryptBytes)
        {
            byte len = binaryReader.ReadByte();
            if (len <= 0)
            {
                return null;
            }
            Debug.Assert(len <= byte.MaxValue);
            for (byte i = 0; i < len; i++)
            {
                ms_arrCacheBytes[i] = binaryReader.ReadByte();
            }
            Utility.Encryption.GetSelfXorBytes(ms_arrCacheBytes, 0, len, encryptBytes);
            string val = Utility.Converter.GetString(ms_arrCacheBytes, 0, len);
            Array.Clear(ms_arrCacheBytes, 0, len);
            return val;
        }

        public static void WriteEncryptedString(this BinaryWriter binaryWriter, string value, byte[] encryptBytes)
        {
            if (string.IsNullOrEmpty(value))
            {
                binaryWriter.Write(0);
                return;
            }
            int nLen = Utility.Converter.GetBytes(value, ms_arrCacheBytes);
            Debug.Assert(nLen <= ms_arrCacheBytes.Length);

            Utility.Encryption.GetSelfXorBytes(ms_arrCacheBytes, encryptBytes);
            binaryWriter.Write((byte) nLen);
            binaryWriter.Write(ms_arrCacheBytes, 0, nLen);
        }
    }
}
