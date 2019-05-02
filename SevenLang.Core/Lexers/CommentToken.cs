namespace SevenLang.Core.Lexers
{
    class CommentToken: Token
    {
        private readonly string comment;
        public override string Text => comment;

        public CommentToken(string comment)
        {
            this.comment = comment;
        }
    }
}
