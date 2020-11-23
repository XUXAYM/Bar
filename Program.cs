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
        static readonly string fromCocktailsFile = $"CocktailsBackup.json";
        static readonly string toCocktailsFile = $"Cocktails.json";
        static readonly string fromToolsFile = $"ToolsBackup.json";
        static readonly string toToolsFile = $"Tools.json";
        static readonly string fromIngredientsFile = $"IngredientsBackup.json";
        static readonly string toIngredientsFile = $"Ingredients.json";
        static bool exit = false;
        static void Main(string[] args)
        {

            while (exit == false)
            {
                Console.WriteLine("1) Парсинг коктейлей;\n2) Парсинг инструментов;\n3) Парсинг ингридиентов;\n0) Выход.");
                switch (Console.ReadLine())
                {
                    case "1":
                        {
                            ChromeOptions options = new ChromeOptions();
                            options.AddArgument("--ignore-certificate-errors");
                            options.AddArgument("--ignore-ssl-errors");
                            ChromeDriver driver = new ChromeDriver(options);
                            ParseAllCocktails(driver);
                            break;
                        }
                    case "2":
                        {
                            ChromeOptions options = new ChromeOptions();
                            options.AddArgument("--ignore-certificate-errors");
                            options.AddArgument("--ignore-ssl-errors");
                            ChromeDriver driver = new ChromeDriver(options);
                            ParseAllTools(driver);
                            break;
                        }
                    case "3":
                        {
                            ChromeOptions options = new ChromeOptions();
                            options.AddArgument("--ignore-certificate-errors");
                            options.AddArgument("--ignore-ssl-errors");
                            ChromeDriver driver = new ChromeDriver(options);
                            ParseAllIngredients(driver);
                            break;
                        }
                    case "0":
                        {
                            exit = true;
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Неизвестная команда, попробуйте снова.");
                            break;
                        }
                }
            }
        }
        #region IngredientsMethods
        static void ParseAllIngredients(ChromeDriver driver)
        {
            List<IngredientsData> ingredients = new List<IngredientsData>();
            int startTableIndex = 1;
            int stopTableIndex = 31;
            int count = 0;
            driver.Navigate().GoToUrl("https://ru.inshaker.com/goods");
            driver.Manage().Window.Maximize();
            Thread.Sleep(500);
            List<string> filter = ImportAlreadySavedIngredients();
            if (filter.Count != 0)
                filter = filter.Distinct().ToList();
            while (true)
            {
                IngredientsData ingredient;
                count++;
                if (filter != null)
                {
                    string text;
                    try
                    {
                        if (startTableIndex > stopTableIndex)
                        {
                            SaveIngredientsToFile(ingredients);
                            filter = ImportAlreadySavedIngredients();
                            break;
                        }
                        text = driver.FindElementByCssSelector($":nth-child({startTableIndex}) > .body > .list > :nth-child({count}) > .common-good-icon > .name").Text;
                    }
                    catch 
                    {
                        SaveIngredientsToFile(ingredients);
                        filter = ImportAlreadySavedIngredients();
                        startTableIndex++;
                        count = 0;
                        continue;
                    }
                    if (!filter.Contains(text))
                    {
                        driver.Navigate().GoToUrl(driver.FindElementByCssSelector($":nth-child({startTableIndex}) > .body > .list > :nth-child({count}) > .common-good-icon").GetAttribute("href"));
                        //while (true)
                        //{
                        //try
                        //{
                        ingredient = ParseIngredientInfo(driver);
                        //    break;
                        //}
                        //catch
                        //{
                        //    continue;
                        //    //}
                        //}
                        ingredients.Add(ingredient);
                        driver.Navigate().Back();
                        driver.Navigate().Refresh();
                    }
                }
                Thread.Sleep(100);
            }
        }
        static IngredientsData ParseIngredientInfo(ChromeDriver driver)
        {
            var name = driver.FindElementByCssSelector(".common-name").Text;
            var description = driver.FindElementById("goods-text").Text;
            var imageURL = driver.FindElementByCssSelector(".common-image-frame").GetAttribute("style");
            imageURL = new string(imageURL.SkipWhile(s => !s.Equals('"')).ToArray()).Replace("\"", "");
            imageURL = new string(imageURL.TakeWhile(s => !s.Equals(')')).ToArray());
            imageURL = "https://ru.inshaker.com" + imageURL;
            var web = new WebClient();
            var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"IngredientsImages\\{Regex.Replace(name, "\"", "")}.jpg");
            web.DownloadFile(imageURL, imagePath);
            web.Dispose();
            return new IngredientsData() { Description = description, Name = name, Degree = 0 };
        }
        static List<string> ImportAlreadySavedIngredients()
        {
            if (!File.Exists(fromIngredientsFile)) return new List<string>();
            List<string> names;
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(IngredientsData[]));
            using (StreamReader stream = new StreamReader(fromToolsFile, System.Text.Encoding.Default))
            {
                IngredientsData[] p = jsonFormatter.ReadObject(stream.BaseStream) as IngredientsData[];
                names = p.Select(c => c.Name).ToList();
            }
            return names;
        }
        static void SaveIngredientsToFile(List<IngredientsData> tools)
        {
            if (tools.Count == 0) return;
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(IngredientsData[]));
            List<IngredientsData> toWrite = new List<IngredientsData>();
            if (File.Exists(fromIngredientsFile))
                using (StreamReader stream = new StreamReader(fromIngredientsFile, System.Text.Encoding.Default))
                {
                    toWrite = (jsonFormatter.ReadObject(stream.BaseStream) as IngredientsData[]).ToList();
                }
            else
                File.Create(fromIngredientsFile).Close();
            toWrite = toWrite.Union(tools).ToList();
            using (FileStream stream = new FileStream(toIngredientsFile, FileMode.Create))
            {
                jsonFormatter.WriteObject(stream, toWrite.ToArray());
            }
            if (File.Exists(fromIngredientsFile))
                File.Delete(fromIngredientsFile);
            File.Copy(toIngredientsFile, fromIngredientsFile);
        }
        #endregion
        #region ToolsMethods
        static void ParseAllTools(ChromeDriver driver)
        {
            List<ToolsData> tools = new List<ToolsData>();
            int startTableIndex = 32;
            int count = 0;
            driver.Navigate().GoToUrl("https://ru.inshaker.com/goods");
            driver.Manage().Window.Maximize();
            Thread.Sleep(500);
            List<string> filter = ImportAlreadySavedTools();
            if (filter.Count != 0)
                filter = filter.Distinct().ToList();
            while (true)
            {
                ToolsData tool;
                count++;
                if (filter != null){
                    string text;
                    try
                    {
                        text = driver.FindElementByCssSelector($":nth-child({startTableIndex}) > .body > .list > :nth-child({count}) > .common-good-icon > .name").Text;
                        //text = driver.FindElementByCssSelector($":nth-child(34) > .body > .list > :nth-child(1) > .common-good-icon > .name").Text;
                    }
                    catch(NoSuchElementException ex)
                    {
                        if (startTableIndex > 50)
                        {
                            SaveToolsToFile(tools);
                            filter = ImportAlreadySavedTools();
                            break;
                        }
                        startTableIndex++;
                        count=0;
                        continue;
                    }
                    if (!filter.Contains(text))
                    {
                        driver.Navigate().GoToUrl(driver.FindElementByCssSelector($":nth-child({startTableIndex}) > .body > .list > :nth-child({count}) > .common-good-icon").GetAttribute("href"));
                        //while (true)
                        //{
                            //try
                            //{
                                tool = ParseToolInfo(driver);
                            //    break;
                            //}
                            //catch
                            //{
                            //    continue;
                        //    //}
                        //}
                        tools.Add(tool); 
                        driver.Navigate().Back();
                        driver.Navigate().Refresh();
                    }
                }
                Thread.Sleep(100);
            }
        }
        static ToolsData ParseToolInfo(ChromeDriver driver)
        {
            var name = driver.FindElementByCssSelector(".common-name").Text;
            var description = driver.FindElementById("goods-text").Text;
            var imageURL = driver.FindElementByCssSelector(".common-image-frame").GetAttribute("style");
            imageURL = new string(imageURL.SkipWhile(s => !s.Equals('"')).ToArray()).Replace("\"","");         
            imageURL = new string(imageURL.TakeWhile(s => !s.Equals(')')).ToArray());
            imageURL = "https://ru.inshaker.com" + imageURL;
            var web = new WebClient();
            var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"ToolsImages\\{name}.jpg");
            web.DownloadFile(imageURL, imagePath);
            web.Dispose();
            return new ToolsData() { Description = description, Name = name };
        }
        static List<string> ImportAlreadySavedTools()
        {
            if (!File.Exists(fromToolsFile)) return new List<string>();
            List<string> names;
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(ToolsData[]));
            using (StreamReader stream = new StreamReader(fromToolsFile, System.Text.Encoding.Default))
            {
                ToolsData[] p = jsonFormatter.ReadObject(stream.BaseStream) as ToolsData[];
                names = p.Select(c => c.Name).ToList();
            }
            return names;
        }
        static void SaveToolsToFile(List<ToolsData> tools)
        {
            if (tools.Count == 0) return;
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(ToolsData[]));
            List<ToolsData> toWrite = new List<ToolsData>();
            if (File.Exists(fromToolsFile))
                using (StreamReader stream = new StreamReader(fromToolsFile, System.Text.Encoding.Default))
                {
                    toWrite = (jsonFormatter.ReadObject(stream.BaseStream) as ToolsData[]).ToList();
                }
            else
                File.Create(fromToolsFile).Close();
            toWrite = toWrite.Union(tools).ToList();
            using (FileStream stream = new FileStream(toToolsFile, FileMode.Create))
            {
                jsonFormatter.WriteObject(stream, toWrite.ToArray());
            }
            if (File.Exists(fromToolsFile))
                File.Delete(fromToolsFile);
            File.Copy(toToolsFile, fromToolsFile);
        }
        #endregion
        #region CoctailsMethods
        static void ParseAllCocktails(ChromeDriver driver)
        {
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
                if (count % 20 == 0)
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
            var ingredientsTable = driver.FindElement(By.XPath(".//*[@class=\"ingredient-tables\"]/table[1]/tbody"));
            var ingredientsNames = ingredientsTable.FindElements(By.XPath(".//a"));
            var ingredientsVolumes = ingredientsTable.FindElements(By.ClassName("amount"));
            cocktail.Ingredients.AddRange(ingredientsNames.Zip(ingredientsVolumes,(n,v)=>(n.Text,Convert.ToInt32(v.Text))));
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
            if (!File.Exists(fromCocktailsFile)) return new List<string>();
            List<string> names;
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(CocktailsData[]));
            using (StreamReader stream = new StreamReader(fromCocktailsFile, System.Text.Encoding.Default))
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
            if (File.Exists(fromCocktailsFile))
                using (StreamReader stream = new StreamReader(fromCocktailsFile, System.Text.Encoding.Default))
                {
                    toWrite = (jsonFormatter.ReadObject(stream.BaseStream) as CocktailsData[]).ToList();
                }
            else
                File.Create(fromCocktailsFile).Close();
            toWrite = toWrite.Union(cocktails).ToList();
            using (FileStream stream = new FileStream(toCocktailsFile, FileMode.Create))
            {
                jsonFormatter.WriteObject(stream, toWrite.ToArray());
            }
            if (File.Exists(fromCocktailsFile))
                File.Delete(fromCocktailsFile);
            File.Copy(toCocktailsFile, fromCocktailsFile);
        }
        #endregion
    }
}
