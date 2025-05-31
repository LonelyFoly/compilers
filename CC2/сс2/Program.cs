using сс2;

/*
 * 


 2.22
G=({S,A,B}, {a,b}, P,S)
P:
S->a|A
A->AB
B->b
бесполезные символы
3
S
A
B
2
a b
4
S->a
S->A
A->AB
B->b



3
S
A
B
2
a b
7
S->A
S->B
A->aB
A->bS
A->b
B->AB
B->Ba
B->AS
B->b



3
E
T
F
5
+ * ( ) a
6
E -> E + T
E -> T
T -> T * F
T -> F
F -> a
F -> ( E )





3
S
A
B
3
a b c
6
S -> Sa
S -> A
A -> Ab
A -> B
B -> b
B -> S


2
S
A
2
a c
4
S -> AS
S -> A
A -> Sa
A -> a



факторизация
3
S
E
t
3
i b a
4
S -> i E t S
S -> i E t S e S
S -> a
E -> b







3
E T F
5
+ * ( ) a
6
E -> E + T
E -> T
T -> T * F
T -> F
F -> a
F -> ( E )


2
A S
1
a
3
S -> A
A -> Sa
A -> a
*/
//левая рекурсия
var grammar = new GrammarL2();

grammar.RemoveLeftRecursion();

Console.WriteLine("\nГрамматика после удаления левой рекурсии:");

grammar.PrintRules();


/*бесполезные символы
3
S
A
B
2
a b
4
S->a
S->A
A->AB
B->b



3
S
A
B
2
a b
7
S->A
S->B
A->aB
A->bS
A->b
B->AB
B->Ba
B->AS
B->b



4
S
A
B
C
2
a b
8
S->A
S->B
A->aB
A->bS
A->b
B->AB
B->Ba
C->C

*/

//удаление бесполезных символов
var grammar2 = outputer.ReadGrammarFromConsole();
var newGrammar = UselessSymbolRemover.Remove(grammar2);
UselessSymbolRemover.debug = false;
Console.WriteLine("\nПолучившаяся грамматика: \n");
outputer.PrintGrammarToConsole(newGrammar);






