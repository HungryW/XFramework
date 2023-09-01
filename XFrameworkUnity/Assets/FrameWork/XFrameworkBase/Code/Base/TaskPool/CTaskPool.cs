using System;
using System.Collections.Generic;

namespace XFrameworkBase
{
    public class CTaskBase : IReference
    {
        public int m_nId { get; private set; }
        public bool m_bDone { get; set; }
        public object m_oUserData { get; private set; }
        public int m_nPriority { get; private set; }

        public CTaskBase()
        {
            m_nId = -1;
            m_bDone = false;
            m_oUserData = null;
            m_nPriority = 0;
        }

        public void Init(int a_nId, int a_nPriority, object a_oUserData)
        {
            m_nId = a_nId;
            m_nPriority = a_nPriority;
            m_oUserData = a_oUserData;
            m_nPriority = 0;
        }

        public virtual void Clear()
        {
            m_nId = -1;
            m_bDone = false;
            m_oUserData = null;
            m_nPriority = 0;
        }
    }

    public interface ITaskAgent<T> where T : CTaskBase
    {
        public T Task { get; }

        public void Init();

        public void Start(CTaskBase task);

        public void Update(float a_fElapseSed, float a_fRealElapseSed);

        public void Reset();

        public void ShutDown();
    }

    public class CTaskPool<T> where T : CTaskBase
    {
        private LinkedList<T> m_listWaitTask;
        private Stack<ITaskAgent<T>> m_freeAgent;
        private LinkedList<ITaskAgent<T>> m_listDoingAgent;

        public CTaskPool()
        {
            m_listWaitTask = new LinkedList<T>();
            m_freeAgent = new Stack<ITaskAgent<T>>();
            m_listDoingAgent = new LinkedList<ITaskAgent<T>>();
        }

        public void AddAgent(ITaskAgent<T> a_agent)
        {
            a_agent.Init();
            m_freeAgent.Push(a_agent);
        }

        public void AddTask(T a_task)
        {
            LinkedListNode<T> cur = m_listWaitTask.First;
            while (cur != null)
            {
                if (cur.Value.m_nPriority > a_task.m_nPriority)
                {
                    m_listWaitTask.AddBefore(cur, a_task);
                    return;
                }
                cur = cur.Next;
            }
            m_listWaitTask.AddLast(a_task);
        }

        public void Update(float a_fElaspseSed, float a_fRealElapseSed)
        {
            _UpdateDoingAgent(a_fElaspseSed, a_fRealElapseSed);
            _UpdateWaitingTask(a_fElaspseSed, a_fRealElapseSed);
        }

        public void Shutdown()
        {
            RemoveAllTask();
            foreach (ITaskAgent<T> agent in m_freeAgent)
            {
                agent.ShutDown();
            }
            m_freeAgent.Clear();
        }

        public void RemoveAllTask()
        {
            foreach (var task in m_listWaitTask)
            {
                CReferencePoolMgr.Release(task);
            }
            m_listWaitTask.Clear();

            LinkedListNode<ITaskAgent<T>> cur = m_listDoingAgent.First;
            while (cur != null)
            {
                _RemoveDoingAgent(cur.Value);
                cur = cur.Next;
            }
            m_listDoingAgent.Clear();
        }

        public void RemoveTaskById(int a_nId)
        {
            foreach (var task in m_listWaitTask)
            {
                if (task.m_nId == a_nId)
                {
                    CReferencePoolMgr.Release(task);
                    m_listWaitTask.Remove(task);
                    return;
                }
            }
            foreach (var agent in m_listDoingAgent)
            {
                if (agent.Task.m_nId == a_nId)
                {
                    _RemoveDoingAgent(agent);
                    return;
                }
            }
        }

        private void _RemoveDoingAgent(ITaskAgent<T> a_agent)
        {
            T task = a_agent.Task;
            a_agent.Reset();
            m_freeAgent.Push(a_agent);
            CReferencePoolMgr.Release(task);
        }

        private void _UpdateDoingAgent(float a_fElapseSed, float a_fRealElapseSed)
        {
            LinkedListNode<ITaskAgent<T>> cur = m_listDoingAgent.First;
            while (cur != null)
            {
                LinkedListNode<ITaskAgent<T>> temp = cur.Next;
                if (cur.Value.Task.m_bDone)
                {
                    CReferencePoolMgr.Release(cur.Value.Task);
                    cur.Value.Reset();
                    m_listDoingAgent.Remove(cur);
                    m_freeAgent.Push(cur.Value);
                }
                else
                {
                    cur.Value.Update(a_fElapseSed, a_fRealElapseSed);
                }
                cur = temp;
            }
        }

        private void _UpdateWaitingTask(float a_fElapseSed, float a_fRealElapseSed)
        {
            while (m_freeAgent.Count > 0 && m_listWaitTask.Count > 0)
            {
                ITaskAgent<T> freeAgent = m_freeAgent.Pop();
                T task = m_listWaitTask.First.Value;
                freeAgent.Start(task);
                m_listWaitTask.Remove(task);
                m_listDoingAgent.AddLast(freeAgent);
            }
        }
    }

}
