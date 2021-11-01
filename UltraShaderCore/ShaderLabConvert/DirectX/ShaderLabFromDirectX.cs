using AssetsTools.NET;
using DirectXDisassembler;
using DirectXDisassembler.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraShaderCore;

namespace ShaderLabConvert
{
    public class ShaderLabFromDirectX
    {
        private ShaderReader shaderReader;
        private byte[] shaderBytes;

        private ShaderVariation vertShaderVar;
        private ShaderVariation fragShaderVar;

        private static char[] swizChars = { 'x', 'y', 'z', 'w' };

        public ShaderLabFromDirectX(ShaderReader shaderReader, byte[] shaderBytes, ShaderVariation vertShaderVar, ShaderVariation fragShaderVar)
        {
            this.shaderReader = shaderReader;
            this.shaderBytes = shaderBytes;
            this.vertShaderVar = vertShaderVar;
            this.fragShaderVar = fragShaderVar;
        }

        public string GetShaderString()
        {
            StringBuilder sb = new StringBuilder();
            int depth = 0;

            AssetTypeValueField baseField = shaderReader.baseField;
            AssetTypeValueField m_ParsedForm = baseField.Get("m_ParsedForm");

            string shaderName = m_ParsedForm.Get("m_Name").GetValue().AsString();
            AddLine(sb, depth, $"Shader \"{shaderName}\"");
            AddLine(sb, depth, "{");
            depth++;
            {
                AddLine(sb, depth, "Properties");
                AddLine(sb, depth, "{");
                depth++;
                {
                    AssetTypeValueField m_Props = m_ParsedForm.Get("m_PropInfo").Get("m_Props").Get("Array");
                    int propCount = m_Props.GetValue().AsArray().size;
                    for (int i = 0; i < propCount; i++)
                    {
                        AssetTypeValueField prop = m_Props[i];
                        AddLine(sb, depth, GetShaderPropString(prop));
                    }
                }
                depth--;
                AddLine(sb, depth, "}");

                AssetTypeValueField m_SubShaders = m_ParsedForm.Get("m_SubShaders").Get("Array");
                int subShaderCount = m_SubShaders.GetValue().AsArray().size;
                for (int i = 0; i < subShaderCount; i++)
                {
                    AddLine(sb, depth, "SubShader");
                    AddLine(sb, depth, "{");
                    depth++;
                    {
                        AssetTypeValueField subShader = m_SubShaders[i];

                        AssetTypeValueField m_Tags = subShader.Get("m_Tags").Get("tags").Get("Array");
                        if (m_Tags.GetValue().AsArray().size > 0)
                        {
                            AddLine(sb, depth, GetShaderTagString(m_Tags));
                        }

                        int m_LOD = subShader.Get("m_LOD").GetValue().AsInt();
                        AddLine(sb, depth, $"LOD {m_LOD}");

                        AddLine(sb, depth, "");

                        AssetTypeValueField m_Passes = subShader.Get("m_Passes").Get("Array");
                        int passCount = m_Passes.GetValue().AsArray().size;
                        for (int j = 0; j < passCount; j++)
                        {
                            AddLine(sb, depth, "Pass");
                            AddLine(sb, depth, "{");
                            depth++;
                            {
                                AssetTypeValueField pass = m_Passes[j];
                                sb.AppendLine(GetShaderPassString(depth));
                            }
                            depth--;
                            AddLine(sb, depth, "}");
                        }
                    }
                    depth--;
                    AddLine(sb, depth, "}");
                }
            }
            depth--;
            AddLine(sb, depth, "}");
            return sb.ToString();
        }

