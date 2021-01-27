using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GrowbrewProxy
{
    class PriceChecker
    {
        public const int SUPPORTED_COLUMNS = 3; // Increase this if there are more values available in the future.
        public struct ItemPrice
        {
            public string name { get; set; }
            public int quantity { get; set; }
            public int price { get; set; }

            public ItemPrice(string name, int quantity, int price)
            {
                this.name = name;
                this.quantity = quantity;
                this.price = price;
            }
        }

        public class ItemPriceList
        {
            public List<ItemPrice> itemPrices = new List<ItemPrice>();

            public string Serialize()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(GetCount().ToString());

                foreach (ItemPrice iprice in itemPrices)
                {
                    sb.AppendJoin('|', iprice.name, iprice.quantity, iprice.price);
                    sb.AppendLine();
                }

                return sb.ToString();
            }

            public static ItemPriceList Deserialize(string rawText)
            {
                ItemPriceList iPriceList = new ItemPriceList();
                if (rawText != "")
                {
                    
                    string[] lines = rawText.Split("\n");

                    if (lines.Length > 0)
                    {
                        int count = 0;
                        if (!int.TryParse(lines[0], out count))
                            throw new Exception("Unable to serialize due to failure of retrieving available item count");

                        foreach (string line in lines)
                        {
                            string[] values = line.Split('|');

                            if (values.Length == SUPPORTED_COLUMNS)
                                iPriceList.Add(new ItemPrice(values[0], int.Parse(values[1]), int.Parse(values[2])));
                        }
                    }
                }
                
                return iPriceList;
            }

            public ItemPrice[] FindByName(string name, bool exact = false)
            {
                List<ItemPrice> iprices = new List<ItemPrice>();
                foreach (ItemPrice iprice in itemPrices)
                {
                    bool needAdd = exact ? name == iprice.name : iprice.name.StartsWith(name);
                    if (needAdd)
                    {
                        iprices.Add(iprice);
                        if (exact) break; // optimize
                    }
                }
                return iprices.ToArray();
            }

            public ItemPrice[] FindByNameIgnoreCase(string name, bool exact = false)
            {
                List<ItemPrice> iprices = new List<ItemPrice>();

                string pNameLower = name.ToLower();
                foreach (ItemPrice iprice in itemPrices)
                {
                    bool needAdd = exact ? name == iprice.name : iprice.name.StartsWith(name);
                    if (needAdd)
                    {
                        iprices.Add(iprice);
                        if (exact) break; // optimize
                    }
                }
                return iprices.ToArray();
            }

            public int GetCount() => itemPrices.Count;
            public void Add(ItemPrice item) => itemPrices.Add(item);

            public bool Remove(ItemPrice item) => itemPrices.Remove(item);
        }

        public static ItemPriceList iPriceList = null; // cache

        public static string RefreshPrices(string url) // Refresh only when it's needed, it's wasteful or laggy to do it everytime, so items are cached.
        {
            string content = "";
            using (var wc = new WebClient())
            {
                try
                {
                    content = wc.DownloadString(url);
                }
                catch (WebException)
                {
                    return content;
                    // if its a different exception it should rather crash for the developer to debug it
                }
            }
            return content;
        }

        public static void SetHTTPS()
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        public static ItemPriceList GetItemPriceListFromUrl(string url) // retrieves the full item price list
        {
            if (iPriceList == null) // Not in cache? Rebuild then, user has to do it's own implementation to choose when to refresh items, perhaps just a resync button?
            {
                string raw = RefreshPrices(url);
                iPriceList = ItemPriceList.Deserialize(raw);
                return iPriceList;
            }
            else if (iPriceList.GetCount() > 0) // the count shall not be zero either, however iPriceList wasn't null which it should so it's gonna throw an error.
            {
                return iPriceList; // return immediately.
            }
            
            throw new Exception($"GetItemPriceListFromUrl({url}) failed for an unknown reason.");
        }
    }
}
