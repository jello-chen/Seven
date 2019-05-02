namespace SevenLang.Core.Lexers
{
    public class BooleanToken: Token
    {
        public bool Value { get; }
        public override string Text => Value ? "#t" : "#f";

        public BooleanToken(bool value) => Value = value;
    }
}
