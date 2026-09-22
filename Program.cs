namespace ABCTwo
{
    internal class Program
    {

        /// <summary>
        /// Entry point for executing the ABC pattern detection and backtest strategy pipeline on historical price data.
        /// </summary>
        static void Main(string[] args)
        {
            double positionSize = 1000;
            double trailPct = 0.025;

            Console.WriteLine("NVDA");
            string filename = "NVDA5y.csv";

            // Load historical price bars from disk.
            var history = Test.GetHistory(filename);

            // Scan price bars for valid ascending ABC chart patterns.
            var patterns = Test.RunTest(history);
            Console.WriteLine($"ABC Count {patterns.Count}");

            // Run Trailing Stop strategy against detected patterns and print performance metrics.
            Strategy.StrategyOne(history, patterns, trailPct, positionSize);
            Console.WriteLine();
        }
    }
}
