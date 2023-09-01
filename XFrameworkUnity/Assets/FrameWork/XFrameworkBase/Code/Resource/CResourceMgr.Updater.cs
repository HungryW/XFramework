using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {
        private sealed partial class CResourceUpdater
        {
            private sealed class CUpdateInfo
            {
                public readonly CResourceName m_resName;
                public readonly ELoadType m_eLoadType;
                public readonly int m_nLen;
                public readonly int m_nHash;
                public readonly int m_nCompressLen;
                public readonly int m_nCompressHash;
                public readonly string m_szResourcePath;
                public bool m_bDownloading;
                public int m_nRetryCount;

                public CUpdateInfo(CResourceName resName, ELoadType eLoadType, int nLen, int nHash, int nCompressLen, int nCompressHash, string szResourcePath)
                {
                    m_resName = resName;
                    m_eLoadType = eLoadType;
                    m_nLen = nLen;
                    m_nHash = nHash;
                    m_nCompressLen = nCompressLen;
                    m_nCompressHash = nCompressHash;
                    m_szResourcePath = szResourcePath;
                    m_bDownloading = false;
                    m_nRetryCount = 0;
                }
            }
        }

        private sealed partial class CResourceUpdater
        {

        }
    }
}
