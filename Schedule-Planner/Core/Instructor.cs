using System;
using Serilog;
using HtmlAgilityPack;
using Schedule_Planner.Util;

namespace Schedule_Planner.Core;

/// <summary>
/// Represents a professor, which can be added to any section.
/// </summary>
public class Instructor
{
    public string Name { get; }
    public string? Url { get; private set; }
    public float? Rating { get; private set; }
    public float? Difficulty { get; private set; }
    private static TimeOnly lastFetch {  get; set; }
    private const string BING = "https://www.bing.com/search?q=ratemyprofessors";
    private const string RESULT_XPATH = "/html/body/div[4]/main/ol/li[@class=\"b_algo\"]";

    public Instructor(string name, string? id, float? rating, float? difficulty)
    {

    }

    /// <summary>
    /// Fetches instructor data from Rate My Professors and converts it into an
    /// Instructor object.
    /// </summary>
    /// <returns>
    /// An Instructor instance or null if the instructor rating wasn't found.
    /// </returns>
    public static Instructor? Fetch(string firstName, string lastName)
    {
        // Grabs the first couple of urls from Bing, getting the RMP teacher ID,
        // then fetches the html content for that teacher.

        HtmlWeb web = new();
        var bingResults = web.Load(BING + $"+{firstName}+{lastName}");
        var resultElements = bingResults.DocumentNode.SelectNodes(RESULT_XPATH);
        string? url = null;

        foreach (var result in resultElements)
        {
            var linkElement = result.SelectSingleNode("div[@class=\"b_title\"]/h2/a");
            string title = linkElement.InnerText.ToLower();
            if (!(title.Contains(firstName) && title.Contains(lastName)))
            {
                continue;  // Title isn't for this professor.
            }
            string href = linkElement.Attributes["href"].Value;
            if (!(href.Contains("ratemyprofessors.com")))
            {
                continue;  // It's not a RMP link.
            }

            // This result should be the desired professor now.
            url = href;
        }

        if (url == null)
        {
            return null;
        }
        string name = firstName + " " + lastName;

        Instructor instructor = new Instructor(name, url, null, null);
        instructor.updateRating();

        return instructor;
    }

    /// <summary>
    /// Fetches the instructor's rating from RMP and updates their Rating
    /// and Difficulty properties.
    /// </summary>
    public void updateRating()
    {
        const string INFO_XPATH = 
            "//*[@class=\"TeacherInfo__StyledTeacher-ti1fio-1 kFNvIp\"]";
        const string RATING_XPATH = 
            "//div[@class=\"RatingValue__Numerator-qw8sqy-2 liyUjw\"]";
        const string NUM_RATINGS_XPATH = 
            "//div[@class=\"RatingValue__NumRatings-qw8sqy-0 jMkisx\"]";

        HtmlWeb web = new();
        var page = web.Load(Url);
        var teacherInfo = page.DocumentNode.SelectSingleNode(INFO_XPATH);

        var rating = teacherInfo.SelectSingleNode(RATING_XPATH);
        string numRatings = teacherInfo.SelectSingleNode(NUM_RATINGS_XPATH).InnerText;

    }
}
