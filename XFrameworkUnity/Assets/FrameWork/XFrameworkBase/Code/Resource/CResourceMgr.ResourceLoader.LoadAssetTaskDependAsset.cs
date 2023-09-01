
using System;
using System.Collections.Generic;
using System.Linq;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {
        private sealed partial class CResourceLoader
        {
            private class CLoadAssetTaskDependAsset : CLoadAssetTaskBase
            {
                private CLoadAssetTaskBase m_mainTask;
                public override bool IsScene()
                {
                    return false;
                }

                public override void Clear()
                {
                    base.Clear();
                    m_mainTask = null;
                }

                public override void OnLoadAssetSuccess(object a_oAsset)
                {
                    base.OnLoadAssetSuccess(a_oAsset);
                    m_mainTask.OnLoadDependAsset(m_refAssetInfo.m_szName, a_oAsset);
                }
                public override void OnLoadAssetFail(ELoadAssetStatus a_eStatus, string a_szErrMsg)
                {
                    base.OnLoadAssetFail(a_eStatus, a_szErrMsg);
                    m_mainTask.OnLoadAssetFail(ELoadAssetStatus.DependencyError, Utility.Text.Format("Can not Load Depend asset '{0}', interal status '{1}' interal error msg '{2}'", m_refAssetInfo.m_szName, a_eStatus, a_szErrMsg));
                }

                public static CLoadAssetTaskDependAsset Create(CResourceInfo a_resInfo, CAssetInfo a_assetInfo, int a_nPriority, CLoadAssetTaskBase a_mainTask, object a_oUserData)
                {
                    CLoadAssetTaskDependAsset task = CReferencePoolMgr.Acquire<CLoadAssetTaskDependAsset>();
                    task._Init(a_resInfo, a_assetInfo, null, a_nPriority, a_oUserData);
                    task.m_mainTask = a_mainTask;
                    return task;
                }

            }
        }
    }
}
