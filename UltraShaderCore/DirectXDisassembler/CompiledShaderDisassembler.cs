using AssetsTools.NET;
using DirectXDisassembler.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraShaderCore;

namespace DirectXDisassembler
{
    public class CompiledShaderDisassembler
    {
        private ShaderData shaderData;
        private CompiledShader shader;
        private ShaderVariation variation;

        public CompiledShaderDisassembler(ShaderData shaderData, CompiledShader shader, ShaderVariation variation)
        {
            this.shaderData = shaderData;
            this.shader = shader;
            this.variation = variation;
        }

        public string Disassemble()
        {
            StringBuilder sb = new StringBuilder();

            SHDR shdr = shader.blocks.First(b => b.fourCc == "SHDR" || b.fourCc == "SHEX") as SHDR;
            ISGN isgn = shader.blocks.First(b => b.fourCc == "ISGN") as ISGN;
            OSGN osgn = shader.blocks.First(b => b.fourCc == "OSGN") as OSGN;
            sb.AppendLine("// UltraShaderCore (UnityShaderEdit) disassembler");
            sb.AppendLine("// Disassembling type " + shdr.shaderType.ToString());
            sb.AppendLine("//");
            sb.AppendLine("// Unity field metadata:");
#if OLDER_VERSION
            sb.Append(GetUnityData());
#else
            // Things like CBs have been removed from fields and are
            // only at the end of the shader bytes
            sb.Append(GetNewUnityData());
#endif
            sb.AppendLine("//");
            string inOutSig = DecodeInOutSignature(isgn, osgn);
            sb.AppendLine("//");
            sb.AppendLine(inOutSig);

            int ifDepth = 0;
            foreach (SHDRInstruction inst in shdr.shaderInstructions)
            {
                string dis = DisassembleInstruction(inst, ref ifDepth);
                sb.AppendLine(dis);
            }

            return sb.ToString();
        }

        private string GetUnityData()
        {
            StringBuilder sb = new StringBuilder();

            AssetTypeValueField subProgram = variation.subProgram;
#if !OLDER_VERSION
            subProgram = subProgram.Get("m_Parameters");
#endif
            Dictionary<int, string> namesReversed = variation.namesReversed;
            AssetTypeValueField m_VectorParams = subProgram.Get("m_VectorParams").Get("Array");
            AssetTypeValueField m_MatrixParams = subProgram.Get("m_MatrixParams").Get("Array");
            AssetTypeValueField m_TextureParams = subProgram.Get("m_TextureParams").Get("Array");
            AssetTypeValueField m_BufferParams = subProgram.Get("m_BufferParams").Get("Array");
            AssetTypeValueField m_ConstantBuffers = subProgram.Get("m_ConstantBuffers").Get("Array");
            AssetTypeValueField m_ConstantBufferBindings = subProgram.Get("m_ConstantBufferBindings").Get("Array");
            AssetTypeValueField m_UAVParams = subProgram.Get("m_UAVParams").Get("Array");
            AddDataFromField(sb, m_VectorParams, namesReversed, "Vector Param");
            AddDataFromField(sb, m_MatrixParams, namesReversed, "Matrix Param");
            AddDataFromField(sb, m_TextureParams, namesReversed, "Texture Param");
            AddDataFromField(sb, m_BufferParams, namesReversed, "Buffer Param");
            AddDataFromFieldNoIndex(sb, m_ConstantBuffers, namesReversed, "Constant Buffer");
            AddDataFromField(sb, m_ConstantBufferBindings, namesReversed, "CB Binding");
            AddDataFromField(sb, m_UAVParams, namesReversed, "UAV Binding");

            return sb.ToString();
        }

        private string GetNewUnityData()
        {
            StringBuilder sb = new StringBuilder();

            ShaderMeta meta = shaderData.shaderMeta;

            for (int i = 0; i < meta.constantBuffers.Count; i++)
            {
                ShaderConstantBuffer constantBuffer = meta.constantBuffers[i];
                string name = constantBuffer.name;
                int byteSize = constantBuffer.byteSize;

                // Not sure why but sometimes there are just empty entries at the beginning
                if (name == string.Empty && byteSize == 0)
                    continue;

                int slotIdx = meta.slots.First(s => s.name == constantBuffer.name).slotIdx;
                sb.AppendLine($"// Constant Buffer \"{name}\" ({byteSize} bytes) on slot {slotIdx} {{");

                for (int j = 0; j < constantBuffer.cbParams.Count; j++)
                {
                    ShaderConstantBufferParam param = constantBuffer.cbParams[j];
                    string paramName = param.name;
                    int paramPos = param.pos;

                    string paramType = "Unknown";

                    if (param.rowCount == 1)
                    {
                        if (param.columnCount == 1)
                            paramType = "Float";
                        if (param.columnCount == 2)
                            paramType = "Vector2";
                        if (param.columnCount == 3)
                            paramType = "Vector3";
                        if (param.columnCount == 4)
                            paramType = "Vector4";
                    }
                    else if (param.rowCount == 4)
                    {
                        if (param.columnCount == 4 && param.isMatrix == 1)
                            paramType = "Matrix4x4";
                    }

                    sb.AppendLine($"//   {paramType} {paramName} at {paramPos}");
                }

                sb.AppendLine("// }");
            }

            return sb.ToString();
        }

