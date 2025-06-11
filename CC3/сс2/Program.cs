using сс2;


//lab 3
Console.WriteLine("{ xa = 5; y = (xa + 2) * 3; z = xa < y}");
var code = "{xa = 5; y = (xa + 2) * 3; z = xa < y}";
var lexer = new Lexer(code);
var parser = new Parser(lexer);
var tree = parser.Program();
tree.Print();


Console.WriteLine("\n\n\n\nПостфиксная запись:");

string rpn = parser.ToRPN(tree);
Console.WriteLine(rpn);

/*lexer = new Lexer(code);
parser = new Parser(lexer);
string rpn = parser.ExpressionToRPN();
Console.WriteLine("RPN: " + rpn);*/
/*var lexer = new Lexer("xa = 5");
var parser = new Parser(lexer);
string rpn = parser.ExpressionToRPN();
Console.WriteLine("RPN: " + rpn);*/





