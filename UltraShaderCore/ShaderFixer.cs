using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraShaderCore
{
    public static class ShaderFixer
    {
        // Convert compressedBlob into a ByteArray
        public static AssetTypeValueField GetByteArrayShader(AssetsManager am, AssetsFileInstance file, AssetFileInfoEx info)
        {
            AssetTypeTemplateField textureTemp = am.GetTemplateBaseField(file.file, info);
            AssetTypeTemplateField compressedBlob = textureTemp.children.FirstOrDefault(f => f.name == "compressedBlob").children[0];
            if (compressedBlob == null)
                return null;
            compressedBlob.valueType = EnumValueTypes.ByteArray;
            AssetsFileReader reader = file.file.reader;
            AssetTypeInstance textureTypeInstance = new AssetTypeInstance(new[] { textureTemp }, reader, info.absoluteFilePos);
            return textureTypeInstance.GetBaseField();
        }
    }
}
