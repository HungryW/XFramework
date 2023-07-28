
using System;
using System.Collections.Generic;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {
        private sealed partial class CResourceLoader
        {
            private class CLoadAssetTaskBase : CTaskBase
            {
                private CResourceObject m_refResourceObj;
                private CAssetObject m_AssetObj;
                private CAssetInfo m_refAssetInfo;
                private List<object> m_listDependAsset;

                public CLoadAssetTaskBase()
                {
                    m_listDependAsset = new List<object>();
                }
                public override void Clear()
                {
                    base.Clear();
                    m_refResourceObj = null;
                    m_AssetObj = null;
                    m_refAssetInfo = null;
                    m_listDependAsset.Clear();
                }
            }
        }
    }
}
