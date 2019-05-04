using SevenLang.Core.Lexers;
using SevenLang.Core.Parsers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace SevenLang.Test
{
    public class ParserTest
    {
        [Fact]
        public void TestListFunction()
        {
            var value = Evaluate("(* 1.1 2 (+ 1 2));this is a list function.", null) as Value<double>;
            Assert.Equal(6.6, value.Raw, 6);
        }

        [Fact]
        public void TestListFunctionWithEnvironment()
        {
            var env = new Dictionary<string, IExpression>
            {
                ["n1"] = new Number(1)
            };
            var value = Evaluate("(* 1.1 2 (+ n1 2))", env) as Value<double>;
            Assert.Equal(6.6, value.Raw, 6);
        }

        [Fact]
        public void TestDefine()
        {
            Dictionary<string, IExpression> env = new Dictionary<string, IExpression>();
            Evaluate("(define n 2)", env);
            Assert.Contains("n", (IDictionary<string, IExpression>)env);
            Assert.Equal(2, ((Number)env["n"]).Raw);
        }

        [Fact]
        public void TestIf()
        {
            var value = Evaluate("(if (< 1 2) 1 0)", null) as Number;
            Assert.Equal(1, value.Raw);
        }

        [Fact]
        public void TestIfWithEnvironment()
        {
            Dictionary<string, IExpression> env = new Dictionary<string, IExpression>()
            {
                ["n1"] = new Number(1),
                ["n2"] = new Number(2),
            };
            var value = Evaluate("(if (or (and #t (> n1 n2)) (not (> n1 n2))) 1 0)", env) as Number;
            Assert.Equal(1, value.Raw);
        }

        [Fact]
        public void TestList()
        {
            var value = Evaluate("(list 1 2 3)", null) as List;
            Assert.Equal("(1 2 3)", value.ToString());
        }

        [Fact]
        public void TestMap()
        {
            Dictionary<string, IExpression> env = new Dictionary<string, IExpression>();
            var value = Evaluate("(map (lambda (n1 n2 n3) (+ n1 n2 n3)) (list 1 2 3) (list 4 5 6) (list 7 8 9))", env) as List;
            Assert.Equal("(12 15 18)", value.ToString());
        }

        [Fact]
        public void TestCall()
        {
            Dictionary<string, IExpression> env = new Dictionary<string, IExpression>();
            Evaluate("(define inc (lambda (n) (+ n 1)))", env);
            var value = Evaluate("(inc 1)", env) as Number;
            Assert.Equal(2, value.Raw);
        }

        [Fact]
        public void TestRecursiveCall()
        {
            Dictionary<string, IExpression> env = new Dictionary<string, IExpression>();
            var value = Evaluate("(define fac (lambda (n) (if (= n 1) 1 (* n (fac (- n 1))))))(fac 5)", env) as Number;
            Assert.Equal(120, value.Raw);
        }

        [Fact]
        public void TestMutualCall()
        {
            Dictionary<string, IExpression> env = new Dictionary<string, IExpression>();
            var value = Evaluate("(define square (lambda (n) (* n n))) (define inc (lambda (n) (+ n 1))) (square (inc 2))", env) as Number;
            Assert.Equal(9, value.Raw);
        }

        private Value Evaluate(string input, Dictionary<string, IExpression> env)
        {
            using (var sr = new StringReader(input))
            {
                using (var lexer = new Lexer(sr))
                {
                    var tokens = lexer.GetTokens();
                    var parser = new Parser();
                    var expr = parser.Parse(tokens);
                    var expressions = parser.Parse(tokens).ToArray();
                    for (int i = 0; i < expressions.Length - 1; i++)
                    {
                        expressions[i].Evaluate(env);
                    }
                    return expressions.Last().Evaluate(env);
                }
            }
        }
    }
}
