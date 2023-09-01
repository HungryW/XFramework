
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;

namespace XFrameworkBase
{

    public partial class CResourceMgr : CGameframeworkMoudle
    {
        public sealed class CResourceIniter
        {
            private readonly CResourceMgr m_resourceMgr;
            private string m_szCurVariant;
            private Action m_fnOnInitComplete;

            public CResourceIniter(CResourceMgr refMgr)
            {
                m_resourceMgr = refMgr;
                m_szCurVariant = null;
                m_fnOnInitComplete = null;
            }

            public void Shutdown()
            {

            }

            public void InitResources(string a_szCurVariant, Action a_fnOnInitSuccess)
            {
                m_szCurVariant = a_szCurVariant;
                m_fnOnInitComplete = a_fnOnInitSuccess;
                string szFilePath = Utility.Path.GetRemotePath(Path.Combine(m_resourceMgr.m_szReadOnlyPath, mc_szPackageVersionListFileName));
                m_resourceMgr.m_helper.LoadBytes(szFilePath, new CLoadBytesCallbacks(_OnLoadPackageVersionListSuccess, _OnLoadPackageVersionListFail), null);
            }

            private void _OnLoadPackageVersionListSuccess(string a_szFileUrl, byte[] a_arrData, float a_fDuration, object a_oUserData)
            {
                MemoryStream memoryStream = null;
                try
                {
                    memoryStream = new MemoryStream(a_arrData, false);
                    CVersionListInfoPackage versionList = m_resourceMgr.m_serializerVersionListPackage.Deserialize(memoryStream);

                    CVersionListInfoPackage.CAssetInfo[] arrAsset = versionList.m_arrAsset;
                    CVersionListInfoPackage.CResourceInfo[] arrRes = versionList.m_arrResource;
                    foreach (var res in arrRes)
                    {
                        if (!string.IsNullOrEmpty(res.m_szVariant) && res.m_szVariant != m_szCurVariant)
                        {
                            continue;
                        }
                        CResourceName resName = new CResourceName(res.m_szName, res.m_szExtension, res.m_szVariant);
                        int[] arrAssetIdx = res.m_arrAssetIdx;
                        foreach (var nIdx in arrAssetIdx)
                        {
                            var assetInfo = arrAsset[nIdx];
                            int[] arrDependAssetIdx = assetInfo.m_arrDependAssetIdx;
                            string[] arrDependAssetName = new string[arrDependAssetIdx.Length];
                            for (int i = 0; i < arrDependAssetName.Length; i++)
                            {
                                arrDependAssetName[i] = arrAsset[arrDependAssetIdx[i]].m_szName;
                            }
                            m_resourceMgr.m_assetInfoMgr.AddAssetInfo(assetInfo.m_szName, resName, arrDependAssetName);
                        }
                        m_resourceMgr.m_resInfoMgr.AddInfo(resName, (ELoadType)res.m_nLoadType, res.m_nLen, res.m_nHashCode, res.m_nLen, res.m_nHashCode, true, true);
                    }
                    m_fnOnInitComplete?.Invoke();
                }
                catch (Exception ex)
                {
                    throw new Exception(Utility.Text.Format("Parse package version list exception '{0}'.", ex), ex);
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

            private void _OnLoadPackageVersionListFail(string a_szFileUrl, string a_szErrorMsg, object a_oUserData)
            {
                throw new Exception(Utility.Text.Format("Package version list '{0}' is invalid, error message is '{1}'.", a_szFileUrl, string.IsNullOrEmpty(a_szErrorMsg) ? "<Empty>" : a_szErrorMsg));
            }
        }
    }

    public sealed class CLoadBytesCallbacks
    {
        public delegate void FnLoadBytesSuccessCallback(string fileUri, byte[] bytes, float duration, object a_oUserData);
        public delegate void FnLoadBytesFailureCallback(string fileUri, string errorMsg, object a_oUserData);

        public readonly FnLoadBytesSuccessCallback m_fnOnLoadSuccess;
        public readonly FnLoadBytesFailureCallback m_fnOnLoadFailure;

        public CLoadBytesCallbacks(FnLoadBytesSuccessCallback a_fnSuccess, FnLoadBytesFailureCallback a_fnFailure)
        {
            m_fnOnLoadSuccess = a_fnSuccess;
            m_fnOnLoadFailure = a_fnFailure;
        }
    }


}
