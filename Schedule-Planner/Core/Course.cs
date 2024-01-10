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
    public Dictionary<int, List<ISection>> Sections {  get; set; } // keys are terms, values are associated classes
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

        // Check to make sure term and prefix exist:
        // TODO: save terms and subjects locally, only search through selectlist if it doesn't exist locally.
        string? invalidArgs = CheckInvalid(prefix, term.ToString(), driver);
        if (invalidArgs != null)
        {
            Log.Warning($"Given prefix or term does not exist: {invalidArgs}");
            throw new ArgumentException($"Given prefix or term does not exist: {invalidArgs}", invalidArgs);
        }

        var submitButton = driver.FindElement(By.XPath("//input[@value='Submit'"));
        submitButton.Click();

        Sections = new Dictionary<int, List<ISection>>
        {
            { term, new List<ISection>() }
        };

        var courseTitles = driver.FindElements(By.XPath("//td[@class='fieldformatboldtext']"));
        foreach (IWebElement course in courseTitles)
        {
            string[] splitText = course.Text.Split(" ");
            if (splitText[0] == prefix && splitText[1] == code.ToString())
            {
                Credits = (splitText[^1])[0];
                var courseSections = driver.FindElements(RelativeBy.WithLocator(By.XPath("//table")).Below(course));

                foreach (IWebElement section in courseSections)
                {
                    var sectionInfo = section.FindElements(By.XPath(".//td"));
                    // skips header and other random table elements:
                    if (sectionInfo.Count() == 0 || sectionInfo[0].Text == "Term")
                    {
                        continue;
                    }
                    // signifies a lab section:
                    else if (sectionInfo[0].Text == "")
                    {
                        Sections.GetValueOrDefault(term).Add(new Lab());
                    }
                }
        }
    }

    public AddTerm(int term, IWebDriver driver)
    {

    }

    /// <summary>
    /// Returns a list of Section objects relating to the given term.
    /// Precondition: driver is loaded onto the Timetable with the desired term and subject,
    /// the Sections, Code, and Prefix fields have been initialized.
    /// </summary>
    private List<ISection> CreateSections(int term, IWebDriver driver)
    {
        
    }


    /// <summary>
    /// Checks whether a given subject and term exist in the timetable.
    /// Returns: a string denoting the invalid prefix or term, or null if both are valid.
    /// </summary>
    private string? CheckInvalid(string prefix, string term, IWebDriver driver)
    {
        var subjSelector = new SelectElement(driver.FindElement(By.Id("subj")));
        IList<IWebElement> subjects = subjSelector.Options;
        var termSelector = new SelectElement(driver.FindElement(By.Id("term")));
        IList<IWebElement> terms = termSelector.Options;
        bool subjValid = false;
        int i = 0;

        // subjects = 5, terms = 3, i[end] = 8
        // 

        while (i < subjects.Count + terms.Count) 
        {
            if (i < subjects.Count)
            {
                if (subjects[i].GetAttribute("value") == prefix) 
                { 
                    // subject exists, so skip to checking term.
                    subjValid = true;
                    i = subjects.Count;
                }
            }
            else
            {
                if (terms[i - subjects.Count].GetAttribute("value") == term)
                {
                    // term exists and subject exists, so return.
                    return null;
                }
            }
            if (i == subjects.Count - 1 && subjValid == false)
            {
                // subject does not exist.
                return prefix;
            }
        }
        // term does not exist.
        return term;
    }
}
