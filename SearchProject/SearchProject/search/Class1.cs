using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace search
{
    public class Class1
    {
        static string cxGoogle = "8afe0bf843a67cb76";
        static string apiKeyGoogle = "AIzaSyAkbZGMAsw2RqaJpd7Na9u6cW3HDJ6OkF8";
        static string cxMSN = "03a6560bb769ede79";
        static string apiKeyMSN = "AIzaSyDJwPTiyg_71CkwUb4otJ4mMKzlGlW2q0U";

        public static void Main(string[] args)
        {
            string searchQuery = string.Empty;
            string total = string.Empty;
            string engine = string.Empty;

            if (args.Count() == 0)
            {
                do
                {
                    Console.WriteLine("Type a text for search separated by space, and then press Enter");
                    searchQuery = Convert.ToString(Console.ReadLine());
                    Console.WriteLine("\n");

                } while (searchQuery == String.Empty);

                char[] delimiterChars = { ' ' };

                if (searchQuery.Contains(" "))
                {
                    string[] aux = searchQuery.Split('"');
                    List<string> tokens = new List<string>();
                    for (int i = 0; i < aux.Length; ++i)
                        if (i % 2 == 0)
                            tokens.AddRange(aux[i].Split(delimiterChars));
                        else
                            tokens.Add(aux[i]);

                    List<string> tokensClear = new List<string>();
                    foreach (var x in tokens) { 
                        if (x != string.Empty)
                                tokensClear.Add(x);
                    }
                    args = tokensClear.ToArray();
                }
                else
                {
                    WebSearch(cxGoogle, apiKeyGoogle, searchQuery, out total, out engine);
                    WebSearch(cxMSN, apiKeyMSN, searchQuery, out total, out engine);
                }
            }

            List<itemTotal> listItemTotal = new List<itemTotal>();

            foreach (var word in args)
            {
                itemTotal itemTotalGoogle = new itemTotal();
                itemTotal itemTotalMSN = new itemTotal();

                WebSearch(cxGoogle, apiKeyGoogle, word, out total, out engine);
                itemTotalGoogle.searchQuery = word;
                itemTotalGoogle.engine = engine;
                itemTotalGoogle.totalResults = Convert.ToInt32(total);
                listItemTotal.Add(itemTotalGoogle);

                WebSearch(cxMSN, apiKeyMSN, word, out total, out engine);
                itemTotalMSN.searchQuery = word;
                itemTotalMSN.engine = engine;
                itemTotalMSN.totalResults = Convert.ToInt32(total);
                listItemTotal.Add(itemTotalMSN);
            }

            //winnerByEngine
            Console.WriteLine("\n");
            var listByEngineDistinct = listItemTotal.Select(x => x.engine).Distinct().ToList();
            foreach (var engineDistinct in listByEngineDistinct)
            {
                var winnerByEngineList = (from r in listItemTotal
                                          where r.engine == engineDistinct
                                          orderby r.totalResults descending
                                          select r).ToList().FirstOrDefault();

                Console.WriteLine("{0} winner: {1}", engineDistinct, winnerByEngineList.searchQuery);
            }

            //winnerByTotalSearchQuery
            if (listItemTotal.Count > 0)
            {
                Console.WriteLine("\n");

                var teamTotalScores = (
                    from listItem in listItemTotal
                    orderby listItem.totalResults descending
                    group listItem by listItem.searchQuery into searchQueryGroup
                    select new
                    {
                        Key = searchQueryGroup.Key,
                        TotalScore = searchQueryGroup.Sum(x => x.totalResults),
                    }).ToList().FirstOrDefault();

                Console.WriteLine("Total winner: {0}", teamTotalScores.Key);
            }
            Console.Read();
        }

        public static void WebSearch(string cx, string apiKey, string searchQuery, out string totalResults, out string engine)
        {
            var request = WebRequest.Create("https://www.googleapis.com/customsearch/v1?key=" + apiKey + "&cx=" + cx + "&q=" + searchQuery);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseString = reader.ReadToEnd();

            JObject jResponse = JObject.Parse(responseString);

            JToken jContext = jResponse["context"];
            engine = (string)jContext["title"];
            JToken jSearchInformation = jResponse["searchInformation"];
            totalResults = (string)jSearchInformation["totalResults"];

            Console.WriteLine("{0}, {1}, totalResults {2}", searchQuery, engine, totalResults);
            //Console.WriteLine("jResponse {0}", jResponse);
        }

        public class itemTotal
        {
            public string searchQuery { get; set; }
            public string engine { get; set; }
            public int totalResults { get; set; }
        }

    }

}