        private void AddDataFromField(StringBuilder sb, AssetTypeValueField array, Dictionary<int, string> namesReversed, string displayName)
        {
            for (int i = 0; i < array.GetValue().AsArray().size; i++)
            {
                AssetTypeValueField field = array[i];
                string name = namesReversed[field.Get("m_NameIndex").GetValue().AsInt()];
                string index = field.Get("m_Index").GetValue().AsString();
                sb.AppendLine($"// {displayName} \"{name}\" [{index}]");
            }
        }

        private void AddDataFromFieldNoIndex(StringBuilder sb, AssetTypeValueField array, Dictionary<int, string> namesReversed, string displayName)
        {
            for (int i = 0; i < array.GetValue().AsArray().size; i++)
            {
                AssetTypeValueField field = array[i];
                string name = namesReversed[field.Get("m_NameIndex").GetValue().AsInt()];
                sb.AppendLine($"// {displayName} \"{name}\" [{i}]");
            }
        }

        private string DecodeInOutSignature(ISGN isgn, OSGN osgn)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// Input Signature:");
            sb.AppendLine("//");
            sb.AppendLine("// Name                 Index   Mask Register SysValue  Format   Used");
            sb.AppendLine("// -------------------- ----- ------ -------- -------- ------- ------");
            foreach (ISGN.Input input in isgn.inputs)
            {
                string name = PadString(input.name, 20, false);
                string index = PadString(input.index.ToString(), 5);
                string mask = PadString(GetMask(input.mask), 6);
                string register = PadString(input.register.ToString(), 8);
                string sysValue = PadString(((SysValueType)input.sysValue).ToString(), 8);
                string format = PadString(((FormatType)input.format).ToString(), 7);
                string used = PadString(GetMask(input.usedMask), 6);
                sb.AppendLine($"// {name} {index} {mask} {register} {sysValue} {format} {used}");
            }
            sb.AppendLine("//");
            sb.AppendLine("// Output Signature:");
            sb.AppendLine("//");
            sb.AppendLine("// Name                 Index   Mask Register SysValue  Format   Used");
            sb.AppendLine("// -------------------- ----- ------ -------- -------- ------- ------");
            foreach (OSGN.Output output in osgn.outputs)
            {
                string name = PadString(output.name, 20, false);
                string index = PadString(output.index.ToString(), 5);
                string mask = PadString(GetMask(output.mask), 6);
                string register = PadString(output.register.ToString(), 8);
                string sysValue = PadString(((SysValueType)output.sysValue).ToString(), 8);
                string format = PadString(((FormatType)output.format).ToString(), 7);
                string used = PadString(GetMask(output.usedMask, true), 6);
                sb.AppendLine($"// {name} {index} {mask} {register} {sysValue} {format} {used}");
            }
            return sb.ToString();
        }

        private string GetMask(byte mask, bool invert = false)
        {
            string maskOut = "";
            char[] maskChars = { 'x', 'y', 'z', 'w' };
            for (int i = 0; i < 4; i++)
            {
                bool on = ((mask & (1 << i)) >> i) == 1;
                if (invert)
                    on = !on;
                if (on)
                    maskOut += maskChars[i];
                else
                    maskOut += " ";
            }
            return maskOut;
        }

        private string PadString(string str, int length, bool alignRight = true)
        {
            if (!alignRight)
            {
                if (str.Length < length)
                {
                    str += new string(' ', length - str.Length);
                }
                else if (str.Length > length)
                {
                    str = str.Substring(0, length);
                }
            }
            else
            {
                if (str.Length < length)
                {
                    str = new string(' ', length - str.Length) + str;
                }
                else if (str.Length > length)
                {
                    str = str.Substring(str.Length - length, length);
                }
            }
            return str;
        }