        //from AS
        private string GetShaderPropString(AssetTypeValueField prop)
        {
            StringBuilder sb = new StringBuilder();

            AssetTypeValueField m_Attributes = prop.Get("m_Attributes").Get("Array");
            int attributeCount = m_Attributes.GetValue().AsArray().size;
            for (int j = 0; j < attributeCount; j++)
            {
                string attribute = m_Attributes[j].GetValue().AsString();
                sb.Append($"[{attribute}] ");
            }

            string m_Name = prop.Get("m_Name").GetValue().AsString();
            string m_Description = prop.Get("m_Description").GetValue().AsString();
            sb.Append($"{m_Name} (\"{m_Description}\", ");

            SerializedPropertyType m_Type = (SerializedPropertyType)prop.Get("m_Type").GetValue().AsInt();

            float[] m_DefValue = new float[4];
            for (int j = 0; j < 4; j++)
            {
                m_DefValue[j] = prop.Get($"m_DefValue[{j}]").GetValue().AsFloat();
            }

            AssetTypeValueField m_DefTexture = prop.Get("m_DefTexture");
            string m_DefaultName = m_DefTexture.Get("m_DefaultName").GetValue().AsString();
            TextureDimension m_TexDim = (TextureDimension)m_DefTexture.Get("m_TexDim").GetValue().AsInt();

            switch (m_Type)
            {
                case SerializedPropertyType.Color:
                    sb.Append("Color");
                    break;
                case SerializedPropertyType.Vector:
                    sb.Append("Vector");
                    break;
                case SerializedPropertyType.Float:
                    sb.Append("Float");
                    break;
                case SerializedPropertyType.Range:
                    sb.Append($"Range({m_DefValue[1]}, {m_DefValue[2]})");
                    break;
                case SerializedPropertyType.Texture:
                    switch (m_TexDim)
                    {
                        case TextureDimension.TexDimAny:
                            sb.Append("any");
                            break;
                        case TextureDimension.TexDim2D:
                            sb.Append("2D");
                            break;
                        case TextureDimension.TexDim3D:
                            sb.Append("3D");
                            break;
                        case TextureDimension.TexDimCUBE:
                            sb.Append("Cube");
                            break;
                        case TextureDimension.TexDim2DArray:
                            sb.Append("2DArray");
                            break;
                        case TextureDimension.TexDimCubeArray:
                            sb.Append("CubeArray");
                            break;
                    }
                    break;
            }
            sb.Append(") = ");
            switch (m_Type)
            {
                case SerializedPropertyType.Color:
                case SerializedPropertyType.Vector:
                    sb.Append($"({m_DefValue[0]},{m_DefValue[1]},{m_DefValue[2]},{m_DefValue[3]})");
                    break;
                case SerializedPropertyType.Float:
                case SerializedPropertyType.Range:
                    sb.Append(m_DefValue[0]);
                    break;
                case SerializedPropertyType.Texture:
                    sb.Append($"\"{m_DefaultName}\" {{ }}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return sb.ToString();
        }

        private string GetShaderTagString(AssetTypeValueField m_Tags)
        {
            StringBuilder sb = new StringBuilder();

            int tagCount = m_Tags.GetValue().AsArray().size;
            sb.Append("Tags { ");
            for (int i = 0; i < tagCount; i++)
            {
                AssetTypeValueField tag = m_Tags[i];
                string first = tag[0].GetValue().AsString();
                string second = tag[1].GetValue().AsString();
                sb.Append($"\"{first}\" = \"{second}\" ");
            }
            sb.Append("}");
            return sb.ToString();
        }
        //////////////////////

        private string GetShaderPassString(int depth)
        {
            StringBuilder sb = new StringBuilder();

            AddLine(sb, depth, "CGPROGRAM");

            AddLine(sb, depth, "#pragma vertex vert");
            AddLine(sb, depth, "#pragma fragment frag");

            AddLine(sb, depth, "");

            AddLine(sb, depth, "#include \"UnityCG.cginc\"");

            ShaderData vertData = shaderReader.GetShaderData(vertShaderVar, shaderBytes);
            ShaderData fragData = shaderReader.GetShaderData(fragShaderVar, shaderBytes);
            CompiledShader dxVertShader = shaderReader.GetDirectXShader(vertData);
            CompiledShader dxFragShader = shaderReader.GetDirectXShader(fragData);

            OSGN vertOsgn = dxVertShader.Osgn;

            // v2f struct (frag input)
            AddLine(sb, depth, "struct v2f");
            AddLine(sb, depth, "{");
            depth++;
            {
                foreach (OSGN.Output output in vertOsgn.outputs)
                {
                    string format = ((FormatType)output.format).ToString() + GetMaskSize(output.mask);
                    string type = output.name + output.index;
                    string name = GetOSGNOutputName(output);

                    AddLine(sb, depth, $"{format} {name} : {type};");
                }
            }
            depth--;
            AddLine(sb, depth, "};");

            AddLine(sb, depth, "");

            // $Globals ConstantBuffer
            ShaderConstantBuffer vertGlobalsCb = vertData.shaderMeta.constantBuffers.FirstOrDefault(b => b.name == "$Globals");
            if (vertGlobalsCb != null)
            {
                foreach (ShaderConstantBufferParam param in vertGlobalsCb.cbParams)
                {
                    string typeName = GetConstantBufferParamType(param);
                    string name = param.name;

                    AddLine(sb, depth, $"{typeName} {name};");
                }

                AddLine(sb, depth, "");
            }

            ShaderConstantBuffer fragGlobalsCb = fragData.shaderMeta.constantBuffers.FirstOrDefault(b => b.name == "$Globals");
            if (fragGlobalsCb != null)
            {
                foreach (ShaderConstantBufferParam param in fragGlobalsCb.cbParams)
                {
                    string typeName = GetConstantBufferParamType(param);
                    string name = param.name;

                    AddLine(sb, depth, $"{typeName} {name};");
                }

                AddLine(sb, depth, "");
            }

            // Global slots
            bool wasGlobalSlot = false;
            foreach (ShaderSlot slot in vertData.shaderMeta.slots)
            {
                if (slot.type == 0)
                {
                    if (slot.args[1] == 4 || slot.args[1] == 5)
                    {
                        AddLine(sb, depth, $"sampler2D {slot.name};");
                        wasGlobalSlot = true;
                    }
                    //idk for 6 and 8
                }
            }

            foreach (ShaderSlot slot in fragData.shaderMeta.slots)
            {
                if (slot.type == 0)
                {
                    if (slot.args[1] == 4 || slot.args[1] == 5)
                    {
                        AddLine(sb, depth, $"sampler2D {slot.name};");
                        wasGlobalSlot = true;
                    }
                    //idk for 6 and 8
                }
            }

            if (wasGlobalSlot)
                AddLine(sb, depth, "");

            AddLine(sb, depth, "v2f vert(appdata_full v)");
            AddLine(sb, depth, "{");
            depth++;
            {
                AddLine(sb, depth, "v2f o;");
                sb.Append(FlipMatrices(vertData, dxVertShader, true, depth));
                ShaderLabFromDirectXPass progDec = new ShaderLabFromDirectXPass(vertData, dxVertShader, vertShaderVar);
                sb.Append(progDec.DecompileProgram(depth));
                AddLine(sb, depth, "return o;");
            }
            depth--;
            AddLine(sb, depth, "}");

            AddLine(sb, depth, "fixed4 frag(v2f i) : SV_Target");
            AddLine(sb, depth, "{");
            depth++;
            {
                AddLine(sb, depth, "fixed4 output0;");
                sb.Append(FlipMatrices(fragData, dxFragShader, true, depth));
                ShaderLabFromDirectXPass progDec = new ShaderLabFromDirectXPass(fragData, dxFragShader, fragShaderVar);
                sb.Append(progDec.DecompileProgram(depth));
                AddLine(sb, depth, "return output0;");
            }
            depth--;
            AddLine(sb, depth, "}");

            AddText(sb, depth, "ENDCG");

            return sb.ToString();
        }

        private int GetMaskSize(byte mask)
        {
            int p = 0;
            for (int i = 0; i < 4; i++)
            {
                if (((mask >> i) & 1) == 1)
                {
                    p++;
                }
            }
            return p;
        }

        private string GetConstantBufferParamType(ShaderConstantBufferParam param)
        {
            string paramType = "unknownType";

            if (param.rowCount == 1)
            {
                if (param.columnCount == 1)
                    paramType = "float";
                if (param.columnCount == 2)
                    paramType = "float2";
                if (param.columnCount == 3)
                    paramType = "float3";
                if (param.columnCount == 4)
                    paramType = "float4";
            }
            else if (param.rowCount == 4)
            {
                if (param.columnCount == 4 && param.isMatrix == 1)
                    paramType = "float4x4";
            }

            return paramType;
        }

        private string GetISGNInputName(ISGN.Input input)
        {
            string type;
            if (input.index > 0)
                type = input.name + input.index;
            else
                type = input.name;

            string name;
            switch (input.name)
            {
                case "SV_POSITION":
                    name = "position";
                    break;
                case "POSITION":
                    name = "vertex";
                    break;
                default:
                    name = type.ToLower();
                    break;
            }
            return name;
        }

        private string GetOSGNOutputName(OSGN.Output output)
        {
            string type;
            if (output.index > 0)
                type = output.name + output.index;
            else
                type = output.name;

            string name;
            switch (output.name)
            {
                case "SV_POSITION":
                    name = "position";
                    break;
                case "POSITION":
                    name = "vertex";
                    break;
                default:
                    name = type.ToLower();
                    break;
            }
            return name;
        }

        // Decompiler stuff

        private string FlipMatrices(ShaderData shaderData, CompiledShader dxShader, bool isVertex, int depth)
        {
            StringBuilder sb = new StringBuilder();

            foreach (ShaderConstantBuffer constantBuffer in shaderData.shaderMeta.constantBuffers)
            {
                foreach (ShaderConstantBufferParam param in constantBuffer.cbParams)
                {
                    if (param.isMatrix > 0)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            AddLine(sb, depth, $"fixed4 _flip_{param.name}_{i};");
                            for (int j = 0; j < 4; j++)
                            {
                                AddLine(sb, depth, $"_flip_{param.name}_{i}.{swizChars[j]} = {param.name}[{j}].{swizChars[i]};");
                            }
                        }
                    }
                }
            }

            return sb.ToString();
        }

        private void AddText(StringBuilder sb, int depth, string text)
        {
            sb.Append(new string(' ', depth * 4));
            sb.Append(text);
        }

        private void AddLine(StringBuilder sb, int depth, string text)
        {
            sb.Append(new string(' ', depth * 4));
            sb.AppendLine(text);
        }
    }
}
