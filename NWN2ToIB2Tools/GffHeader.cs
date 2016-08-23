using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWN2ToIB2Tools
{
    public class GffHeader
    {
        public uint FieldArrayCount;
        public uint FieldArrayOffset;
        public uint FieldDataArrayCount;
        public uint FieldDataArrayOffset;
        public uint FieldIndicesArrayCount;
        public uint FieldIndicesArrayOffset;
        public string FileType;
        public string FileVersion;
        public uint LabelArrayCount;
        public uint LabelArrayOffset;
        public uint ListIndicesArrayCount;
        public uint ListIndicesArrayOffset;
        public uint StructArrayCount;
        public uint StructArrayOffset;

        public GffHeader()
        {

        }
    }
}
