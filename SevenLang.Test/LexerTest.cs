using SevenLang.Core.Lexers;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SevenLang.Test
{
    public class LexerTest
    {
        [Fact]
        public void TestNormal()
        {
            var sr = new StringReader("(+ 1.1 2);this is a comment.");
            using (var lexer = new Lexer(sr))
            {
                var tokens = lexer.GetTokens().ToList();
                Assert.Collection(tokens,
                    t1 => Assert.Equal("(", t1.Text),
                    t2 => Assert.Equal("+", t2.Text),
                    t3 => Assert.Equal("1.1", t3.Text),
                    t4 => Assert.Equal("2", t4.Text),
                    t5 => Assert.Equal(")", t5.Text),
                    t6 => Assert.Equal("this is a comment.", t6.Text),
                    t7 => Assert.Equal("<EOF>", t7.Text));
            }
        }

        [Fact]
        public void TestLeftBracketToken()
        {
            var sr = new StringReader("(");
            using (var lexer = new Lexer(sr))
            {
                var tokens = lexer.GetTokens().ToList();
                Assert.Collection(tokens,
                    t1 => Assert.Equal("(", t1.Text),
                    t2 => Assert.Equal("<EOF>", t2.Text));
            }
        }

        [Fact]
        public void TestLeftBracketTokenWithWhitespace()
        {
            var sr = new StringReader(" ( ");
            using (var lexer = new Lexer(sr))
            {
                var tokens = lexer.GetTokens().ToList();
                Assert.Collection(tokens,
                    t1 => Assert.Equal("(", t1.Text),
                    t2 => Assert.Equal("<EOF>", t2.Text));
            }
        }

        [Fact]
        public void TestRightBracketToken()
        {
            var sr = new StringReader(")");
            using (var lexer = new Lexer(sr))
            {
                var tokens = lexer.GetTokens().ToList();
                Assert.Collection(tokens,
                    t1 => Assert.Equal(")", t1.Text),
                    t2 => Assert.Equal("<EOF>", t2.Text));
            }
        }

        [Fact]
        public void TestRightBracketTokenWithWhitespace()
        {
            var sr = new StringReader(" ) ");
            using (var lexer = new Lexer(sr))
            {
                var tokens = lexer.GetTokens().ToList();
                Assert.Collection(tokens,
                    t1 => Assert.Equal(")", t1.Text),
                    t2 => Assert.Equal("<EOF>", t2.Text));
            }
        }

        [Fact]
        public void TestIdentifierToken()
        {
            var sr = new StringReader("begin");
            using (var lexer = new Lexer(sr))
            {
                var tokens = lexer.GetTokens().ToList();
                Assert.Collection(tokens,
                    t1 => Assert.Equal("begin", t1.Text),
                    t2 => Assert.Equal("<EOF>", t2.Text));
            }
        }

        [Fact]
        public void TestIdentifierTokenWithWhitespace()
        {
            var sr = new StringReader(" begin ");
            using (var lexer = new Lexer(sr))
            {
                var tokens = lexer.GetTokens().ToList();
                Assert.Collection(tokens,
                    t1 => Assert.Equal("begin", t1.Text),
                    t2 => Assert.Equal("<EOF>", t2.Text));
            }
        }

        [Fact]
        public void TestStringToken()
        {
            var sr = new StringReader("\"wow\"");
            using (var lexer = new Lexer(sr))
            {
                var tokens = lexer.GetTokens().ToList();
                Assert.Collection(tokens,
                    t1 => Assert.Equal("wow", t1.Text),
                    t2 => Assert.Equal("<EOF>", t2.Text));
            }
        }

        [Fact]
        public void TestStringTokenWithWhitespace()
        {
            var sr = new StringReader(" \"wow\" ");
            using (var lexer = new Lexer(sr))
            {
                var tokens = lexer.GetTokens().ToList();
                Assert.Collection(tokens,
                    t1 => Assert.Equal("wow", t1.Text),
                    t2 => Assert.Equal("<EOF>", t2.Text));
            }
        }

        [Fact]
        public void TestNumberToken()
        {
            var sr = new StringReader("0.5");
            using (var lexer = new Lexer(sr))
            {
                var tokens = lexer.GetTokens().ToList();
                Assert.Collection(tokens,
                    t1 => Assert.Equal("0.5", t1.Text),
                    t2 => Assert.Equal("<EOF>", t2.Text));
            }
        }

        [Fact]
        public void TestNumberTokenWithWhitespace()
        {
            var sr = new StringReader(" 0.5 ");
            using (var lexer = new Lexer(sr))
            {
                var tokens = lexer.GetTokens().ToList();
                Assert.Collection(tokens,
                    t1 => Assert.Equal("0.5", t1.Text),
                    t2 => Assert.Equal("<EOF>", t2.Text));
            }
        }
    }
}
