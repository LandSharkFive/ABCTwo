
namespace ABCTwo
{
    /// <summary>
    /// Represents a single historical price bar containing high and low values.
    /// </summary>
    public record PriceBar(DateTime Date, double Open, double High, double Low, double Close);

    /// <summary>
    /// Specifies the direction of a detected technical price pivot.
    /// </summary>
    public enum PivotType { High, Low }

    /// <summary>
    /// Represents a calculated swing pointx in the price series.
    /// </summary>
    public record PivotPoint(DateTime Date, double Price, PivotType Type, int Index);

    /// <summary>
    /// ABC chart pattern and its calculated price target.
    /// </summary>
    public record Pattern(
        PivotPoint PointA,
        PivotPoint PointB,
        PivotPoint PointC,
        PivotPoint PointE,
        double TargetD
    );

    /// <summary>
    /// Identify the technical price pivots and scanning historical data for ascending ABC chart patterns.
    /// </summary>
    public static class Detector
    {
        /// <summary>
        /// Evaluates a collection of price bars to identify the latest valid ascending ABC pattern 
        /// meeting specified risk/reward constraints.
        /// </summary>
        public static List<Pattern> GetPatterns(List<PriceBar> candles, int globalOffset = 0)
        {
            var pivots = IdentifyPivots(candles);

            if (!TryFindLatestAbc(pivots, out var pointA, out var pointB, out var pointC))
            {
                return new List<Pattern>();
            }

            int localEnterIndex = pointC.Index + 1;

            // Safety check: point C cannot be the very last candle in the window
            if (localEnterIndex >= candles.Count)
            {
                return new List<Pattern>();
            }

            double targetD = pointC.Price + (pointB.Price - pointA.Price);
            double enterPrice = candles[localEnterIndex].Open;
            int globalEnterAt = globalOffset + localEnterIndex;
            var pointE = new PivotPoint(candles[localEnterIndex].Date, enterPrice, PivotType.Low, globalEnterAt);

            double reward = targetD - enterPrice;
            double risk = enterPrice - pointC.Price;

            // Filter by Risk/Reward constraints.
            if (reward <= 2.0 || reward <= risk)
            {
                return new List<Pattern>();
            }

            return new List<Pattern>
            {
                new Pattern(pointA, pointB, pointC, pointE, targetD)
            };
        }

        /// <summary>
        /// Iterates through historical price bars to find swing high and swing low pivot points.
        /// </summary>
        private static List<PivotPoint> IdentifyPivots(List<PriceBar> candles)
        {
            var pivots = new List<PivotPoint>();
            for (int i = 1; i < candles.Count - 1; i++)
            {
                if (IsSwingLow(candles, i))
                    pivots.Add(new PivotPoint(candles[i].Date, candles[i].Low, PivotType.Low, i));
                else if (IsSwingHigh(candles, i))
                    pivots.Add(new PivotPoint(candles[i].Date, candles[i].High, PivotType.High, i));
            }
            return pivots;
        }

        /// <summary>
        /// States for the reverse-scanning ABC pattern discovery state machine.
        /// </summary>
        private enum AbcSearchState { FindC, FindB, FindA }

        /// <summary>
        /// Scans identified pivots backward in time to locate the most recent valid ascending ABC structure.
        /// </summary>
        private static bool TryFindLatestAbc(
            List<PivotPoint> pivots,
            out PivotPoint pointA,
            out PivotPoint pointB,
            out PivotPoint pointC)
        {
            pointA = null;
            pointB = null;
            pointC = null;
            AbcSearchState state = AbcSearchState.FindC;

            for (int i = pivots.Count - 1; i >= 0; i--)
            {
                var current = pivots[i];

                switch (state)
                {
                    case AbcSearchState.FindC:
                        if (current.Type == PivotType.Low)
                        {
                            pointC = current;
                            state = AbcSearchState.FindB;
                        }
                        break;

                    case AbcSearchState.FindB:
                        if (current.Type == PivotType.High && current.Price > pointC.Price)
                        {
                            pointB = current;
                            state = AbcSearchState.FindA;
                        }
                        break;

                    case AbcSearchState.FindA:
                        if (current.Type == PivotType.Low &&
                            pointB.Price > current.Price &&
                            pointC.Price > current.Price)
                        {
                            pointA = current;
                            return true;
                        }
                        break;
                }
            }

            return false;
        }

        /// <summary>
        /// Prints up to a specified number of detected patterns to the console.
        /// </summary>
        public static void PrintPatternsByCount(List<Pattern> list, int count)
        {
            int index = 0;
            foreach (Pattern pat in list.Take(count))
            {
                index++;
                PrintPattern(pat, index);
            }
        }

        /// <summary>
        /// Displays detailed target and pivot metrics for a single pattern instance.
        /// </summary>
        public static void PrintPattern(Pattern pat, int index)
        {
            Console.WriteLine($"Index {index}");
            Console.WriteLine($"A {pat.PointA.Date} {pat.PointA.Price:F2}");
            Console.WriteLine($"B {pat.PointB.Date} {pat.PointB.Price:F2}");
            Console.WriteLine($"C {pat.PointC.Date} {pat.PointC.Price:F2}");
            Console.WriteLine($"D {pat.TargetD:F2}");
            Console.WriteLine($"E {pat.PointE.Date} {pat.PointE.Price:F2}");
            Console.WriteLine();
        }


        /// <summary>
        /// Determines if the bar at index i forms a 3-bar swing low.
        /// </summary>
        public static bool IsSwingLow(List<PriceBar> candles, int i)
        {
            return candles[i].Low < candles[i - 1].Low && candles[i].Low < candles[i + 1].Low;
        }

        /// <summary>
        /// Determines if the bar at index <paramref name="i"/> forms a 3-bar swing high.
        /// </summary>
        public static bool IsSwingHigh(List<PriceBar> candles, int i)
        {
            return candles[i].High > candles[i - 1].High && candles[i].High > candles[i + 1].High;
        }

        /// <summary>
        /// Calculates the midpoint price of a given price bar.
        /// </summary>
        public static double Mid2(PriceBar bar) => (bar.High + bar.Low) / 2.0;

        /// <summary>
        /// Calculates the linear regression slope normalized as a percentage of the latest closing price.
        /// </summary>
        public static double GetNormalSlope(List<PriceBar> candles)
        {
            if (candles == null || candles.Count == 0)
                return 0;

            double slope = GetSlope(candles);
            double currentPrice = candles[^1].Close;

            if (currentPrice == 0) return 0;

            // Returns slope as a percentage of current price per bar
            return (slope / currentPrice) * 100.0;
        }

        /// <summary>
        /// Computes the ordinary least squares (OLS) linear regression slope on bar closing prices.
        /// </summary>
        public static double GetSlope(List<PriceBar> candles)
        {
            int n = candles.Count;
            if (n == 0)
                throw new ArgumentException("List must not be empty.");

            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

            for (int i = 0; i < n; i++)
            {
                double x = i;
                double y = candles[i].Close;

                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }

            double denominator = n * sumX2 - sumX * sumX;
            if (denominator == 0)
            {
                // Insufficient variance in X.
                return 0;
            }

            return (n * sumXY - sumX * sumY) / denominator;
        }


    }
}
