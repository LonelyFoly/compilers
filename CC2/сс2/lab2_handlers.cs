using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace сс2
{
    public class SymbolL2
{
    public string Name { get; }

    public SymbolL2(string representation)
    {
        Name = representation;
    }

    public override string ToString() => Name;

    public override bool Equals(object obj)
    {
        return obj is SymbolL2 SymbolL2 && Name == SymbolL2.Name;
    }

    public override int GetHashCode() => Name.GetHashCode();

    public static SymbolL2 operator +(SymbolL2 a, SymbolL2 b)
    {
        return new SymbolL2(a.Name + b.Name);
    }

    public SymbolL2 AddApostrophe()
    {
        return new SymbolL2(Name + "'");
    }
    }

public class GrammarL2
    {
        public HashSet<SymbolL2> Nonterminals { get; private set; }
        public HashSet<SymbolL2> Terminals { get; private set; }
        public Dictionary<SymbolL2, HashSet<List<SymbolL2>>> Rules { get; private set; }
        public SymbolL2 StartSymbol { get; private set; }

        public GrammarL2()
        {
            int numberOfNonterminals = int.Parse(Console.ReadLine()!);
            Nonterminals = new HashSet<SymbolL2>(
                Console.ReadLine()!.Trim().Split(' ').Select(s => new SymbolL2(s))
            );
            int numberOfTerminals = int.Parse(Console.ReadLine()!);

            Terminals = new HashSet<SymbolL2>(
                Console.ReadLine()!.Trim().Split(' ').Select(s => new SymbolL2(s))
            );

            int numberOfRules = int.Parse(Console.ReadLine()!);

            Rules = Nonterminals.ToDictionary(nt => nt, nt => new HashSet<List<SymbolL2>>());

            for (int i = 0; i < numberOfRules; i++)
            {
                var line = Console.ReadLine()!.Trim();
                var parts = line.Split("->", StringSplitOptions.RemoveEmptyEntries);
                var left = new SymbolL2(parts[0].Trim());
                var right = parts[1].Trim().Split(' ').Select(s => new SymbolL2(s)).ToList();

                if (!Rules.ContainsKey(left))
                    Rules[left] = new HashSet<List<SymbolL2>>();

                Rules[left].Add(right);
            }


        }

        public void PrintRules()
        {
            foreach (var rules in Rules)
            {
                var left = rules.Key;
                foreach (var r in rules.Value)
                {
                    var right = r.Count == 0
                        ? "~"
                        : string.Join(" ", r.Select(sym => sym.ToString()));
                    Console.WriteLine($"{left} -> {right}");
                }
            }
        }

        public void RemoveLeftRecursion()
        {
            var nonterminals = Nonterminals.ToList();

            for (int i = 0; i < nonterminals.Count; i++)
            {
                while (true)
                {
                    bool changed = false;

                    for (int j = 0; j < i; j++)
                    {
                        var ai = nonterminals[i];
                        var aj = nonterminals[j];

                        var gen_rules = Rules[ai].ToList();
                        foreach (var selected_rule in gen_rules)
                        {
                            if (selected_rule.Count > 0 && selected_rule[0].Equals(aj))
                            {
                                changed = true;
                                var beta = selected_rule.Skip(1).ToList();
                                Rules[ai].Remove(selected_rule);

                                foreach (var alpha in Rules[aj])
                                {
                                    var newRule = new List<SymbolL2>();
                                    foreach (var sym in alpha)
                                    {
                                        string repr = sym.Name;
                                        for (int t = 0; t < repr.Length;)
                                        {
                                            // Если текущий символ + апостроф = объединить
                                            if (t + 1 < repr.Length && repr[t + 1] == '\'')
                                            {
                                                newRule.Add(new SymbolL2(repr.Substring(t, 2)));
                                                t += 2;
                                            }
                                            else
                                            {
                                                newRule.Add(new SymbolL2(repr[t].ToString())); 
                                                t++;
                                            }
                                        }
                                    }
                                    newRule.AddRange(beta);
                                    Rules[ai].Add(newRule);
                                }
                            }
                        }
                    }

                    if (!changed)
                        break;
                }

                /*var directRecursives = Rules[nonterminals[i]]
                    .Where(prod => prod.Count > 0 && prod[0].Equals(nonterminals[i]))
                    .ToList();*/
                var ai_debug = nonterminals[i];
                Console.WriteLine($"\nПроверка прямой левой рекурсии для: {ai_debug}");

                var directRecursives = Rules[ai_debug]
                    .Where(r =>
                    {
                        bool isDirect = r.Count > 0 && r[0].Equals(ai_debug);
                        Console.WriteLine($"  Правило: {ai_debug} -> {string.Join(" ", r.Select(s => s.ToString()))} " +
                                          $"| {(isDirect ? "Прямая рекурсия" : "Нет рекурсии")}");
                        return isDirect;
                    })
                    .ToList();

                Console.WriteLine($"Найдено {directRecursives.Count} правил с прямой левой рекурсией для {ai_debug}");


                if (directRecursives.Count == 0)
                    continue;

                var betas = new List<List<SymbolL2>>();
                var alphas = new List<List<SymbolL2>>();

                foreach (var rule in Rules[nonterminals[i]].ToList())
                {
                    Rules[nonterminals[i]].Remove(rule);
                    if (rule.Count > 0 && rule[0].Equals(nonterminals[i]))
                    {
                        alphas.Add(rule.Skip(1).ToList());
                    }
                    else
                    {
                        if (rule.Count == 1 && rule[0].Name == "~")
                            betas.Add(new List<SymbolL2>());
                        else
                            betas.Add(rule);
                    }
                }

                var newNonterminal = nonterminals[i].AddApostrophe();
                Nonterminals.Add(newNonterminal);
                Rules[newNonterminal] = new HashSet<List<SymbolL2>>();

                foreach (var beta in betas)
                {
                    var newRule = new List<SymbolL2>(beta) { newNonterminal };
                    Rules[nonterminals[i]].Add(newRule);
                }

                foreach (var alpha in alphas)
                {
                    var newRule = new List<SymbolL2>(alpha) { newNonterminal };
                    Rules[newNonterminal].Add(newRule);
                }

                Rules[newNonterminal].Add(new List<SymbolL2> { new SymbolL2("~") });
            }
        }
    }


}
