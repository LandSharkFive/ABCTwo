namespace ABCTwo
{
    /// <summary>
    /// Test harness for loading CSV data and orchestrating pattern detection runs.
    /// </summary>
    public static class Test
    {
        /// <summary>
        /// Loads a list of historical price bars from a specified CSV file path.
        /// </summary>
        public static List<PriceBar> GetHistory(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine($"File not found at {filename}");
                return new List<PriceBar>();
            }
            return CsvDataLoader.LoadFromCsv(filename);
        }

        /// <summary>
        /// Runs the pattern detector against loaded stock history and logs initial pattern output.
        /// </summary>
        public static List<Pattern> RunTest(List<PriceBar> history)
        {
            List<Pattern> result = new List<Pattern>();
            const int windowSize = 20;
            const int lookback = windowSize - 1;

            for (int i = lookback; i < history.Count; i++)
            {
                int startOffset = i - lookback;

                // Use C# range syntax to slice the list.
                var candles = history[(startOffset)..(i + 1)];

                if (Detector.GetNormalSlope(candles) > 0.01)
                {
                    var patterns = Detector.GetPatterns(candles, startOffset);
                    if (patterns.Count > 0 && patterns[0].PointE.Index == i)
                    {
                        result.Add(patterns[0]);
                    }
                }
            }

            return result;
        }


    }
}
