using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace XFrameworkBase
{
    public partial class CDownloadMgr
    {
        private class CDownloadAgent : ITaskAgent<CDownloadTask>, IDisposable
        {
            private CDownloadTask m_task;
            private IDownloadAgentHelper m_helper;
            private FileStream m_fileStream;
            private long m_nStartPos;
            private long m_nDownloadLen;
            private long m_nSaveLen;
            private int m_nUnFlushSize;
            private float m_fWaitTime;
            private bool m_bDisposabled;

            public Action<CDownloadAgent> m_fnOnStart;
            public Action<CDownloadAgent> m_fnOnComplete;
            public Action<CDownloadAgent, string> m_fnOnFail;

            public CDownloadAgent(IDownloadAgentHelper agentHelper)
            {
                m_helper = agentHelper;
                m_task = null;
                m_fileStream = null;
                m_bDisposabled = false;
                m_nDownloadLen = 0;
                m_nStartPos = 0;
                m_nSaveLen = 0;
                m_nUnFlushSize = 0;
                m_fWaitTime = 0;
                m_bDisposabled = false;

                m_fnOnStart = null;
                m_fnOnComplete = null;
                m_fnOnFail = null;
            }
            public void Init()
            {
                m_helper.m_EventDownloadAgentHelperUpdateBytes += _OnHelperDownloadBytes;
                m_helper.m_EventDownloadAgentHelperComplete += _OnHelperDownloadComplete;
                m_helper.m_EventDownloadAgentHelperFail += _OnHelperDownloadFail;
            }
            public void ShutDown()
            {
                Dispose();
                m_helper.m_EventDownloadAgentHelperUpdateBytes -= _OnHelperDownloadBytes;
                m_helper.m_EventDownloadAgentHelperComplete -= _OnHelperDownloadComplete;
                m_helper.m_EventDownloadAgentHelperFail -= _OnHelperDownloadFail;
            }

            private void _OnHelperDownloadBytes(CEventArgDownloadAgentHelperUpdateBytes a_arg)
            {
                m_fWaitTime = 0;

                m_fileStream.Write(a_arg.m_arrBytes, 0, a_arg.m_arrBytes.Length);
                m_nSaveLen += a_arg.m_arrBytes.Length;
                m_nUnFlushSize += a_arg.m_arrBytes.Length;
                if (m_nUnFlushSize > m_task.m_nFlushSize)
                {
                    m_fileStream.Flush();
                    m_nUnFlushSize = 0;
                }
            }

            private void _OnHelperDownloadComplete(CEventArgDownloadAgentHelperComplete a_arg)
            {
                m_fWaitTime = 0;
                m_nDownloadLen = a_arg.m_nLen;
                Debug.Assert(m_nDownloadLen + m_nStartPos == m_nSaveLen);

                m_fileStream.Close();
                m_fileStream = null;

                if (File.Exists(m_task.m_szFilePath))
                {
                    File.Delete(m_task.m_szFilePath);
                }

                string szTempFilePath = Utility.Text.Format("{0}.download", m_task.m_szFilePath);
                File.Move(szTempFilePath, m_task.m_szFilePath);

                m_task.m_eState = EDownLoadState.Done;
                m_task.m_bDone = true;
                if (m_fnOnComplete != null)
                {
                    m_fnOnComplete.Invoke(this);
                }
            }

            private void _OnHelperDownloadFail(CEventArgDownloadAgentHelperFail a_arg)
            {
                if (a_arg.m_bDeleteTempFile)
                {
                    string szTempFilePath = Utility.Text.Format("{0}.download", m_task.m_szFilePath);
                    if (File.Exists(szTempFilePath))
                    {
                        File.Delete(szTempFilePath);
                    }
                }
                m_task.m_eState = EDownLoadState.Error;
                m_task.m_bDone = true;
                if (m_fnOnFail != null)
                {
                    m_fnOnFail.Invoke(this, a_arg.m_szErrorMsg);
                }
            }

            public void Start(CTaskBase task)
            {
                Debug.Assert(task is CDownloadTask);
                m_task = (CDownloadTask)task;
                m_task.m_eState = EDownLoadState.Doing;

                string szTempFilePath = Utility.Text.Format("{0}.download", m_task.m_szFilePath);

                try
                {
                    if (File.Exists(szTempFilePath))
                    {
                        m_fileStream = File.OpenWrite(szTempFilePath);
                        m_fileStream.Seek(0, SeekOrigin.End);
                        m_nSaveLen = m_nStartPos = m_fileStream.Length;
                        m_nDownloadLen = 0;
                    }
                    else
                    {
                        string szDir = Path.GetDirectoryName(m_task.m_szFilePath);
                        if (!Directory.Exists(szDir))
                        {
                            Directory.CreateDirectory(szDir);
                        }
                        m_fileStream = new FileStream(szTempFilePath, FileMode.Create, FileAccess.Write);
                        m_nStartPos = m_nDownloadLen = m_nSaveLen = 0;
                    }
                    m_helper.Download(m_task.m_szULR, m_nSaveLen);
                    if (m_fnOnStart != null)
                    {
                        m_fnOnStart.Invoke(this);
                    }
                }
                catch (Exception ex)
                {
                    CEventArgDownloadAgentHelperFail arg = CEventArgDownloadAgentHelperFail.Create(ex.ToString(), false);
                    _OnHelperDownloadFail(arg);
                    CEventArgDownloadAgentHelperFail.Release(arg);
                }
            }

            public CDownloadTask Task => m_task;

            public void Dispose()
            {
                _Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void _Dispose(bool a_bDispose)
            {
                if (m_bDisposabled)
                {
                    return;
                }
                m_bDisposabled = true;

                if (a_bDispose)
                {
                    if (m_fileStream != null)
                    {
                        m_fileStream.Dispose();
                        m_fileStream = null;
                    }
                }
            }


            public void Reset()
            {
                if (m_fileStream != null)
                {
                    m_fileStream.Close();
                    m_fileStream = null;
                }
                m_fWaitTime = 0f;
                m_nDownloadLen = 0;
                m_nSaveLen = 0;
                m_nStartPos = 0;
                m_nUnFlushSize = 0;
                m_helper.Reset();
            }

            public void Update(float a_fElapseSed, float a_fRealElapseSed)
            {
                if (m_task.m_eState != EDownLoadState.Doing)
                {
                    return;
                }
                m_fWaitTime += a_fRealElapseSed;
                if (m_fWaitTime > m_task.m_fTimeout)
                {
                    m_fWaitTime = 0;
                    CEventArgDownloadAgentHelperFail arg = CEventArgDownloadAgentHelperFail.Create("Time Out", false);
                    _OnHelperDownloadFail(arg);
                    CEventArgDownloadAgentHelperFail.Release(arg);
                }
            }

        }
    }
}
