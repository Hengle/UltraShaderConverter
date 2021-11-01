public class Class46
{
	internal class Class47
	{
		internal string src;

		internal string op;

		internal InstructionOperand[] args;

		internal Class47(string s)
		{
			s = s.Trim();
			src = s;
			int num = s.IndexOfAny(" 	(".ToCharArray());
			if (num < 0)
			{
				op = s;
				return;
			}
			op = s.Substring(0, num).Trim(' ', '\t');
			s = s.Substring(num);
			while (s.TrimStart().StartsWith("(") && s.Contains(")"))
			{
				op += s.Substring(0, s.IndexOf(")") + 1);
				s = s.Substring(s.IndexOf(")") + 1);
			}
			string[] array = new string[4]
			{
				" linearcentroid",
				" noperspective",
				" constant",
				" linear"
			};
			foreach (string text in array)
			{
				int num2 = s.IndexOf(text);
				if (num2 >= 0)
				{
					op += s.Substring(0, num2 + text.Length);
					s = s.Substring(num2 + text.Length).TrimStart();
					break;
				}
			}
			while (s.TrimStart().StartsWith("(") && s.Contains(")"))
			{
				op += s.Substring(0, s.IndexOf(")") + 1);
				s = s.Substring(s.IndexOf(")") + 1);
			}
			string text2 = s.Trim(' ', '\t');
			List<string> list = new List<string>();
			string text3 = null;
			int num3 = 0;
			for (int j = 0; j < text2.Length; j++)
			{
				char c = text2[j];
				char c2 = ((j + 1 < text2.Length) ? text2[j + 1] : '\0');
				switch (c)
				{
					case ',':
						if (num3 == 0)
						{
							text3 = text3?.Trim();
							if (!string.IsNullOrEmpty(text3))
							{
								list.Add(text3);
							}
							text3 = null;
						}
						else
						{
							text3 += c;
						}
						continue;
					case 'l':
						if (c2 == '(')
						{
							text3 += "l(";
							num3++;
							j++;
							continue;
						}
						break;
				}
				if (c == 'd' && c2 == '(')
				{
					text3 += "d(";
					num3++;
					j++;
					continue;
				}
				switch (c)
				{
					case '(':
						text3 += c;
						num3++;
						break;
					case ')':
						text3 += c;
						num3--;
						break;
					default:
						text3 += c;
						break;
				}
			}
			text3 = text3?.Trim();
			if (!string.IsNullOrEmpty(text3))
			{
				list.Add(text3);
			}
			args = new InstructionOperand[list.Count];
			int num4 = 0;
			foreach (string item in list)
			{
				InstructionOperand @class = InstructionOperand.ParseFromString(item.Trim());
				args[num4++] = @class;
			}
		}

		public override string ToString()
		{
			string string_ = "DATA_LINE: \n";
			string_ = string_ + "  src: " + src + "\n";
			string_ = string_ + "  op: " + op + "\n";
			string_ += "  args: {";
			if (args != null)
			{
				InstructionOperand[] array = args;
				foreach (InstructionOperand @class in array)
				{
					string_ = string_ + ", " + @class;
				}
			}
			return string_ + "}\n";
		}
	}

	internal class InstructionOperand
	{
		internal bool abs;

		internal string idx;

		internal string name;

		internal bool neg;

		internal string suffix;

		internal string[] vals;

		internal bool idxNotNullOrEmpty => !string.IsNullOrEmpty(idx);

		internal bool idxIsDigitOnly
		{
			get
			{
				if (string.IsNullOrEmpty(idx))
				{
					return false;
				}
				string text = idx;
				int num = 0;
				while (true)
				{
					if (num < text.Length)
					{
						if (!char.IsDigit(text[num]))
						{
							break;
						}
						num++;
						continue;
					}
					return true;
				}
				return false;
			}
		}

		public override string ToString()
		{
			string text = "{";
			if (name != null)
			{
				text = text + " name: \"" + name + "\"";
			}
			if (suffix != null)
			{
				text = text + " suffix: \"" + suffix + "\"";
			}
			if (idx != null)
			{
				text = text + " idx: \"" + idx + "\"";
			}
			if (neg)
			{
				text = text + " neg: " + neg;
			}
			if (abs)
			{
				text = text + " abs: " + abs;
			}
			if (vals != null)
			{
				text = text + " vals: {" + string.Join(", ", vals) + "}";
			}
			return text + " }";
		}

		internal static InstructionOperand ParseFromString(string string_4)
		{
			string text = string_4?.Trim();
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			InstructionOperand @class = new InstructionOperand();
			if (text.StartsWith("-"))
			{
				@class.neg = true;
				text = text.TrimStart('-');
			}
			if (text.StartsWith("|"))
			{
				@class.abs = true;
				text = text.Trim('|', ' ');
			}
			if (!text.StartsWith("l(") && !text.StartsWith("d("))
			{
				if (text.StartsWith("("))
				{
					@class.vals = text.Substring(1, text.Length - 2).Trim().Split(',');
					for (int i = 0; i < @class.vals.Length; i++)
					{
						@class.vals[i] = @class.vals[i].Trim();
					}
					return @class;
				}
				if (!char.IsDigit(text[0]) && text.IndexOf('.') >= 0)
				{
					int startIndex = 0;
					if (text.Contains("]"))
					{
						startIndex = text.LastIndexOf(']') + 1;
					}
					int num = text.IndexOf('.', startIndex);
					if (num > 0)
					{
						@class.suffix = text.Substring(num + 1);
						text = text.Substring(0, num);
					}
				}
				if (text.Contains("[") && text.EndsWith("]"))
				{
					@class.idx = text.Substring(text.IndexOf('[') + 1).TrimEnd(']');
					text = text.Substring(0, text.IndexOf('['));
				}
				@class.name = text;
				return @class;
			}
			@class.vals = text.Substring(2, text.Length - 3).Trim().Split(',');
			for (int j = 0; j < @class.vals.Length; j++)
			{
				@class.vals[j] = @class.vals[j].Trim();
			}
			return @class;
		}
	}

	internal class ShaderParamBinding
	{
		internal string name;

		internal string bind;

		internal string mask;

		internal ShaderParamDesc desc;

		internal Class51[] class51_0;

		internal int int_0;

		public override string ToString()
		{
			string text = "{";
			if (name != null)
			{
				text = text + " name: \"" + name + "\"";
			}
			if (desc != null)
			{
				text = text + " desc: " + desc;
			}
			return text + " }";
		}
	}

	internal class ShaderParamDesc
	{
		internal string name;

		internal string bind;

		internal string mask;

		internal string register;

		internal int index;

		internal string valueType;

		internal int arraySize;

		internal bool isUniform; //idkg

		internal bool isVector;

		internal bool isUAV;

		internal bool isMatrix;

		internal string cbufferName;

		internal Class51[] class51_0;

		public override string ToString()
		{
			string text = "{";
			if (name != null)
			{
				text = text + " name: \"" + name + "\"";
			}
			if (bind != null)
			{
				text = text + " bind: \"" + bind + "\"";
			}
			if (mask != null)
			{
				text = text + " mask: \"" + mask + "\"";
			}
			text = text + " index: \"" + index + "\"";
			if (register != null)
			{
				text = text + " register: \"" + register + "\"";
			}
			if (cbufferName != null)
			{
				text = text + " cbuffer_name: \"" + cbufferName + "\"";
			}
			if (class51_0 != null)
			{
				text += "\n";
				Class51[] array = class51_0;
				foreach (Class51 @class in array)
				{
					text = text + "    " + @class.ToString() + "\n";
				}
			}
			return text + " }";
		}
	}

	internal class Class51
	{
		internal string name;

		internal string desc;

		internal int offset;

		internal int size;

		internal int int_2;

		internal string string_2;

		public override string ToString()
		{
			string text = "{";
			if (name != null)
			{
				text = text + " name: \"" + name + "\"";
			}
			if (desc != null)
			{
				text = text + " desc: " + desc;
			}
			text = text + " offset: " + offset;
			text = text + " size: " + size;
			return text + " }";
		}
	}

	private enum GenerateTypeEnum
	{
		GLS,
		HLSL
	}

	private enum ShaderTypeEnum
	{
		Vertex,
		Pixel
	}

	internal class Class52
	{
		internal string opStrSimplified;

		internal string blockTag;

		internal Class52(string op_str)
		{
			opStrSimplified = op_str?.Replace(".xyzw", "");
		}

		internal Class52(string op_str, string block_tag)
		{
			opStrSimplified = op_str?.Replace(".xyzw", "");
			blockTag = block_tag;
		}

		internal static Class52 smethod_0(string format, params object[] args)
		{
			return smethod_1(BasicStringFormat(format, args));
		}

		internal static Class52 smethod_1(string op_str, string block_tag = null)
		{
			return new Class52(op_str, block_tag);
		}
	}

	internal delegate string[] Delegate1(string op);

	internal delegate Class52 Delegate2(HashSet<string> op_args, InstructionOperand[] param, Class47 orig);

	private class Class53
	{
		internal Delegate1 delegate1_0;

		internal string[] string_0;
	}

	private class BlockTags
	{
		internal string name;

		internal HashSet<string> endNames = new HashSet<string>();
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class Class55
	{
		public static readonly Class55 class55_0 = new Class55();

		public static Delegate1 delegate1_0;

		public static Delegate1 delegate1_1;

		public static Delegate1 delegate1_2;

		public static Delegate1 delegate1_3;

		public static Delegate1 delegate1_4;

		public static Delegate1 delegate1_5;

		public static Delegate1 delegate1_6;

		public static Delegate1 delegate1_7;

		public static Delegate1 delegate1_8;

		public static Delegate1 delegate1_9;

		public static Delegate1 delegate1_10;

		public static Delegate1 delegate1_11;

		public static Delegate1 delegate1_12;

		public static Delegate1 delegate1_13;

		public static Delegate1 delegate1_14;

		public static Delegate1 delegate1_15;

		public static Delegate1 delegate1_16;

		public static Delegate1 delegate1_17;

		public static Delegate1 delegate1_18;

		public static Delegate1 delegate1_19;

		public static Delegate1 delegate1_20;

		public static Delegate1 delegate1_21;

		public static Delegate1 delegate1_22;

		public static Delegate1 delegate1_23;

		public static Delegate1 delegate1_24;

		public static Delegate1 delegate1_25;

		public static Delegate1 delegate1_26;

		public static Delegate1 delegate1_27;

		public static Delegate1 delegate1_28;

		public static Delegate1 delegate1_29;

		public static Delegate1 delegate1_30;

		public static Delegate1 delegate1_31;

		public static Delegate1 delegate1_32;

		public static Delegate1 delegate1_33;

		public static Delegate1 delegate1_34;

		public static Delegate1 delegate1_35;

		public static Delegate1 delegate1_36;

		public static Delegate1 delegate1_37;

		public static Delegate1 delegate1_38;

		public static Delegate1 delegate1_39;

		public static Delegate1 delegate1_40;

		public static Delegate1 delegate1_41;

		public static Delegate1 delegate1_42;

		public static Delegate1 delegate1_43;

		public static Delegate1 delegate1_44;

		public static Delegate1 delegate1_45;

		public static Delegate1 delegate1_46;

		public static Delegate1 delegate1_47;

		public static Delegate1 delegate1_48;

		public static Delegate1 delegate1_49;

		public static Delegate1 delegate1_50;

		public static Delegate1 delegate1_51;

		public static Delegate1 delegate1_52;

		public static Delegate1 delegate1_53;

		public static Delegate1 delegate1_54;

		public static Delegate1 delegate1_55;

		public static Delegate1 delegate1_56;

		public static Delegate1 delegate1_57;

		public static Delegate1 delegate1_58;

		public static Delegate1 delegate1_59;

		public static Delegate1 delegate1_60;

		public static Delegate1 delegate1_61;

		public static Delegate1 delegate1_62;

		public static Delegate1 delegate1_63;

		public static Delegate1 delegate1_64;

		public static Delegate1 delegate1_65;

		public static Delegate1 delegate1_66;

		public static Delegate1 delegate1_67;

		public static Delegate1 delegate1_68;

		public static Delegate1 delegate1_69;

		public static Delegate1 delegate1_70;

		public static Delegate1 delegate1_71;

		public static Delegate1 delegate1_72;

		public static Delegate1 delegate1_73;

		public static Delegate1 delegate1_74;

		public static Delegate1 delegate1_75;

		public static Delegate1 delegate1_76;

		public static Delegate1 delegate1_77;

		public static Delegate1 delegate1_78;

		public static Delegate2 delegate2_0;

		public static Delegate2 delegate2_1;

		public static Delegate2 delegate2_2;

		public static Delegate2 delegate2_3;

		public static Delegate2 delegate2_4;

		public static Delegate2 delegate2_5;

		public static Delegate2 delegate2_6;

		public static Delegate2 delegate2_7;

		public static Delegate2 delegate2_8;

		public static Delegate2 delegate2_9;

		public static Delegate2 delegate2_10;

		public static Delegate2 delegate2_11;

		public static Delegate2 delegate2_12;

		public static Delegate2 delegate2_13;

		public static Delegate2 delegate2_14;

		public static Delegate2 delegate2_15;

		internal string[] method_0(string string_0)
		{
			if (string_0.StartsWith("dp") && char.IsDigit(string_0[2]))
			{
				return new string[1] { string_0.Substring(3) };
			}
			return null;
		}

		internal string[] method_1(string string_0)
		{
			return OpcodeMatches(string_0, "d", "swapc");
		}

		internal string[] method_2(string string_0)
		{
			return OpcodeMatches(string_0, "d", "movc");
		}

		internal string[] method_3(string string_0)
		{
			if (!(string_0 == "movc") && !(string_0 == "dmovc"))
			{
				return OpcodeMatches(string_0, "d", "mov");
			}
			return null;
		}

		internal string[] method_4(string string_0)
		{
			return OpcodeMatches(string_0, "di", "add");
		}

		internal string[] method_5(string string_0)
		{
			return OpcodeMatches(string_0, "d", "mul");
		}

		internal string[] method_6(string string_0)
		{
			return OpcodeMatches(string_0, "", "umul");
		}

		internal string[] method_7(string string_0)
		{
			return OpcodeMatches(string_0, "", "imul");
		}

		internal string[] method_8(string string_0)
		{
			return OpcodeMatches(string_0, "ui", "mad");
		}

		internal string[] method_9(string string_0)
		{
			return OpcodeMatches(string_0, "d", "div");
		}

		internal string[] method_10(string string_0)
		{
			return OpcodeMatches(string_0, "", "udiv");
		}

		internal string[] method_11(string string_0)
		{
			return OpcodeMatches(string_0, "uid", "max");
		}

		internal string[] method_12(string string_0)
		{
			return OpcodeMatches(string_0, "uid", "min");
		}

		internal string[] method_13(string string_0)
		{
			return OpcodeMatches(string_0, "", "sincos");
		}

		internal string[] method_14(string string_0)
		{
			return OpcodeMatches(string_0, "", "log");
		}

		internal string[] method_15(string string_0)
		{
			string[] array = OpcodeMatches(string_0, "", "sample_l_indexable");
			if (array != null)
			{
				return array;
			}
			return OpcodeMatches(string_0, "", "sample");
		}

		internal string[] method_16(string string_0)
		{
			return OpcodeMatches(string_0, "", "gather4");
		}

		internal string[] method_17(string string_0)
		{
			return OpcodeMatches(string_0, "", "deriv_rty");
		}

		internal string[] method_18(string string_0)
		{
			return OpcodeMatches(string_0, "", "deriv_rtx");
		}

		internal string[] method_19(string string_0)
		{
			return OpcodeMatches(string_0, "di", "eq");
		}

		internal string[] method_20(string string_0)
		{
			return OpcodeMatches(string_0, "di", "ne");
		}

		internal string[] method_21(string string_0)
		{
			return OpcodeMatches(string_0, "", "not");
		}

		internal string[] method_22(string string_0)
		{
			return OpcodeMatches(string_0, "uid", "lt");
		}

		internal string[] method_23(string string_0)
		{
			return OpcodeMatches(string_0, "uid", "ge");
		}

		internal string[] method_24(string string_0)
		{
			return OpcodeMatches(string_0, "ui", "shl");
		}

		internal string[] method_25(string string_0)
		{
			return OpcodeMatches(string_0, "ui", "shr");
		}

		internal string[] method_26(string string_0)
		{
			return OpcodeMatches(string_0, "", "discard");
		}

		internal string[] method_27(string string_0)
		{
			return OpcodeMatches(string_0, "", "if");
		}

		internal string[] method_28(string string_0)
		{
			if (!(string_0 == "else"))
			{
				return null;
			}
			return new string[0];
		}

		internal Class52 method_29(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
		{
			return Class52.smethod_1("} else {", "else");
		}

		internal string[] method_30(string string_0)
		{
			if (!(string_0 == "endif"))
			{
				return null;
			}
			return new string[0];
		}

		internal Class52 method_31(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
		{
			return Class52.smethod_1("}", "endif");
		}

		internal string[] method_32(string string_0)
		{
			if (!(string_0 == "break"))
			{
				return null;
			}
			return new string[0];
		}

		internal Class52 method_33(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
		{
			return Class52.smethod_1("break", "break");
		}

		internal string[] method_34(string string_0)
		{
			return OpcodeMatches(string_0, "", "breakc");
		}

		internal string[] method_35(string string_0)
		{
			if (!(string_0 == "loop"))
			{
				return null;
			}
			return new string[0];
		}

		internal Class52 method_36(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
		{
			return Class52.smethod_1("while(true) {", "loop");
		}

		internal string[] method_37(string string_0)
		{
			if (!(string_0 == "endloop"))
			{
				return null;
			}
			return new string[0];
		}

		internal Class52 method_38(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
		{
			return Class52.smethod_1("}", "endloop");
		}

		internal string[] method_39(string string_0)
		{
			if (!(string_0 == "continue"))
			{
				return null;
			}
			return new string[0];
		}

		internal Class52 method_40(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
		{
			return Class52.smethod_1("continue");
		}

		internal string[] method_41(string string_0)
		{
			return OpcodeMatches(string_0, "", "continuec");
		}

		internal string[] method_42(string string_0)
		{
			if (!(string_0 == "rsq"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_43(string string_0)
		{
			if (!(string_0 == "sqrt"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_44(string string_0)
		{
			if (!(string_0 == "frc"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_45(string string_0)
		{
			return OpcodeMatches(string_0, "d", "rcp");
		}

		internal string[] method_46(string string_0)
		{
			if (!(string_0 == "exp"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_47(string string_0)
		{
			if (!(string_0 == "round_ni"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_48(string string_0)
		{
			if (!(string_0 == "round_pi"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_49(string string_0)
		{
			if (!(string_0 == "round_ne"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_50(string string_0)
		{
			if (!(string_0 == "round_z"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_51(string string_0)
		{
			if (!(string_0 == "ftoi"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_52(string string_0)
		{
			if (!(string_0 == "ftou"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_53(string string_0)
		{
			return OpcodeMatches(string_0, "uid", "tof");
		}

		internal string[] method_54(string string_0)
		{
			if (!(string_0 == "and"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_55(string string_0)
		{
			if (!(string_0 == "or"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_56(string string_0)
		{
			if (!(string_0 == "xor"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_57(string string_0)
		{
			if (!(string_0 == "ret"))
			{
				return null;
			}
			return new string[0];
		}

		internal Class52 method_58(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
		{
			return Class52.smethod_1("return");
		}

		internal string[] method_59(string string_0)
		{
			return OpcodeMatches(string_0, "", "retc");
		}

		internal string[] method_60(string string_0)
		{
			return OpcodeMatches(string_0, "", "vs_");
		}

		internal Class52 method_61(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
		{
			return Class52.smethod_1("// Vertex shader");
		}

		internal string[] method_62(string string_0)
		{
			return OpcodeMatches(string_0, "", "ps_");
		}

		internal Class52 method_63(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
		{
			return Class52.smethod_1("// Pixel shader");
		}

		internal string[] method_64(string string_0)
		{
			return OpcodeMatches(string_0, "", "cs_");
		}

		internal Class52 method_65(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
		{
			return Class52.smethod_1("// CS shader");
		}

		internal string[] method_66(string string_0)
		{
			return OpcodeMatches(string_0, "", "dcl");
		}

		internal string[] method_67(string string_0)
		{
			if (!(string_0 == "bfi"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_68(string string_0)
		{
			if (!(string_0 == "bfrev"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_69(string string_0)
		{
			if (!(string_0 == "countbits"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_70(string string_0)
		{
			if (!(string_0 == "sat"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_71(string string_0)
		{
			if (!(string_0 == "neg"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_72(string string_0)
		{
			if (!(string_0 == "abs"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_73(string string_0)
		{
			return OpcodeMatches(string_0, "", "ld_structured");
		}

		internal string[] method_74(string string_0)
		{
			return OpcodeMatches(string_0, "", "ld_indexable");
		}

		internal string[] method_75(string string_0)
		{
			if (!(string_0 == "ld"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_76(string string_0)
		{
			return OpcodeMatches(string_0, "", "store_structured");
		}

		internal string[] method_77(string string_0)
		{
			return OpcodeMatches(string_0, "", "store_uav_typed");
		}

		internal string[] method_78(string string_0)
		{
			return OpcodeMatches(string_0, "", "sync");
		}

		internal Class52 method_79(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
		{
			return Class52.smethod_0("sync");
		}

		internal string[] method_80(string string_0)
		{
			if (!(string_0 == "call"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_81(string string_0)
		{
			return OpcodeMatches(string_0, "", "callc");
		}

		internal string[] method_82(string string_0)
		{
			if (!(string_0 == "label"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_83(string string_0)
		{
			if (!(string_0 == "case"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_84(string string_0)
		{
			if (!(string_0 == "default"))
			{
				return null;
			}
			return new string[0];
		}

		internal Class52 method_85(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
		{
			return Class52.smethod_1(BasicStringFormat("default:"), "default");
		}

		internal string[] method_86(string string_0)
		{
			if (!(string_0 == "switch"))
			{
				return null;
			}
			return new string[0];
		}

		internal string[] method_87(string string_0)
		{
			if (!(string_0 == "endswitch"))
			{
				return null;
			}
			return new string[0];
		}

		internal Class52 method_88(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
		{
			return Class52.smethod_1(BasicStringFormat("}"), "endswitch");
		}

		internal string[] method_89(string string_0)
		{
			if (!(string_0 == "cut"))
			{
				return null;
			}
			return new string[0];
		}

		internal Class52 method_90(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
		{
			return Class52.smethod_0("cut()");
		}

		internal string[] method_91(string string_0)
		{
			if (!(string_0 == "emit"))
			{
				return null;
			}
			return new string[0];
		}

		internal Class52 method_92(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
		{
			return Class52.smethod_0("emit()");
		}

		internal string[] method_93(string string_0)
		{
			if (!(string_0 == "nop"))
			{
				return null;
			}
			return new string[0];
		}

		internal Class52 method_94(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
		{
			return Class52.smethod_0("// nop");
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	internal struct Struct4
	{
		public string[] string_0;
	}

	internal bool bool_0;

	private List<ShaderParamDesc> cbufferParams;

	private List<ShaderParamDesc> bindingParams;

	private List<ShaderParamDesc> inputParams;

	private List<ShaderParamDesc> shaderParams;

	private List<ShaderParamDesc> outputParams;

	private Dictionary<string, ShaderParamBinding> displayToShaderParamBinding = new Dictionary<string, ShaderParamBinding>();

	private List<ShaderParamBinding> shaderParamBindings = new List<ShaderParamBinding>();

	private GenerateTypeEnum generateType;

	private ShaderTypeEnum shaderType;

	private static Dictionary<string, int> maskToIndex = new Dictionary<string, int>
	{
		{ "x", 0 },
		{ "y", 1 },
		{ "z", 2 },
		{ "w", 3 }
	};

	private Dictionary<string, int> maskToOffset = new Dictionary<string, int>
	{
		{ "x", 0 },
		{ "y", 4 },
		{ "z", 8 },
		{ "w", 12 }
	};

	private int int_0;

	private Dictionary<Delegate1, Delegate2> dictionary_3;

	private static Dictionary<string, BlockTags> blockTagMap = new Dictionary<string, BlockTags>
	{
		{
			"if",
			new BlockTags
			{
				name = "if",
				endNames = new HashSet<string>(new string[2]
				{
					"else",
					"endif"
				})
			}
		},
		{
			"else",
			new BlockTags
			{
				name = "else",
				endNames = new HashSet<string>(new string[1] { "endif" })
			}
		},
		{
			"loop",
			new BlockTags
			{
				name = "loop",
				endNames = new HashSet<string>(new string[1] { "endloop" })
			}
		},
		{
			"switch",
			new BlockTags
			{
				name = "switch",
				endNames = new HashSet<string>(new string[1] { "endswitch" })
			}
		},
		{
			"case",
			new BlockTags
			{
				name = "case",
				endNames = new HashSet<string>(new string[2]
				{
					"case",
					"break"
				})
			}
		},
		{
			"default",
			new BlockTags
			{
				name = "default",
				endNames = new HashSet<string>(new string[2]
				{
					"default",
					"break"
				})
			}
		}
	};

	private Stack<BlockTags> stack_0 = new Stack<BlockTags>();

	private Dictionary<Delegate1, Delegate2> Dictionary_0
	{
		get
		{
			if (dictionary_3 != null)
			{
				return dictionary_3;
			}
			dictionary_3 = new Dictionary<Delegate1, Delegate2>();
			dictionary_3[(string string_0) => (string_0.StartsWith("dp") && char.IsDigit(string_0[2])) ? new string[1] { string_0.Substring(3) } : null] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_171, out var class48_172, out var class48_173);
				string text167 = GetDisplayString(class48_171);
				string text168 = GetDisplayString(class48_172);
				string text169 = GetDisplayString(class48_173);
				return hashSet_0.Contains("_sat") ? Class52.smethod_0("%s = saturate(dot(%s, %s))", text167, text168, text169) : Class52.smethod_0("%s = dot(%s, %s)", text167, text168, text169);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "d", "swapc")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get5OperandsSafe(class48_0, out var class48_166, out var class48_167, out var class48_168, out var class48_169, out var class48_170);
				string text157 = "float" + (string.IsNullOrEmpty(class48_166.suffix) ? "" : class48_166.suffix.Length.ToString());
				string string_33 = null;
				ShaderParamBinding class2 = GetParamBindingAndFixSuffix(class48_166.name, class48_166.suffix, ref string_33);
				object obj;
				if (class2 != null)
				{
					if (class2 == null)
					{
						obj = null;
					}
					else
					{
						ShaderParamDesc class50_2 = class2.desc;
						if (class50_2 == null)
						{
							obj = null;
						}
						else
						{
							obj = class50_2.valueType;
							if (obj != null)
							{
								goto IL_0082;
							}
						}
					}
					obj = text157;
					goto IL_0082;
				}
				goto IL_0084;
				IL_0082:
				text157 = (string)obj;
				goto IL_0084;
				IL_0084:
				string text158 = GetDisplayString(class48_166);
				string text159 = GetDisplayString(class48_167);
				string text160 = GetDisplayString(class48_168, class48_166);
				string text161 = GetDisplayString(class48_169, class48_166);
				string text162 = GetDisplayString(class48_170, class48_166);
				string text163 = "swapc_tmp" + int_0++;
				string text164 = text157 + " " + text163 + " = " + BasicStringFormat("%s ? %s : %s", text160, text162, text161) + "; ";
				string text165 = BasicStringFormat("%s = %s ? %s : %s", text159, text160, text161, text162) + "; ";
				string text166 = BasicStringFormat("%s = %s", text158, text163) + ";";
				if (text158 == null || text158 == "null")
				{
					text164 = null;
					text166 = null;
				}
				if (text159 == null || text159 == "null")
				{
					text165 = null;
				}
				return Class52.smethod_1((text164 + text165 + text166)?.TrimEnd(' '));
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "d", "movc")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get4OperandsSafe(class48_0, out var class48_162, out var class48_163, out var class48_164, out var class48_165);
				string text153 = GetDisplayString(class48_162);
				string text154 = GetDisplayString(class48_163, class48_162);
				string text155 = GetDisplayString(class48_164, class48_162);
				string text156 = GetDisplayString(class48_165, class48_162);
				return Class52.smethod_0("%s = %s ? %s : %s", text153, text154, text155, text156);
			};
			dictionary_3[(string string_0) => (!(string_0 == "movc") && !(string_0 == "dmovc")) ? OpcodeMatches(string_0, "d", "mov") : null] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_159, out var class48_160, out var _);
				string text151 = GetDisplayString(class48_159);
				string text152 = GetDisplayString(class48_160, class48_159);
				return hashSet_0.Contains("_sat") ? Class52.smethod_0("%s = saturate(%s)", text151, text152) : Class52.smethod_0("%s = %s", text151, text152);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "di", "add")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_156, out var class48_157, out var class48_158);
				string text147 = GetDisplayString(class48_156);
				string text148 = GetDisplayString(class48_157, class48_156);
				string text149 = GetDisplayString(class48_158, class48_156);
				string text150 = null;
				text150 = ((text149[0] != '-') ? BasicStringFormat("%s + %s", text148, text149) : BasicStringFormat("%s - %s", text148, text149.Substring(1)));
				return hashSet_0.Contains("_sat") ? Class52.smethod_0("%s = saturate(%s)", text147, text150) : Class52.smethod_0("%s = %s", text147, text150);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "d", "mul")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_153, out var class48_154, out var class48_155);
				string text143 = GetDisplayString(class48_153);
				string text144 = GetDisplayString(class48_154, class48_153);
				string text145 = GetDisplayString(class48_155, class48_153);
				string text146 = null;
				text146 = BasicStringFormat("%s * %s", text144, text145);
				return hashSet_0.Contains("_sat") ? Class52.smethod_0("%s = saturate(%s)", text143, text146) : Class52.smethod_0("%s = %s", text143, text146);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "umul")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get4OperandsSafe(class48_0, out var class48_149, out var class48_150, out var class48_151, out var class48_152);
				string text137 = GetDisplayString(class48_149);
				string text138 = GetDisplayString(class48_150);
				string text139 = GetDisplayString(class48_151, class48_149);
				string text140 = GetDisplayString(class48_152, class48_150);
				string text141 = BasicStringFormat("(%s * %s) >> 32", text139, text140);
				string text142 = BasicStringFormat("(%s * %s) & 0xffffffff", text139, text140);
				text141 = text137 + " = " + text141 + ";";
				text142 = text138 + " = " + text142 + ";";
				if (text137 == null || text137 == "null")
				{
					text141 = null;
				}
				if (text138 == null || text138 == "null")
				{
					text142 = null;
				}
				return Class52.smethod_1(text141 + text142);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "imul")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get4OperandsSafe(class48_0, out var class48_145, out var class48_146, out var class48_147, out var class48_148);
				string text131 = GetDisplayString(class48_145);
				string text132 = GetDisplayString(class48_146);
				string text133 = GetDisplayString(class48_147, class48_145);
				string text134 = GetDisplayString(class48_148, class48_146);
				string text135 = BasicStringFormat("(%s * %s) >> 32", text133, text134);
				string text136 = BasicStringFormat("(%s * %s) & 0xffffffff", text133, text134);
				text135 = text131 + " = " + text135 + ";";
				text136 = text132 + " = " + text136 + ";";
				if (text131 == null || text131 == "null")
				{
					text135 = null;
				}
				if (text132 == null || text132 == "null")
				{
					text136 = null;
				}
				return Class52.smethod_1(text135 + text136);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "ui", "mad")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get4OperandsSafe(class48_0, out var class48_141, out var class48_142, out var class48_143, out var class48_144);
				string text126 = GetDisplayString(class48_141);
				string text127 = GetDisplayString(class48_142, class48_141);
				string text128 = GetDisplayString(class48_143, class48_141);
				string text129 = GetDisplayString(class48_144, class48_141);
				string text130 = null;
				text130 = ((text129[0] != '-') ? BasicStringFormat("%s + %s", text128, text129) : BasicStringFormat("%s - %s", text128, text129.Substring(1)));
				return hashSet_0.Contains("_sat") ? Class52.smethod_0("%s = saturate(%s * %s)", text126, text127, text130) : Class52.smethod_0("%s = %s * %s", text126, text127, text130);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "d", "div")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_138, out var class48_139, out var class48_140);
				string text122 = GetDisplayString(class48_138);
				string text123 = GetDisplayString(class48_139, class48_138);
				string text124 = GetDisplayString(class48_140, class48_138);
				string text125 = null;
				text125 = BasicStringFormat("%s / %s", text123, text124);
				return hashSet_0.Contains("_sat") ? Class52.smethod_0("%s = saturate(%s)", text122, text125) : Class52.smethod_0("%s = %s", text122, text125);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "udiv")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get4OperandsSafe(class48_0, out var class48_134, out var class48_135, out var class48_136, out var class48_137);
				string text116 = GetDisplayString(class48_134);
				string text117 = GetDisplayString(class48_135);
				string text118 = GetDisplayString(class48_136, class48_134);
				string text119 = GetDisplayString(class48_137, class48_135);
				string text120 = BasicStringFormat("%s / %s", text118, text119);
				string text121 = BasicStringFormat("%s *(1 / %s)", text118, text119);
				text120 = text116 + " = " + text120 + ";";
				text121 = text117 + " = " + text121 + ";";
				if (text116 == null || text116 == "null")
				{
					text120 = null;
				}
				if (text117 == null || text117 == "null")
				{
					text121 = null;
				}
				return Class52.smethod_1(text120 + text121);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "uid", "max")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_131, out var class48_132, out var class48_133);
				string text113 = GetDisplayString(class48_131);
				string text114 = GetDisplayString(class48_132, class48_131);
				string text115 = GetDisplayString(class48_133, class48_131);
				return Class52.smethod_0("%s = max(%s, %s)", text113, text114, text115);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "uid", "min")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_128, out var class48_129, out var class48_130);
				string text110 = GetDisplayString(class48_128);
				string text111 = GetDisplayString(class48_129, class48_128);
				string text112 = GetDisplayString(class48_130, class48_128);
				return Class52.smethod_0("%s = min(%s, %s)", text110, text111, text112);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "sincos")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_125, out var class48_126, out var class48_127);
				string text104 = GetDisplayString(class48_125);
				string text105 = GetDisplayString(class48_126);
				string text106 = GetDisplayString(class48_127, class48_125);
				string text107 = GetDisplayString(class48_127, class48_126);
				string text108 = BasicStringFormat("sin(%s)", text106);
				string text109 = BasicStringFormat("cos(%s)", text107);
				if (hashSet_0.Contains("_sat"))
				{
					text108 = "saturate(" + text108 + ")";
					text109 = "saturate(" + text109 + ")";
				}
				text108 = text104 + " = " + text108 + ";";
				text109 = text105 + " = " + text109 + ";";
				if (text104 == null || text104 == "null")
				{
					text108 = null;
				}
				if (text105 == null || text105 == "null")
				{
					text109 = null;
				}
				return Class52.smethod_1(text108 + text109);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "log")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_122, out var class48_123, out var _);
				string text102 = GetDisplayString(class48_122);
				string text103 = GetDisplayString(class48_123, class48_122);
				return Class52.smethod_0("%s = log(%s)", text102, text103);
			};
			dictionary_3[delegate (string string_0)
			{
				string[] array = OpcodeMatches(string_0, "", "sample_l_indexable");
				return (array != null) ? array : OpcodeMatches(string_0, "", "sample");
			}
			] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get5OperandsSafe(class48_0, out var class48_117, out var class48_118, out var class48_119, out var class48_120, out var class48_121);
				string text99 = GetDisplayString(class48_117);
				GetDisplayString(out var string_28, out var string_29, class48_118, null, getBodyAndMaskSeparately: true);
				GetDisplayString(out var string_30, out var string_31, class48_119, class48_117, getBodyAndMaskSeparately: true);
				GetDisplayString(class48_120);
				string text100 = GetDisplayString(class48_121);
				string text101 = "tex2D";
				string string_32 = "";
				ShaderParamBinding @class = GetParamBindingAndFixSuffix(class48_119?.name, class48_119?.suffix, ref string_32);
				if (@class?.desc?.valueType?.ToLower() == "texture3d".ToLower())
				{
					text101 = "tex3D";
				}
				else if (@class?.desc?.valueType?.ToLower() == "texture1d".ToLower())
				{
					text101 = "tex1D";
				}
				else if (@class?.desc?.valueType?.ToLower() == "texture2DArray".ToLower())
				{
					text101 = "texture2DArray";
					if (text100 != null && text100 != "null")
					{
						return Class52.smethod_1(text99 + " = SAMPLE_TEXTURE2D_ARRAY(" + string_30 + ", sampler_" + string_30 + ", " + string_28 + ((string_29 == null) ? null : ("." + string_29.Substring(0, Math.Min(string_29.Length, 2)))) + ", " + text100 + ") ");
					}
					return Class52.smethod_1(text99 + " = SAMPLE_TEXTURE2D_ARRAY(" + string_30 + ", sampler_" + string_30 + ", " + string_28 + ((string_29 == null) ? null : ("." + string_29.Substring(0, Math.Min(string_29.Length, 2)))) + ") ");
				}
				return Class52.smethod_0("%s = " + text101 + "(%s, %s%s).%s  /* " + class47_0.src + " */ ", text99, string_30, string_28, (string_29 == null) ? null : ("." + string_29.Substring(0, Math.Min(string_29.Length, 2))), string_31);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "gather4")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get4OperandsSafe(class48_0, out var class48_113, out var class48_114, out var class48_115, out var class48_116);
				string text97 = GetDisplayString(class48_113);
				GetDisplayString(out var string_24, out var string_25, class48_114, null, getBodyAndMaskSeparately: true);
				GetDisplayString(out var string_26, out var string_27, class48_115, class48_113, getBodyAndMaskSeparately: true);
				string text98 = GetDisplayString(class48_116);
				return Class52.smethod_0("%s = textureGather(%s, %s%s).%s /*sample_state %s*/", text97, string_26, string_24, (string_25 == null) ? null : ("." + string_25.Substring(0, Math.Min(string_25.Length, 2))), string_27, text98);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "deriv_rty")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_111, out var class48_112);
				string text93 = GetDisplayString(class48_111);
				string text94 = GetDisplayString(class48_112, class48_111);
				string string_23 = "y";
				string text95 = null;
				if (hashSet_0.Contains("_coarse"))
				{
					text95 = "_coarse";
				}
				if (hashSet_0.Contains("_fine"))
				{
					text95 = "_fine";
				}
				string text96 = BasicStringFormat("dd%s%s(%s)", string_23, text95, text94);
				if (hashSet_0.Contains("_sat"))
				{
					text96 = "saturate(" + text96 + ")";
				}
				return Class52.smethod_0("%s = %s", text93, text96);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "deriv_rtx")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_109, out var class48_110);
				string text89 = GetDisplayString(class48_109);
				string text90 = GetDisplayString(class48_110, class48_109);
				string string_22 = "x";
				string text91 = null;
				if (hashSet_0.Contains("_coarse"))
				{
					text91 = "_coarse";
				}
				if (hashSet_0.Contains("_fine"))
				{
					text91 = "_fine";
				}
				string text92 = BasicStringFormat("dd%s%s(%s)", string_22, text91, text90);
				if (hashSet_0.Contains("_sat"))
				{
					text92 = "saturate(" + text92 + ")";
				}
				return Class52.smethod_0("%s = %s", text89, text92);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "di", "eq")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_106, out var class48_107, out var class48_108);
				string text86 = GetDisplayString(class48_106);
				string text87 = GetDisplayString(class48_107, class48_106);
				string text88 = GetDisplayString(class48_108, class48_106);
				return Class52.smethod_0("%s = %s == %s", text86, text87, text88);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "di", "ne")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_103, out var class48_104, out var class48_105);
				string text83 = GetDisplayString(class48_103);
				string text84 = GetDisplayString(class48_104, class48_103);
				string text85 = GetDisplayString(class48_105, class48_103);
				return Class52.smethod_0("%s = %s != %s", text83, text84, text85);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "not")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_101, out var class48_102);
				string text81 = GetDisplayString(class48_101);
				string text82 = GetDisplayString(class48_102);
				return Class52.smethod_0("%s = !%s", text81, text82);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "uid", "lt")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_98, out var class48_99, out var class48_100);
				string text78 = GetDisplayString(class48_98);
				string text79 = GetDisplayString(class48_99, class48_98);
				string text80 = GetDisplayString(class48_100, class48_98);
				return Class52.smethod_0("%s = %s < %s", text78, text79, text80);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "uid", "ge")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_95, out var class48_96, out var class48_97);
				string text75 = GetDisplayString(class48_95);
				string text76 = GetDisplayString(class48_96, class48_95);
				string text77 = GetDisplayString(class48_97, class48_95);
				return Class52.smethod_0("%s = %s >= %s", text75, text76, text77);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "ui", "shl")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_92, out var class48_93, out var class48_94);
				string text72 = GetDisplayString(class48_92);
				string text73 = GetDisplayString(class48_93);
				string text74 = GetDisplayString(class48_94);
				return Class52.smethod_0("%s = %s << %s", text72, text73, text74);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "ui", "shr")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_89, out var class48_90, out var class48_91);
				string text69 = GetDisplayString(class48_89);
				string text70 = GetDisplayString(class48_90);
				string text71 = GetDisplayString(class48_91);
				return Class52.smethod_0("%s = %s >> %s", text69, text70, text71);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "discard")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_87, out var _);
				string text68 = GetDisplayString(class48_87);
				if (hashSet_0.Contains("_z"))
				{
					return Class52.smethod_0("if (%s == 0) discard", text68);
				}
				return hashSet_0.Contains("_nz") ? Class52.smethod_0("if (%s != 0) discard", text68) : Class52.smethod_0("discard");
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "if")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_85, out var _);
				string text67 = GetDisplayString(class48_85);
				if (hashSet_0.Contains("_z"))
				{
					return Class52.smethod_1(BasicStringFormat("if (%s == 0) {", text67), "if");
				}
				return hashSet_0.Contains("_nz") ? Class52.smethod_1(BasicStringFormat("if (%s != 0) {", text67), "if") : Class52.smethod_0("discard");
			};
			dictionary_3[(string string_0) => (!(string_0 == "else")) ? null : new string[0]] = (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0) => Class52.smethod_1("} else {", "else");
			dictionary_3[(string string_0) => (!(string_0 == "endif")) ? null : new string[0]] = (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0) => Class52.smethod_1("}", "endif");
			dictionary_3[(string string_0) => (!(string_0 == "break")) ? null : new string[0]] = (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0) => Class52.smethod_1("break", "break");
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "breakc")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_83, out var _);
				string text66 = GetDisplayString(class48_83);
				if (text66 == null)
				{
					return Class52.smethod_1("break", "break");
				}
				if (hashSet_0.Contains("_z"))
				{
					return Class52.smethod_0("if (%s == 0) break", text66);
				}
				return hashSet_0.Contains("_nz") ? Class52.smethod_0("if (%s != 0) break", text66) : Class52.smethod_0("// ERROR for break " + text66);
			};
			dictionary_3[(string string_0) => (!(string_0 == "loop")) ? null : new string[0]] = (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0) => Class52.smethod_1("while(true) {", "loop");
			dictionary_3[(string string_0) => (!(string_0 == "endloop")) ? null : new string[0]] = (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0) => Class52.smethod_1("}", "endloop");
			dictionary_3[(string string_0) => (!(string_0 == "continue")) ? null : new string[0]] = (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0) => Class52.smethod_1("continue");
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "continuec")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_81, out var _);
				string text65 = GetDisplayString(class48_81);
				if (text65 == null)
				{
					return Class52.smethod_1("continue");
				}
				if (hashSet_0.Contains("_z"))
				{
					return Class52.smethod_0("if (%s == 0) continue", text65);
				}
				return hashSet_0.Contains("_nz") ? Class52.smethod_0("if (%s != 0) continue", text65) : Class52.smethod_0("// ERROR for continue " + text65);
			};
			dictionary_3[(string string_0) => (!(string_0 == "rsq")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_79, out var class48_80);
				string text63 = GetDisplayString(class48_79);
				string text64 = GetDisplayString(class48_80, class48_79);
				return Class52.smethod_0("%s = rsqrt(%s)", text63, text64);
			};
			dictionary_3[(string string_0) => (!(string_0 == "sqrt")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_77, out var class48_78);
				string text61 = GetDisplayString(class48_77);
				string text62 = GetDisplayString(class48_78, class48_77);
				return Class52.smethod_0("%s = sqrt(%s)", text61, text62);
			};
			dictionary_3[(string string_0) => (!(string_0 == "frc")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_75, out var class48_76);
				string text59 = GetDisplayString(class48_75);
				string text60 = GetDisplayString(class48_76, class48_75);
				return Class52.smethod_0("%s = frac(%s)", text59, text60);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "d", "rcp")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_73, out var class48_74);
				string text57 = GetDisplayString(class48_73);
				string text58 = GetDisplayString(class48_74, class48_73);
				return Class52.smethod_0("%s = rcp(%s)", text57, text58);
			};
			dictionary_3[(string string_0) => (!(string_0 == "exp")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_71, out var class48_72);
				string text55 = GetDisplayString(class48_71);
				string text56 = GetDisplayString(class48_72);
				return Class52.smethod_0("%s = exp(%s)", text55, text56);
			};
			dictionary_3[(string string_0) => (!(string_0 == "round_ni")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_69, out var class48_70);
				string text53 = GetDisplayString(class48_69);
				string text54 = GetDisplayString(class48_70);
				return Class52.smethod_0("%s = floor(%s)", text53, text54);
			};
			dictionary_3[(string string_0) => (!(string_0 == "round_pi")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_67, out var class48_68);
				string text51 = GetDisplayString(class48_67);
				string text52 = GetDisplayString(class48_68);
				return Class52.smethod_0("%s = ceil(%s)", text51, text52);
			};
			dictionary_3[(string string_0) => (!(string_0 == "round_ne")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_65, out var class48_66);
				string text49 = GetDisplayString(class48_65);
				string text50 = GetDisplayString(class48_66);
				return Class52.smethod_0("%s = floor(%s) /* round_ne, nearest even */", text49, text50);
			};
			dictionary_3[(string string_0) => (!(string_0 == "round_z")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_63, out var class48_64);
				string text47 = GetDisplayString(class48_63);
				string text48 = GetDisplayString(class48_64);
				return Class52.smethod_0("%s = trunc(%s) /* round_z, round towards zero */", text47, text48);
			};
			dictionary_3[(string string_0) => (!(string_0 == "ftoi")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_61, out var class48_62);
				string text45 = GetDisplayString(class48_61);
				string text46 = GetDisplayString(class48_62);
				return Class52.smethod_0("%s = floor(%s) /* ftoi */", text45, text46);
			};
			dictionary_3[(string string_0) => (!(string_0 == "ftou")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_59, out var class48_60);
				string text43 = GetDisplayString(class48_59);
				string text44 = GetDisplayString(class48_60);
				return Class52.smethod_0("%s = floor(%s) /* ftou */", text43, text44);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "uid", "tof")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_57, out var class48_58);
				string text41 = GetDisplayString(class48_57);
				string text42 = GetDisplayString(class48_58);
				return Class52.smethod_0("%s = %s /* round-to-nearest-even rounding */", text41, text42);
			};
			dictionary_3[(string string_0) => (!(string_0 == "and")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_54, out var class48_55, out var class48_56);
				string text38 = GetDisplayString(class48_54);
				string text39 = GetDisplayString(class48_55, class48_54);
				string text40 = GetDisplayString(class48_56, class48_54);
				if (text40.Contains("0x3f800000"))
				{
					return Class52.smethod_0("%s = %s /* and 0x3f800000 */", text38, text39);
				}
				if (class48_54.suffix != null)
				{
					text39 = "uint" + ((class48_54.suffix.Length > 1) ? string.Concat(class48_54.suffix.Length) : "") + "(" + text39 + ")";
					text40 = "uint" + ((class48_54.suffix.Length > 1) ? string.Concat(class48_54.suffix.Length) : "") + "(" + text40 + ")";
				}
				return Class52.smethod_0("%s = %s & %s", text38, text39, text40);
			};
			dictionary_3[(string string_0) => (!(string_0 == "or")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_51, out var class48_52, out var class48_53);
				string text35 = GetDisplayString(class48_51);
				string text36 = GetDisplayString(class48_52, class48_51);
				string text37 = GetDisplayString(class48_53, class48_51);
				if (class48_51.suffix != null)
				{
					text36 = "uint" + ((class48_51.suffix.Length > 1) ? string.Concat(class48_51.suffix.Length) : "") + "(" + text36 + ")";
					text37 = "uint" + ((class48_51.suffix.Length > 1) ? string.Concat(class48_51.suffix.Length) : "") + "(" + text37 + ")";
				}
				return Class52.smethod_0("%s = %s | %s", text35, text36, text37);
			};
			dictionary_3[(string string_0) => (!(string_0 == "xor")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_48, out var class48_49, out var class48_50);
				string text32 = GetDisplayString(class48_48);
				string text33 = GetDisplayString(class48_49, class48_48);
				string text34 = GetDisplayString(class48_50, class48_48);
				if (class48_48.suffix != null)
				{
					text33 = "uint" + ((class48_48.suffix.Length > 1) ? string.Concat(class48_48.suffix.Length) : "") + "(" + text33 + ")";
					text34 = "uint" + ((class48_48.suffix.Length > 1) ? string.Concat(class48_48.suffix.Length) : "") + "(" + text34 + ")";
				}
				return Class52.smethod_0("%s = %s ^ %s", text32, text33, text34);
			};
			dictionary_3[(string string_0) => (!(string_0 == "ret")) ? null : new string[0]] = (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0) => Class52.smethod_1("return");
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "retc")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_46, out var _);
				string text31 = GetDisplayString(class48_46);
				if (text31 == null)
				{
					return Class52.smethod_1("return");
				}
				if (hashSet_0.Contains("_z"))
				{
					return Class52.smethod_0("if (%s == 0) return", text31);
				}
				return hashSet_0.Contains("_nz") ? Class52.smethod_0("if (%s != 0) return", text31) : Class52.smethod_0("// ERROR for return " + text31);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "vs_")] = (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0) => Class52.smethod_1("// Vertex shader");
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "ps_")] = (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0) => Class52.smethod_1("// Pixel shader");
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "cs_")] = (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0) => Class52.smethod_1("// CS shader");
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "dcl")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_44, out var _);
				if (hashSet_0.Contains("_temps"))
				{
					string s = GetDisplayString(class48_44);
					List<string> list = new List<string>();
					for (int i = 0; i < int.Parse(s); i++)
					{
						string text30 = "u_xlat" + i;
						list.Add("float4 " + text30 + ";");
						displayToShaderParamBinding["r" + i] = new ShaderParamBinding
						{
							name = text30,
							bind = "r" + i
						};
					}
					if (list.Count > 0)
					{
						return Class52.smethod_0(string.Join("\n", list.ToArray()));
					}
					return Class52.smethod_0("");
				}
				if (hashSet_0.Contains("_resource") && (hashSet_0.Contains("_texture2darray") || hashSet_0.Contains("_texture3d") || hashSet_0.Contains("_texture2d") || hashSet_0.Contains("_texture1d")))
				{
					if (displayToShaderParamBinding.TryGetValue(class48_44.name?.ToLower(), out var value))
					{
						if (hashSet_0.Contains("_texture2darray"))
						{
							value.desc.valueType = "texture2darray";
						}
						if (hashSet_0.Contains("_texture3d"))
						{
							value.desc.valueType = "texture3d";
						}
						if (hashSet_0.Contains("_texture2d"))
						{
							value.desc.valueType = "texture2d";
						}
						if (hashSet_0.Contains("_texture1d"))
						{
							value.desc.valueType = "texture1d";
						}
						return Class52.smethod_0(null);
					}
					return Class52.smethod_0("");
				}
				if (hashSet_0.Contains("_constantbuffer"))
				{
					if (displayToShaderParamBinding.TryGetValue(class48_44.name.ToLower(), out var value2))
					{
						if (value2.desc.valueType.EndsWith("4"))
						{
							value2.int_0 = int.Parse(class48_44.idx) - 4 * ((value2.desc.arraySize == 0) ? 1 : value2.desc.arraySize);
						}
						if (value2.desc.valueType.EndsWith("3"))
						{
							value2.int_0 = int.Parse(class48_44.idx) - 3;
						}
						if (value2.desc.valueType.EndsWith("2"))
						{
							value2.int_0 = int.Parse(class48_44.idx) - 2;
						}
						if (value2.desc.valueType.EndsWith("1"))
						{
							value2.int_0 = int.Parse(class48_44.idx) - 1;
						}
						return Class52.smethod_0(null);
					}
					return Class52.smethod_0("// " + class48_44.name + "  " + class48_44.idx + " not find binding");
				}
				if (hashSet_0.Contains("_immediateConstantBuffer"))
				{
					string string_21 = "iConstantBuffer";
					ShaderParamDesc class50_ = new ShaderParamDesc
					{
						valueType = "float4",
						name = "iConstantBuffer",
						isMatrix = true,
						mask = "xyzw"
					};
					displayToShaderParamBinding["icb"] = new ShaderParamBinding
					{
						name = string_21,
						bind = "icb",
						desc = class50_
					};
					return Class52.smethod_0("float4[] " + string_21 + " = " + class48_44.name + ";");
				}
				return Class52.smethod_0(null);
			};
			dictionary_3[(string string_0) => (!(string_0 == "bfi")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get4OperandsSafe(class48_0, out var class48_40, out var class48_41, out var class48_42, out var class48_43);
				string text26 = GetDisplayString(class48_40);
				string text27 = GetDisplayString(class48_41);
				string text28 = GetDisplayString(class48_42);
				string text29 = GetDisplayString(class48_43);
				return Class52.smethod_0("bitmask = (((1 << %s) - 1) << %s) & 0xffffffff;  dest = ((%s << %s) & bitmask) | (%s & ~bitmask)", text26, text27, text28, text27, text29);
			};
			dictionary_3[(string string_0) => (!(string_0 == "bfrev")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_38, out var class48_39);
				string text24 = GetDisplayString(class48_38);
				string text25 = GetDisplayString(class48_39);
				return Class52.smethod_0("%s = reverse_bit(%s)", text24, text25);
			};
			dictionary_3[(string string_0) => (!(string_0 == "countbits")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_36, out var class48_37);
				string text22 = GetDisplayString(class48_36);
				string text23 = GetDisplayString(class48_37);
				return Class52.smethod_0("%s = countbits(%s)", text22, text23);
			};
			dictionary_3[(string string_0) => (!(string_0 == "sat")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_34, out var class48_35);
				string text20 = GetDisplayString(class48_34);
				string text21 = GetDisplayString(class48_35);
				return Class52.smethod_0("%s = saturate(%s)", text20, text21);
			};
			dictionary_3[(string string_0) => (!(string_0 == "neg")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_32, out var class48_33);
				string text18 = GetDisplayString(class48_32);
				string text19 = GetDisplayString(class48_33);
				return Class52.smethod_0("%s = -%s", text18, text19);
			};
			dictionary_3[(string string_0) => (!(string_0 == "abs")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_30, out var class48_31);
				string text16 = GetDisplayString(class48_30);
				string text17 = GetDisplayString(class48_31);
				return Class52.smethod_0("%s = abs(%s)", text16, text17);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "ld_structured")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get4OperandsSafe(class48_0, out var class48_26, out var class48_27, out var class48_28, out var class48_29);
				string text15 = GetDisplayString(class48_26);
				GetDisplayString(out var string_15, out var string_16, class48_27, null, getBodyAndMaskSeparately: true);
				GetDisplayString(out var string_17, out var _, class48_28, null, getBodyAndMaskSeparately: true);
				GetDisplayString(out var string_19, out var string_20, class48_29, class48_26, getBodyAndMaskSeparately: true);
				return Class52.smethod_0("%s = %s[%s%s][%s].%s  /* " + class47_0.src + " */", text15, string_19, string_15, (string_16 == null) ? null : ("." + string_16.Substring(0, Math.Min(string_16.Length, 2))), string_17, string_20);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "ld_indexable")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get4OperandsSafe(class48_0, out var class48_22, out var class48_23, out var class48_24, out var class48_25);
				string text14 = GetDisplayString(class48_22);
				GetDisplayString(out var string_9, out var string_10, class48_23, null, getBodyAndMaskSeparately: true);
				GetDisplayString(out var string_11, out var _, class48_24, null, getBodyAndMaskSeparately: true);
				GetDisplayString(out var string_13, out var string_14, class48_25, class48_22, getBodyAndMaskSeparately: true);
				return Class52.smethod_0("%s = %s[%s.%s][%s].%s /* ld */", text14, string_13, string_9, string_10?.Substring(0, Math.Min(string_10.Length, 2)), string_11, string_14);
			};
			dictionary_3[(string string_0) => (!(string_0 == "ld")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get4OperandsSafe(class48_0, out var class48_18, out var class48_19, out var class48_20, out var class48_21);
				string text13 = GetDisplayString(class48_18);
				GetDisplayString(out var string_3, out var string_4, class48_19, null, getBodyAndMaskSeparately: true);
				GetDisplayString(out var string_5, out var _, class48_20, null, getBodyAndMaskSeparately: true);
				GetDisplayString(out var string_7, out var string_8, class48_21, class48_18, getBodyAndMaskSeparately: true);
				return Class52.smethod_0("%s = %s[%s.%s][%s].%s /* ld */", text13, string_7, string_3, string_4?.Substring(0, Math.Min(string_4.Length, 2)), string_5, string_8);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "store_structured")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get4OperandsSafe(class48_0, out var class48_14, out var class48_15, out var class48_16, out var class48_17);
				GetDisplayString(out var string_, out var string_2, class48_14, null, getBodyAndMaskSeparately: true);
				string text10 = GetDisplayString(class48_15);
				string text11 = GetDisplayString(class48_16);
				string text12 = GetDisplayString(class48_17, class48_14);
				return Class52.smethod_0("%s[%s][%s].%s = %s /* store structured*/", string_, text10, text11, string_2, text12);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "store_uav_typed")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get3OperandsSafe(class48_0, out var class48_11, out var class48_12, out var class48_13);
				string text7 = GetDisplayString(class48_11);
				string text8 = GetDisplayString(class48_12);
				string text9 = GetDisplayString(class48_13);
				return Class52.smethod_0("%s[%s] = %s", text7, text8, text9);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "sync")] = (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0) => Class52.smethod_0("sync");
			dictionary_3[(string string_0) => (!(string_0 == "call")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_9, out var _);
				string text6 = GetDisplayString(class48_9);
				return Class52.smethod_0("goto %s", text6);
			};
			dictionary_3[(string string_0) => OpcodeMatches(string_0, "", "callc")] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_7, out var class48_8);
				string text4 = GetDisplayString(class48_7);
				string text5 = GetDisplayString(class48_8);
				if (text4 == null)
				{
					return Class52.smethod_1("goto %s", text5);
				}
				if (hashSet_0.Contains("_z"))
				{
					return Class52.smethod_0("if (%s == 0) goto %s", text4, text5);
				}
				return hashSet_0.Contains("_nz") ? Class52.smethod_0("if (%s != 0) goto %s", text4, text5) : Class52.smethod_0("// ERROR for goto  " + text4 + " " + text5);
			};
			dictionary_3[(string string_0) => (!(string_0 == "label")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_5, out var _);
				string text3 = GetDisplayString(class48_5);
				return Class52.smethod_0("%s: ", text3);
			};
			dictionary_3[(string string_0) => (!(string_0 == "case")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_3, out var _);
				string text2 = GetDisplayString(class48_3);
				return Class52.smethod_1(BasicStringFormat("case %s:", text2), "case");
			};
			dictionary_3[(string string_0) => (!(string_0 == "default")) ? null : new string[0]] = (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0) => Class52.smethod_1(BasicStringFormat("default:"), "default");
			dictionary_3[(string string_0) => (!(string_0 == "switch")) ? null : new string[0]] = delegate (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
			{
				Get2OperandsSafe(class48_0, out var class48_, out var _);
				string text = GetDisplayString(class48_);
				return Class52.smethod_1(BasicStringFormat("switch(%s) {", text), "switch");
			};
			dictionary_3[(string string_0) => (!(string_0 == "endswitch")) ? null : new string[0]] = (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0) => Class52.smethod_1(BasicStringFormat("}"), "endswitch");
			dictionary_3[(string string_0) => (!(string_0 == "cut")) ? null : new string[0]] = (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0) => Class52.smethod_0("cut()");
			dictionary_3[(string string_0) => (!(string_0 == "emit")) ? null : new string[0]] = (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0) => Class52.smethod_0("emit()");
			dictionary_3[(string string_0) => (!(string_0 == "nop")) ? null : new string[0]] = (HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0) => Class52.smethod_0("// nop");
			return dictionary_3;
		}
	}

	private ShaderParamBinding GetParamBindingAndFixSuffix(string name, string originalSuffix, ref string croppedSuffix)
	{
		if (!string.IsNullOrEmpty(originalSuffix))
		{
			foreach (ShaderParamBinding item in shaderParamBindings)
			{
				if (!(item.bind == name) || item.mask == null)
				{
					continue;
				}
				bool flag;
				//check if every letter in original suffix is in this param binding
				if (!(flag = item.mask == originalSuffix))
				{
					foreach (char value in originalSuffix)
					{
						if (item.mask.IndexOf(value) >= 0)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					continue;
				}
				if (!string.IsNullOrEmpty(croppedSuffix))
				{
					if (item.mask == "zw")
					{
						croppedSuffix = croppedSuffix.Replace("z", "x").Replace("w", "y");
					}
					if (item.mask.Length == 1)
					{
						croppedSuffix = croppedSuffix.Replace("y", "x").Replace("z", "x").Replace("w", "x");
					}
				}
				return item;
			}
		}
		//includes temporary registers
		if (displayToShaderParamBinding.TryGetValue(name, out var value2))
		{
			return value2;
		}
		return null;
	}

	internal string CropMaskToDestOperand(InstructionOperand operand, InstructionOperand destOperand)
	{
		if (string.IsNullOrEmpty(destOperand?.suffix))
		{
			return operand.suffix;
		}
		if (string.IsNullOrEmpty(operand?.suffix))
		{
			return null;
		}
		string destOperandSuffix = destOperand.suffix;
		List<int> destOperandSuffixInt = new List<int>();
		for (int i = 0; i < destOperandSuffix.Length; i++)
		{
			destOperandSuffixInt.Add(maskToIndex[destOperandSuffix.Substring(i, 1)]);
		}
		List<string> result = new List<string>();
		string operandSuffix = operand.suffix;
		string operandSuffixLast = operandSuffix.Substring(operandSuffix.Length - 1, 1);
		for (int j = 0; j < destOperandSuffixInt.Count; j++)
		{
			int num = destOperandSuffixInt[j];
			if (num >= operandSuffix.Length)
			{
				result.Add(operandSuffixLast);
			}
			else
			{
				result.Add(operandSuffix.Substring(num, 1));
			}
		}
		return string.Join("", result.ToArray());
	}

	private string[] method_2(string[] string_0, InstructionOperand class48_0)
	{
		if (!IsObjectTrue(class48_0.suffix))
		{
			return string_0;
		}
		string string_ = class48_0.suffix;
		List<int> list = new List<int>();
		for (int i = 0; i < string_.Length; i++)
		{
			list.Add(maskToIndex[string_.Substring(i, 1)]);
		}
		List<string> list2 = new List<string>();
		string item = string_0[string_0.Length - 1];
		for (int j = 0; j < list.Count; j++)
		{
			int num = list[j];
			if (num >= string_0.Length)
			{
				list2.Add(item);
			}
			else
			{
				list2.Add(string_0[num]);
			}
		}
		return list2.ToArray();
	}

	private int method_3(InstructionOperand class48_0)
	{
		string name = class48_0.name;
		string suffix = class48_0.suffix;
		if (GetParamBindingAndFixSuffix(name, class48_0.suffix, ref suffix) != null && class48_0.idxNotNullOrEmpty)
		{
			return int.Parse(class48_0.idx) * 16 + maskToOffset[class48_0.suffix.Substring(0, 1)];
		}
		return 0;
	}

	//1 if bool==true, string.length>0, or array.length>0
	private static bool IsObjectTrue(object object_0)
	{
		if (object_0 == null)
		{
			return false;
		}
		object obj;
		if ((obj = object_0) is bool)
		{
			return (bool)obj;
		}
		string value;
		if ((value = object_0 as string) != null)
		{
			return !string.IsNullOrEmpty(value);
		}
		Array array;
		if ((array = object_0 as Array) != null)
		{
			return array.Length > 0;
		}
		return !string.IsNullOrEmpty(object_0.ToString());
	}

	internal string GetDisplayString(InstructionOperand operand, InstructionOperand destOperand = null, bool bool_1 = false)
	{
		GetDisplayString(out var string_, out var _, operand, destOperand, bool_1);
		return string_;
	}

	internal void GetDisplayString(out string string_0, out string string_1, InstructionOperand operand, InstructionOperand destOperand = null, bool getBodyAndMaskSeparately = false)
	{
		string_0 = null;
		string_1 = null;
		if (operand == null)
		{
			return;
		}
		string operandName = operand?.name;
		string croppedSuffix = operand?.suffix;
		if (destOperand != null && !string.IsNullOrEmpty(croppedSuffix))
		{
			croppedSuffix = CropMaskToDestOperand(operand, destOperand);
		}
		ShaderParamBinding paramBinding = null;
		if (operandName != null)
		{
			paramBinding = GetParamBindingAndFixSuffix(operandName, operand.suffix, ref croppedSuffix);
		}
		string text2 = null;
		string text3 = ".";
		if (paramBinding != null)
		{
			operandName = paramBinding.name;
			if (paramBinding.desc != null)
			{
				if (paramBinding.desc.isUniform)
				{
					if (paramBinding.desc.isVector || paramBinding.desc.isUAV)
					{
						text3 = ".";
						text2 = null;
					}
					if (paramBinding.desc.isMatrix && IsObjectTrue(operand.idx) && operand.idx.Contains("+"))
					{
						string[] array = operand.idx.Split('+');
						for (int i = 0; i < array.Length; i++)
						{
							array[i] = array[i]?.Trim();
							if (!char.IsDigit(array[i][0]))
							{
								InstructionOperand class2 = InstructionOperand.ParseFromString(array[i]);
								if (class2 != null)
								{
									array[i] = GetDisplayString(class2);
								}
							}
						}
						if (char.IsDigit(array[array.Length - 1][0]))
						{
							array[array.Length - 1] = (int.Parse(array[array.Length - 1]) - paramBinding.int_0).ToString();
						}
						text3 = "";
						text2 = "[" + string.Join(" + ", array) + "]";
					}
					else if (paramBinding.desc.isMatrix && IsObjectTrue(operand.idx) && operand.idxIsDigitOnly)
					{
						text3 = "";
						text2 = "[" + (int.Parse(operand.idx) - paramBinding.int_0) + "]";
					}
				}
				else
				{
					Class51[] class51_ = paramBinding.class51_0;
					if (IsObjectTrue(operand.idx) && operand.idxIsDigitOnly)
					{
						int num = method_3(operand);
						int? num2 = null;
						int num3 = class51_.Length - 1;
						while (num3 >= 0)
						{
							if (class51_[num3].offset > num)
							{
								num3--;
								continue;
							}
							num2 = num3;
							break;
						}
						if (num2.HasValue)
						{
							text2 = class51_[num2.Value].name;
							if (class51_[num2.Value].size > 16)
							{
								text2 = text2 + "[" + (num - class51_[num2.Value].offset) / 16 + "]";
							}
						}
						else
						{
							Console.WriteLine("cant find var offset " + operandName);
						}
					}
				}
			}
		}
		else if (IsObjectTrue(operand.idx))
		{
			text2 = "[" + operand.idx + "]";
			text3 = "";
		}
		if (IsObjectTrue(text2) && IsObjectTrue(croppedSuffix))
		{
			text2 = text2 + "." + croppedSuffix;
		}
		else if (!IsObjectTrue(text2))
		{
			if (IsObjectTrue(croppedSuffix))
			{
				text2 = croppedSuffix;
			}
			else
			{
				text3 = "";
			}
		}
		object obj;
		if (operand.vals != null && operand.vals.Length != 0)
		{
			if (!IsObjectTrue(destOperand))
			{
				obj = null;
			}
			else
			{
				obj = method_2(operand.vals, destOperand);
				if (obj != null)
				{
					goto IL_0385;
				}
			}
			obj = operand.vals;
			goto IL_0385;
		}
		goto IL_03bc;
		IL_0385:
		string[] array2 = (string[])obj;
		int num4 = array2.Length;
		operandName = ((num4 != 1) ? string.Format("float{0}({1})", num4, string.Join(", ", array2)) : array2[0].ToString());
		goto IL_03bc;
		IL_03bc:
		if (IsObjectTrue(getBodyAndMaskSeparately))
		{
			string_0 = operandName;
			string_1 = text2;
			return;
		}
		string text4 = null;
		text4 = ((!IsObjectTrue(text2)) ? operandName : (operandName + text3 + text2));
		if (operand.abs)
		{
			text4 = "abs(" + text4 + ")";
		}
		if (operand.neg)
		{
			text4 = "-" + text4;
		}
		string_0 = text4;
	}

	private static string BasicStringFormat(string format, params object[] args)
	{
		if (string.IsNullOrEmpty(format))
		{
			return format;
		}
		int num = 0;
		int num2 = 0;
		string text = "";
		while (true)
		{
			int num3 = format.IndexOf("%", num2);
			if (num3 < 0 || num3 + 1 >= format.Length)
			{
				break;
			}
			text += format.Substring(num2, num3 - num2);
			if (format[num3 + 1] != '%')
			{
				if (num < args.Length)
				{
					string text2 = args[num++]?.ToString();
					if (format[num3 + 1] == 'f' || format[num3 + 1] == 'g')
					{
						text2 = text2?.Replace(",", ".");
					}
					text += text2;
				}
			}
			else
			{
				text += format[num3 + 1];
			}
			num2 = num3 + 2;
		}
		if (num2 >= 0 && num2 < format.Length)
		{
			text += format.Substring(num2);
		}
		return text;
	}

	internal static string[] OpcodeMatches(string str, string optionalPrefix, string match)
	{
		if (!string.IsNullOrEmpty(optionalPrefix))
		{
			for (int i = 0; i < optionalPrefix.Length; i++)
			{
				string text = optionalPrefix[i] + match;
				if (str.StartsWith(text))
				{
					if (str.Length > text.Length)
					{
						return smethod_3(str.Substring(text.Length));
					}
					return new string[0];
				}
			}
		}
		if (str.StartsWith(match))
		{
			if (str.Length > match.Length)
			{
				return smethod_3(str.Substring(match.Length));
			}
			return new string[0];
		}
		return null;
	}

	private void Get2OperandsSafe(InstructionOperand[] class48_0, out InstructionOperand class48_1, out InstructionOperand class48_2)
	{
		Get4OperandsSafe(class48_0, out class48_1, out class48_2, out var _, out var _);
	}

	private void Get3OperandsSafe(InstructionOperand[] class48_0, out InstructionOperand class48_1, out InstructionOperand class48_2, out InstructionOperand class48_3)
	{
		Get4OperandsSafe(class48_0, out class48_1, out class48_2, out class48_3, out var _);
	}

	private void Get4OperandsSafe(InstructionOperand[] class48_0, out InstructionOperand class48_1, out InstructionOperand class48_2, out InstructionOperand class48_3, out InstructionOperand class48_4)
	{
		Get5OperandsSafe(class48_0, out class48_1, out class48_2, out class48_3, out class48_4, out var _);
	}

	private void Get5OperandsSafe(InstructionOperand[] class48_0, out InstructionOperand class48_1, out InstructionOperand class48_2, out InstructionOperand class48_3, out InstructionOperand class48_4, out InstructionOperand class48_5)
	{
		int num = 0;
		class48_1 = ((0 < class48_0.Length) ? class48_0[num++] : null);
		class48_2 = ((num < class48_0.Length) ? class48_0[num++] : null);
		class48_3 = ((num < class48_0.Length) ? class48_0[num++] : null);
		class48_4 = ((num < class48_0.Length) ? class48_0[num++] : null);
		class48_5 = ((num < class48_0.Length) ? class48_0[num++] : null);
	}

	private Class53 method_10(string string_0)
	{
		if (string.IsNullOrEmpty(string_0))
		{
			return null;
		}
		foreach (KeyValuePair<Delegate1, Delegate2> item in Dictionary_0)
		{
			string[] array = item.Key(string_0);
			if (array != null)
			{
				return new Class53
				{
					delegate1_0 = item.Key,
					string_0 = array
				};
			}
		}
		return null;
	}

	public string method_11(string string_0)
	{
		try
		{
			Class47[] class47_ = method_12(string_0);
			StringBuilder stringBuilder = new StringBuilder();
			stack_0.Push(new BlockTags());
			method_15(class47_, stringBuilder);
			return stringBuilder.ToString();
		}
		catch (Exception exception_)
		{
			Console.WriteLine(exception_);
			return null;
		}
	}

	private Class47[] method_12(string string_0)
	{
		string[] array = string_0.Replace("\r", "").Split('\n');
		method_13(array);
		List<Class47> list = new List<Class47>();
		if (string.IsNullOrEmpty(string_0))
		{
			return list.ToArray();
		}
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i];
			if (string.IsNullOrEmpty(text) || text.TrimStart().StartsWith("//"))
			{
				continue;
			}
			if (text.StartsWith("dcl_immediateConstantBuffer"))
			{
				text = text.Substring("dcl_immediateConstantBuffer".Length + 1);
				for (int j = i + 1; j < array.Length; j++)
				{
					string text2 = array[j]?.Trim();
					if (!text2.StartsWith("{") && !text2.StartsWith(","))
					{
						break;
					}
					text += text2;
					i++;
				}
				Class47 @class = new Class47("dcl_immediateConstantBuffer");
				@class.args = new InstructionOperand[1];
				@class.args[0] = new InstructionOperand();
				@class.args[0].name = text;
				list.Add(@class);
			}
			else
			{
				list.Add(new Class47(text));
			}
		}
		return list.ToArray();
	}

	private void method_13(string[] string_0)
	{
		Struct4 struct4_ = default(Struct4);
		struct4_.string_0 = string_0;
		for (int i = 0; i < struct4_.string_0.Length; i++)
		{
			if (struct4_.string_0[i] == null || !struct4_.string_0[i].StartsWith("ps_"))
			{
				if (struct4_.string_0[i] != null && struct4_.string_0[i].StartsWith("vs_"))
				{
					shaderType = ShaderTypeEnum.Vertex;
					break;
				}
				continue;
			}
			shaderType = ShaderTypeEnum.Pixel;
			break;
		}
		int num = smethod_4("Buffer Definitions:", 0, ref struct4_);
		int num2 = -1;
		int num3 = smethod_4("Resource Bindings:", 0, ref struct4_);
		int num4 = -1;
		if (num3 >= 0 && num2 < 0)
		{
			num2 = num3 - 1;
		}
		int num5 = smethod_4("Shader params:", 0, ref struct4_);
		int num6 = -1;
		if (num5 >= 0 && num4 < 0)
		{
			num4 = num5 - 1;
		}
		int num7 = smethod_4("Input signature:", 0, ref struct4_);
		int num8 = -1;
		if (num7 >= 0)
		{
			if (num2 < 0)
			{
				num2 = num7 - 1;
			}
			if (num4 < 0)
			{
				num4 = num7 - 1;
			}
			if (num6 < 0)
			{
				num6 = num7 - 1;
			}
		}
		int num9 = smethod_4("Output signature:", 0, ref struct4_);
		int num10 = -1;
		if (num7 >= 0)
		{
			if (num2 < 0)
			{
				num2 = num9 - 1;
			}
			if (num4 < 0)
			{
				num4 = num9 - 1;
			}
			if (num8 < 0)
			{
				num8 = num9 - 1;
			}
			if (num6 < 0)
			{
				num6 = num9 - 1;
			}
		}
		int num11 = num9;
		if (num11 < 0)
		{
			num11 = num7;
		}
		if (num11 < 0)
		{
			num11 = num5;
		}
		if (num11 < 0)
		{
			num11 = num3;
		}
		if (num11 < 0)
		{
			num11 = num;
		}
		if (num11 >= 0)
		{
			for (int j = num11 + 1; j < struct4_.string_0.Length && struct4_.string_0[j] != null; j++)
			{
				if (!struct4_.string_0[j].TrimStart(' ', '\t').StartsWith("//"))
				{
					break;
				}
				num11 = j;
			}
		}
		if (num11 >= 0)
		{
			if (num2 < 0)
			{
				num2 = num11;
			}
			if (num4 < 0)
			{
				num4 = num11;
			}
			if (num8 < 0)
			{
				num8 = num11;
			}
			if (num10 < 0)
			{
				num10 = num11;
			}
			if (num6 < 0)
			{
				num6 = num11;
			}
		}
		cbufferParams = new List<ShaderParamDesc>();
		bindingParams = new List<ShaderParamDesc>();
		if (num3 >= 0)
		{
			for (int k = num3 + 4; k < num4; k++)
			{
				string[] array = TrimIndentionAndSplitSpace(struct4_.string_0[k]);
				if (array.Length >= 6)
				{
					bindingParams.Add(new ShaderParamDesc
					{
						name = array[1],
						bind = array[5]
					});
				}
			}
		}
		shaderParams = new List<ShaderParamDesc>();
		if (num5 >= 0)
		{
			for (int l = num5 + 1; l < num6; l++)
			{
				string text = struct4_.string_0[l];
				string string_ = smethod_6(text, "name");
				string text2 = smethod_6(text, "type");
				string string_2 = smethod_6(text, "value_type");
				string text3 = smethod_6(text, "cbIndex");
				string text4 = smethod_6(text, "index");
				string text5 = smethod_6(text, "Uniform");
				string text6 = smethod_6(text, "arraySize");
				int int_ = 0;
				if (!string.IsNullOrEmpty(text6))
				{
					int_ = int.Parse(text6);
				}
				bool bool_ = text2 == "Matrix";
				bool bool_2 = text2 == "Vector";
				bool bool_3 = text2 == "UAV";
				string text7 = null;
				if (text3 != null)
				{
					text7 = "cb" + text3;
				}
				else
				{
					if (text2 == "Texture")
					{
						text7 = "t" + text4;
					}
					if (text2 == "BufferParam")
					{
						text7 = "t" + text4;
					}
					if (text2 == "Matrix")
					{
						text7 = "mat" + text4;
					}
					if (text2 == "Vector")
					{
						text7 = "vec" + text4;
					}
					if (text2 == "UAV")
					{
						text7 = "u" + text4;
					}
					if (text2 == "Sampler")
					{
						text7 = "s" + text4;
					}
				}
				if (text3 != null || text5 == "1")
				{
					if (text7 == null)
					{
						Console.WriteLine("ERR: " + text);
						continue;
					}
					shaderParams.Add(new ShaderParamDesc
					{
						name = string_,
						bind = text7,
						valueType = string_2,
						arraySize = int_,
						isMatrix = bool_,
						isVector = bool_2,
						isUAV = bool_3,
						isUniform = true
					});
				}
			}
		}
		inputParams = new List<ShaderParamDesc>();
		if (num7 >= 0)
		{
			for (int m = num7 + 4; m < num8; m++)
			{
				string[] array2 = TrimIndentionAndSplitSpace(struct4_.string_0[m]);
				if (array2.Length >= 8)
				{
					inputParams.Add(new ShaderParamDesc
					{
						name = array2[1],
						index = int.Parse(array2[2]),
						bind = "v" + array2[4],
						mask = array2[3],
						register = array2[4],
						valueType = array2[6] + ((array2[3].Length > 1) ? string.Concat(array2[3].Length) : "")
					});
				}
				else if (array2.Length >= 5)
				{
					inputParams.Add(new ShaderParamDesc
					{
						name = array2[1],
						index = int.Parse(array2[2]),
						bind = "v" + array2[4],
						mask = array2[3],
						register = array2[4]
					});
				}
			}
		}
		outputParams = new List<ShaderParamDesc>();
		if (num9 >= 0)
		{
			for (int n = num9 + 4; n < num10; n++)
			{
				string[] array3 = TrimIndentionAndSplitSpace(struct4_.string_0[n]);
				if (array3.Length >= 8)
				{
					outputParams.Add(new ShaderParamDesc
					{
						name = array3[1],
						index = int.Parse(array3[2]),
						bind = "o" + array3[4],
						mask = array3[3],
						register = array3[4],
						valueType = array3[6] + ((array3[3].Length > 1) ? string.Concat(array3[3].Length) : "")
					});
				}
				else if (array3.Length >= 5)
				{
					outputParams.Add(new ShaderParamDesc
					{
						name = array3[1],
						index = int.Parse(array3[2]),
						bind = "o" + array3[4],
						mask = array3[3],
						register = array3[4]
					});
				}
			}
		}
		if (bool_0)
		{
			Console.WriteLine("shaderParams_data: ");
			foreach (ShaderParamDesc item in shaderParams)
			{
				Console.WriteLine(item.ToString());
			}
			Console.WriteLine("cbuff: ");
			foreach (ShaderParamDesc item2 in cbufferParams)
			{
				Console.WriteLine(item2.ToString());
			}
			Console.WriteLine("bindings: ");
			foreach (ShaderParamDesc item3 in bindingParams)
			{
				Console.WriteLine(item3.ToString());
			}
			Console.WriteLine("inputs: ");
			foreach (ShaderParamDesc item4 in inputParams)
			{
				Console.WriteLine(item4.ToString());
			}
			Console.WriteLine("outputs: ");
			foreach (ShaderParamDesc item5 in outputParams)
			{
				Console.WriteLine(item5.ToString());
			}
		}
		method_14();
	}

	private void method_14()
	{
		string[] maskChars = new string[4]
		{
			"x", "y", "z", "w"
		};
		Dictionary<string, Class51[]> dictionary = new Dictionary<string, Class51[]>();
		foreach (ShaderParamDesc item in cbufferParams)
		{
			dictionary[item.cbufferName] = item.class51_0;
			Class51[] class51_ = item.class51_0;
			foreach (Class51 @class in class51_)
			{
				@class.int_2 = @class.offset / 16;
				@class.string_2 = maskChars[(@class.offset - @class.int_2 * 16) / 4 + 1];
			}
		}
		displayToShaderParamBinding = new Dictionary<string, ShaderParamBinding>();
		foreach (ShaderParamDesc item2 in bindingParams)
		{
			if (dictionary.TryGetValue(item2.name, out var value))
			{
				ShaderParamBinding class2 = new ShaderParamBinding
				{
					bind = item2.bind,
					name = item2.name,
					class51_0 = value
				};
				displayToShaderParamBinding[item2.bind] = class2;
				shaderParamBindings.Add(class2);
			}
			else
			{
				Console.WriteLine("Not find:  CBuffMap[" + item2.name + "]");
			}
		}
		foreach (ShaderParamDesc item3 in shaderParams)
		{
			ShaderParamBinding class3 = new ShaderParamBinding
			{
				bind = item3.bind,
				name = item3.name,
				desc = item3
			};
			displayToShaderParamBinding[item3.bind] = class3;
			shaderParamBindings.Add(class3);
		}
		foreach (ShaderParamDesc item4 in inputParams)
		{
			string name = item4.name;
			if (!(name == "TEXCOORD") && !(name == "POSITION"))
			{
				if (item4.index > 0)
				{
					name += item4.index;
				}
			}
			else
			{
				name += item4.index;
			}
			if (generateType == GenerateTypeEnum.GLS)
			{
				if (!name.StartsWith("SV_"))
				{
					name = ((shaderType != 0) ? ("vs_" + name) : ("in_" + name));
				}
			}
			else
			{
				name = "in." + name;
			}
			ShaderParamBinding class4 = new ShaderParamBinding
			{
				bind = item4.bind,
				name = name,
				desc = item4,
				mask = item4.mask
			};
			shaderParamBindings.Add(class4);
			displayToShaderParamBinding[item4.bind] = class4;
		}
		foreach (ShaderParamDesc item5 in outputParams)
		{
			string text2 = item5.name;
			if (!(text2 == "TEXCOORD") && !(text2 == "POSITION"))
			{
				if (item5.index > 0)
				{
					text2 += item5.index;
				}
			}
			else
			{
				text2 += item5.index;
			}
			if (generateType == GenerateTypeEnum.GLS)
			{
				if (!text2.StartsWith("SV_"))
				{
					text2 = ((shaderType != 0) ? ("SV_" + text2) : ("vs_" + text2));
				}
			}
			else
			{
				text2 = "out." + text2;
			}
			ShaderParamBinding class5 = new ShaderParamBinding
			{
				bind = item5.bind,
				name = text2,
				desc = item5,
				mask = item5.mask
			};
			shaderParamBindings.Add(class5);
			displayToShaderParamBinding[item5.bind] = class5;
		}
	}

	private void method_15(Class47[] class47_0, StringBuilder stringBuilder_0)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Class47[] array = class47_0;
		foreach (Class47 @class in array)
		{
			if (@class.op.StartsWith("dcl_"))
			{
				Class53 class2 = method_10(@class.op);
				Delegate1 key = class2?.delegate1_0;
				string[] array2 = class2?.string_0;
				Delegate2 @delegate = Dictionary_0[key];
				HashSet<string> op_args = ((array2 == null || array2.Length == 0) ? new HashSet<string>() : new HashSet<string>(array2));
				string text = @delegate(op_args, @class.args, @class)?.opStrSimplified;
				if (!string.IsNullOrEmpty(text))
				{
					stringBuilder.AppendLine(text ?? "");
				}
			}
		}
		if (generateType == GenerateTypeEnum.GLS)
		{
			if (cbufferParams.Count > 0)
			{
				foreach (ShaderParamDesc item in cbufferParams)
				{
					stringBuilder_0.AppendLine(("class " + item.cbufferName) ?? "");
					stringBuilder_0.AppendLine("{");
					Class51[] class51_ = item.class51_0;
					foreach (Class51 class3 in class51_)
					{
						stringBuilder_0.AppendLine(BasicStringFormat("  %s  %s;", class3.desc, class3.name));
					}
					stringBuilder_0.AppendLine("}");
				}
			}
			if (shaderParams.Count > 0)
			{
				stringBuilder_0.AppendLine("//");
				stringBuilder_0.AppendLine("// Uniform:");
				stringBuilder_0.AppendLine("//");
				foreach (ShaderParamDesc item2 in shaderParams)
				{
					if (item2.isUniform && !string.IsNullOrEmpty(item2.valueType) && !string.IsNullOrEmpty(item2.name))
					{
						string text2 = item2.valueType;
						if (text2?.ToLower() == "Texture1D".ToLower())
						{
							text2 = "sampler1D";
						}
						if (text2?.ToLower() == "Texture2D".ToLower())
						{
							text2 = "sampler2D";
						}
						if (text2?.ToLower() == "Texture3D".ToLower())
						{
							text2 = "sampler3D";
						}
						if (text2?.ToLower() == "texture2darray".ToLower())
						{
							text2 = "sampler2DArray";
						}
						stringBuilder_0.AppendLine("uniform " + text2 + " " + item2.name + ";");
					}
				}
				stringBuilder_0.AppendLine("");
			}
			if (inputParams.Count > 0)
			{
				stringBuilder_0.AppendLine("//");
				stringBuilder_0.AppendLine("// Inputs:");
				stringBuilder_0.AppendLine("//");
				foreach (ShaderParamDesc item3 in inputParams)
				{
					string text3 = item3.name;
					if (!(item3.name == "TEXCOORD") && !(text3 == "POSITION"))
					{
						if (item3.index > 0)
						{
							text3 = item3.name + item3.index;
						}
					}
					else
					{
						text3 = item3.name + item3.index;
					}
					if (!text3.StartsWith("SV_"))
					{
						text3 = ((shaderType != 0) ? ("vs_" + text3) : ("in_" + text3));
					}
					stringBuilder_0.AppendLine("in " + item3.valueType + " " + text3 + ";");
				}
				stringBuilder_0.AppendLine("");
			}
			if (outputParams.Count > 0)
			{
				stringBuilder_0.AppendLine("//");
				stringBuilder_0.AppendLine("// Outputs:");
				stringBuilder_0.AppendLine("//");
				foreach (ShaderParamDesc item4 in outputParams)
				{
					string text4 = item4.name;
					if (!(item4.name == "TEXCOORD") && !(text4 == "POSITION"))
					{
						if (item4.index > 0)
						{
							text4 = item4.name + item4.index;
						}
					}
					else
					{
						text4 = item4.name + item4.index;
					}
					if (!text4.StartsWith("SV_"))
					{
						text4 = ((shaderType != 0) ? ("SV_" + text4) : ("vs_" + text4));
					}
					stringBuilder_0.AppendLine("out " + item4.valueType + " " + text4 + ";");
				}
				stringBuilder_0.AppendLine("");
			}
		}
		else
		{
			if (cbufferParams.Count > 0)
			{
				foreach (ShaderParamDesc item5 in cbufferParams)
				{
					stringBuilder_0.AppendLine(("class " + item5.cbufferName) ?? "");
					stringBuilder_0.AppendLine("{");
					Class51[] class51_ = item5.class51_0;
					foreach (Class51 class4 in class51_)
					{
						stringBuilder_0.AppendLine(BasicStringFormat("  %s  %s;", class4.desc, class4.name));
					}
					stringBuilder_0.AppendLine("}");
				}
			}
			if (inputParams.Count > 0)
			{
				stringBuilder_0.AppendLine("");
				stringBuilder_0.AppendLine("class INPUT");
				stringBuilder_0.AppendLine("{");
				foreach (ShaderParamDesc item6 in inputParams)
				{
					string text5 = item6.name;
					if (!(item6.name == "TEXCOORD") && !(text5 == "POSITION"))
					{
						if (item6.index > 0)
						{
							text5 = item6.name + item6.index;
						}
					}
					else
					{
						text5 = item6.name + item6.index;
					}
					stringBuilder_0.AppendLine("  " + item6.valueType + " " + text5 + ";");
				}
				stringBuilder_0.AppendLine("}");
				stringBuilder_0.AppendLine("");
			}
			if (outputParams.Count > 0)
			{
				stringBuilder_0.AppendLine("");
				stringBuilder_0.AppendLine("class OUT");
				stringBuilder_0.AppendLine("{");
				foreach (ShaderParamDesc item7 in outputParams)
				{
					string text6 = item7.name;
					if (!(item7.name == "TEXCOORD") && !(text6 == "POSITION"))
					{
						if (item7.index > 0)
						{
							text6 = item7.name + item7.index;
						}
					}
					else
					{
						text6 = item7.name + item7.index;
					}
					stringBuilder_0.AppendLine("  " + item7.valueType + " " + text6 + ";");
				}
				stringBuilder_0.AppendLine("}");
				stringBuilder_0.AppendLine("");
			}
		}
		stringBuilder_0.Append(stringBuilder);
		stringBuilder_0.AppendLine("void main()");
		stringBuilder_0.AppendLine("{");
		array = class47_0;
		foreach (Class47 class5 in array)
		{
			if (class5.op.StartsWith("dcl_"))
			{
				continue;
			}
			if (bool_0)
			{
				stringBuilder_0.AppendLine(class5?.ToString());
			}
			if (string.IsNullOrEmpty(class5.op))
			{
				continue;
			}
			Class53 class6 = method_10(class5.op);
			Delegate1 delegate2 = class6?.delegate1_0;
			string[] array3 = class6?.string_0;
			if (delegate2 == null)
			{
				stringBuilder_0.AppendLine("// Not implement: " + class5.op);
				continue;
			}
			Delegate2 delegate3 = Dictionary_0[delegate2];
			HashSet<string> op_args2 = ((array3 == null || array3.Length == 0) ? new HashSet<string>() : new HashSet<string>(array3));
			Class52 class7 = delegate3(op_args2, class5.args, class5);
			string text7 = class7?.opStrSimplified;
			string text8 = class7?.blockTag;
			if (!string.IsNullOrEmpty(text7) || !string.IsNullOrEmpty(text8))
			{
				if (stack_0.Count > 0 && text8 != null && stack_0.Peek().endNames.Contains(text8))
				{
					stack_0.Pop();
				}
				char c = text7[text7.Length - 1];
				string text9 = ((c == '}' || c == '{') ? "" : ";");
				string text10 = null;
				if ((c == '}' || c == '{') && text7.Trim().Length > 1)
				{
					text7 = text7.Substring(0, text7.Length - 1);
					text10 = c.ToString() ?? "";
				}
				stringBuilder_0.AppendLine(new string(' ', 4 * stack_0.Count) + text7 + text9);
				if (text10 != null)
				{
					stringBuilder_0.AppendLine(new string(' ', 4 * stack_0.Count) + text10);
				}
				if (text8 != null && blockTagMap.TryGetValue(text8, out var value))
				{
					stack_0.Push(value);
				}
			}
		}
		stringBuilder_0.AppendLine("}");
	}

	[CompilerGenerated]
	internal static string[] smethod_3(string string_0)
	{
		List<string> list = new List<string>();
		string text = null;
		for (int i = 0; i < string_0.Length; i++)
		{
			char c = string_0[i];
			switch (c)
			{
				case '_':
					if (text != null)
					{
						list.Add(text);
					}
					text = "_";
					break;
				default:
					text += c;
					break;
				case '\t':
				case ' ':
				case ',':
					if (text != null)
					{
						list.Add(text);
					}
					text = null;
					break;
			}
		}
		if (text != null && text != "_")
		{
			list.Add(text);
		}
		text = null;
		return list.ToArray();
	}

	[CompilerGenerated]
	private Class52 method_16(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		string text3 = GetDisplayString(class48_3);
		if (hashSet_0.Contains("_sat"))
		{
			return Class52.smethod_0("%s = saturate(dot(%s, %s))", text, text2, text3);
		}
		return Class52.smethod_0("%s = dot(%s, %s)", text, text2, text3);
	}

	[CompilerGenerated]
	private Class52 method_17(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get5OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3, out var class48_4, out var class48_5);
		string text = "float" + (string.IsNullOrEmpty(class48_.suffix) ? "" : class48_.suffix.Length.ToString());
		string string_ = null;
		ShaderParamBinding @class = GetParamBindingAndFixSuffix(class48_.name, class48_.suffix, ref string_);
		object obj;
		if (@class != null)
		{
			if (@class == null)
			{
				obj = null;
			}
			else
			{
				ShaderParamDesc class50_ = @class.desc;
				if (class50_ == null)
				{
					obj = null;
				}
				else
				{
					obj = class50_.valueType;
					if (obj != null)
					{
						goto IL_0082;
					}
				}
			}
			obj = text;
			goto IL_0082;
		}
		goto IL_0084;
		IL_0082:
		text = (string)obj;
		goto IL_0084;
		IL_0084:
		string text2 = GetDisplayString(class48_);
		string text3 = GetDisplayString(class48_2);
		string text4 = GetDisplayString(class48_3, class48_);
		string text5 = GetDisplayString(class48_4, class48_);
		string text6 = GetDisplayString(class48_5, class48_);
		string text7 = "swapc_tmp" + int_0++;
		string text8 = text + " " + text7 + " = " + BasicStringFormat("%s ? %s : %s", text4, text6, text5) + "; ";
		string text9 = BasicStringFormat("%s = %s ? %s : %s", text3, text4, text5, text6) + "; ";
		string text10 = BasicStringFormat("%s = %s", text2, text7) + ";";
		if (text2 == null || text2 == "null")
		{
			text8 = null;
			text10 = null;
		}
		if (text3 == null || text3 == "null")
		{
			text9 = null;
		}
		return Class52.smethod_1((text8 + text9 + text10)?.TrimEnd(' '));
	}

	[CompilerGenerated]
	private Class52 method_18(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get4OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3, out var class48_4);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		string text3 = GetDisplayString(class48_3, class48_);
		string text4 = GetDisplayString(class48_4, class48_);
		return Class52.smethod_0("%s = %s ? %s : %s", text, text2, text3, text4);
	}

	[CompilerGenerated]
	private Class52 method_19(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var _);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		if (hashSet_0.Contains("_sat"))
		{
			return Class52.smethod_0("%s = saturate(%s)", text, text2);
		}
		return Class52.smethod_0("%s = %s", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_20(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		string text3 = GetDisplayString(class48_3, class48_);
		string text4 = null;
		text4 = ((text3[0] != '-') ? BasicStringFormat("%s + %s", text2, text3) : BasicStringFormat("%s - %s", text2, text3.Substring(1)));
		if (hashSet_0.Contains("_sat"))
		{
			return Class52.smethod_0("%s = saturate(%s)", text, text4);
		}
		return Class52.smethod_0("%s = %s", text, text4);
	}

	[CompilerGenerated]
	private Class52 method_21(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		string text3 = GetDisplayString(class48_3, class48_);
		string text4 = null;
		text4 = BasicStringFormat("%s * %s", text2, text3);
		if (hashSet_0.Contains("_sat"))
		{
			return Class52.smethod_0("%s = saturate(%s)", text, text4);
		}
		return Class52.smethod_0("%s = %s", text, text4);
	}

	[CompilerGenerated]
	private Class52 method_22(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get4OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3, out var class48_4);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		string text3 = GetDisplayString(class48_3, class48_);
		string text4 = GetDisplayString(class48_4, class48_2);
		string text5 = BasicStringFormat("(%s * %s) >> 32", text3, text4);
		string text6 = BasicStringFormat("(%s * %s) & 0xffffffff", text3, text4);
		text5 = text + " = " + text5 + ";";
		text6 = text2 + " = " + text6 + ";";
		if (text == null || text == "null")
		{
			text5 = null;
		}
		if (text2 == null || text2 == "null")
		{
			text6 = null;
		}
		return Class52.smethod_1(text5 + text6);
	}

	[CompilerGenerated]
	private Class52 method_23(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get4OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3, out var class48_4);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		string text3 = GetDisplayString(class48_3, class48_);
		string text4 = GetDisplayString(class48_4, class48_2);
		string text5 = BasicStringFormat("(%s * %s) >> 32", text3, text4);
		string text6 = BasicStringFormat("(%s * %s) & 0xffffffff", text3, text4);
		text5 = text + " = " + text5 + ";";
		text6 = text2 + " = " + text6 + ";";
		if (text == null || text == "null")
		{
			text5 = null;
		}
		if (text2 == null || text2 == "null")
		{
			text6 = null;
		}
		return Class52.smethod_1(text5 + text6);
	}

	[CompilerGenerated]
	private Class52 method_24(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get4OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3, out var class48_4);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		string text3 = GetDisplayString(class48_3, class48_);
		string text4 = GetDisplayString(class48_4, class48_);
		string text5 = null;
		text5 = ((text4[0] != '-') ? BasicStringFormat("%s + %s", text3, text4) : BasicStringFormat("%s - %s", text3, text4.Substring(1)));
		if (hashSet_0.Contains("_sat"))
		{
			return Class52.smethod_0("%s = saturate(%s * %s)", text, text2, text5);
		}
		return Class52.smethod_0("%s = %s * %s", text, text2, text5);
	}

	[CompilerGenerated]
	private Class52 method_25(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		string text3 = GetDisplayString(class48_3, class48_);
		string text4 = null;
		text4 = BasicStringFormat("%s / %s", text2, text3);
		if (hashSet_0.Contains("_sat"))
		{
			return Class52.smethod_0("%s = saturate(%s)", text, text4);
		}
		return Class52.smethod_0("%s = %s", text, text4);
	}

	[CompilerGenerated]
	private Class52 method_26(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get4OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3, out var class48_4);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		string text3 = GetDisplayString(class48_3, class48_);
		string text4 = GetDisplayString(class48_4, class48_2);
		string text5 = BasicStringFormat("%s / %s", text3, text4);
		string text6 = BasicStringFormat("%s *(1 / %s)", text3, text4);
		text5 = text + " = " + text5 + ";";
		text6 = text2 + " = " + text6 + ";";
		if (text == null || text == "null")
		{
			text5 = null;
		}
		if (text2 == null || text2 == "null")
		{
			text6 = null;
		}
		return Class52.smethod_1(text5 + text6);
	}

	[CompilerGenerated]
	private Class52 method_27(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		string text3 = GetDisplayString(class48_3, class48_);
		return Class52.smethod_0("%s = max(%s, %s)", text, text2, text3);
	}

	[CompilerGenerated]
	private Class52 method_28(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		string text3 = GetDisplayString(class48_3, class48_);
		return Class52.smethod_0("%s = min(%s, %s)", text, text2, text3);
	}

	[CompilerGenerated]
	private Class52 method_29(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		string text3 = GetDisplayString(class48_3, class48_);
		string text4 = GetDisplayString(class48_3, class48_2);
		string text5 = BasicStringFormat("sin(%s)", text3);
		string text6 = BasicStringFormat("cos(%s)", text4);
		if (hashSet_0.Contains("_sat"))
		{
			text5 = "saturate(" + text5 + ")";
			text6 = "saturate(" + text6 + ")";
		}
		text5 = text + " = " + text5 + ";";
		text6 = text2 + " = " + text6 + ";";
		if (text == null || text == "null")
		{
			text5 = null;
		}
		if (text2 == null || text2 == "null")
		{
			text6 = null;
		}
		return Class52.smethod_1(text5 + text6);
	}

	[CompilerGenerated]
	private Class52 method_30(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var _);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		return Class52.smethod_0("%s = log(%s)", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_31(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get5OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3, out var class48_4, out var class48_5);
		string text = GetDisplayString(class48_);
		GetDisplayString(out var string_, out var string_2, class48_2, null, getBodyAndMaskSeparately: true);
		GetDisplayString(out var string_3, out var string_4, class48_3, class48_, getBodyAndMaskSeparately: true);
		GetDisplayString(class48_4);
		string text2 = GetDisplayString(class48_5);
		string text3 = "tex2D";
		string string_5 = "";
		ShaderParamBinding @class = GetParamBindingAndFixSuffix(class48_3?.name, class48_3?.suffix, ref string_5);
		if (@class?.desc?.valueType?.ToLower() == "texture3d".ToLower())
		{
			text3 = "tex3D";
		}
		else if (@class?.desc?.valueType?.ToLower() == "texture1d".ToLower())
		{
			text3 = "tex1D";
		}
		else if (@class?.desc?.valueType?.ToLower() == "texture2DArray".ToLower())
		{
			text3 = "texture2DArray";
			if (text2 != null && text2 != "null")
			{
				return Class52.smethod_1(text + " = SAMPLE_TEXTURE2D_ARRAY(" + string_3 + ", sampler_" + string_3 + ", " + string_ + ((string_2 == null) ? null : ("." + string_2.Substring(0, Math.Min(string_2.Length, 2)))) + ", " + text2 + ") ");
			}
			return Class52.smethod_1(text + " = SAMPLE_TEXTURE2D_ARRAY(" + string_3 + ", sampler_" + string_3 + ", " + string_ + ((string_2 == null) ? null : ("." + string_2.Substring(0, Math.Min(string_2.Length, 2)))) + ") ");
		}
		return Class52.smethod_0("%s = " + text3 + "(%s, %s%s).%s  /* " + class47_0.src + " */ ", text, string_3, string_, (string_2 == null) ? null : ("." + string_2.Substring(0, Math.Min(string_2.Length, 2))), string_4);
	}

	[CompilerGenerated]
	private Class52 method_32(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get4OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3, out var class48_4);
		string text = GetDisplayString(class48_);
		GetDisplayString(out var string_, out var string_2, class48_2, null, getBodyAndMaskSeparately: true);
		GetDisplayString(out var string_3, out var string_4, class48_3, class48_, getBodyAndMaskSeparately: true);
		string text2 = GetDisplayString(class48_4);
		return Class52.smethod_0("%s = textureGather(%s, %s%s).%s /*sample_state %s*/", text, string_3, string_, (string_2 == null) ? null : ("." + string_2.Substring(0, Math.Min(string_2.Length, 2))), string_4, text2);
	}

	[CompilerGenerated]
	private Class52 method_33(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		string string_ = "y";
		string text3 = null;
		if (hashSet_0.Contains("_coarse"))
		{
			text3 = "_coarse";
		}
		if (hashSet_0.Contains("_fine"))
		{
			text3 = "_fine";
		}
		string text4 = BasicStringFormat("dd%s%s(%s)", string_, text3, text2);
		if (hashSet_0.Contains("_sat"))
		{
			text4 = "saturate(" + text4 + ")";
		}
		return Class52.smethod_0("%s = %s", text, text4);
	}

	[CompilerGenerated]
	private Class52 method_34(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		string string_ = "x";
		string text3 = null;
		if (hashSet_0.Contains("_coarse"))
		{
			text3 = "_coarse";
		}
		if (hashSet_0.Contains("_fine"))
		{
			text3 = "_fine";
		}
		string text4 = BasicStringFormat("dd%s%s(%s)", string_, text3, text2);
		if (hashSet_0.Contains("_sat"))
		{
			text4 = "saturate(" + text4 + ")";
		}
		return Class52.smethod_0("%s = %s", text, text4);
	}

	[CompilerGenerated]
	private Class52 method_35(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		string text3 = GetDisplayString(class48_3, class48_);
		return Class52.smethod_0("%s = %s == %s", text, text2, text3);
	}

	[CompilerGenerated]
	private Class52 method_36(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		string text3 = GetDisplayString(class48_3, class48_);
		return Class52.smethod_0("%s = %s != %s", text, text2, text3);
	}

	[CompilerGenerated]
	private Class52 method_37(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		return Class52.smethod_0("%s = !%s", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_38(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		string text3 = GetDisplayString(class48_3, class48_);
		return Class52.smethod_0("%s = %s < %s", text, text2, text3);
	}

	[CompilerGenerated]
	private Class52 method_39(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		string text3 = GetDisplayString(class48_3, class48_);
		return Class52.smethod_0("%s = %s >= %s", text, text2, text3);
	}

	[CompilerGenerated]
	private Class52 method_40(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		string text3 = GetDisplayString(class48_3);
		return Class52.smethod_0("%s = %s << %s", text, text2, text3);
	}

	[CompilerGenerated]
	private Class52 method_41(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		string text3 = GetDisplayString(class48_3);
		return Class52.smethod_0("%s = %s >> %s", text, text2, text3);
	}

	[CompilerGenerated]
	private Class52 method_42(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var _);
		string text = GetDisplayString(class48_);
		if (hashSet_0.Contains("_z"))
		{
			return Class52.smethod_0("if (%s == 0) discard", text);
		}
		if (hashSet_0.Contains("_nz"))
		{
			return Class52.smethod_0("if (%s != 0) discard", text);
		}
		return Class52.smethod_0("discard");
	}

	[CompilerGenerated]
	private Class52 method_43(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var _);
		string text = GetDisplayString(class48_);
		if (hashSet_0.Contains("_z"))
		{
			return Class52.smethod_1(BasicStringFormat("if (%s == 0) {", text), "if");
		}
		if (hashSet_0.Contains("_nz"))
		{
			return Class52.smethod_1(BasicStringFormat("if (%s != 0) {", text), "if");
		}
		return Class52.smethod_0("discard");
	}

	[CompilerGenerated]
	private Class52 method_44(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var _);
		string text = GetDisplayString(class48_);
		if (text == null)
		{
			return Class52.smethod_1("break", "break");
		}
		if (hashSet_0.Contains("_z"))
		{
			return Class52.smethod_0("if (%s == 0) break", text);
		}
		if (hashSet_0.Contains("_nz"))
		{
			return Class52.smethod_0("if (%s != 0) break", text);
		}
		return Class52.smethod_0("// ERROR for break " + text);
	}

	[CompilerGenerated]
	private Class52 method_45(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var _);
		string text = GetDisplayString(class48_);
		if (text == null)
		{
			return Class52.smethod_1("continue");
		}
		if (hashSet_0.Contains("_z"))
		{
			return Class52.smethod_0("if (%s == 0) continue", text);
		}
		if (hashSet_0.Contains("_nz"))
		{
			return Class52.smethod_0("if (%s != 0) continue", text);
		}
		return Class52.smethod_0("// ERROR for continue " + text);
	}

	[CompilerGenerated]
	private Class52 method_46(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		return Class52.smethod_0("%s = rsqrt(%s)", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_47(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		return Class52.smethod_0("%s = sqrt(%s)", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_48(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		return Class52.smethod_0("%s = frac(%s)", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_49(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		return Class52.smethod_0("%s = rcp(%s)", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_50(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		return Class52.smethod_0("%s = exp(%s)", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_51(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		return Class52.smethod_0("%s = floor(%s)", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_52(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		return Class52.smethod_0("%s = ceil(%s)", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_53(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		return Class52.smethod_0("%s = floor(%s) /* round_ne, nearest even */", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_54(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		return Class52.smethod_0("%s = trunc(%s) /* round_z, round towards zero */", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_55(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		return Class52.smethod_0("%s = floor(%s) /* ftoi */", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_56(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		return Class52.smethod_0("%s = floor(%s) /* ftou */", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_57(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		return Class52.smethod_0("%s = %s /* round-to-nearest-even rounding */", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_58(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		string text3 = GetDisplayString(class48_3, class48_);
		if (text3.Contains("0x3f800000"))
		{
			return Class52.smethod_0("%s = %s /* and 0x3f800000 */", text, text2);
		}
		if (class48_.suffix != null)
		{
			text2 = "uint" + ((class48_.suffix.Length > 1) ? string.Concat(class48_.suffix.Length) : "") + "(" + text2 + ")";
			text3 = "uint" + ((class48_.suffix.Length > 1) ? string.Concat(class48_.suffix.Length) : "") + "(" + text3 + ")";
		}
		return Class52.smethod_0("%s = %s & %s", text, text2, text3);
	}

	[CompilerGenerated]
	private Class52 method_59(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		string text3 = GetDisplayString(class48_3, class48_);
		if (class48_.suffix != null)
		{
			text2 = "uint" + ((class48_.suffix.Length > 1) ? string.Concat(class48_.suffix.Length) : "") + "(" + text2 + ")";
			text3 = "uint" + ((class48_.suffix.Length > 1) ? string.Concat(class48_.suffix.Length) : "") + "(" + text3 + ")";
		}
		return Class52.smethod_0("%s = %s | %s", text, text2, text3);
	}

	[CompilerGenerated]
	private Class52 method_60(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2, class48_);
		string text3 = GetDisplayString(class48_3, class48_);
		if (class48_.suffix != null)
		{
			text2 = "uint" + ((class48_.suffix.Length > 1) ? string.Concat(class48_.suffix.Length) : "") + "(" + text2 + ")";
			text3 = "uint" + ((class48_.suffix.Length > 1) ? string.Concat(class48_.suffix.Length) : "") + "(" + text3 + ")";
		}
		return Class52.smethod_0("%s = %s ^ %s", text, text2, text3);
	}

	[CompilerGenerated]
	private Class52 method_61(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var _);
		string text = GetDisplayString(class48_);
		if (text == null)
		{
			return Class52.smethod_1("return");
		}
		if (hashSet_0.Contains("_z"))
		{
			return Class52.smethod_0("if (%s == 0) return", text);
		}
		if (hashSet_0.Contains("_nz"))
		{
			return Class52.smethod_0("if (%s != 0) return", text);
		}
		return Class52.smethod_0("// ERROR for return " + text);
	}

	[CompilerGenerated]
	private Class52 method_62(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var _);
		if (hashSet_0.Contains("_temps"))
		{
			string s = GetDisplayString(class48_);
			List<string> list = new List<string>();
			for (int i = 0; i < int.Parse(s); i++)
			{
				string text = "u_xlat" + i;
				list.Add("float4 " + text + ";");
				displayToShaderParamBinding["r" + i] = new ShaderParamBinding
				{
					name = text,
					bind = "r" + i
				};
			}
			if (list.Count > 0)
			{
				return Class52.smethod_0(string.Join("\n", list.ToArray()));
			}
			return Class52.smethod_0("");
		}
		if (hashSet_0.Contains("_resource") && (hashSet_0.Contains("_texture2darray") || hashSet_0.Contains("_texture3d") || hashSet_0.Contains("_texture2d") || hashSet_0.Contains("_texture1d")))
		{
			if (displayToShaderParamBinding.TryGetValue(class48_.name?.ToLower(), out var value))
			{
				if (hashSet_0.Contains("_texture2darray"))
				{
					value.desc.valueType = "texture2darray";
				}
				if (hashSet_0.Contains("_texture3d"))
				{
					value.desc.valueType = "texture3d";
				}
				if (hashSet_0.Contains("_texture2d"))
				{
					value.desc.valueType = "texture2d";
				}
				if (hashSet_0.Contains("_texture1d"))
				{
					value.desc.valueType = "texture1d";
				}
				return Class52.smethod_0(null);
			}
			return Class52.smethod_0("");
		}
		if (hashSet_0.Contains("_constantbuffer"))
		{
			if (displayToShaderParamBinding.TryGetValue(class48_.name.ToLower(), out var value2))
			{
				if (value2.desc.valueType.EndsWith("4"))
				{
					value2.int_0 = int.Parse(class48_.idx) - 4 * ((value2.desc.arraySize == 0) ? 1 : value2.desc.arraySize);
				}
				if (value2.desc.valueType.EndsWith("3"))
				{
					value2.int_0 = int.Parse(class48_.idx) - 3;
				}
				if (value2.desc.valueType.EndsWith("2"))
				{
					value2.int_0 = int.Parse(class48_.idx) - 2;
				}
				if (value2.desc.valueType.EndsWith("1"))
				{
					value2.int_0 = int.Parse(class48_.idx) - 1;
				}
				return Class52.smethod_0(null);
			}
			return Class52.smethod_0("// " + class48_.name + "  " + class48_.idx + " not find binding");
		}
		if (hashSet_0.Contains("_immediateConstantBuffer"))
		{
			string string_ = "iConstantBuffer";
			ShaderParamDesc class50_ = new ShaderParamDesc
			{
				valueType = "float4",
				name = "iConstantBuffer",
				isMatrix = true,
				mask = "xyzw"
			};
			displayToShaderParamBinding["icb"] = new ShaderParamBinding
			{
				name = string_,
				bind = "icb",
				desc = class50_
			};
			return Class52.smethod_0("float4[] " + string_ + " = " + class48_.name + ";");
		}
		return Class52.smethod_0(null);
	}

	[CompilerGenerated]
	private Class52 method_63(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get4OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3, out var class48_4);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		string text3 = GetDisplayString(class48_3);
		string text4 = GetDisplayString(class48_4);
		return Class52.smethod_0("bitmask = (((1 << %s) - 1) << %s) & 0xffffffff;  dest = ((%s << %s) & bitmask) | (%s & ~bitmask)", text, text2, text3, text2, text4);
	}

	[CompilerGenerated]
	private Class52 method_64(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		return Class52.smethod_0("%s = reverse_bit(%s)", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_65(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		return Class52.smethod_0("%s = countbits(%s)", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_66(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		return Class52.smethod_0("%s = saturate(%s)", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_67(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		return Class52.smethod_0("%s = -%s", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_68(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		return Class52.smethod_0("%s = abs(%s)", text, text2);
	}

	[CompilerGenerated]
	private Class52 method_69(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get4OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3, out var class48_4);
		string text = GetDisplayString(class48_);
		GetDisplayString(out var string_, out var string_2, class48_2, null, getBodyAndMaskSeparately: true);
		GetDisplayString(out var string_3, out var _, class48_3, null, getBodyAndMaskSeparately: true);
		GetDisplayString(out var string_5, out var string_6, class48_4, class48_, getBodyAndMaskSeparately: true);
		return Class52.smethod_0("%s = %s[%s%s][%s].%s  /* " + class47_0.src + " */", text, string_5, string_, (string_2 == null) ? null : ("." + string_2.Substring(0, Math.Min(string_2.Length, 2))), string_3, string_6);
	}

	[CompilerGenerated]
	private Class52 method_70(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get4OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3, out var class48_4);
		string text = GetDisplayString(class48_);
		GetDisplayString(out var string_, out var string_2, class48_2, null, getBodyAndMaskSeparately: true);
		GetDisplayString(out var string_3, out var _, class48_3, null, getBodyAndMaskSeparately: true);
		GetDisplayString(out var string_5, out var string_6, class48_4, class48_, getBodyAndMaskSeparately: true);
		return Class52.smethod_0("%s = %s[%s.%s][%s].%s /* ld */", text, string_5, string_, string_2?.Substring(0, Math.Min(string_2.Length, 2)), string_3, string_6);
	}

	[CompilerGenerated]
	private Class52 method_71(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get4OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3, out var class48_4);
		string text = GetDisplayString(class48_);
		GetDisplayString(out var string_, out var string_2, class48_2, null, getBodyAndMaskSeparately: true);
		GetDisplayString(out var string_3, out var _, class48_3, null, getBodyAndMaskSeparately: true);
		GetDisplayString(out var string_5, out var string_6, class48_4, class48_, getBodyAndMaskSeparately: true);
		return Class52.smethod_0("%s = %s[%s.%s][%s].%s /* ld */", text, string_5, string_, string_2?.Substring(0, Math.Min(string_2.Length, 2)), string_3, string_6);
	}

	[CompilerGenerated]
	private Class52 method_72(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get4OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3, out var class48_4);
		GetDisplayString(out var string_, out var string_2, class48_, null, getBodyAndMaskSeparately: true);
		string text = GetDisplayString(class48_2);
		string text2 = GetDisplayString(class48_3);
		string text3 = GetDisplayString(class48_4, class48_);
		return Class52.smethod_0("%s[%s][%s].%s = %s /* store structured*/", string_, text, text2, string_2, text3);
	}

	[CompilerGenerated]
	private Class52 method_73(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get3OperandsSafe(class48_0, out var class48_, out var class48_2, out var class48_3);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		string text3 = GetDisplayString(class48_3);
		return Class52.smethod_0("%s[%s] = %s", text, text2, text3);
	}

	[CompilerGenerated]
	private Class52 method_74(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var _);
		string text = GetDisplayString(class48_);
		return Class52.smethod_0("goto %s", text);
	}

	[CompilerGenerated]
	private Class52 method_75(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var class48_2);
		string text = GetDisplayString(class48_);
		string text2 = GetDisplayString(class48_2);
		if (text == null)
		{
			return Class52.smethod_1("goto %s", text2);
		}
		if (hashSet_0.Contains("_z"))
		{
			return Class52.smethod_0("if (%s == 0) goto %s", text, text2);
		}
		if (hashSet_0.Contains("_nz"))
		{
			return Class52.smethod_0("if (%s != 0) goto %s", text, text2);
		}
		return Class52.smethod_0("// ERROR for goto  " + text + " " + text2);
	}

	[CompilerGenerated]
	private Class52 method_76(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var _);
		string text = GetDisplayString(class48_);
		return Class52.smethod_0("%s: ", text);
	}

	[CompilerGenerated]
	private Class52 method_77(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var _);
		string text = GetDisplayString(class48_);
		return Class52.smethod_1(BasicStringFormat("case %s:", text), "case");
	}

	[CompilerGenerated]
	private Class52 method_78(HashSet<string> hashSet_0, InstructionOperand[] class48_0, Class47 class47_0)
	{
		Get2OperandsSafe(class48_0, out var class48_, out var _);
		string text = GetDisplayString(class48_);
		return Class52.smethod_1(BasicStringFormat("switch(%s) {", text), "switch");
	}

	[CompilerGenerated]
	internal static int smethod_4(string string_0, int int_1, ref Struct4 struct4_0)
	{
		if (int_1 < 0)
		{
			int_1 = 0;
		}
		int num = int_1;
		while (true)
		{
			if (num < struct4_0.string_0.Length)
			{
				if (struct4_0.string_0[num] != null && struct4_0.string_0[num].Contains(string_0))
				{
					break;
				}
				num++;
				continue;
			}
			return -1;
		}
		return num;
	}

	[CompilerGenerated]
	internal static string[] TrimIndentionAndSplitSpace(string string_0)
	{
		if (string.IsNullOrEmpty(string_0))
		{
			return new string[0];
		}
		string_0 = string_0
			.Replace("	", " ")
			.Replace("      ", " ")
			.Replace("     ", " ")
			.Replace("    ", " ")
			.Replace("   ", " ")
			.Replace("  ", " ")
			.Replace("  ", " ")
			.Replace("  ", " ");
		return string_0.Split(' ');
	}

	[CompilerGenerated]
	internal static string smethod_6(string string_0, string string_1)
	{
		string text = string_1 + "=";
		string[] array = string_0.Split(' ');
		int num = 0;
		string text2;
		while (true)
		{
			if (num < array.Length)
			{
				text2 = array[num];
				if (!string.IsNullOrEmpty(text2) && text2.StartsWith(text))
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return text2.Substring(text.Length);
	}
}