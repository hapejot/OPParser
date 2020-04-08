using System;
using System.Text;
using System.Collections.Generic;

namespace Hapejot.OPParser
{
    [Serializable]
    public class ParseTreeNode 
    {
        private string _name;
        private List<ParseTreeNode> _children = new List<ParseTreeNode>();
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public ParseTreeNode this[int i]
        {
            get { return _children[i]; }
            set 
            {
                for (int x = _children.Count; x <= i; x++)
                    _children.Add(null);
                _children[i] = value; 
            }
        }

        public int Arity
        {
            get { return _children.Count; }
        }

        public void PrintTo(StringBuilder pResult)
        {
            pResult.Append(Name);
            if (_children.Count > 0)
            {
                pResult.Append("(");
                string sep = "";
                for (int i = 0; i < _children.Count; i++)
                {
                    pResult.Append(sep);
                    sep = ",";
                    if (_children[i] != null)
                    {
                        _children[i].PrintTo(pResult);
                    }
                }
                pResult.Append(")");
            }
        }

        public virtual ValueNode GetNode()
        {
            return new ValueNode(_children[0].Name);
        }

    }
}
