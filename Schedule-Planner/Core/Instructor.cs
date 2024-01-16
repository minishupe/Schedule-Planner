using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Planner.Core;

/// <summary>
/// Represents a professor, which can be added to any section.
/// </summary>
public class Instructor
{
    public string Name { get; }
    public string Id { get; set; }
    public float Rating { get; private set; }
    public float Difficulty { get; private set; }

    public Instructor()
    {

    }

    /// <summary>
    /// Fetches instructor data from Rate My Professors and converts it into an
    /// Instructor object.
    /// </summary>
    /// <returns>An Instructor instance</returns>
    private static Instructor Fetch()
    {
        // I should do this for courses too.

    }
}
