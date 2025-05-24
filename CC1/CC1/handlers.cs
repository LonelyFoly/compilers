using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC1
{
    public class ThompsonBuilder
    {
        private int _stateId = 0;

        public NFA Build(string regex)
        {
            string expanded = AddConcat(regex);
            string postfix = ToPostfix(expanded);
            return PostfixToNFA(postfix);
        }

        private string AddConcat(string regex)
        {
            string result = "";
            for (int i = 0; i < regex.Length; i++)
            {
                char c1 = regex[i];
                result += c1;

                if (i + 1 < regex.Length)
                {
                    char c2 = regex[i + 1];
                    if ((char.IsLetterOrDigit(c1) || c1 == ')' || c1 == '*') &&
                        (char.IsLetterOrDigit(c2) || c2 == '('))
                    {
                        result += '.';
                    }
                }
            }
            return result;
        }

        private int Precedence(char op)
        {
            return op switch
            {
                '*' => 3,
                '.' => 2,
                '|' => 1,
                _ => 0
            };
        }

        private string ToPostfix(string regex)
        {
            string output = "";
            Stack<char> stack = new();

            foreach (char token in regex)
            {
                if (char.IsLetterOrDigit(token))
                {
                    output += token;
                }
                else if (token == '(')
                {
                    stack.Push(token);
                }
                else if (token == ')')
                {
                    while (stack.Peek() != '(')
                        output += stack.Pop();
                    stack.Pop(); // Удалить '('
                }
                else
                {
                    while (stack.Count > 0 && Precedence(stack.Peek()) >= Precedence(token))
                        output += stack.Pop();
                    stack.Push(token);
                }
            }

            while (stack.Count > 0)
                output += stack.Pop();

            return output;
        }

        private NFA PostfixToNFA(string postfix)
        {
            Stack<NFA> stack = new();

            foreach (char token in postfix)
            {
                if (char.IsLetterOrDigit(token))
                {
                    State start = new(_stateId++);
                    State accept = new(_stateId++);
                    start.AddTransition(token.ToString(), accept);
                    stack.Push(new NFA(start, accept));
                }
                else if (token == '*')
                {
                    var nfa = stack.Pop();
                    State start = new(_stateId++);
                    State accept = new(_stateId++);
                    start.AddTransition("~", nfa.Start);
                    start.AddTransition("~", accept);
                    nfa.Accept.AddTransition("~", nfa.Start);
                    nfa.Accept.AddTransition("~", accept);
                    stack.Push(new NFA(start, accept));
                }
                else if (token == '.')
                {
                    var nfa2 = stack.Pop();
                    var nfa1 = stack.Pop();
                    nfa1.Accept.AddTransition("~", nfa2.Start);
                    stack.Push(new NFA(nfa1.Start, nfa2.Accept));
                }
                else if (token == '|')
                {
                    var nfa2 = stack.Pop();
                    var nfa1 = stack.Pop();
                    State start = new(_stateId++);
                    State accept = new(_stateId++);
                    start.AddTransition("~", nfa1.Start);
                    start.AddTransition("~", nfa2.Start);
                    nfa1.Accept.AddTransition("~", accept);
                    nfa2.Accept.AddTransition("~", accept);
                    stack.Push(new NFA(start, accept));
                }
                else if (token == '+')
                {
                    var nfa = stack.Pop();
                    State start = new(_stateId++);
                    State accept = new(_stateId++);

                    
                    start.AddTransition("~", nfa.Start);
                    nfa.Accept.AddTransition("~", nfa.Start);
                    nfa.Accept.AddTransition("~", accept);

                    stack.Push(new NFA(start, accept));
                }

            }

            return stack.Pop();
        }

        public void PrintNFA(NFA nfa)
        {
            Console.WriteLine("\n--- Построенный НКА ---");
            Console.WriteLine("Начальный: " + nfa.Start.Id);
            Console.WriteLine("Конечный: " + nfa.Accept.Id);
            HashSet<int> visited = new();
            Queue<State> queue = new();
            queue.Enqueue(nfa.Start);

            while (queue.Count > 0)
            {
                State state = queue.Dequeue();
                if (visited.Contains(state.Id)) continue;
                visited.Add(state.Id);

                foreach (var trans in state.Transitions)
                {
                    foreach (var target in trans.Value)
                    {
                        Console.WriteLine($"({state.Id}) -[{(trans.Key == null ? "~" : trans.Key)}]-> ({target.Id})");
                        queue.Enqueue(target);
                    }
                }
            }
        }
        public void PrintDFA(DfaState start)
        {
            var visited = new HashSet<int>();
            var queue = new Queue<DfaState>();

            visited.Add(start.Id);
            queue.Enqueue(start);

            Console.WriteLine("\n--- Построенный ДКА ---");
            Console.WriteLine("Start state: " + start.Id);
            while (queue.Count > 0)
            {
                var state = queue.Dequeue();
                foreach (var kv in state.Transitions)
                {
                    Console.WriteLine($"({state.Id}) -[{kv.Key}]-> ({kv.Value.Id})");
                    if (!visited.Contains(kv.Value.Id))
                    {
                        visited.Add(kv.Value.Id);
                        queue.Enqueue(kv.Value);
                    }
                }

                if (state.IsAccepting)
                    Console.WriteLine($"(State {state.Id}) — accepting");
            }
        }
        public void PrintMinimalDfa(List<MinimalDfa> states)
        {
            Console.WriteLine("\n--- Минимальный ДКА ---");
            foreach (var state in states)
            {
                foreach (var kv in state.Transitions)
                {
                    Console.WriteLine($"({state.Id}) -[{kv.Key}]-> ({kv.Value.Id})");
                }

                if (state.IsAccepting)
                    Console.WriteLine($"(State {state.Id}) — accepting");
            }
        }



    }

    public class NfaToDfaConverter
    {
        private int _stateId = 0;

        public DfaState Convert(NFA nfa)
        {
            var dfaStates = new Dictionary<string, DfaState>();
            var queue = new Queue<DfaState>();

            HashSet<State> startClosure = EpsilonClosure(new HashSet<State> { nfa.Start });
            string startKey = StateSetToString(startClosure);

            var startDfaState = new DfaState(_stateId++, startClosure)
            {
                IsAccepting = startClosure.Contains(nfa.Accept)
            };

            dfaStates[startKey] = startDfaState;
            queue.Enqueue(startDfaState);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var transitions = GetTransitions(current.NfaStates);

                foreach (var symbol in transitions.Keys)
                {
                    var targetNfaStates = transitions[symbol];
                    var closure = EpsilonClosure(targetNfaStates);
                    var key = StateSetToString(closure);

                    if (!dfaStates.ContainsKey(key))
                    {
                        var newDfaState = new DfaState(_stateId++, closure)
                        {
                            IsAccepting = closure.Contains(nfa.Accept)
                        };

                        dfaStates[key] = newDfaState;
                        queue.Enqueue(newDfaState);
                    }

                    current.Transitions[symbol] = dfaStates[key];
                }
            }

            return startDfaState;
        }

        private HashSet<State> EpsilonClosure(HashSet<State> states)
        {
            var stack = new Stack<State>(states);
            var result = new HashSet<State>(states);

            while (stack.Count > 0)
            {
                var state = stack.Pop();
                if (state.Transitions.TryGetValue("~", out var epsilonTargets))
                {
                    foreach (var target in epsilonTargets)
                    {
                        if (result.Add(target))
                            stack.Push(target);
                    }
                }
            }

            return result;
        }

        private Dictionary<char, HashSet<State>> GetTransitions(HashSet<State> states)
        {
            var transitions = new Dictionary<char, HashSet<State>>();

            foreach (var state in states)
            {
                foreach (var kv in state.Transitions)
                {
                    if (kv.Key == "~") continue; // Пропускаем ~

                    char symbol = kv.Key[0];
                    if (!transitions.ContainsKey(symbol))
                        transitions[symbol] = new HashSet<State>();

                    foreach (var target in kv.Value)
                        transitions[symbol].Add(target);
                }
            }

            return transitions;
        }

        private string StateSetToString(HashSet<State> states)
        {
            var sorted = states.Select(s => s.Id).OrderBy(id => id);
            return string.Join(",", sorted);
        }
    }
    public class DfaMinimizer
    {



        public (List<MinimalDfa> states, Dictionary<DfaState, MinimalDfa> stateMap) Minimize(DfaState dfaStart)
        {
            var allStates = GetAllStates(dfaStart);
            int n = allStates.Count;

            var idMap = allStates.Select((s, i) => (s, i)).ToDictionary(p => p.s, p => p.i);
            var reverseIdMap = idMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
            bool[,] distinguishable = new bool[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    bool isI = allStates[i].IsAccepting;
                    bool isJ = allStates[j].IsAccepting;
                    if (isI != isJ)
                        distinguishable[i, j] = distinguishable[j, i] = true;
                }
            }

            bool changed;
            do
            {
                changed = false;
                for (int i = 0; i < n; i++)
                {
                    for (int j = i + 1; j < n; j++)
                    {
                        if (distinguishable[i, j]) continue;

                        foreach (char symbol in GetAllSymbols(allStates[i], allStates[j]))
                        {
                            if (!allStates[i].Transitions.TryGetValue(symbol, out var toI)) continue;
                            if (!allStates[j].Transitions.TryGetValue(symbol, out var toJ)) continue;

                            int toIId = idMap[toI];
                            int toJId = idMap[toJ];

                            if (toIId == toJId) continue;

                            if (distinguishable[toIId, toJId] || distinguishable[toJId, toIId])
                            {
                                distinguishable[i, j] = distinguishable[j, i] = true;
                                changed = true;
                                break;
                            }
                        }
                    }
                }
            } while (changed);

            int[] group = new int[n];
            for (int i = 0; i < n; i++) group[i] = i;

            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    if (!distinguishable[i, j])
                        group[j] = group[i];

            var groupMap = new Dictionary<int, MinimalDfa>();
            for (int i = 0; i < n; i++)
            {
                int g = group[i];
                if (!groupMap.ContainsKey(g))
                {
                    groupMap[g] = new MinimalDfa
                    {
                        Id = g,
                        IsAccepting = allStates[i].IsAccepting
                    };
                }
            }

            for (int i = 0; i < n; i++)
            {
                var from = allStates[i];
                var fromGroup = group[i];
                var minFrom = groupMap[fromGroup];

                foreach (var kv in from.Transitions)
                {
                    var to = kv.Value;
                    int toGroup = group[idMap[to]];
                    minFrom.Transitions[kv.Key] = groupMap[toGroup];
                }
            }

            var stateMap = new Dictionary<DfaState, MinimalDfa>();
            for (int i = 0; i < n; i++)
            {
                stateMap[allStates[i]] = groupMap[group[i]];
            }

            return (groupMap.Values.ToList(), stateMap);

        }

        private List<DfaState> GetAllStates(DfaState start)
        {
            var result = new List<DfaState>();
            var visited = new HashSet<DfaState>();
            var queue = new Queue<DfaState>();

            visited.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var state = queue.Dequeue();
                result.Add(state);

                foreach (var next in state.Transitions.Values)
                {
                    if (visited.Add(next))
                        queue.Enqueue(next);
                }
            }

            return result;
        }

        private HashSet<char> GetAllSymbols(DfaState a, DfaState b)
        {
            var symbols = new HashSet<char>();
            foreach (var key in a.Transitions.Keys)
                symbols.Add(key);
            foreach (var key in b.Transitions.Keys)
                symbols.Add(key);
            return symbols;
        }
    }



}
