
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {
        public sealed class CSerializerVersionListRemote : CFrameWorkSerializer<CVersionListInfoRemote>
        {
            private static readonly byte[] Header = new byte[] { (byte)'X', (byte)'F', (byte)'R' };

            protected override byte[] __GetHeader()
            {
                return Header;
            }
        }

        public sealed class CVersionListInfoRemote
        {
            public static readonly CAssetInfo[] ms_EmptyAssetInfo = new CAssetInfo[] { };
            public static readonly CResourceInfo[] ms_EmptyResourceInfo = new CResourceInfo[] { };

            public readonly string m_szGameVersion;
            public readonly int m_nResVersion;
            public readonly CAssetInfo[] m_arrAsset;
            public readonly CResourceInfo[] m_arrResource;

            public CVersionListInfoRemote(string a_szGameVersion, int a_nResVersion, CResourceInfo[] a_arrResInfo, CAssetInfo[] a_arrAssetInfo)
            {
                m_szGameVersion = a_szGameVersion;
                m_nResVersion = a_nResVersion;
                m_arrResource = a_arrResInfo;
                m_arrAsset = a_arrAssetInfo;
            }

            public sealed class CAssetInfo
            {
                public readonly string m_szName;
                public readonly int[] m_arrDependAssetIdx;

                public CAssetInfo(string a_szName, int[] arrDependAssetIdx)
                {
                    m_szName = a_szName;
                    m_arrDependAssetIdx = arrDependAssetIdx;
                }
            }

            public sealed class CResourceInfo
            {
                public readonly string m_szName;
                public readonly string m_szVariant;
                public readonly string m_szExtension;
                public readonly byte m_nLoadType;
                public readonly int m_nLen;
                public readonly int m_nHashCode;
                public readonly int m_nCompressLen;
                public readonly int m_nCompressHash;
                public readonly int[] m_arrAssetIdx;

                public CResourceInfo(string szName, string szVariant, string szExtension, byte nLoadType, int nLen, int nHashCode, int a_nCompressLen, int a_nCopressHash, int[] arrAssetIdx)
                {
                    m_szName = szName;
                    m_szVariant = szVariant;
                    m_szExtension = szExtension;
                    m_nLoadType = nLoadType;
                    m_nLen = nLen;
                    m_nHashCode = nHashCode;
                    m_nCompressLen = a_nCompressLen;
                    m_nCompressHash = a_nCopressHash;
                    m_arrAssetIdx = arrAssetIdx;
                }
            }
        }

        public static partial class BuiltinVersionListSerializer
        {
            public static CVersionListInfoRemote RemoteVeriosnListDeserializeCallback_V0(Stream stream)
            {
                using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8))
                {
                    byte[] arrEncryptBytes = binaryReader.ReadBytes(mc_nCacheHashBytesLen);
                    string szApplicationGameVerison = binaryReader.ReadEncryptedString(arrEncryptBytes);
                    int nInernalResourceVersion = binaryReader.Read7BitEncodedInt32();
                    int nAssetCount = binaryReader.Read7BitEncodedInt32();
                    CVersionListInfoRemote.CAssetInfo[] arrAsset = nAssetCount > 0 ? new
                        CVersionListInfoRemote.CAssetInfo[nAssetCount] : CVersionListInfoRemote.ms_EmptyAssetInfo;

                    for (int i = 0; i < nAssetCount; i++)
                    {
                        string szAssetName = binaryReader.ReadEncryptedString(arrEncryptBytes);
                        int nDependAssetNum = binaryReader.Read7BitEncodedInt32();
                        int[] arrDependIdx = nDependAssetNum > 0 ? new int[nDependAssetNum] : ms_emptyArray;
                        for (int j = 0; j < nDependAssetNum; j++)
                        {
                            arrDependIdx[j] = binaryReader.Read7BitEncodedInt32();
                        }
                        arrAsset[i] = new CVersionListInfoRemote.CAssetInfo(szAssetName, arrDependIdx);
                    }

                    int nResourceCount = binaryReader.Read7BitEncodedInt32();
                    CVersionListInfoRemote.CResourceInfo[] arrResource = nResourceCount > 0 ? new CVersionListInfoRemote.CResourceInfo[nResourceCount] : CVersionListInfoRemote.ms_EmptyResourceInfo;
                    for (int i = 0; i < nResourceCount; i++)
                    {
                        string szName = binaryReader.ReadEncryptedString(arrEncryptBytes);
                        string szVariant = binaryReader.ReadEncryptedString(arrEncryptBytes);
                        string szExtension = binaryReader.ReadEncryptedString(arrEncryptBytes);
                        byte loadType = binaryReader.ReadByte();
                        int nLen = binaryReader.Read7BitEncodedInt32();
                        int nHash = binaryReader.Read7BitEncodedInt32();
                        int nCompressLen = binaryReader.Read7BitEncodedInt32();
                        int nCompressHash = binaryReader.Read7BitEncodedInt32();    
                        int nAssetIdxCount = binaryReader.Read7BitEncodedInt32();
                        int[] arrAssetIdx = nAssetIdxCount > 0 ? new int[nAssetIdxCount] : ms_emptyArray;
                        for (int j = 0; j < nAssetIdxCount; j++)
                        {
                            arrAssetIdx[j] = binaryReader.Read7BitEncodedInt32();
                        }
                        arrResource[i] = new CVersionListInfoRemote.CResourceInfo(szName, szVariant, szExtension, loadType, nLen, nHash, nCompressLen, nCompressHash, arrAssetIdx);
                    }
                    return new CVersionListInfoRemote(szApplicationGameVerison, nInernalResourceVersion, arrResource, arrAsset);
                }
            }
        }
    }
}
