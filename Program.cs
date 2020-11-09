using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace СocktailParser
{
    class Program
    {
        [DataContract]
        class CocktailsData
        {
            [DataMember]
            internal string Name { get; set; }
            [DataMember]
            internal List<string> Tags { get; set; }
            [DataMember]
            internal List<(string,int)> Ingridients { get; set; }
            [DataMember]
            internal List<string> Tools { get; set; }
            [DataMember]
            internal string Recipe { get; set; }
            [DataMember]
            internal string Description { get; set; }
            public CocktailsData()
            {
                Ingridients = new List<(string, int)>();
                Tools = new List<string>();
                Tags = new List<string>();
            }
            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }
            public override bool Equals(object obj)
            {
                if (obj.GetType() != this.GetType()) return false;

                CocktailsData cocktail = (CocktailsData)obj;
                return (Name == cocktail.Name && Tags.SequenceEqual(cocktail.Tags) && Tools.SequenceEqual(cocktail.Tools) && Recipe == cocktail.Recipe && Description == cocktail.Description && Ingridients.SequenceEqual(cocktail.Ingridients));
            }
        }
        static string fromFile = $"CocktailsBackup.json";
        static string toFile = $"Cocktails.json";
        static void Main(string[] args)
        {
            ChromeDriver driver = new ChromeDriver();
            List<CocktailsData> cocktails = new List<CocktailsData>();
            int count = 0;
            driver.Navigate().GoToUrl("https://ru.inshaker.com/cocktails");
            driver.Manage().Window.Maximize();
            Thread.Sleep(500);
            List<string> filter = ImportAlreadySavedCocktails();
            if (filter.Count != 0)
                filter = filter.Distinct().ToList();
            while (true)
            {
                count++;
                var c = driver.FindElementByCssSelector($":nth-child({count})>.cocktail-item-preview");
                if (filter != null)
                    if (!filter.Contains(c.FindElement(By.ClassName("cocktail-item-name")).Text))
                    {
                        Thread.Sleep(500);
                        c.Click();
                        Thread.Sleep(200);
                        CocktailsData cocktail;
                        while (true)
                        {
                            try
                            {
                                cocktail = ParseCoctailInfo(driver);
                                break;
                            }
                            catch
                            {
                                continue;
                            }
                        }
                        cocktails.Add(cocktail);
                        driver.Navigate().Back();
                        driver.Navigate().Refresh();
                        Thread.Sleep(100);
                    }
                if(count % 20 == 0)
                {
                    SaveCocktailsToFile(cocktails);
                    filter = ImportAlreadySavedCocktails();
                    driver.Navigate().GoToUrl(driver.FindElement(By.XPath(".//*[@class=\"common-more common-list-state\"]")).GetAttribute("href"));
                    Thread.Sleep(200);
                }
            }
        }
        static CocktailsData ParseCoctailInfo(ChromeDriver driver)
        {
            var element = driver.FindElement(By.CssSelector(".common-title"));
            var cocktail = new CocktailsData();
            cocktail.Name = element.FindElement(By.CssSelector(".common-name")).Text;
            var tags = element.FindElement(By.CssSelector(".tags")).FindElements(By.ClassName("item"));
            foreach (IWebElement el in tags)
            {
                cocktail.Tags.Add(el.FindElement(By.ClassName("tag")).Text);
            }
            var ingridientsTable = driver.FindElement(By.XPath(".//*[@class=\"ingredient-tables\"]/table[1]/tbody"));
            var ingridientsNames = ingridientsTable.FindElements(By.XPath(".//a"));
            var ingridientsVolumes = ingridientsTable.FindElements(By.ClassName("amount"));
            cocktail.Ingridients.AddRange(ingridientsNames.Zip(ingridientsVolumes,(n,v)=>(n.Text,Convert.ToInt32(v.Text))));
            var toolsTable = driver.FindElement(By.XPath(".//*[@class=\"ingredient-tables\"]/table[2]/tbody"));
            var toolsNames = toolsTable.FindElements(By.XPath(".//a"));
            cocktail.Tools.AddRange(toolsNames.Select(n => n.Text));
            var recipe = driver.FindElementsByXPath(".//*[@class=\"steps\"]//li");
            cocktail.Recipe = recipe.Select(r => r.Text).Aggregate((partialPhrase, word) => $"{partialPhrase}\n {word}");
            try
            {
                cocktail.Description = driver.FindElement(By.XPath("//*[@class=\"body\"]//p")).Text;
            }
            catch (Exception)
            {
                try
                {
                    cocktail.Description = driver.FindElement(By.XPath("//*[@class=\"body\"]")).Text;
                }
                catch (Exception)
                {
                    cocktail.Description = "";
                }
            }
            var imageURL = driver.FindElementByCssSelector(".image").GetAttribute("src");
            var web = new WebClient();
            var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Images\\{Regex.Replace(Regex.Replace(cocktail.Name, "\"", ""), @"\?", "")}.jpg");
            web.DownloadFile(imageURL, imagePath);
            web.Dispose();
            return cocktail;
        }
        static List<string> ImportAlreadySavedCocktails()
        {
            if (!File.Exists(fromFile)) return new List<string>();
            List<string> names;
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(CocktailsData[]));
            using (StreamReader stream = new StreamReader(fromFile, System.Text.Encoding.Default))
            {
                CocktailsData[] p = jsonFormatter.ReadObject(stream.BaseStream) as CocktailsData[];
                names = p.Select(c => c.Name).ToList();
            }
            return names;
        }
        static void SaveCocktailsToFile(List<CocktailsData> cocktails)
        {
            if (cocktails.Count == 0) return;
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(CocktailsData[]));
            List<CocktailsData> toWrite = new List<CocktailsData>();
            if (File.Exists(fromFile))
                using (StreamReader stream = new StreamReader(fromFile, System.Text.Encoding.Default))
                {
                    toWrite = (jsonFormatter.ReadObject(stream.BaseStream) as CocktailsData[]).ToList();
                }
            else
                File.Create(fromFile).Close();
            toWrite = toWrite.Union(cocktails).ToList();
            using (FileStream stream = new FileStream(toFile, FileMode.Create))
            {
                jsonFormatter.WriteObject(stream, toWrite.ToArray());
            }
            if (File.Exists(fromFile))
                File.Delete(fromFile);
            File.Copy(toFile, fromFile);
        }
        static Image ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return (Image)destImage;
        }
        static (int height, int width) CalculateNewSize(Image image)
        {
            int scope = image.Height / 100;
            return (100, image.Width / scope);
        }
    }
}
