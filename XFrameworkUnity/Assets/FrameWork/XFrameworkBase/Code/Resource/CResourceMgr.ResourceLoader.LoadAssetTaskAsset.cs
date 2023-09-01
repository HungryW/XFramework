
using System;
using System.Collections.Generic;
using System.Linq;

namespace XFrameworkBase
{
    public sealed class CLoadAssetCallbacks
    {
        public delegate void FnLoadAssetSuccess(string a_szAssetName, object a_oAsset, object a_oUserData);
        public delegate void FnLoadAssetFailure(string a_szAssetName, ELoadAssetStatus a_eStatus, string a_szError, object a_oUserData);
        public delegate void FnLoadAssetUpdate(string a_szAssetName, float a_fProgress, object a_oUserData);
        public delegate void FnLoadAssetDependAssetSuccess(string a_szAssetName, string a_szDependAssetName, int a_nLoadedNum, int a_nTotalNum, object a_oUserData);

        public readonly FnLoadAssetSuccess m_OnLoadAssetSuccess;
        public readonly FnLoadAssetFailure m_OnLoadAssetFail;
        public readonly FnLoadAssetUpdate m_OnLoadAssetUpdate;
        public readonly FnLoadAssetDependAssetSuccess m_onLoadDependAssetSuccess;

        public CLoadAssetCallbacks(FnLoadAssetSuccess onLoadAssetSuccess, FnLoadAssetFailure onLoadAssetFail, FnLoadAssetUpdate onLoadAssetUpdate, FnLoadAssetDependAssetSuccess onLoadDependAssetSuccess)
        {
            m_OnLoadAssetSuccess = onLoadAssetSuccess;
            m_OnLoadAssetFail = onLoadAssetFail;
            m_OnLoadAssetUpdate = onLoadAssetUpdate;
            m_onLoadDependAssetSuccess = onLoadDependAssetSuccess;
        }

        public CLoadAssetCallbacks(FnLoadAssetSuccess a_fnOnSuccess, FnLoadAssetFailure a_fnOnFail) : this(a_fnOnSuccess, a_fnOnFail, null, null)
        {

        }
    }

    public partial class CResourceMgr : CGameframeworkMoudle
    {
        private sealed partial class CResourceLoader
        {
            private class CLoadAssetTaskAsset : CLoadAssetTaskBase
            {
                private CLoadAssetCallbacks m_LoadAssetCallbacks;
                public override bool IsScene()
                {
                    return false;
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

                public static CLoadAssetTaskAsset Create(CResourceInfo a_resInfo, CAssetInfo a_assetInfo, Type a_assetType, int a_nPriority, CLoadAssetCallbacks a_loadAssetCallback, object a_oUserData)
                {
                    CLoadAssetTaskAsset task = CReferencePoolMgr.Acquire<CLoadAssetTaskAsset>();
                    task._Init(a_resInfo, a_assetInfo, a_assetType, a_nPriority, a_oUserData);
                    task.m_LoadAssetCallbacks = a_loadAssetCallback;
                    return task;
                }

            }
        }
    }
}
