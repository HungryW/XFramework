using System;
using System.Collections.Generic;
using System.Text;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {
        private sealed partial class CResourceChecker
        {
            private sealed partial class CCheckerInfo
            {
                public readonly CResourceName m_resourceName;
                private ECheckStatus m_status;
                private bool m_bNeedRemove;
                private CRemoteVersionInfo m_remoteVersionInfo;
                private CLocalVersionInfo m_localReadOnlyVersionInfo;
                private CLocalVersionInfo m_localReadWriteVersionInfo;

                public CCheckerInfo(CResourceName resourceName)
                {
                    m_resourceName = resourceName;
                    m_status = ECheckStatus.Unknown;
                    m_bNeedRemove = false;
                    m_remoteVersionInfo = default(CRemoteVersionInfo);
                    m_localReadOnlyVersionInfo = default(CLocalVersionInfo);
                    m_localReadWriteVersionInfo = default(CLocalVersionInfo);
                }

                public ECheckStatus Status { get { return m_status; } }
                public bool NeedRemove { get { return m_bNeedRemove; } }
                public ELoadType LoadType { get { return m_remoteVersionInfo.m_eLoadType; } }
                public int Len { get { return m_remoteVersionInfo.m_nLen; } }
                public int Hash { get { return m_remoteVersionInfo.m_nHash; } } 
                public int CompressLen { get { return m_remoteVersionInfo.m_nCompressedLen; } }
                public int CompressHash { get { return m_remoteVersionInfo.m_nCompressedHash; } }

                public void SetRemoteVersionInfo(ELoadType a_eLoadType, int a_nLen, int a_nHash, int a_nCompressLen, int a_nCompressHash)
                {
                    m_remoteVersionInfo = new CRemoteVersionInfo(a_eLoadType, a_nLen, a_nHash, a_nCompressLen, a_nCompressHash);
                }

                public void SetLocalReadOnlyVersionInfo(ELoadType a_eLoadType, int a_nLen, int a_nHash)
                {
                    m_localReadOnlyVersionInfo = new CLocalVersionInfo(a_eLoadType, a_nLen, a_nHash);
                }

                public void SetLocalReadWriteVersionInfo(ELoadType a_eLoadType, int a_nLen, int a_nHash)
                {
                    m_localReadWriteVersionInfo = new CLocalVersionInfo(a_eLoadType, a_nLen, a_nHash);
                }

                public void RefreshStatus(string a_szCurVariant)
                {
                    if (!m_remoteVersionInfo.m_bExit)
                    {
                        m_status = ECheckStatus.Disuse;
                        m_bNeedRemove = m_localReadWriteVersionInfo.m_bExit;
                        return;
                    }
                    if (m_resourceName.m_szVariant == null || m_resourceName.m_szVariant == a_szCurVariant)
                    {
                        if (m_localReadOnlyVersionInfo.m_bExit
                            && m_localReadOnlyVersionInfo.m_eLoadType == m_remoteVersionInfo.m_eLoadType
                            && m_localReadOnlyVersionInfo.m_nLen == m_remoteVersionInfo.m_nLen
                            && m_localReadOnlyVersionInfo.m_nHash == m_remoteVersionInfo.m_nHash
                            )
                        {
                            m_status = ECheckStatus.StorageInReadOnly;
                            m_bNeedRemove = m_localReadWriteVersionInfo.m_bExit;
                        }
                        else if (m_localReadWriteVersionInfo.m_bExit
                            && m_localReadWriteVersionInfo.m_eLoadType == m_remoteVersionInfo.m_eLoadType
                            && m_localReadWriteVersionInfo.m_nLen == m_remoteVersionInfo.m_nLen
                            && m_localReadWriteVersionInfo.m_nHash == m_remoteVersionInfo.m_nHash)
                        {
                            m_status = ECheckStatus.StorageInReadWrite;
                        }
                        else
                        {
                            m_status = ECheckStatus.Update;
                            m_bNeedRemove = m_localReadWriteVersionInfo.m_bExit;
                        }
                    }
                    else
                    {
                        m_status = ECheckStatus.Unavailable;
                        m_bNeedRemove = m_localReadWriteVersionInfo.m_bExit;
                    }
                }
            }

            public enum ECheckStatus : byte
            {
                Unknown = 0,
                StorageInReadOnly,
                StorageInReadWrite,
                Unavailable,
                Update,
                Disuse
            }


            private sealed partial class CCheckerInfo
            {
                private struct CLocalVersionInfo
                {
                    public readonly bool m_bExit;
                    public readonly ELoadType m_eLoadType;
                    public readonly int m_nLen;
                    public readonly int m_nHash;

                    public CLocalVersionInfo(ELoadType a_eLoadType, int a_nLen, int a_nHash)
                    {
                        m_bExit = true;
                        m_eLoadType = a_eLoadType;
                        m_nLen = a_nLen;
                        m_nHash = a_nHash;
                    }
                }

                private struct CRemoteVersionInfo
                {
                    public readonly bool m_bExit;
                    public readonly ELoadType m_eLoadType;
                    public readonly int m_nLen;
                    public readonly int m_nHash;
                    public readonly int m_nCompressedLen;
                    public readonly int m_nCompressedHash;

                    public CRemoteVersionInfo(ELoadType a_eLoadType, int a_nLen, int a_nHash, int a_nCompressedLen, int a_nCompressedHash)
                    {
                        m_bExit = true;
                        m_eLoadType = a_eLoadType;
                        m_nLen = a_nLen;
                        m_nHash = a_nHash;
                        m_nCompressedLen = a_nCompressedLen;
                        m_nCompressedHash = a_nCompressedHash;
                    }

                }
            }
        }
    }
}
