

using System;
using System.Diagnostics;
using System.IO;

namespace XFrameworkBase
{
    public static partial class Utility
    {
        /// <summary>
        /// 校验相关的实用函数。
        /// </summary>
        public static partial class Verifier
        {
            private const int CachedBytesLength = 0x1000;
            private static readonly byte[] s_CachedBytes = new byte[CachedBytesLength];
            private static readonly Crc32 s_Algorithm = new Crc32();

            /// <summary>
            /// 计算二进制流的 CRC32。
            /// </summary>
            /// <param name="bytes">指定的二进制流。</param>
            /// <returns>计算后的 CRC32。</returns>
            public static int GetCrc32(byte[] bytes)
            {
                Debug.Assert(bytes != null);

                return GetCrc32(bytes, 0, bytes.Length);
            }

            /// <summary>
            /// 计算二进制流的 CRC32。
            /// </summary>
            /// <param name="bytes">指定的二进制流。</param>
            /// <param name="offset">二进制流的偏移。</param>
            /// <param name="length">二进制流的长度。</param>
            /// <returns>计算后的 CRC32。</returns>
            public static int GetCrc32(byte[] bytes, int offset, int length)
            {
                Debug.Assert(bytes != null);
                Debug.Assert(offset >= 0 && length >= 0 && offset + length <= bytes.Length);

                s_Algorithm.HashCore(bytes, offset, length);
                int result = (int)s_Algorithm.HashFinal();
                s_Algorithm.Initialize();
                return result;
            }

            /// <summary>
            /// 计算二进制流的 CRC32。
            /// </summary>
            /// <param name="stream">指定的二进制流。</param>
            /// <returns>计算后的 CRC32。</returns>
            public static int GetCrc32(Stream stream)
            {
                Debug.Assert(stream != null);

                while (true)
                {
                    int bytesRead = stream.Read(s_CachedBytes, 0, CachedBytesLength);
                    if (bytesRead > 0)
                    {
                        s_Algorithm.HashCore(s_CachedBytes, 0, bytesRead);
                    }
                    else
                    {
                        break;
                    }
                }

                int result = (int)s_Algorithm.HashFinal();
                s_Algorithm.Initialize();
                Array.Clear(s_CachedBytes, 0, CachedBytesLength);
                return result;
            }

            /// <summary>
            /// 获取 CRC32 数值的二进制数组。
            /// </summary>
            /// <param name="crc32">CRC32 数值。</param>
            /// <returns>CRC32 数值的二进制数组。</returns>
            public static byte[] GetCrc32Bytes(int crc32)
            {
                return new byte[] { (byte)((crc32 >> 24) & 0xff), (byte)((crc32 >> 16) & 0xff), (byte)((crc32 >> 8) & 0xff), (byte)(crc32 & 0xff) };
            }

            /// <summary>
            /// 获取 CRC32 数值的二进制数组。
            /// </summary>
            /// <param name="crc32">CRC32 数值。</param>
            /// <param name="bytes">要存放结果的数组。</param>
            public static void GetCrc32Bytes(int crc32, byte[] bytes)
            {
                GetCrc32Bytes(crc32, bytes, 0);
            }

            /// <summary>
            /// 获取 CRC32 数值的二进制数组。
            /// </summary>
            /// <param name="crc32">CRC32 数值。</param>
            /// <param name="bytes">要存放结果的数组。</param>
            /// <param name="offset">CRC32 数值的二进制数组在结果数组内的起始位置。</param>
            public static void GetCrc32Bytes(int crc32, byte[] bytes, int offset)
            {
                Debug.Assert(bytes != null);
                Debug.Assert(offset >= 0 && offset+4<=bytes.Length);

                bytes[offset] = (byte)((crc32 >> 24) & 0xff);
                bytes[offset + 1] = (byte)((crc32 >> 16) & 0xff);
                bytes[offset + 2] = (byte)((crc32 >> 8) & 0xff);
                bytes[offset + 3] = (byte)(crc32 & 0xff);
            }

            internal static int GetCrc32(Stream stream, byte[] code, int length)
            {
                Debug.Assert(stream != null);
                Debug.Assert(code != null); 
                Debug.Assert(code.Length > 0);
                int codeLength = code.Length;

                int bytesLength = (int)stream.Length;
                if (length < 0 || length > bytesLength)
                {
                    length = bytesLength;
                }

                int codeIndex = 0;
                while (true)
                {
                    int bytesRead = stream.Read(s_CachedBytes, 0, CachedBytesLength);
                    if (bytesRead > 0)
                    {
                        if (length > 0)
                        {
                            for (int i = 0; i < bytesRead && i < length; i++)
                            {
                                s_CachedBytes[i] ^= code[codeIndex++];
                                codeIndex %= codeLength;
                            }

                            length -= bytesRead;
                        }

                        s_Algorithm.HashCore(s_CachedBytes, 0, bytesRead);
                    }
                    else
                    {
                        break;
                    }
                }

                int result = (int)s_Algorithm.HashFinal();
                s_Algorithm.Initialize();
                Array.Clear(s_CachedBytes, 0, CachedBytesLength);
                return result;
            }
        }
    }
}
