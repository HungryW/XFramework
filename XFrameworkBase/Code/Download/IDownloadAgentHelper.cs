using System;
using System.Collections.Generic;
using System.Text;

namespace XFrameworkBase
{
    public class CEventArgDownloadAgentHelperUpdateBytes : IReference
    {
        public byte[] m_arrBytes { get; private set; }
        public int m_nOffset { get; private set; }
        public int m_nLen { get; private set; }

        public void Clear()
        {
            m_arrBytes = null;
            m_nOffset = 0;
            m_nLen = 0;
        }

        public static CEventArgDownloadAgentHelperUpdateBytes Create(byte[] a_arrBytes, int a_nOffset, int a_nLen)
        {
            CEventArgDownloadAgentHelperUpdateBytes arg = CReferencePoolMgr.Acquire<CEventArgDownloadAgentHelperUpdateBytes>();
            arg.m_arrBytes = a_arrBytes;
            arg.m_nLen = a_nLen;
            arg.m_nOffset = a_nOffset;
            return arg;
        }

        public static void Release(CEventArgDownloadAgentHelperUpdateBytes a_arg)
        {
            CReferencePoolMgr.Release(a_arg);
        }
    }

    public class CEventArgDownloadAgentHelperComplete : IReference
    {
        public long m_nLen { get; private set; }
        public static CEventArgDownloadAgentHelperComplete Create(int a_nLen)
        {
            CEventArgDownloadAgentHelperComplete arg = CReferencePoolMgr.Acquire<CEventArgDownloadAgentHelperComplete>();
            arg.m_nLen = a_nLen;
            return arg;
        }

        public static void Release(CEventArgDownloadAgentHelperComplete a_arg)
        {
            CReferencePoolMgr.Release(a_arg);
        }

        public void Clear()
        {
            m_nLen = 0;
        }
    }

    public class CEventArgDownloadAgentHelperFail : IReference
    {
        public string m_szErrorMsg { get; private set; }

        public bool m_bDeleteTempFile { get; private set; }

        public void Clear()
        {
            m_szErrorMsg = null;
            m_bDeleteTempFile = false;
        }

        public static CEventArgDownloadAgentHelperFail Create(string a_szErrorMsg, bool a_bDeleteTempFile)
        {
            CEventArgDownloadAgentHelperFail arg = CReferencePoolMgr.Acquire<CEventArgDownloadAgentHelperFail>();
            arg.m_szErrorMsg = a_szErrorMsg;
            arg.m_bDeleteTempFile = a_bDeleteTempFile;
            return arg;
        }

        public static void Release(CEventArgDownloadAgentHelperFail a_arg)
        {
            CReferencePoolMgr.Release(a_arg);
        }
    }
    public interface IDownloadAgentHelper
    {
        public event Action<CEventArgDownloadAgentHelperUpdateBytes> m_EventDownloadAgentHelperUpdateBytes;
        public event Action<CEventArgDownloadAgentHelperComplete> m_EventDownloadAgentHelperComplete;
        public event Action<CEventArgDownloadAgentHelperFail> m_EventDownloadAgentHelperFail;
        public void Download(string a_szUrl, long a_nStart);

        public void Reset();
    }
}
