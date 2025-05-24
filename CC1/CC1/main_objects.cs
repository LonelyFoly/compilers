using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC1
{
    public class State
    {
        public int Id { get; set; }
        public Dictionary<string, List<State>> Transitions { get; } = new();

        public State(int id)
        {
            Id = id;
        }

        public void AddTransition(string symbol, State target)
        {
            if (!Transitions.ContainsKey(symbol))
                Transitions[symbol] = new List<State>();
            Transitions[symbol].Add(target);
        }
    }

    public class NFA
    {
        public State Start { get; set; }
        public State Accept { get; set; }

        public NFA(State start, State accept)
        {
            Start = start;
            Accept = accept;
        }
    }
    public class DfaState
    {
        public int Id { get; set; }
        public HashSet<State> NfaStates { get; set; } = new();
        public Dictionary<char, DfaState> Transitions { get; set; } = new();
        public bool IsAccepting { get; set; }

        public DfaState(int id, HashSet<State> nfaStates)
        {
            Id = id;
            NfaStates = nfaStates;
        }
    }
    public class MinimalDfa
    {
        public int Id;
        public bool IsAccepting;
        public Dictionary<char, MinimalDfa> Transitions = new();
        public bool Accepts(string input)
        {
            var current = this;
            foreach (char c in input)
            {
                if (!current.Transitions.TryGetValue(c, out var next))
                    return false;
                current = next;
            }
            return current.IsAccepting;
        }

    }




    public class DfaState2
    {
        public int Id { get; set; }
        public HashSet<DfaState2> NfaStates { get; set; } = new();
        public Dictionary<char, DfaState2> Transitions { get; set; } = new();
        public bool IsAccepting { get; set; }

        public DfaState2(int id, HashSet<DfaState2> nfaStates)
        {
            Id = id;
            NfaStates = nfaStates;
        }

        public void AddTransition(char symbol, DfaState2 target)
        {
            Transitions[symbol] = target;
        }

        public bool Accepts(string input)
        {
            var current = this;
            foreach (char c in input)
            {
                if (!current.Transitions.TryGetValue(c, out var next))
                    return false; // если нет перехода
                current = next;
            }

            return current.IsAccepting;
        }

        // Добавим метод для вывода состояний и переходов
        public void PrintTransitions()
        {
            Console.WriteLine($"Состояние {Id} {(IsAccepting ? "(Принимающее)" : "")}");
            foreach (var transition in Transitions)
            {
                Console.WriteLine($"  Переход по символу '{transition.Key}' в состояние {transition.Value.Id}");
            }
        }
    }
    public class SingleStringDFA
    {
        public DfaState2 Start { get; }

        public SingleStringDFA(string word)
        {
            var states = new List<DfaState2>();
            var currentState = new HashSet<DfaState2>();

            // Создаём состояния для каждого символа
            for (int i = 0; i <= word.Length; i++)
            {
                bool accepting = (i == word.Length);
                var newState = new DfaState2(i, currentState);
                states.Add(newState);
                if (i < word.Length)
                {
                    currentState.Add(newState); // добавляем в коллекцию состояний
                }
            }

            // Добавляем переходы
            for (int i = 0; i < word.Length; i++)
            {
                var stateFrom = states[i];
                var stateTo = states[i + 1];
                stateFrom.AddTransition(word[i], stateTo);
            }

            // Стартовое состояние — первое
            Start = states[0];
        }

        public bool Accepts(string input)
        {
            var current = Start;
            foreach (char c in input)
            {
                if (!current.Transitions.TryGetValue(c, out var next))
                    return false;
                current = next;
            }

            return current.IsAccepting;
        }

        // Метод для вывода всего автомата
        public void PrintDFA()
        {
            Console.WriteLine("ДКА для строки:");

            // Сначала создаём коллекцию для всех состояний, которые будут обработаны
            var states = new HashSet<DfaState2> { Start };
            var processedStates = new HashSet<DfaState2>(); // для отслеживания уже обработанных состояний

            var queue = new Queue<DfaState2>();
            queue.Enqueue(Start);

            while (queue.Count > 0)
            {
                var currentState = queue.Dequeue();

                // Если состояние уже обработано, пропускаем его
                if (processedStates.Contains(currentState))
                    continue;

                // Добавляем состояние в список обработанных
                processedStates.Add(currentState);

                // Печатаем переходы для этого состояния
                currentState.PrintTransitions();

                // Добавляем все состояния переходов в очередь, если они ещё не были обработаны
                foreach (var nextState in currentState.Transitions.Values)
                {
                    if (!processedStates.Contains(nextState))
                    {
                        queue.Enqueue(nextState);
                    }
                }
            }
        }

    }
}
