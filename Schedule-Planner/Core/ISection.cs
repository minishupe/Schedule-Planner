using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Planner.Core;

/// <summary>
/// Interface for a generic course section.
/// </summary>
interface ISection
{
    public int Crn { get; }
    public string Days { get; }
    public TimeSpan Time {  get; }
    public string Instructor {  get; }
    public string Room { get; }
    public int[] Seats { get; }
    public string[]? Traits { get; }


}
