using System;
using System.Collections.Generic;
using System.Text;

namespace XFrameworkBase
{
    public interface IResourceHelper
    {
        public void ReleaseResource(object a_res);

        public void UnloadScene(string a_szSceneAssetName, object a_oUserData);
    }

    public interface ILoadAssetAgentHelper
    {
        event Action<CEventLoadAssetAgentHelperUpdate> EventLoadUpdate;
        event Action<CEventLoadAssetAgentHelperReadFileComplete> EventReadFileComplete;
        event Action<CEventLoadAssetAgentHelperReadBytesComplete> EventReadBytesComplete;
        event Action<CEventLoadAssetAgentHelperParseBytesComplete> EventParseBytesComplete;
        event Action<CEventLoadAssetAgentHelperLoadAssetComplete> EventLoadAssetComplete;
        event Action<CEventLoadAssetAgentHelperLoadAssetError> EventLoadAssetError;

        void ReadFile(string a_szFullPath);
        void ReadBytes(string a_szFullPath);
        void ParseBytes(byte[] a_arrBytes);
        void LoadAsset(object a_oResource, string a_szAssetName, Type a_tAssetType, bool a_bIsScene);
        void Reset();
    }
}
