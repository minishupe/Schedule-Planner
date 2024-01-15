﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Planner.Util;

/// <summary>
/// A basic utility class for representing a block of time between a defined start and end.
/// </summary>
public class TimeBlock
{
    public int StartHour { get; set; }
    public int StartMinute { get; set; }
    public int EndHour { get; set; }
    public int EndMinute { get; set; }

    public TimeBlock(int startHour, int startMinute, int endHour, int endMinute)
    {
        this.StartHour = startHour;
        this.StartMinute = startMinute;
        this.EndHour = endHour;
        this.EndMinute = endMinute;
    }
    
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
        int startHr = int.Parse(start.Split(':')[0]);
        int endHr = int.Parse(end.Split(':')[0]);
        
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

        int startMin = int.Parse(start.Split(':')[1]);
        int endMin = int.Parse(end.Split(':')[1]);

        return new TimeBlock(startHr, startMin, endHr, endMin);
    }

    /// <summary>
    /// Returns whether the current Time Block overlaps in time with another Time Block.
    /// </summary>
    /// <param name="other"></param>
    /// <returns>True if this objects overlaps with the given object, otherwise false.</returns>
    public bool Overlaps(TimeBlock other)
    {
        if (this.EndHour < other.StartHour || other.EndHour < this.StartHour)
        {
            if (this.EndMinute < other.EndMinute ||  other.EndMinute < this.StartMinute)
            {
                return true;
            }
        }
        return true;
    }

    public override string ToString()
    {
        // Prints the timeblock as HH:MM XM - HH:MM XM.
        string start;
        if (StartHour > 12)
        {
            start = (StartHour - 12).ToString() + ":" + StartMinute.ToString() + " pm";
        }
        else
        {
            start = StartHour.ToString() + ":" + StartMinute.ToString() + " am";
        }

        string end;
        if (EndHour > 12)
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
