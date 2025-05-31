using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace сс2
{
    enum TokenType
    {
        Identifier,
        Number,
        Plus, Minus, Mul, Div,
        Less, LessEqual, Equal, NotEqual, Greater, GreaterEqual,
        Assign,
        LParen, RParen,
        LBrace, RBrace,
        Semicolon,
        EOF,
    }

    class Token
    {
        public TokenType Type { get; }
        public string Value { get; }

        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString() => $"{Type} ({Value})";
    }

}
