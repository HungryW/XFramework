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
        }
    }
}
