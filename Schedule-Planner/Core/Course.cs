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
    public string Prefix { get; init; }
    public int Code { get; init; }
    public string Name { get; init; }
    public int Credits { get; init; }
    public Dictionary<int, int> NumSections {  get; set; }
    private static readonly string dataPath = $@"{Environment.ProcessPath}\Data\";
    private const string TimetableUrl = "https://web4u.banner.wwu.edu/pls/wwis/wwskcfnd.TimeTable";


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
        if (driver.Url != TimetableUrl)
        {
            driver.Url = TimetableUrl;
        }
        if (driver.Title != "WWU TimeTable of Classes")
        {
            Log.Warning($"Unexpected TimeTable title: {driver.Title}");
        }

        // Check the prefix exists:
        var subjSelector = new SelectElement(driver.FindElement(By.Id("subj")));
        IList<IWebElement> subjects = subjSelector.Options;
        foreach (IWebElement item in subjects)
        {
            if (item.GetAttribute("value") == prefix)
            {
                this.Prefix = prefix;
                break;
            }
        }
        if (Prefix == null)
        {
            Log.Warning($"Could not find prefix {prefix} in subject list");
            throw new ArgumentException($"Course prefix {prefix} is not valid");
        }

        // Check the term exists:

        
    }

    public AddTerm(int term, IWebDriver driver)
    {

    }

    /// <summary>
    /// Checks whether a given value exists in a list of web elements.
    /// </summary>
    private bool CheckExists(string item, IList<IWebElement> options)
    {
        foreach (IWebElement option in options)
        {
            if (option.GetAttribute("value") == item)
            {
                return true;
            }
        }
        return false;
    }
}
