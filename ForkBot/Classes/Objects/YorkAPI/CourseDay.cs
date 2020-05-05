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
        public Dictionary<string, DateTime> DayTimes = new Dictionary<string, DateTime>();
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
            if (time == "")
            {
                DayTimes.Add(day, DateTime.MinValue);
                return;
            }

            var splitTime = time.Split(':').Select(x=>Convert.ToInt32(x)).ToArray();
            DayTimes.Add(day, new DateTime(1,1,1,splitTime[0],splitTime[1],0));
        }
    }
}
