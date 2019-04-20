using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace SevenLang.Core.Lexers
{
    [DebuggerDisplay("Text = {Text}")]
    public class Token
    {
        public static readonly Token EOF = new Token() { Text = "<EOF>" };
        public static readonly string EOL = Environment.NewLine;
        public virtual string Text { get; private set; } = string.Empty;
    }

    enum TokenType
    {
        LeftBracket,
        RightBracket,
        Identifier,
        Number,
        String,
        Comment,
    }
}
