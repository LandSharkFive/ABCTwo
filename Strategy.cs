
namespace ABCTwo
{
    /// <summary>
    /// Tracks realized profit or loss for an individual completed trade.
    /// </summary>
    public class EquityPoint
    {
        public DateTime Date { get; set; }
        public double Profit { get; set; }
    }

    /// <summary>
    /// Tracks total account balance at a specific point in time.
    /// </summary>
    public class BalancePoint
    {
        public DateTime Date { get; set; }
        public double Balance { get; set; }
    }

    /// <summary>
    /// Executes trading strategies and backtesting evaluation against detected pattern setups.
    /// </summary>
    public static class Strategy
    {
        /// <summary>
        /// Executes a trailing stop trading strategy on detected ABC patterns, entering at Point E 
        /// and ratcheting a percentage stop upward as price reaches new highs until stopped out.
        /// </summary>
        public static void StrategyOne(List<PriceBar> history, List<Pattern> patterns, double trailPct = 0.025, double positionSize = 1000.0, double startingCapital = 10000.0)
        {
            int targetHits = 0;
            var equityCurve = new List<EquityPoint>();

            foreach (var p in patterns)
            {
                int enterAt = p.PointE.Index;
                if (enterAt >= history.Count)
                    continue;

                double enterPrice = p.PointE.Price;
                double shares = Math.Floor(positionSize / enterPrice);
                if (shares <= 0) continue;

                double currentStop = 0.995 * p.PointC.Price;
                double highestPrice = enterPrice;
                bool hitTarget = false;

                for (int j = enterAt; j < history.Count; j++)
                {
                    var bar = history[j];

                    // Track if Target D was reached during the run.
                    if (!hitTarget && bar.High >= p.TargetD)
                    {
                        hitTarget = true;
                        targetHits++;
                    }

                    // Check for Trailing Stop Breach.
                    if (bar.Low <= currentStop)
                    {
                        double exitPrice = Math.Min(bar.Open, currentStop);
                        double tradeProfit = (exitPrice - enterPrice) * shares;
                        equityCurve.Add(new EquityPoint { Date = bar.Date, Profit = tradeProfit });
                        break;
                    }

                    // Ratchet stop upward on higher highs.
                    if (bar.High > highestPrice)
                    {
                        highestPrice = bar.High;
                        double newTrailStop = highestPrice * (1.0 - trailPct);
                        if (newTrailStop > currentStop)
                        {
                            currentStop = newTrailStop;
                        }
                    }
                }
            }

            ProcessAndOutputResults($"Strategy One (Trailing Stop {trailPct * 100:F1}%)", targetHits, equityCurve, startingCapital, "balance1.csv");
        }

        /// <summary>
        /// Computes performance metrics (win rate, total profit, drawdown) and prints a summary report.
        /// </summary>
        private static void ProcessAndOutputResults(string strategyName, int targetHits, List<EquityPoint> equityCurve, double startingCapital, string fileName)
        {
            int totalTrades = equityCurve.Count;
            int winningTrades = equityCurve.Count(e => e.Profit > 0);
            int losingTrades = equityCurve.Count(e => e.Profit < 0);
            double winRate = totalTrades > 0 ? ((double)winningTrades / totalTrades) * 100.0 : 0.0;

            var dailyProfits = equityCurve
                .GroupBy(e => e.Date)
                .Select(g => new { Date = g.Key, DailyProfit = g.Sum(e => e.Profit) })
                .OrderBy(e => e.Date);

            double currentBalance = startingCapital;
            var balanceCurve = new List<BalancePoint>();

            foreach (var day in dailyProfits)
            {
                currentBalance += day.DailyProfit;
                balanceCurve.Add(new BalancePoint { Date = day.Date, Balance = currentBalance });
            }

            double totalProfit = equityCurve.Sum(e => e.Profit);

            // Drawdown calculation anchored to actual equity peaks
            double maxDrawdown = 0;
            double peak = startingCapital;

            foreach (var point in balanceCurve)
            {
                if (point.Balance > peak)
                {
                    peak = point.Balance;
                }

                double drawdown = peak - point.Balance;
                if (drawdown > maxDrawdown)
                {
                    maxDrawdown = drawdown;
                }
            }

            Console.WriteLine(strategyName);
            Console.WriteLine($"Total Trades:      {totalTrades}");
            Console.WriteLine($"Winning Trades:    {winningTrades}");
            Console.WriteLine($"Losing Trades:     {losingTrades}");
            Console.WriteLine($"Win Rate:          {winRate:F2}%");
            Console.WriteLine($"Target D Hits:     {targetHits}");
            Console.WriteLine($"Total Profit:      ${totalProfit:F2}");
            Console.WriteLine($"Max Drawdown:      ${maxDrawdown:F2}\n");

            SaveAccountBalanceToCsv(fileName, balanceCurve);
        }

        /// <summary>
        /// Exports the calculated daily account balance curve to a formatted CSV file.
        /// </summary>
        private static void SaveAccountBalanceToCsv(string filePath, List<BalancePoint> balanceCurve)
        {
            using (var writer = new System.IO.StreamWriter(filePath))
            {
                writer.WriteLine("Date,AccountBalance");
                foreach (var item in balanceCurve)
                {
                    writer.WriteLine($"{item.Date:yyyy-MM-dd},{item.Balance:F2}");
                }
            }
        }



    }
}
