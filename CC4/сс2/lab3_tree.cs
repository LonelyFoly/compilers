using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace сс2
{
    public class ParseNode
    {
        public string Name { get; }
        public List<ParseNode> Children { get; }

        public ParseNode(string name)
        {
            Name = name;
            Children = new List<ParseNode>();
        }

        public void Add(ParseNode child) => Children.Add(child);

        public void Print(string indent = "", bool last = true)
        {
            Console.Write(indent);
            Console.Write(last ? "└─ " : "├─ ");
            Console.WriteLine(Name);

            for (int i = 0; i < Children.Count; i++)
                Children[i].Print(indent + (last ? "   " : "│  "), i == Children.Count - 1);
        }
    }


}
