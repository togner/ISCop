using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using StyleCop;
using StyleCop.CSharp;

namespace ISCop.StyleCopRules
{
    [SourceAnalyzer(typeof(CsParser))]
    public class ISCopRules : SourceAnalyzer
    {
        public const string ScriptMainShouldHandleErrorsRuleName = "ScriptMainShouldHandleErrors";
        private static readonly Dictionary<string, string> ExpectedCatchExpressions = new Dictionary<string, string>
        {
            { @"Dts\.Events\.FireError\(0,.+,.+\.Message.+\.StackTrace,[\s]+string.Empty,[\s]+0\)", "Dts.Events.FireError(0, <package name>, <exception message + stacktrace>, string.Empty, 0)" },
            { @"Dts\.TaskResult = \(int\)ScriptResults\.Failure", "Dts.TaskResult = (int)ScriptResults.Failure"}
        };

        public override void AnalyzeDocument(CodeDocument document)
        {
            var csharpDocument = (CsDocument)document;
            if (csharpDocument.RootElement != null && !csharpDocument.RootElement.Generated)
            {
                csharpDocument.WalkDocument(this.VisitMainMethod);
            }
        }

        private bool VisitMainMethod(CsElement element, CsElement parentElement, object context)
        {
            if (element.ElementType == ElementType.Method 
                && element.Name.Equals("method Main", StringComparison.InvariantCultureIgnoreCase))
            {
                // find first (top-most) try token
                var tryToken = element.ElementTokens.FirstOrDefault(t => t.CsTokenType == CsTokenType.Try);
                if (tryToken == null)
                {
                    this.AddViolation(element, element.LineNumber, ISCopRules.ScriptMainShouldHandleErrorsRuleName, string.Empty);
                }
                else
                {
                    // check that catch statement contains expected expressions
                    var expectedCatchChildExpressions = new List<string>(ISCopRules.ExpectedCatchExpressions.Keys);
                    foreach (var catchStatement in ((TryStatement)tryToken.Parent).CatchStatements)
                    {
                        catchStatement.WalkStatement(this.VisitCatchChildStatements, expectedCatchChildExpressions);
                    }
                    if (expectedCatchChildExpressions.Count > 0)
                    {
                        this.AddViolation(element, 
                            element.LineNumber, 
                            ISCopRules.ScriptMainShouldHandleErrorsRuleName, 
                            string.Format(CultureInfo.CurrentCulture, " Catch block should contain \"{0}\"", ISCopRules.ExpectedCatchExpressions[expectedCatchChildExpressions[0]]));
                    }
                }
            }
            return true;
        }

        private bool VisitCatchChildStatements(Statement statement, Expression parentExpression, Statement parentStatement, CsElement parentElement, List<string> context)
        {
            if (statement.StatementType == StatementType.Expression)
            {
                var expectedExpressions = context;
                var expression = ((ExpressionStatement)statement).Expression;
                var consumedExpression = expectedExpressions.FirstOrDefault(expr => Regex.IsMatch(expression.Text, expr));
                if (consumedExpression != null)
                {
                    expectedExpressions.Remove(consumedExpression);
                }
            }
            return true;
        }
    }
}
