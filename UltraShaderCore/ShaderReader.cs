using AssetsTools.NET;
using AssetsTools.NET.Extra.Decompressors.LZ4;
using DirectXDisassembler;
using System;
using System.Collections.Generic;
using System.IO;

namespace UltraShaderCore
{
    public class ShaderReader
    {
        private AssetsFile assetsFile;

        public string name;
        public AssetTypeValueField baseField;

        public ShaderReader(AssetsFile assetsFile, AssetTypeValueField baseField)
        {
            this.assetsFile = assetsFile;
            this.baseField = baseField;
            name = baseField.Get("m_ParsedForm").Get("m_Name").GetValue().AsString();
        }

        public ShaderData GetShaderData(ShaderVariation variation, byte[] shaderByteData)
        {
            ShaderStruct shaderStruct;
            using (MemoryStream ms = new MemoryStream(shaderByteData))
            using (AssetsFileReader r = new AssetsFileReader(ms))
            {
                shaderStruct = new ShaderStruct(assetsFile, r);
            }

            uint m_BlobIndex = variation.subProgram.Get("m_BlobIndex").GetValue().AsUInt();

            return shaderStruct.datas[(int)m_BlobIndex];
        }

        public CompiledShader GetDirectXShader(ShaderData shaderData)
        {
            byte[] shaderSPData = shaderData.shaderBytes;
            byte[] shaderSPDataClipped;
            using (MemoryStream ms = new MemoryStream(shaderSPData))
            using (BinaryReader r = new BinaryReader(ms))
            {
                r.BaseStream.Position = 0x18;
                int clipSize = r.ReadInt32();
                r.BaseStream.Position = 0;
                shaderSPDataClipped = r.ReadBytes(clipSize);
            }

            File.WriteAllBytes("use_dd_shader.dat", shaderSPDataClipped);

            return new CompiledShader(new MemoryStream(shaderSPDataClipped));
        }

        public List<ShaderVariation> GetShaderVariations(ShaderPlatform platform)
        {
            List<ShaderVariation> variations = new List<ShaderVariation>();

            AssetTypeValueField m_SubShaders = baseField.Get("m_ParsedForm").Get("m_SubShaders").Get("Array");
            for (int i = 0; i < m_SubShaders.GetValue().AsArray().size; i++)
            {
                AssetTypeValueField subShader = m_SubShaders[i];
                AssetTypeValueField m_Passes = subShader.Get("m_Passes").Get("Array");
                for (int j = 0; j < m_Passes.GetValue().AsArray().size; j++)
                {
                    AssetTypeValueField pass = m_Passes[j];
                    AssetTypeValueField m_NameIndices = pass.Get("m_NameIndices").Get("Array");
                    Dictionary<int, string> m_NameIndicesReversed = GetNameReverseLookup(m_NameIndices);

                    AssetTypeValueField progVertex = pass.Get("progVertex").Get("m_SubPrograms").Get("Array");
                    AssetTypeValueField progFragment = pass.Get("progFragment").Get("m_SubPrograms").Get("Array");
                    AssetTypeValueField progGeometry = pass.Get("progGeometry").Get("m_SubPrograms").Get("Array");
                    AssetTypeValueField progHull = pass.Get("progHull").Get("m_SubPrograms").Get("Array");
                    AssetTypeValueField progDomain = pass.Get("progDomain").Get("m_SubPrograms").Get("Array");

                    variations.AddRange(GetPassShaderVariations(platform, progVertex, m_NameIndicesReversed));
                    variations.AddRange(GetPassShaderVariations(platform, progFragment, m_NameIndicesReversed));
                    variations.AddRange(GetPassShaderVariations(platform, progGeometry, m_NameIndicesReversed));
                    variations.AddRange(GetPassShaderVariations(platform, progHull, m_NameIndicesReversed));
                    variations.AddRange(GetPassShaderVariations(platform, progDomain, m_NameIndicesReversed));
                }
            }

            return variations;
        }

        private bool IsShaderTypePlatform(ShaderPlatform platform, ShaderType type)
        {
            switch (type)
            {
                case ShaderType.OpenGL:
                    return platform == ShaderPlatform.OpenGL;

                case ShaderType.OpenGLES:
                    return platform == ShaderPlatform.OpenGLES || platform == ShaderPlatform.OpenGLESPC;

                case ShaderType.OpenGLES31AEP:
                case ShaderType.OpenGLES31:
                case ShaderType.OpenGLES3:
                    return platform == ShaderPlatform.OpenGLES3;

                case ShaderType.Direct3D11_Vertex_40:
                case ShaderType.Direct3D11_Vertex_50:
                case ShaderType.Direct3D11_Pixel_40:
                case ShaderType.Direct3D11_Pixel_50:
                case ShaderType.Direct3D11_Geometry_40:
                case ShaderType.Direct3D11_Geometry_50:
                case ShaderType.Direct3D11_Hull_50:
                case ShaderType.Direct3D11_Domain_50:
                    return platform == ShaderPlatform.Direct3D11;

                default:
                    return false;
            }
        }

