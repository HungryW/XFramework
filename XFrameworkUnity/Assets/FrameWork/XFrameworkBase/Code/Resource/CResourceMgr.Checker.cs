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
        private sealed partial class CResourceChecker
        {
            private readonly CResourceMgr m_resourceMgr;
            private readonly Dictionary<CResourceName, CCheckerInfo> m_mapCheckInfo;
            private string m_szCurVariant;
            private bool m_bRemoteVersionListReady;
            private bool m_bReadOnlyListReady;
            private bool m_bReadWriteListReady;

            public Action<CResourceName, ELoadType, int, int, int, int> m_fnResourceNeedUpdate;
            public Action<int, int, long, long> m_fnResourceCheckComplete;

            public CResourceChecker(CResourceMgr resourceMgr)
            {
                m_resourceMgr = resourceMgr;
                m_mapCheckInfo = new Dictionary<CResourceName, CCheckerInfo>();
                m_szCurVariant = null;
                m_bRemoteVersionListReady = false;
                m_bReadOnlyListReady = false;
                m_bReadWriteListReady = false;
            }

            public void Shutdown()
            {
                m_mapCheckInfo.Clear();
            }

            public void CheckResource(string a_szCurVariant)
            {
                m_szCurVariant = a_szCurVariant;
                m_bReadOnlyListReady = false;
                m_bReadWriteListReady = false;
                m_bRemoteVersionListReady = false;
                m_mapCheckInfo.Clear();

                string szRemoteFilePath = Utility.Path.GetRemotePath(Path.Combine(m_resourceMgr.m_szReadWritePath, mc_szRemoteVersionListFileName));
                m_resourceMgr.m_helper.LoadBytes(szRemoteFilePath, new CLoadBytesCallbacks(_OnRemoteLoadSuccess, _OnRemoteLoadFail), null);

                string szReadOnlyFilePath = Utility.Path.GetRemotePath(Path.Combine(m_resourceMgr.m_szReadOnlyPath, mc_szLocalVersionListFileName));
                m_resourceMgr.m_helper.LoadBytes(szReadOnlyFilePath, new CLoadBytesCallbacks(_OnReadOnlyLoadSuccess, _OnReadOnlyLoadFail), null);

                string szReadWriteFilePath = Utility.Path.GetRemotePath(Path.Combine(m_resourceMgr.m_szReadWritePath, mc_szLocalVersionListFileName));
                m_resourceMgr.m_helper.LoadBytes(szReadWriteFilePath, new CLoadBytesCallbacks(_OnReadWriteLoadSuccess, _OnReadWriteLoadFail), null);
            }
            private void _RefreshCheckInfoStatus()
            {
                if (!m_bReadOnlyListReady || !m_bReadWriteListReady || !m_bRemoteVersionListReady)
                {
                    return;
                }
                int nRemoveCount = 0;
                int nUpdateCount = 0;
                long nUpdateTotalLen = 0;
                long nUpdateTotalCompressLen = 0;

                foreach (var val in m_mapCheckInfo)
                {
                    CCheckerInfo ci = val.Value;
                    ci.RefreshStatus(m_szCurVariant);
                    if (ci.Status == ECheckStatus.StorageInReadOnly)
                    {
                        m_resourceMgr.m_resInfoMgr.AddInfo(ci.m_resourceName, ci.LoadType, ci.Len, ci.Hash, ci.CompressLen, ci.Hash, true, true);
                    }
                    else if (ci.Status == ECheckStatus.StorageInReadWrite)
                    {
                        m_resourceMgr.m_resInfoMgr.AddInfo(ci.m_resourceName, ci.LoadType, ci.Len, ci.Hash, ci.CompressLen, ci.Hash, false, true);
                        m_resourceMgr.m_readWriteResInfoMgr.AddInfo(ci.m_resourceName, ci.LoadType, ci.Len, ci.Hash);
                    }
                    else if (ci.Status == ECheckStatus.Update)
                    {
                        m_resourceMgr.m_resInfoMgr.AddInfo(ci.m_resourceName, ci.LoadType, ci.Len, ci.Hash, ci.CompressLen, ci.Hash, false, false);
                        nUpdateCount++;
                        nUpdateTotalLen += ci.Len;
                        nUpdateTotalCompressLen += ci.CompressLen;
                        if (m_fnResourceNeedUpdate != null)
                        {
                            m_fnResourceNeedUpdate.Invoke(ci.m_resourceName, ci.LoadType, ci.Len, ci.Hash, ci.CompressLen, ci.CompressHash);
                        }
                    }
                    else if (ci.Status == ECheckStatus.Unavailable || ci.Status == ECheckStatus.Disuse)
                    {
                        //DoNothing
                    }
                    else
                    {
                        throw new Exception(Utility.Text.Format("Check resources '{0}' error with unknown status.", ci.m_resourceName.FullName));
                    }
                    if (ci.NeedRemove)
                    {
                        nRemoveCount++;
                        string szResourcePath = Utility.Path.GetRegularPath(Path.Combine(m_resourceMgr.m_szReadWritePath, ci.m_resourceName.FullName));
                        if (File.Exists(szResourcePath))
                        {
                            File.Delete(szResourcePath);
                        }
                    }
                    if (m_fnResourceCheckComplete != null)
                    {
                        m_fnResourceCheckComplete(nRemoveCount, nUpdateCount, nUpdateTotalLen, nUpdateTotalCompressLen);
                    }
                }
            }
            private void _OnRemoteLoadSuccess(string a_szFilePath, byte[] a_arrData, float a_fDuration, object a_oUserData)
            {
                Debug.Assert(!m_bRemoteVersionListReady);
                MemoryStream memoryStream = null;
                try
                {
                    memoryStream = new MemoryStream(a_arrData, false);
                    CVersionListInfoRemote versionList = m_resourceMgr.m_serializerVersionListRemote.Deserialize(memoryStream);
                    CVersionListInfoRemote.CAssetInfo[] arrAsset = versionList.m_arrAsset;
                    CVersionListInfoRemote.CResourceInfo[] arrRes = versionList.m_arrResource;
                    foreach (var res in arrRes)
                    {
                        if (res.m_szVariant != null && res.m_szVariant != m_szCurVariant)
                        {
                            continue;
                        }
                        CResourceName resName = new CResourceName(res.m_szName, res.m_szVariant, res.m_szExtension);
                        int[] arrAssetIdx = res.m_arrAssetIdx;
                        foreach (int idx in arrAssetIdx)
                        {
                            var asset = arrAsset[idx];
                            int[] arrDependIdx = asset.m_arrDependAssetIdx;
                            string[] arrDependName = new string[arrDependIdx.Length];
                            for (int j = 0; j < arrDependIdx.Length; j++)
                            {
                                var dependAsset = arrAsset[arrDependIdx[j]];
                                arrDependName[j] = dependAsset.m_szName;
                            }
                            m_resourceMgr.m_assetInfoMgr.AddAssetInfo(asset.m_szName, resName, arrDependName);
                        }
                        _GetOrAddCheckInfo(resName).SetRemoteVersionInfo((ELoadType)res.m_nLoadType, res.m_nLen, res.m_nHashCode, res.m_nCompressLen, res.m_nCompressHash);
                    }
                    m_bRemoteVersionListReady = true;
                    _RefreshCheckInfoStatus();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    if (memoryStream != null)
                    {
                        memoryStream.Dispose();
                        memoryStream = null;
                    }
                }
            }

            private void _OnRemoteLoadFail(string a_szFilePath, string a_szErrorMsg, object a_oUserData)
            {
                throw new Exception(Utility.Text.Format("Updatable version list '{0}' is invalid, error message is '{1}'.", a_szFilePath, string.IsNullOrEmpty(a_szErrorMsg) ? "<Empty>" : a_szErrorMsg));
            }


            private void _OnReadOnlyLoadSuccess(string a_szFilePath, byte[] a_arrData, float a_fDuration, object a_oUserData)
            {
                Debug.Assert(!m_bReadOnlyListReady);
                MemoryStream memoryStream = null;
                try
                {
                    memoryStream = new MemoryStream(a_arrData, false);
                    CVersionListInfoLocal versionList = m_resourceMgr.m_serializerVersionListLocal.Deserialize(memoryStream);
                    CVersionListInfoLocal.CResourceInfo[] arrRes = versionList.m_arrResource;
                    foreach (var res in arrRes)
                    {
                        CResourceName resName = new CResourceName(res.m_szName, res.m_szExtension, res.m_szVariant);
                        _GetOrAddCheckInfo(resName).SetLocalReadOnlyVersionInfo((ELoadType)res.m_nLoadType, res.m_nLen, res.m_nHashCode);
                    }
                    m_bReadOnlyListReady = true;
                    _RefreshCheckInfoStatus();
                }
                catch (Exception ex)
                {
                    throw new Exception(Utility.Text.Format("Parse read-only version list exception '{0}'.", ex), ex);
                }
                finally
                {
                    if (memoryStream != null)
                    {
                        memoryStream.Dispose();
                        memoryStream = null;
                    }
                }

            }

            private void _OnReadOnlyLoadFail(string a_szFilePath, string a_szErrorMsg, object a_oUserData)
            {
                m_bReadOnlyListReady = true;
                _RefreshCheckInfoStatus();
            }

            private void _OnReadWriteLoadSuccess(string a_szFilePath, byte[] a_arrData, float a_fDuration, object a_oUserData)
            {
                Debug.Assert(!m_bReadWriteListReady);
                MemoryStream memoryStream = null;
                try
                {
                    memoryStream = new MemoryStream(a_arrData, false);
                    CVersionListInfoLocal versionList = m_resourceMgr.m_serializerVersionListLocal.Deserialize(memoryStream);
                    CVersionListInfoLocal.CResourceInfo[] arrRes = versionList.m_arrResource;
                    foreach (var res in arrRes)
                    {
                        CResourceName resName = new CResourceName(res.m_szName, res.m_szExtension, res.m_szVariant);
                        _GetOrAddCheckInfo(resName).SetLocalReadWriteVersionInfo((ELoadType)res.m_nLoadType, res.m_nLen, res.m_nHashCode);
                    }
                    m_bReadWriteListReady = true;
                    _RefreshCheckInfoStatus();
                }
                catch (Exception ex)
                {
                    throw new Exception(Utility.Text.Format("Parse read-only version list exception '{0}'.", ex), ex);
                }
                finally
                {
                    if (memoryStream != null)
                    {
                        memoryStream.Dispose();
                        memoryStream = null;
                    }
                }
            }

            private void _OnReadWriteLoadFail(string a_szFilePath, string a_szErrorMsg, object a_oUserData)
            {
                m_bReadWriteListReady = true;
                _RefreshCheckInfoStatus();
            }

            private CCheckerInfo _GetOrAddCheckInfo(CResourceName a_resName)
            {
                CCheckerInfo info = null;
                if (!m_mapCheckInfo.TryGetValue(a_resName, out info))
                {
                    info = new CCheckerInfo(a_resName);
                    m_mapCheckInfo.Add(a_resName, info);
                }
                return info;
            }
        }
    }
}
