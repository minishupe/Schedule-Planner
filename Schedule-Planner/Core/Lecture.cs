using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Planner.Core;

/// <summary>
/// A lecture section, can optionally be accompanied by a lab section.
/// </summary>
public class Lecture : ISection
{
    public int Crn { get; }
    public string Days { get; }
    public TimeSpan Time { get; }
    public string Instructor { get; set; }
    public string Room { get; set; }
    public int[] Seats { get; set; } // formatted: cap, enrolled, available
    public string[]? Traits { get; }
    public float? Rating { get; set; }


}
