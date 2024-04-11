using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Schedule_Planner.Util;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Planner.Core;

/// <summary>
/// Represents a course and all its sections for a single term
/// </summary>
class Course
{
    public string Prefix { get; }
    public int Code { get; }
    public int Term { get; }
    public string? Name { get; set; }
    public int Credits { get; set; }
    public List<Lecture> Lectures { get; set; }
    private static readonly string dataPath = $@"{Environment.ProcessPath}\Data\";
    private const string TIMETABLE = "https://web4u.banner.wwu.edu/pls/wwis/wwskcfnd.TimeTable";

    public Course(
        string prefix,
        int code,
        string? name,
        int credits,
        List<Lecture> lectures)
    {
        Prefix = prefix;
        Code = code;
        Name = name;
        Credits = credits;
        Lectures = lectures;
    }

    /// <summary>
    /// Creates a Course object from the WWU timetable.
    /// </summary>
    /// <param name="prefix">The course prefix, i.e. subject</param>
    /// <param name="code">The course code</param>
    /// <param name="driver">The webdriver to use for scraping</param>
    /// <returns>A Course object or null if specified prefix/code does not exist</returns>
    public static Course? Fetch(string prefix, int code, int term, IWebDriver driver)
    {
        const string TITLES_XPATH = "//td[@class='fieldformatboldtext']";

        // Various checks on the driver:
        if (driver == null) {
            throw new ArgumentNullException(nameof(driver));
        }
        if (driver.Url != TIMETABLE) {
            driver.Url = TIMETABLE;
        }
        if (driver.Title != "WWU TimeTable of Classes") {
            Log.Warning($"Unexpected TimeTable title: {driver.Title}");
        }

        // Ensure prefix exists:
        SelectElement subjSelector = new(driver.FindElement(By.Id("subj")));
        bool subjValid = false;
        foreach (IWebElement subject in subjSelector.Options) {
            if (subject.GetAttribute("value") == prefix) {
                subjValid = true;
                break;
            }
        }
        if (!subjValid) {
            Log.Warning($"Could not create course: given prefix {prefix} does not exist");
            return null;
        }

        // Ensure term exists:
        SelectElement termSelector = new(driver.FindElement(By.Id("term")));
        bool termValid = false;
        foreach (IWebElement option in termSelector.Options) {
            if (option.GetAttribute("value") == term.ToString()) {
                termValid = true;
                break;
            }
        }
        if (!termValid) {
            Log.Warning($"Could not create course: given term {term} does not exist");
            return null;
        }

        // Select term and subject then load page:
        subjSelector.SelectByValue(prefix);
        termSelector.SelectByValue(term.ToString());
        driver.FindElement(By.XPath("//input[@value='Submit']")).Click();

        List<Lecture> lectures = new();
        int credits = 0;
        string? name = null;
        // Get course:
        foreach (IWebElement course in driver.FindElements(By.XPath(TITLES_XPATH))) {
            string[] splitText = course.Text.Split(' ');
            if (splitText[0] != prefix || splitText[1] != code.ToString()) {
                // Not the course we want
                continue;
            }

            credits = splitText[^1][0];
            name = course.FindElement(By.TagName("a")).Text;

            // Read sections:
            ReadOnlyCollection<IWebElement> courseSections = 
                driver.FindElements(RelativeBy.WithLocator(By.XPath("//table")).Below(course));
            foreach (IWebElement section in courseSections) {
                ReadOnlyCollection<IWebElement> sectionInfo =
                    section.FindElements(By.XPath(".//td"));
                if (sectionInfo.Count() == 0 || sectionInfo[0].Text == "Term") {
                    // Element is a header or some other random table element
                    continue;
                } else if (sectionInfo[0].Text == "") {
                    // Element is a lab section
                    lectures[^1].Lab = new Lab();
                } else if (sectionInfo.Count < 13) {
                    // Element is a course title
                    break;
                } else {
                    // Element should be a lecture section
                    string[] instructorName = sectionInfo[4].Text.Split(", ");
                    Lecture lecture = new(
                        int.Parse(sectionInfo[1].Text),
                        sectionInfo[2].Text,
                        TimeBlock.Parse(sectionInfo[3].Text),
                        Instructor.Fetch(instructorName[0], instructorName[1]),
                        sectionInfo[5].Text,
                        int.Parse(sectionInfo[8].Text),
                        int.Parse(sectionInfo[7].Text),
                        null
                        );
                    lectures.Add(lecture);
                }

            }
        }
    }









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
        if (driver.Url != TimetableUrl) {
            driver.Url = TimetableUrl;
        }
        if (driver.Title != "WWU TimeTable of Classes") {
            Log.Warning($"Unexpected TimeTable title: {driver.Title}");
        }

        // Check to make sure prefix exists:
        var subjSelector = new SelectElement(driver.FindElement(By.Id("subj")));
        IList<IWebElement> subjects = subjSelector.Options;
        bool subjValid = false;
        foreach (IWebElement subject in subjects) {
            if (subject.GetAttribute("value") == prefix) {
                subjValid = true;
                break;
            }
        }
        if (!subjValid) {
            Log.Warning($"Given prefix does not exist: {prefix}");
            throw new ArgumentException($"Given prefix does not exist: {prefix}");
        }
        this.Prefix = prefix;
        this.Code = code;

        // Call CreateSections() to validate term and set the actual sections.
        bool addSuccess = AddTerm(term, driver);

        if (!addSuccess) {
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
        if (driver.Url != TimetableUrl) {
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
        foreach (IWebElement option in terms) {
            if (option.GetAttribute("value") == term.ToString()) {
                termValid = true;
                break;
            }
        }
        if (!termValid) {
            return false;
        }

        // Create new section list for the term, creating a new term if necessary:
        if (Sections == null) {
            Sections = new Dictionary<int, List<Lecture>> { { term, new List<Lecture>() } };
        } else {
            Sections[term] = new List<Lecture>();
        }

        // Add sections to the term:
        var courseTitles = driver.FindElements(By.XPath("//td[@class='fieldformatboldtext']"));
        foreach (IWebElement course in courseTitles) {
            string[] splitText = course.Text.Split(" ");
            if (splitText[0] != Prefix && splitText[1] != Code.ToString()) {
                continue;
            }

            if (updateInfo) // update name and credits.
            {
                Credits = splitText[^1][0]; // Possible error? Can you implicitly convert char to int?
                Name = course.FindElement(By.TagName("a")).Text;
            }

            var courseSections = driver.FindElements(RelativeBy.WithLocator(By.XPath("//table")).Below(course));

            foreach (IWebElement section in courseSections) {
                var sectionInfo = section.FindElements(By.XPath(".//td"));
                // skips header and other random table elements:
                if (sectionInfo.Count() == 0 || sectionInfo[0].Text == "Term") {
                    continue;
                }
                // signifies a lab section:
                else if (sectionInfo[0].Text == "") {
                    Sections[term].Last().Lab = new Lab(section);

                }
                // It is a course title:
                else if (sectionInfo.Count < 13) {
                    break;
                }
                // should be a normal lecture section now:
                else {
                    Sections[term].Add(new Lecture(section));
                    success = true;
                }
            }
            break;
        }
        return success;
    }

    public static Course Fetch()
    {

    }
}
