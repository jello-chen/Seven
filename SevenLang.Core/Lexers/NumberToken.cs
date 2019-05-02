namespace SevenLang.Core.Lexers
{
    class NumberToken: Token
    {
        public double Value { get; }
        public override string Text => Value.ToString();

        public NumberToken(double value) => Value = value;
    }
}
