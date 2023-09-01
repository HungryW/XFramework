
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
                private static int ms_nIdSeed = 0;
                protected CResourceInfo m_refResourceInfo;
                protected CAssetInfo m_refAssetInfo;
                protected CResourceObject m_refResourceObj;
                protected CAssetObject m_AssetObj;
                protected List<object> m_listDependAsset;
                protected Type m_tAssetType;

                public CLoadAssetTaskBase()
                {
                    m_listDependAsset = new List<object>();
                }
                public override void Clear()
                {
                    base.Clear();
                    m_refResourceInfo = null;
                    m_refResourceObj = null;
                    m_AssetObj = null;
                    m_refAssetInfo = null;
                    m_tAssetType = null;
                    m_listDependAsset.Clear();
                }

                protected void _Init(CResourceInfo a_refResoureInfo, CAssetInfo a_refAssetInfo, Type a_tAssetType, int a_nPriority, object a_oUserData)
                {
                    Init(ms_nIdSeed++, a_nPriority, a_oUserData);
                    m_refResourceInfo = a_refResoureInfo;
                    m_refAssetInfo = a_refAssetInfo;
                    m_tAssetType = a_tAssetType;
                }

                public virtual bool IsScene() { return false; }

                public void SetResourceObj(CResourceObject a_resObj)
                {
                    m_refResourceObj = a_resObj;
                }

                public void AddLoadedDependAssets(object a_oDependAsset)
                {
                    m_listDependAsset.Add(a_oDependAsset);
                }

                public CResourceObject GetResourceObj() { return m_refResourceObj; }
                public CAssetInfo GetAssetInfo() { return m_refAssetInfo; }
                public CResourceInfo GetResourceInfo() { return m_refResourceInfo; }
                public List<object> GetLoadedDependAssets() { return m_listDependAsset; }

                public Type GetAssetType() { return m_tAssetType; }

                public virtual void OnLoadAssetSuccess(object a_oAsset)
                {

                }

                public virtual void OnLoadAssetUpdate(ELoadAssetProgress a_eProgressType, float a_fProgress)
                {

                }

                public virtual void OnLoadDependAsset(string a_szDependAssetName, object a_oDependAsset)
                {
                    m_listDependAsset.Add(a_oDependAsset);
                }

                public virtual void OnLoadAssetFail(ELoadAssetStatus a_eStatus, string a_szErrMsg)
                {

                }
            }
        }
    }
}
