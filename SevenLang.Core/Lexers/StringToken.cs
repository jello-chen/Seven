namespace SevenLang.Core.Lexers
{
    class StringToken: Token
    {
        public string Literal { get; }
        public override string Text => Literal;

        public StringToken(string literal) => Literal = literal;
    }
}
