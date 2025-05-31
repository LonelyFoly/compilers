using сс2;

namespace TestProject1
{
    [Collection("SequentialTests")]
    public class UnitTest1
    {




        [Fact]
        public void UselessSymbolRemoverTest()
        {
            var input = string.Join('\n', new[]
            {
        "3",
        "S",
        "A",
        "B",
        "2",
        "a b",
        "4",
        "S->a",
        "S->A",
        "A->AB",
        "B->b"
    });

            Console.SetIn(new StringReader(input));

            var grammar = outputer.ReadGrammarFromConsole();
            var newGrammar = UselessSymbolRemover.Remove(grammar);

            var outputWriter = new StringWriter();
            Console.SetOut(outputWriter);

            Console.WriteLine("\nПолучившаяся грамматика: \n");
            outputer.PrintGrammarToConsole(newGrammar);

            var actualOutput = outputWriter.ToString().Replace("\r", "").Trim();

            var expectedOutput = string.Join('\n', new[]
            {
        "Получившаяся грамматика: ",
        "",
        "1",
        "S",
        "1",
        "a",
        "1",
        "S -> a"
    });

            var expected = expectedOutput.Trim();

            Assert.Equal(expected, actualOutput);
        }
        [Fact]
        public void LeftFactorizationHandlerTest()
        {
            var input = string.Join('\n', new[]
            {
        "3",
        "S",
        "E",
        "t",
        "3",
        "i b a",
        "4",
        "S -> i E t S",
        "S -> i E t S e S",
        "S -> a",
        "E -> b"
    });

            Console.SetIn(new StringReader(input));

            Console.WriteLine("Введите грамматику: \n");
            var grammar = outputer.ReadGrammarFromConsole();
            var handler2 = new LeftFactorizationHandler();
            var newGrammar = handler2.LeftFactorize(grammar);
            var outputWriter = new StringWriter();
            Console.SetOut(outputWriter);

            Console.WriteLine("\nПолучившаяся грамматика: \n");
            outputer.PrintGrammarToConsole(newGrammar);

            var actualOutput = outputWriter.ToString().Replace("\r", "").Trim();
            var expectedOutput = string.Join('\n', new[]
            {
        "Получившаяся грамматика: ",
        "",
        "4",
        "S E t S'",
        "3",
        "i b a",
        "5",
        "S -> a",
        "E -> b",
        "S -> i E t S S'",
        "S' -> ~",
        "S' -> e S"
    });

            var expected = expectedOutput.Trim();

            Assert.Equal(expected, actualOutput);
        }

    }

}