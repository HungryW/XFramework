
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XFrameworkBase
{
    public partial class CResourceMgr : CGameframeworkMoudle
    {
        private sealed partial class CResourceLoader
        {
            /// <summary>
            /// 资源的引用计数分为两个部分
            ///     1.资源包含的Asset的引用,当新加载(第一次加载)一个Asset时引用+1,当卸载一个Asset时引用-1
            ///     2.客体资源被其他资源依赖的个数,每个主体资源加载客体资源引用+1,每个主体资源卸载客体资源引用-1
            ///     A依赖B,B是A的客体资源,A是B的主体资源
            /// 客体资源引用增加的流程
            ///     因为依赖关系是Asset的依赖关系,仅仅加载创建资源后并不能获得依赖关系数据,所以依赖引用的增加要推迟到Asset创建时
            ///     *主体资源创建
            ///     主体Asset创建,获得客体Asset列表
            ///     *获得客体资源列表
            ///     筛选尚未添加的客体资源列表
            ///     *每个客体资源的引用+1
            ///     每个主体资源创建只会导致同一个客体资源引用增加1次
            /// 客体资源引用减少的流程
            ///     主体资源释放
            ///     获得客体资源列表
            ///     每个客体资源的引用-1
            /// </summary>
            private sealed class CResourceObjectMgr
            {
                private CResourceLoader m_refLoader;
                private IObjectPool<CResourceObject> m_resPool;
                private Dictionary<object, int> m_mapResBeDependNum;

                public CResourceObjectMgr(CResourceLoader refLoader)
                {
                    m_resPool = null;
                    m_mapResBeDependNum = new Dictionary<object, int>();
                    m_refLoader = refLoader;
                }

                public void Shutdown()
                {
                    m_mapResBeDependNum.Clear();
                }

                public void CreateResPool(IObjectPoolManager a_poolMgr)
                {
                    m_resPool = a_poolMgr.CreateMultiSpawnObjectPool<CResourceObject>("Resource Pool");
                }

                public void CreateResourceObject(string a_szResName, object a_oRes, IResourceHelper a_resHelper )
                {
                    CResourceObject resObj = CResourceObject.Create(a_szResName, a_oRes, this, a_resHelper);
                    m_resPool.Register(resObj, true);
                }

                public int GetBeDependNum(object a_oResTarget)
                {
                    int n = 0;
                    m_mapResBeDependNum.TryGetValue(a_oResTarget, out n);
                    return n;
                }

                public void RemoveDependInfo(object a_oResTarget)
                {
                    m_mapResBeDependNum.Remove(a_oResTarget);
                }

                public void AddBeDependNum(object a_oResTarget, int a_nNum)
                {
                    if (!m_mapResBeDependNum.ContainsKey(a_oResTarget))
                    {
                        m_mapResBeDependNum.Add(a_oResTarget, 0);
                    }
                    m_mapResBeDependNum[a_oResTarget] += a_nNum;
                    Debug.Assert(m_mapResBeDependNum[a_oResTarget] >= 0);
                }

                public IObjectPool<CResourceObject> GetResPool()
                {
                    return m_resPool;
                }

            }

            private sealed class CResourceObject : CObjectBase
            {
                private CResourceObjectMgr m_refMgr;
                private IResourceHelper m_refResourceHelper;
                private List<object> m_listDependRes;

                public CResourceObject()
                {
                    m_listDependRes = new List<object>();
                    m_refMgr = null;
                    m_refResourceHelper = null;
                }

                public override void Clear()
                {
                    base.Clear();
                    m_listDependRes.Clear();
                    m_refMgr = null;
                }

                public static CResourceObject Create(string a_szResName, object a_oRes, CResourceObjectMgr a_refResObjMgr, IResourceHelper a_refHelper)
                {
                    CResourceObject obj = CReferencePoolMgr.Acquire<CResourceObject>();
                    obj.Initialize(a_szResName, a_oRes);
                    obj.m_refMgr = a_refResObjMgr;
                    obj.m_refResourceHelper = a_refHelper;
                    return obj;
                }

                public void AddDependRes(object a_oOtherRes)
                {
                    if (Target == a_oOtherRes)
                    {
                        return;
                    }
                    if (m_listDependRes.Contains(a_oOtherRes))
                    {
                        return;
                    }
                    m_refMgr.AddBeDependNum(a_oOtherRes, 1);
                    m_listDependRes.Add(a_oOtherRes);
                }

                public override bool CustomCanReleaseFlag
                {
                    get
                    {
                        return m_refMgr.GetBeDependNum(Target) == 0 && base.CustomCanReleaseFlag;
                    }
                }
                protected internal override void Release(bool isShutdown)
                {
                    if (!isShutdown)
                    {
                        foreach (var dependRes in m_listDependRes)
                        {
                            m_refMgr.AddBeDependNum(dependRes, -1);
                        }
                    }
                    m_refMgr.RemoveDependInfo(Target);
                    m_refResourceHelper.ReleaseResource(Target);
                }
            }
        }

    }
}
