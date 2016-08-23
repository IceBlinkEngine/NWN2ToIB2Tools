using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWN2ToIB2Tools
{
    public class GffStruct
    {
        public uint DataOrDataOffset = 0;
        public uint StructType = 0;
        public int FieldCount = 0;
        public List<GffField> Fields = new List<GffField>();

        public GffStruct()
        {

        }
    }
}
