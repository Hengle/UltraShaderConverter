using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsTools.NET.Extra.Decompressors.LZ4;
using DirectXDisassembler;
using ShaderLabConvert;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UltraShaderCore;

namespace UltraShaderConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Ultra shader converter");

            if (args.Length == 0)
            {
                Console.WriteLine("usc lst [assets file]");
                Console.WriteLine("usc shv [assets file] [path id]");
                Console.WriteLine("usc dis [assets file] [path id] [variation]");
                Console.WriteLine("usc dec [assets file] [path id] [vert variation] [frag variation]");
                return;
            }

            if (args.Length < 2)
            {
                Console.WriteLine("Not enough args");
                return;
            }

            string mode = args[0];

            string file = args[1];
            if (!File.Exists(file))
            {
                Console.WriteLine("Couldn't find that file");
                return;
            }

            AssetsManager am = new AssetsManager();
            am.LoadClassPackage("classdata.tpk");

            AssetsFileInstance fileInst = am.LoadAssetsFile(file, false);
            am.LoadClassDatabaseFromPackage(fileInst.file.typeTree.unityVersion);

            if (mode == "lst")
            {
                foreach (AssetFileInfoEx shaderInf in fileInst.table.GetAssetsOfType(48))
                {
                    AssetTypeValueField shaderBf = ShaderFixer.GetByteArrayShader(am, fileInst, shaderInf);

                    string name = shaderBf.Get("m_ParsedForm").Get("m_Name").GetValue().AsString();
                    long pathId = shaderInf.index;

                    Console.WriteLine($"{name} ({pathId})");
                }
            }
            else if (mode == "shv" || mode == "dis" || mode == "dec")
            {
                if (!long.TryParse(args[2], out long pathId))
                {
                    Console.WriteLine("PathID is not a number");
                    return;
                }

                AssetFileInfoEx shaderInf = fileInst.table.GetAssetInfo(pathId);
                AssetTypeValueField shaderBf = ShaderFixer.GetByteArrayShader(am, fileInst, shaderInf);

                ShaderReader shaderReader = new ShaderReader(fileInst.file, shaderBf);

                Console.WriteLine($"Loaded shader {shaderReader.name}");

                List<ShaderVariation> shaderVars = shaderReader.GetShaderVariations(ShaderPlatform.Direct3D11);

                if (mode == "shv")
                {
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Not enough args");
                        return;
                    }

                    int i = 0;
                    foreach (ShaderVariation shaderVar in shaderVars)
                    {
                        string keywords = string.Join(", ", shaderVar.keywords);
                        if (keywords == string.Empty)
                            keywords = "<No Keywords>";
                        Console.WriteLine($"{i} (Pass {shaderVar.passIdx}): {keywords} {shaderVar.type}");
                        i++;
                    }
                }
                else if (mode == "dis")
                {
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Not enough args");
                        return;
                    }

                    if (!int.TryParse(args[3], out int shaderVarIdx))
                    {
                        Console.WriteLine("Shader variation index is not a number");
                        return;
                    }

                    ShaderVariation shaderVar = shaderVars[shaderVarIdx];
                    byte[] shaderBytes = shaderReader.GetDecompressedShaderData(ShaderPlatform.Direct3D11);

                    File.WriteAllBytes("test.dxshdr", shaderBytes);

                    ShaderData shaderData = shaderReader.GetShaderData(shaderVar, shaderBytes);
                    CompiledShader dxShader = shaderReader.GetDirectXShader(shaderData);
                    CompiledShaderDisassembler dxDis = new CompiledShaderDisassembler(shaderData, dxShader, shaderVar);
                    Console.WriteLine(dxDis.Disassemble());
                }
                else if (mode == "dec")
                {
                    if (args.Length < 5)
                    {
                        Console.WriteLine("Not enough args");
                        return;
                    }

                    if (!int.TryParse(args[3], out int vertShaderVarIdx))
                    {
                        Console.WriteLine("Shader variation index is not a number");
                        return;
                    }

                    if (!int.TryParse(args[4], out int fragShaderVarIdx))
                    {
                        Console.WriteLine("Shader variation index is not a number");
                        return;
                    }

                    ShaderVariation vertShaderVar = shaderVars[vertShaderVarIdx];
                    ShaderVariation fragShaderVar = shaderVars[fragShaderVarIdx];
                    byte[] shaderBytes = shaderReader.GetDecompressedShaderData(ShaderPlatform.Direct3D11);

                    File.WriteAllBytes("test.dxshdr", shaderBytes);

                    ShaderLabFromDirectX dxConverter = new ShaderLabFromDirectX(shaderReader, shaderBytes, vertShaderVar, fragShaderVar);
                    Console.WriteLine(dxConverter.GetShaderString());
                }
            }
        }
    }
}
