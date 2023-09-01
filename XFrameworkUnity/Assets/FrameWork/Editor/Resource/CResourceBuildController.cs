using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using XFrameworkBase;

namespace XFrameWork.Editor
{
    public sealed partial class CResourceBuildController
    {
        private const string ms_szRemoteVersionFileName = "RemoteVersion.dat";
        private const string ms_szLocalVersionFileName = "LocalVersion.dat";
        private const string ms_szExtension = "dat";
        private static readonly int ms_nAssetStrLen = "Assets".Length;

        private readonly string m_szConfigPath;
        private readonly CResourceCollection m_ResourceCollection;
        private readonly CResourceAnalyzer m_ResourceAnalyzer;
        private readonly SortedDictionary<string, CResourceData> m_mapResourceDatas;

        public int m_nInternalResourceVersion;
        public EPlatform m_ePlatform;
        public EABCompressType m_eABCompressType;
        public bool m_bAdditionalCompressSelected;
        public string m_szCompressHelperTypeName;
        public bool m_bForceRebuildSelected;
        public string m_szBuildEventHandlerTypeName;
        public string m_szOutputDirectory;
        public bool m_bOutputPackageSelected;
        public bool m_bOutputFullSelected;
        public bool m_bOutputPackedSelected;


        public CResourceBuildController()
        {
            m_szConfigPath = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "Framwork/Config/ResourceBuildConfig.xml"));

            m_ResourceCollection = new CResourceCollection();
            m_ResourceAnalyzer = new CResourceAnalyzer(m_ResourceCollection);
            m_mapResourceDatas = new SortedDictionary<string, CResourceData>();


