using System;
using System.Collections.Generic;
using System.Text;

namespace XFrameworkBase
{
    public static partial class Utility
    {
        public static class Text
        {
            public static string Format(string a_szFormat, params object[] a_args)
            {
                return string.Format(a_szFormat, a_args);
            }
        }

        public static class Path
        {
            public static string GetRegularPath(string a_szPath)
            {
                if (string.IsNullOrEmpty(a_szPath))
                    return string.Empty;
                return a_szPath.Replace('\\', '/');
            }

            /// <summary>
            /// 获取远程格式的路径（带有file:// 或 http:// 前缀）。
            /// </summary>
            /// <param name="path">原始路径。</param>
            /// <returns>远程格式路径。</returns>
            public static string GetRemotePath(string path)
            {
                string regularPath = GetRegularPath(path);
                if (regularPath == null)
                {
                    return null;
                }

                return regularPath.Contains("://") ? regularPath : ("file:///" + regularPath).Replace("file:////", "file:///");
            }
        }

    }
}
