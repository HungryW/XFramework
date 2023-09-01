using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XFrameworkBase
{
    public abstract class CGameframeworkMoudle
    {
        public abstract int Priority { get; }
        public abstract void Update(float a_fElapseSed, float a_fRealElapseSed);

        public abstract void Shutdown();
    }

    public static class CGameframeworkEntry
    {
        private static LinkedList<CGameframeworkMoudle> m_listAllMoudle = new LinkedList<CGameframeworkMoudle>();
        private static Dictionary<Type, CGameframeworkMoudle> m_mapAllMoudle = new Dictionary<Type, CGameframeworkMoudle>();

        public static void Update(float a_ElapseSed, float a_fRealElapseSed)
        {
            foreach (var m in m_listAllMoudle)
            {
                m.Update(a_ElapseSed, a_fRealElapseSed);
            }
        }

        public static void Shutdown()
        {
            foreach (var m in m_listAllMoudle)
            {
                m.Shutdown();
            }
            m_listAllMoudle.Clear();
            m_mapAllMoudle.Clear();
            CReferencePoolMgr.CleanAll();
        }
        public static T GetMoudle<T>() where T : CGameframeworkMoudle, new()
        {
            Type t = typeof(T);
            CGameframeworkMoudle moudle = _GetMoudle(t);
            if (null == moudle)
            {
                moudle = (CGameframeworkMoudle)Activator.CreateInstance(t);
                _AddMoudle(moudle);
            }
            return (T)moudle;
        }

        private static CGameframeworkMoudle _GetMoudle(Type a_t)
        {
            CGameframeworkMoudle moudle = null;
            m_mapAllMoudle.TryGetValue(a_t, out moudle);
            return moudle;
        }

        private static void _AddMoudle(CGameframeworkMoudle a_moudle)
        {
            Type t = a_moudle.GetType();
            Debug.Assert(!m_mapAllMoudle.ContainsKey(t));
            m_mapAllMoudle.Add(t, a_moudle);
            LinkedListNode<CGameframeworkMoudle> cur = m_listAllMoudle.First;
            while (cur != null)
            {
                if (cur.Value.Priority > a_moudle.Priority)
                {
                    m_listAllMoudle.AddBefore(cur, a_moudle);
                    return;
                }
                cur = cur.Next;
            }
            m_listAllMoudle.AddLast(a_moudle);
        }

    }
}
