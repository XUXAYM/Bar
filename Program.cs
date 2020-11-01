using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
            internal List<string> Recipe { get; set; }
            [DataMember]
            internal string Description { get; set; }
            public CocktailsData()
            {
                Ingridients = new List<(string, int)>();
                Tools = new List<string>();
                Recipe = new List<string>();
                Tags = new List<string>();
            }
        }
        static void Main(string[] args)
        {
            ChromeOptions options = new ChromeOptions();
            //options.AddArguments(@"--user-data-dir=C:\Users\Maximus\AppData\Local\Google\Chrome\User Data");
            ChromeDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://ru.inshaker.com/cocktails");
            //var element = driver.FindElementByClassName("search-results common-box cocktail-grid");
            var element = driver.FindElementByCssSelector(".search-results");
            var coctailsPreview = element.FindElements(By.CssSelector(".cocktail-item-preview"));
            Thread.Sleep(3000);
            coctailsPreview[0].Click();
            Thread.Sleep(500);
            CocktailsData cocktail = ParseCoctailInfo(driver);
            driver.Navigate().Back();
            Console.WriteLine(cocktail.Name);
            Console.ReadKey();
        }

        static void GoToNewTab(ChromeDriver driver, string url)
        {
            driver.FindElement(By.CssSelector("body")).SendKeys(Keys.Control + "t");
            string newTabInstance = driver.WindowHandles[driver.WindowHandles.Count - 1].ToString();
            driver.SwitchTo().Window(newTabInstance);
            driver.Navigate().GoToUrl(url);
        }
        static CocktailsData ParseCoctailInfo(ChromeDriver driver)
        {
            Thread.Sleep(200);
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
            //cocktail.Ingridients.AddRange(from n in ingridientsNames from v in ingridientsVolumes select (n.Text, Convert.ToInt32(v.Text)));
            cocktail.Ingridients.AddRange(ingridientsNames.Zip(ingridientsVolumes,(n,v)=>(n.Text,Convert.ToInt32(v.Text))));
            var toolsTable = driver.FindElement(By.XPath(".//*[@class=\"ingredient-tables\"]/table[2]/tbody"));
            var toolsNames = toolsTable.FindElements(By.XPath(".//a"));
            cocktail.Tools.AddRange(toolsNames.Select(n => n.Text));
            var recipe = driver.FindElementsByXPath(".//*[@class=\"steps\"]//li");
            cocktail.Recipe.AddRange(recipe.Select(r=>r.Text));
            cocktail.Description = driver.FindElement(By.XPath(".//*[@class=\"body\"]//p")).Text;
            return cocktail;
        }
    }
}
