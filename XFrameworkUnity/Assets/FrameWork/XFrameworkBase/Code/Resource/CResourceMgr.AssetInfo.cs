
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {

        private sealed class CAssetInfoMgr
        {
            private Dictionary<string, CAssetInfo> m_mapAllAssetInfo;
            public CAssetInfoMgr()
            {
                m_mapAllAssetInfo = new Dictionary<string, CAssetInfo>();
            }

            public void Clean()
            {
                m_mapAllAssetInfo.Clear();
            }

            public void AddAssetInfo(string a_szName, CResourceName a_resName, string[] arrDependAssetName)
            {
                Debug.Assert(GetInfo(a_szName) == null);
                CAssetInfo info = new CAssetInfo(a_szName, a_resName, arrDependAssetName);
                m_mapAllAssetInfo.Add(a_szName, info);
            }
            public CAssetInfo GetInfo(string a_szName)
            {
                CAssetInfo info = null;
                m_mapAllAssetInfo.TryGetValue(a_szName, out info);
                return info;
            }
        }
        private sealed class CAssetInfo
        {
            public string m_szName { get; private set; }
            public CResourceName m_resName { get; private set; }

            private string[] m_arrDependAssetName;

            public CAssetInfo(string szName, CResourceName resName, string[] arrDependAssetName)
            {
                m_szName = szName;
                m_resName = resName;
                m_arrDependAssetName = arrDependAssetName;
            }

            public string[] GetDependAssetName()
            {
                return m_arrDependAssetName;
            }
        }
    }
}
