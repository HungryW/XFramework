using System;
using System.Collections.Generic;
using System.Text;

namespace XFrameworkBase
{
    public partial class CDownloadMgr
    {
        public enum EDownLoadState
        {
            Todo = 0,
            Doing,
            Done,
            Error,
        }
        private class CDownloadTask : CTaskBase
        {
            private static int ms_nIdSeed = 0;

            public string m_szULR { get; private set; }
            public string m_szFilePath { get; private set; }

            public EDownLoadState m_eState { get; set; }
            public float m_fTimeout { get; private set; }
            public int m_nFlushSize { get; private set; }

            public override void Clear()
            {
                base.Clear();
                m_szULR = null;
                m_szFilePath = null;
                m_eState = EDownLoadState.Todo;
            }
            public static CDownloadTask Create(string a_szUrl, string a_szFilePath, float a_fTimeout, int a_nFlushSize, int a_nPrority, object a_oUserData)
            {
                CDownloadTask task = CReferencePoolMgr.Acquire<CDownloadTask>();
                task.Init(ms_nIdSeed++, a_nPrority, a_oUserData);
                task.m_szULR = a_szUrl;
                task.m_szFilePath = a_szFilePath;
                task.m_eState = EDownLoadState.Todo;
                task.m_fTimeout = a_fTimeout;
                task.m_nFlushSize = a_nFlushSize;
                return task;
            }

            public static void Release(CDownloadTask task)
            {
                CReferencePoolMgr.Release(task);
            }
        }
    }
}
