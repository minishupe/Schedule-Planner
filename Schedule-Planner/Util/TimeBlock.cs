namespace Schedule_Planner.Util;

/// <summary>
/// Represents a block of two times, accurate to the minute.
/// </summary>
/// <param name="StartHour">The hour component of the starting time</param>
/// <param name="StartMinute">The minute component of the starting time</param>
/// <param name="EndHour">The hour component of the ending time</param>
/// <param name="EndMinute">The minute component of the ending time</param>
public readonly record struct TimeBlock(byte StartHour, byte StartMinute, byte EndHour, byte EndMinute)
{
    /// <summary>
    /// Attempts to parse a string representing a time block.
    /// <para />
    /// The string should be in the format HH:MM-HH:MM XM, where XM is either am or pm.
    /// <br />
    /// Time blocks must be less than 24 hours, so for example, 11:30-12:30 pm will be parsed as
    /// 11:30 am - 12:30 pm.
    /// </summary>
    /// <returns>A TimeBlock representing the string passed in.</returns>
    public static TimeBlock Parse(string str)
    {
        string designator = str[^2..].ToLower();
        string start = str.Split('-')[0].Trim();
        string end = str.Split('-')[1][..^2].Trim();
        byte startHr = byte.Parse(start.Split(':')[0]);
        byte endHr = byte.Parse(end.Split(':')[0]);

        // Convert pm times into 24-hour format:
        if (designator == "pm")
        {
            // Accounts for times that start in am and end in pm, like 11 am - 1 pm:
            if (startHr <= 12 && (endHr < startHr))
            {
                endHr += 12;
            }
            else if (startHr != 12 && endHr != 12)
            {
                startHr += 12;
                endHr += 12;
            }
        }

        byte startMin = byte.Parse(start.Split(':')[1]);
        byte endMin = byte.Parse(end.Split(':')[1]);

        return new TimeBlock(startHr, startMin, endHr, endMin);
    }

    /// <summary>
    /// Returns whether the current Time Block overlaps in time with another Time Block.
    /// </summary>
    /// <param name="other">The object to compare this object with</param>
    /// <returns>True if this objects overlaps with the given object, otherwise false.</returns>
    public bool Overlaps(TimeBlock other)
    {
        if (this.EndHour < other.StartHour || other.EndHour < this.StartHour)
        {
            if (this.EndMinute < other.EndMinute || other.EndMinute < this.StartMinute)
            {
                return false;
            }
        }
        return true;
    }

    public override string ToString()
    {
        // Prints the timeblock as "HH:MM XM - HH:MM XM".
        string start;
        if (StartHour >= 12)
        {
            start = (StartHour - 12).ToString() + ":" + StartMinute.ToString() + " pm";
        }
        else
        {
            start = StartHour.ToString() + ":" + StartMinute.ToString() + " am";
        }

        string end;
        if (EndHour >= 12)
        {
            end = (EndHour - 12).ToString() + ":" + EndMinute.ToString() + " pm";
        }
        else
        {
            end = EndHour.ToString() + ":" + EndMinute.ToString() + " am";
        }

        return start + " - " + end;
    }
}
