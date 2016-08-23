using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWN2ToIB2Tools
{
    public enum GffFieldType
    {
        GFF_BYTE = 0,
        GFF_CHAR = 1,
        GFF_WORD = 2,
        GFF_SHORT = 3,
        GFF_DWORD = 4,
        GFF_INT = 5,
        GFF_DWORD64 = 6,
        GFF_INT64 = 7,
        GFF_FLOAT = 8,
        GFF_DOUBLE = 9,
        GFF_CEXOSTRING = 10,
        GFF_RESREF = 11,
        GFF_CEXOLOCSTRING = 12,
        GFF_VOID = 13,
        GFF_STRUCT = 14,
        GFF_LIST = 15
    }

    public class GffField
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public GffFieldType Type = GffFieldType.GFF_BYTE;
        public int LabelIndex = 0;
        public string Label = "";
        public uint DataOrDataOffset = 0; //if simple type then is value, else is the uint byte offset into the Field Data block
        //public object Data = 0;
        public object Data = 0;        
        public uint ValueDword = 0;
        public uint ValueWord = 0;
        public int ValueByte = 0;
        public int ValueInt = 0;
        public int ValueShort = 0;
        public float ValueFloat = 0;
        public string ValueChar = "";
        public string ValueCExoLocString = "";
        public string ValueCExoString = "";
        public string ValueCResRef = "";
        public GffStruct Owner;
        
        public double ValueDouble = 0;
        public ulong ValueDword64 = 0;
        public long ValueInt64 = 0;
        public byte[] ValueVoidData = null;
        

        public GffField()
        {

        }
    }    
}
