using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace XFrameWork.Editor
{
    public sealed partial class CResourceCollection
    {
        private SortedDictionary<string, CResource> m_mapResources;
        private SortedDictionary<string, CAsset> m_mapAssets;

        private readonly string m_szConfigPath;

        public CResourceCollection()
        {
            m_szConfigPath = XFrameworkBase.Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "Framwork/Config/ResourceCollection.xml"));

            m_mapResources = new SortedDictionary<string, CResource>();
            m_mapAssets = new SortedDictionary<string, CAsset>();
        }

        public void Clear()
        {
            m_mapResources.Clear();
            m_mapAssets.Clear();
        }

        public bool Load()
        {
            Clear();
            if (!File.Exists(m_szConfigPath))
            {
                return false;
            }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(m_szConfigPath);
                XmlNode xmlRoot = xmlDocument.SelectSingleNode("ResourceCollection");
                XmlNode xmlResources = xmlRoot.SelectSingleNode("Resources");
                XmlNode xmlAsset = xmlRoot.SelectSingleNode("Assets");

                foreach (XmlNode xmlNode in xmlResources.ChildNodes)
                {
                    if (xmlNode.Name != "Resource")
                    {
                        continue;
                    }

                    XmlElement xmlElement = (XmlElement)xmlNode;
                    string szName = xmlElement.GetAttribute("Name");
                    string szVariant = xmlElement.GetAttribute("Variant");
                    string szLoadType = xmlElement.GetAttribute("LoadType");
                    string szPacked = xmlElement.GetAttribute("Packed");

                    ELoadType eLoadType = (ELoadType)Enum.Parse(typeof(ELoadType), szLoadType);
                    bool bPacked = bool.Parse(szPacked);

                    AddResource(szName, szVariant, eLoadType, bPacked);
                }

                foreach (XmlNode xmlNode in xmlAsset.ChildNodes)
                {
                    if (xmlNode.Name != "Asset")
                    {
                        continue;
                    }
                    XmlElement xmlElement = (XmlElement)xmlNode;
                    string szGuid = xmlElement.GetAttribute("Guid");
                    string szResourceName = xmlElement.GetAttribute("ResourceName");
                    string szVariant = xmlElement.GetAttribute("Variant");
                    AssignAsset(szGuid, szResourceName, szVariant);
                }
                return true;
            }
            catch
            {
                File.Delete(m_szConfigPath);
                return false;
            }
        }

        public bool Save()
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "utf-8", null));
                XmlElement xmlRoot = xmlDocument.CreateElement("ResourceCollection");
                xmlDocument.AppendChild(xmlRoot);

                XmlElement xmlResources = xmlDocument.CreateElement("Resources");
                xmlRoot.AppendChild(xmlResources);

                XmlElement xmlAssets = xmlDocument.CreateElement("Assets");
                xmlRoot.AppendChild(xmlAssets);

                foreach (CResource resource in m_mapResources.Values)
                {
                    XmlElement xmlResource = xmlDocument.CreateElement("Resource");
                    xmlResource.SetAttribute("Name", resource.m_szName);
                    xmlResource.SetAttribute("Variant", resource.m_szVariant);
                    xmlResource.SetAttribute("LoadType", resource.m_eLoadType.ToString());
                    xmlResource.SetAttribute("Packed", resource.m_bPacked.ToString());
                    xmlResources.AppendChild(xmlResource);
                }

                foreach (CAsset asset in m_mapAssets.Values)
                {
                    XmlElement xmlAsset = xmlDocument.CreateElement("Asset");
                    xmlAsset.SetAttribute("Guid", asset.m_szGuid);
                    xmlAssets.SetAttribute("ResourceName", asset.m_Resource.m_szName);
                    xmlAsset.SetAttribute("Variant", asset.m_Resource.m_szVariant);
                    xmlAsset.AppendChild(xmlAsset);
                }

                string szDirPath = Path.GetDirectoryName(m_szConfigPath);
                if (!Directory.Exists(szDirPath))
                {
                    Directory.CreateDirectory(szDirPath);
                }
                xmlDocument.Save(m_szConfigPath);
                AssetDatabase.Refresh();
                return true;
            }
            catch
            {
                if (File.Exists(m_szConfigPath))
                {
                    File.Delete(m_szConfigPath);
                }
                return false;
            }
        }


        public CResource[] GetAllResources()
        {
            return m_mapResources.Values.ToArray();
        }

        public CResource GetResource(string a_szName, string a_szVariant)
        {
            CResource resource = null;
            m_mapResources.TryGetValue(_GetResourceKey(a_szName, a_szVariant), out resource));
            return resource;
        }

        public bool HasResource(string a_szName, string a_szVariant)
        {
            return GetResource(a_szName, a_szVariant) != null;
        }

        public bool AddResource(string a_szName, string a_szVariant, ELoadType a_eLoadType, bool a_bPacked)
        {
            if (HasResource(a_szName, a_szVariant))
            {
                return false;
            }
            CResource resource = new CResource(a_szName, a_szVariant, a_eLoadType, a_bPacked);
            m_mapResources.Add(resource.FullName.ToLowerInvariant(), resource);
            return true;
        }

        public bool RemoveResource(string a_szName, string a_szVariant)
        {
            CResource resource = GetResource(a_szName, a_szVariant);
            if (null == resource)
            {
                return false;
            }
            List<CAsset> listAsset = resource.GetAllAsset();
            foreach (CAsset asset in listAsset)
            {
                m_mapAssets.Remove(asset.m_szGuid);
            }
            m_mapResources.Remove(resource.FullName.ToLowerInvariant());
            resource.Clear();
            return true;
        }

        public bool SetResourceLoadType(string a_szName, string a_szVariant, ELoadType a_eLoadType)
        {
            CResource resource = GetResource(a_szName, a_szVariant);
            if (null == resource)
            {
                return false;
            }
            if (a_eLoadType == ELoadType.LoadFormBinary && resource.GetAllAsset().Count > 1)
            {
                return false;
            }
            resource.m_eLoadType = a_eLoadType;
            return true;
        }

        public bool SetResourcePacked(string a_szName, string a_szVariant, bool a_bPacked)
        {
            CResource resource = GetResource(a_szName, a_szVariant);
            if (null == resource)
            {
                return false;
            }
            resource.m_bPacked = a_bPacked;
            return true;
        }

        private string _GetResourceKey(string a_szName, string a_szVariant)
        {
            return (a_szVariant == null ? a_szName : string.Format("{0}.{1}", a_szName, a_szVariant)).ToLowerInvariant();
        }

        public CAsset[] GetAllAssets()
        {
            return m_mapAssets.Values.ToArray();
        }

        public CAsset[] GetAssetsInResource(string a_szName, string a_szVariant)
        {
            CResource resource = GetResource(a_szName, a_szVariant);
            if (null == resource)
            {
                return new CAsset[0];
            }
            return resource.GetAllAsset().ToArray();
        }

        public CAsset GetAsset(string a_szGuid)
        {
            CAsset asset = null;
            m_mapAssets.TryGetValue(a_szGuid, out asset);
            return asset;
        }

        public bool HasAsset(string a_szGuid)
        {
            return GetAsset(a_szGuid) != null;
        }

        public bool AssignAsset(string a_szGuid, string a_szName, string a_szVariant)
        {
            CResource resource = GetResource(a_szName, a_szVariant);
            if (null == resource)
            {
                return false;
            }
            string szAssetName = AssetDatabase.GUIDToAssetPath(a_szGuid);
            if (string.IsNullOrEmpty(szAssetName))
            {
                return false;
            }

            if (resource.HasAsset(a_szGuid))
            {
                return false;
            }
            bool bIsScene = szAssetName.EndsWith(".unity");
            if ((bIsScene && resource.m_eAssetType == EAssetType.Asset)
               || (!bIsScene && resource.m_eAssetType == EAssetType.Scene))
            {
                return false;
            }
            CAsset asset = GetAsset(a_szGuid);
            if (resource.IsLoadFromBinary && resource.GetAllAsset().Count > 0 && asset != resource.GetAllAsset()[0])
            {
                return false;
            }
            if (asset == null)
            {
                asset = new CAsset(a_szGuid, resource);
                m_mapAssets.Add(a_szGuid, asset);
            }
            resource.AssignAsset(asset, bIsScene);
            return true;
        }

        public bool UnAssignAsset(string a_szGuid)
        {
            CAsset asset = GetAsset(a_szGuid);
            if (null == asset)
            {
                return false;
            }
            asset.m_Resource.UnAssignAsset(asset);
            m_mapAssets.Remove(a_szGuid);
            return true;
        }
    }
    public sealed class CAsset : IComparable<CAsset>
    {
        public string m_szGuid;
        public CResource m_Resource;

        public CAsset(string szGuid, CResource resource)
        {
            m_szGuid = szGuid;
            m_Resource = resource;
        }

        public CAsset(string szGuid) : this(szGuid, null)
        {

        }

        public string Name
        {
            get
            {
                return AssetDatabase.GUIDToAssetPath(m_szGuid);
            }
        }

        public int CompareTo(CAsset asset)
        {
            return string.Compare(m_szGuid, asset.m_szGuid, StringComparison.Ordinal);
        }
    }

    public enum ELoadType
    {
        LoadFromFile = 0,
        LoadFromMemory,
        LoadFormBinary,
    }

    public enum EAssetType
    {
        Unknow = -1,
        Asset = 0,
        Scene,
    }

    public sealed class CResource
    {
        public string m_szName;
        public string m_szVariant;
        public ELoadType m_eLoadType;
        public bool m_bPacked;
        public EAssetType m_eAssetType;
        private readonly List<CAsset> m_listAssets;

        public CResource(string szName, string szVariant, ELoadType eLoadType, bool bPacked)
        {
            m_szName = szName;
            m_szVariant = szVariant;
            m_eLoadType = eLoadType;
            m_bPacked = bPacked;
            m_eAssetType = EAssetType.Unknow;
            m_listAssets = new List<CAsset>();
        }

        public void Clear()
        {
            m_listAssets.Clear();
        }

        public void ReName(string a_szName, string a_szVariant)
        {
            m_szName = a_szName;
            m_szVariant = a_szVariant;
        }

        public void AssignAsset(CAsset a_asset, bool a_bIsScene)
        {
            if (a_asset.m_Resource != null)
            {
                a_asset.m_Resource.UnAssignAsset(a_asset);
            }
            m_eAssetType = a_bIsScene ? EAssetType.Scene : EAssetType.Asset;
            a_asset.m_Resource = this;
            m_listAssets.Add(a_asset);
            m_listAssets.Sort(_AssetComparer);
        }

        private int _AssetComparer(CAsset a, CAsset b)
        {
            return string.Compare(a.m_szGuid, b.m_szGuid, StringComparison.Ordinal);
        }

        public void UnAssignAsset(CAsset a_asset)
        {
            a_asset.m_Resource = null;
            m_listAssets.Remove(a_asset);
            if (m_listAssets.Count == 0)
            {
                m_eAssetType = EAssetType.Unknow;
            }
        }

        public List<CAsset> GetAllAsset()
        {
            return m_listAssets;
        }

        public bool HasAsset(string a_szGuid)
        {
            return m_listAssets.Exists((a) => { return a.m_szGuid == a_szGuid; });
        }

        public string FullName
        {
            get
            {
                return m_szVariant == null ? m_szName : string.Format("{0}.{1}", m_szName, m_szVariant);
            }
        }

        public bool IsLoadFromBinary
        {
            get
            {
                return m_eLoadType == ELoadType.LoadFormBinary;
            }
        }

    }
}
