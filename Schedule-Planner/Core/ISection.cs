using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Schedule_Planner.Util;

namespace Schedule_Planner.Core;

/// <summary>
/// Interface for a generic course section.
/// </summary>
interface ISection
{
    public int Crn { get; }
    public string Days { get; }
    public TimeBlock Time { get; }
    public Instructor Instructor { get; }
    public string Room { get; }
    public int EnrolledSeats { get; }
    public int MaxSeats { get; }
    public string[]? Traits { get; }


}
