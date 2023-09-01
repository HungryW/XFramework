using System;
using System.Collections.Generic;
using System.Text;

namespace XFrameworkBase
{
    internal class CTypeNamePair : IEquatable<CTypeNamePair>
    {
        private Type m_type;
        private string m_szName;

        public CTypeNamePair(Type a_t) : this(a_t, string.Empty)
        {

        }
        public CTypeNamePair(Type a_t, string a_szName)
        {
            m_type = a_t;
            m_szName = a_szName;
        }
        public bool Equals(CTypeNamePair other)
        {
            return other.m_type == m_type && m_szName == other.m_szName;
        }

        public static bool operator ==(CTypeNamePair a, CTypeNamePair b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(CTypeNamePair a, CTypeNamePair b)
        {
            return !(a == b);
        }
    }
}
