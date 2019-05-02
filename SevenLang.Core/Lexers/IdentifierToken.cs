namespace SevenLang.Core.Lexers
{
    class IdentifierToken: Token
    {
        public string Identifier { get; }
        public override string Text => Identifier.Replace("\r", "\\r").Replace("\n", "\\n");

        public IdentifierToken(string identifier)
        {
            this.Identifier = identifier;
        }
    }
}
