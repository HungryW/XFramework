
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {
        public static int[] ms_emptyArray = new int[] { };

        private sealed class CVersionListInfoPackage
        {
            public readonly int m_nGameVersion;
            public readonly int m_nResVersion;
            public readonly CAssetInfo[] m_arrAsset;
            public readonly CResourceInfo[] m_arrResource;

            public CVersionListInfoPackage(int a_nGameVersion, int a_nResVersion, CResourceInfo[] a_arrResInfo, CAssetInfo[] a_arrAssetInfo)
            {
                m_nGameVersion = a_nGameVersion;
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
                public readonly int[] m_arrAssetIdx;

                public CResourceInfo(string szName, string szVariant, string szExtension, byte nLoadType, int nLen, int nHashCode, int[] arrAssetIdx)
                {
                    m_szName = szName;
                    m_szVariant = szVariant;
                    m_szExtension = szExtension;
                    m_nLoadType = nLoadType;
                    m_nLen = nLen;
                    m_nHashCode = nHashCode;
                    m_arrAssetIdx = arrAssetIdx;
                }
            }


        }
    }
}
