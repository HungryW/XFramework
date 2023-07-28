using System;
using System.Collections.Generic;

namespace XFrameworkBase
{
    public partial class CDownloadMgr : CGameframeworkMoudle
    {
        private CTaskPool<CDownloadTask> m_taskPool;

        public event Action<CEventDownloadStart> m_EventDownloadStart;
        public event Action<CEventDownloadComplete> m_EventDownloadComplete;
        public event Action<CEventDownloadFail> m_EventDownloadFail;

        public CDownloadMgr()
        {
            m_taskPool = new CTaskPool<CDownloadTask>();
            m_EventDownloadStart = null;
            m_EventDownloadComplete = null;
            m_EventDownloadFail = null;
        }

        public override int Priority => 5;
        public override void Shutdown()
        {
            m_taskPool.Shutdown();
        }

        public override void Update(float a_fElapseSed, float a_fRealElapseSed)
        {
            m_taskPool.Update(a_fElapseSed, a_fRealElapseSed);
        }

        public void AddAgent(IDownloadAgentHelper a_helper)
        {
            CDownloadAgent agent = new CDownloadAgent(a_helper);
            agent.m_fnOnStart = _OnDownLoadStart;
            agent.m_fnOnComplete = _OnDownLoadComplete;
            agent.m_fnOnFail = _OnDownLoadFail;
            m_taskPool.AddAgent(agent);
        }

        public void Download(string a_szUrl, string a_szFilePath, int a_nPrority, object a_oUserData)
        {
            CDownloadTask task = CDownloadTask.Create(a_szUrl, a_szFilePath, 30, 1024, a_nPrority, a_oUserData);
            m_taskPool.AddTask(task);
        }

        public void RemoveTask(int a_nId)
        {
            m_taskPool.RemoveTaskById(a_nId);
        }
        public void RemoveAllTask()
        {
            m_taskPool.RemoveAllTask();
        }

        private void _OnDownLoadStart(CDownloadAgent a_agent)
        {
            if (m_EventDownloadStart != null)
            {
                CDownloadTask task = a_agent.Task;
                CEventDownloadStart eve = CEventDownloadStart.Create(task.m_nId, task.m_szULR, task.m_szFilePath, 0, task.m_oUserData);
                m_EventDownloadStart.Invoke(eve);
                CEventDownloadStart.Release(eve);
            }
        }

        private void _OnDownLoadComplete(CDownloadAgent a_agent)
        {
            if (m_EventDownloadComplete != null)
            {
                CDownloadTask task = a_agent.Task;
                CEventDownloadComplete eve = CEventDownloadComplete.Create(task.m_nId, task.m_szULR, task.m_szFilePath, 0, task.m_oUserData);
                m_EventDownloadComplete.Invoke(eve);
                CEventDownloadComplete.Release(eve);
            }
        }

        private void _OnDownLoadFail(CDownloadAgent a_agent, string a_szFailMsg)
        {
            if (m_EventDownloadFail != null)
            {
                CDownloadTask task = a_agent.Task;
                CEventDownloadFail eve = CEventDownloadFail.Create(task.m_nId, task.m_szULR, task.m_szFilePath, a_szFailMsg, task.m_oUserData);
                m_EventDownloadFail.Invoke(eve);
                CEventDownloadFail.Release(eve);
            }
        }
    }


    public class CEventDownloadData
    {
        public int m_nId { get; private set; }
        public string m_szUrl { get; private set; }
        public string m_szFilePath { get; private set; }
        public long m_nCurLen { get; private set; }
        public object m_oUserData { get; private set; }

        public CEventDownloadData()
        {
            Clean();
        }
        public void Init(int a_nId, string a_szUrl, string a_szFilePath, long a_nCurLen, object a_oUserData)
        {
            m_nId = a_nId;
            m_szUrl = a_szUrl;
            m_szFilePath = a_szFilePath;
            m_nCurLen = a_nCurLen;
            m_oUserData = a_oUserData;
        }

        public void Clean()
        {
            m_nId = -1;
            m_szUrl = null;
            m_szFilePath = null;
            m_nCurLen = 0;
            m_oUserData = null;
        }
    }
    public class CEventDownloadStart : IReference
    {
        private CEventDownloadData m_data;
        public CEventDownloadStart()
        {
            m_data = new CEventDownloadData();
        }

        public CEventDownloadData DownloadData
        {
            get;
            private set;
        }

        public void Clear()
        {
            m_data.Clean();
        }

        public static CEventDownloadStart Create(int a_nId, string a_szUrl, string a_szFilePath, long a_nCurLen, object a_oUserData)
        {
            CEventDownloadStart cEventDownloadStart = CReferencePoolMgr.Acquire<CEventDownloadStart>();
            cEventDownloadStart.m_data.Init(a_nId, a_szUrl, a_szFilePath, a_nCurLen, a_oUserData);
            return cEventDownloadStart;
        }

        public static void Release(CEventDownloadStart a_event)
        {
            CReferencePoolMgr.Release(a_event);
        }
    }

    public class CEventDownloadComplete : IReference
    {
        private CEventDownloadData m_data;
        public CEventDownloadComplete()
        {
            m_data = new CEventDownloadData();
        }

        public CEventDownloadData DownloadData
        {
            get;
            private set;
        }

        public void Clear()
        {
            m_data.Clean();
        }

        public static CEventDownloadComplete Create(int a_nId, string a_szUrl, string a_szFilePath, long a_nCurLen, object a_oUserData)
        {
            CEventDownloadComplete eve = CReferencePoolMgr.Acquire<CEventDownloadComplete>();
            eve.m_data.Init(a_nId, a_szUrl, a_szFilePath, a_nCurLen, a_oUserData);
            return eve;
        }

        public static void Release(CEventDownloadComplete a_event)
        {
            CReferencePoolMgr.Release(a_event);
        }
    }

    public class CEventDownloadFail : IReference
    {
        private CEventDownloadData m_data;
        public CEventDownloadFail()
        {
            m_data = new CEventDownloadData();
            ErrorMsg = null;
        }

        public CEventDownloadData DownloadData
        {
            get;
            private set;
        }

        public string ErrorMsg
        {
            get;
            private set;
        }

        public void Clear()
        {
            m_data.Clean();
            ErrorMsg = null;
        }

        public static CEventDownloadFail Create(int a_nId, string a_szUrl, string a_szFilePath, string a_szErrorMsg, object a_oUserData)
        {
            CEventDownloadFail eve = CReferencePoolMgr.Acquire<CEventDownloadFail>();
            eve.m_data.Init(a_nId, a_szUrl, a_szFilePath, 0, a_oUserData);
            eve.ErrorMsg = a_szErrorMsg;
            return eve;
        }

        public static void Release(CEventDownloadFail a_event)
        {
            CReferencePoolMgr.Release(a_event);
        }
    }

}
