using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YorkU
{
    class CourseDay
    {
        public string Term { get; }
        public string Section { get; }
        public string Professor { get; }
        public Dictionary<string, string> DayTimes = new Dictionary<string, string>();
        public string CAT { get; }
        public bool HasLabs { get; } = false;

        public CourseDay(string term, string section, string prof, string CAT)
        {
            Term = term;
            Section = section;
            Professor = prof;

            if (CAT == "&nbsp;")
            {
                HasLabs = true;
                this.CAT = "";
            }
            else this.CAT = CAT;
        }

        public void AddDayTime(string day, string time)
        {
            DayTimes.Add(day, time);
        }
    }
}