            m_ePlatform = EPlatform.Undefined;
            m_eABCompressType = EABCompressType.LZ4;
            m_szCompressHelperTypeName = string.Empty;
            m_bAdditionalCompressSelected = false;
            m_bForceRebuildSelected = false;
            m_szBuildEventHandlerTypeName = string.Empty;
            m_szOutputDirectory = string.Empty;
            m_bOutputFullSelected = m_bOutputPackageSelected = m_bOutputPackedSelected = true;
        }

        public bool IsVaildOutputDir
        {
            get
            {
                return Directory.Exists(m_szOutputDirectory);
            }
        }
        public string AppGameVersion
        {
            get
            {
                return Application.version;
            }
        }

        public string WorkingPath
        {
            get
            {
                DirectoryInfo dir = new DirectoryInfo(Utility.Text.Format("{0}/Working/", m_szOutputDirectory));
                return Utility.Path.GetRegularPath(dir.FullName);
            }
        }

        public string OutputPackagePath
        {
            get
            {
                return _GetOutputSubPath("Package");
            }
        }

        public string OutputFullPath
        {
            get
            {
                return _GetOutputSubPath("Full");
            }
        }

        public string OutputPackedPath
        {
            get
            {
                return _GetOutputSubPath("Packed");
            }
        }

        private string _GetOutputSubPath(string a_szSubType)
        {
            string szAppVersion = AppGameVersion.Replace('.', '_');
            DirectoryInfo dir = new DirectoryInfo(Utility.Text.Format("{0}/{1}/{2}_{3}", m_szOutputDirectory, a_szSubType, szAppVersion, m_nInternalResourceVersion));
            return Utility.Path.GetRegularPath(dir.FullName);
        }

        public bool IsPlatformSelected(EPlatform a_ePlatform)
        {
            return (m_ePlatform & a_ePlatform) != 0;
        }

        public bool Load()
        {
            if (!File.Exists(m_szConfigPath))
            {
                return false;
            }
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(m_szConfigPath);
                XmlNode xmlRoot = xmlDocument.SelectSingleNode("ResourceBuild");
                XmlNode xmlSetting = xmlRoot.SelectSingleNode("Settings");

                XmlNodeList xmlNodeList = xmlSetting.ChildNodes;
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    XmlNode xmlNode = xmlNodeList.Item(i);
                    switch (xmlNode.Name)
                    {
                        case "InternalResourceVersion":
                            {
                                m_nInternalResourceVersion = int.Parse(xmlNode.InnerText);
                                break;
                            }
                        case "Platform":
                            {
                                m_ePlatform = (EPlatform)int.Parse(xmlNode.InnerText);
                                break;
                            }
                        case "ABCompressType":
                            {
                                m_eABCompressType = (EABCompressType)int.Parse(xmlNode.InnerText);
                                break;
                            }
                        case "CompressHelperTypeName":
                            {
                                m_szCompressHelperTypeName = xmlNode.InnerText;
                                break;
                            }
                        case "AdditionalCompressSelected":
                            {
                                m_bAdditionalCompressSelected = bool.Parse(xmlNode.InnerText);
                                break;
                            }
                        case "ForceRebuildSelected":
                            {
                                m_bForceRebuildSelected = bool.Parse(xmlNode.InnerText);
                                break;
                            }
                        case "BuildEventHandlerTypeName":
                            {
                                m_szBuildEventHandlerTypeName = xmlNode.InnerText;
                                break;
                            }
                        case "OutputDirectory":
                            {
                                m_szOutputDirectory = xmlNode.InnerText;
                                break;
                            }
                        case "OutputPackageSelected":
                            {
                                m_bOutputPackageSelected = bool.Parse(xmlNode.InnerText);
                                break;
                            }
                        case "OutputFullSelected":
                            {
                                m_bOutputFullSelected = bool.Parse(xmlNode.InnerText);
                                break;
                            }
                        case "OutputPackedSelected":
                            {
                                m_bOutputPackedSelected = bool.Parse(xmlNode.InnerText);
                                break;
                            }
                    }

                }
            }
            catch
            {
                File.Delete(m_szConfigPath);
                return false;
            }

            return true;
        }

        public bool Save()
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDocument.AppendChild(xmlDeclaration);

                XmlElement xmlElementRoot = xmlDocument.CreateElement("ResourceBuild");
                xmlDeclaration.AppendChild(xmlElementRoot);

                XmlElement xmlSetting = xmlDocument.CreateElement("Settings");
                xmlElementRoot.AppendChild(xmlSetting);

                XmlElement xmlElement = null;

                xmlElement = xmlDocument.CreateElement("InternalResourceVersion");
                xmlElement.InnerText = m_nInternalResourceVersion.ToString();
                xmlSetting.AppendChild(xmlElement);

                xmlElement = xmlDocument.CreateElement("Platform");
                xmlElement.InnerText = ((int)m_ePlatform).ToString();
                xmlSetting.AppendChild(xmlElement);

                xmlElement = xmlDocument.CreateElement("ABCompressType");
                xmlElement.InnerText = ((int)m_eABCompressType).ToString();
                xmlSetting.AppendChild(xmlElement);

                xmlElement = xmlDocument.CreateElement("CompressHelperTypeName");
                xmlElement.InnerText = m_szCompressHelperTypeName;
                xmlSetting.AppendChild(xmlElement);

                xmlElement = xmlDocument.CreateElement("AdditionalCompressSelected");
                xmlElement.InnerText = m_bAdditionalCompressSelected.ToString();
                xmlSetting.AppendChild(xmlElement);

                xmlElement = xmlDocument.CreateElement("ForceRebuildSelected");
                xmlElement.InnerText = m_bForceRebuildSelected.ToString();
                xmlSetting.AppendChild(xmlElement);

                xmlElement = xmlDocument.CreateElement("BuildEventHandlerTypeName");
                xmlElement.InnerText = m_szBuildEventHandlerTypeName;
                xmlSetting.AppendChild(xmlElement);

                xmlElement = xmlDocument.CreateElement("OutputDirectory");
                xmlElement.InnerText = m_szOutputDirectory;
                xmlSetting.AppendChild(xmlElement);

                xmlElement = xmlDocument.CreateElement("OutputPackageSelected");
                xmlElement.InnerText = m_bOutputPackageSelected.ToString();
                xmlSetting.AppendChild(xmlElement);

                xmlElement = xmlDocument.CreateElement("OutputFullSelected");
                xmlElement.InnerText = m_bOutputFullSelected.ToString();
                xmlSetting.AppendChild(xmlElement);

                xmlElement = xmlDocument.CreateElement("OutputPackedSelected");
                xmlElement.InnerText = m_bOutputPackedSelected.ToString();
                xmlSetting.AppendChild(xmlElement);

                string szDirName = Path.GetDirectoryName(m_szConfigPath);
                if (!Directory.Exists(szDirName))
                {
                    Directory.CreateDirectory(szDirName);
                }
                xmlDocument.Save(m_szConfigPath);
                AssetDatabase.Refresh();

            }
            catch
            {
                File.Delete(m_szConfigPath);
                return false;
            }
            return true;
        }
    }

    public sealed partial class CResourceBuildController
    {
        public bool BuildResources()
        {
            if (!IsVaildOutputDir)
            {
                return false;
            }
            _InitDir();

            if (!m_ResourceCollection.Load())
            {
                return false;
            }
            m_ResourceAnalyzer.Analyze();

            AssetBundleBuild[] arrABData = null;
            CResourceData[] arrABResourceData = null;
            CResourceData[] arrBinResourceData = null;
            if (!_PrepareBuildData(out arrABData, out arrABResourceData, out arrBinResourceData))
            {
                return false;
            }

            BuildAssetBundleOptions buildAssetBundleOptions = _GetBuildABOptions();
        }

        private bool _BuildResources(EPlatform a_ePlatform, BuildAssetBundleOptions a_options, AssetBundleBuild[] a_arrABData, CResourceData[] a_arrABResource, CResourceData[] a_arrBinResource)
        {
            if (!IsPlatformSelected(a_ePlatform))
            {
                return true;
            }

            string szPlatformName = a_ePlatform.ToString();

            string szWorkingPath = Utility.Text.Format("{0}{1}/", WorkingPath, szPlatformName);

            string szOutputPackagePath = Utility.Text.Format("{0}{1}/", OutputPackagePath, szPlatformName);
            if (m_bOutputPackageSelected)
            {
                Directory.CreateDirectory(szOutputPackagePath);
            }

            string szOutptFullPath = Utility.Text.Format("{0}{1}/", OutputFullPath, szPlatformName);   
            if(m_bOutputFullSelected)
            {
                Directory.CreateDirectory(szOutptFullPath);
            }

            string szOutputPackedPath = Utility.Text.Format("{0}{1}/", OutputPackedPath, szPlatformName);
            if(m_bOutputPackedSelected)
            {
                Directory.CreateDirectory(szOutputPackedPath);
            } 

        }

        private bool _PrepareBuildData(out AssetBundleBuild[] a_outArrABData, out CResourceData[] a_outArrABResourceData, out CResourceData[] a_outArrBinResourceData)
        {
            a_outArrABData = null;
            a_outArrABResourceData = null;
            a_outArrBinResourceData = null;
            m_mapResourceDatas.Clear();

            CResource[] arrAllRes = m_ResourceCollection.GetAllResources();
            foreach (var res in arrAllRes)
            {
                m_mapResourceDatas.Add(res.FullName, new CResourceData(res.m_szName, res.m_szVariant, res.m_eLoadType, res.m_bPacked));
            }

            CAsset[] arrAllAsset = m_ResourceCollection.GetAllAssets();
            foreach (var asset in arrAllAsset)
            {
                string szAssetFileFullPath = Application.dataPath.Substring(0, Application.dataPath.Length - ms_nAssetStrLen) + asset.Name;
                if (!File.Exists(szAssetFileFullPath))
                {
                    return false;
                }

                byte[] arrAssetData = File.ReadAllBytes(szAssetFileFullPath);
                int nHash = Utility.Verifier.GetCrc32(arrAssetData);

                List<string> listDependAssetName = new List<string>();
                CDependencyData dependencyData = m_ResourceAnalyzer.GetDependencyData(asset.Name);
                List<CAsset> arrDependAsset = dependencyData.GetDependencyAsset();
                foreach (var dependAsset in arrDependAsset)
                {
                    listDependAssetName.Add(dependAsset.Name);
                }
                listDependAssetName.Sort();
                m_mapResourceDatas[asset.m_Resource.FullName].AddAssetData(asset.m_szGuid, asset.Name, arrAssetData.Length, nHash, listDependAssetName.ToArray());
            }

            List<AssetBundleBuild> listABData = new List<AssetBundleBuild>();
            List<CResourceData> listABResourceData = new List<CResourceData>();
            List<CResourceData> listBinResoureceData = new List<CResourceData>();

            foreach (CResourceData resData in m_mapResourceDatas.Values)
            {
                if (resData.GetAllAssets().Length <= 0)
                {
                    return false;
                }
                if (resData.IsLoadFromBinary())
                {
                    listBinResoureceData.Add(resData);
                }
                else
                {
                    listABResourceData.Add(resData);

                    AssetBundleBuild ab = new AssetBundleBuild();
                    ab.assetBundleName = resData.m_szName;
                    ab.assetBundleVariant = resData.m_szVariant;
                    ab.assetNames = resData.GetAllAssetNames();
                    listABData.Add(ab);
                }
            }

            a_outArrABData = listABData.ToArray();
            a_outArrABResourceData = listABResourceData.ToArray();
            a_outArrBinResourceData = listBinResoureceData.ToArray();
            return true;
        }

        private void _InitDir()
        {

            if (Directory.Exists(OutputPackagePath))
            {
                Directory.Delete(OutputPackagePath, true);
            }
            Directory.CreateDirectory(OutputPackagePath);

            if (Directory.Exists(OutputFullPath))
            {
                Directory.Delete(OutputFullPath, true);
            }
            Directory.CreateDirectory(OutputFullPath);

            if (Directory.Exists(OutputPackedPath))
            {
                Directory.Delete(OutputPackedPath, true);
            }
            Directory.CreateDirectory(OutputPackedPath);
        }

        private BuildAssetBundleOptions _GetBuildABOptions()
        {
            BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.DeterministicAssetBundle;
            if (m_bForceRebuildSelected)
            {
                buildAssetBundleOptions |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            }

            if (m_eABCompressType == EABCompressType.Uncrompress)
            {
                buildAssetBundleOptions |= BuildAssetBundleOptions.UncompressedAssetBundle;
            }
            else if (m_eABCompressType == EABCompressType.LZ4)
            {
                buildAssetBundleOptions |= BuildAssetBundleOptions.ChunkBasedCompression;
            }
            return buildAssetBundleOptions;
        }
    }

    public enum EABCompressType
    {
        Uncrompress = 0,
        LZ4 = 1,
        LZMA = 2,
    }
    public enum EPlatform
    {
        Undefined = 0,
        Windows64 = 1 << 0,
        Android = 1 << 1,
        IOS = 1 << 2,
        WebGL = 1 << 3,
    }
    public sealed partial class CResourceBuildController
    {
        private sealed class CResourceBaseInfo
        {
            public readonly EPlatform m_ePlatform;
            public readonly int m_nLen;
            public readonly int m_nHash;
            public readonly int m_nCompressLen;
            public readonly int m_nCompressHash;

            public CResourceBaseInfo(EPlatform ePlatform, int nLen, int nHash, int nCompressLen, int nCompressHash)
            {
                m_ePlatform = ePlatform;
                m_nLen = nLen;
                m_nHash = nHash;
                m_nCompressLen = nCompressLen;
                m_nCompressHash = nCompressHash;
            }
        }

        private sealed class CAssetData
        {
            public readonly string m_szGuid;
            public readonly string m_szName;
            public readonly int m_nLen;
            public readonly int m_nHash;
            public readonly string[] m_arrDependAssetNames;

            public CAssetData(string szGuid, string szName, int nLen, int nHash, string[] arrDependAssetNames)
            {
                m_szGuid = szGuid;
                m_szName = szName;
                m_nLen = nLen;
                m_nHash = nHash;
                m_arrDependAssetNames = arrDependAssetNames;
            }
        }

        private sealed class CResourceData
        {
            public readonly string m_szName;
            public readonly string m_szVariant;
            public readonly ELoadType m_eLoadType;
            public readonly bool m_bPacked;
            private readonly List<CAssetData> m_listAssetData;
            private readonly List<CResourceBaseInfo> m_listResourceBaseInfos;

            public CResourceData(string szName, string szVariant, ELoadType eLoadType, bool bPacked)
            {
                m_szName = szName;
                m_szVariant = szVariant;
                m_eLoadType = eLoadType;
                m_bPacked = bPacked;
                m_listAssetData = new List<CAssetData>();
                m_listResourceBaseInfos = new List<CResourceBaseInfo>();
            }

            public void AddAssetData(string a_szGuid, string a_szName, int a_nLen, int a_nHash, string[] a_arrDependAssetName)
            {
                m_listAssetData.Add(new CAssetData(a_szGuid, a_szName, a_nLen, a_nHash, a_arrDependAssetName));
            }

            public bool IsLoadFromBinary()
            {
                return m_eLoadType == ELoadType.LoadFormBinary;
            }

            public string[] GetAllAssetGuids()
            {
                string[] arrGuids = new string[m_listAssetData.Count];
                for (int i = 0; i < arrGuids.Length; i++)
                {
                    arrGuids[i] = m_listAssetData[i].m_szGuid;
                }
                return arrGuids;
            }

            public CAssetData[] GetAllAssets()
            {
                return m_listAssetData.ToArray();
            }

            public CAssetData GetAssetDataByName(string a_szAssetName)
            {
                foreach (var asset in m_listAssetData)
                {
                    if (asset.m_szName == a_szAssetName)
                    {
                        return asset;
                    }
                }
                return null;
            }

            public void AddCode(EPlatform a_ePlatform, int a_nLen, int a_nHash, int a_nCompressLen, int a_nCompressHash)
            {
                m_listResourceBaseInfos.Add(new CResourceBaseInfo(a_ePlatform, a_nLen, a_nHash, a_nCompressLen, a_nCompressHash));
            }

            public CResourceBaseInfo GetBaseInfoByPlatform(EPlatform a_ePlatform)
            {
                foreach (var baseInfo in m_listResourceBaseInfos)
                {
                    if (baseInfo.m_ePlatform == a_ePlatform)
                    {
                        return baseInfo;
                    }
                }
                return null;
            }

            public CResourceBaseInfo[] GetAllBaseInfo()
            {
                return m_listResourceBaseInfos.ToArray();
            }

            public string[] GetAllAssetNames()
            {
                string[] assetNames = new string[m_listAssetData.Count];
                for (int i = 0; i < assetNames.Length; i++)
                {
                    assetNames[i] = m_listAssetData[i].m_szName;
                }
                return assetNames;
            }
        }
    }
}
