using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace XFrameWork.Editor
{
    public sealed class CDependencyData
    {
        private List<CResource> m_listDependencyResource;
        private List<CAsset> m_listDependencyAsset;
        private List<string> m_listDependencyScatteredAsset;

        public CDependencyData()
        {
            m_listDependencyResource = new List<CResource>();
            m_listDependencyAsset = new List<CAsset>();
            m_listDependencyScatteredAsset = new List<string>();
        }

        public void AddDependencyAsset(CAsset asset)
        {
            m_listDependencyAsset.Add(asset);
            if (m_listDependencyResource.Contains(asset.m_Resource) == false)
            {
                m_listDependencyResource.Add(asset.m_Resource);
            }
        }

        public void AddDependencyScatteredAsset(string assetName)
        {
            m_listDependencyScatteredAsset.Add(assetName);
        }

        public void RefreshData()
        {
            m_listDependencyAsset.Sort((a, b) => { return a.Name.CompareTo(b.Name); });
            m_listDependencyResource.Sort((a, b) => { return a.FullName.CompareTo(b.FullName); });
            m_listDependencyScatteredAsset.Sort();
        }

        public List<CResource> GetDependencyResource()
        {
            return m_listDependencyResource;
        }

        public List<CAsset> GetDependencyAsset()
        {
            return m_listDependencyAsset;
        }

        public List<string> GetDependencyScatteredAsset()
        {
            return m_listDependencyScatteredAsset;
        }
    }

    public sealed class CStamp
    {
        public readonly string m_szHostAssetName;
        public readonly string m_szDependencyAssetName;

        public CStamp(string szHostAssetName, string szDependencyAssetName)
        {
            m_szHostAssetName = szHostAssetName;
            m_szDependencyAssetName = szDependencyAssetName;
        }
    }

    public sealed class CResourceAnalyzer
    {
        private readonly CResourceCollection m_resourceCollection;
        private readonly Dictionary<string, CDependencyData> m_mapDependencyData;
        private readonly Dictionary<string, List<CAsset>> m_mapScatteredAsset;
        private readonly List<string[]> m_listCircularDependency;
        private readonly HashSet<CStamp> m_setAnalyzedStamps;

        public CResourceAnalyzer(CResourceCollection resourceCollection)
        {
            m_resourceCollection = resourceCollection == null ? new CResourceCollection() : resourceCollection;
            m_mapDependencyData = new Dictionary<string, CDependencyData>();
            m_mapScatteredAsset = new Dictionary<string, List<CAsset>>();
            m_listCircularDependency = new List<string[]>();
            m_setAnalyzedStamps = new HashSet<CStamp>();
        }

        public void Clear()
        {
            m_resourceCollection.Clear();
            m_mapDependencyData.Clear();
            m_mapScatteredAsset.Clear();
            m_listCircularDependency.Clear();
            m_setAnalyzedStamps.Clear();
        }

        public bool Prepare()
        {
            m_resourceCollection.Clear();
            return m_resourceCollection.Load();
        }

        public void Analyze()
        {
            m_mapDependencyData.Clear();
            m_mapScatteredAsset.Clear();
            m_listCircularDependency.Clear();
            m_setAnalyzedStamps.Clear();


            HashSet<string> setScriptAssetNames = _GetFilterAssetNames("t:Script");
            CAsset[] arrCollectionAssets = m_resourceCollection.GetAllAssets();
            foreach (CAsset asset in arrCollectionAssets)
            {
                string szAssetName = asset.Name;
                CDependencyData dependencyData = new CDependencyData();
                _AnalyzeAsset(szAssetName, asset, dependencyData, setScriptAssetNames);
                dependencyData.RefreshData();
                m_mapDependencyData.Add(szAssetName, dependencyData);
            }

            foreach (var listAsset in m_mapScatteredAsset.Values)
            {
                listAsset.Sort((a, b) => { return a.Name.CompareTo(b.Name); });
            }
        }

        private void _AnalyzeAsset(string a_szAssetName, CAsset a_hostAsset, CDependencyData a_dependencyData, HashSet<string> a_setScriptAssetName)
        {
            string[] arrDependcyAssetNames = AssetDatabase.GetDependencies(a_szAssetName, false);
            foreach (string szDependcyAssetName in arrDependcyAssetNames)
            {
                if (a_setScriptAssetName.Contains(szDependcyAssetName))
                {
                    continue;
                }
                if (szDependcyAssetName == a_szAssetName)
                {
                    continue;
                }
                if (szDependcyAssetName.EndsWith(".Unity", System.StringComparison.Ordinal))
                {
                    continue;
                }

                CStamp stamp = new CStamp(a_hostAsset.Name, szDependcyAssetName);
                if (m_setAnalyzedStamps.Contains(stamp))
                {
                    continue;
                }
                m_setAnalyzedStamps.Add(stamp);

                string szGuid = AssetDatabase.AssetPathToGUID(szDependcyAssetName);
                if (string.IsNullOrEmpty(szGuid))
                {
                    continue;
                }

                CAsset asset = m_resourceCollection.GetAsset(szGuid);
                if (asset != null)
                {
                    a_dependencyData.AddDependencyAsset(asset);
                }
                else
                {
                    a_dependencyData.AddDependencyScatteredAsset(szDependcyAssetName);
                    List<CAsset> list = new List<CAsset>();
                    if (!m_mapScatteredAsset.TryGetValue(szDependcyAssetName, out list))
                    {
                        list = new List<CAsset>();
                        m_mapScatteredAsset.Add(szDependcyAssetName, list);
                    }
                    list.Add(a_hostAsset);
                    _AnalyzeAsset(szDependcyAssetName, a_hostAsset, a_dependencyData, a_setScriptAssetName);
                }
            }
        }

        private HashSet<string> _GetFilterAssetNames(string a_szFilter)
        {
            string[] arrAssetGuidss = AssetDatabase.FindAssets(a_szFilter);
            HashSet<string> setAssetNames = new HashSet<string>();
            foreach (string guid in arrAssetGuidss)
            {
                setAssetNames.Add(AssetDatabase.GUIDToAssetPath(guid));
            }
            return setAssetNames;
        }

        public CAsset GetAsset(string a_szAssetName)
        {
            string szGuid = AssetDatabase.AssetPathToGUID(a_szAssetName);
            return m_resourceCollection.GetAsset(szGuid);
        }

        public string[] GetAssetNames(string a_szFilter)
        {
            HashSet<string> setAssetNames = _GetFilterAssetNames(a_szFilter);
            var filterResult = m_mapDependencyData.Where((pair) => { return setAssetNames.Contains(pair.Key); });
            var orederResult = filterResult.OrderBy((pair) => { return pair.Key; });
            return orederResult.Select((pair) => { return pair.Key; }).ToArray();
        }

        public string[] GetScatteredAssetNames(string a_szFilter)
        {
            HashSet<string> setAssetNames = _GetFilterAssetNames(a_szFilter);
            var filterResult = m_mapScatteredAsset.Where((pair) => { return setAssetNames.Contains(pair.Key); });
            var orederResult = filterResult.OrderBy((pair) => { return pair.Key; });
            return orederResult.Select((pair) => { return pair.Key; }).ToArray();
        }

        public CAsset[] GetScatteredHostAssets(string a_szScatteredAssetName)
        {
            List<CAsset> listHostAssets = new List<CAsset>();
            List<CAsset> list;
            if (m_mapScatteredAsset.TryGetValue(a_szScatteredAssetName, out list))
            {
                listHostAssets.AddRange(list);
            }
            return listHostAssets.ToArray();
        }

        public CDependencyData GetDependencyData(string a_szAssetName)
        {
            CDependencyData dependencyData = null;
            m_mapDependencyData.TryGetValue(a_szAssetName, out dependencyData);
            return dependencyData;
        }
    }
}