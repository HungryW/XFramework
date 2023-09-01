
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {
        public sealed class CSerializerVersionListLocal : CFrameWorkSerializer<CVersionListInfoLocal>
        {
            private static readonly byte[] Header = new byte[] { (byte)'X', (byte)'F', (byte)'L' };

            protected override byte[] __GetHeader()
            {
                return Header;
            }
        }

        public sealed class CVersionListInfoLocal
        {
            public static readonly CResourceInfo[] ms_EmptyResourceInfo = new CResourceInfo[] { };

            public readonly CResourceInfo[] m_arrResource;

            public CVersionListInfoLocal(CResourceInfo[] a_arrResInfo)
            {
                m_arrResource = a_arrResInfo;
            }

            public sealed class CResourceInfo
            {
                public readonly string m_szName;
                public readonly string m_szVariant;
                public readonly string m_szExtension;
                public readonly byte m_nLoadType;
                public readonly int m_nLen;
                public readonly int m_nHashCode;

                public CResourceInfo(string szName, string szVariant, string szExtension, byte nLoadType, int nLen, int nHashCode)
                {
                    m_szName = szName;
                    m_szVariant = szVariant;
                    m_szExtension = szExtension;
                    m_nLoadType = nLoadType;
                    m_nLen = nLen;
                    m_nHashCode = nHashCode;
                }
            }
        }

        public static partial class BuiltinVersionListSerializer
        {
            public static CVersionListInfoLocal LocalVeriosnListDeserializeCallback_V0(Stream stream)
            {
                using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8))
                {
                    byte[] arrEncryptBytes = binaryReader.ReadBytes(mc_nCacheHashBytesLen);
                    int nResourceCount = binaryReader.Read7BitEncodedInt32();
                    CVersionListInfoLocal.CResourceInfo[] arrResource = nResourceCount > 0 ? new CVersionListInfoLocal.CResourceInfo[nResourceCount] : CVersionListInfoLocal.ms_EmptyResourceInfo;
                    for (int i = 0; i < nResourceCount; i++)
                    {
                        string szName = binaryReader.ReadEncryptedString(arrEncryptBytes);
                        string szVariant = binaryReader.ReadEncryptedString(arrEncryptBytes);
                        string szExtension = binaryReader.ReadEncryptedString(arrEncryptBytes);
                        byte loadType = binaryReader.ReadByte();
                        int nLen = binaryReader.Read7BitEncodedInt32();
                        int nHash = binaryReader.Read7BitEncodedInt32();
                        arrResource[i] = new CVersionListInfoLocal.CResourceInfo(szName, szVariant, szExtension, loadType, nLen, nHash);
                    }
                    return new CVersionListInfoLocal(arrResource);
                }
            }
        }
    }
}
