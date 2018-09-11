using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;

namespace ForkBot
{
    class Course
    {
        public string Department { get; }
        public string Subject { get; }
        public string Coursecode { get; }
        public double Credit { get; }
        public string CourseLink { get; } = "";
        public string ScheduleLink { get; }
        public string Description { get; }
        public string Title { get; }
        public CourseSchedule Schedule { get; }
        static Exception CourseNotFoundException = new Exception("Unable to find course on course list. Ensure course code is correct.");
        static Exception CourseNotLoadedException = new Exception("Course not loaded. Ensure course is loaded properly before using this function.");
        private HtmlWeb web = new HtmlWeb();
        public Course(string code)
        {
            var courses = File.ReadAllLines("Files/courselist.txt");
            foreach (string course in courses)
            {
                if (course.ToLower().Contains(code.ToLower()))
                {
                    var info = course.Split(' ');
                    Department = info[0].Split('/')[0];
                    Subject = info[0].Split('/')[1];
                    Coursecode = info[1];
                    Credit = Convert.ToDouble(info[2].Split('\t')[0]);
                    CourseLink = $"https://w2prod.sis.yorku.ca/Apps/WebObjects/cdm.woa/wa/crsq?fa={Department}&sj={Subject}&cn={Coursecode}&cr={Credit}&ay=2018&ss=FW";
                    break;
                }
            }
            if (CourseLink == "") throw CourseNotFoundException;

            var pageDoc = web.Load(CourseLink).DocumentNode;

            var desc = pageDoc.SelectSingleNode("/html[1]/body[1]/table[1]/tr[2]/td[2]/table[1]/tr[2]/td[1]/table[1]/tr[1]/td[1]").ChildNodes[5].InnerText;
            Title = pageDoc.SelectSingleNode("/html[1]/body[1]/table[1]/tr[2]/td[2]/table[1]/tr[2]/td[1]/table[1]/tr[1]/td[1]/table[1]/tr[1]/td[1]").InnerText.Replace("&nbsp;", "");
            Description = desc.Replace("&quot;", "\"");
            var scheduleNode = pageDoc.SelectSingleNode("/html[1]/body[1]/table[1]/tr[2]/td[2]/table[1]/tr[2]/td[1]/table[1]/tr[1]/td[1]/p[7]/a[1]");
            ScheduleLink = "https://w2prod.sis.yorku.ca" + scheduleNode.Attributes[0].Value;
            Schedule = new CourseSchedule(this);
        }
        
    }
}
