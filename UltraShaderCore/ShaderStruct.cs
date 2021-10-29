using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraShaderCore
{
    public class ShaderStruct
    {
        private AssetsFile assetsFile;

        public List<ShaderTableEntry> entries;
        public List<ShaderData> datas;
        public List<long> poses;
        //anything past here doesn't really matter
        //because it's all in the asset data anyway
        //TODO: not true anymore, these fields are empty in U2021+
        public ShaderStruct(AssetsFile assetsFile, AssetsFileReader reader)
        {
            this.assetsFile = assetsFile;
            int[] version = VersionUtils.GetVersionArray(assetsFile);

            reader.bigEndian = false;
            int entriesCount = reader.ReadInt32();
            entries = new List<ShaderTableEntry>(entriesCount);
            bool readUnknownTableField = version[0] > 2019 || (version[0] == 2019 && version[1] >= 3);
            for (int i = 0; i < entriesCount; i++)
            {
                entries.Add(new ShaderTableEntry(reader, readUnknownTableField));
            }
            poses = new List<long>(entriesCount);
            datas = new List<ShaderData>(entriesCount);
            for (int i = 0; i < entriesCount; i++)
            {
                poses.Add(entries[i].start);
                datas.Add(new ShaderData(assetsFile, reader, entries[i]));
            }
        }
    }

    public class ShaderTableEntry
    {
        public int start;
        public int length;
        public int unknown;

        public ShaderTableEntry(int start, int length)
        {
            this.start = start;
            this.length = length;
        }
        public ShaderTableEntry(int start, int length, int unknown)
        {
            this.start = start;
            this.length = length;
            this.unknown = unknown;
        }
        public ShaderTableEntry(AssetsFileReader reader, bool readUnknown)
        {
            start = reader.ReadInt32();
            length = reader.ReadInt32();
            if (readUnknown)
                unknown = reader.ReadInt32();
        }
    }

    public class ShaderData
    {
        public int date;
        public ShaderType type;
        public int statsALU;
        public int statsTEX;
        public int statsFlow;
        public int statsTempRegister;
        public List<string> keywords;
        public List<string> localKeywords;
        public int shaderSize;
        public byte[] unknownZeros;
        public byte[] shaderBytes;
        public ShaderMeta shaderMeta;
        public ShaderData(AssetsFile assetsFile, AssetsFileReader reader, ShaderTableEntry entry)
        {
            int[] version = VersionUtils.GetVersionArray(assetsFile);

            reader.Position = entry.start;
            date = reader.ReadInt32();
            type = (ShaderType)reader.ReadInt32();
            statsALU = reader.ReadInt32();
            statsTEX = reader.ReadInt32();
            statsFlow = reader.ReadInt32();
            statsTempRegister = reader.ReadInt32(); //ver >= 201608170 (as)
            int keywordCount = reader.ReadInt32();
            keywords = new List<string>(keywordCount);
            for (int i = 0; i < keywordCount; i++)
            {
                keywords.Add(reader.ReadCountStringInt32());
                reader.Align();
            }

            if (date >= 201806140)
            {
                int localKeywordCount = reader.ReadInt32();
                localKeywords = new List<string>(localKeywordCount);
                for (int i = 0; i < localKeywordCount; i++)
                {
                    keywords.Add(reader.ReadCountStringInt32());
                    reader.Align();
                }
            }

            shaderSize = reader.ReadInt32();

            long shaderStartPos = reader.Position;

            byte unkcnt1 = reader.ReadByte();
            byte unkcnt2 = reader.ReadByte();
            byte cbCount = reader.ReadByte();
            byte unkcnt4 = reader.ReadByte();
            byte unkcnt5 = reader.ReadByte();
            byte unkcnt6 = reader.ReadByte();

            //TODO FIGURE OUT VERSION
            if (version[0] >= 2021)
                unknownZeros = reader.ReadBytes(0x20);

            long dxbcShaderStart = reader.Position;
            shaderBytes = reader.ReadBytes(shaderSize - (int)(dxbcShaderStart - shaderStartPos));
            reader.Align();
            shaderMeta = new ShaderMeta(reader);
        }
    }

    public class ShaderMeta
    {
        // Not sure about any of this
        public int sourceMap; 
        public List<ShaderChannelBinding> channelBindings;
        public List<ShaderConstantBuffer> constantBuffers;
        public List<ShaderSlot> slots;
        public ShaderMeta(AssetsFileReader reader)
        {
            sourceMap = reader.ReadInt32();
            int channelBindingCount = reader.ReadInt32();
            channelBindings = new List<ShaderChannelBinding>(channelBindingCount);
            for (int i = 0; i < channelBindingCount; i++)
            {
                channelBindings.Add(new ShaderChannelBinding(reader));
            }

            int constantBufferCount = reader.ReadInt32();
            constantBuffers = new List<ShaderConstantBuffer>(constantBufferCount);
            for (int i = 0; i < constantBufferCount; i++)
            {
                constantBuffers.Add(new ShaderConstantBuffer(reader));
            }

            int slotCount = reader.ReadInt32();
            slots = new List<ShaderSlot>(slotCount);
            for (int i = 0; i < slotCount; i++)
            {
                slots.Add(new ShaderSlot(reader));
            }
        }
    }

    // ParserBindChannels m_Channels?
    public class ShaderChannelBinding
    {
        public int source;
        public int target;
        public ShaderChannelBinding(AssetsFileReader reader)
        {
            source = reader.ReadInt32();
            target = reader.ReadInt32();
        }
    }

    public class ShaderConstantBuffer
    {
        public string name;
        public int byteSize;
        public List<ShaderConstantBufferParam> cbParams;
        public ShaderConstantBuffer(AssetsFileReader reader)
        {
            name = reader.ReadCountStringInt32();
            reader.Align();

            byteSize = reader.ReadInt32();

            int paramCount = reader.ReadInt32();
            cbParams = new List<ShaderConstantBufferParam>(paramCount);
            for (int i = 0; i < paramCount; i++)
            {
                cbParams.Add(new ShaderConstantBufferParam(reader));
            }

            //todo, not supported yet!!!!! (I haven't seen this be anything other than 0)
            int structParamCount = reader.ReadInt32();
        }
    }

    public class ShaderConstantBufferParam
    {
        public string name;
        public int paramType; //always 0
        public int rowCount; //4 for matrix4x4, 1 for everything else
        public int columnCount; //entry size, ex. 2 for float2, 4 for float4 and matrix4x4
        public int isMatrix; //1 for matrix4x4/3x3, 0 for everything else
        public int vectorSize;
        public int pos;
        public ShaderConstantBufferParam(AssetsFileReader reader)
        {
            name = reader.ReadCountStringInt32();
            reader.Align();
            paramType = reader.ReadInt32();
            rowCount = reader.ReadInt32();
            columnCount = reader.ReadInt32();
            isMatrix = reader.ReadInt32();
            vectorSize = reader.ReadInt32();
            pos = reader.ReadInt32();
        }
    }

    public class ShaderSlot
    {
        public string name;
        public int type;
        public int slotIdx;
        public int[] args;
        public ShaderSlot(AssetsFileReader reader)
        {
            name = reader.ReadCountStringInt32();
            reader.Align();
            type = reader.ReadInt32();
            slotIdx = reader.ReadInt32();
            if (type == 0) //texture
            {
                args = new int[2];
                args[0] = reader.ReadInt32(); //sampler index
                args[1] = reader.ReadInt32(); //texture data: 0bAAB (A=dimensions, B=isMultiSampled)
            }
            else if (type == 1) //constantBuffer
            {
                args = new int[1];
                args[0] = reader.ReadInt32(); //always 0?
            }
            else if (type == 2) //buffer
            {
                args = new int[1];
                args[0] = reader.ReadInt32(); //always 0?
            }
            else if (type == 3) //uav
            {
                args = new int[1];
                args[0] = reader.ReadInt32(); //origIdx
            }
            else if (type == 4) //sampler
            {
                args = new int[1];
                args[0] = reader.ReadInt32(); //bindPoint
            }
            else
            {
                throw new Exception("idkkkk");
            }
        }
    }
}
