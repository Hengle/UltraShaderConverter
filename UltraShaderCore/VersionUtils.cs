using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraShaderCore
{
    public static class VersionUtils
    {
        public static int[] GetVersionArray(AssetsFile file)
        {
            string verStr = file.typeTree.unityVersion;
            return GetVersionArray(verStr);
        }
        public static int[] GetVersionArray(string verStr)
        {
            string[] strSplits = verStr.Split('.', 'f', 'p');
            return new int[]
            {
                int.Parse(strSplits[0]),
                int.Parse(strSplits[1]),
                int.Parse(strSplits[2]),
                int.Parse(strSplits[3])
            };
        }
    }
}
