using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace XFrameworkBase
{
    public abstract class CFrameWorkSerializer<T>
    {
        private readonly Dictionary<byte, Func<Stream, T, bool>> m_mapSerializeCallbacks;
        private readonly Dictionary<byte, Func<Stream, T>> m_mapDeserializeCallbacks;
        private byte m_nLastVersion;

        public CFrameWorkSerializer()
        {
            m_nLastVersion = 0;
            m_mapSerializeCallbacks = new Dictionary<byte, Func<Stream, T, bool>>();
            m_mapDeserializeCallbacks = new Dictionary<byte, Func<Stream, T>>();
        }

        public void RegisterSerializeCallback(byte a_Version, Func<Stream, T, bool> callback)
        {
            Debug.Assert(callback != null);

            m_mapSerializeCallbacks[a_Version] = callback;
            if (a_Version > m_nLastVersion)
            {
                m_nLastVersion = a_Version;
            }
        }

        public void RegisterDeSerializeCallback(byte a_nVersion, Func<Stream, T> a_callback)
        {
            Debug.Assert(a_callback != null);
            m_mapDeserializeCallbacks[a_nVersion] = a_callback;
        }

        public bool Serialize(Stream stream, T a_data)
        {
            return Serialize(stream, a_data, m_nLastVersion);
        }

        public bool Serialize(Stream a_stream, T a_data, byte a_nVersion)
        {
            byte[] arrHeader = __GetHeader();
            a_stream.WriteByte(arrHeader[0]);
            a_stream.WriteByte(arrHeader[1]);
            a_stream.WriteByte(arrHeader[2]);
            a_stream.WriteByte(a_nVersion);
            Debug.Assert(m_mapSerializeCallbacks.ContainsKey(a_nVersion));
            return m_mapSerializeCallbacks[a_nVersion](a_stream, a_data);
        }

        public T Deserialize(Stream a_stream)
        {
            byte[] arrHeader = __GetHeader();
            byte h1 = (byte)a_stream.ReadByte();
            byte h2 = (byte)a_stream.ReadByte();
            byte h3 = (byte)a_stream.ReadByte();
            Debug.Assert(h1 == arrHeader[0] && h2 == arrHeader[1] && h3 == arrHeader[2]);

            byte version = (byte)a_stream.ReadByte();
            Debug.Assert(m_mapDeserializeCallbacks.ContainsKey(version));
            return m_mapDeserializeCallbacks[version](a_stream);
        }

        protected abstract byte[] __GetHeader();
    }
}
