
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {
        private sealed class CResourceInfoMgr
        {
            private Dictionary<CResourceName, CResourceInfo> m_mapAllResourceInfo;

            public CResourceInfoMgr()
            {
                m_mapAllResourceInfo = new Dictionary<CResourceName, CResourceInfo>();
            }
            public void Clean()
            {
                m_mapAllResourceInfo.Clear();
            }

            public void AddInfo(CResourceName resName, ELoadType a_eLoadType, int nLen, int nHash, int nCompressLen, int nCompressHash, bool bInReadOnlyDir, bool bIsReady)
            {
                Debug.Assert(GetInfo(resName) == null);
                CResourceInfo info = new CResourceInfo(resName, a_eLoadType, nLen, nHash, nCompressLen, nCompressHash, bInReadOnlyDir, bIsReady);
                m_mapAllResourceInfo.Add(resName, info);
            }

            public void MarkResourceReady(CResourceName a_resName)
            {
                var info = GetInfo(a_resName);
                if (null != info)
                {
                    info.MarkReady();
                }
            }
            public CResourceInfo GetInfo(CResourceName a_resName)
            {
                CResourceInfo info = null;
                m_mapAllResourceInfo.TryGetValue(a_resName, out info);
                return info;
            }
        }

        private sealed class CResourceInfo
        {
            public CResourceName m_resName { get; private set; }
            public ELoadType m_eloadType { get; private set; }
            public int m_nLen { get; private set; }
            public int m_nHash { get; private set; }
            public int m_nCompressLen { get; private set; }
            public int m_nCompressHash { get; private set; }
            public bool m_bInReadOnlyDir { get; private set; }
            public bool m_bIsReady { get; private set; }

            public CResourceInfo(CResourceName resName, ELoadType a_eLoadType, int nLen, int nHash, int nCompressLen, int nCompressHash, bool bInReadOnlyDir, bool bIsReady)
            {
                m_resName = resName;
                m_eloadType = a_eLoadType;
                m_nLen = nLen;
                m_nHash = nHash;
                m_nCompressLen = nCompressLen;
                m_nCompressHash = nCompressHash;
                m_bInReadOnlyDir = bInReadOnlyDir;
                m_bIsReady = bIsReady;
            }

            public void MarkReady()
            {
                m_bIsReady = true;
            }
        }
        public enum ELoadType
        {
            LoadFormFile = 0,
            LoadFormMemory,
            LoadFormBinary,
        }
        private sealed class CResourceName : IComparable<CResourceName>, IEquatable<CResourceName>
        {
            private static Dictionary<CResourceName, string> ms_mapAllFullName = new Dictionary<CResourceName, string>();
            public string m_szName { get; private set; }
            public string m_szExtension { get; private set; }
            public string m_szVariant { get; private set; }

            public CResourceName(string a_szName, string a_szExtenion, string a_szVariant)
            {
                m_szName = a_szName;
                m_szExtension = a_szExtenion;
                m_szVariant = string.IsNullOrEmpty(a_szVariant) ? string.Empty : a_szVariant;
            }

            public string FullName
            {
                get
                {
                    string szFullName = string.Empty;
                    if (!ms_mapAllFullName.TryGetValue(this, out szFullName))
                    {
                        if (string.IsNullOrEmpty(m_szVariant))
                        {
                            szFullName = Utility.Text.Format("{0}.{1}", m_szName, m_szExtension);
                        }
                        else
                        {
                            szFullName = Utility.Text.Format("{0}.{1}.{2}", m_szName, m_szExtension, m_szVariant);
                        }
                        ms_mapAllFullName.Add(this, szFullName);
                    }
                    return szFullName;
                }
            }

            public int CompareTo(CResourceName other)
            {
                int nR = string.CompareOrdinal(m_szName, other.m_szName);
                if (nR != 0)
                {
                    return nR;
                }
                nR = string.CompareOrdinal(m_szExtension, other.m_szExtension);
                if (nR != 0)
                {
                    return nR;
                }
                nR = string.CompareOrdinal(m_szVariant, other.m_szVariant);
                return nR;
            }

            public bool Equals(CResourceName other)
            {
                return string.Equals(m_szName, other.m_szName, StringComparison.Ordinal)
                    && string.Equals(m_szExtension, other.m_szExtension, StringComparison.Ordinal)
                    && string.Equals(m_szVariant, other.m_szVariant, StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                if (obj is CResourceName)
                {
                    return Equals((CResourceName)obj);
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                if (string.IsNullOrEmpty(m_szVariant))
                {
                    return m_szName.GetHashCode() ^ m_szExtension.GetHashCode();
                }
                else
                {
                    return m_szName.GetHashCode() ^ m_szExtension.GetHashCode() ^ m_szVariant.GetHashCode();
                }
            }

            public static bool operator ==(CResourceName a, CResourceName b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(CResourceName a, CResourceName b)
            {
                return !a.Equals(b);
            }

        }
    }
}
