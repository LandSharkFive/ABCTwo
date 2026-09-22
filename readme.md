# ABC Pattern Backtesting Framework

ABCTwo is a C# backtesting engine designed to detect ascending ABC chart patterns in historical stock price data and evaluate trading performance using customizable risk management strategies (e.g., dynamic trailing stops).

---

## 📌 Features

* **CSV Data Ingestion**: Parses standard Yahoo Finance daily price data (Date, Open, High, Low, Close).
* **Swing Pivot Detection**: Identifies 3-bar swing highs and swing lows to detect price turns.
* **Ascending ABC Pattern Identification**:
  * **Point A**: First major swing low.
  * **Point B**: Higher swing high following Point A (Price B > Price A).
  * **Point C**: Higher swing low following Point B (Price C > Price A and Price C < Price B).
  * **Target D**: Projected target computed as D = C + (B - A).
  * **Point E**: Entry candle (the candle immediately following Point C).
* **Slope & Trend Filter**: Calculates the linear regression slope normalized to current stock prices to ensure trades are taken only during upward momentum.
* **Risk/Reward Filtering**: Filters out pattern setups where the reward-to-risk ratio or absolute gain metrics fall below defined thresholds.
* **Backtesting & Strategy Engine**:
  * **Trailing Stop Execution**: Dynamic ratchet stops that trailing price highs by a set percentage.
  * **Performance Metrics**: Computes Win Rate, Total Profit, Target D Hit Rate, and Maximum Drawdown.
* **CSV Export**: Outputs daily equity balance curves to CSV format for plotting and further analysis.

---

## 🚀 Getting Started

### Prerequisites

* [.NET 6.0 SDK](https://dotnet.microsoft.com/download) or higher.
* Historical CSV market data (e.g., downloaded from Yahoo Finance) placed in the execution directory.

### Running the Project

1. **Clone the repository** or download the source files.
2. Place your target CSV file (e.g., `NVDA5y.csv`) into the application root directory.
3. Build and run the project using the .NET CLI:

```bash
dotnet run
```

---

## ⚙️ How It Works

### 1. Pattern Detection Logic

The detector evaluates sliding windows of price data (e.g., 20 candles).
1. Ensures positive slope normalized percentage check ($> 0.01\%$).
2. Identifies 3-bar swing pivots:
   * **Swing Low**: Low[i] < Low[i-1] and Low[i] < Low[i+1]
   * **Swing High**: High[i] > High[i-1] and High[i] > High[i+1]
3. Scans backwards using a state machine to locate valid $(A, B, C)$ sequences.
4. Calculates entry point $E$ on the open of candle C + 1 and target price D.

### 2. Backtest Strategy Execution

* **Position Size**: $1,000 fixed position allocation.
* **Initial Stop Loss**: 0.5% below Point C (0.995 * Price C).
* **Trailing Stop**: Trailing stop ratchets up by 2.5% below the highest high reached post-entry.

---

## 📊 Sample Output

Running the program produces console statistics and writes an account balance series to `balance1.csv`:

```text
NVDA
ABC Count 14
Strategy One (Trailing Stop 2.5%)
Total Trades:      14
Winning Trades:    10
Losing Trades:     4
Win Rate:          71.43%
Target D Hits:     9
Total Profit:      $342.50
Max Drawdown:      $45.10
```

---

## 📄 License

This project is open source and available under the MIT License.