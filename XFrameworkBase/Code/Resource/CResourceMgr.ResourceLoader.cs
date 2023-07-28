
using System;
using System.Collections.Generic;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {
        private sealed partial class CResourceLoader
        {
            private CResourceMgr m_refMgr;
            private CResourceObjectMgr m_resourceObjMgr;
            private CAssetObjectMgr m_assetObjMgr;

            public CResourceLoader(CResourceMgr refMgr)
            {
                m_refMgr = refMgr;
                m_resourceObjMgr = new CResourceObjectMgr(this);
                m_assetObjMgr = new CAssetObjectMgr(this);
            }

            public void CreateObjPool(IObjectPoolManager a_refPoolMgr)
            {
                m_resourceObjMgr.CreateResPool(a_refPoolMgr);
                m_assetObjMgr.CreateAssetPool(a_refPoolMgr);
            }

            public void Shutdown()
            {
                m_assetObjMgr.Shutdown();
                m_resourceObjMgr.Shutdown();
            }

            public void Update(float a_fElapseSed, float a_fRealElapseSed)
            {

            }
        }
    }
}
