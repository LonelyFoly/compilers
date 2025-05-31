using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace сс2
{
    public class leftHandler
    {
        public Grammar RemoveLeftRecursion(Grammar grammar)
        {
            var nonterminals = grammar.Nonterminals.ToList();
            var rules = new List<Rule>(grammar.Rules);
            var originalRulesPerNonterminal = new Dictionary<Nonterminal, List<Rule>>();

            foreach (var nt in nonterminals)
            {
                originalRulesPerNonterminal[nt] = rules.Where(r => r.LeftSide.Count == 1 && r.LeftSide[0].Name.Equals(nt.Name)).ToList();
            }

            for (int i = 0; i < nonterminals.Count; i++)
            {
                var Ai = nonterminals[i];

                // Обработка косвенной леворекурсии
                for (int j = 0; j < i; j++)
                {
                    var Aj = nonterminals[j];
                    var newRules = new List<Rule>();

                    var toReplace = rules.Where(r =>
                        r.LeftSide.Count == 1 && r.LeftSide[0].Name.Equals(Ai.Name) &&
                        r.RightSide.Count > 0 && r.RightSide[0].Name[0].Equals(Aj.Name))
                        .ToList();

                    if (toReplace.Any())
                    {
                        Console.WriteLine($"Косвенная леворекурсия для {Ai.Name}: найдены правила, начинающиеся с {Aj.Name}:");
                        foreach (var rule in toReplace)
                        {
                            Console.WriteLine($"  Правило для замены: {RuleToString(rule)}");
                        }
                    }

                    foreach (var rule in toReplace)
                    {
                        rules.Remove(rule);

                        foreach (var rj in originalRulesPerNonterminal[Aj])
                        {
                            var newRight = new List<Symbol>(rj.RightSide);

                            // Разбираем первый символ правой части исходного правила
                            // Например, если rule.RightSide[0].Name == "AB", берем все символы, начиная со второго (т.е. "B")
                            if (rule.RightSide.Count > 0)
                            {
                                var firstSymbolName = rule.RightSide[0].Name;
                                if (firstSymbolName.Length > 1)
                                {
                                    // Добавляем символы с 1-го индекса (второй и далее) как отдельные Symbol
                                    var restSymbols = firstSymbolName
                                        .Substring(1)
                                        .Select(c => new Symbol(c.ToString()))
                                        .ToList();

                                    newRight.AddRange(restSymbols);
                                }

                                // Если в правой части есть ещё символы после первого — добавляем их тоже
                                if (rule.RightSide.Count > 1)
                                {
                                    newRight.AddRange(rule.RightSide.Skip(1));
                                }
                            }

                            var newRule = new Rule(new List<Symbol> { Ai }, newRight);
                            newRules.Add(newRule);
                            Console.WriteLine($"  Добавляем новое правило вместо косвенной рекурсии: {RuleToString(newRule)}");
                        }

                    }

                    rules.AddRange(newRules);
                }

                originalRulesPerNonterminal[Ai] = rules.Where(r =>
                    r.LeftSide.Count == 1 && r.LeftSide[0].Name.Equals(Ai.Name))
                    .ToList();

                // Поиск прямой леворекурсии
                var directRec = new List<Rule>();

                foreach (var r in originalRulesPerNonterminal[Ai])
                {
                    if (r.RightSide.Count > 0)
                    {
                        var firstSymbol = r.RightSide[0];

                       
                        bool isLeftRecursive = firstSymbol.Name.Length > 0 && firstSymbol.Name[0] == Ai.Name[0];

                        // Для дебага выводим информацию
                        Console.WriteLine($"Правило: {Ai.Name} -> {string.Join(" ", r.RightSide.Select(s => s.Name))}");
                        Console.WriteLine($"  Первый символ справа: '{firstSymbol.Name}'");
                        Console.WriteLine($"  Сравнение с '{Ai.Name[0]}': {isLeftRecursive}");

                        if (isLeftRecursive)
                        {
                            directRec.Add(r);
                            Console.WriteLine("  --> Это прямая леворекурсия");
                        }
                        else
                        {
                            Console.WriteLine("  --> Не прямая леворекурсия");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Правило: {Ai.Name} -> (пустое)");
                        Console.WriteLine("  --> Правило с пустой правой частью");
                    }
                }

                if (directRec.Any())
                {
                    Console.WriteLine($"Прямая леворекурсия обнаружена в правилах для {Ai.Name}:");
                    foreach (var rule in directRec)
                    {
                        Console.WriteLine($"  Прямое рекурсивное правило: {RuleToString(rule)}");
                    }
                }
                else
                {
                    Console.WriteLine($"Прямой леворекурсии для {Ai.Name} не обнаружено.");
                }

                if (directRec.Any())
                {
                    var alphaList = new List<List<Symbol>>();
                    var betaList = new List<List<Symbol>>();

                    foreach (var rule in originalRulesPerNonterminal[Ai])
                    {
                        rules.Remove(rule);
                    }

                    foreach (var rule in directRec)
                    {
                        var alpha = new List<Symbol>();

                        if (rule.RightSide.Count > 0)
                        {
                            var firstSymbolName = rule.RightSide[0].Name;

                            if (firstSymbolName.Length > 1)
                            {
                                // Разбираем все символы, начиная со второго, как отдельные символы
                                alpha.AddRange(firstSymbolName.Substring(1).Select(c => new Symbol(c.ToString())));
                            }

                            // Добавляем остальные символы после первого элемента списка RightSide, если есть
                            if (rule.RightSide.Count > 1)
                            {
                                alpha.AddRange(rule.RightSide.Skip(1));
                            }
                        }

                        alphaList.Add(alpha);
                    }


                    foreach (var rule in originalRulesPerNonterminal[Ai].Except(directRec))
                    {
                        betaList.Add(rule.RightSide);
                    }

                    Console.WriteLine($"Для {Ai.Name} альфа (левая часть рекурсии):");
                    foreach (var alpha in alphaList)
                    {
                        Console.WriteLine($"  {SymbolsToString(alpha)}");
                    }

                    Console.WriteLine($"Для {Ai.Name} бета (альтернативы без рекурсии):");
                    foreach (var beta in betaList)
                    {
                        Console.WriteLine($"  {SymbolsToString(beta)}");
                    }

                    var newNonterminal = new Nonterminal($"{Ai.Name}'", isNullable: true);
                    grammar.Nonterminals.Add(newNonterminal);

                    foreach (var beta in betaList)
                    {
                        var newBeta = new List<Symbol>(beta) { newNonterminal };
                        var newRule = new Rule(new List<Symbol> { Ai }, newBeta);
                        rules.Add(newRule);
                        Console.WriteLine($"Добавляем новое правило без рекурсии: {RuleToString(newRule)}");
                    }

                    foreach (var alpha in alphaList)
                    {
                        var newAlpha = new List<Symbol>(alpha) { newNonterminal };
                        var newRule = new Rule(new List<Symbol> { newNonterminal }, newAlpha);
                        var newRule2 = new Rule(new List<Symbol> { Ai }, newAlpha);
                        rules.Add(newRule);
                        rules.Add(newRule2);
                        Console.WriteLine($"Добавляем новое рекурсивное правило: {RuleToString(newRule)}");
                    }

                    var emptyRule = new Rule(new List<Symbol> { newNonterminal }, new List<Symbol> { new EmptySymbol() });
                    rules.Add(emptyRule);
                    Console.WriteLine($"Добавляем правило пустого вывода: {RuleToString(emptyRule)}");

                    originalRulesPerNonterminal[newNonterminal] = rules.Where(r =>
                        r.LeftSide.Count == 1 && r.LeftSide[0].Name.Equals(newNonterminal.Name))
                        .ToList();
                }
            }

            return new Grammar
            {
                Axiom = grammar.Axiom,
                Terminals = grammar.Terminals,
                Nonterminals = grammar.Nonterminals,
                Rules = rules
            };
        }

        private string RuleToString(Rule rule)
        {
            var left = SymbolsToString(rule.LeftSide);
            var right = SymbolsToString(rule.RightSide);
            return $"{left} -> {right}";
        }

        private string SymbolsToString(IEnumerable<Symbol> symbols)
        {
            return string.Join(" ", symbols.Select(s =>
            {
                if (s is Nonterminal nt) return nt.Name;
                if (s is Terminal t) return t.Name;
                if (s is EmptySymbol) return "~";
                return s.ToString();
            }));
        }
    }

    public class LeftFactorizationHandler
    {
        public Grammar LeftFactorize(Grammar grammar)
        {
            var rules = new List<Rule>(grammar.Rules);
            var nonterminals = grammar.Nonterminals.ToList();
            var changed = true;

            while (changed)
            {
                changed = false;

                foreach (var nt in nonterminals.ToList())
                {
                    var ntRules = rules.Where(r => r.LeftSide.Count == 1 && r.LeftSide[0].Equals(nt)).ToList();

                    var prefixGroups = ntRules
                        .Where(r => r.RightSide.Count > 0)
                        .GroupBy(r => r.RightSide[0], SymbolEqualityComparer.Instance)
                        .Where(g => g.Count() > 1)
                        .ToList();

                    if (!prefixGroups.Any())
                        continue;

                    foreach (var group in prefixGroups)
                    {
                        var commonPrefix = FindLPrefix(group.Select(r => r.RightSide).ToList());

                        if (commonPrefix.Count == 0)
                            continue;

                        changed = true;

                        var groupRules = group.ToList();
                        var otherRules = ntRules.Except(groupRules).ToList();
                        var newNt = new Nonterminal($"{nt.Name}'");
                        grammar.Nonterminals.Add(newNt);
                        nonterminals.Add(newNt);

                        foreach (var r in groupRules)
                            rules.Remove(r);

                        var newRightSide = new List<Symbol>(commonPrefix) { newNt };
                        rules.Add(new Rule(new List<Symbol> { nt }, newRightSide));

                        foreach (var r in groupRules)
                        {
                            var suffix = r.RightSide.Skip(commonPrefix.Count).ToList();
                            if (suffix.Count == 0)
                                suffix.Add(new EmptySymbol());

                            rules.Add(new Rule(new List<Symbol> { newNt }, suffix));
                        }


                        break; 
                    }

                    if (changed)
                        break;
                }
            }

            return new Grammar
            {
                Axiom = grammar.Axiom,
                Terminals = grammar.Terminals,
                Nonterminals = grammar.Nonterminals,
                Rules = rules
            };
        }

        private List<Symbol> FindLPrefix(List<List<Symbol>> sequences)
        {
            if (sequences == null || sequences.Count == 0)
                return new List<Symbol>();

            var prefix = new List<Symbol>();

            for (int i = 0; ; i++)
            {
                Symbol current = null;

                foreach (var seq in sequences)
                {
                    if (i >= seq.Count)
                        return prefix;

                    if (current == null)
                        current = seq[i];
                    else if (!current.Equals(seq[i]))
                        return prefix;
                }

                prefix.Add(current);
            }
        }
        
        private class SymbolEqualityComparer : IEqualityComparer<Symbol>
        {
            public static SymbolEqualityComparer Instance { get; } = new SymbolEqualityComparer();

            public bool Equals(Symbol x, Symbol y)
            {
                return x.Equals(y);
            }

            public int GetHashCode(Symbol obj)
            {
                return obj.GetHashCode();
            }
        }
    }

    public static class outputer
    {
        public static Grammar ReadGrammarFromConsole()
        {
            int nontermCount = int.Parse(Console.ReadLine()!);
            var nonterminals = new List<Nonterminal>();
            for (int i = 0; i < nontermCount; i++)
                nonterminals.Add(new Nonterminal(Console.ReadLine()!));

            int termCount = int.Parse(Console.ReadLine()!);
            var terminalNames = Console.ReadLine()!.Split(' ');
            var terminals = terminalNames.Select(name => new Terminal(name)).ToList();

            int ruleCount = int.Parse(Console.ReadLine()!);
            var rules = new List<Rule>();
            for (int i = 0; i < ruleCount; i++)
            {
                var line = Console.ReadLine()!;
                var parts = line.Split("->");
                var left = parts[0].Trim();
                var rightParts = parts[1].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var leftSymbol = new List<Symbol> { new Nonterminal(left) };
                var rightSymbols = new List<Symbol>();
                foreach (var symbol in rightParts)
                {
                    if (symbol == "~")
                        continue;
                    if (nonterminals.Any(nt => nt.Name == symbol))
                        rightSymbols.Add(new Nonterminal(symbol));
                    else
                        rightSymbols.Add(new Terminal(symbol));
                }

                if (rightSymbols.Count == 0)
                    rightSymbols.Add(new EmptySymbol());

                rules.Add(new Rule(leftSymbol, rightSymbols));
            }

            return new Grammar
            {
                Axiom = nonterminals.First(),
                Nonterminals = new HashSet<Nonterminal>(nonterminals),
                Terminals = new HashSet<Terminal>(terminals),
                Rules = rules
            };
        }

        public static void PrintGrammarToConsole(Grammar grammar)
        {
            Console.WriteLine(grammar.Nonterminals.Count);
            Console.WriteLine(string.Join(" ", grammar.Nonterminals.Select(n => n.Name)));
            Console.WriteLine(grammar.Terminals.Count);
            Console.WriteLine(string.Join(" ", grammar.Terminals.Select(t => t.Name)));

            Console.WriteLine(grammar.Rules.Count);
            foreach (var rule in grammar.Rules)
            {
                var left = rule.LeftSide[0].ToString();
                var right = rule.RightSide.Any()
                    ? string.Join(" ", rule.RightSide.Select(s => s is EmptySymbol ? "~" : s.ToString()))
                    : "~";

                Console.WriteLine($"{left} -> {right}");
            }
        }
    }
    public class NonEmptyLanguageChecker
    {
        private readonly Grammar grammar;

        public NonEmptyLanguageChecker(Grammar grammar)
        {
            this.grammar = grammar;
        }

        public HashSet<Nonterminal> GetGeneratingNonterminals()
        {
            var generating = new HashSet<Nonterminal>();
            bool changed;

            do
            {
                changed = false;
                foreach (var rule in grammar.Rules)
                {
                    if (rule.LeftSide.Count != 1 || rule.LeftSide[0] is not Nonterminal A)
                        continue;

                    if (generating.Contains(A))
                        continue;

                    bool rightAllGenerates = rule.RightSide.All(sym =>
                        sym is Terminal ||
                        sym is EmptySymbol ||
                        (sym is Nonterminal nt && generating.Contains(nt)));

                    if (rightAllGenerates)
                    {
                        generating.Add(A);
                        changed = true;
                    }
                }
            } while (changed);

            return generating;
        }

        public bool LanguageIsNonEmpty()
        {
            var generating = GetGeneratingNonterminals();
            return generating.Contains(grammar.Axiom);
        }
    }
    /*
    2.7:
    Вход: кс грамматика G={N,Σ, P,S}
    Выход: "да" если L(G) != пустое множество, "нет", в противном случае
    Метод: Строим множества N0,N1,... рекурсивно:
    1) положить N0=пустое множество и i-1
    2) Положить Ni = {A | A-> a ∈ P и a ∈ (Ni-1 U Σ)*} Σ Ni-1
    3) Если Ni != Ni-1, положить i=i+1 и перейти к шагу (2)
    4) Если S∈Ne, выдать выход "да", в противном случае "нет"
    Так как Ne ∈ N, то алгоритм должен остановиться самое большое после n+1 повторений шага (2), еесли N содержит n нетерминалов.


*/

    public static class GrammarEmptyLanguageChecker
    {


        public static bool IsLanguageNonEmpty(Grammar grammar)
        {
            var N = grammar.Nonterminals;
            var Σ = grammar.Terminals;
            var P = grammar.Rules;
            var S = grammar.Axiom;

            var NiMinus1 = new HashSet<Nonterminal>();
            var Ni = new HashSet<Nonterminal>();

            int n = N.Count;
            int i = 1;

            while (true)
            {
                Ni.Clear();

                foreach (var A in N)
                {
                    foreach (var rule in P.Where(r => r.LeftSide.Count == 1 && r.LeftSide[0].Equals(A)))
                    {
                        bool allSymbolsInNiMinus1OrSigma = true;
                        foreach (var symbol in rule.RightSide)
                        {
                            if (symbol is Terminal)
                                continue;
                            if (symbol is Nonterminal nt && NiMinus1.Contains(nt))
                                continue;
                            allSymbolsInNiMinus1OrSigma = false;
                            break;
                        }

                        if (allSymbolsInNiMinus1OrSigma)
                        {
                            Ni.Add(A);
                            break;
                        }
                    }
                }

                if (Ni.SetEquals(NiMinus1))
                    break;

                NiMinus1 = new HashSet<Nonterminal>(Ni);

                i++;
                if (i > n + 1)
                    break;
            }

            // Шаг 4: Проверяем принадлежит ли аксиома Ne
            return NiMinus1.Contains((Nonterminal)S);
        }
    }
    

    /*
    2.8: устранение недостижимых символов
    Вход: кс грамматика G={N,Σ, P,S}
    Выход: кс грамматика G`={N`,Σ`, P`,S} у которой 
    1. L(G`) = L(G)
    2. для всех X∈N` U Σ` существуют такие цепочки a и b из (N` U Σ`) что S =>* aXb

    Метод: Строим множества N0,N1,... рекурсивно:
    1) положить V0={S} и i=1
    2) Положить Vi = {X | в P есть A->aXb и A∈Vi-1} U Vi-1
    3) Если Vi!=Vi-1, положить в i=i+1 и перейти к шагу 2). В противном случае пусть
    N`= Vi ∩ N,
    Σ` = Vi ∩ Σ
    P` состоит из правил множества P, содержащих только символы из Vi, G`=(N`,Σ`,P`,S)


*/

    public class UnreachableSymbolRemover
    {
        private Grammar grammar;

        public UnreachableSymbolRemover(Grammar grammar)
        {
            this.grammar = grammar;
        }

        public Grammar RemoveUnreachableSymbols()
        {
            var Vprev = new HashSet<Symbol> { grammar.Axiom };
            var Vnext = new HashSet<Symbol>(Vprev);

            while (true)
            {
                foreach (var rule in grammar.Rules)
                {
                    if (rule.LeftSide.Count != 1) continue;
                    var A = rule.LeftSide[0];
                    if (!Vprev.Contains(A)) continue;

                    foreach (var sym in rule.RightSide)
                    {
                        Vnext.Add(sym);
                    }
                }

                if (Vnext.SetEquals(Vprev)) break;

                Vprev = new HashSet<Symbol>(Vnext);
            }

            // Фильтрация
            var newNonterminals = grammar.Nonterminals.Where(nt => Vnext.Contains(nt)).ToHashSet();
            var newTerminals = grammar.Terminals.Where(t => Vnext.Contains(t)).ToHashSet();
            var newRules = grammar.Rules
                .Where(rule =>
                    rule.LeftSide.All(s => Vnext.Contains(s)) &&
                    rule.RightSide.All(s => Vnext.Contains(s)))
                .ToList();

            return new Grammar
            {
                Nonterminals = newNonterminals,
                Terminals = newTerminals,
                Rules = newRules,
                Axiom = grammar.Axiom
            };
        }
    }


    /*
    2.9: устранение бесполезных символов
    Вход: кс грамматика G={N,Σ, P,S} у которой L(G) != пустое множество
    Выход: кс грамматика G`={N`,Σ`, P`,S} у которой  L(G`) = L(G) и в N` U Σ` нет бесполезных символов
    1. L(G`) = L(G)
    2. для всех X∈N` U Σ` существуют такие цепочки a и b из (N` U Σ`) что S =>* aXb

    Метод:
    1) Применив к G алгоритм 2.7 получить Ne. Положить G1=(N∩Ne, Σ, P1,S), где P1 состоит из правил множества P, содержащих только символы из Ne U Σ
    2) Применив к G1 алгоритм 2.8, получить G`=(N`,Σ`, P`,S).


*/
    /*public static class UselessSymbolRemover
    {



        public static Grammar Remove(Grammar grammar)
        {

            // Шаг 1: получить Ne — множество порождающих нетерминалов
            var checker = new NonEmptyLanguageChecker(grammar);
            var generating = checker.GetGeneratingNonterminals();

            // Фильтрация правил: оставляем только те, где все символы из generating ∪ terminals
            var filteredRules = grammar.Rules
                .Where(rule =>
                    rule.LeftSide.All(s => s is Nonterminal nt && generating.Contains(nt)) &&
                    rule.RightSide.All(s =>
                        s is Terminal ||
                        s is EmptySymbol ||
                        (s is Nonterminal nt && generating.Contains(nt))))
                .ToList();

            // Создать промежуточную грамматику G1
            var G1 = new Grammar
            {
                Nonterminals = new HashSet<Nonterminal>(generating),
                Terminals = grammar.Terminals,
                Rules = filteredRules,
                Axiom = grammar.Axiom
            };

            // Шаг 2: применить алгоритм 2.8
            var remover = new UnreachableSymbolRemover(G1);
            return remover.RemoveUnreachableSymbols();
        }
    }*/
    public static class UselessSymbolRemover
    {
        public static bool debug = false;
        public static Grammar Remove(Grammar grammar)
        {
            var generating = GetGeneratingNonterminals(grammar);
            var validSymbols = new HashSet<string>(
    grammar.Terminals.Select(t => t.Name)
    .Concat(generating.Select(g => g.Name))
);
            var filteredRules = grammar.Rules
                .Where(rule =>
                    rule.LeftSide.All(s => s is Nonterminal nt && generating.Contains(nt)) &&
                    rule.RightSide.All(s =>
          s.Name.All(ch =>
            validSymbols.Contains(ch.ToString()))
        )
    )
                .ToList();

            var G1 = new Grammar
            {
                Nonterminals = new HashSet<Nonterminal>(generating),
                Terminals = grammar.Terminals,
                Rules = filteredRules,
                Axiom = grammar.Axiom
            };

            var remover = new UnreachableSymbolRemover(G1);
            return remover.RemoveUnreachableSymbols();
        }

        private static HashSet<Nonterminal> GetGeneratingNonterminals(Grammar grammar)
        {
            var N = grammar.Nonterminals;
            var sigma = grammar.Terminals;
            var P = grammar.Rules;

            var NiMinus1 = new HashSet<Nonterminal>();
            var Ni = new HashSet<Nonterminal>();

            int n = N.Count;
            int i = 1;

            while (true)
            {
                Ni = new HashSet<Nonterminal>(NiMinus1);

                foreach (var A in N)
                {
                    foreach (var rule in P.Where(r => r.LeftSide.Count == 1 && r.LeftSide[0].Equals(A)))
                    {
                        bool allDone = true;
                        foreach (var symbol in rule.RightSide)
                        {
                            string symbolStr = symbol.ToString();

                            foreach (char ch in symbolStr)
                            {
                                bool isTerminal = sigma.Any(t => t.Name == ch.ToString());

                                if (isTerminal || symbol is EmptySymbol)
                                    continue;
                                var nt = new Nonterminal(ch.ToString());

                                if (NiMinus1.Contains(nt))
                                    continue;
                                if (debug)
                                    Console.WriteLine($"Правило {A} -> {string.Join("", rule.RightSide.Select(s => s.ToString()))} не подходит, т.к. символ {ch} не в NiMinus1 и не терминал");
                                allDone = false;
                                break;
                            }

                            if (!allDone)
                                break;
                        }

                        if (allDone)
                        {
                            Ni.Add(A);
                            if (debug)
                                Console.WriteLine($"Добавлен порождающий нетерминал {A}");
                            break;
                        }
                    }
                }
                if (debug)
                    Console.WriteLine($"Итерация {i}: порождающие нетерминалы = {{ {string.Join(", ", Ni.Select(nt => nt.Name))} }}");
                if (Ni.SetEquals(NiMinus1))
                    break;

                NiMinus1 = new HashSet<Nonterminal>(Ni);

                i++;
                if (i > n + 1)
                    break;
            }


            return NiMinus1;
        }
    }

}
