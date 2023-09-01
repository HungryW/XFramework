
using System;
using System.Collections.Generic;
using System.IO;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {
        private sealed partial class CReadWriteVersionListChecker
        {
            private const int mc_nCheckLenPerFrame = 1024 * 1024;

            private readonly CResourceMgr m_resourceMgr;
            private List<CCheckResourceInfo> m_listCheckInfos;
            private bool m_bLoadVersionFileSuccess;
            private int m_nCurChecIndex;
            private bool m_bFailFlag;

            public Action<bool> m_fnOnCheckVersionListComplete;

            public CReadWriteVersionListChecker(CResourceMgr a_resMgr)
            {
                m_resourceMgr = a_resMgr;
                m_listCheckInfos = new List<CCheckResourceInfo>();
                m_bLoadVersionFileSuccess = false;
                m_nCurChecIndex = 0;
                m_bFailFlag = false;


                m_fnOnCheckVersionListComplete = null;
            }
            public void Shutdown()
            {
                m_listCheckInfos.Clear();
                m_bLoadVersionFileSuccess = false;
                m_nCurChecIndex = 0;
                m_bFailFlag = false;
            }

            public void CheckVersionList()
            {
                m_bLoadVersionFileSuccess = false;
                m_nCurChecIndex = 0;
                m_bFailFlag = false;
                string szVersionListPath = Utility.Path.GetRemotePath(Path.Combine(m_resourceMgr.m_szReadWritePath, mc_szLocalVersionListFileName));
                m_resourceMgr.m_helper.LoadBytes(szVersionListPath, new CLoadBytesCallbacks(_OnLoadVersionFileSuccess, _OnLoadVersionFileFail), null);
            }

            private void _OnLoadVersionFileSuccess(string a_szFilePath, byte[] a_arrData, float a_fDuration, object a_oUserData)
            {
                MemoryStream memoryStream = null;
                try
                {
                    memoryStream = new MemoryStream(a_arrData, false);
                    CVersionListInfoLocal versionList = m_resourceMgr.m_serializerVersionListLocal.Deserialize(memoryStream);
                    CVersionListInfoLocal.CResourceInfo[] arrRes = versionList.m_arrResource;

                    long nTotalLen = 0L;
                    foreach (var res in arrRes)
                    {

                        CResourceName resName = new CResourceName(res.m_szName, res.m_szExtension, res.m_szVariant);
                        nTotalLen += res.m_nLen;
                        m_listCheckInfos.Add(new CCheckResourceInfo(resName, (ELoadType)res.m_nLoadType, res.m_nLen, res.m_nHashCode));
                    }
                    m_bLoadVersionFileSuccess = true;
                }
                catch (Exception e)
                {
                    throw new Exception(Utility.Text.Format("Load version file fail, error message: {0}", e.Message));
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

            private void _OnLoadVersionFileFail(string a_szFilePath, string a_szErrorMsg, object a_oUserData)
            {
                if (m_fnOnCheckVersionListComplete != null)
                {
                    m_fnOnCheckVersionListComplete(true);
                }

            }

            public void Update(float elapseSed, float realElapseSed)
            {
                if (!m_bLoadVersionFileSuccess)
                {
                    return;
                }
                int nLen = 0;
                while (m_nCurChecIndex < m_listCheckInfos.Count)
                {
                    CCheckResourceInfo info = m_listCheckInfos[m_nCurChecIndex];
                    nLen += info.m_nLen;

                    if (_CheckResource(info))
                    {
                        m_nCurChecIndex++;
                    }
                    else
                    {
                        m_bFailFlag = true;
                        m_listCheckInfos.RemoveAt(m_nCurChecIndex);
                    }

                    if (nLen > mc_nCheckLenPerFrame)
                    {
                        return;
                    }
                }

                m_bLoadVersionFileSuccess = false;
                if (m_bFailFlag)
                {
                    _GenerateRealVersionFile();
                }

                if (m_fnOnCheckVersionListComplete != null)
                {
                    m_fnOnCheckVersionListComplete(m_bFailFlag);
                }

            }

            private bool _CheckResource(CCheckResourceInfo a_info)
            {
                string szResourcePath = Utility.Path.GetRegularPath(Path.Combine(m_resourceMgr.m_szReadWritePath, a_info.m_resName.FullName));
                if (!File.Exists(szResourcePath))
                {
                    return false;
                }
                using (FileStream fileStream = new FileStream(szResourcePath, FileMode.Open, FileAccess.Read))
                {
                    int nHashCode = Utility.Verifier.GetCrc32(fileStream);
                    if (nHashCode == a_info.m_nHash && fileStream.Length == a_info.m_nLen)
                    {
                        return true;
                    }
                    File.Delete(szResourcePath);
                    return false;
                }
            }

            private void _GenerateRealVersionFile()
            {
                if (m_listCheckInfos.Count == 0)
                {
                    return;
                }
                string szVersionListPath = Utility.Path.GetRegularPath(Path.Combine(m_resourceMgr.m_szReadWritePath, mc_szLocalVersionListFileName));
                string szTempVersionListPath = Utility.Text.Format("{0}.tmp", szVersionListPath);
                FileStream fileStream = null;
                try
                {
                    fileStream = new FileStream(szTempVersionListPath, FileMode.Create, FileAccess.Write);
                    CVersionListInfoLocal.CResourceInfo[] arrRes = new CVersionListInfoLocal.CResourceInfo[m_listCheckInfos.Count];
                    for (int i = 0; i < arrRes.Length; i++)
                    {
                        var checkInfo = m_listCheckInfos[i];
                        arrRes[i] = new CVersionListInfoLocal.CResourceInfo(checkInfo.m_resName.m_szName, checkInfo.m_resName.m_szVariant, checkInfo.m_resName.m_szExtension, (byte)checkInfo.m_eLoadType, checkInfo.m_nLen, checkInfo.m_nHash);
                    }
                    CVersionListInfoLocal versionInfo = new CVersionListInfoLocal(arrRes);
                    m_resourceMgr.m_serializerVersionListLocal.Serialize(fileStream, versionInfo);
                }
                catch (Exception ex)
                {
                    if (File.Exists(szTempVersionListPath))
                    {
                        File.Delete(szTempVersionListPath);
                    }
                    throw new Exception(Utility.Text.Format("Generate read-write version list fail '{0}'", ex.Message));
                }
                finally
                {
                    if(fileStream != null)
                    {
                        fileStream.Dispose();
                        fileStream = null;
                    }
                }

                if (File.Exists(szVersionListPath))
                {
                    File.Delete(szVersionListPath);
                }

                File.Move(szTempVersionListPath, szVersionListPath);
            }
        }
        private sealed partial class CReadWriteVersionListChecker
        {
            private class CCheckResourceInfo
            {
                public readonly CResourceName m_resName;
                public readonly ELoadType m_eLoadType;
                public readonly int m_nLen;
                public readonly int m_nHash;

                public CCheckResourceInfo(CResourceName a_resName, ELoadType a_eLoadType, int a_nLen, int a_nHash)
                {
                    m_resName = a_resName;
                    m_eLoadType = a_eLoadType;
                    m_nLen = a_nLen;
                    m_nHash = a_nHash;
                }
            }
        }
    }
}
