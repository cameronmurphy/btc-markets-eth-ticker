using System;
using System.IO;
using System.Net;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace Camurphy.BtcMarketsEthTicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string BtcMarketsPriceApiUrl = "https://btcmarkets.net/data/market/BTCMarkets/ETH/AUD/tick";
        private const int LastPriceDivisor = 100000000;
        private readonly DispatcherTimer _refreshTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            _refreshTimer.Interval = TimeSpan.FromTicks(1);
            _refreshTimer.Tick += RefreshTimer_TickHandler;
            _refreshTimer.Start();
        }

        /// <summary>
        /// Enables dragging by windows contents
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void RefreshTimer_TickHandler(object sender, EventArgs e)
        {
            if (_refreshTimer.Interval == TimeSpan.FromTicks(1))
            {
                _refreshTimer.Interval = TimeSpan.FromMinutes(1);
            }

            var lastPrice = FetchLastPrice();
            var lastPriceInt = long.MinValue;

            if (long.TryParse(lastPrice, out lastPriceInt))
            {
                var lastPriceFloat = lastPriceInt / (float) LastPriceDivisor;
                lastPrice = lastPriceFloat.ToString("C");
            }

            PriceTextBlock.Text = lastPrice;
        }

        private static string FetchLastPrice()
        {
            using (var client = new WebClient())
            {
                var content = client.DownloadString(BtcMarketsPriceApiUrl);

                using (var reader = new JsonTextReader(new StringReader(content)))
                {
                    var lastPriceFound = false;

                    while (reader.Read())
                    {
                        if (lastPriceFound && reader.TokenType == JsonToken.Integer)
                        {
                            return reader.Value.ToString();
                        }

                        if (reader.TokenType == JsonToken.PropertyName && reader.Value.ToString() == "lastPrice")
                        {
                            lastPriceFound = true;
                        }
                    }
                }
            }

            return "Error";
        }
    }
}

