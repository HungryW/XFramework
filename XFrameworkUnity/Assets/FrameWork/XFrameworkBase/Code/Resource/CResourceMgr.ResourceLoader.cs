
using System;
using System.Collections.Generic;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {
        private sealed partial class CResourceLoader
        {
            private CResourceMgr m_refMgr;
            private CResourceObjectMgr m_resourceObjMgr;
            private CAssetObjectMgr m_assetObjMgr;
            private CTaskPool<CLoadAssetTaskBase> m_taskPool;
            private Dictionary<string, object> m_mapSceneObjs;

            public CResourceLoader(CResourceMgr refMgr)
            {
                m_refMgr = refMgr;
                m_resourceObjMgr = new CResourceObjectMgr(this);
                m_assetObjMgr = new CAssetObjectMgr(this);
                m_taskPool = new CTaskPool<CLoadAssetTaskBase>();
                m_mapSceneObjs = new Dictionary<string, object>();
            }

            public void CreateObjPool(IObjectPoolManager a_refPoolMgr)
            {
                m_resourceObjMgr.CreateResPool(a_refPoolMgr);
                m_assetObjMgr.CreateAssetPool(a_refPoolMgr);
            }

            public void AddLoadAssetAgent(ILoadAssetAgentHelper a_helper, IResourceHelper a_resHelper, string a_szReadOnlyPath, string a_szReadWritePath)
            {
                CLoadAssetAgent agent = new CLoadAssetAgent(a_helper, a_resHelper, this, a_szReadOnlyPath, a_szReadWritePath);
                m_taskPool.AddAgent(agent);
            }

            public void Shutdown()
            {
                m_assetObjMgr.Shutdown();
                m_resourceObjMgr.Shutdown();
                m_taskPool.Shutdown();
            }

            public void Update(float a_fElapseSed, float a_fRealElapseSed)
            {
                m_taskPool.Update(a_fElapseSed, a_fRealElapseSed);
            }

            public void LoadAsset(string a_szAssetName, Type a_tAssetType, CLoadAssetCallbacks a_callbacks, int a_nPriority, object a_oUserData)
            {
                CResourceInfo resInfo = null;
                CAssetInfo assetInfo = null;
                if (!_CheckAsset(a_szAssetName, out assetInfo, out resInfo) || resInfo == null || assetInfo == null)
                {
                    string szErrorMsg = Utility.Text.Format("Can not load asset '{0}'", a_szAssetName);
                    if (a_callbacks.m_OnLoadAssetFail != null)
                    {
                        ELoadAssetStatus eLoadAssetStatus = (resInfo != null && !resInfo.m_bIsReady) ? ELoadAssetStatus.NotReady : ELoadAssetStatus.NotExist;
                        a_callbacks.m_OnLoadAssetFail(a_szAssetName, eLoadAssetStatus, szErrorMsg, a_oUserData);
                    }
                    return;
                }

                CLoadAssetTaskAsset mainTask = CLoadAssetTaskAsset.Create(resInfo, assetInfo, a_tAssetType, a_nPriority, a_callbacks, a_oUserData);

                string[] arrDependAssetName = assetInfo.GetDependAssetName();
                foreach (string szDependName in arrDependAssetName)
                {
                    if (!_LoadDependAsset(szDependName, mainTask, a_nPriority, a_oUserData))
                    {
                        string szErrorMsg = Utility.Text.Format("Can not load Depend asset '{0} when load Asset '{1}'", szDependName, a_szAssetName);
                        if (a_callbacks.m_OnLoadAssetFail != null)
                        {
                            a_callbacks.m_OnLoadAssetFail(a_szAssetName, ELoadAssetStatus.DependencyError, szErrorMsg, a_oUserData);
                        }
                        return;
                    }
                }

                m_taskPool.AddTask(mainTask);
                if (!resInfo.m_bIsReady)
                {
                    m_refMgr._UpdateResource(resInfo.m_resName);
                }
            }

            public void UnloadAsset(object a_Asset)
            {
                m_assetObjMgr.GetAssetPool().Unspawn(a_Asset);
            }

            public void LoadScene(string a_szSceneName, int a_nPriority, CLoadAssetCallbacks a_callbacks, object a_oUserData)
            {
                CResourceInfo resInfo = null;
                CAssetInfo assetInfo = null;
                if (!_CheckAsset(a_szSceneName, out assetInfo, out resInfo))
                {
                    string szErrorMsg = Utility.Text.Format("Can not load Scene '{0}'", a_szSceneName);
                    if (a_callbacks.m_OnLoadAssetFail != null)
                    {
                        ELoadAssetStatus eLoadAssetStatus = (resInfo != null && !resInfo.m_bIsReady) ? ELoadAssetStatus.NotReady : ELoadAssetStatus.NotExist;
                        a_callbacks.m_OnLoadAssetFail(a_szSceneName, eLoadAssetStatus, szErrorMsg, a_oUserData);
                    }
                    return;
                }

                CLoadAssetTaskScene mainTask = CLoadAssetTaskScene.Create(resInfo, assetInfo, a_nPriority, a_callbacks, a_oUserData);
                string[] arrDependAssetName = assetInfo.GetDependAssetName();
                foreach (string szName in arrDependAssetName)
                {
                    if (!_LoadDependAsset(szName, mainTask, a_nPriority, a_oUserData))
                    {
                        string szErrorMsg = Utility.Text.Format("Can not load Depend asset '{0} when load Scene '{1}'", szName, a_szSceneName);
                        if (a_callbacks.m_OnLoadAssetFail != null)
                        {
                            a_callbacks.m_OnLoadAssetFail(a_szSceneName, ELoadAssetStatus.DependencyError, szErrorMsg, a_oUserData);
                        }
                        return;
                    }
                }
                m_taskPool.AddTask(mainTask);
                if (!resInfo.m_bIsReady)
                {
                    m_refMgr._UpdateResource(resInfo.m_resName);
                }
            }

            public void UnloadScene(string a_szSceneName, object a_oUserData)
            {
                object sceneAsset = null;
                if (m_mapSceneObjs.TryGetValue(a_szSceneName, out sceneAsset))
                {
                    m_mapSceneObjs.Remove(a_szSceneName);
                    m_assetObjMgr.GetAssetPool().Unspawn(sceneAsset);
                    m_assetObjMgr.GetAssetPool().ReleaseObject(sceneAsset);
                }

                m_refMgr.m_helper.UnloadScene(a_szSceneName, a_oUserData);
            }
            private bool _LoadDependAsset(string a_szAssetName, CLoadAssetTaskBase a_mainTask, int a_nPriority, object a_oUserData)
            {
                CResourceInfo resInfo = null;
                CAssetInfo assetInfo = null;
                if (!_CheckAsset(a_szAssetName, out assetInfo, out resInfo))
                {
                    return false;
                }
                CLoadAssetTaskDependAsset task = CLoadAssetTaskDependAsset.Create(resInfo, assetInfo, a_nPriority, a_mainTask, a_oUserData);

                string[] arrDependAssetName = assetInfo.GetDependAssetName();
                foreach (string szAssetName in arrDependAssetName)
                {
                    if (!_LoadDependAsset(szAssetName, task, a_nPriority, a_oUserData))
                    {
                        return false;
                    }
                }
                m_taskPool.AddTask(task);
                if (!resInfo.m_bIsReady)
                {
                    m_refMgr._UpdateResource(resInfo.m_resName);
                }
                return true;
            }

            private bool _CheckAsset(string a_szAssetName, out CAssetInfo a_outAssetInfo, out CResourceInfo a_outResourceInfo)

            {
                a_outAssetInfo = null;
                a_outResourceInfo = null;
                if (string.IsNullOrEmpty(a_szAssetName))
                {
                    return false;
                }
                a_outAssetInfo = m_refMgr.m_assetInfoMgr.GetInfo(a_szAssetName);
                if (null == a_outAssetInfo)
                {
                    return false;
                }
                a_outResourceInfo = m_refMgr.m_resInfoMgr.GetInfo(a_outAssetInfo.m_resName);
                if (a_outResourceInfo == null)
                {
                    return false;
                }
                return m_refMgr.m_eMode == EResourceMode.UpdatableWhilePlaying ? true : a_outResourceInfo.m_bIsReady;
            }
        }
    }
}
