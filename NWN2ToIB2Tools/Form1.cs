using Newtonsoft.Json;
using OEIShared.IO.GFF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NWN2ToIB2Tools
{
    public partial class Form1 : Form
    {
        public List<Creature> creaturesList = new List<Creature>();        
        public List<Item> itemsList = new List<Item>();
        public List<Prop> propsList = new List<Prop>();
        public string mainDirectory = "";
        public int nextIndex = 100000;

        public Form1()
        {
            InitializeComponent();
            mainDirectory = Directory.GetCurrentDirectory();
            createLists();
        }

        public void createLists()
        {
            string jobDir = "";
            jobDir = mainDirectory + "\\nwn2_files";
            foreach (string f in Directory.GetFiles(jobDir, "*.*", SearchOption.AllDirectories))
            {
                string filenameNoExt = Path.GetFileNameWithoutExtension(f);
                filenameNoExt = filenameNoExt.Replace(" ", "_");
                string fileExt = Path.GetExtension(f);
                //GFFFile gfftemp = new GFFFile(f);
                //if (gfftemp != null)
                //{
                //    dumpData(mainDirectory + "\\nwn2_files\\" + filenameNoExt + "_" + fileExt + ".txt", gfftemp);
                //}
                //gfftemp = null;

                if ((f.EndsWith(".UTI")) || (f.EndsWith(".UTC")))
                {
                    GFFFile gff = new GFFFile(f);
                    if (gff != null)
                    {
                        if (f.EndsWith(".UTI"))
                        {
                            addItem(gff);
                        }
                        else if (f.EndsWith(".UTC"))
                        {
                            addCreature(gff);
                            addProp(gff);
                        }
                    }
                    gff = null;
                }
                else if (f.EndsWith(".GIT"))
                {
                    GFFFile gff = new GFFFile(f);
                    if (gff != null)
                    {
                        string fullpathForArea = f.Replace(".GIT",".ARE");
                        GFFFile gffArea = new GFFFile(fullpathForArea);
                        if (gffArea != null)
                        {
                            Area newArea = createNewArea(gffArea);                            
                            //go through GIT and find all creatures and place in .lvl file
                            addPropsToArea(gff, newArea);
                            saveAreaFile(mainDirectory + "\\nwn2_files\\" + filenameNoExt + ".lvl", newArea);
                        }                        
                    }
                    gff = null;
                }
            }
            saveItemsFile(mainDirectory + "\\nwn2_files\\items.json");
            saveCreaturesFile(mainDirectory + "\\nwn2_files\\creatures.json");
            savePropsFile(mainDirectory + "\\nwn2_files\\props.json");
        }

        public void dumpData(string filename, GFFFile gff)
        {
            string dump = "";
            foreach (DictionaryEntry field in gff.TopLevelStruct.Fields)
            {
                string val = "";                
                if (field.Value is OEIShared.IO.GFF.GFFOEIExoLocStringField)
                {
                    OEIShared.IO.GFF.GFFOEIExoLocStringField val1 = (OEIShared.IO.GFF.GFFOEIExoLocStringField)field.Value;
                    if (val1.ValueCExoLocString.Strings.Count > 0)
                    {
                        val = val1.ValueCExoLocString.Strings[0].Value;
                    }
                }
                if (field.Value is OEIShared.IO.GFF.GFFOEIExoStringField)
                {
                    OEIShared.IO.GFF.GFFOEIExoStringField val1 = (OEIShared.IO.GFF.GFFOEIExoStringField)field.Value;
                    val = val1.ValueCExoString.Value;
                }
                if (field.Value is OEIShared.IO.GFF.GFFResRefField)
                {
                    OEIShared.IO.GFF.GFFResRefField val1 = (OEIShared.IO.GFF.GFFResRefField)field.Value;
                    val = val1.ValueCResRef.Value;
                }
                if (field.Value is OEIShared.IO.GFF.GFFIntField)
                {
                    OEIShared.IO.GFF.GFFIntField val1 = (OEIShared.IO.GFF.GFFIntField)field.Value;
                    val = val1.Value.ToString();
                }
                if (field.Value is OEIShared.IO.GFF.GFFByteField)
                {
                    OEIShared.IO.GFF.GFFByteField val1 = (OEIShared.IO.GFF.GFFByteField)field.Value;
                    val = val1.Value.ToString();
                }
                if (field.Value is OEIShared.IO.GFF.GFFShortField)
                {
                    OEIShared.IO.GFF.GFFShortField val1 = (OEIShared.IO.GFF.GFFShortField)field.Value;
                    val = val1.Value.ToString();
                }
                if (field.Value is OEIShared.IO.GFF.GFFDwordField)
                {
                    OEIShared.IO.GFF.GFFDwordField val1 = (OEIShared.IO.GFF.GFFDwordField)field.Value;
                    val = val1.Value.ToString();
                }
                if (field.Value is OEIShared.IO.GFF.GFFWordField)
                {
                    OEIShared.IO.GFF.GFFWordField val1 = (OEIShared.IO.GFF.GFFWordField)field.Value;
                    val = val1.Value.ToString();
                }
                dump += (field.Key.ToString() + ":" + field.Value.ToString() + ":" + val + Environment.NewLine);                
            }
            //write string to file
            File.WriteAllText(filename, dump);
        }

        public void addItem(GFFFile gff)
        {
            Item newItem = new Item();
            foreach (DictionaryEntry field in gff.TopLevelStruct.Fields)
            {
                string key = (string)field.Key;
                //resref
                if (key.Equals("TemplateResRef"))
                {
                    OEIShared.IO.GFF.GFFResRefField val1 = (OEIShared.IO.GFF.GFFResRefField)field.Value;
                    newItem.resref = val1.ValueCResRef.Value;
                }
                //tag
                else if (key.Equals("Tag"))
                {
                    OEIShared.IO.GFF.GFFOEIExoStringField val1 = (OEIShared.IO.GFF.GFFOEIExoStringField)field.Value;
                    newItem.tag = val1.ValueCExoString.Value;
                }                
                //desc or descId
                else if ((key.Equals("Description")) || (key.Equals("DescIdentified")))
                {
                    OEIShared.IO.GFF.GFFOEIExoLocStringField val1 = (OEIShared.IO.GFF.GFFOEIExoLocStringField)field.Value;
                    if (val1.ValueCExoLocString.Strings.Count > 0)
                    {
                        if (val1.ValueCExoLocString.Strings[0].Value.Length > newItem.desc.Length)
                        {
                            newItem.desc = val1.ValueCExoLocString.Strings[0].Value;
                        }
                    }
                }
                //cost + modifycost
                else if (key.Equals("Cost"))
                {
                    OEIShared.IO.GFF.GFFDwordField val1 = (OEIShared.IO.GFF.GFFDwordField)field.Value;
                    newItem.value += val1.ValueInt;
                }
                //cost + modifycost
                else if (key.Equals("ModifyCost"))
                {
                    OEIShared.IO.GFF.GFFIntField val1 = (OEIShared.IO.GFF.GFFIntField)field.Value;
                    newItem.value += val1.ValueInt;
                }
                //plot
                else if (key.Equals("Plot"))
                {
                    OEIShared.IO.GFF.GFFByteField val1 = (OEIShared.IO.GFF.GFFByteField)field.Value;
                    if (val1.ValueInt > 0)
                    {
                        newItem.plotItem = true;
                    }
                    else
                    {
                        newItem.plotItem = false;
                    }
                }
                //name
                else if (key.Equals("LocalizedName"))
                {
                    OEIShared.IO.GFF.GFFOEIExoLocStringField val1 = (OEIShared.IO.GFF.GFFOEIExoLocStringField)field.Value;
                    if (val1.ValueCExoLocString.Strings.Count > 0)
                    {
                        newItem.name = val1.ValueCExoLocString.Strings[0].Value;
                    }
                }
                //category
                else if (key.Equals("Classification"))
                {
                    OEIShared.IO.GFF.GFFOEIExoStringField val1 = (OEIShared.IO.GFF.GFFOEIExoStringField)field.Value;
                    string cat = val1.ValueCExoString.Value;
                    if (cat.Contains("184395"))
                    {
                        newItem.ItemCategoryName = "Plot-HC";
                        newItem.category = "General";
                    }
                    else if (cat.Contains("184383"))
                    {
                        newItem.ItemCategoryName = "Shields-HC";
                        newItem.category = "Shield";
                    }
                    else if (cat.Contains("184349"))
                    {
                        newItem.ItemCategoryName = "Boots-HC";
                        newItem.category = "Feet";
                    }
                    else if (cat.Contains("184359"))
                    {
                        newItem.ItemCategoryName = "Helmet-HC";
                        newItem.category = "Head";
                    }
                    else if (cat.Contains("184348"))
                    {
                        newItem.ItemCategoryName = "Armor-HC";
                        newItem.category = "Armor";
                    }
                    else if (cat.Contains("184425"))
                    {
                        newItem.ItemCategoryName = "Books-HC";
                        newItem.category = "General";
                    }
                    else if (cat.Contains("184418"))
                    {
                        newItem.ItemCategoryName = "Amulets-HC";
                        newItem.category = "Neck";
                    }
                    else if (cat.Contains("184420"))
                    {
                        newItem.ItemCategoryName = "Rings-HC";
                        newItem.category = "Ring";
                    }
                    else if (cat.Contains("184419"))
                    {
                        newItem.ItemCategoryName = "Potions-HC";
                        newItem.category = "General";
                    }
                    else if (cat.Contains("184350"))
                    {
                        newItem.ItemCategoryName = "Misc-HC";
                        newItem.category = "General";
                    }
                    else if (cat.Contains("184386"))
                    {
                        newItem.ItemCategoryName = "Weapons-HC";
                        newItem.category = "Ranged";
                    }
                    else if (cat.Contains("184363"))
                    {
                        newItem.ItemCategoryName = "Weapons-HC";
                        newItem.category = "Melee";
                    }
                    else
                    {
                        newItem.ItemCategoryName = val1.ValueCExoString.Value;
                    }
                }




                /*if (field.Value is OEIShared.IO.GFF.GFFOEIExoLocStringField)
                {
                    OEIShared.IO.GFF.GFFOEIExoLocStringField val1 = (OEIShared.IO.GFF.GFFOEIExoLocStringField)field.Value;
                    if (val1.ValueCExoLocString.Strings.Count > 0)
                    {
                        val = val1.ValueCExoLocString.Strings[0].Value;
                    }
                }
                if (field.Value is OEIShared.IO.GFF.GFFOEIExoStringField)
                {
                    OEIShared.IO.GFF.GFFOEIExoStringField val1 = (OEIShared.IO.GFF.GFFOEIExoStringField)field.Value;
                    val = val1.ValueCExoString.Value;
                }
                if (field.Value is OEIShared.IO.GFF.GFFResRefField)
                {
                    OEIShared.IO.GFF.GFFResRefField val1 = (OEIShared.IO.GFF.GFFResRefField)field.Value;
                    val = val1.ValueCResRef.Value;
                }
                if (field.Value is OEIShared.IO.GFF.GFFIntField)
                {
                    OEIShared.IO.GFF.GFFIntField val1 = (OEIShared.IO.GFF.GFFIntField)field.Value;
                    val = val1.Value.ToString();
                }
                if (field.Value is OEIShared.IO.GFF.GFFByteField)
                {
                    OEIShared.IO.GFF.GFFByteField val1 = (OEIShared.IO.GFF.GFFByteField)field.Value;
                    val = val1.Value.ToString();
                }
                if (field.Value is OEIShared.IO.GFF.GFFShortField)
                {
                    OEIShared.IO.GFF.GFFShortField val1 = (OEIShared.IO.GFF.GFFShortField)field.Value;
                    val = val1.Value.ToString();
                }
                if (field.Value is OEIShared.IO.GFF.GFFDwordField)
                {
                    OEIShared.IO.GFF.GFFDwordField val1 = (OEIShared.IO.GFF.GFFDwordField)field.Value;
                    val = val1.Value.ToString();
                }
                if (field.Value is OEIShared.IO.GFF.GFFWordField)
                {
                    OEIShared.IO.GFF.GFFWordField val1 = (OEIShared.IO.GFF.GFFWordField)field.Value;
                    val = val1.Value.ToString();
                }
                dump += (field.Key.ToString() + ":" + field.Value.ToString() + ":" + val + Environment.NewLine);
                */
            }
            itemsList.Add(newItem);
        }
        public void addCreature(GFFFile gff)
        {
            Creature newItem = new Creature();
            string firstname = "";
            string lastname = "";
            foreach (DictionaryEntry field in gff.TopLevelStruct.Fields)
            {
                string key = (string)field.Key;
                //resref
                if (key.Equals("TemplateResRef"))
                {
                    OEIShared.IO.GFF.GFFResRefField val1 = (OEIShared.IO.GFF.GFFResRefField)field.Value;
                    newItem.cr_resref = val1.ValueCResRef.Value;
                }
                //tag
                else if (key.Equals("Tag"))
                {
                    OEIShared.IO.GFF.GFFOEIExoStringField val1 = (OEIShared.IO.GFF.GFFOEIExoStringField)field.Value;
                    newItem.cr_tag = val1.ValueCExoString.Value;
                }
                //desc
                else if (key.Equals("Description"))
                {
                    OEIShared.IO.GFF.GFFOEIExoLocStringField val1 = (OEIShared.IO.GFF.GFFOEIExoLocStringField)field.Value;
                    if (val1.ValueCExoLocString.Strings.Count > 0)
                    {
                        if (val1.ValueCExoLocString.Strings[0].Value.Length > newItem.cr_desc.Length)
                        {
                            newItem.cr_desc = val1.ValueCExoLocString.Strings[0].Value;
                        }
                    }
                }
                //firstname
                else if (key.Equals("FirstName"))
                {
                    OEIShared.IO.GFF.GFFOEIExoLocStringField val1 = (OEIShared.IO.GFF.GFFOEIExoLocStringField)field.Value;
                    if (val1.ValueCExoLocString.Strings.Count > 0)
                    {
                        if (val1.ValueCExoLocString.Strings[0].Value.Length > newItem.cr_desc.Length)
                        {
                            firstname = val1.ValueCExoLocString.Strings[0].Value;
                        }
                    }
                }
                //lastname
                else if (key.Equals("LastName"))
                {
                    OEIShared.IO.GFF.GFFOEIExoLocStringField val1 = (OEIShared.IO.GFF.GFFOEIExoLocStringField)field.Value;
                    if (val1.ValueCExoLocString.Strings.Count > 0)
                    {
                        if (val1.ValueCExoLocString.Strings[0].Value.Length > newItem.cr_desc.Length)
                        {
                            lastname = val1.ValueCExoLocString.Strings[0].Value;
                        }
                    }
                }
                //conversation
                else if (key.Equals("Conversation"))
                {
                    //OEIShared.IO.GFF.GFFResRefField val1 = (OEIShared.IO.GFF.GFFResRefField)field.Value;
                    //newItem. = val1.ValueCResRef.Value;
                }
                //hp
                else if (key.Equals("MaxHitPoints"))
                {
                    OEIShared.IO.GFF.GFFShortField val1 = (OEIShared.IO.GFF.GFFShortField)field.Value;
                    newItem.hp += val1.ValueInt;
                    newItem.hpMax += val1.ValueInt;
                }
                else if (key.Equals("refbonus"))
                {
                    OEIShared.IO.GFF.GFFShortField val1 = (OEIShared.IO.GFF.GFFShortField)field.Value;
                    newItem.reflex += val1.ValueInt;
                }
                else if (key.Equals("willbonus"))
                {
                    OEIShared.IO.GFF.GFFShortField val1 = (OEIShared.IO.GFF.GFFShortField)field.Value;
                    newItem.will += val1.ValueInt;
                }
                else if (key.Equals("fortbonus"))
                {
                    OEIShared.IO.GFF.GFFShortField val1 = (OEIShared.IO.GFF.GFFShortField)field.Value;
                    newItem.fortitude += val1.ValueInt;
                }
                //category
                else if (key.Equals("Classification"))
                {
                    OEIShared.IO.GFF.GFFOEIExoStringField val1 = (OEIShared.IO.GFF.GFFOEIExoStringField)field.Value;
                    string cat = val1.ValueCExoString.Value;
                    if (cat.Contains("184305"))
                    {
                        newItem.cr_parentNodeName = "Animals-HC";
                    }
                    else if (cat.Contains("184321"))
                    {
                        newItem.cr_parentNodeName = "Elementals-HC";
                    }
                    else if (cat.Contains("184320"))
                    {
                        newItem.cr_parentNodeName = "Fey-HC";
                    }
                    else if (cat.Contains("184330"))
                    {
                        newItem.cr_parentNodeName = "Giants-HC";
                    }
                    else if (cat.Contains("184314"))
                    {
                        newItem.cr_parentNodeName = "Humanoids-HC";
                    }
                    else if (cat.Contains("184335"))
                    {
                        newItem.cr_parentNodeName = "NPCs-HC";
                    }
                    else if (cat.Contains("184308"))
                    {
                        newItem.cr_parentNodeName = "Outsiders-HC";
                    }
                    else if (cat.Contains("184329"))
                    {
                        newItem.cr_parentNodeName = "Undead-HC";
                    }                    
                    else
                    {
                        newItem.cr_parentNodeName = val1.ValueCExoString.Value;
                    }
                }
            }
            newItem.cr_name = firstname;
            if (lastname.Length > 0)
            {
                newItem.cr_name = firstname + " " + lastname;
            }
            newItem.cr_tokenFilename = "prp_captive";
            creaturesList.Add(newItem);
        }
        public void addProp(GFFFile gff)
        {
            Prop newItem = new Prop();
            string firstname = "";
            string lastname = "";
            foreach (DictionaryEntry field in gff.TopLevelStruct.Fields)
            {
                string key = (string)field.Key;
                //tag
                if (key.Equals("TemplateResRef"))
                {
                    OEIShared.IO.GFF.GFFResRefField val1 = (OEIShared.IO.GFF.GFFResRefField)field.Value;
                    newItem.PropTag = val1.ValueCResRef.Value;
                }
                //tag
                else if (key.Equals("Tag"))
                {
                    //OEIShared.IO.GFF.GFFOEIExoStringField val1 = (OEIShared.IO.GFF.GFFOEIExoStringField)field.Value;
                    //newItem.PropTag = val1.ValueCExoString.Value;
                }
                //desc
                else if (key.Equals("Description"))
                {
                    OEIShared.IO.GFF.GFFOEIExoLocStringField val1 = (OEIShared.IO.GFF.GFFOEIExoLocStringField)field.Value;
                    if (val1.ValueCExoLocString.Strings.Count > 0)
                    {
                        newItem.MouseOverText = val1.ValueCExoLocString.Strings[0].Value;                        
                    }
                }
                //firstname
                else if (key.Equals("FirstName"))
                {
                    OEIShared.IO.GFF.GFFOEIExoLocStringField val1 = (OEIShared.IO.GFF.GFFOEIExoLocStringField)field.Value;
                    if (val1.ValueCExoLocString.Strings.Count > 0)
                    {
                        firstname = val1.ValueCExoLocString.Strings[0].Value;                        
                    }
                }
                //lastname
                else if (key.Equals("LastName"))
                {
                    OEIShared.IO.GFF.GFFOEIExoLocStringField val1 = (OEIShared.IO.GFF.GFFOEIExoLocStringField)field.Value;
                    if (val1.ValueCExoLocString.Strings.Count > 0)
                    {
                        lastname = val1.ValueCExoLocString.Strings[0].Value;                        
                    }
                }
                //conversation
                else if (key.Equals("Conversation"))
                {
                    OEIShared.IO.GFF.GFFResRefField val1 = (OEIShared.IO.GFF.GFFResRefField)field.Value;
                    newItem.ConversationWhenOnPartySquare = val1.ValueCResRef.Value;
                }
                //category
                else if (key.Equals("Classification"))
                {
                    OEIShared.IO.GFF.GFFOEIExoStringField val1 = (OEIShared.IO.GFF.GFFOEIExoStringField)field.Value;
                    string cat = val1.ValueCExoString.Value;
                    if (cat.Contains("184305"))
                    {
                        newItem.PropCategoryName = "Animals-HC";
                    }
                    else if (cat.Contains("184321"))
                    {
                        newItem.PropCategoryName = "Elementals-HC";
                    }
                    else if (cat.Contains("184320"))
                    {
                        newItem.PropCategoryName = "Fey-HC";
                    }
                    else if (cat.Contains("184330"))
                    {
                        newItem.PropCategoryName = "Giants-HC";
                    }
                    else if (cat.Contains("184314"))
                    {
                        newItem.PropCategoryName = "Humanoids-HC";
                    }
                    else if (cat.Contains("184335"))
                    {
                        newItem.PropCategoryName = "NPCs-HC";
                    }
                    else if (cat.Contains("184308"))
                    {
                        newItem.PropCategoryName = "Outsiders-HC";
                    }
                    else if (cat.Contains("184329"))
                    {
                        newItem.PropCategoryName = "Undead-HC";
                    }
                    else
                    {
                        newItem.PropCategoryName = val1.ValueCExoString.Value;
                    }
                }
            }
            newItem.PropName = firstname;
            newItem.MouseOverText = firstname;
            if (lastname.Length > 0)
            {
                newItem.PropName = firstname + " " + lastname;
                newItem.MouseOverText = firstname + " " + lastname;
            }
            newItem.ImageFileName = "prp_captive";
            propsList.Add(newItem);
        }

        public void addPropsToArea(GFFFile gff, Area area)
        {
            //go through list of creatures and create Props
            foreach (DictionaryEntry field in gff.TopLevelStruct.Fields)
            {
                string key = (string)field.Key;
                //tag
                if (key.Equals("Creature List"))
                {
                    OEIShared.IO.GFF.GFFListField val1 = (OEIShared.IO.GFF.GFFListField)field.Value;
                    OEIShared.IO.GFF.GFFList valList = (OEIShared.IO.GFF.GFFList)val1.Value;
                    foreach(OEIShared.IO.GFF.GFFStruct fld in valList.StructList)
                    {
                        Prop newItem = new Prop();
                        string firstname = "";
                        string lastname = "";
                        foreach (DictionaryEntry field2 in fld.Fields)
                        {
                            string key2 = (string)field2.Key;
                            //tag
                            if (key2.Equals("TemplateResRef"))
                            {
                                OEIShared.IO.GFF.GFFResRefField val2 = (OEIShared.IO.GFF.GFFResRefField)field2.Value;
                                newItem.PropTag = val2.ValueCResRef.Value + "_" + nextIndex.ToString();
                                nextIndex++;
                            }
                            //desc
                            else if (key2.Equals("Description"))
                            {
                                OEIShared.IO.GFF.GFFOEIExoLocStringField val2 = (OEIShared.IO.GFF.GFFOEIExoLocStringField)field2.Value;
                                if (val2.ValueCExoLocString.Strings.Count > 0)
                                {
                                    newItem.MouseOverText = val2.ValueCExoLocString.Strings[0].Value;
                                }
                            }
                            //firstname
                            else if (key2.Equals("FirstName"))
                            {
                                OEIShared.IO.GFF.GFFOEIExoLocStringField val2 = (OEIShared.IO.GFF.GFFOEIExoLocStringField)field2.Value;
                                if (val2.ValueCExoLocString.Strings.Count > 0)
                                {
                                    firstname = val2.ValueCExoLocString.Strings[0].Value;
                                }
                            }
                            //lastname
                            else if (key2.Equals("LastName"))
                            {
                                OEIShared.IO.GFF.GFFOEIExoLocStringField val2 = (OEIShared.IO.GFF.GFFOEIExoLocStringField)field2.Value;
                                if (val2.ValueCExoLocString.Strings.Count > 0)
                                {
                                    lastname = val2.ValueCExoLocString.Strings[0].Value;
                                }
                            }
                            //Xposition
                            else if (key2.Equals("XPosition"))
                            {
                                OEIShared.IO.GFF.GFFFloatField val2 = (OEIShared.IO.GFF.GFFFloatField)field2.Value;
                                int xLoc = (int)((val2.ValueFloat + 0.5f) / 3f);
                                newItem.LocationX = xLoc;
                                //nwn2 each square is 9x9 units so a 4x4 area is 36x36 units and all creatures will be located in the inner 2x2 space so (9,9) to (27,27)
                                //IB2 will assume each 3x3 squares are equal to 9x9 units in nwn2
                            }
                            //Yposition
                            else if (key2.Equals("YPosition"))
                            {                                                              
                                OEIShared.IO.GFF.GFFFloatField val2 = (OEIShared.IO.GFF.GFFFloatField)field2.Value;
                                int yLoc = (int)((val2.ValueFloat + 0.5f) / 3f);
                                //need to invert the y value since IB2 measure top to bottom and nwn2 bottom to top so use MapSizeY - yLoc  
                                newItem.LocationY = area.MapSizeY - yLoc;
                                //nwn2 each square is 9x9 units so a 4x4 area is 36x36 units and all creatures will be located in the inner 2x2 space so (9,9) to (27,27)
                                //IB2 will assume each 3x3 squares are equal to 9x9 units in nwn2
                            }
                            //conversation
                            else if (key2.Equals("Conversation"))
                            {
                                OEIShared.IO.GFF.GFFResRefField val2 = (OEIShared.IO.GFF.GFFResRefField)field2.Value;
                                newItem.ConversationWhenOnPartySquare = val2.ValueCResRef.Value;
                            }                            
                        }
                        newItem.PropName = firstname;
                        newItem.MouseOverText = firstname;
                        if (lastname.Length > 0)
                        {
                            newItem.PropName = firstname + " " + lastname;
                            newItem.MouseOverText = firstname + " " + lastname;
                        }
                        newItem.ImageFileName = "prp_captive";
                        area.Props.Add(newItem);
                    }
                }
            }
        }        
        private Area createNewArea(GFFFile gffARE)
        {
            //create tilemap
            Area area = new Area();
                        
            foreach (DictionaryEntry field in gffARE.TopLevelStruct.Fields)
            {
                string key = (string)field.Key;
                if (key.Equals("ResRef"))
                {
                    OEIShared.IO.GFF.GFFResRefField val1 = (OEIShared.IO.GFF.GFFResRefField)field.Value;
                    string resref = val1.ValueCResRef.Value;
                    area.Filename = resref.Replace(" ","_");
                }
                //in game name
                else if (key.Equals("Name"))
                {
                    OEIShared.IO.GFF.GFFOEIExoLocStringField val1 = (OEIShared.IO.GFF.GFFOEIExoLocStringField)field.Value;
                    if (val1.ValueCExoLocString.Strings.Count > 0)
                    {
                        area.inGameAreaName = val1.ValueCExoLocString.Strings[0].Value;
                    }
                }
                //width
                else if (key.Equals("Width"))
                {
                    OEIShared.IO.GFF.GFFIntField val1 = (OEIShared.IO.GFF.GFFIntField)field.Value;
                    area.MapSizeX = val1.ValueInt * 3;
                }
                //height
                else if (key.Equals("Height"))
                {
                    OEIShared.IO.GFF.GFFIntField val1 = (OEIShared.IO.GFF.GFFIntField)field.Value;
                    area.MapSizeY = val1.ValueInt * 3;
                }
            }
            
            for (int index = 0; index < (area.MapSizeX * area.MapSizeY); index++)
            {
                Tile newTile = new Tile();
                newTile.Layer1Filename = "t_grass";
                newTile.Walkable = true;
                newTile.LoSBlocked = false;
                newTile.Visible = false;
                area.Tiles.Add(newTile);
            }
            return area;
        }
        
        public void saveAreaFile(string filename, Area a)
        {
            string json = JsonConvert.SerializeObject(a, Newtonsoft.Json.Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.Write(json.ToString());
            }
        }
        public void saveCreaturesFile(string filename)
        {
            string json = JsonConvert.SerializeObject(creaturesList, Newtonsoft.Json.Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.Write(json.ToString());
            }
        }
        public void saveItemsFile(string filename)
        {
            string json = JsonConvert.SerializeObject(itemsList, Newtonsoft.Json.Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.Write(json.ToString());
            }
        }
        public void savePropsFile(string filename)
        {
            string json = JsonConvert.SerializeObject(propsList, Newtonsoft.Json.Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.Write(json.ToString());
            }
        }
    }
}
