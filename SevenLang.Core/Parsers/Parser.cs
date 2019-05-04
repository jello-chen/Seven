using SevenLang.Core.Lexers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SevenLang.Core.Parsers
{
    public class Parser
    {
        public IEnumerable<IExpression> Parse(IEnumerable<Token> tokens)
        {
            var queue = new Queue<Token>(tokens.Where(t => t != Token.EOF && (!(t is CommentToken))));
            while (queue.Count > 0)
            {
                var exp = ParseImpl(queue);
                yield return exp.GetExpression();
            }
        }

        private SyntaxExpression ParseImpl(Queue<Token> tokens)
        {
            var token = tokens.Dequeue();
            if (token.Text == ")") throw new Exception("Unexpected '(' character");
            if (token.Text == "(")
            {
                var list = new SyntaxExpressionList();
                while (tokens.Peek().Text != ")")
                {
                    list.Exps.Enqueue(ParseImpl(tokens));
                }
                tokens.Dequeue();
                return list;
            }
            return new SyntaxExpressionAtom(token);
        }
    }

    public abstract class SyntaxExpression
    {
        public abstract IExpression GetExpression();
    }

    public class SyntaxExpressionAtom : SyntaxExpression
    {
        public SyntaxExpressionAtom(Token token)
        {
            Token = token;
        }

        public Token Token { get; }

        public override IExpression GetExpression()
        {
            switch (Token)
            {
                case NumberToken n:
                    return new Number(n.Value);
                case BooleanToken b:
                    return new Bool(b.Value);
                case StringToken s:
                    return new Literal(s.Literal);
                default:
                    return new Variable(Token.Text);
            }
        }
    }

    public class SyntaxExpressionList : SyntaxExpression
    {
        public Queue<SyntaxExpression> Exps { get; set; } = new Queue<SyntaxExpression>();

        public override IExpression GetExpression()
        {
            var head = Exps.Dequeue();
            var list = head as SyntaxExpressionList;
            if (list != null)
                return new Call(list.GetExpression(), Exps.Select(x => x.GetExpression()).ToArray());
            var atom = head as SyntaxExpressionAtom;
            switch (atom.Token.Text)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                case "<":
                case ">":
                case "=":
                case ">=":
                case "<=":
                case "!=":
                case "and":
                case "or":
                case "not":
                case "max":
                case "min":
                    return new ListFunction(atom.Token.Text, Exps.Select(x => x.GetExpression()).ToList());
                case "lambda":
                    var parameters = (SyntaxExpressionList)Exps.Dequeue();
                    var body = Exps.Dequeue().GetExpression();
                    return new Lambda(body, parameters.Exps.Select(x => ((SyntaxExpressionAtom)x).Token.Text).ToList());
                case "define":
                    return new Define(((SyntaxExpressionAtom)Exps.Dequeue()).Token.Text, Exps.Dequeue().GetExpression());
                case "if":
                    return new If(Exps.Dequeue().GetExpression(), Exps.Dequeue().GetExpression(), Exps.Dequeue().GetExpression());
                case "list":
                    return new List(Exps.Select(x => x.GetExpression()).ToArray());
                case "map":
                    return new Function(atom.Token.Text, Exps.Select(x => x.GetExpression()).ToArray());
                default:
                    return new Call(atom.GetExpression(), Exps.Select(x => x.GetExpression()).ToArray());
            }
        }
    }

    public interface IExpression
    {
        Value Evaluate(Dictionary<string, IExpression> env);
    }

    public abstract class Value : IExpression
    {
        public virtual Value Evaluate(Dictionary<string, IExpression> env) => this;
    }

    public abstract class Value<T>: Value
    {
        public Value(T raw) => Raw = raw;

        public T Raw { get; }

        public override string ToString() => Raw.ToString();
    }

    public class Bool : Value<bool>
    {
        public Bool(bool raw) : base(raw)
        {
        }

        public override string ToString() => Raw ? "#t" : "#f";
    }

    public class Number : Value<double>
    {
        public Number(double raw) : base(raw)
        {
        }
    }

    public class Literal : Value<string>
    {
        public Literal(string raw) : base(raw)
        {
        }
    }

    public class Closure: Value
    {
        public Closure(Lambda lambda, Dictionary<string, IExpression> context)
        {
            Lambda = lambda;
            Context = context;
        }

        public Lambda Lambda { get; }
        public Dictionary<string, IExpression> Context { get; }
    }

    public class Variable : IExpression
    {
        public Variable(string name) => Name = name;

        public string Name { get; }

        public Value Evaluate(Dictionary<string, IExpression> env) => env[Name].Evaluate(env);
    }

    public class Call : IExpression
    {
        public Call(IExpression body, params IExpression[] arguments)
        {
            Body = body;
            Arguments = arguments;
        }

        public IExpression Body { get; }
        public IExpression[] Arguments { get; }

        public Value Evaluate(Dictionary<string, IExpression> env)
        {
            var closure = Body.Evaluate(env) as Closure;
            var runtime = Arguments.Select(x => x.Evaluate(env));

            foreach (var pair in closure.Lambda.Parameters.Zip(runtime, (p, arg) => new { p, arg}))
            {
                closure.Context[pair.p] = pair.arg;
            }

            return closure.Lambda.Body.Evaluate(closure.Context);
        }
    }

    public class Define : IExpression
    {
        public Define(string name, IExpression expression)
        {
            Name = name;
            Expression = expression;
        }

        public string Name { get; }
        public IExpression Expression { get; }

        public Value Evaluate(Dictionary<string, IExpression> env)
        {
            env[Name] = Expression;
            return null;
        }
    }

    public class If : IExpression
    {
        public If(IExpression test, IExpression trueBranch, IExpression elseBranch)
        {
            Test = test;
            TrueBranch = trueBranch;
            ElseBranch = elseBranch;
        }

        public IExpression Test { get; }
        public IExpression TrueBranch { get; }
        public IExpression ElseBranch { get; }

        public Value Evaluate(Dictionary<string, IExpression> env)
        {
            var test = Test.Evaluate(env) as Value<bool>;
            return test.Raw ? TrueBranch.Evaluate(env) : ElseBranch.Evaluate(env);
        }
    }

    public class Lambda : IExpression
    {
        public Lambda(IExpression body, List<string> parameters)
        {
            Body = body;
            Parameters = parameters;
        }

        public IExpression Body { get; }
        public List<string> Parameters { get; }

        public Value Evaluate(Dictionary<string, IExpression> env)
        {
            return new Closure(this, env);
        }
    }

    public class ListFunction : IExpression
    {
        public ListFunction(string op, List<IExpression> arguments)
        {
            Op = op;
            Arguments = arguments;
        }

        public string Op { get; }
        public List<IExpression> Arguments { get; }

        public Value Evaluate(Dictionary<string, IExpression> env)
        {
            if(Op == "+" || Op == "-" || Op == "*" || Op == "/" || Op == "max" || Op == "min")
            {
                IEnumerable<double> nums = Arguments.Select(x => ((Number)x.Evaluate(env)).Raw);

                if (Op == "max")
                    return new Number(nums.Max());
                if (Op == "min")
                    return new Number(nums.Min());

                Func<double, double, double> func = null;
                switch (Op)
                {
                    case "+":
                        func = (n1, n2) => n1 + n2;
                        break;
                    case "-":
                        func = (n1, n2) => n1 - n2;
                        break;
                    case "*":
                        func = (n1, n2) => n1 * n2;
                        break;
                    case "/":
                        func = (n1, n2) => n1 / n2;
                        break;
                }
                return new Number(nums.Aggregate(func));
            }
            if(Op == "<" || Op == ">" || Op == "=" || Op == "!=" || Op == ">=" || Op == "<=")
            {
                double n1 = ((Number)Arguments[0].Evaluate(env)).Raw;
                double n2 = ((Number)Arguments[1].Evaluate(env)).Raw;
                switch (Op)
                {
                    case "<":
                        return new Bool(n1 < n2);
                    case ">":
                        return new Bool(n1 > n2);
                    case "=":
                        return new Bool(n1 == n2);
                    case "!=":
                        return new Bool(n1 != n2);
                    case ">=":
                        return new Bool(n1 >= n2);
                    case "<=":
                        return new Bool(n1 <= n2);
                }
            }
            if(Op == "and" || Op == "or" || Op == "not")
            {
                bool b1 = ((Bool)Arguments[0].Evaluate(env)).Raw;
                switch (Op)
                {
                    case "not":
                        return new Bool(!b1);
                    default:
                        {
                            bool b2 = ((Bool)Arguments[1].Evaluate(env)).Raw;
                            return Op == "and" ? new Bool(b1 && b2) : new Bool(b1 || b2);
                        }
                }
            }
            throw new NotImplementedException();
        }
    }

    public class Function : IExpression
    {
        public Function(string name, params IExpression[] parameters)
        {
            Name = name;
            Parameters = parameters;
        }

        public string Name { get; }
        public IExpression[] Parameters { get; }

        public Value Evaluate(Dictionary<string, IExpression> env)
        {
            switch (Name)
            {
                case "map":
                    var variable = Parameters[0] as Variable;
                    var lists = Parameters.Skip(1).Select(x => (List)x.Evaluate(env)).ToArray();
                    if (variable != null)
                    {
                        if(variable.Name == "+")
                        {
                            return lists.Aggregate((s1, s2) =>
                            {
                                var items = new IExpression[s1.Parameters.Length];
                                for (int i = 0; i < s1.Parameters.Length; i++)
                                {
                                    var n1 = s1.Parameters[i] as Number;
                                    var n2 = s2.Parameters[i] as Number;
                                    items[i] = new Number(n1.Raw + n2.Raw);
                                }
                                return new List(items);
                            });
                        }
                    }
                    else
                    {
                        var closure = Parameters[0].Evaluate(env) as Closure;
                        if(closure != null)
                        {
                            var itemLength = lists[0].Parameters.Length;
                            var items = new IExpression[itemLength];
                            for (int i = 0; i < itemLength; i++)
                            {
                                foreach (var pair in closure.Lambda.Parameters.Zip(lists.Select(args => args.Parameters[i]), (p, arg) => new { p, arg }))
                                {
                                    closure.Context[pair.p] = pair.arg;
                                }
                                items[i] = closure.Lambda.Body.Evaluate(closure.Context);
                            }
                            return new List(items);
                        }
                    }
                    break;
            }
            throw new NotSupportedException();
        }
    }

    public class List : Value
    {
        public List(IExpression[] parameters)
        {
            Parameters = parameters;
        }

        public IExpression[] Parameters { get; }

        public override string ToString()
            => "(" + string.Join(" ", Parameters.Select(v => v.ToString())) + ")";
    }
}
