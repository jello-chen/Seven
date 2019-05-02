using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SevenLang.Core.Lexers
{
    public class Lexer : IDisposable
    {
        private static readonly char[] VALID_CHARACTER = new char[] { '+', '-', '*', '/', '=', '!', '<', '>' };

        private readonly TextReader textReader;

        public Lexer(TextReader textReader)
        {
            this.textReader = textReader;
        }

        public IEnumerable<Token> GetTokens()
        {
            int c = -1;
            while ((c = this.textReader.Peek()) != -1)
            {
                switch (c)
                {
                    case ' ':
                    case '\r':
                    case '\n':
                        this.textReader.Read();
                        break;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        {
                            string s = null;
                            int d = -1;
                            while ((d = this.textReader.Peek()) != -1 && (d == '.' || d >= '0' && d <= '9'))
                            {
                                d = this.textReader.Read();
                                s += ((char)d).ToString();
                            }
                            yield return new NumberToken(double.Parse(s));
                        }
                        break;
                    case '#':
                        {
                            this.textReader.Read();
                            int d = this.textReader.Read();
                            if (d == 't')
                                yield return new BooleanToken(true);
                            else if (d == 'f')
                                yield return new BooleanToken(false);
                            else
                                throw new Exception("Unexpected char.");
                        }
                        break;
                    case '"':
                        {
                            this.textReader.Read();
                            string s = null;
                            int d = -1;
                            while ((d = this.textReader.Peek()) != -1)
                            {
                                d = this.textReader.Read();
                                if (d == '"') break;
                                s += ((char)d).ToString();
                            }
                            yield return new StringToken(s);
                        }
                        break;
                    case '(':
                        this.textReader.Read();
                        yield return new LeftBracketToken();
                        break;
                    case ')':
                        this.textReader.Read();
                        yield return new RightBracketToken();
                        break;
                    case ';':
                        this.textReader.Read();
                        yield return new CommentToken(this.textReader.ReadLine());
                        break;
                    default:
                        {
                            string identifier = null;
                            int d = -1;
                            while ((d = this.textReader.Peek()) != -1)
                            {
                                if (!char.IsLetterOrDigit((char)d) && !VALID_CHARACTER.Contains((char)d))
                                    break;
                                d = this.textReader.Read();
                                identifier += ((char)d).ToString();
                            }
                            if(identifier == null)
                                throw new ArgumentException("Unexpected char.");
                            yield return new IdentifierToken(identifier);
                        }
                        break;
                }
            }
            yield return Token.EOF;
        }

        public void Dispose()
        {
            textReader?.Dispose();
        }
    }
}
