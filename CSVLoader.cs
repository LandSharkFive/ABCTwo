using System.Globalization;

namespace ABCTwo
{
    public static class CsvDataLoader
    {
        /// <summary>
        /// Reads historical stock data from a CSV file, parses date, high, and low values, 
        /// and returns a list ordered chronologically by date.
        /// </summary>
        public static List<PriceBar> LoadFromCsv(string filePath)
        {
            var data = new List<PriceBar>();
            var lines = File.ReadAllLines(filePath);

            // Skip header line
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');

                // Expecting Yahoo Finance standard columns: Date, Open, High, Low, Close, ...
                if (DateTime.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) &&
                    double.TryParse(parts[1], CultureInfo.InvariantCulture, out var open) &&
                    double.TryParse(parts[2], CultureInfo.InvariantCulture, out var high) &&
                    double.TryParse(parts[3], CultureInfo.InvariantCulture, out var low) &&
                    double.TryParse(parts[4], CultureInfo.InvariantCulture, out var close))
                {
                    data.Add(new PriceBar(date, open, high, low, close));
                }
            }

            // Ensure ascending chronological order
            data.Sort((x, y) => x.Date.CompareTo(y.Date));
            return data;
        }



    }
}
