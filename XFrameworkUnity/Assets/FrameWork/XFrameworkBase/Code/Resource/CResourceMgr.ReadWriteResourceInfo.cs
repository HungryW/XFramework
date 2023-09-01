
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {
        private sealed class CReadWriteResourceInfoMgr
        {
            private Dictionary<CResourceName, CReadWriteResourceInfo> m_mapAllResourceInfo;

            public CReadWriteResourceInfoMgr()
            {
                m_mapAllResourceInfo = new Dictionary<CResourceName, CReadWriteResourceInfo>();
            }
            public void Clean()
            {
                m_mapAllResourceInfo.Clear();
            }

            public void AddInfo(CResourceName resName, ELoadType a_eLoadType, int nLen, int nHash)
            {
                CReadWriteResourceInfo info = new CReadWriteResourceInfo(a_eLoadType, nLen, nHash);
                m_mapAllResourceInfo.Add(resName, info);
            }
        }

        private sealed class CReadWriteResourceInfo
        {
            public ELoadType m_eloadType { get; private set; }
            public int m_nLen { get; private set; }
            public int m_nHash { get; private set; }

            public CReadWriteResourceInfo(ELoadType a_eLoadType, int nLen, int nHash)
            {
                m_eloadType = a_eLoadType;
                m_nLen = nLen;
                m_nHash = nHash;
            }
        }
    }
}
