
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {
        private sealed partial class CResourceLoader
        {
            private sealed class CLoadAssetAgent : ITaskAgent<CLoadAssetTaskBase>
            {
                private static readonly HashSet<string> ms_setLoadingAssetNames = new HashSet<string>(StringComparer.Ordinal);
                private static readonly HashSet<string> ms_setLoadingResourceNames = new HashSet<string>(StringComparer.Ordinal);

                private static readonly Dictionary<string, string> ms_mapResourceFullPath = new Dictionary<string, string>();
                public static void Clear()
                {
                    ms_setLoadingAssetNames.Clear();
                    ms_setLoadingResourceNames.Clear();
                    ms_mapResourceFullPath.Clear();
                }

                private readonly ILoadAssetAgentHelper m_refHelper;
                private readonly IResourceHelper m_refResourceHelper;
                private readonly CResourceLoader m_refLoader;
                private readonly string m_szReadOnlyPath;
                private readonly string m_szReadWritePath;
                private CLoadAssetTaskBase m_Task;

                public CLoadAssetTaskBase Task => m_Task;

                public CLoadAssetAgent(ILoadAssetAgentHelper refHelper, IResourceHelper refResourceHelper, CResourceLoader refLoader, string szReadOnlyPath, string szReadWritePath)
                {
                    m_refHelper = refHelper;
                    m_refResourceHelper = refResourceHelper;
                    m_refLoader = refLoader;
                    m_szReadOnlyPath = szReadOnlyPath;
                    m_szReadWritePath = szReadWritePath;
                    m_Task = null;
                }


                public void Init()
                {
                    m_refHelper.EventLoadUpdate += _OnAgentHelperUpdate;
                    m_refHelper.EventLoadAssetComplete += _OnAgentHelperLoadAssetComplete;
                    m_refHelper.EventLoadAssetError += _OnAgentHelperLoadAssetError;
                    m_refHelper.EventReadFileComplete += _OnAgentHelperReadFileComplete;
                    m_refHelper.EventReadBytesComplete += _OnAgentHelperReadBytesComplete;
                    m_refHelper.EventParseBytesComplete += _OnAgentHelperParseBytesComplete;
                }

                public void ShutDown()
                {
                    Reset();
                    m_refHelper.EventLoadUpdate -= _OnAgentHelperUpdate;
                    m_refHelper.EventLoadAssetComplete -= _OnAgentHelperLoadAssetComplete;
                    m_refHelper.EventLoadAssetError -= _OnAgentHelperLoadAssetError;
                    m_refHelper.EventReadFileComplete -= _OnAgentHelperReadFileComplete; 
                    m_refHelper.EventReadBytesComplete -= _OnAgentHelperReadBytesComplete;  
                    m_refHelper.EventParseBytesComplete -= _OnAgentHelperParseBytesComplete;
                }
                public void Update(float a_fElapseSed, float a_fRealElapseSed)
                {
                }

                public void Start(CTaskBase task)
                {
                    Debug.Assert(task != null);
                    Debug.Assert(task is CLoadAssetTaskBase);

                    m_Task = task as CLoadAssetTaskBase;
                    CResourceInfo resInfo = m_Task.GetResourceInfo();
                    if (!resInfo.m_bIsReady)
                    {
                        return;
                    }
                    if (ms_setLoadingAssetNames.Contains(m_Task.GetAssetInfo().m_szName))
                    {
                        return;
                    }
                    if (!m_Task.IsScene())
                    {
                        CAssetObject assetObj = m_refLoader.m_assetObjMgr.GetAssetPool().Spawn(m_Task.GetAssetInfo().m_szName);
                        if (assetObj != null)
                        {
                            _OnAssetObjReady(assetObj);
                            return;
                        }
                    }

                    string[] listDependAssetName = m_Task.GetAssetInfo().GetDependAssetName();
                    foreach (string name in listDependAssetName)
                    {
                        if (!m_refLoader.m_assetObjMgr.GetAssetPool().CanSpawn(name))
                        {
                            return;
                        }
                    }

                    string szResourceName = resInfo.m_resName.m_szName;
                    if (ms_setLoadingResourceNames.Contains(szResourceName))
                    {
                        return;
                    }

                    ms_setLoadingAssetNames.Add(m_Task.GetAssetInfo().m_szName);
                    CResourceObject resObj = m_refLoader.m_resourceObjMgr.GetResPool().Spawn(szResourceName);
                    if (resObj != null)
                    {
                        _OnResourceObjReady(resObj);
                        return;
                    }

                    ms_setLoadingResourceNames.Add(szResourceName);

                    string szResourceFullPath = null;
                    if (!ms_mapResourceFullPath.TryGetValue(szResourceName, out szResourceFullPath))
                    {
                        szResourceFullPath = Utility.Path.GetRegularPath(Path.Combine(resInfo.m_bInReadOnlyDir ? m_szReadOnlyPath : m_szReadWritePath, resInfo.m_resName.FullName));

                        ms_mapResourceFullPath.Add(szResourceName, szResourceFullPath);
                    }

                    if (resInfo.m_eloadType == ELoadType.LoadFormFile)
                    {
                        m_refHelper.ReadFile(szResourceFullPath);
                    }
                    else if (resInfo.m_eloadType == ELoadType.LoadFormMemory)
                    {
                        m_refHelper.ReadBytes(szResourceFullPath);
                    }

                }

                public void Reset()
                {
                    m_refHelper.Reset();
                    m_Task = null;
                }

                private void _OnResourceObjReady(CResourceObject a_oResourceObj)
                {
                    m_Task.SetResourceObj(a_oResourceObj);
                    m_refHelper.LoadAsset(a_oResourceObj, m_Task.GetAssetInfo().m_szName, m_Task.GetAssetType(), m_Task.IsScene());
                }
                private void _OnAssetObjReady(CAssetObject a_oAssetObj)
                {
                    m_refHelper.Reset();

                    object asset = a_oAssetObj.Target;
                    if (m_Task.IsScene())
                    {

                    }
                    m_Task.OnLoadAssetSuccess(asset);
                    m_Task.m_bDone = true;
                }

                private void _OnAgentHelperUpdate(CEventLoadAssetAgentHelperUpdate a_oAgentHelperUpdate)
                {
                    m_Task.OnLoadAssetUpdate(a_oAgentHelperUpdate.ProgressType, a_oAgentHelperUpdate.Progress);
                }

                private void _OnAgentHelperReadFileComplete(CEventLoadAssetAgentHelperReadFileComplete a_oAgentHelperReadFileComplete)
                {
                    _OnLoadResObjComplete(a_oAgentHelperReadFileComplete.Resource);
                }

                private void _OnAgentHelperReadBytesComplete(CEventLoadAssetAgentHelperReadBytesComplete a_oAgentHelperReadBytesComplete)
                {
                    byte[] bytes = a_oAgentHelperReadBytesComplete.GetBytes();
                    m_refHelper.ParseBytes(bytes);
                }

                private void _OnAgentHelperParseBytesComplete(CEventLoadAssetAgentHelperParseBytesComplete a_oAgentHelperParseBytesComplete)
                {
                    _OnLoadResObjComplete(a_oAgentHelperParseBytesComplete.Resource);
                }

                private void _OnLoadResObjComplete(object a_oRes)
                {
                    string szResName = m_Task.GetResourceInfo().m_resName.m_szName;
                    CResourceObject resObj = CResourceObject.Create(szResName, a_oRes, m_refLoader.m_resourceObjMgr, m_refResourceHelper);
                    m_refLoader.m_resourceObjMgr.GetResPool().Register(resObj, true);
                    ms_setLoadingResourceNames.Remove(szResName);
                    _OnResourceObjReady(resObj);
                }

                private void _OnAgentHelperLoadAssetComplete(CEventLoadAssetAgentHelperLoadAssetComplete a_oAgentHelperLoadAssetComplete)
                {
                    CAssetObject assetObj = null;
                    string szAssetName = m_Task.GetAssetInfo().m_szName;
                    if (m_Task.IsScene())
                    {
                        assetObj = m_refLoader.m_assetObjMgr.GetAssetPool().Spawn(szAssetName);
                    }

                    if (assetObj == null)
                    {
                        List<object> listDependAsset = m_Task.GetLoadedDependAssets();
                        assetObj = m_refLoader.m_assetObjMgr.CreateAsset(szAssetName, a_oAgentHelperLoadAssetComplete.Asset, listDependAsset, m_Task.GetResourceObj());
                    }
                    ms_setLoadingAssetNames.Remove(szAssetName);
                    _OnAssetObjReady(assetObj);
                }

                private void _OnAgentHelperLoadAssetError(CEventLoadAssetAgentHelperLoadAssetError a_oAgentHelperLoadAssetError)
                {
                    m_refHelper.Reset();
                    m_Task.OnLoadAssetFail(a_oAgentHelperLoadAssetError.ELoadAssetStatus, a_oAgentHelperLoadAssetError.Error);
                    ms_setLoadingAssetNames.Remove(m_Task.GetAssetInfo().m_szName);
                    ms_setLoadingResourceNames.Remove(m_Task.GetResourceInfo().m_resName.m_szName);
                    m_Task.m_bDone = true;
                }
            }
        }
    }
}
