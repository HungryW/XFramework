﻿
using System;
using System.Collections.Generic;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {
        private CResourceInfoMgr m_resInfoMgr;
        private CAssetInfoMgr m_assetInfoMgr;
        private CResourceLoader m_resourceLoader;


        public CResourceMgr()
        {
            m_resInfoMgr = new CResourceInfoMgr();
            m_assetInfoMgr = new CAssetInfoMgr();
            m_resourceLoader = new CResourceLoader(this);
        }

        public override int Priority => throw new NotImplementedException();

        public override void Shutdown()
        {
            m_resourceLoader.Shutdown();
            m_resInfoMgr.Clean();
            m_assetInfoMgr.Clean();
        }


        public override void Update(float a_fElapseSed, float a_fRealElapseSed)
        {
            m_resourceLoader.Update(a_fElapseSed, a_fRealElapseSed);
        }
    }
}