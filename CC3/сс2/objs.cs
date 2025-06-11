using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace сс2
{
    public class Symbol
    {
        public string Name { get; set; }
        public bool IsNullable { get; set; }

        public Symbol(string name)
        {
            Name = name;
            IsNullable = false;
        }

        public override bool Equals(object obj)
        {
            if (obj is Symbol other)
            {
                return Name == other.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name?.Sum(c => (int)c) ?? 0;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class EmptySymbol : Symbol
    {
        public EmptySymbol() : base(null)
        {
            IsNullable = false;
        }

        public override bool Equals(object obj)
        {
            return obj is EmptySymbol;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "~";
        }
    }
    public class Terminal : Symbol
    {
        public Terminal(string name) : base(name) { }

        public override bool Equals(object obj)
        {
            return obj is Terminal other && Name == other.Name;
        }
    }

    public class Nonterminal : Symbol
    {
        public Nonterminal(string name, bool isNullable = true) : base(name)
        {
            IsNullable = isNullable;
        }

        /* public override bool Equals(object obj)
         {
             return obj is Nonterminal other &&
                    Name == other.Name &&
                    IsNullable == other.IsNullable;
         }

         public override int GetHashCode()
         {
             int resultHash = Name?.Sum(c => (int)c) ?? 0;
             return IsNullable ? resultHash : resultHash + 1000;
         }*/
        public override bool Equals(object obj)
        {
            return obj is Nonterminal other && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return IsNullable ? Name : $"nonnullable{{{Name}}}";
        }

        public Nonterminal CreateNonnullableNonterminal()
        {
            return new Nonterminal(Name, isNullable: false);
        }
    }

    public class ComplexNonterminal : Nonterminal
    {
        public List<Symbol> NameList { get; set; }

        public ComplexNonterminal(List<Symbol> nameList, bool isNullable = true)
            : base(null, isNullable)
        {
            NameList = nameList;
        }

        public override int GetHashCode()
        {
            return NameList.Sum(s => s.Name?.GetHashCode() ?? 0);
        }

        public override bool Equals(object obj)
        {
            return obj is ComplexNonterminal other &&
                   NameList.SequenceEqual(other.NameList);
        }

        public override string ToString()
        {
            return "[" + string.Join("", NameList.Select(s => s.ToString())) + "]";
        }

        public bool StartsWithNonterminal()
        {
            return NameList.Count > 0 && NameList[0] is Nonterminal;
        }

        public bool StartsWithTerminal()
        {
            return NameList.Count > 0 && NameList[0] is Terminal;
        }
    }

    public class Rule
    {
        public List<Symbol> LeftSide { get; set; }
        public List<Symbol> RightSide { get; set; }

        public Rule(List<Symbol> leftSide, List<Symbol> rightSide)
        {
            LeftSide = leftSide;
            RightSide = rightSide;
        }

        public override string ToString()
        {
            string left = string.Join("", LeftSide.Select(s => s.ToString()));
            string right = string.Join("", RightSide.Select(s => s.ToString()));
            return $"{left} -> {right}";
        }

        public bool IsEmpty()
        {
            return RightSide.Count == 1 && RightSide[0] is EmptySymbol;
        }
    }

    public class Grammar
    {
        public Symbol Axiom { get; set; }
        public HashSet<Terminal> Terminals { get; set; } = new();
        public HashSet<Nonterminal> Nonterminals { get; set; } = new();
        public List<Rule> Rules { get; set; } = new();

        public override string ToString()
        {
            string result = $"axiom: {Axiom}\n";
            result += $"terminals: [{string.Join(",", Terminals)}]\n";
            result += $"nonterminals: [{string.Join(",", Nonterminals)}]\n";
            result += "rules:\n";
            foreach (var rule in Rules)
            {
                result += $"\t{rule}\n";
            }
            return result;
        }
        bool SymbolNameStartsWith(Symbol s1, Symbol s2)
        {
            string GetPrefix(string name)
            {
                if (string.IsNullOrEmpty(name)) return "";
                if (name.Length >= 2 && name[1] == '`') return name.Substring(0, 2);
                return name.Substring(0, 1);
            }

            return GetPrefix(s1.Name) == GetPrefix(s2.Name);
        }

        public void RemoveLeftRecursion()
        {
            Dictionary<Nonterminal, HashSet<Nonterminal>> dependencyGraph = new();
            foreach (var nt in Nonterminals)
                dependencyGraph[nt] = new HashSet<Nonterminal>();

            foreach (var rule in Rules)
            {
                if (rule.LeftSide.Count == 1 && rule.LeftSide[0] is Nonterminal lhsNt)
                {
                    if (rule.RightSide.Count > 0 && rule.RightSide[0] is Nonterminal rhsNt)
                    {
                        dependencyGraph[lhsNt].Add(rhsNt);
                    }
                }
            }
            List<Nonterminal> sorted = new();
            HashSet<Nonterminal> visited = new();
            HashSet<Nonterminal> tempMark = new();

            void Visit(Nonterminal n)
            {
                if (tempMark.Contains(n))
                    return;
                    //throw new Exception("SORT ERROR");

                if (!visited.Contains(n))
                {
                    tempMark.Add(n);
                    foreach (var dep in dependencyGraph[n])
                    {
                        Visit(dep);
                    }
                    tempMark.Remove(n);
                    visited.Add(n);
                    sorted.Add(n);
                }
            }

            foreach (var nt in Nonterminals)
            {
                if (!visited.Contains(nt))
                    Visit(nt);
            }


            var nonterminalsList = sorted;

            for (int i = 0; i < nonterminalsList.Count; i++)
            {
                var Ai = nonterminalsList[i];

                // Замена непрямой левой рекурсии
                for (int j = 0; j < i; j++)
                {
                    var Aj = nonterminalsList[j];

                    var newRules = new List<Rule>();

                    //debug
                    var matchingRules = new List<Rule>();

                    foreach (var rule in Rules)
                    {
                        Console.WriteLine($"Проверяем правило: {rule}");

                        bool leftMatches = rule.LeftSide.SequenceEqual(new List<Symbol> { Ai });
                        Console.WriteLine($"\tLeftSide == {Ai}? {leftMatches}");

                        if (rule.RightSide.Count > 0)
                        {
                            var firstRightSymbol = rule.RightSide[0];
                            if (rule.RightSide[0].Name == "")
                                firstRightSymbol = rule.RightSide[1];

                            Console.WriteLine($"\tПервый символ в правой части: {firstRightSymbol}");

                            bool rightMatches = SymbolNameStartsWith(firstRightSymbol, Aj);
                            Console.WriteLine($"\tRightSide[0] == {Aj}? {rightMatches}");

                            if (leftMatches && rightMatches)
                            {
                                Console.WriteLine("\t Правило подходит");
                                matchingRules.Add(rule);
                            }
                        }
                        else
                        {
                            Console.WriteLine("\t Правило имеет пустую правую часть");
                        }
                    }
                    //

                    foreach (var rule in Rules.Where(r => r.LeftSide.SequenceEqual(new List<Symbol> { Ai }) && SymbolNameStartsWith(r.RightSide[0], Aj) == true).ToList())
                    {
                        Rules.Remove(rule);

                        foreach (var r in Rules.Where(r2 => r2.LeftSide.SequenceEqual(new List<Symbol> { Aj })))
                        {

                            var t = r.RightSide[0].Name + rule.RightSide[0].Name.Substring(1);
                            var newRight = new List<Symbol>(r.RightSide);
                            newRight[0].Name = t;
                           // newRight.AddRange(new List<Symbol> { new Symbol(rule.RightSide[0].Name.Substring(1)) } );
                            newRules.Add(new Rule(new List<Symbol> { Ai }, newRight));
                        }
                    }

                    Rules.AddRange(newRules);
                }

                // Поиск прямой левой рекурсии
                var direct = Rules
                    .Where(r => r.LeftSide.SequenceEqual(new List<Symbol> { Ai }))
                    .ToList();

                var alphaRules = new List<List<Symbol>>();
                var betaRules = new List<List<Symbol>>();

                foreach (var rule in direct)
                {
                    if (rule.RightSide.Count > 0 && SymbolNameStartsWith(rule.RightSide[0], Ai))
                    {
                        alphaRules.Add(/*rule.RightSide.Skip(1).ToList()*/
                            new List<Symbol> { new Symbol(rule.RightSide[0].Name.Substring(1)) });
                    }
                    else
                    {
                        betaRules.Add(rule.RightSide);
                    }
                }

                if (alphaRules.Count > 0)
                {
                    Rules.RemoveAll(r => r.LeftSide.SequenceEqual(new List<Symbol> { Ai }));

                    var AiPrime = new Nonterminal(Ai.Name + "'", true);
                    Nonterminals.Add(AiPrime);

                    foreach (var beta in betaRules)
                    {
                        var newRight = new List<Symbol>(beta) { AiPrime };
                        Rules.Add(new Rule(new List<Symbol> { Ai }, newRight));
                    }

                    foreach (var alpha in alphaRules)
                    {
                        var newRight = new List<Symbol>(alpha) { AiPrime };
                        Rules.Add(new Rule(new List<Symbol> { AiPrime }, newRight));
                    }

                    Rules.Add(new Rule(new List<Symbol> { AiPrime }, new List<Symbol> { new Symbol("~") }));
                }
            }
        }
       

    }
}
