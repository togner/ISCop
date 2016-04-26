using System;

namespace Togner.Common.ConsoleApp
{
    public static class ConsolePrinter
    {
        private const string Line = "---------------------------------------";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)")]
        public static void Print(Exception exception, string message)
        {
            Console.WriteLine(message);
            Console.WriteLine(ConsolePrinter.Line);
            while (exception != null)
            {
                Console.WriteLine(exception.Message);
                Console.WriteLine();
                Console.WriteLine(exception.GetType().FullName);
                Console.WriteLine();
                Console.WriteLine(exception.StackTrace);
                Console.WriteLine(ConsolePrinter.Line);
                exception = exception.InnerException;
            }
        }
    }
}
