using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XFrameworkBase
{
    public abstract class CEventArgBase : IReference
    {
        public abstract int Id { get; }
        public abstract void Clear();
    }

    public partial class CEventPool<T> where T : CEventArgBase
    {
        private class CEvent : IReference
        {
            public T m_eventArg { get; private set; }
            public object m_oSender { get; private set; }
            public CEvent()
            {
                m_eventArg = null;
                m_oSender = null;
            }

            public void Clear()
            {
                m_oSender = null;
                m_eventArg = null;
            }

            public static CEvent Create(T a_arg, object a_oSender)
            {
                CEvent eve = CReferencePoolMgr.Acquire<CEvent>();
                eve.m_oSender = a_oSender;
                eve.m_eventArg = a_arg;
                return eve;
            }

            public static void Release(CEvent a_event)
            {
                if (null != a_event)
                {
                    CReferencePoolMgr.Release(a_event);
                }
            }
        }
    }

    public partial class CEventPool<T>
    {
        private Queue<CEvent> m_qEvent;
        private Dictionary<int, LinkedList<EventHandler<T>>> m_mapHandler;

        public CEventPool()
        {
            m_qEvent = new Queue<CEvent>();
            m_mapHandler = new Dictionary<int, LinkedList<EventHandler<T>>>();
        }

        public void Subscribe(int a_nEventId, EventHandler<T> a_handle)
        {
            LinkedList<EventHandler<T>> list = _GetOrAddHandlerList(a_nEventId);
            list.AddLast(a_handle);
        }

        public void UnSubcribe(int a_nEventId, EventHandler<T> a_handle)
        {
            Debug.Assert(null != a_handle);
            Debug.Assert(m_mapHandler.ContainsKey(a_nEventId));
            m_mapHandler[a_nEventId].Remove(a_handle);
        }

        public void Fire(object a_oSender, T a_arg)
        {
            CEvent eve = CEvent.Create(a_arg, a_oSender);
            m_qEvent.Enqueue(eve);
        }

        public void FrieNow(object a_oSender, T a_arg)
        {
            CEvent eve = CEvent.Create(a_arg, a_oSender);
            _HandleEvent(eve);
            CEvent.Release(eve);
        }

        public void Update()
        {
            _HandleAllEvent();
        }
        private void _HandleAllEvent()
        {
            while (m_qEvent.Count > 0)
            {
                CEvent eveCur = m_qEvent.Dequeue();
                _HandleEvent(eveCur);
            }
        }

        private void _HandleEvent(CEvent a_eve)
        {
            LinkedList<EventHandler<T>> list = _GetOrAddHandlerList(a_eve.m_eventArg.Id);
            LinkedListNode<EventHandler<T>> cur = list.First;
            LinkedListNode<EventHandler<T>> temp = null;
            while (cur != null)
            {
                temp = cur.Next;
                cur.Value.Invoke(a_eve.m_oSender, a_eve.m_eventArg);
                cur = temp;
            }
            CEvent.Release(a_eve);
        }


        private LinkedList<EventHandler<T>> _GetOrAddHandlerList(int a_nEventId)
        {
            LinkedList<EventHandler<T>> list = null;
            if (!m_mapHandler.TryGetValue(a_nEventId, out list))
            {
                list = new LinkedList<EventHandler<T>>();
                m_mapHandler.Add(a_nEventId, list);
            }
            return list;
        }
    }
}