        private List<ShaderVariation> GetPassShaderVariations(ShaderPlatform platform, AssetTypeValueField subPrograms, Dictionary<int, string> namesReversed)
        {
            int[] version = VersionUtils.GetVersionArray(assetsFile);

            List<ShaderVariation> variations = new List<ShaderVariation>();

            int subProgramSize = subPrograms.GetValue().AsArray().size;
            for (int i = 0; i < subProgramSize; i++)
            {
                AssetTypeValueField subProgram = subPrograms[i];

                int m_GpuProgramType = subProgram.Get("m_GpuProgramType").GetValue().AsInt();
                ShaderType shaderType = (ShaderType)m_GpuProgramType;
                if (!IsShaderTypePlatform(platform, shaderType))
                    continue;


                List<string> keywords = new List<string>();
                if (version[0] >= 2019)
                {
                    AssetTypeValueField m_GlobalKeywordIndices = subProgram.Get("m_GlobalKeywordIndices").Get("Array");
                    AssetTypeValueField m_LocalKeywordIndices = subProgram.Get("m_LocalKeywordIndices").Get("Array");

                    for (int j = 0; j < m_GlobalKeywordIndices.GetValue().AsArray().size; j++)
                    {
                        string keywordName = namesReversed[(int)m_GlobalKeywordIndices[j].GetValue().AsUInt()];
                        keywords.Add(keywordName);
                    }
                    for (int j = 0; j < m_LocalKeywordIndices.GetValue().AsArray().size; j++)
                    {
                        string keywordName = namesReversed[(int)m_LocalKeywordIndices[j].GetValue().AsUInt()];
                        keywords.Add(keywordName);
                    }
                }
                else
                {
                    AssetTypeValueField m_KeywordIndices = subProgram.Get("m_KeywordIndices").Get("Array");

                    for (int l = 0; l < m_KeywordIndices.GetValue().AsArray().size; l++)
                    {
                        string keywordName = namesReversed[(int)m_KeywordIndices[l].GetValue().AsUInt()];
                        keywords.Add(keywordName);
                    }
                }

                ShaderVariation shaderVariation = new ShaderVariation(subProgram, platform, shaderType, keywords, namesReversed);
                variations.Add(shaderVariation);
            }

            return variations;
        }

        private Dictionary<int, string> GetNameReverseLookup(AssetTypeValueField m_NameIndices)
        {
            Dictionary<int, string> m_NameIndicesReversed = new Dictionary<int, string>();
            for (int i = 0; i < m_NameIndices.GetValue().AsArray().size; i++)
            {
                string first = m_NameIndices[i].Get("first").GetValue().AsString();
                int second = m_NameIndices[i].Get("second").GetValue().AsInt();
                m_NameIndicesReversed[second] = first;
            }
            return m_NameIndicesReversed;
        }

        public byte[] GetDecompressedShaderData(ShaderPlatform platform)
        {
            int[] version = VersionUtils.GetVersionArray(assetsFile);

            AssetTypeValueField platforms = baseField.Get("platforms").Get("Array");
            AssetTypeValueField offsets = baseField.Get("offsets").Get("Array");
            AssetTypeValueField compressedLengths = baseField.Get("compressedLengths").Get("Array");
            AssetTypeValueField decompressedLengths = baseField.Get("decompressedLengths").Get("Array");

            int shaderIndex = GetShaderIndexByPlatform(platform, platforms);
            if (shaderIndex == -1)
            {
                if (platforms.GetValue().AsArray().size > 0)
                {
                    string platName = ((ShaderPlatform)platforms[0].GetValue().AsUInt()).ToString();
                    throw new Exception("Looked for DirectX11 shader but instead found " + platName);
                }
                else
                {
                    throw new Exception("No platforms found");
                }
            }

            uint offset, compressedLength, decompressedLength;
            if (version[0] > 2019 || (version[0] == 2019 && version[1] >= 3))
            {
                offset = offsets[shaderIndex][0][0].GetValue().AsUInt();
                compressedLength = compressedLengths[shaderIndex][0][0].GetValue().AsUInt();
                decompressedLength = decompressedLengths[shaderIndex][0][0].GetValue().AsUInt();
            }
            else
            {
                offset = offsets[shaderIndex].GetValue().AsUInt();
                compressedLength = compressedLengths[shaderIndex].GetValue().AsUInt();
                decompressedLength = decompressedLengths[shaderIndex].GetValue().AsUInt();
            }

            byte[] compShaderByteData = GetByteArrayRegion(baseField.Get("compressedBlob").Get("Array"), offset, compressedLength);
            byte[] decompShaderByteData;

            // WARNING GENSHIN ONLY!!!! DON'T FORGET TO REMOVE THIS LATER
            if (compressedLength == decompressedLength)
            {
                decompShaderByteData = compShaderByteData;
            }
            else
            {
                decompShaderByteData = new byte[decompressedLength];
                using (MemoryStream ms = new MemoryStream(compShaderByteData))
                using (Lz4DecoderStream ds = new Lz4DecoderStream(ms))
                {
                    ds.Read(decompShaderByteData, 0, (int)decompressedLength);
                }
            }

            return decompShaderByteData;
        }

        private byte[] GetByteArrayRegion(AssetTypeValueField field, uint offset, uint length)
        {
            byte[] srcArray = field.GetValue().AsByteArray().data;
            byte[] dstArray = new byte[length];
            Buffer.BlockCopy(srcArray, (int)offset, dstArray, 0, (int)length);
            return dstArray;
        }

        private int GetShaderIndexByPlatform(ShaderPlatform plat, AssetTypeValueField platforms)
        {
            int platformsSize = platforms.GetValue().AsArray().size;
            for (int i = 0; i < platformsSize; i++)
            {
                if ((ShaderPlatform)platforms[i].GetValue().AsUInt() == plat)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
