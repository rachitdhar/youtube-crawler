using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;

class Crawler
{
    static string destinationFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "urls.txt");
    static string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs.txt");
    static long URL_PROCESSING_LIMIT = 1_000_000;
    static int BULK_WRITE_SIZE = 20;
    static long urlsProcessedCount = 0;

    static List<string> ExtractLinks(string url, IWebDriver driver)
    {
        List<string> pageHyperlinks = new List<string>();
        string youtubePattern = @"^(https:\/\/www\.youtube\.com\/watch\?v=|https:\/\/youtu\.be\/)[a-zA-Z0-9_-]+$";
        try
        {
            driver.Navigate().GoToUrl(url);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var elements = wait.Until(d => d.FindElements(By.XPath("//a[@href]")));
            pageHyperlinks = elements.Select(element => element.GetAttribute("href"))
                                    .Where(href => !string.IsNullOrEmpty(href) && Regex.IsMatch(href, youtubePattern))
                                    .Distinct().ToList();
        }
        catch (Exception ex)
        {
            File.AppendAllText(logFilePath, $"Error in extraction of links: {ex.Message}");
        }
        return pageHyperlinks;
    }
    static void DisplayProgressBar()
    {
        int barWidth = 50; // Width of progress bar
        double percent = (double)urlsProcessedCount / URL_PROCESSING_LIMIT;
        int filled = (int)(percent * barWidth);

        Console.Write("\r["); // Carriage return to overwrite the line
        Console.Write(new string('█', filled)); // Filled part
        Console.Write(new string('-', barWidth - filled)); // Unfilled part
        Console.Write($"] {percent * 100:0.0}%"); // Percentage display
        urlsProcessedCount += BULK_WRITE_SIZE;
    }

    private void Crawl(Queue<string> urlQueue, HashSet<string> previouslyQueuedURLS, List<string> listToBeWritten, IWebDriver driver)
    {
        if (urlQueue.Count == 0 || previouslyQueuedURLS.Count >= URL_PROCESSING_LIMIT) return;

        string url = urlQueue.Dequeue();
        previouslyQueuedURLS.Add(url);
        listToBeWritten.Add(url);
        
        // write in bulk to improve performance
        if (listToBeWritten.Count >= BULK_WRITE_SIZE)
        {
            File.AppendAllText(destinationFilePath, string.Join("\n", listToBeWritten));
            listToBeWritten.Clear();
            DisplayProgressBar();
        }
        List<string> pageHyperlinks = ExtractLinks(url, driver);

        foreach (string link in pageHyperlinks)
        {
            if (!previouslyQueuedURLS.Contains(link))
                urlQueue.Enqueue(link);
        }
        Crawl(urlQueue, previouslyQueuedURLS, listToBeWritten, driver);
        return;
    }

    public static void Main()
    {
        Crawler app = new Crawler();
        List<string> baseURLS = new List<string>() { "" }; // initialize with one or more youtube video urls

        Queue<string> urlQueue = new Queue<string>(baseURLS);
        HashSet<string> previouslyQueuedURLS = new HashSet<string>();

        var options = new ChromeOptions();
        options.AddArgument("--headless"); // run chrome in headless mode (no GUI)
        options.AddArgument("--log-level=3"); // don't log warnings and info
        using IWebDriver driver = new ChromeDriver(options);
        DisplayProgressBar();
        Thread.Sleep(2000);

        app.Crawl(urlQueue, previouslyQueuedURLS, new List<string>(), driver);
        driver.Quit();
    }
}
