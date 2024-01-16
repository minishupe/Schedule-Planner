using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using Schedule_Planner.Util;

namespace Schedule_Planner.Core;

/// <summary>
/// A lecture section, can optionally be accompanied by a lab section.
/// </summary>
public class Lecture : ISection
{
    public int Crn { get; }
    public string Days { get; }
    public TimeBlock Time { get; }
    public Instructor Instructor { get; private set; }
    public string Room { get; private set; }
    public int EnrolledSeats { get; private set; }
    public int MaxSeats { get; private set; }
    public string[]? Traits { get; }
    public Lab? Lab { get; set; }


    /// <summary>
    /// Creates a new Lecture object directly from a table web element.
    /// </summary>
    /// <param name="sectionTable">A raw table element from the Timetable.</param>
    public Lecture(IWebElement sectionTable)
    {
        var cells = sectionTable.FindElements(By.XPath(".//td"));
        this.Crn = int.Parse(cells[1].Text);
        this.Days = cells[2].Text;
        this.Time = TimeBlock.Parse(cells[3].Text);
    }



}
