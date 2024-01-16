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
    public string Prefix { get; }
    public int Code { get; }
    public string? Name { get; private set; }
    public int Credits { get; private set; }
    public Dictionary<int, List<Lecture>> Sections { get; private set; } // keys are terms, values are associated classes
    private static readonly string dataPath = $@"{Environment.ProcessPath}\Data\";
    private const string TimetableUrl = "https://web4u.banner.wwu.edu/pls/wwis/wwskcfnd.TimeTable";


    /// <summary>
    /// Constructs a new course object that is locally saved.
    /// </summary>
    public Course(string prefix, int code)
    {
        throw new NotImplementedException();
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

        // Check to make sure prefix exists:
        var subjSelector = new SelectElement(driver.FindElement(By.Id("subj")));
        IList<IWebElement> subjects = subjSelector.Options;
        bool subjValid = false;
        foreach (IWebElement subject in subjects)
        {
            if (subject.GetAttribute("value") == prefix)
            {
                subjValid = true;
                break;
            }
        }
        if (!subjValid)
        {
            Log.Warning($"Given prefix does not exist: {prefix}");
            throw new ArgumentException($"Given prefix does not exist: {prefix}");
        }
        this.Prefix = prefix;
        this.Code = code;

        // Call CreateSections() to validate term and set the actual sections.
        bool addSuccess = AddTerm(term, driver);

        if (!addSuccess)
        {
            Log.Warning($"Given term {term} or course code {code} does not exist");
            throw new ArgumentException($"Given term {term} or course code {code} does not exist");
        }
    }


    /// <summary>
    /// Adds a term and its corresponding sections to this object's Sections.
    /// <para />
    /// <br>Preconditions: Subject prefix exists within Timetable. </br>
    /// </summary>
    /// <returns>A boolean denoting whether the term and its sections were successfully added.</returns>
    public bool AddTerm(int term, IWebDriver driver, bool updateInfo = false)
    {
        bool success = false;
        // Load onto timetable and get the subject, if needed:
        if (driver.Url != TimetableUrl)
        {
            driver.Url = TimetableUrl;
        }
        // TODO: check if subject is already loaded, skipping this next part.
        var subjSelector = new SelectElement(driver.FindElement(By.Id("subj")));
        subjSelector.SelectByValue(Prefix);
        var submitButton = driver.FindElement(By.XPath("//input[@value='Submit'"));
        submitButton.Click();

        // Check that the term exists:
        var termSelector = new SelectElement(driver.FindElement(By.Id("term")));
        IList<IWebElement> terms = termSelector.Options;
        bool termValid = false;
        foreach(IWebElement option in terms)
        {
            if (option.GetAttribute("value") == term.ToString())
            {
                termValid = true;
                break;
            }
        }
        if (!termValid)
        {
            return false;
        }

        // Create new section list for the term, creating a new term if necessary:
        if (Sections == null)
        {
            Sections = new Dictionary<int, List<Lecture>> { { term, new List<Lecture>() } };
        }
        else
        {
            Sections[term] = new List<Lecture>();
        }

        // Add sections to the term:
        var courseTitles = driver.FindElements(By.XPath("//td[@class='fieldformatboldtext']"));
        foreach (IWebElement course in courseTitles)
        {
            string[] splitText = course.Text.Split(" ");
            if (splitText[0] != Prefix && splitText[1] != Code.ToString())
            {
                continue;
            }

            if (updateInfo) // update name and credits.
            {
                Credits = splitText[^1][0]; // Possible error? Can you implicitly convert char to int?
                Name = course.FindElement(By.TagName("a")).Text;
            }
            
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
                    Sections[term].Last().Lab = new Lab(section);

                }
                // It is a course title:
                else if (sectionInfo.Count < 13) 
                {
                    break;
                }
                // should be a normal lecture section now:
                else
                {
                    Sections[term].Add(new Lecture(section));
                    success = true;
                }
            }
            break;
        }
        return success;
    }


}
