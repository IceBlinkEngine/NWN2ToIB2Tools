using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NWN2ToIB2Tools
{
    [Serializable]
    public class ContentNode
    {        
        public int idNum = -1;
        public bool pcNode = true;
        public int linkTo = 0;
        public bool ShowOnlyOnce = false;
        public bool NodeIsActive = true;
        public string NodePortraitBitmap = "";
        public string NodeNpcName = "";
        public string conversationText = "Continue";
        public bool IsExpanded = true;
        public List<int> subnodePointer = new List<int>();
        public List<SyncStruct> syncStructs = new List<SyncStruct>();
        public List<ContentNode> subNodes = new List<ContentNode>();
        public List<Action> actions = new List<Action>();
        public List<Condition> conditions = new List<Condition>();
        public bool isLink = false;

        public ContentNode()
        {
        }

        public ContentNode SearchContentNodeById(int checkIdNum)
        {
            ContentNode tempNode = null;
            if (idNum == checkIdNum)
            {
                return this;
            }
            foreach (ContentNode subNode in subNodes)
            {
                tempNode = subNode.SearchContentNodeById(checkIdNum);
                if (tempNode != null)
                {
                    return tempNode;
                }
            }
            return null;
        }
        public ContentNode DeepCopy()
        {
            ContentNode copy = new ContentNode();

            copy.idNum = this.idNum;
            copy.pcNode = this.pcNode;
            copy.linkTo = this.linkTo;
            copy.ShowOnlyOnce = this.ShowOnlyOnce;
            copy.NodeIsActive = this.NodeIsActive;
            copy.NodePortraitBitmap = this.NodePortraitBitmap;
            copy.NodeNpcName = this.NodeNpcName;
            copy.conversationText = this.conversationText;
            copy.IsExpanded = this.IsExpanded;
            copy.isLink = this.isLink;

            copy.syncStructs.Clear();
            foreach (SyncStruct s in this.syncStructs)
            {
                copy.syncStructs.Add(s.DeepCopy());
            }

            copy.actions.Clear();
            foreach (Action a in this.actions)
            {
                copy.actions.Add(a.DeepCopy());
            }

            copy.conditions.Clear();
            foreach (Condition c in this.conditions)
            {
                copy.conditions.Add(c.DeepCopy());
            }
            //do not copy subnodes or pointers

            return copy;
        }
    }        
}
