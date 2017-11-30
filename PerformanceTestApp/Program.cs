namespace Praetorius.SequenceRecognizer.PerformanceTestApp
{
    class Program
    {
        static void Main()
        {
            const int alphabetSize = 4;

            var sequences = new[]
            {
                new[] { 1, 3 },
                new[] { 2, 2, 2 },
                new[] { 2, 2, 1, 1 },
                new[] { 2, 2, 2, 2 },
                new[] { 2, 2, 2, 3 }
            };

            for (int i = 0; i < 100000; ++i)
            {
                var g = new StateGraph(alphabetSize, sequences);

                var sm = new StateMachine(g);

                for (int k = 0; k < alphabetSize; ++k)
                {
                    for (int j = 0; j < 1000 * k; ++j)
                    {
                        sm.AcceptSymbol(k);

                        var sequence = sm.Sequence;
                    }
                }
            }
        }
    }
}
