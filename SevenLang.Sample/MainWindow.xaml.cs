using SevenLang.Core.Lexers;
using SevenLang.Core.Parsers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace SevenLang.Sample
{
    /// <summary>
    /// MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeSourceCode();
        }

        private void BtnEvaluate_Click(object sender, RoutedEventArgs e)
        {
            var env = new Dictionary<string, IExpression>();
            var value = Evaluate(this.tbSourceCode.Text, env) as Number;
            this.tbOutput.Text = value?.Raw.ToString();
        }

        private void InitializeSourceCode()
        {
            this.tbSourceCode.Text = "(define fac (lambda (n) (if (= n 1) 1 (* n (fac (- n 1))))))(fac 5);;Using recursion to calculate factorial";
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
