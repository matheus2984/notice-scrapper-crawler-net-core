using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using notice_scrapper_crawler_net_core.models;
using PuppeteerSharp;

namespace notice_scrapper_crawler_net_core
{
    class Program
    {
        static string[] GetLinks(string url)
        {
            var webClient = new WebClient();
            string result = webClient.DownloadString(url);
            var linkParser = new Regex(@"(\b(?:https?:|www\.)\S+\b)(<|"")", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return linkParser.Matches(result)
                        .OfType<Match>()
                        .Select(m => m.Groups[0].Value.Replace("<", "").Replace("\"", ""))
                        .ToHashSet()
                        .ToArray();
        }

        static string Xml2Json(string url)
        {
            var webClient = new WebClient();
            string result = webClient.DownloadString(url);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);

            return JsonConvert.SerializeXmlNode(doc);
        }

        static string GetContent(string url, string tag)
        {
            string content = "";

            Task t = new Task(async () =>
            {
             //   await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
                var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    ExecutablePath = "~/usr/bin/chromium-browser",
                    Args = new[]
                    {
                        "--no-sandbox",
                        "--disable-setuid-sandbox"
                    }
                });
                var page = await browser.NewPageAsync();
                await page.GoToAsync(url);
                var el = "()=>document.querySelector('" + tag + "').innerText";
                content = (string)await page.EvaluateExpressionAsync(el);

            });
            t.RunSynchronously();
            t.Wait();
            return content;
        }

        static void Main(string[] args)
        {
            /*
                https://g1.globo.com/rss/g1/economia/
            */
           // foreach (var link in GetLinks("https://g1.globo.com/rss/g1/economia/"))
             //   Console.WriteLine(link);

            //var json = Xml2Json("https://g1.globo.com/rss/g1/economia/");
           /// System.IO.File.WriteAllText("data.json", json);
           // Console.WriteLine(json);


            Console.WriteLine(
                GetContent(
                    "https://economia.uol.com.br/colunas/carla-araujo/2020/12/21/anvisa-concede-certificacao-de-boas-pratica-para-fabrica-da-coronavac.htm",
                    ".text"));
        }
    }
}
