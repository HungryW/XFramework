
using System;
using System.Collections.Generic;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {
        private const string mc_szPackageVersionListFileName = "PackageVersionList.dat";
        private const string mc_szRemoteVersionListFileName = "RemoteVersionList.dat";
        private const string mc_szLocalVersionListFileName = "LocalVersionList.dat";

        private EResourceMode m_eMode;
        private IResourceHelper m_helper;
        private CRemoteVersionListChecker m_remoteVersionChecker;
        private CResourceChecker m_resourceChecker;
        private CResourceInfoMgr m_resInfoMgr;
        private CAssetInfoMgr m_assetInfoMgr;
        private CResourceLoader m_resourceLoader;
       
        private CReadWriteResourceInfoMgr m_readWriteResInfoMgr;

        private CCachedStream m_cacheStream;

        private CSerializerVersionListPackage m_serializerVersionListPackage;
        private CSerializerVersionListRemote m_serializerVersionListRemote;
        private CSerializerVersionListLocal m_serializerVersionListLocal;

        private string m_szReadOnlyPath;
        private string m_szReadWritePath;
        private string m_szUpdateUriPrefix;
        public CResourceMgr()
        {
            m_eMode = EResourceMode.Package;
            m_helper = null;
            m_remoteVersionChecker = new CRemoteVersionListChecker(this);
            m_resourceChecker= new CResourceChecker(this);
            m_resInfoMgr = new CResourceInfoMgr();
            m_assetInfoMgr = new CAssetInfoMgr();
            m_resourceLoader = new CResourceLoader(this);
        
            m_readWriteResInfoMgr = new CReadWriteResourceInfoMgr();
            m_cacheStream = new CCachedStream();
        }

        public override int Priority => throw new NotImplementedException();

        public override void Shutdown()
        {
            m_resourceLoader.Shutdown();
            m_resInfoMgr.Clean();
            m_assetInfoMgr.Clean();
            m_resourceChecker.Shutdown();
            m_readWriteResInfoMgr.Clean();
            m_remoteVersionChecker.Shutdown();
            m_cacheStream.FreeCache();
        }


        public override void Update(float a_fElapseSed, float a_fRealElapseSed)
        {
            m_resourceLoader.Update(a_fElapseSed, a_fRealElapseSed);
        }

        private void _UpdateResource(CResourceName a_resName)
        {

        }
    }
}