        private string DisassembleInstruction(SHDRInstruction inst, ref int ifDepth)
        {
            if (inst.opcode == Opcode.endif || inst.opcode == Opcode.@else)
            {
                ifDepth -= 2;
            }
            string ifPadding = new string(' ', ifDepth);
            if (inst.opcode == Opcode.@if || inst.opcode == Opcode.@else)
            {
                ifDepth += 2;
            }

            string line = ifPadding + inst.opcode.ToString();
            bool firstOp = true;
            foreach (SHDRInstructionOperand op in inst.operands)
            {
                if (firstOp)
                {
                    line += " ";
                    firstOp = false;
                }
                else
                {
                    line += ", ";
                }

                if (op.swizzle != null && op.swizzle.Length > 0)
                {
                    char[] swizChars = { 'x', 'y', 'z', 'w' };
                    string swizStr = "";
                    for (int i = 0; i < op.swizzle.Length; i++)
                    {
                        swizStr += swizChars[op.swizzle[i]];
                    }

                    line += GetOperandName(op);
                    //if (op.operand != Operand.ConstantBuffer) //10/23/2021 why is this if statement here?
                        line += "." + swizStr;
                }
                else
                {
                    line += GetOperandName(op);
                }
            }

            if (SHDRInstruction.IsDeclaration(inst.opcode))
            {
                if (firstOp)
                {
                    line += " ";
                }
                switch (inst.opcode)
                {
                    case Opcode.dcl_globalFlags:
                    {
                        string ps = "";
                        int globalFlags = inst.declData.globalFlags;
                        if ((globalFlags & 1) == 1)
                            ps += ", refactoringAllowed";
                        if ((globalFlags & 2) == 2)
                            ps += ", enableDoublePrecisionFloatOps";
                        if ((globalFlags & 4) == 4)
                            ps += ", forceEarlyDepthStencil";
                        if ((globalFlags & 8) == 8)
                            ps += ", enableRawAndStructuredBuffers";
                        if ((globalFlags & 16) == 16)
                            ps += ", skipOptimization";
                        if ((globalFlags & 32) == 32)
                            ps += ", enableMinimumPrecision";
                        if ((globalFlags & 64) == 64)
                            ps += ", enable11_1DoubleExtensions";
                        if ((globalFlags & 128) == 128)
                            ps += ", enable11_1ShaderExtensions";
                        line += ps.TrimStart(',');
                        break;
                    }
                    case Opcode.dcl_constantbuffer:
                    {
                        if (inst.declData.constantBufferType == ConstantBufferType.DynamicIndexed)
                            line += ", dynamicIndexed";
                        else
                            line += ", immediateIndexed";
                        break;
                    }
                    case Opcode.dcl_temps:
                    {
                        line += inst.declData.numTemps;
                        break;
                    }
                    case Opcode.dcl_input_sgv:
                    case Opcode.dcl_input_ps_sgv:
                    case Opcode.dcl_output_siv:
                    {
                        line += ", " + inst.declData.nameToken;
                        break;
                    }
                    case Opcode.dcl_input_ps:
                    {
                        line = $"{line.Substring(0, line.IndexOf(' '))} {inst.declData.interpolation} {line.Substring(line.IndexOf(' ') + 1)}";
                        break;
                    }
                    case Opcode.dcl_sampler:
                    {
                        line += $"s{inst.declData.samplerIndex},";

                        if (inst.declData.samplerMode == SamplerMode.Default)
                            line += " mode_default";
                        else
                            line += " mode_comparison";
                        break;
                    }
                        //todo : assembleSystemValue
                }
            }

            return line;
        }
        private string GetOperandName(SHDRInstructionOperand op)
        {
            string prefix = "";
            if (op.extended)
            {
                if (((op.extendedData & 0x40) >> 6) == 1)
                {
                    prefix += "-";
                }
                if (((op.extendedData & 0x80) >> 7) == 1)
                {
                    prefix += "|";
                }
            }
            switch (op.operand)
            {
                case Operand.ConstantBuffer:
                    return $"{prefix}cb{op.arraySizes[0]}[{op.arraySizes[1]}]";
                case Operand.Input:
                    return $"{prefix}v{op.arraySizes[0]}";
                case Operand.Output:
                    return $"{prefix}o{op.arraySizes[0]}";
                case Operand.Immediate32:
                {
                    if (op.immValues.Length == 1)
                        return $"{prefix}l({(float)op.immValues[0]})";
                    if (op.immValues.Length == 4)
                        return $"{prefix}l({(float)op.immValues[0]}, {(float)op.immValues[1]}, {(float)op.immValues[2]}, {(float)op.immValues[3]})";
                    else
                        return $"{prefix}l()";
                }
                case Operand.Temp:
                    if (op.arraySizes.Length > 0)
                        return $"{prefix}r{op.arraySizes[0]}";
                    else
                        return $"{prefix}r0";
                case Operand.Resource:
                    return $"{prefix}t{op.arraySizes[0]}";
                case Operand.Sampler:
                    return $"{prefix}s{op.arraySizes[0]}";
                default:
                    return prefix + op.operand.ToString(); //haven't added this operand yet
            }
        }
    }
}
