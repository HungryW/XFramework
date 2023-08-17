
using System;
using System.Collections.Generic;
using System.IO;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {

        private MemoryStream CacheStream { get { return m_cacheStream.Stream; } }
        private void _PrepareCacheStream() { m_cacheStream.PrepareCache(); }
        private void FreeCacheStream() { m_cacheStream.FreeCache(); }
        private sealed class CCachedStream
        {
            private MemoryStream m_cache;

            public MemoryStream Stream { get { return m_cache; } }
            public void PrepareCache()
            {
                if (m_cache == null)
                {
                    m_cache = new MemoryStream();
                }
                m_cache.Position = 0;
                m_cache.SetLength(0);
            }

            public void FreeCache()
            {
                if (m_cache != null)
                {
                    m_cache.Dispose();
                    m_cache = null;
                }
            }
        }

        private sealed class CRemoteVersionListChecker
        {
            private readonly CResourceMgr m_resourceMgr;
            private CDownloadMgr m_downloadMgr;
            private int m_nLen;
            private int m_nHash;
            private int m_nCompressedLen;
            private int m_nCompressedHash;

            public Action<string, string> m_fnUpdateSuccess;
            public Action<string, string> m_fnUpdateFail;

            public CRemoteVersionListChecker(CResourceMgr a_resMgr)
            {
                m_resourceMgr = a_resMgr;
                m_downloadMgr = null;
                m_nLen = 0;
                m_nHash = 0;
                m_nCompressedLen = 0;
                m_nCompressedHash = 0;

                m_fnUpdateSuccess = null;
                m_fnUpdateFail = null;
            }

            public void SetDownloadMgr(CDownloadMgr a_downloadMgr)
            {
                m_downloadMgr = a_downloadMgr;
                m_downloadMgr.m_EventDownloadComplete += _OnDownloadSuccess;
                m_downloadMgr.m_EventDownloadFail += _OnDownloadFail;   
            }

            public void Shutdown()
            {
                if(m_downloadMgr != null)
                {
                    m_downloadMgr.m_EventDownloadFail -= _OnDownloadFail;
                    m_downloadMgr.m_EventDownloadComplete -= _OnDownloadSuccess;
                }
            }

            public bool CheckNeedUpdateFile(int a_nLastVersion)
            {
                string szRemoteListFileName = Utility.Path.GetRegularPath(Path.Combine(m_resourceMgr.m_szReadWritePath, mc_szRemoteVersionListFileName));
                if (!File.Exists(szRemoteListFileName))
                {
                    return true;
                }
                int nFileVersion = 0;
                FileStream fileStream = null;
                try
                {
                    fileStream = new FileStream(szRemoteListFileName, FileMode.Open, FileAccess.Read);
                    CVersionListInfoRemote versionList = m_resourceMgr.m_serializerVersionListRemote.Deserialize(fileStream);
                    nFileVersion = versionList.m_nResVersion;
                }
                catch
                {
                    return true;
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Dispose();
                        fileStream = null;
                    }
                }

                return nFileVersion != a_nLastVersion;
            }

            public void UpdateFile(int a_nLen, int a_nHash, int a_nCompressLen, int a_nCompressHash)
            {
                m_nLen = a_nLen;
                m_nHash = a_nHash;
                m_nCompressedLen = a_nCompressLen;
                m_nCompressedHash = a_nCompressHash;

                string szSaveFilePath = Utility.Path.GetRegularPath(Path.Combine(m_resourceMgr.m_szReadWritePath, mc_szRemoteVersionListFileName));
                int nDotPos = szSaveFilePath.LastIndexOf(".");
                string szFileName = szSaveFilePath.Substring(0, nDotPos);
                string szExtension = szSaveFilePath.Substring(nDotPos + 1);
                string szRemoteFileName = Utility.Text.Format("{0}.{1:x8}.{2}", szFileName, m_nHash, szExtension);
                string szRemoteUri = Utility.Path.GetRemotePath(Path.Combine(m_resourceMgr.m_szUpdateUriPrefix, szRemoteFileName));
                m_downloadMgr.Download(szRemoteUri, szSaveFilePath, 0, this);
            }

            private void _OnDownloadSuccess(CEventDownloadComplete a_arg)
            {
                CRemoteVersionListChecker checker = a_arg.DownloadData.m_oUserData as CRemoteVersionListChecker;
                if (checker == null || checker != this)
                {
                    return;
                }
                try
                {
                    using (FileStream fileStream = new FileStream(a_arg.DownloadData.m_szFilePath, FileMode.Open, FileAccess.ReadWrite))
                    {
                        int nCompressLen = (int)fileStream.Length;
                        if (nCompressLen != m_nCompressedLen)
                        {
                            fileStream.Close();
                            _OnHandleDownloadFail(a_arg.DownloadData, Utility.Text.Format("Latest version list compressed length error, need '{0}', downloaded '{1}'.", m_nCompressedLen, nCompressLen));
                            return;
                        }
                        fileStream.Position = 0L;
                        int nCompressHash = Utility.Verifier.GetCrc32(fileStream);
                        if (nCompressHash != m_nCompressedHash)
                        {
                            fileStream.Close();
                            _OnHandleDownloadFail(a_arg.DownloadData, Utility.Text.Format("Latest version list compressed hash code error, need '{0}', downloaded '{1}'.", m_nCompressedHash, nCompressHash));
                            return;
                        }

                        fileStream.Position = 0L;
                        m_resourceMgr._PrepareCacheStream();
                        if (!Utility.Compression.Decompress(fileStream, m_resourceMgr.CacheStream))
                        {
                            fileStream.Close();
                            _OnHandleDownloadFail(a_arg.DownloadData, Utility.Text.Format("Unable to decompress latest version list '{0}'.", a_arg.DownloadData.m_szUrl));
                            return;
                        }
                        int nLen = (int)m_resourceMgr.CacheStream.Length;
                        if (nLen != m_nLen)
                        {
                            fileStream.Close();
                            _OnHandleDownloadFail(a_arg.DownloadData, Utility.Text.Format("Latest version list  length error, need '{0}', downloaded '{1}'.", m_nLen, nLen));
                            return;
                        }

                        fileStream.Position = 0L;
                        fileStream.SetLength(0L);
                        fileStream.Write(m_resourceMgr.CacheStream.GetBuffer(), 0, nLen);
                    }
                    if (m_fnUpdateSuccess != null)
                    {
                        m_fnUpdateSuccess(a_arg.DownloadData.m_szUrl, a_arg.DownloadData.m_szFilePath);
                    }
                }
                catch (Exception e)
                {
                    _OnHandleDownloadFail(a_arg.DownloadData, e.Message);
                }
            }

            private void _OnDownloadFail(CEventDownloadFail a_arg)
            {
                CRemoteVersionListChecker checker = a_arg.DownloadData.m_oUserData as CRemoteVersionListChecker;
                if (checker == null || checker != this)
                {
                    return;
                }
                _OnHandleDownloadFail(a_arg.DownloadData, a_arg.ErrorMsg);
            }

            private void _OnHandleDownloadFail(CEventDownloadData a_downloadData, string a_szErrorMsg)
            {
                if (File.Exists(a_downloadData.m_szFilePath))
                {
                    File.Delete(a_downloadData.m_szFilePath);
                }

                if (m_fnUpdateFail != null)
                {
                    m_fnUpdateFail(a_downloadData.m_szUrl, a_szErrorMsg);
                }
            }
        }
    }
}
