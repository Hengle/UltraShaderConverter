using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraShaderCore
{
    public class ShaderVariation
    {
        public AssetTypeValueField subProgram;
        public int passIdx;
        public ShaderPlatform platform;
        public ShaderType type;
        public List<string> keywords;
        public Dictionary<int, string> namesReversed;

        public ShaderVariation(AssetTypeValueField subProgram, int passIdx, ShaderPlatform platform, ShaderType type, List<string> keywords, Dictionary<int, string> namesReversed)
        {
            this.subProgram = subProgram;
            this.passIdx = passIdx;
            this.platform = platform;
            this.type = type;
            this.keywords = keywords;
            this.namesReversed = namesReversed;
        }
    }
}
