using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace YorkU
{
    class CourseSchedule
    {
        public Course Course { get; }
        public List<CourseDay> Days { get; }
        private HtmlWeb web = new HtmlWeb();

        public CourseSchedule(Course course)
        {
            Course = course;
            Days = new List<CourseDay>();
            var pageDoc = web.Load(course.ScheduleLink).DocumentNode;

            var table = pageDoc.SelectSingleNode("/html[1]/body[1]/table[1]/tr[2]/td[2]/table[1]/tr[2]/td[1]/table[1]/tr[1]/td[1]/table[2]");
            
            foreach (HtmlNode child in table.ChildNodes)
            {
                if (child.Name == "tr")
                {
                    var termSec = child.SelectSingleNode($"td/table/tr[1]").InnerText.Replace("\n", "").Replace("\t", "").Replace("&nbsp;", "").Trim(',').Trim();
                    var sessionDir = child.SelectSingleNode($"td/table/tr[2]").InnerText.Replace("Please click here to see availability.", "").Replace("&nbsp;", " ").Trim().Replace("   ", ", ");
                    var schedule = child.SelectSingleNode($"td/table/tr[3]/td/table/tr[2]");
                    var type = schedule.ChildNodes[0].InnerText;
                    var timedayInfo = schedule.ChildNodes[1].InnerText.Replace("&nbsp;", "").Trim().Replace("     ", "|").Replace(" ", "").Replace("(Glendoncampus)", "").Split(new char[]{'|'}, StringSplitOptions.RemoveEmptyEntries);

                    var TermAndSec = termSec.Split(new char[]{' '},StringSplitOptions.RemoveEmptyEntries);
                    var CAT = schedule.ChildNodes[2].InnerText;

                    var term = TermAndSec[0] + " " + TermAndSec[1];
                    var section = TermAndSec[2] + " " + TermAndSec[3];
                    
                    var professor = sessionDir;

                    if (timedayInfo.Count() == 1 && timedayInfo[0] == "") break;

                    CourseDay cDay = new CourseDay(term, section, professor, CAT);
                    for (int i = 0; i < timedayInfo.Count(); i++)
                    {
                        string day = Convert.ToString(timedayInfo[i][0]);
                        string time = "";
                        if (!timedayInfo[i].StartsWith("0") && !type.StartsWith("ONLN"))
                        {
                            for (int o = 2; o < timedayInfo[i].Length; o++)
                            {
                                if (timedayInfo[i][o - 2] == ':') time = timedayInfo[i].Substring(1, o);
                            }


                            Dictionary<string, string> DayConversion = new Dictionary<string, string>() { { "M", "Monday" }, { "T", "Tuesday" }, { "W", "Wednesday" }, { "R", "Thursday" }, { "F", "Friday" } };
                            cDay.AddDayTime(DayConversion[day], time);
                        }
                        else cDay.AddDayTime("", "");
                    }

                    Days.Add(cDay);
                }
            }
        }
    }
}
