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
                                sb.AppendLine(GetShaderPassString(pass, depth));
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

        private string GetShaderPassString(AssetTypeValueField pass, int depth)
        {
            StringBuilder sb = new StringBuilder();

            AddLine(sb, depth, "CGPROGRAM");

            // Get the Pass's DX data since we can't trust fields anymore
            bool usesVertex = pass.Get("progVertex").Get("m_SubPrograms").Get("Array").GetValue().AsArray().size > 0;
            bool usesFragment = pass.Get("progFragment").Get("m_SubPrograms").Get("Array").GetValue().AsArray().size > 0;
            //bool usesGeometry = pass.Get("progGeometry").Get("m_SubPrograms").Get("Array").GetValue().AsArray().size > 0;
            //bool usesHull = pass.Get("progHull").Get("m_SubPrograms").Get("Array").GetValue().AsArray().size > 0;
            //bool usesDomain = pass.Get("progDomain").Get("m_SubPrograms").Get("Array").GetValue().AsArray().size > 0;
            //bool usesRayTracing = pass.Get("progRayTracing").Get("m_SubPrograms").Get("Array").GetValue().AsArray().size > 0;

            if (usesVertex)
                AddLine(sb, depth, "#pragma vertex vert");
            if (usesFragment)
                AddLine(sb, depth, "#pragma fragment frag");
            //if (usesGeometry)
            //    AddLine(sb, depth, "#pragma geometry geo"); // TODO
            //if (usesHull)
            //    AddLine(sb, depth, "#pragma hull hul"); // TODO
            //if (usesDomain)
            //    AddLine(sb, depth, "#pragma domain dom"); // TODO
            //if (usesRayTracing)
            //    AddLine(sb, depth, "#pragma raytracing raytrace"); // TODO

            AddLine(sb, depth, "");

            AddLine(sb, depth, "#include \"UnityCG.cginc\"");

            //// TEMPORARY!!!!
            //List<ShaderVariation> shaderVars = shaderReader.GetShaderVariations(ShaderPlatform.Direct3D11);
            //ShaderVariation vertVar = shaderVars.First(sv => sv.keywords.Count == 0 && sv.type == ShaderType.Direct3D11_Vertex_40);
            //ShaderVariation fragVar = shaderVars.First(sv => sv.keywords.Count == 0 && sv.type == ShaderType.Direct3D11_Pixel_40);
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
                sb.Append(DecompileShader(vertData, dxVertShader, true, depth));
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
                sb.Append(DecompileShader(fragData, dxFragShader, false, depth));
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

        private string DecompileShader(ShaderData shaderData, CompiledShader dxShader, bool isVertex, int depth)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder lsb = new StringBuilder();

            SHDR shdr = dxShader.Shdr;
            List<SHDRInstruction> instructions = shdr.shaderInstructions;

            int startDepth = depth;
            bool unitySpecCube1DefinedYet = false;

            foreach (SHDRInstruction inst in instructions)
            {
                // Get amount of temporary variables and write them
                if (inst.opcode == Opcode.dcl_temps)
                {
                    int tempCount = inst.declData.numTemps;
                    for (int i = 0; i < tempCount; i++)
                        AddLine(lsb, depth, $"fixed4 temp{i};");
                }

                // Other declarations are not important to us
                if (SHDRInstruction.IsDeclaration(inst.opcode))
                    continue;

                string prefix = "";
                string suffix = "";

                if (inst.saturated)
                {
                    prefix += "saturate(";
                    suffix += ")";
                }

                switch (inst.opcode)
                {
                    case Opcode.add:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        OperandInfo op1 = GetOperandDisplay(shaderData, dxShader, inst.operands[1], isVertex);
                        OperandInfo op2 = GetOperandDisplay(shaderData, dxShader, inst.operands[2], isVertex);
                        string op0Str = op0.GetDisplayString();
                        string op1Str = op1.GetDisplayString(op0.Swizzle);
                        string op2Str = op2.GetDisplayString(op0.Swizzle);
                        AddLine(sb, depth, $"{op0Str} = {prefix}{op1Str} + {op2Str}{suffix};");
                        break;
                    }
                    case Opcode.mul:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        OperandInfo op1 = GetOperandDisplay(shaderData, dxShader, inst.operands[1], isVertex);
                        OperandInfo op2 = GetOperandDisplay(shaderData, dxShader, inst.operands[2], isVertex);
                        string op0Str = op0.GetDisplayString();
                        string op1Str = op1.GetDisplayString(op0.Swizzle);
                        string op2Str = op2.GetDisplayString(op0.Swizzle);
                        AddLine(sb, depth, $"{op0Str} = {prefix}{op1Str} * {op2Str}{suffix};");
                        break;
                    }
                    case Opcode.div:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        OperandInfo op1 = GetOperandDisplay(shaderData, dxShader, inst.operands[1], isVertex);
                        OperandInfo op2 = GetOperandDisplay(shaderData, dxShader, inst.operands[2], isVertex);
                        string op0Str = op0.GetDisplayString();
                        string op1Str = op1.GetDisplayString(op0.Swizzle);
                        string op2Str = op2.GetDisplayString(op0.Swizzle);
                        AddLine(sb, depth, $"{op0Str} = {prefix}{op1Str} / {op2Str}{suffix};");
                        break;
                    }
                    case Opcode.and:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        OperandInfo op1 = GetOperandDisplay(shaderData, dxShader, inst.operands[1], isVertex);
                        OperandInfo op2 = GetOperandDisplay(shaderData, dxShader, inst.operands[2], isVertex);
                        string op0Str = op0.GetDisplayString();
                        string op1Str = op1.GetDisplayString(op0.Swizzle);
                        string op2Str = op2.GetDisplayString(op0.Swizzle);

                        // Ignore & 0x3f800000
                        if (inst.operands[2].immValues.Length > 0 && inst.operands[2].immValues[0] == 1)
                        {
                            break;
                        }

                        // HACK: Remove decimal point (should only be XXX.0 but you never know)
                        if (op2Str.Contains("."))
                        {
                            op2Str = op2Str.Substring(0, op2Str.IndexOf('.'));
                        }

                        int swizSize = inst.operands[1].swizzle.Length;
                        string swizSuffix = swizSize > 1 ? swizSize.ToString() : ""; //uint, uint2, uint3, etc.
                        AddLine(sb, depth, $"{op0Str} = {prefix}uint{swizSuffix}({op1Str}) & {op2Str}{suffix};");
                        break;
                    }
                    case Opcode.dp2:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        OperandInfo op1 = GetOperandDisplay(shaderData, dxShader, inst.operands[1], isVertex);
                        OperandInfo op2 = GetOperandDisplay(shaderData, dxShader, inst.operands[2], isVertex);
                        int[] mask = new int[] { 0, 1 };
                        string op0Str = op0.GetDisplayString();
                        string op1Str = op1.GetDisplayString(mask);
                        string op2Str = op2.GetDisplayString(mask);
                        AddLine(sb, depth, $"{op0Str} = {prefix}dot({op1Str}, {op2Str}){suffix};");
                        break;
                    }
                    case Opcode.dp3:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        OperandInfo op1 = GetOperandDisplay(shaderData, dxShader, inst.operands[1], isVertex);
                        OperandInfo op2 = GetOperandDisplay(shaderData, dxShader, inst.operands[2], isVertex);
                        int[] mask = new int[] { 0, 1, 2 };
                        string op0Str = op0.GetDisplayString();
                        string op1Str = op1.GetDisplayString(mask);
                        string op2Str = op2.GetDisplayString(mask);
                        AddLine(sb, depth, $"{op0Str} = {prefix}dot({op1Str}, {op2Str}){suffix};");
                        break;
                    }
                    case Opcode.dp4:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        OperandInfo op1 = GetOperandDisplay(shaderData, dxShader, inst.operands[1], isVertex);
                        OperandInfo op2 = GetOperandDisplay(shaderData, dxShader, inst.operands[2], isVertex);
                        int[] mask = new int[] { 0, 1, 2, 3 };
                        string op0Str = op0.GetDisplayString();
                        string op1Str = op1.GetDisplayString(mask);
                        string op2Str = op2.GetDisplayString(mask);
                        AddLine(sb, depth, $"{op0Str} = {prefix}dot({op1Str}, {op2Str}){suffix};");
                        break;
                    }
                    // TODO
                    case Opcode.min:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        OperandInfo op1 = GetOperandDisplay(shaderData, dxShader, inst.operands[1], isVertex);
                        OperandInfo op2 = GetOperandDisplay(shaderData, dxShader, inst.operands[2], isVertex);
                        string op0Str = op0.GetDisplayString();
                        string op1Str = op1.GetDisplayString();
                        string op2Str = op2.GetDisplayString();
                        AddLine(sb, depth, $"{op0Str} = {prefix}min({op1Str}, {op2Str}){suffix};");
                        break;
                    }
                    case Opcode.max:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        OperandInfo op1 = GetOperandDisplay(shaderData, dxShader, inst.operands[1], isVertex);
                        OperandInfo op2 = GetOperandDisplay(shaderData, dxShader, inst.operands[2], isVertex);
                        string op0Str = op0.GetDisplayString();
                        string op1Str = op1.GetDisplayString();
                        string op2Str = op2.GetDisplayString();
                        AddLine(sb, depth, $"{op0Str} = {prefix}max({op1Str}, {op2Str}){suffix};");
                        break;
                    }
                    case Opcode.mad:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        OperandInfo op1 = GetOperandDisplay(shaderData, dxShader, inst.operands[1], isVertex);
                        OperandInfo op2 = GetOperandDisplay(shaderData, dxShader, inst.operands[2], isVertex);
                        OperandInfo op3 = GetOperandDisplay(shaderData, dxShader, inst.operands[3], isVertex);
                        string op0Str = op0.GetDisplayString();
                        string op1Str = op1.GetDisplayString(op0.Swizzle);
                        string op2Str = op2.GetDisplayString(op0.Swizzle);
                        string op3Str = op3.GetDisplayString(op0.Swizzle);
                        AddLine(sb, depth, $"{op0Str} = {prefix}{op1Str} * {op2Str} + {op3Str}{suffix};");
                        break;
                    }
                    case Opcode.sqrt:
                    {
                        // ????? not two args?
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        string op0Str = op0.GetDisplayString();
                        AddLine(sb, depth, $"{op0Str} = {prefix}sqrt({op0Str}){suffix};");
                        break;
                    }
                    case Opcode.rsq:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        string op0Str = op0.GetDisplayString();
                        AddLine(sb, depth, $"{op0Str} = {prefix}rsqrt({op0Str}){suffix};");
                        break;
                    }
                    case Opcode.mov:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        OperandInfo op1 = GetOperandDisplay(shaderData, dxShader, inst.operands[1], isVertex);
                        string op0Str = op0.GetDisplayString();
                        string op1Str = op1.GetDisplayString(op0.Swizzle);
                        AddLine(sb, depth, $"{op0Str} = {prefix}{op1Str}{suffix};");
                        break;
                    }
                    case Opcode.movc:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        OperandInfo op1 = GetOperandDisplay(shaderData, dxShader, inst.operands[1], isVertex);
                        OperandInfo op2 = GetOperandDisplay(shaderData, dxShader, inst.operands[2], isVertex);
                        OperandInfo op3 = GetOperandDisplay(shaderData, dxShader, inst.operands[3], isVertex);
                        string op0Str = op0.GetDisplayString();
                        string op1Str = op1.GetDisplayString(op0.Swizzle);
                        string op2Str = op2.GetDisplayString(op0.Swizzle);
                        string op3Str = op3.GetDisplayString(op0.Swizzle);
                        AddLine(sb, depth, $"{op0Str} = {prefix}{op1Str} ? {op2Str} : {op3Str}{suffix};");
                        break;
                    }
                    case Opcode.eq:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        OperandInfo op1 = GetOperandDisplay(shaderData, dxShader, inst.operands[1], isVertex);
                        OperandInfo op2 = GetOperandDisplay(shaderData, dxShader, inst.operands[2], isVertex);
                        string op0Str = op0.GetDisplayString();
                        string op1Str = op1.GetDisplayString();
                        string op2Str = op2.GetDisplayString();
                        AddLine(sb, depth, $"{op0Str} = {op1Str} == {op2Str};");
                        break;
                    }
                    case Opcode.ne:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        OperandInfo op1 = GetOperandDisplay(shaderData, dxShader, inst.operands[1], isVertex);
                        OperandInfo op2 = GetOperandDisplay(shaderData, dxShader, inst.operands[2], isVertex);
                        string op0Str = op0.GetDisplayString();
                        string op1Str = op1.GetDisplayString();
                        string op2Str = op2.GetDisplayString();
                        AddLine(sb, depth, $"{op0Str} = {op1Str} != {op2Str};");
                        break;
                    }
                    case Opcode.lt:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        OperandInfo op1 = GetOperandDisplay(shaderData, dxShader, inst.operands[1], isVertex);
                        OperandInfo op2 = GetOperandDisplay(shaderData, dxShader, inst.operands[2], isVertex);
                        string op0Str = op0.GetDisplayString();
                        string op1Str = op1.GetDisplayString();
                        string op2Str = op2.GetDisplayString();
                        AddLine(sb, depth, $"{op0Str} = {op1Str} < {op2Str};");
                        break;
                    }
                    case Opcode.ge:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        OperandInfo op1 = GetOperandDisplay(shaderData, dxShader, inst.operands[1], isVertex);
                        OperandInfo op2 = GetOperandDisplay(shaderData, dxShader, inst.operands[2], isVertex);
                        string op0Str = op0.GetDisplayString();
                        string op1Str = op1.GetDisplayString();
                        string op2Str = op2.GetDisplayString();
                        AddLine(sb, depth, $"{op0Str} = {op1Str} >= {op2Str};");
                        break;
                    }
                    case Opcode.@if:
                    {
                        int testType = (inst.instData & 0x40000) >> 18;
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        string op0Str = op0.GetDisplayString();

                        if (testType == 0)
                            AddLine(sb, depth, $"if (!{op0Str})");
                        else if (testType == 1)
                            AddLine(sb, depth, $"if ({op0Str})");

                        AddLine(sb, depth, "{");
                        depth++;

                        break;
                    }
                    case Opcode.@else:
                    {
                        depth--;
                        AddLine(sb, depth, "}");
                        AddLine(sb, depth, "else");
                        AddLine(sb, depth, "{");
                        depth++;

                        break;
                    }
                    case Opcode.endif:
                    {
                        depth--;
                        AddLine(sb, depth, "}");

                        break;
                    }
                    case Opcode.sample:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        OperandInfo op2 = GetOperandDisplay(shaderData, dxShader, inst.operands[2], isVertex);
                        OperandInfo op1 = GetOperandDisplay(shaderData, dxShader, inst.operands[1], isVertex);
                        string op0Str = op0.GetDisplayString();
                        string op1Str = op1.GetDisplayString();
                        string op2Str = op2.GetDisplayString();
                        // Bad hack since unity_ProbeVolumeSH won't work normally by tex3D
                        // I think this is the only one out of the box, but theoretically
                        // anyone could make a UNITY_DECLARE_TEX3D_FLOAT(blah)
                        if (op2Str == "unity_ProbeVolumeSH")
                            AddLine(sb, depth, $"{op0Str} = UNITY_SAMPLE_TEX3D_SAMPLER({op2Str}, {op2Str}, {op1Str});");
                        else
                            AddLine(sb, depth, $"{op0Str} = tex2D({op2Str}, {op1Str});");
                        break;
                    }
                    case Opcode.sample_l:
                    {
                        OperandInfo op0 = GetOperandDisplay(shaderData, dxShader, inst.operands[0], isVertex);
                        OperandInfo op2 = GetOperandDisplay(shaderData, dxShader, inst.operands[2], isVertex);
                        OperandInfo op1 = GetOperandDisplay(shaderData, dxShader, inst.operands[1], isVertex);
                        OperandInfo op4 = GetOperandDisplay(shaderData, dxShader, inst.operands[4], isVertex);
                        string op0Str = op0.GetDisplayString();
                        string op1Str = op1.GetDisplayString();
                        string op2Str = op2.GetDisplayString();
                        string op4Str = op4.GetDisplayString();
                        if (op2Str == "unity_SpecCube0" || op2Str == "unity_SpecCube1")
                            AddLine(sb, depth, $"{op0Str} = UNITY_SAMPLE_TEXCUBE_LOD({op2Str}, {op1Str}, {op4Str});");
                        else
                            AddLine(sb, depth, $"{op0Str} = tex3D({op2Str}, {op1Str});");

                        if (op2Str == "unity_SpecCube1" && !unitySpecCube1DefinedYet)
                        {
                            // Terrible hack since the UNITY_SAMPLE_TEXCUBE macro requires samplerunity_SpecCube1 to exist
                            // normally it's passed in as samplerunity_SpecCube0 with UNITY_PASS_TEXCUBE but SAMPLE_TEXCUBE
                            // doesn't support this.
                            unitySpecCube1DefinedYet = true;
                            AddLine(lsb, startDepth, "SamplerState samplerunity_SpecCube1 = samplerunity_SpecCube0;");
                        }

                        break;
                    }
                    case Opcode.ret:
                    {
                        // Do nothing
                        break;
                    }
                    default:
                    {
                        AddLine(sb, depth, $"undefinedop_{inst.opcode}();");
                        break;
                    }
                }
            }

            return lsb.ToString() + sb.ToString();
        }

        private OperandInfo GetOperandDisplay(ShaderData shaderData, CompiledShader dxShader, SHDRInstructionOperand op, bool isVertex)
        {
            string bodyText = GetOperandBody(shaderData, dxShader, op, isVertex);
            return new OperandInfo(op, bodyText);
        }

        // Modified from disassembler
        private string GetOperandBody(ShaderData shaderData, CompiledShader dxShader, SHDRInstructionOperand op, bool isVertex)
        {
            switch (op.operand)
            {
                case Operand.ConstantBuffer:
                {
                    int cbSlotIdx = op.arraySizes[0];
                    int cbArrIdx = op.arraySizes[1];

                    // This is really jank and probably will cause issues
                    // but I really hope it doesn't
                    int minSwiz = op.swizzle.Min();
                    int maxSwiz = op.swizzle.Max();

                    int opCbStart = cbArrIdx * 16 + minSwiz * 4;
                    int opCbEnd = cbArrIdx * 16 + maxSwiz * 4 + 4;

                    // Figure out what constant buffer param this cb[?].???? belongs to

                    ShaderConstantBufferParam opParam = null;

                    ShaderSlot slot = shaderData.shaderMeta.slots.First(s => s.slotIdx == cbSlotIdx && s.type == 1);
                    ShaderConstantBuffer constantBuffer = shaderData.shaderMeta.constantBuffers.First(b => b.name == slot.name);
                    foreach (ShaderConstantBufferParam param in constantBuffer.cbParams)
                    {
                        int paramCbStart = param.pos;
                        int paramCbSize = param.rowCount * param.columnCount * 4;
                        int paramCbEnd = paramCbStart + paramCbSize;

                        if (opCbStart >= paramCbStart && opCbEnd <= paramCbEnd)
                        {
                            opParam = param;
                            break;
                        }
                    }

                    if (opParam == null)
                        return $"cb{cbSlotIdx}[{cbArrIdx}]"; // Fallback

                    string opParamName = opParam.name;
                    int opParamIdx = -1;

                    if (opParam.isMatrix > 0)
                    {
                        opParamIdx = (opCbStart - opParam.pos) / 16;
                    }

                    for (int i = 0; i < op.swizzle.Length; i++)
                    {
                        int swizAddr = cbArrIdx * 16 + op.swizzle[i] * 4;
                        int swizOff = swizAddr - opParam.pos;
                        // Stupid idea but it makes my life easier
                        op.swizzle[i] = swizOff / 4 % 4;
                    }

                    if (opParamIdx == -1)
                        return opParamName;
                    else
                        return $"_flip_{opParamName}_{opParamIdx}";
                    //return $"{prefix}cb{op.arraySizes[0]}[{op.arraySizes[1]}]";
                }
                case Operand.Input:
                {
                    // Search by first swizzle letter
                    int searchMask = (op.swizzle.Length != 0) ? (1 << op.swizzle[0]) : 0;
                    ISGN.Input input = dxShader.Isgn.inputs.First(
                        o => o.register == op.arraySizes[0] && ((searchMask & o.mask) == searchMask)
                    );

                    if (isVertex)
                    {
                        return $"v.{GetISGNInputName(input)}";
                    }
                    else
                    {
                        return $"i.{GetISGNInputName(input)}";
                    }
                    //return $"{prefix}v{op.arraySizes[0]}";
                }
                case Operand.Output:
                {
                    if (isVertex)
                    {
                        // Search by first swizzle letter
                        int searchMask = (op.swizzle.Length != 0) ? (1 << op.swizzle[0]) : 0;
                        OSGN.Output output = dxShader.Osgn.outputs.First(
                            o => o.register == op.arraySizes[0] && ((searchMask & o.mask) == searchMask)
                        );

                        // Move swizzles (for example, .zw -> .xy) based on first letter
                        int maskTest = output.mask;
                        int moveCount = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            if ((maskTest & 1) == 1)
                                break;
                            moveCount++;
                            maskTest >>= 1;
                        }
                        for (int i = 0; i < op.swizzle.Length; i++)
                        {
                            op.swizzle[i] -= moveCount;
                        }

                        return $"o.{GetOSGNOutputName(output)}";
                    }
                    else
                    {
                        // Sometimes we have multiple outputs and I have
                        // no idea how unity figures out which are which
                        return $"output{op.arraySizes[0]}";
                    }
                    //return $"{prefix}o{op.arraySizes[0]}";
                }
                case Operand.Immediate32:
                {
                    if (op.immValues.Length == 1)
                        return $"{FormatFloat((float)op.immValues[0])}";
                    if (op.immValues.Length == 4)
                        return $"fixed4({FormatFloat((float)op.immValues[0])}, {FormatFloat((float)op.immValues[1])}, {FormatFloat((float)op.immValues[2])}, {FormatFloat((float)op.immValues[3])})";
                    else
                        return $"()"; // ?
                }
                case Operand.Temp:
                {
                    if (op.arraySizes.Length > 0)
                        return $"temp{op.arraySizes[0]}";
                    else
                        return $"temp0";
                }
                case Operand.Resource:
                {
                    ShaderSlot texSlot = shaderData.shaderMeta.slots.First(
                        s => s.type == 0 && s.slotIdx == op.arraySizes[0]
                    );
                    return texSlot.name;
                    //return $"{prefix}t{op.arraySizes[0]}";
                }
                case Operand.Sampler:
                {
                    ShaderSlot sampSlot = shaderData.shaderMeta.slots.First(
                        s => s.type == 0 && s.args[0] == op.arraySizes[0]
                    );
                    return sampSlot.name;
                    //return $"{prefix}s{op.arraySizes[0]}";
                }
                default:
                    return op.operand.ToString(); //haven't added this operand yet
            }
        }

        private string FormatFloat(float f)
        {
            string s = f.ToString();
            if (!s.Contains(".") && !s.Contains("E"))
                s += ".0";
            return s;
        }

        ///////////////////

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

        public class OperandInfo
        {
            public SHDRInstructionOperand op;
            public string body;
            public int[] Swizzle => op.swizzle;
            public bool MustDisplaySwizzle
            {
                get
                {
                    return Swizzle != null && Swizzle.Length != 0 && !(
                        Swizzle.Length == 4 &&
                        Swizzle[0] == 0 &&
                        Swizzle[1] == 1 &&
                        Swizzle[2] == 2 &&
                        Swizzle[3] == 3
                    );
                }
            }

            public OperandInfo(SHDRInstructionOperand op, string body)
            {
                this.op = op;
                this.body = body;
            }

            public string GetDisplayString(int[] targetSwizzle = null)
            {
                string prefix = "";
                string suffix = "";
                if (op.extended)
                {
                    if (((op.extendedData & 0x40) >> 6) == 1)
                    {
                        prefix += "-";
                    }
                    if (((op.extendedData & 0x80) >> 7) == 1)
                    {
                        prefix += "abs(";
                        suffix += ")";
                    }
                }

                if (MustDisplaySwizzle)
                {
                    string swizStr = "";
                    if (targetSwizzle != null && (targetSwizzle.Length == 3 && Swizzle.Length == 4))
                    {
                        for (int i = 0; i < targetSwizzle.Length; i++)
                        {
                            swizStr += swizChars[Swizzle[targetSwizzle[i]]];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < Swizzle.Length; i++)
                        {
                            swizStr += swizChars[Swizzle[i]];
                        }
                    }

                    return prefix + body + "." + swizStr + suffix;
                }
                else
                {
                    return prefix + body + suffix;
                }
            }
        }
    }
}
