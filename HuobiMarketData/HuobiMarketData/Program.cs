using HuoBiApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HuobiMarketData
{
    class Program
    {
        static Dictionary<string, TickData> tickDatas;
        static Thread thread;
        static void Main(string[] args)
        {
            tickDatas = new Dictionary<string, TickData>();
            HuobiMarket market = new HuobiMarket();
            market.OnMessage += TickUpdate;
            market.OnConnecteed += HBConnected;
            string url = "wss://api.huobipro.com/ws";
            try
            {
                market.Init(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            List<string> symbols = new List<string>() { "btcusdt", "eosbtc", "eosusdt" };
            foreach (var item in symbols)
            {
                tickDatas[item] = new TickData();
                string str = "market." + item +".depth.step0";
                market.Subscribe(str, "33233011");
            }

            //thread = new Thread(new ThreadStart(RecordeDatas));


            Console.ReadLine();
        }

        private static void RecordeDatas()
        {
            foreach (var item in tickDatas)
            {
                string currentpath = Environment.CurrentDirectory + @"\" + item + ".txt";
                using (StreamWriter sw = new StreamWriter(currentpath, true))
                {
                    sw.WriteLine()
                }
            }
        }

        static void HBConnected(object sender, ConnectedEventArgs args)
        {
            if (args.isConnected)
                Console.WriteLine("Connected True");
            else
                Console.WriteLine("Connected False");
        }

        static private void TickUpdate(object sender, HuoBiMessageReceivedEventArgs args)
        {
            var msg = args.Message;
            try
            {
                if (msg.Contains("bid"))
                {
                    HuoBiTick tick;
                    int tickCount = 0;
                    JObject jo = (JObject)JsonConvert.DeserializeObject(msg);
                    string symbol = jo["ch"].ToString().Split('.')[1];
                    tick = JsonConvert.DeserializeObject<HuoBiTick>(jo["tick"].ToString());

                    if (!tickDatas.TryGetValue(symbol, out TickData quote))
                        quote = new TickData();

                    List<string> list = msg.Split(':').ToList();
                    List<string> listSymbol = list[1].Split('.').ToList();

                    quote.UpdateTime = DateTime.Now;
                    quote.Exchange = "HB";
                    quote.BestBid = (decimal)tick.bids[0][0];
                    quote.BestBidQuantity = (decimal)tick.bids[0][1];
                    quote.BestAsk = (decimal)tick.asks[0][0];
                    quote.BestAskQuantity = (decimal)tick.asks[0][1];

                    if (tick.asks.Count > 10)
                        tickCount = 10;
                    else
                        tickCount = tick.asks.Count;
                    quote.AskPriceQueue = new decimal[tickCount];
                    quote.AskQuantityQueue = new decimal[tickCount];
                    quote.AskPriceQueueCount = tickCount;
                    quote.AskQuantityQueueCount = tickCount;
                    quote.BidPriceQueue = new decimal[tickCount];
                    quote.BidQuantityQueue = new decimal[tickCount];
                    quote.BidPriceQueueCount = tickCount;
                    quote.BidQuantityQueueCount = tickCount;
                    for (int i = 0; i < tickCount; i++)
                    {
                        quote.AskPriceQueue[i] = (decimal)tick.asks[i][0];
                        quote.AskQuantityQueue[i] = (decimal)tick.asks[i][1];
                        quote.BidPriceQueue[i] = (decimal)tick.bids[i][0];
                        quote.BidQuantityQueue[i] = (decimal)tick.bids[i][1];
                    }

                    //var instrument = instrumentList.Where(x => x.Symbol == symbolDict[listSymbol[1]]).FirstOrDefault();
                    quote.InstrumentId = 0;
                    quote.Symbol = symbol;

                    tickDatas[symbol] = quote;

                    Console.WriteLine("TickData > Symbol : " + quote.Symbol + ", BestBid : " + quote.BestBid + ", BestAsk : " + quote.BestAsk);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }



    class TickData
    {
        public decimal[] AskPriceQueue { get; set; }
        public int AskPriceQueueCount { get; set; }
        public decimal[] AskQuantityQueue { get; set; }
        public int AskQuantityQueueCount { get; set; }
        public decimal BestAsk { get; set; }
        public decimal BestAskQuantity { get; set; }
        public decimal BestBid { get; set; }
        public decimal BestBidQuantity { get; set; }
        public decimal[] BidPriceQueue { get; set; }
        public int BidPriceQueueCount { get; set; }
        public decimal[] BidQuantityQueue { get; set; }
        public int BidQuantityQueueCount { get; set; }
        public double Close { get; set; }
        public decimal CumulativeVolume { get; set; }
        public string Exchange { get; set; }
        public decimal High { get; set; }
        public double IEP { get; set; }
        public decimal IEV { get; set; }
        public int InstrumentId { get; set; }
        public decimal Last { get; set; }
        public decimal LastTradedVolume { get; set; }
        public decimal Low { get; set; }
        public double LowerLimit { get; set; }
        public double Nominal { get; set; }
        public double Open { get; set; }
        public string Symbol { get; set; }
        public DateTime UpdateTime { get; set; }
        public double UpperLimit { get; set; }
    }
}
