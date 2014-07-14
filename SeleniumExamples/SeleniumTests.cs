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
        //all interactions with the browser are done through an interface
        //this way we can swap out implementations without changing code
        private IWebDriver driver;

        [SetUp]
        public void TestSetup()
        {
            //enabling native events so we can execute Actions
            driver = new FirefoxDriver(new FirefoxProfile() { EnableNativeEvents = false });

            //testing with a different browser is as easy as specifying a different implementation of IWebDriver
            //driver = new ChromeDriver();

            //tell the browser to go to a specific URL
            driver.Navigate().GoToUrl("http://ca.yahoo.com/");
        }

        [TearDown]
        public void TestTeardown()
        {
            //close the open browser (and any related processes)
            driver.Quit();
        }

        [Test]
        public void SearchForCheeseOnYahoo()
        {
            //most interactions are done by "finding elements" and then doing things to/with them
            //here we're finding a textbox with name="p"
            IWebElement searchbox = driver.FindElement(By.Name("p"));

            //simulate typing 'cheese' into the textbox (or whatever element we found)
            searchbox.SendKeys("cheese");

            //simulate an enter key press
            searchbox.Submit();

            var firstResult = driver.FindElement(By.Id("link-1"));
            //get the current text value of the #link-1 element
            firstResult.Text.Should().StartWith("Cheese");
        }

        [Test]
        public void MakeSureThereIs10Results()
        {
            IWebElement searchbox = driver.FindElement(By.Name("p"));
            searchbox.SendKeys("cheese");
            searchbox.Submit();

            var searchResultContainer = driver.FindElement(By.TagName("ol"));
            //can also select a range of elements - selenium returns an enumerable of all matching elements
            var searchResults = searchResultContainer.FindElements(By.TagName("li"));
            searchResults.Count.Should().Be(10);
        }

        [Test]
        public void ExecuteJavascript()
        {
            IWebElement searchbox = driver.FindElement(By.Name("p"));
            searchbox.SendKeys("cheese");
            searchbox.Submit();

            //let's be super secure and execute an abritrary string of javascript code!
            ((IJavaScriptExecutor) driver).ExecuteScript("document.getElementById('link-1').innerHTML = 'I like cheese';");

            var firstResult = driver.FindElement(By.Id("link-1"));
            firstResult.Text.Should().Be("I like cheese");
        }

        [Test]
        public void WaitForSuggestionsToShowUp()
        {
            IWebElement searchbox = driver.FindElement(By.Name("p"));
            searchbox.SendKeys("cheese");

            //this first FindElement will fail, because it shows up after an ajax call
            //var suggestions = driver.FindElement(By.ClassName("sa-holder"));

            //instead, we tell selenium you're allowed to wait up to 10 seconds for this element to appear
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var suggestions = wait.Until(d => d.FindElement(By.ClassName("sa-holder")));

            //we can also find elements by context - here we're looking for all <li> elements
            //*within* the .sa-holder element (instead of driver, which searches the entire page)
            suggestions.FindElements(By.TagName("li")).Count.Should().Be(10);
        }

        [Test]
        public void TakingAction()
        {
            driver.Navigate().GoToUrl("http://www.w3schools.com/tags/tryit.asp?filename=tryhtml_select_multiple");
            //switch our context to an embedded iFrame on the page - changes the focus of all FindElement calls
            driver.SwitchTo().Frame(driver.FindElement(By.Id("iframeResult")));

            //SelectElement is a helper class (Selenium.Support.UI) that wrap select boxes on pages
            //to avoid extra code when working with <option>s
            var selectElement = new SelectElement(driver.FindElement(By.Name("cars")));

            //to do something like a multi-select, more than one action is required
            //Actions allow you to do this by chaining events
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
            //instead of typing a bunch of driver.FindElement methods for every test,
            //we can specify a page object with properties and [FindsBy] attributes (see below)
            //the PageFactory populates that object with element references for us
            PageFactory.InitElements(driver, homepage);

            homepage.SearchBox.SendKeys("cheese");
            homepage.SearchButton.Click();
        }
    }

    public class YahooHomepage
    {
        [FindsBy(Using="p",How=How.Name)]
        public IWebElement SearchBox { get; set; }

        //the default is How=How.Id, so we don't have to specify
        [FindsBy(Using="search-submit")]
        public IWebElement SearchButton { get; set; }
    }
}
