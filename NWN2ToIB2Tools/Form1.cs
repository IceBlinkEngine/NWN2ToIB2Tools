using Newtonsoft.Json;
using OEIShared.IO.GFF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace NWN2ToIB2Tools
{
    public partial class Form1 : Form
    {
        public string mainDirectory = "";
        public List<Creature> creaturesList = new List<Creature>();        
        public List<Item> itemsList = new List<Item>();
        public List<Prop> propsList = new List<Prop>();
        public List<JournalQuest> moduleJournal = new List<JournalQuest>();
        public List<Convo> moduleConvoList = new List<Convo>();
        public List<string> portraits_needed = new List<string>();        
        public int nextIndex = 100000;
        public int nextLinkIdNumber = 100000;
        public List<SyncStruct> dlgStartingnodeList = new List<SyncStruct>();
        public List<ContentNode> dlgEntrynodeList = new List<ContentNode>();
        public List<ContentNode> dlgReplynodeList = new List<ContentNode>();

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
                
                if ((f.EndsWith(".UTI")) || (f.EndsWith(".UTC")))
                {
                    GffFile gff = new GffFile(f);
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
                else if ((f.EndsWith(".JRL") || f.EndsWith(".jrl")))
                {
                    GffFile gff = new GffFile(f);
                    if (gff != null)
                    {
                        convertJournal(gff);
                    }
                    gff = null;
                }
                else if ((f.EndsWith(".DLG") || f.EndsWith(".dlg")))
                {
                    GffFile gff = new GffFile(f);
                    if (gff != null)
                    {
                        fillDlgLists(gff);
                        Convo newConvo = makeIbCon(filenameNoExt);
                        saveConvo(newConvo, mainDirectory + "\\nwn2_files\\" + filenameNoExt + ".json");
                    }
                    gff = null;
                }
                else if (f.EndsWith(".GIT"))
                {
                    GffFile gff = new GffFile(f);
                    if (gff != null)
                    {
                        string fullpathForArea = f.Replace(".GIT", ".ARE");
                        GffFile gffArea = new GffFile(fullpathForArea);
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
            File.AppendAllLines("portraits_needed.txt", portraits_needed);
        }
                
        public void addItem(GffFile gff)
        {
            Item newItem = new Item();
            foreach (GffField field in gff.TopLevelStruct.Fields)
            {
                string key = field.Label;
                //resref
                if (key.Equals("TemplateResRef"))
                {
                    newItem.resref = field.Data.ToString();
                }
                //tag
                else if (key.Equals("Tag"))
                {
                    newItem.tag = field.Data.ToString();
                }                
                //desc or descId
                else if ((key.Equals("Description")) || (key.Equals("DescIdentified")))
                {
                    newItem.desc = field.Data.ToString();
                }
                //cost + modifycost
                else if (key.Equals("Cost"))
                {
                    newItem.value += (int)field.ValueInt;
                }
                //cost + modifycost
                else if (key.Equals("ModifyCost"))
                {
                    newItem.value += (int)field.ValueInt;
                }
                //plot
                else if (key.Equals("Plot"))
                {
                    int isPlot = (int)field.ValueByte;
                    if (isPlot > 0)
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
                    newItem.name = field.Data.ToString();                    
                }
                //category
                else if (key.Equals("Classification"))
                {
                    string cat = field.Data.ToString();
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
                        newItem.ItemCategoryName = field.Data.ToString();
                    }
                }                
            }
            itemsList.Add(newItem);
        }
        public void addCreature(GffFile gff)
        {
            Creature newItem = new Creature();
            string firstname = "";
            string lastname = "";
            foreach (GffField field in gff.TopLevelStruct.Fields)
            {
                string key = field.Label;
                //resref
                if (key.Equals("TemplateResRef"))
                {
                    newItem.cr_resref = field.Data.ToString();
                }
                //tag
                else if (key.Equals("Tag"))
                {
                    newItem.cr_tag = field.Data.ToString();
                }
                //desc
                else if (key.Equals("Description"))
                {
                    newItem.cr_desc = field.Data.ToString();
                }
                //firstname
                else if (key.Equals("FirstName"))
                {
                    firstname = field.Data.ToString();
                }
                //lastname
                else if (key.Equals("LastName"))
                {
                    lastname = field.Data.ToString();
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
                    newItem.hp = (int)field.ValueShort;
                    newItem.hpMax = (int)field.ValueShort;
                }
                else if (key.Equals("refbonus"))
                {
                    newItem.reflex = (int)field.ValueShort;
                }
                else if (key.Equals("willbonus"))
                {
                    newItem.will = (int)field.ValueShort;
                }
                else if (key.Equals("fortbonus"))
                {
                    newItem.fortitude = (int)field.ValueShort;
                }
                //category
                else if (key.Equals("Classification"))
                {
                    string cat = field.Data.ToString();
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
                        newItem.cr_parentNodeName = field.Data.ToString();
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
        public void addProp(GffFile gff)
        {
            Prop newItem = new Prop();
            string firstname = "";
            string lastname = "";
            foreach (GffField field in gff.TopLevelStruct.Fields)
            {
                string key = field.Label;
                //tag
                if (key.Equals("TemplateResRef"))
                {
                    newItem.PropTag = field.Data.ToString();
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
                    newItem.MouseOverText = field.Data.ToString();
                }
                //firstname
                else if (key.Equals("FirstName"))
                {
                    firstname = field.Data.ToString();
                }
                //lastname
                else if (key.Equals("LastName"))
                {
                    lastname = field.Data.ToString();
                }
                //conversation
                else if (key.Equals("Conversation"))
                {
                    newItem.ConversationWhenOnPartySquare = field.Data.ToString();
                }
                //category
                else if (key.Equals("Classification"))
                {
                    string cat = field.Data.ToString();
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
                        newItem.PropCategoryName = field.Data.ToString();
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

        public void addPropsToArea(GffFile gff, Area area)
        {
            //go through list of creatures and create Props
            foreach (GffField field in gff.TopLevelStruct.Fields)
            {
                string key = field.Label;
                //tag
                if (key.Equals("Creature List"))
                {
                    GffList valList = (GffList)field.Data;
                    foreach(GffStruct fld in valList.StructList)
                    {
                        Prop newItem = new Prop();
                        string firstname = "";
                        string lastname = "";
                        foreach (GffField field2 in fld.Fields)
                        {
                            string key2 = field2.Label;
                            //tag
                            if (key2.Equals("TemplateResRef"))
                            {
                                newItem.PropTag = field2.Data.ToString() + "_" + nextIndex.ToString();
                                nextIndex++;
                            }
                            //desc
                            else if (key2.Equals("Description"))
                            {
                                newItem.MouseOverText = field2.Data.ToString();
                            }
                            //firstname
                            else if (key2.Equals("FirstName"))
                            {
                                firstname = field2.Data.ToString();
                            }
                            //lastname
                            else if (key2.Equals("LastName"))
                            {
                                lastname = field2.Data.ToString();
                            }
                            //Xposition
                            else if (key2.Equals("XPosition"))
                            {
                                int xLoc = (int)((field2.ValueFloat + 0.5f) / 3f);
                                newItem.LocationX = xLoc;
                                //nwn2 each square is 9x9 units so a 4x4 area is 36x36 units and all creatures will be located in the inner 2x2 space so (9,9) to (27,27)
                                //IB2 will assume each 3x3 squares are equal to 9x9 units in nwn2
                            }
                            //Yposition
                            else if (key2.Equals("YPosition"))
                            {                                                              
                                int yLoc = (int)((field2.ValueFloat + 0.5f) / 3f);
                                //need to invert the y value since IB2 measure top to bottom and nwn2 bottom to top so use MapSizeY - yLoc  
                                newItem.LocationY = area.MapSizeY - yLoc;
                                //nwn2 each square is 9x9 units so a 4x4 area is 36x36 units and all creatures will be located in the inner 2x2 space so (9,9) to (27,27)
                                //IB2 will assume each 3x3 squares are equal to 9x9 units in nwn2
                            }
                            //conversation
                            else if (key2.Equals("Conversation"))
                            {
                                newItem.ConversationWhenOnPartySquare = field2.Data.ToString();
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
        private Area createNewArea(GffFile gffARE)
        {
            //create tilemap
            Area area = new Area();
                        
            foreach (GffField field in gffARE.TopLevelStruct.Fields)
            {
                string key = field.Label;
                if (key.Equals("ResRef"))
                {
                    string resref = field.Data.ToString();
                    area.Filename = resref.Replace(" ","_");
                }
                //in game name
                else if (key.Equals("Name"))
                {
                    area.inGameAreaName = field.Data.ToString();
                }
                //width
                else if (key.Equals("Width"))
                {
                    area.MapSizeX = (int)field.ValueInt * 3;
                }
                //height
                else if (key.Equals("Height"))
                {
                    area.MapSizeY = (int)field.ValueInt * 3;
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
        
        public void convertJournal(GffFile gff)
        {
            foreach (GffField field in gff.TopLevelStruct.Fields)
            {
                string key0 = field.Label;
                if (key0.Equals("Categories"))
                {
                    GffList valList0 = (GffList)field.Data;
                    foreach (GffStruct fld0 in valList0.StructList)
                    {
                        JournalQuest newQuest = new JournalQuest();
                        foreach (GffField field20 in fld0.Fields)
                        {
                            string key = field20.Label;
                            //resref
                            if (key.Equals("Name"))
                            {
                                newQuest.Name = field20.Data.ToString();
                            }
                            //tag
                            else if (key.Equals("Tag"))
                            {
                                newQuest.Tag = field20.Data.ToString();
                            }
                            else if (key.Equals("EntryList"))
                            {
                                GffList valList = (GffList)field20.Data;
                                foreach (GffStruct fld in valList.StructList)
                                {
                                    #region Quests
                                    JournalEntry newEntry = new JournalEntry();
                                    newEntry.EntryTitle = newQuest.Name;
                                    foreach (GffField field2 in fld.Fields)
                                    {
                                        #region Entries
                                        string key2 = field2.Label;
                                        //EntryText
                                        if (key2.Equals("Text"))
                                        {
                                            newEntry.EntryText = field2.Data.ToString();
                                        }
                                        //EntryId (int)
                                        else if (key2.Equals("ID"))
                                        {
                                            newEntry.EntryId = field2.ValueInt;
                                            newEntry.Tag = field2.ValueInt.ToString();
                                        }
                                        //EndPoint (bool)
                                        else if (key2.Equals("End"))
                                        {
                                            int isEnd = field2.ValueByte;
                                            if (isEnd > 0)
                                            {
                                                newEntry.EndPoint = true;
                                            }
                                            else
                                            {
                                                newEntry.EndPoint = false;
                                            }
                                        }
                                        #endregion
                                    }
                                    newQuest.Entries.Add(newEntry);
                                    #endregion
                                }
                            }
                        }
                        moduleJournal.Add(newQuest);
                    }
                }
            }
        }
        
        public void fillDlgLists(GffFile gff)
        {
            dlgStartingnodeList.Clear();
            dlgEntrynodeList.Clear();
            dlgReplynodeList.Clear();
            int nextIdNumber = 1;

            foreach (GffField ibf in gff.TopLevelStruct.Fields)
            {
                if (ibf.Label.Equals("StartingList")) //add all startingList nodes, these are NPC nodes
                {                    
                    GffList thisList = (GffList)ibf.Data;
                    foreach (GffStruct istr in thisList.StructList) //this is a list of the root level entry nodes
                    {
                        //STRUCTS are containers of field(s), the fields have the actual data for this node
                        //these nodes will only have pointers (any data in SyncStruct actually) added at this time, later 
                        //we'll get the actual node data once the EntryList is populated with nodes
                        SyncStruct newSyncStruct = new SyncStruct();
                        foreach (GffField ifld in istr.Fields)
                        {
                            #region all the SyncStruct data                      
                            if (ifld.Label.Equals("Index"))
                            {
                                //this is the index of an EntryList Dialog Struct node
                                newSyncStruct.Index = ifld.ValueDword;
                            }
                            else if (ifld.Label.Equals("ShowOnce"))
                            {
                                newSyncStruct.ShowOnce = false;
                                if (ifld.ValueInt != 0)
                                {
                                    newSyncStruct.ShowOnce = true;
                                }
                            }
                            else if (ifld.Label.Equals("IsChild"))
                            {
                                newSyncStruct.IsChild = false;
                                if (ifld.ValueInt != 0)
                                {
                                    newSyncStruct.IsChild = true;
                                }
                            }
                            #endregion
                        }
                        dlgStartingnodeList.Add(newSyncStruct);
                    }                    
                }
                if (ibf.Label.Equals("EntryList")) //these are all NPC nodes
                {
                    GffList thisList = (GffList)ibf.Data;
                    foreach (GffStruct istr in thisList.StructList)
                    {
                        //this node 'newEntryNode' will get its conditionals, showonce, isLink from its parent node's subNode list once it is assigned
                        ContentNode newEntryNode = new ContentNode();
                        newEntryNode.pcNode = false;
                        newEntryNode.idNum = nextIdNumber;
                        nextIdNumber++;
                        foreach (GffField ifld in istr.Fields)
                        {
                            #region all the DialogStruct data    

                            if (ifld.Label.Equals("Text"))
                            {
                                newEntryNode.conversationText = ifld.Data.ToString();
                                if ((newEntryNode.conversationText.Equals("")) || (newEntryNode.conversationText.Equals("||")))
                                {
                                    newEntryNode.conversationText = "Continue";
                                }
                            }
                            if ((ifld.Label.Equals("Speaker")) && (!ifld.Data.ToString().Equals("")))
                            {
                                newEntryNode.NodePortraitBitmap = "ptr_" + ifld.Data.ToString();
                                if (!portraits_needed.Contains(newEntryNode.NodePortraitBitmap))
                                {
                                    portraits_needed.Add(newEntryNode.NodePortraitBitmap);
                                }
                                newEntryNode.NodeNpcName = char.ToUpper(ifld.Data.ToString()[0]) + ifld.Data.ToString().Substring(1);
                            }
                            if (ifld.Label.Equals("RepliesList"))
                            {
                                GffList  replyIndexList = (GffList)ifld.Data;
                                foreach (GffStruct irepstr in replyIndexList.StructList)
                                {
                                    ContentNode subNodeOnEntryNode = new ContentNode();
                                    SyncStruct newSyncStruct = new SyncStruct();
                                    foreach (GffField irepfld in irepstr.Fields)
                                    {
                                        // these nodes will only have pointers(any data in SyncStruct actually) added at this time, later
                                        // we'll get the actual node data from the pointed to ReplyList DialogStruct
                                        #region all the SyncStruct data
                                        if (irepfld.Label.Equals("Index"))
                                        {
                                            newSyncStruct.Index = irepfld.ValueDword;
                                        }
                                        else if (irepfld.Label.Equals("ShowOnce"))
                                        {
                                            newSyncStruct.ShowOnce = false;
                                            if (irepfld.ValueInt != 0)
                                            {
                                                newSyncStruct.ShowOnce = true;
                                            }
                                        }
                                        else if (irepfld.Label.Equals("IsChild"))
                                        {
                                            newSyncStruct.IsChild = false;
                                            if (irepfld.ValueInt != 0)
                                            {
                                                newSyncStruct.IsChild = true;
                                            }
                                        }
                                        #endregion
                                    }
                                    newEntryNode.syncStructs.Add(newSyncStruct);
                                }
                            }
                            if (ifld.Label.Equals("ScriptList"))
                            {
                                GffList scriptList = (GffList)ifld.Data;
                                foreach (GffStruct irepstr in scriptList.StructList)
                                {
                                    foreach (GffField irepfld in irepstr.Fields)
                                    {
                                        if (irepfld.Label.Equals("Script"))
                                        {
                                            //this is the index of EntryList Dialog Struct
                                        }
                                    }
                                }
                            }
                            #endregion
                        }                        
                        dlgEntrynodeList.Add(newEntryNode);
                    }
                }
                if (ibf.Label.Equals("ReplyList")) //These are all PC nodes
                {
                    GffList thisList = (GffList)ibf.Data;
                    foreach (GffStruct istr in thisList.StructList)
                    {
                        //this node 'newReplyNode' will get its conditionals, showonce, isLink from its parent node's subNode list once it is assigned
                        ContentNode newReplyNode = new ContentNode();
                        newReplyNode.pcNode = true;
                        newReplyNode.idNum = nextIdNumber;
                        nextIdNumber++;
                        foreach (GffField ifld in istr.Fields)
                        {
                            #region all the DialogStruct data
                            if (ifld.Label.Equals("Text"))
                            {
                                newReplyNode.conversationText = ifld.Data.ToString();
                                if ((newReplyNode.conversationText.Equals("")) || (newReplyNode.conversationText.Equals("||")))
                                {
                                    newReplyNode.conversationText = "Continue";
                                }
                            }
                            if ((ifld.Label.Equals("Speaker")) && (!ifld.Data.ToString().Equals("")))
                            {
                                newReplyNode.NodePortraitBitmap = "ptr_" + ifld.Data.ToString();
                                if (!portraits_needed.Contains(newReplyNode.NodePortraitBitmap))
                                {
                                    portraits_needed.Add(newReplyNode.NodePortraitBitmap);
                                }
                                newReplyNode.NodeNpcName = char.ToUpper(ifld.Data.ToString()[0]) + ifld.Data.ToString().Substring(1);
                            }
                            if (ifld.Label.Equals("EntriesList"))
                            {
                                //this is the index of EntryList Dialog Struct
                                GffList replyIndexList = (GffList)ifld.Data;
                                foreach (GffStruct irepstr in replyIndexList.StructList)
                                {
                                    ContentNode subNodeOnReplyNode = new ContentNode();
                                    SyncStruct newSyncStruct = new SyncStruct();
                                    foreach (GffField irepfld in irepstr.Fields)
                                    {
                                        // these nodes will only have pointers(any data in SyncStruct actually) added at this time, later
                                        // we'll get the actual node data from the pointed to EntryList DialogStruct
                                        #region all the SyncStruct data
                                        if (irepfld.Label.Equals("Index"))
                                        {
                                            newSyncStruct.Index = irepfld.ValueDword;
                                        }
                                        else if (irepfld.Label.Equals("ShowOnce"))
                                        {
                                            newSyncStruct.ShowOnce = false;
                                            if (irepfld.ValueInt != 0)
                                            {
                                                newSyncStruct.ShowOnce = true;
                                            }
                                        }
                                        else if (irepfld.Label.Equals("IsChild"))
                                        {
                                            newSyncStruct.IsChild = false;
                                            if (irepfld.ValueInt != 0)
                                            {
                                                newSyncStruct.IsChild = true;
                                            }
                                        }
                                        #endregion
                                    }
                                    newReplyNode.syncStructs.Add(newSyncStruct);
                                }
                            }
                            if (ifld.Label.Equals("ScriptList"))
                            {
                                GffList scriptList = (GffList)ifld.Data;
                                foreach (GffStruct irepstr in scriptList.StructList)
                                {
                                    foreach (GffField irepfld in irepstr.Fields)
                                    {
                                        if (irepfld.Label.Equals("Script"))
                                        {
                                            //this is the index of EntryList Dialog Struct
                                        }
                                    }
                                }
                            }                            
                            #endregion
                        }                        
                        dlgReplynodeList.Add(newReplyNode);
                    }
                }
            }
        }        
        public Convo makeIbCon(string filename)
        {
            //THE BASICS
            //the DialogStruct data will be merged with the SyncStruct data on the parent node's subNodes by 
            //going through each parent subNode, using the pointer to get the actual DialogStruct data and adding that
            //data to the subNode.            
            Convo newConvo = new Convo();
            newConvo.ConvoFileName = filename;
            newConvo.DefaultNpcName = filename;
            newConvo.NpcPortraitBitmap = "ptr_" + filename;

            //this is the root node of the convo, all the StartingList nodes will be subnodes of this rootNode
            ContentNode rootNode = new ContentNode();
            rootNode.idNum = 0;
            rootNode.conversationText = "root";
            //nodes in the StartingList, these nodes are added to the rootNode after they find (pointers) and load their subnodes (all branches recursively)
            foreach (SyncStruct startingListSync in dlgStartingnodeList) 
            {
                ContentNode subNodeOfRootNode = dlgEntrynodeList[(int)startingListSync.Index].DeepCopy();
                //add syncstruct data to this node
                subNodeOfRootNode.isLink = startingListSync.IsChild;
                subNodeOfRootNode.ShowOnlyOnce = startingListSync.ShowOnce;
                //TODO add all conditionals in sync to this above node as well

                if (!startingListSync.IsChild) //node is NOT a link so add all subNodes
                {
                    foreach (SyncStruct sync2 in subNodeOfRootNode.syncStructs) //pointers to nodes in 
                    {
                        subNodeOfRootNode.subNodes.Add(addIbContentNode(dlgReplynodeList[(int)sync2.Index], sync2));
                    }
                }
                else //is a link so create a blank node with a idNum incremented start at 100000 and assign the link to ID
                {
                    //make a blank node
                    subNodeOfRootNode = new ContentNode(); 
                    //add syncstruct data to this node
                    subNodeOfRootNode.isLink = startingListSync.IsChild;
                    subNodeOfRootNode.ShowOnlyOnce = startingListSync.ShowOnce;
                    //assign LinkIdNumber
                    subNodeOfRootNode.idNum = nextLinkIdNumber;
                    nextLinkIdNumber++;
                    //assign linkTo
                    if (startingListSync.Index < dlgEntrynodeList.Count)
                    {
                        subNodeOfRootNode.linkTo = dlgEntrynodeList[(int)startingListSync.Index].idNum;
                    }
                    else //broken or bad link so comment
                    {
                        subNodeOfRootNode.linkTo = -1;
                        subNodeOfRootNode.conversationText = "[BROKEN LINK:]";
                    }
                }                
                rootNode.subNodes.Add(subNodeOfRootNode);
            }           
            newConvo.subNodes.Add(rootNode);
            return newConvo;
        }
        public ContentNode addIbContentNode(ContentNode thisNode, SyncStruct thisSync)
        {
            ContentNode newNode = thisNode.DeepCopy(); //does not copy the subnodes or pointers
            //need to add sync stuff to this newNode
            newNode.isLink = thisSync.IsChild;
            newNode.ShowOnlyOnce = thisSync.ShowOnce;            

            foreach (SyncStruct s in newNode.syncStructs)
            {
                if (newNode.pcNode) //PC node
                {
                    if (s.IsChild) //node is a link
                    {
                        //make a blank node
                        ContentNode subNodeOfNewNode = new ContentNode();
                        //add syncstruct data to this node
                        subNodeOfNewNode.isLink = s.IsChild;
                        subNodeOfNewNode.ShowOnlyOnce = s.ShowOnce;
                        //assign LinkIdNumber
                        subNodeOfNewNode.idNum = nextLinkIdNumber;
                        nextLinkIdNumber++;
                        //assign linkTo
                        if (s.Index < dlgEntrynodeList.Count)
                        {
                            subNodeOfNewNode.linkTo = dlgEntrynodeList[(int)s.Index].idNum;
                        }
                        else //broken or bad link so comment
                        {
                            subNodeOfNewNode.linkTo = -1;
                            subNodeOfNewNode.conversationText = "[BROKEN LINK:]";
                        }
                        newNode.subNodes.Add(subNodeOfNewNode);
                    }
                    else
                    {
                        newNode.subNodes.Add(addIbContentNode(dlgEntrynodeList[(int)s.Index], s));
                    }
                }
                else //NPC node
                {
                    if (s.IsChild) //node is a link
                    {
                        //make a blank node
                        ContentNode subNodeOfNewNode = new ContentNode();
                        //add syncstruct data to this node
                        subNodeOfNewNode.isLink = s.IsChild;
                        subNodeOfNewNode.ShowOnlyOnce = s.ShowOnce;
                        //assign LinkIdNumber
                        subNodeOfNewNode.idNum = nextLinkIdNumber;
                        nextLinkIdNumber++;
                        //assign linkTo
                        if (s.Index < dlgReplynodeList.Count)
                        {
                            subNodeOfNewNode.linkTo = dlgReplynodeList[(int)s.Index].idNum;
                        }
                        else //broken or bad link so comment
                        {
                            subNodeOfNewNode.linkTo = -1;
                            subNodeOfNewNode.conversationText = "[BROKEN LINK:]";
                        }                        
                        newNode.subNodes.Add(subNodeOfNewNode);
                    }
                    else
                    {
                        newNode.subNodes.Add(addIbContentNode(dlgReplynodeList[(int)s.Index], s));
                    }
                }
            }
            return newNode;
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
        public void saveConvo(Convo conv, string filename)
        {
            string json = JsonConvert.SerializeObject(conv, Newtonsoft.Json.Formatting.Indented);
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

    public class SyncStruct
    {
        public uint Index = 0;
        public bool ShowOnce = false;
        public bool IsChild = false;
        public List<Condition> conditions = new List<Condition>();

        public SyncStruct()
        {

        }
        public SyncStruct DeepCopy()
        {
            SyncStruct copy = new SyncStruct();
            copy.Index = this.Index;
            copy.ShowOnce = this.ShowOnce;
            copy.IsChild = this.IsChild;
            copy.conditions.Clear();
            foreach(Condition c in this.conditions)
            {
                copy.conditions.Add(c.DeepCopy());
            }
            return copy;
        }
    }
}
