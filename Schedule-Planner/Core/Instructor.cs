﻿using HtmlAgilityPack;
using Schedule_Planner.Util;
using Serilog;
using System;

namespace Schedule_Planner.Core;

/// <summary>
/// Represents a professor, which can be added to any section.
/// </summary>
public class Instructor
{
    public string Name { get; }
    public int Tid { get; }
    public float? Rating { get; set; }
    public float? Difficulty { get; set; }
    public DateOnly LastUpdated { get; set; }
    private const string BING = "https://www.bing.com/search?q=ratemyprofessors";
    private const string RESULT_XPATH = "/html/body/div[4]/main/ol/li[@class=\"b_algo\"]";
    private const string RMP = "https://www.ratemyprofessors.com/professor/";

    /// <summary>
    /// Constructs an instructor object.
    /// </summary>
    /// <param name="name">The professor's first and last name</param>
    /// <param name="url">The RMP link</param>
    /// <param name="rating">The RMP overall rating</param>
    /// <param name="difficulty">The RMP overall difficulty</param>
    /// <param name="lastUpdated">Date of most recent RMP fetch. Defaults to 
    /// current date if null</param>
    /// <exception cref="ArgumentNullException"></exception>

    public Instructor(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Tid = 0;
        Rating = 0;
        Difficulty = 0;
        LastUpdated = DateOnly.FromDateTime(DateTime.Now);
    }

    public Instructor(string name, int tid)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Tid = tid;
    }

    public Instructor(
        string name,
        int tid,
        float? rating,
        float? difficulty,
        DateOnly lastUpdated)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Tid = tid;
        Rating = rating;
        Difficulty = difficulty;
        LastUpdated = lastUpdated;
    }

    /// <summary>
    /// Fetches instructor data from Rate My Professors or the Cache and converts
    /// it into an Instructor object.
    /// </summary>
    /// <returns>
    /// An Instructor instance. If the instructor is not found, a Staff instructor object
    /// </returns>
    public static Instructor? Fetch(string name)
    {
        string? url = null;
        if (name != null || name != "Staff") {

        }
        HtmlWeb web = new();
        var bingResults = web.Load(BING + $"+{name}");
        var resultElements = bingResults.DocumentNode.SelectNodes(RESULT_XPATH);

        foreach (var result in resultElements) {
            var linkElement = result.SelectSingleNode("div[@class=\"b_title\"]/h2/a");
            string title = linkElement.InnerText.ToLower();
            if (!title.Contains(name)) {
                continue;  // Title isn't for this professor.
            }
            string href = linkElement.Attributes["href"].Value;
            if (!(href.Contains("ratemyprofessors.com"))) {
                continue;  // It's not a RMP link.
            }

            // This result should be the desired professor now.
            url = href;
        }

        if (url == null) {
            return null;
        }
        //string name = firstName + " " + lastName;
        int tid = int.Parse(url.Split('/')[^1]);

        Instructor instructor = new(name, tid, null, null, null);
        instructor.UpdateRating();  // Get their rating

        return instructor;
    }

    /// <summary>
    /// Fetches the instructor's rating from RMP and updates their Rating
    /// and Difficulty properties.
    /// </summary>
    public void UpdateRating()
    {
        // TODO: add number of ratings and difficulty to this class.
        const string INFO_XPATH =
            "//*[@class=\"TeacherInfo__StyledTeacher-ti1fio-1 kFNvIp\"]";
        const string RATING_XPATH =
            "//div[@class=\"RatingValue__Numerator-qw8sqy-2 liyUjw\"]";
        //const string NUM_RATINGS_XPATH = 
        //    "//div[@class=\"RatingValue__NumRatings-qw8sqy-0 jMkisx\"]";

        HtmlWeb web = new();
        HtmlDocument page = web.Load(GetUrl());
        HtmlNode teacherInfo = page.DocumentNode.SelectSingleNode(INFO_XPATH);

        string rating = teacherInfo.SelectSingleNode(RATING_XPATH).InnerText;
        //string numRatings = teacherInfo.SelectSingleNode(NUM_RATINGS_XPATH).InnerText;

        Rating = float.Parse(rating);
        LastUpdated = DateOnly.FromDateTime(DateTime.Now);
    }

    /// <summary>
    /// Returns the ratemyprofessor.com URL of the given instructor
    /// </summary>
    public string GetUrl()
    {
        return RMP + Tid.ToString();
    }
}
