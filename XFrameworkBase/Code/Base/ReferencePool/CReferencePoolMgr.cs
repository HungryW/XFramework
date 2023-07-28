using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XFrameworkBase
{
    public interface IReference
    {
        void Clear();
    }
    public static partial class CReferencePoolMgr
    {
        private class CReferencePool
        {
            private Type m_tType;
            private Stack<IReference> m_pool;

            public CReferencePool(Type a_t)
            {
                m_tType = a_t;
                m_pool = new Stack<IReference>();
            }

            public T Acquire<T>() where T : class, IReference, new()
            {
                Debug.Assert(typeof(T) == m_tType, "Type Not Match");
                if (m_pool.Count == 0)
                {
                    T t = new T();
                    return t;
                }
                return (T)m_pool.Pop();
            }

            public void Release(IReference a_reference)
            {
                Debug.Assert(a_reference != null);
                a_reference.Clear();
                m_pool.Push(a_reference);
            }

            public void CleanAll()
            {
                m_pool.Clear();
            }
        }

    }


    public static partial class CReferencePoolMgr
    {
        private static Dictionary<Type, CReferencePool> m_mapPool = new Dictionary<Type, CReferencePool>();

        public static T Acquire<T>() where T : class, IReference, new()
        {
            CReferencePool pool = _GetOrAddPool(typeof(T));
            return pool.Acquire<T>();
        }

        public static void Release(IReference a_reference)
        {
            CReferencePool pool = _GetOrAddPool(a_reference.GetType());
            pool.Release(a_reference);
        }

        public static void CleanAll()
        {
            foreach (var pool in m_mapPool.Values)
            {
                pool.CleanAll();
            }
            m_mapPool.Clear();
        }
        private static CReferencePool _GetOrAddPool(Type a_t)
        {
            CReferencePool pool;
            m_mapPool.TryGetValue(a_t, out pool);
            if (null == pool)
            {
                pool = new CReferencePool(a_t);
                m_mapPool.Add(a_t, pool);
            }
            return pool;
        }
    }
}
