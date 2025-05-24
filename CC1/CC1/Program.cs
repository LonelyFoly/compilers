namespace CC1
{
    internal class Program
    {
        static void Main()
        {
            //1
            Console.Write("Введите регулярное выражение: ");
            string input = Console.ReadLine();

            var builder = new ThompsonBuilder();
            var nfa = builder.Build(input);
            builder.PrintNFA(nfa);

            //2
            var converter = new NfaToDfaConverter();
            DfaState dfaStart = converter.Convert(nfa);
            builder.PrintDFA(dfaStart);

            //3
            /*var minimizer = new DfaMinimizer();
            var minimized = minimizer.Minimize(dfaStart);

            builder.PrintMinimalDfa(minimized);*/


            //4
            var minimizer = new DfaMinimizer();
            var (minStates, stateMap) = minimizer.Minimize(dfaStart);
            var startMin = stateMap[dfaStart]; // получаем стартовое состояние минимального автомата

            builder.PrintMinimalDfa(minStates);


            while (true)
            {
                Console.Write("Введите цепочку символов: ");
                string testInput = Console.ReadLine();

                bool accepted = startMin.Accepts(testInput);

                Console.WriteLine(accepted
                    ? "\tПринято"
                    : "\tНЕ принято");


            }


        }
    }
}
