using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace NWN2ToIB2Tools
{
    public class GffFile
    {
        public GffHeader FileHeader;
        public string Filename;
        public GffStruct TopLevelStruct;
        public byte[] fileBytes;

        //used while reading in all data first before processing
        public List<GffStruct> structList = new List<GffStruct>();
        public List<GffField> fieldList = new List<GffField>();
        public List<string> labelList = new List<string>();


        public GffFile()
        {

        }

        public GffFile(string sFilename)
        {
            fileBytes = File.ReadAllBytes(sFilename);
            uint i = 0;
            FileHeader = new GffHeader();
            //header
            FileHeader.FileType = readString(fileBytes, i, 4);
            FileHeader.FileVersion = readString(fileBytes, i += 4, 4);
            FileHeader.StructArrayOffset = readUInt(fileBytes, i += 4, 4);
            FileHeader.StructArrayCount = readUInt(fileBytes, i += 4, 4);
            FileHeader.FieldArrayOffset = readUInt(fileBytes, i += 4, 4);
            FileHeader.FieldArrayCount = readUInt(fileBytes, i += 4, 4);
            FileHeader.LabelArrayOffset = readUInt(fileBytes, i += 4, 4);
            FileHeader.LabelArrayCount = readUInt(fileBytes, i += 4, 4);
            FileHeader.FieldDataArrayOffset = readUInt(fileBytes, i += 4, 4);
            FileHeader.FieldDataArrayCount = readUInt(fileBytes, i += 4, 4);
            FileHeader.FieldIndicesArrayOffset = readUInt(fileBytes, i += 4, 4);
            FileHeader.FieldIndicesArrayCount = readUInt(fileBytes, i += 4, 4);
            FileHeader.ListIndicesArrayOffset = readUInt(fileBytes, i += 4, 4);
            FileHeader.ListIndicesArrayCount = readUInt(fileBytes, i += 4, 4);

            //read structs
            uint startIndex = FileHeader.StructArrayOffset - 4;
            for (int x = 0; x < FileHeader.StructArrayCount; x++)
            {
                GffStruct newStruct = new GffStruct();
                newStruct.StructType = readUInt(fileBytes, startIndex += 4, 4);
                newStruct.DataOrDataOffset = readUInt(fileBytes, startIndex += 4, 4);
                newStruct.FieldCount = readInt(fileBytes, startIndex += 4, 4);
                structList.Add(newStruct);
            }

            //read field array
            startIndex = FileHeader.FieldArrayOffset - 4;
            for (int x = 0; x < FileHeader.FieldArrayCount; x++)
            {
                GffField newField = new GffField();
                newField.Type = (GffFieldType)readInt(fileBytes, startIndex += 4, 4);
                newField.LabelIndex = readInt(fileBytes, startIndex += 4, 4);
                newField.DataOrDataOffset = readUInt(fileBytes, startIndex += 4, 4);
                newField.ValueDword = readUInt(fileBytes, startIndex, 4);
                newField.ValueWord = readUInt(fileBytes, startIndex, 4);
                newField.ValueByte = readInt(fileBytes, startIndex, 4);
                newField.ValueShort = readInt(fileBytes, startIndex, 4);
                newField.ValueInt = readInt(fileBytes, startIndex, 4);
                newField.ValueFloat = readFloat(fileBytes, startIndex, 4);
                fieldList.Add(newField);
            }

            //read labels
            startIndex = FileHeader.LabelArrayOffset - 16;
            for (int x = 0; x < FileHeader.LabelArrayCount; x++)
            {
                string str = readString(fileBytes, startIndex += 16, 16);
                labelList.Add(str);
            }

            //try processing all the structs
            File.Delete("dump.txt");
            TopLevelStruct = BuildStructEntry(structList[0]);
            // serialize JSON directly to a file
            using (StreamWriter file = File.CreateText("test.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.None;
                serializer.Serialize(file, TopLevelStruct);
            }
            foreach(GffField fld in TopLevelStruct.Fields)
            {
                File.AppendAllText("dump.txt", fld.Label + ":" + fld.Type.ToString() + ":" + fld.Data.ToString() + Environment.NewLine);
            }
            //fillDlgLists(topStruct);
            //IbNode root = makeIbCon();
        }

        public GffStruct BuildStructEntry(GffStruct s)
        {
            GffStruct newIbs = new GffStruct();
            if (s.FieldCount == 1) //DataOrDataOffset is index into Field Array
            {
                //print out Field Array data block
                GffField newIbf = BuildFieldArrayEntry((int)s.DataOrDataOffset, newIbs);
                newIbs.Fields.Add(newIbf);
                return newIbs;
            }
            else
            {
                //DataOrDataOffset is byte offset into the Field Indices Array with array of DWORD elements equal to FieldCount.
                //each DWORD element contains an index into the Field Array.
                uint offsetToFieldIndices = s.DataOrDataOffset + FileHeader.FieldIndicesArrayOffset - 4;
                for (int t = 0; t < s.FieldCount; t++)
                {
                    //look at each DWORD in Field Indices and get the index in the Field Array
                    int indx = readInt(fileBytes, offsetToFieldIndices += 4, 4);
                    GffField newIbf = BuildFieldArrayEntry(indx, newIbs);
                    newIbs.Fields.Add(newIbf);
                }
                return newIbs;
            }
        }
        public GffList BuildListEntry(uint index, GffStruct ibs)
        {
            GffList newList = new GffList();
            uint offsetToFieldIndices = index;
            int ListSize = readInt(fileBytes, index, 4);
            for (int h = 0; h < ListSize; h++)
            {
                int indxOfStruct = readInt(fileBytes, offsetToFieldIndices += 4, 4);
                GffStruct newS = BuildStructEntry(structList[indxOfStruct]);
                newList.StructList.Add(newS);
            }
            return newList;
        }
        public GffField BuildFieldArrayEntry(int index, GffStruct ibs)
        {
            GffField f = fieldList[index];
            GffField ibf = new GffField();
            string label = "unknown";
            if (f.LabelIndex < labelList.Count)
            {
                label = labelList[f.LabelIndex];
            }
            ibf.Label = label;
            ibf.Type = f.Type;
            ibf.ValueInt = f.ValueInt;
            ibf.ValueDword = f.ValueDword;
            ibf.ValueFloat = f.ValueFloat;
            string type = f.Type.ToString();
            string data = "unknown";
            object IbData = "unknown";
            if (f.Type == GffFieldType.GFF_BYTE)
            {
                IbData = f.DataOrDataOffset;                
                data = f.DataOrDataOffset.ToString();
            }
            else if (f.Type == GffFieldType.GFF_CHAR)
            {
                IbData = f.DataOrDataOffset;
                data = Convert.ToChar(f.DataOrDataOffset).ToString();
            }
            else if (f.Type == GffFieldType.GFF_WORD)
            {
                IbData = f.DataOrDataOffset;
                data = f.DataOrDataOffset.ToString();
            }
            else if (f.Type == GffFieldType.GFF_SHORT)
            {
                IbData = f.DataOrDataOffset;
                data = f.DataOrDataOffset.ToString();
            }
            else if (f.Type == GffFieldType.GFF_DWORD)
            {
                //IbData = f.DataOrDataOffset;
                IbData = f.ValueDword;
                data = f.DataOrDataOffset.ToString();
            }
            else if (f.Type == GffFieldType.GFF_INT)
            {
                IbData = f.DataOrDataOffset;
                data = f.DataOrDataOffset.ToString();
            }
            else if (f.Type == GffFieldType.GFF_FLOAT)
            {
                //IbData = f.DataOrDataOffset;
                IbData = f.ValueFloat;
                data = f.DataOrDataOffset.ToString();
            }
            else if (f.Type == GffFieldType.GFF_RESREF)
            {
                data = readResRef(fileBytes, f.DataOrDataOffset + FileHeader.FieldDataArrayOffset);
                IbData = data;
            }
            else if (f.Type == GffFieldType.GFF_CEXOSTRING)
            {
                data = readExoString(fileBytes, f.DataOrDataOffset + FileHeader.FieldDataArrayOffset);
                IbData = data;
            }
            else if (f.Type == GffFieldType.GFF_CEXOLOCSTRING)
            {
                data = readExoLocString(fileBytes, f.DataOrDataOffset + FileHeader.FieldDataArrayOffset);
                IbData = data;
            }
            else if (f.Type == GffFieldType.GFF_STRUCT)
            {
                IbData = BuildStructEntry(structList[(int)f.DataOrDataOffset]);
                ibf.Data = IbData;
                return ibf;
            }
            else if (f.Type == GffFieldType.GFF_LIST)
            {
                IbData = BuildListEntry((uint)f.DataOrDataOffset + FileHeader.ListIndicesArrayOffset, ibs);
                ibf.Data = IbData;
                return ibf;
            }
            ibf.Data = IbData;
            return ibf;
        }
        public string readString(byte[] array, uint index, int length)
        {
            string val = "";
            for (uint i = index; i < index + length; i++)
            {
                char c = Convert.ToChar(array[i]);
                if (c == '\0') { continue; }
                val += Convert.ToChar(array[i]).ToString();
            }
            return val;
        }
        public uint readUInt(byte[] array, uint index, int length)
        {
            byte[] newArray = new byte[length];
            Array.Copy(array, index, newArray, 0, length);
            uint iu = BitConverter.ToUInt32(newArray, 0);
            return iu;
        }
        public int readInt(byte[] array, uint index, int length)
        {
            byte[] newArray = new byte[length];
            Array.Copy(array, index, newArray, 0, length);
            int i = BitConverter.ToInt32(newArray, 0);            
            return i;
        }
        public float readFloat(byte[] array, uint index, int length)
        {
            byte[] newArray = new byte[length];
            Array.Copy(array, index, newArray, 0, length);
            float fi = BitConverter.ToSingle(newArray, 0);
            return fi;
        }
        public string readResRef(byte[] array, uint index)
        {
            string val = "";
            byte length = array[index];
            uint startIndexOfString = index + 1;
            for (uint i = startIndexOfString; i < startIndexOfString + length; i++)
            {
                char c = Convert.ToChar(array[i]);
                if (c == '\0') { continue; }
                val += Convert.ToChar(array[i]).ToString();
            }
            return val;
        }
        public string readExoString(byte[] array, uint index)
        {
            string val = "";
            int length = readInt(array, index, 4);
            uint startIndexOfString = index + 4;
            for (uint i = startIndexOfString; i < startIndexOfString + length; i++)
            {
                char c = Convert.ToChar(array[i]);
                if (c == '\0') { continue; }
                val += Convert.ToChar(array[i]).ToString();
            }
            return val;
        }
        public string readExoLocString(byte[] array, uint index)
        {
            string val = "";
            uint length = readUInt(array, index, 4);
            uint stringRef = readUInt(array, index + 4, 4);
            uint stringCount = readUInt(array, index + 8, 4);
            if (stringCount < 1) { return ""; }
            uint subStringId = readUInt(array, index + 12, 4);
            uint subStringLength = readUInt(array, index + 16, 4);
            uint startIndexOfString = index + 20;
            for (uint i = startIndexOfString; i < startIndexOfString + subStringLength; i++)
            {
                char c = Convert.ToChar(array[i]);
                if (c == '\0') { continue; }
                val += Convert.ToChar(array[i]).ToString();
            }
            return val;
        }
    }
}
