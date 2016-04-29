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
        // TODO: Separate classes
        public const string MainShouldHandleErrorsRuleName = "MainShouldHandleErrors";
        public const string PublicMethodsShouldHandleErrorsRuleName = "PublicMethodsShouldHandleErrors";
        private static readonly Dictionary<string, string> MainExpectedCatchExpressions = new Dictionary<string, string>
        {
            { @"Dts\.Events\.FireError\(0,.+,.+\.Message.+\.StackTrace,[\s]+string.Empty,[\s]+0\)", "Dts.Events.FireError(0, <package name>, <exception message + stacktrace>, string.Empty, 0)" },
            { @"Dts\.TaskResult = \(int\)ScriptResults\.Failure", "Dts.TaskResult = (int)ScriptResults.Failure"}
        };
        private static readonly Dictionary<string, string> PublicMethodsExpectedCatchExpressions = new Dictionary<string, string>
        {
            { @"ComponentMetaData\.FireWarning\(0, ComponentMetaData\.Name\.Trim\(\),.+\.Message.+\.StackTrace.+string.Empty,[\s]+0\)", "ComponentMetaData.FireWarning(0, ComponentMetaData.Name.Trim(), <exception message + stacktrace>, string.Empty, 0)" },
            { "Row\\.Status = \"FAILED\"", "Row.Status = \"FAILED\""}
        };

        public override void AnalyzeDocument(CodeDocument document)
        {
            var csharpDocument = (CsDocument)document;
            if (csharpDocument.RootElement != null && !csharpDocument.RootElement.Generated)
            {
                csharpDocument.WalkDocument(this.VisitMethod);
            }
        }

        private bool VisitMethod(CsElement element, CsElement parentElement, object context)
        {
            if (element.ElementType == ElementType.Method)
            {
                if (element.Name.Equals("method Main", StringComparison.OrdinalIgnoreCase))
                {
                    this.ValidateTryCatch(element, ISCopRules.MainShouldHandleErrorsRuleName, ISCopRules.MainExpectedCatchExpressions);
                }
                else if (element.AccessModifier == AccessModifierType.Public)
                {
                    this.ValidateTryCatch(element, ISCopRules.PublicMethodsShouldHandleErrorsRuleName, ISCopRules.PublicMethodsExpectedCatchExpressions);
                }
            }
            return true;
        }

        private void ValidateTryCatch(CsElement element, string ruleName, Dictionary<string, string> expectedCatchExpressions)
        {
            // find first (top-most) try token
            var tryToken = element.ElementTokens.FirstOrDefault(t => t.CsTokenType == CsTokenType.Try);
            if (tryToken == null)
            {
                this.AddViolation(element, 
                    element.LineNumber,
                    ruleName,
                    element.FullNamespaceName, 
                    string.Empty);
            }
            else
            {
                // check that catch statement contains expected expressions
                var expectedCatchChildExpressions = new List<string>(expectedCatchExpressions.Keys);
                foreach (var catchStatement in ((TryStatement)tryToken.Parent).CatchStatements)
                {
                    catchStatement.WalkStatement(this.VisitCatchChildStatements, expectedCatchChildExpressions);
                }
                if (expectedCatchChildExpressions.Count > 0)
                {
                    this.AddViolation(element,
                        element.LineNumber,
                        ruleName,
                        element.FullNamespaceName, 
                        string.Format(CultureInfo.CurrentCulture, " Catch block should contain \"{0}\"", expectedCatchExpressions[expectedCatchChildExpressions[0]]));
                }
            }
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
