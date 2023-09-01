
using System;
using System.Collections.Generic;
using System.Linq;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {
        private sealed partial class CResourceLoader
        {
            private class CLoadAssetTaskScene : CLoadAssetTaskBase
            {
                private CLoadAssetCallbacks m_LoadAssetCallbacks;
                public override bool IsScene()
                {
                    return true;
                }

                public override void Clear()
                {
                    base.Clear();
                    m_LoadAssetCallbacks = null;
                }

                public override void OnLoadAssetSuccess(object a_oAsset)
                {
                    base.OnLoadAssetSuccess(a_oAsset);
                    if (m_LoadAssetCallbacks.m_OnLoadAssetSuccess != null)
                    {
                        m_LoadAssetCallbacks.m_OnLoadAssetSuccess.Invoke(m_refAssetInfo.m_szName, a_oAsset, m_oUserData);
                    }
                }
                public override void OnLoadAssetFail(ELoadAssetStatus a_eStatus, string a_szErrMsg)
                {
                    base.OnLoadAssetFail(a_eStatus, a_szErrMsg);
                    if (m_LoadAssetCallbacks.m_OnLoadAssetFail != null)
                    {
                        m_LoadAssetCallbacks.m_OnLoadAssetFail.Invoke(m_refAssetInfo.m_szName, a_eStatus, a_szErrMsg, m_oUserData);
                    }
                }

                public override void OnLoadAssetUpdate(ELoadAssetProgress a_eProgressType, float a_fProgress)
                {
                    base.OnLoadAssetUpdate(a_eProgressType, a_fProgress);
                    if (m_LoadAssetCallbacks.m_OnLoadAssetUpdate != null)
                    {
                        m_LoadAssetCallbacks.m_OnLoadAssetUpdate.Invoke(m_refAssetInfo.m_szName, a_fProgress, m_oUserData);
                    }
                }

                public override void OnLoadDependAsset(string a_szDependAssetName, object a_oDependAsset)
                {
                    base.OnLoadDependAsset(a_szDependAssetName, a_oDependAsset);
                    if (m_LoadAssetCallbacks.m_onLoadDependAssetSuccess != null)
                    {
                        m_LoadAssetCallbacks.m_onLoadDependAssetSuccess.Invoke(m_refAssetInfo.m_szName, a_szDependAssetName, m_listDependAsset.Count, m_refAssetInfo.GetDependAssetName().Length, m_oUserData);
                    }
                }

                public static CLoadAssetTaskScene Create(CResourceInfo a_resInfo, CAssetInfo a_assetInfo, int a_nPriority, CLoadAssetCallbacks a_loadAssetCallback, object a_oUserData)
                {
                    CLoadAssetTaskScene task = CReferencePoolMgr.Acquire<CLoadAssetTaskScene>();
                    task._Init(a_resInfo, a_assetInfo, null, a_nPriority, a_oUserData);
                    task.m_LoadAssetCallbacks = a_loadAssetCallback;
                    return task;
                }

            }
        }
    }
}
