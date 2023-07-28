
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {
        /// <summary>
        /// Asset的引用计数分为两个部分
        ///     1.Asset load次数,每Load一次引用+1,每Unload一次引用-1
        ///     2.作为客体Asset被依赖的次数,每创建一个主体Asset引用+1,每释放一个主体Asset引用-1
        /// Asset获取次数分为两个部分
        ///     1.作为主体资源每load一次引用+1,每unload一次引用-1
        ///     2.作为客体资源被动load一次引用+1,主体资源unload一次引用-1
        /// </summary>
        private sealed partial class CResourceLoader
        {
            private sealed class CAssetObjectMgr
            {
                private CResourceLoader m_refLoader;
                private Dictionary<object, int> m_mapAssetBeDependNum;
                private Dictionary<object, object> m_mapAsset2Res;
                private IObjectPool<CAssetObject> m_assetPool;

                public CAssetObjectMgr(CResourceLoader refLoader)
                {
                    m_mapAssetBeDependNum = new Dictionary<object, int>();
                    m_mapAsset2Res = new Dictionary<object, object>();
                    m_assetPool = null;
                    m_refLoader = refLoader;
                }

                public void Shutdown()
                {
                    m_mapAsset2Res.Clear();
                    m_mapAssetBeDependNum.Clear();
                }

                public void CreateAsset(string a_szAssetName, object a_oAsset, List<object> a_listDependAsset, CResourceObject a_resObj)
                {
                    CAssetObject asset = CAssetObject.Create(a_szAssetName, a_oAsset, a_listDependAsset, a_resObj.Target, this);
                    m_assetPool.Register(asset, true);

                    m_mapAsset2Res.Add(a_oAsset, a_resObj.Target);
                    foreach (var dependAsset in a_listDependAsset)
                    {
                        object dependRes;
                        if (m_mapAsset2Res.TryGetValue(dependAsset, out dependRes))
                        {
                            a_resObj.AddDependRes(dependRes);
                        }
                    }
                }

                public void RemoveAssetObjectInfo(CAssetObject a_asset)
                {
                    m_mapAsset2Res.Remove(a_asset.Target);
                    m_mapAssetBeDependNum.Remove(a_asset.Target);
                }


                public void CreateAssetPool(IObjectPoolManager a_poolMgr)
                {
                    m_assetPool = a_poolMgr.CreateMultiSpawnObjectPool<CAssetObject>();
                }

                public void AddDependNum(object a_oAsset, int a_nAdd)
                {
                    if (!m_mapAssetBeDependNum.ContainsKey(a_oAsset))
                    {
                        m_mapAssetBeDependNum.Add(a_oAsset, 0);
                    }
                    m_mapAssetBeDependNum[a_oAsset] += a_nAdd;
                    Debug.Assert(m_mapAssetBeDependNum[a_oAsset] >= 0);
                }


                public int GetDependNum(object a_oAsset)
                {
                    int n = 0;
                    m_mapAssetBeDependNum.TryGetValue(a_oAsset, out n);
                    return n;
                }

                public IObjectPool<CResourceObject> GetResPool()
                {
                    return m_refLoader.m_resourceObjMgr.GetResPool();
                }

                public IObjectPool<CAssetObject> GetAssetPool()
                {
                    return m_assetPool;
                }
            }
            private sealed class CAssetObject : CObjectBase
            {
                private CAssetObjectMgr m_refMgr;
                private object m_oRes;
                private List<object> m_listDependAsset;

                public CAssetObject()
                {
                    m_refMgr = null;
                    m_oRes = null;
                    m_listDependAsset = new List<object>();
                }

                public override void Clear()
                {
                    base.Clear();
                    m_listDependAsset.Clear();
                    m_refMgr = null;
                    m_oRes = null;
                }

                public static CAssetObject Create(string a_szName, object a_asset, List<object> a_listDependAsset, object a_oRes, CAssetObjectMgr a_refMgr)
                {
                    CAssetObject asset = CReferencePoolMgr.Acquire<CAssetObject>();
                    asset.Initialize(a_szName, a_asset);
                    asset.m_refMgr = a_refMgr;
                    asset.m_oRes = a_oRes;
                    asset.m_listDependAsset.AddRange(a_listDependAsset);
                    foreach (var dependAsset in a_listDependAsset)
                    {
                        asset.m_refMgr.AddDependNum(dependAsset, 1);
                    }
                    return asset;
                }

                protected internal override void OnUnspawn()
                {
                    base.OnUnspawn();
                    foreach (var dependAsset in m_listDependAsset)
                    {
                        m_refMgr.GetAssetPool().Unspawn(dependAsset);
                    }
                }

                public override bool CustomCanReleaseFlag
                {
                    get
                    {
                        return m_refMgr.GetDependNum(Target) <= 0 && base.CustomCanReleaseFlag;
                    }
                }

                protected internal override void Release(bool isShutdown)
                {
                    if (!isShutdown)
                    {
                        foreach (var dependAsset in m_listDependAsset)
                        {
                            m_refMgr.AddDependNum(dependAsset, -1);
                        }
                        m_refMgr.GetResPool().Unspawn(m_oRes);
                    }

                    m_refMgr.RemoveAssetObjectInfo(this);
                }
            }
        }
    }
}
