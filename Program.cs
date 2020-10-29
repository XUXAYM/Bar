using OpenQA.Selenium.Chrome;
using System;

namespace СocktailParser
{
    class Program
    {
        static void Main(string[] args)
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments(@"--user-data-dir=C:\Users\Maximus\AppData\Local\Google\Chrome\User Data");
            ChromeDriver driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl("https://ru.inshaker.com/cocktails");
        }
    }
}
