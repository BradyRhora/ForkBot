using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForkBot
{
    class CourseDay
    {
        public string Term { get; }
        public string Section { get; }
        public string Professor { get; }
        public Dictionary<string, string> DayTimes = new Dictionary<string, string>();
        public string CAT { get; }

        public CourseDay(string term, string section, string prof, string CAT)
        {
            Term = term;
            Section = section;
            Professor = prof;
            this.CAT = CAT;
        }

        public void AddDayTime(string day, string time)
        {
            DayTimes.Add(day, time);
        }
    }
}
