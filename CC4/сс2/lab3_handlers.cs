using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace сс2
{
    public class Lexer
    {
        private string text;
        private int pos;
        private char Current => pos < text.Length ? text[pos] : '\0';

        public Lexer(string input)
        {
            text = input;
            pos = 0;
        }

        public Token NextToken()
        {
            while (char.IsWhiteSpace(Current)) pos++;

            if (char.IsLetter(Current))
            {
                string ident = ReadWhile(char.IsLetterOrDigit);
                return new Token(TokenType.Identifier, ident);
            }

            if (char.IsDigit(Current))
            {
                string num = ReadWhile(char.IsDigit);
                return new Token(TokenType.Number, num);
            }

            return Current switch
            {
                '+' => CreateAndAdvance(TokenType.Plus),
                '-' => CreateAndAdvance(TokenType.Minus),
                '*' => CreateAndAdvance(TokenType.Mul),
                '/' => CreateAndAdvance(TokenType.Div),
                '(' => CreateAndAdvance(TokenType.LParen),
                ')' => CreateAndAdvance(TokenType.RParen),
                '{' => CreateAndAdvance(TokenType.LBrace),
                '}' => CreateAndAdvance(TokenType.RBrace),
                ';' => CreateAndAdvance(TokenType.Semicolon),
                '=' => MatchNext('=') ? new Token(TokenType.Equal, "==") : CreateAndAdvance(TokenType.Assign),
                '<' => MatchNext('=') ? new Token(TokenType.LessEqual, "<=") :
                       MatchNext('>') ? new Token(TokenType.NotEqual, "<>") :
                                        CreateAndAdvance(TokenType.Less),
                '>' => MatchNext('=') ? new Token(TokenType.GreaterEqual, ">=") :
                                        CreateAndAdvance(TokenType.Greater),
                '\0' => new Token(TokenType.EOF, ""),
                _ => throw new Exception($"Unknown character: {Current}")
            };
        }


        private string ReadWhile(Func<char, bool> condition)
        {
            int start = pos;
            while (condition(Current)) pos++;
            return text[start..pos];
        }

        private Token CreateAndAdvance(TokenType type)
        {
            char c = Current;
            pos++;
            return new Token(type, c.ToString());
        }

        private bool MatchNext(char expected)
        {
            if (pos + 1 < text.Length && text[pos + 1] == expected)
            {
                pos += 2;
                return true;
            }
            return false;
        }
    }


    //v2 with nodes in output
    public class Parser
    {
        private  Lexer lexer;
        private Token current;

        public Parser(Lexer lexer)
        {
            this.lexer = lexer;
            current = lexer.NextToken();
        }

        private void Eat(TokenType type)
        {
            if (current.Type == type)
                current = lexer.NextToken();
            else
                throw new Exception($"Waited for {type}, got {current.Type}");
        }

        public ParseNode Program()
        {
            var node = new ParseNode("программа");
            node.Add(Block());
            return node;
        }

        private ParseNode Block()
        {
            var node = new ParseNode("блок");
            Eat(TokenType.LBrace);
            node.Add(StatementList());
            Eat(TokenType.RBrace);
            return node;
        }
        private readonly HashSet<string> _printedCommands = new();
        public string ToRPN(ParseNode node)
        {
            Console.WriteLine($"[ToRPN] Узел: {node.Name}");

            switch (node.Name)
            {
                case "программа":
                case "блок":
                case "списокОператоров":
                case "хвост":
                    return string.Join("\n", node.Children.Select(ToRPN).Where(s => !string.IsNullOrEmpty(s)));

                case "оператор":
                    var idNode = node.Children[0];
                    var exprNode = node.Children[2];

                    string id = ExtractValue(idNode.Name);
                    string expr = ExpressionToRPN(exprNode);

                    string assign = $"{expr} {id} =";
                    Console.WriteLine($"[ToRPN] Присваивание: {assign}");
                    return assign;

                default:
                    return "";
            }
        }


        string ExpressionToRPN(ParseNode expr)
        {
            Console.WriteLine($"[Expression] Узел: {expr.Name}, кол-во детей: {expr.Children.Count}");

            if (expr.Children.Count == 1)
            {
                string result = ArithmeticExpressionToRPN(expr.Children[0]);
                Console.WriteLine($"[Expression] Одно арифметическое выражение: {result}");
                return result;
            }
            else if (expr.Children.Count == 3)
            {
                string left = ArithmeticExpressionToRPN(expr.Children[0]);
                string op = RelationOpToRPN(expr.Children[1]);
                string right = ArithmeticExpressionToRPN(expr.Children[2]);

                string result = $"{left} {right} {op}";
                Console.WriteLine($"[Expression] Сравнение: {result}");
                return result;
            }
            else
            {
                throw new Exception("Unexpected expression node children count");
            }
        }



        string RelationOpToRPN(ParseNode node)
        {
            string symbol = node.Name switch
            {
                "меньше" => "<",
                "меньшеИлиРавно" => "<=",
                "больше" => ">",
                "большеИлиРавно" => ">=",
                "равно" => "==",
                "неравно" => "<>",
                _ => throw new Exception($"Неизвестный логический оператор: {node.Name}")
            };

            Console.WriteLine($"[RelationOp] {node.Name} → {symbol}");
            return symbol;
        }




        string ArithmeticExpressionToRPN(ParseNode node)
        {
            Console.WriteLine($"[ArithmeticExpr] Старт");

            string result = ArithmeticTermToRPN(node.Children[0]) + " ";

            for (int i = 1; i < node.Children.Count; i += 2)
            {
                var opNode = node.Children[i];
                var termNode = node.Children[i + 1];

                string term = ArithmeticTermToRPN(termNode);
                string op = opNode.Name == "плюс" ? "+" :
                            opNode.Name == "минус" ? "-" :
                            throw new Exception($"Неизвестный оператор {opNode.Name}");

                result += $"{term} {op} ";
                Console.WriteLine($"[ArithmeticExpr] Добавили: {term} {op}");
            }

            Console.WriteLine($"[ArithmeticExpr] Конец: {result.Trim()}");
            return result.Trim();
        }


        string ArithmeticTermToRPN(ParseNode node)
        {
            Console.WriteLine($"[Term] Старт");

            string result = FactorToRPN(node.Children[0]) + " ";

            for (int i = 1; i < node.Children.Count; i += 2)
            {
                var opNode = node.Children[i];
                var factorNode = node.Children[i + 1];

                string factor = FactorToRPN(factorNode);
                string op = opNode.Name == "умножить" ? "*" :
                            opNode.Name == "деление" ? "/" :
                            throw new Exception($"Неизвестный оператор {opNode.Name}");

                result += $"{factor} {op} ";
                Console.WriteLine($"[Term] Добавили: {factor} {op}");
            }

            Console.WriteLine($"[Term] Конец: {result.Trim()}");
            return result.Trim();
        }

        string FactorToRPN(ParseNode node)
        {
            if (node.Children.Count == 1)
            {
                var child = node.Children[0];

                string value = child.Name.StartsWith("идентификатор") || child.Name.StartsWith("константа")
                    ? child.Name.Split('(', ')')[1]
                    : throw new Exception("Неизвестный тип фактора");

                Console.WriteLine($"[Factor] Узел: {value}");
                return value;
            }
            else if (node.Children.Count == 3)
            {
                Console.WriteLine($"[Factor] Скобочное выражение");
                return ArithmeticExpressionToRPN(node.Children[1]);
            }
            else
            {
                throw new Exception("Некорректная структура фактора");
            }
        }


        string ExtractValue(string name)
        {
            int start = name.IndexOf('(') + 1;
            int end = name.IndexOf(')');
            return name.Substring(start, end - start);
        }

   
        private ParseNode StatementList()
        {
            var node = new ParseNode("списокОператоров");
            node.Add(Statement());
            node.Add(Tail());
            return node;
        }

        private ParseNode Tail()
        {
            var node = new ParseNode("хвост");

            if (current.Type == TokenType.Semicolon)
            {
                Eat(TokenType.Semicolon);
                node.Add(Statement());
                node.Add(Tail());
            }
            // else eps { ничего не делаем}

            return node;
        }


        private ParseNode Statement()
        {
            var node = new ParseNode("оператор");

            if (current.Type == TokenType.Identifier)
            {
                var id = current.Value;
                Eat(TokenType.Identifier);
                node.Add(new ParseNode($"идентификатор({id})"));
                Eat(TokenType.Assign);
                node.Add(new ParseNode("присвоение"));
                node.Add(Expression());
            }
            else if (current.Type == TokenType.LBrace)
            {
                node.Add(Block());
            }
            else
            {
                throw new Exception("error");
            }

            return node;
        }

        private ParseNode Expression()
        {
            var node = new ParseNode("выражение");
            node.Add(ArithmeticExpression());

            if (IsRelOp(current.Type))
            {
                node.Add(RelOp());
                node.Add(ArithmeticExpression());
            }

            return node;
        }

        private ParseNode ArithmeticExpression()
        {
            var node = new ParseNode("арифмВыражение");
            node.Add(Term());

            while (current.Type == TokenType.Plus || current.Type == TokenType.Minus)
            {
                node.Add(AddOp());
                node.Add(Term());
            }

            return node;
        }

        private ParseNode Term()
        {
            var node = new ParseNode("терм");
            node.Add(Factor());

            while (current.Type == TokenType.Mul || current.Type == TokenType.Div)
            {
                node.Add(MulOp());
                node.Add(Factor());
            }

            return node;
        }

        private ParseNode Factor()
        {
            var node = new ParseNode("фактор");

            if (current.Type == TokenType.Identifier)
            {
                var id = current.Value;
                Eat(TokenType.Identifier);
                node.Add(new ParseNode($"идентификатор({id})"));
            }
            else if (current.Type == TokenType.Number)
            {
                var num = current.Value;
                Eat(TokenType.Number);
                node.Add(new ParseNode($"константа({num})"));
            }
            else if (current.Type == TokenType.LParen)
            {
                Eat(TokenType.LParen);
                node.Add(new ParseNode("л_скобка"));
                node.Add(ArithmeticExpression());
                Eat(TokenType.RParen);
                node.Add(new ParseNode("п_скобка"));
            }
            else
                throw new Exception("Expected identifier, number, or expression in parentheses");

            return node;
        }

        private ParseNode AddOp()
        {
            if (current.Type == TokenType.Plus)
            {
                Eat(TokenType.Plus);
                return new ParseNode("плюс");
            }
            else
            {
                Eat(TokenType.Minus);
                return new ParseNode("минус");
            }
        }

        private ParseNode MulOp()
        {
            if (current.Type == TokenType.Mul)
            {
                Eat(TokenType.Mul);
                return new ParseNode("умножить");
            }
            else
            {
                Eat(TokenType.Div);
                return new ParseNode("деление");
            }
        }

        /*private ParseNode RelOp()
        {
            var op = current.Type;
            Eat(op);
            return new ParseNode(op.ToString());
        }*/
        private ParseNode RelOp()
        {
            switch (current.Type)
            {
                case TokenType.Less:
                    Eat(TokenType.Less);
                    return new ParseNode("меньше");
                case TokenType.LessEqual:
                    Eat(TokenType.LessEqual);
                    return new ParseNode("меньшеИлиРавно");
                case TokenType.Equal:
                    Eat(TokenType.Equal);
                    return new ParseNode("равно");
                case TokenType.NotEqual:
                    Eat(TokenType.NotEqual);
                    return new ParseNode("неравно");
                case TokenType.Greater:
                    Eat(TokenType.Greater);
                    return new ParseNode("больше");
                case TokenType.GreaterEqual:
                    Eat(TokenType.GreaterEqual);
                    return new ParseNode("большеИлиРавно");
                default:
                    throw new Exception($"Ожидался логический оператор, но получен: {current.Type}");
            }
        }


        private bool IsRelOp(TokenType type) => type switch
        {
            TokenType.Less => true,
            TokenType.LessEqual => true,
            TokenType.Equal => true,
            TokenType.NotEqual => true,
            TokenType.Greater => true,
            TokenType.GreaterEqual => true,
            _ => false
        };
    }


}
