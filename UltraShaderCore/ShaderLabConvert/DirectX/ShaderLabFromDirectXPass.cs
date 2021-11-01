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
    public class ShaderLabFromDirectXPass
    {
        private ShaderData _shaderData;
        private CompiledShader _dxShader;
        private ShaderVariation _shaderVar;

        private StringBuilder sb;
        private StringBuilder lsb;
        private int _indent;
        private int _initIndent;

        // Exceptional cases
        private bool _unitySpecCube1DefinedYet;

        private static char[] SWIZ_CHARS = { 'x', 'y', 'z', 'w' };
        private bool IsVertex
        {
            get
            {
                return _shaderVar.type == ShaderType.Direct3D9_Vertex_20 ||
                       _shaderVar.type == ShaderType.Direct3D9_Vertex_30 ||
                       _shaderVar.type == ShaderType.Direct3D11_Vertex_40 ||
                       _shaderVar.type == ShaderType.Direct3D11_Vertex_50;
            }
        }

        public ShaderLabFromDirectXPass(ShaderData shaderData, CompiledShader dxShader, ShaderVariation shaderVar)
        {
            _shaderData = shaderData;
            _dxShader = dxShader;
            _shaderVar = shaderVar;

            _instructionHandlers = new()
            {
                { Opcode.mov,      new InstHandler(HandleMov)     },
                { Opcode.movc,     new InstHandler(HandleMovc)    },
                { Opcode.add,      new InstHandler(HandleAdd)     },
                { Opcode.mul,      new InstHandler(HandleMul)     },
                { Opcode.div,      new InstHandler(HandleDiv)     },
                { Opcode.mad,      new InstHandler(HandleMad)     },
                { Opcode.and,      new InstHandler(HandleAnd)     },
                { Opcode.min,      new InstHandler(HandleMin)     },
                { Opcode.max,      new InstHandler(HandleMax)     },
                { Opcode.sqrt,     new InstHandler(HandleSqrt)    },
                { Opcode.rsq,      new InstHandler(HandleRsq)     },
                { Opcode.log,      new InstHandler(HandleLog)     },
                { Opcode.exp,      new InstHandler(HandleExp)     },
                { Opcode.dp2,      new InstHandler(HandleDp2)     },
                { Opcode.dp3,      new InstHandler(HandleDp3)     },
                { Opcode.dp4,      new InstHandler(HandleDp4)     },
                { Opcode.sample,   new InstHandler(HandleSample)  },
                { Opcode.sample_l, new InstHandler(HandleSampleL) },
                { Opcode.@if,      new InstHandler(HandleIf)      },
                { Opcode.@else,    new InstHandler(HandleElse)    },
                { Opcode.endif,    new InstHandler(HandleEndIf)   },
                { Opcode.eq,       new InstHandler(HandleEq)      },
                { Opcode.ne,       new InstHandler(HandleNeq)     },
                { Opcode.lt,       new InstHandler(HandleLt)      },
                { Opcode.ge,       new InstHandler(HandleGe)      },
                { Opcode.ret,      new InstHandler(HandleRet)     },
                //dec
                { Opcode.dcl_temps, new InstHandler(HandleTemps)  },
            };
        }

        private delegate void InstHandler(SHDRInstruction inst);
        private Dictionary<Opcode, InstHandler> _instructionHandlers;

        public string DecompileProgram(int indent)
        {
            sb = new StringBuilder();
            lsb = new StringBuilder();

            _initIndent = indent;
            _indent = indent;

            // Reset exceptional cases
            _unitySpecCube1DefinedYet = false;

            SHDR shdr = _dxShader.Shdr;
            List<SHDRInstruction> instructions = shdr.shaderInstructions;

            foreach (SHDRInstruction inst in instructions)
            {
                if (_instructionHandlers.ContainsKey(inst.opcode))
                {
                    _instructionHandlers[inst.opcode](inst);
                }
                else if (!SHDRInstruction.IsDeclaration(inst.opcode))
                {
                    AddLine(sb, $"{inst.opcode}_unsupported;");
                }
            }

            return lsb.ToString() + sb.ToString();
        }


        private void HandleMov(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, op0.swizzle, out _, out _);
            string operation = WrapSaturated(inst, $"{op1d}");
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleMovc(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            SHDRInstructionOperand op2 = inst.operands[2];
            SHDRInstructionOperand op3 = inst.operands[3];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, op0.swizzle, out _, out _);
            string op2d = GetOperandDisplayString(op2, op0.swizzle, out _, out _);
            string op3d = GetOperandDisplayString(op3, op0.swizzle, out _, out _);
            string operation = WrapSaturated(inst, $"{op1d} ? {op2d} : {op3d}");
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleAdd(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            SHDRInstructionOperand op2 = inst.operands[2];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, op0.swizzle, out _, out _);
            string op2d = GetOperandDisplayString(op2, op0.swizzle, out _, out _);
            string operation = WrapSaturated(inst, $"{op1d} + {op2d}");
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleMul(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            SHDRInstructionOperand op2 = inst.operands[2];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, op0.swizzle, out _, out _);
            string op2d = GetOperandDisplayString(op2, op0.swizzle, out _, out _);
            string operation = WrapSaturated(inst, $"{op1d} * {op2d}");
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleDiv(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            SHDRInstructionOperand op2 = inst.operands[2];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, op0.swizzle, out _, out _);
            string op2d = GetOperandDisplayString(op2, op0.swizzle, out _, out _);
            string operation = WrapSaturated(inst, $"{op1d} / {op2d}");
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleMad(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            SHDRInstructionOperand op2 = inst.operands[2];
            SHDRInstructionOperand op3 = inst.operands[3];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, op0.swizzle, out _, out _);
            string op2d = GetOperandDisplayString(op2, op0.swizzle, out _, out _);
            string op3d = GetOperandDisplayString(op3, op0.swizzle, out _, out _);
            string operation = WrapSaturated(inst, $"{op1d} * {op2d} + {op3d}");
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleAnd(SHDRInstruction inst)
        {
            // TODO
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            SHDRInstructionOperand op2 = inst.operands[2];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, op0.swizzle, out _, out _);
            string op2d = GetOperandDisplayString(op2, op0.swizzle, out _, out _);
            if (op2d != "1")
            {
                string operation = WrapSaturated(inst, $"uint({op1d}) & uint({op2d})");
                AddLine(sb, $"{op0d} = {operation};");
            }
        }

        private void HandleMin(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            SHDRInstructionOperand op2 = inst.operands[2];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, op0.swizzle, out _, out _);
            string op2d = GetOperandDisplayString(op2, op0.swizzle, out _, out _);
            string operation = $"min({op1d}, {op2d})";
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleMax(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            SHDRInstructionOperand op2 = inst.operands[2];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, op0.swizzle, out _, out _);
            string op2d = GetOperandDisplayString(op2, op0.swizzle, out _, out _);
            string operation = $"max({op1d}, {op2d})";
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleSqrt(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, op0.swizzle, out _, out _);
            string operation = $"sqrt({op1d})";
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleRsq(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, op0.swizzle, out _, out _);
            string operation = $"rsqrt({op1d})";
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleLog(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, op0.swizzle, out _, out _);
            string operation = $"log({op1d})";
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleExp(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, op0.swizzle, out _, out _);
            string operation = $"exp({op1d})";
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleDp2(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            SHDRInstructionOperand op2 = inst.operands[2];
            int[] mask = new int[] { 0, 1 };
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, mask, out _, out _);
            string op2d = GetOperandDisplayString(op2, mask, out _, out _);
            string operation = WrapSaturated(inst, $"dot({op1d}, {op2d})");
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleDp3(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            SHDRInstructionOperand op2 = inst.operands[2];
            int[] mask = new int[] { 0, 1, 2 };
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, mask, out _, out _);
            string op2d = GetOperandDisplayString(op2, mask, out _, out _);
            string operation = WrapSaturated(inst, $"dot({op1d}, {op2d})");
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleDp4(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            SHDRInstructionOperand op2 = inst.operands[2];
            int[] mask = new int[] { 0, 1, 2, 3 };
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, mask, out _, out _);
            string op2d = GetOperandDisplayString(op2, mask, out _, out _);
            string operation = WrapSaturated(inst, $"dot({op1d}, {op2d})");
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleSample(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            SHDRInstructionOperand op2 = inst.operands[2];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, out string op1b, out _);
            string op2d = GetOperandDisplayString(op2, out string op2b, out _);

            string operation;
            if (op2b == "unity_ProbeVolumeSH")
                operation = $"UNITY_SAMPLE_TEX3D_SAMPLER({op2b}, {op2b}, {op1b})";
            else
                operation = $"tex2D({op2b}, {op1b})";

            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleSampleL(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            SHDRInstructionOperand op2 = inst.operands[2];
            SHDRInstructionOperand op3 = inst.operands[3];
            SHDRInstructionOperand op4 = inst.operands[4];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, out string op1b, out _);
            string op2d = GetOperandDisplayString(op2, out string op2b, out _);
            string op3d = GetOperandDisplayString(op3, out string op3b, out _);
            string op4d = GetOperandDisplayString(op4, out string op4b, out _);

            string operation;
            if (op2b == "unity_SpecCube0" || op2b == "unity_SpecCube1")
                operation = $"UNITY_SAMPLE_TEXCUBE_LOD({op2b}, {op1b}, {op4b})";
            else
                operation = $"tex3D({op2b}, {op1b})";

            AddLine(sb, $"{op0d} = {operation};");

            if (op2b == "unity_SpecCube1" && !_unitySpecCube1DefinedYet)
            {
                // Terrible hack since the UNITY_SAMPLE_TEXCUBE macro requires samplerunity_SpecCube1 to exist
                // normally it's passed in as samplerunity_SpecCube0 with UNITY_PASS_TEXCUBE but SAMPLE_TEXCUBE
                // doesn't support this.
                _unitySpecCube1DefinedYet = true;
                AddLine(lsb, _initIndent, "SamplerState samplerunity_SpecCube1 = samplerunity_SpecCube0;");
            }
        }

        private void HandleIf(SHDRInstruction inst)
        {
            int testType = (inst.instData & 0x40000) >> 18;
            SHDRInstructionOperand op0 = inst.operands[0];
            string op0d = GetOperandDisplayString(op0, out _, out _);

            if (testType == 0)
                AddLine(sb, $"if (!{op0d})");
            else if (testType == 1)
                AddLine(sb, $"if ({op0d})");

            AddLine(sb, "{");
            _indent++;
        }

        private void HandleElse(SHDRInstruction inst)
        {
            _indent--;
            AddLine(sb, "}");
            AddLine(sb, "else");
            AddLine(sb, "{");
            _indent++;
        }

        private void HandleEndIf(SHDRInstruction inst)
        {
            _indent--;
            AddLine(sb, "}");
        }

        private void HandleEq(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            SHDRInstructionOperand op2 = inst.operands[2];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, op0.swizzle, out _, out _);
            string op2d = GetOperandDisplayString(op2, op0.swizzle, out _, out _);
            string operation = $"{op1d} == {op2d}";
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleNeq(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            SHDRInstructionOperand op2 = inst.operands[2];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, op0.swizzle, out _, out _);
            string op2d = GetOperandDisplayString(op2, op0.swizzle, out _, out _);
            string operation = $"{op1d} != {op2d}";
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleLt(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            SHDRInstructionOperand op2 = inst.operands[2];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, op0.swizzle, out _, out _);
            string op2d = GetOperandDisplayString(op2, op0.swizzle, out _, out _);
            string operation = $"{op1d} < {op2d}";
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleGe(SHDRInstruction inst)
        {
            SHDRInstructionOperand op0 = inst.operands[0];
            SHDRInstructionOperand op1 = inst.operands[1];
            SHDRInstructionOperand op2 = inst.operands[2];
            string op0d = GetOperandDisplayString(op0, out _, out _);
            string op1d = GetOperandDisplayString(op1, op0.swizzle, out _, out _);
            string op2d = GetOperandDisplayString(op2, op0.swizzle, out _, out _);
            string operation = $"{op1d} >= {op2d}";
            AddLine(sb, $"{op0d} = {operation};");
        }

        private void HandleRet(SHDRInstruction inst)
        {
            // Do nothing, handled elsewhere
        }

        private void HandleTemps(SHDRInstruction inst)
        {
            int tempCount = inst.declData.numTemps;
            for (int i = 0; i < tempCount; i++)
                AddLine(lsb, _initIndent, $"fixed4 temp{i};");
        }

        private string GetOperandDisplayString(SHDRInstructionOperand operand, out string body, out int[] mask)
        {
            return GetOperandDisplayString(operand, null, out body, out mask);
        }

        private string GetOperandDisplayString(SHDRInstructionOperand operand, int[] target, out string body, out int[] mask)
        {
            int[] croppedMask = operand.swizzle;
            if (croppedMask != null)
            {
                if (croppedMask.Length == 0)
                {
                    croppedMask = new int[] { 0, 1, 2, 3 };
                }
                if (target != null)
                {
                    croppedMask = MatchMaskToTarget(croppedMask, target);
                }
            }
            switch (operand.operand)
            {
                case Operand.ConstantBuffer:
                {
                    int cbSlotIdx = operand.arraySizes[0];
                    int cbArrIdx = operand.arraySizes[1];

                    List<int> operandMaskAddresses = new List<int>();
                    foreach (int operandMask in croppedMask)
                    {
                        operandMaskAddresses.Add(cbArrIdx * 16 + operandMask * 4);
                    }

                    HashSet<ShaderConstantBufferParam> cbParams = new HashSet<ShaderConstantBufferParam>();

                    ShaderSlot slot = _shaderData.shaderMeta.slots.First(s => s.slotIdx == cbSlotIdx && s.type == 1);
                    ShaderConstantBuffer constantBuffer = _shaderData.shaderMeta.constantBuffers.First(b => b.name == slot.name);

                    // Search children fields
                    foreach (ShaderConstantBufferParam param in constantBuffer.cbParams)
                    {
                        int paramCbStart = param.pos;
                        int paramCbSize = param.rowCount * param.columnCount * 4;
                        int paramCbEnd = paramCbStart + paramCbSize;

                        foreach (int operandMaskAddress in operandMaskAddresses)
                        {
                            if (operandMaskAddress >= paramCbStart && operandMaskAddress < paramCbEnd)
                            {
                                cbParams.Add(param);
                            }
                        }
                    }

                    // Search children structs and its fields
                    foreach (ShaderStructParam stParam in constantBuffer.stParams)
                    {
                        foreach (ShaderConstantBufferParam cbParam in stParam.structParams)
                        {
                            int paramCbStart = cbParam.pos;
                            int paramCbSize = cbParam.rowCount * cbParam.columnCount * 4;
                            int paramCbEnd = paramCbStart + paramCbSize;

                            foreach (int operandMaskAddress in operandMaskAddresses)
                            {
                                if (operandMaskAddress >= paramCbStart && operandMaskAddress < paramCbEnd)
                                {
                                    cbParams.Add(cbParam);
                                }
                            }
                        }
                    }

                    // Multiple cbuffers got opto'd into one operation
                    if (cbParams.Count > 1)
                    {
                        List<string> paramStrs = new List<string>();
                        foreach (ShaderConstantBufferParam param in cbParams)
                        {
                            string paramName = param.name;
                            // TO FIX, offset wrong
                            int[] matchedMask = MatchMaskToConstantBuffer(croppedMask, param.pos, param.rowCount);
                            string maskStr = MaskToString(matchedMask);
                            paramStrs.Add($"{paramName}.{maskStr}");
                        }
                        body = $"fixed{cbParams.Count}({string.Join(',', paramStrs)})";
                        mask = null;
                        break;
                    }
                    else if (cbParams.Count == 1)
                    {
                        ShaderConstantBufferParam param = cbParams.First();
                        body = param.name;

                        // Matrix
                        if (param.isMatrix > 0)
                        {
                            int matrixIdx = cbArrIdx - param.pos / 16;
                            body = $"_flip_{body}_{matrixIdx}";
                        }
                        mask = MatchMaskToConstantBuffer(croppedMask, param.pos, param.rowCount);
                        break;
                    }
                    body = $"cb{cbSlotIdx}[{cbArrIdx}]";
                    mask = croppedMask;
                    break;
                }
                case Operand.Input:
                {
                    // Search by first swizzle letter
                    int searchMask = (croppedMask.Length != 0) ? (1 << croppedMask[0]) : 0;
                    ISGN.Input input = _dxShader.Isgn.inputs.First(
                        o => o.register == operand.arraySizes[0] && ((searchMask & o.mask) == searchMask)
                    );

                    if (IsVertex)
                    {
                        body = $"v.{GetISGNInputName(input)}";
                        mask = croppedMask;
                        break;
                    }
                    else
                    {
                        body = $"i.{GetISGNInputName(input)}";
                        mask = croppedMask;
                        break;
                    }
                }
                case Operand.Output:
                {
                    if (IsVertex)
                    {
                        // Search by first swizzle letter
                        int searchMask = (croppedMask.Length != 0) ? (1 << croppedMask[0]) : 0;
                        OSGN.Output output = _dxShader.Osgn.outputs.First(
                            o => o.register == operand.arraySizes[0] && ((searchMask & o.mask) == searchMask)
                        );

                        int[] matchedMask = MatchMaskToOutput(croppedMask, output);

                        body = $"o.{GetOSGNOutputName(output)}";
                        mask = matchedMask;
                        break;
                    }
                    else
                    {
                        // Sometimes we have multiple outputs and I have
                        // no idea how unity figures out which are which
                        body = $"output{operand.arraySizes[0]}";
                        mask = croppedMask;
                        break;
                    }
                }
                case Operand.Immediate32:
                {
                    if (operand.immValues.Length == 1)
                        body = $"{FormatFloat((float)operand.immValues[0])}";
                    else if (operand.immValues.Length == 4)
                        body = $"fixed4({FormatFloat((float)operand.immValues[0])}, {FormatFloat((float)operand.immValues[1])}, {FormatFloat((float)operand.immValues[2])}, {FormatFloat((float)operand.immValues[3])})";
                    else
                        body = $"()"; // ?

                    mask = croppedMask;
                    break;
                }
                case Operand.Temp:
                {
                    if (operand.arraySizes.Length > 0)
                        body = $"temp{operand.arraySizes[0]}";
                    else
                        body = $"temp0";

                    mask = croppedMask;
                    break;
                }
                case Operand.Resource:
                {
                    ShaderSlot texSlot = _shaderData.shaderMeta.slots.First(
                        s => s.type == 0 && s.slotIdx == operand.arraySizes[0]
                    );

                    body = texSlot.name;
                    mask = croppedMask;
                    break;
                }
                case Operand.Sampler:
                {
                    ShaderSlot sampSlot = _shaderData.shaderMeta.slots.First(
                        s => s.type == 0 && s.args[0] == operand.arraySizes[0]
                    );

                    body = sampSlot.name;
                    mask = croppedMask;
                    break;
                }
                default:
                    body = $"undefined_{operand.operand}";
                    mask = croppedMask;
                    break;
            }

            string displayStr = mask != null ? $"{body}.{MaskToString(mask)}" : body;
            if (((operand.extendedData & 0x80) >> 7) == 1)
            {
                displayStr = $"abs({displayStr})";
            }
            if (((operand.extendedData & 0x40) >> 6) == 1)
            {
                displayStr = $"-{displayStr}";
            }
            return displayStr;
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

        // This is bad lol, need to figure out when we should do this or not
        private string FormatFloat(float f)
        {
            string s = f.ToString();
            if (!s.Contains(".") && !s.Contains("E"))
                s += ".0";
            return s;
        }

        private int[] MatchMaskToTarget(int[] source, int[] target)
        {
            if (source.Length <= target.Length)
            {
                return source;
            }
            int[] result = new int[target.Length];
            for (int i = 0; i < target.Length; i++)
            {
                if (i >= source.Length)
                    result[i] = source[^1];
                else
                    result[i] = source[target[i]];
            }
            return result;
        }

        private int[] MatchMaskToConstantBuffer(int[] mask, int pos, int size)
        {
            // Mask is aligned (x, xy, xyz, xyz)
            if (pos % 16 == 0)
            {
                return mask;
            }

            int offset = (pos / 4) % 4;
            List<int> result = new List<int>();
            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i] >= offset && mask[i] < offset + size)
                    result.Add(mask[i] - offset);
            }
            return result.ToArray();
        }

        private int[] MatchMaskToOutput(int[] mask, OSGN.Output output)
        {
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

            int[] result = new int[mask.Length];
            for (int i = 0; i < mask.Length; i++)
            {
                result[i] = mask[i] - moveCount;
            }
            return result;
        }

        private string MaskToString(int[] mask)
        {
            string str = "";
            foreach (int m in mask)
            {
                str += SWIZ_CHARS[m];
            }
            return str;
        }

        private string WrapSaturated(SHDRInstruction inst, string str)
        {
            if (inst.saturated)
            {
                str = $"saturate({str})";
            }
            return str;
        }

        ///////////////////

        private void AddText(StringBuilder sb, string text)
        {
            sb.Append(new string(' ', _indent * 4));
            sb.Append(text);
        }

        private void AddLine(StringBuilder sb, string text)
        {
            sb.Append(new string(' ', _indent * 4));
            sb.AppendLine(text);
        }

        private void AddText(StringBuilder sb, int indent, string text)
        {
            sb.Append(new string(' ', indent * 4));
            sb.Append(text);
        }

        private void AddLine(StringBuilder sb, int indent, string text)
        {
            sb.Append(new string(' ', indent * 4));
            sb.AppendLine(text);
        }
    }
}
