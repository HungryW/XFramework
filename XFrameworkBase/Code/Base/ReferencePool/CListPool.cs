using System;
using System.Collections.Generic;
using System.Text;

namespace XFrameworkBase
{
    public class CListPool<T> : IReference
    {
        private List<T> m_list;

        public CListPool()
        {
            m_list = new List<T>();
        }
        public void Clear()
        {
            m_list.Clear();
        }

        public static CListPool<T> Create()
        {
            CListPool<T> pool = CReferencePoolMgr.Acquire<CListPool<T>>();
            return pool;
        }

        public static void Release(CListPool<T> a_list)
        {
            CReferencePoolMgr.Release(a_list);
        }
    }
}
