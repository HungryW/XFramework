
using System;
using System.Collections.Generic;

namespace XFrameworkBase
{
    public enum EResourceMode
    {
        Package = 1,
        Updatable,
        UpdatableWhilePlaying,
    }
    public enum ELoadAssetProgress
    {
        UnKnow = 0,
        ReadResource,
        LoadResource,
        LoadAsset,
        LoadScene,
    }

    public enum ELoadAssetStatus
    {
        Success = 0,
        NotExist,
        NotReady,
        DependencyError,
        TypeError,
        AssetError,
    }
    public sealed class CEventLoadAssetAgentHelperUpdate : IReference
    {

        public ELoadAssetProgress ProgressType
        {
            get;
            private set;
        }

        public float Progress
        {
            get;
            private set;
        }
        public void Clear()
        {
            Progress = 0;
            ProgressType = ELoadAssetProgress.UnKnow;
        }

        public static CEventLoadAssetAgentHelperUpdate Create(ELoadAssetProgress a_eProgressType, float a_fProgress)
        {
            CEventLoadAssetAgentHelperUpdate arg = CReferencePoolMgr.Acquire<CEventLoadAssetAgentHelperUpdate>();
            arg.ProgressType = a_eProgressType;
            arg.Progress = a_fProgress;
            return arg;
        }
    }

    public sealed class CEventLoadAssetAgentHelperReadFileComplete : IReference
    {
        public object Resource
        {
            get;
            private set;
        }

        public void Clear()
        {
            Resource = null;
        }

        public CEventLoadAssetAgentHelperReadFileComplete Create(object resource)
        {
            CEventLoadAssetAgentHelperReadFileComplete arg = CReferencePoolMgr.Acquire<CEventLoadAssetAgentHelperReadFileComplete>();
            arg.Resource = resource;
            return arg;
        }
    }

    public sealed class CEventLoadAssetAgentHelperReadBytesComplete : IReference
    {
        private byte[] m_arrBytes;

        public void Clear()
        {
            m_arrBytes = null;
        }

        public byte[] GetBytes()
        {
            return m_arrBytes;
        }

        public static CEventLoadAssetAgentHelperReadBytesComplete Create(byte[] arrBytes)
        {
            CEventLoadAssetAgentHelperReadBytesComplete arg = CReferencePoolMgr.Acquire<CEventLoadAssetAgentHelperReadBytesComplete>();
            arg.m_arrBytes = arrBytes;
            return arg;
        }
    }

    public sealed class CEventLoadAssetAgentHelperParseBytesComplete : IReference
    {
        public object Resource
        {
            get;
            private set;
        }

        public void Clear()
        {
            Resource = null;
        }

        public CEventLoadAssetAgentHelperParseBytesComplete Create(object resource)
        {
            CEventLoadAssetAgentHelperParseBytesComplete arg = CReferencePoolMgr.Acquire<CEventLoadAssetAgentHelperParseBytesComplete>();
            arg.Resource = resource;
            return arg;
        }
    }

    public sealed class CEventLoadAssetAgentHelperLoadAssetComplete : IReference
    {
        public object Asset
        {
            get;
            private set;
        }

        public void Clear()
        {
            Asset = null;
        }

        public CEventLoadAssetAgentHelperLoadAssetComplete Create(object asset)
        {
            CEventLoadAssetAgentHelperLoadAssetComplete arg = CReferencePoolMgr.Acquire<CEventLoadAssetAgentHelperLoadAssetComplete>();
            arg.Asset = asset;
            return arg;
        }
    }

    public sealed class CEventLoadAssetAgentHelperLoadAssetError : IReference
    {
        public ELoadAssetStatus ELoadAssetStatus { get; private set; }
        public string Error { get; private set; }

        public void Clear()
        {
            ELoadAssetStatus = ELoadAssetStatus.Success;
            Error = string.Empty;
        }

        public static CEventLoadAssetAgentHelperLoadAssetError Create(ELoadAssetStatus eLoadAssetStatus, string error)
        {
            CEventLoadAssetAgentHelperLoadAssetError arg = CReferencePoolMgr.Acquire<CEventLoadAssetAgentHelperLoadAssetError>();
            arg.ELoadAssetStatus = eLoadAssetStatus;
            arg.Error = error;
            return arg;
        }
    }
}
