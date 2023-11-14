using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Serilog;

namespace Schedule_Planner.Core;

/// <summary>
/// Represents a course and all its sections in any term.
/// </summary>
class Course
{
    string prefix { get; init; }
    int code { get; init; }
    string name { get; init; }
    int credits { get; init; }
    Dictionary<int, int> numSections {  get; set; }
    private static readonly string dataPath = $@"{Environment.ProcessPath}\Data\";
    private const string timetableUrl = "https://web4u.banner.wwu.edu/pls/wwis/wwskcfnd.TimeTable"


    /// <summary>
    /// Constructs a new course object that is locally saved.
    /// </summary>
    public Course(string prefix, int code)
    {

    }

    /// <summary>
    /// Constructs a new course object using the online timetable.
    /// </summary>
    public Course(string prefix, int code, int term, IWebDriver driver)
    {
        if (driver.Url != timetableUrl)
        {
            driver.Url = timetableUrl;
        }
        if (driver.Title != "WWU TimeTable of Classes")
        {
            Log.Warning($"Unexpected TimeTable title: {driver.Title}");
        }
        var subjSelector = new SelectElement(driver.FindElement(By.Id("subj")));

    }

    public AddTerm(int term, IWebDriver driver)
    {

    }
}
