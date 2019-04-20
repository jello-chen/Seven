using System;
using System.Collections.Generic;
using System.Text;

namespace SevenLang.Core.Lexers
{
    class NumberToken: Token
    {
        public double Value { get; }
        public override string Text => Value.ToString();

        public NumberToken(double value) => Value = value;
    }
}
