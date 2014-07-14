using System;
using FluentAssertions;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace SeleniumExamples
{
    [TestFixture]
    public class SeleniumTests
    {
        private IWebDriver driver;

        [SetUp]
        public void TestSetup()
        {
            driver = new FirefoxDriver(new FirefoxProfile() { EnableNativeEvents = false });
            //driver = new ChromeDriver();
            driver.Navigate().GoToUrl("http://ca.yahoo.com/");
        }

        [TearDown]
        public void TestTeardown()
        {
            driver.Quit();
        }

        [Test]
        public void SearchForCheeseOnYahoo()
        {
            IWebElement searchbox = driver.FindElement(By.Name("p"));
            searchbox.SendKeys("cheese");
            searchbox.Submit();

            var firstResult = driver.FindElement(By.Id("link-1"));
            firstResult.Text.Should().StartWith("Cheese");
        }

        [Test]
        public void MakeSureThereIs10Results()
        {
            IWebElement searchbox = driver.FindElement(By.Name("p"));
            searchbox.SendKeys("cheese");
            searchbox.Submit();

            var searchResultContainer = driver.FindElement(By.TagName("ol"));
            var searchResults = searchResultContainer.FindElements(By.TagName("li"));
            searchResults.Count.Should().Be(10);
        }

        [Test]
        public void ExecuteJavascript()
        {
            IWebElement searchbox = driver.FindElement(By.Name("p"));
            searchbox.SendKeys("cheese");
            searchbox.Submit();

            ((IJavaScriptExecutor) driver).ExecuteScript("document.getElementById('link-1').innerHTML = 'I like cheese';");

            var firstResult = driver.FindElement(By.Id("link-1"));
            firstResult.Text.Should().Be("I like cheese");
        }

        [Test]
        public void WaitForSuggestionsToShowUp()
        {
            IWebElement searchbox = driver.FindElement(By.Name("p"));
            searchbox.SendKeys("cheese");

            //var suggestions = driver.FindElement(By.ClassName("sa-holder"));

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var suggestions = wait.Until(d => d.FindElement(By.ClassName("sa-holder")));

            suggestions.FindElements(By.TagName("li")).Count.Should().Be(10);
        }

        [Test]
        public void TakingAction()
        {
            driver.Navigate().GoToUrl("http://www.w3schools.com/tags/tryit.asp?filename=tryhtml_select_multiple");
            driver.SwitchTo().Frame(driver.FindElement(By.Id("iframeResult")));

            var selectElement = new SelectElement(driver.FindElement(By.Name("cars")));
            var action = new Actions(driver);
            action.Click(selectElement.Options[0])
                  .KeyDown(Keys.Control)
                  .Click(selectElement.Options[2])
                  .KeyUp(Keys.Control)
                  .Perform();

            selectElement.AllSelectedOptions.Count.Should().Be(2);
        }

        [Test]
        public void UsingAPageFactory()
        {
            var homepage = new YahooHomepage();
            PageFactory.InitElements(driver, homepage);

            homepage.SearchBox.SendKeys("cheese");
            homepage.SearchButton.Click();
        }
    }

    public class YahooHomepage
    {
        [FindsBy(Using="p",How=How.Name)]
        public IWebElement SearchBox { get; set; }

        [FindsBy(Using="search-submit")]
        public IWebElement SearchButton { get; set; }
    }
}
