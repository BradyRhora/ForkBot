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
        private string Department;
        private string Subject;
        private string Coursecode;
        private double Credit;
        private string CourseLink = "";
        private string ScheduleLink;
        private string Description;
        private string Title;
        private string Term;

        private CourseSchedule Schedule;
        static private HtmlWeb web = new HtmlWeb();

        public Course()
        {

        }

        public Course(string code, string term = "")
        {
            var courses = File.ReadAllLines("Files/courselist.txt");
            string courseLine = "";
            foreach (string course in courses)
            {
                if (course.ToLower().Contains(code.ToLower()))
                {
                    courseLine = course;
                    break;
                }
            }

            LoadCourse(courseLine, term);
        }

        public void LoadCourse(string courseLine, string term = "")
        {
            if (term == "") Term = Var.term;
            else Term = term;
            var info = courseLine.Split(' ');
            Department = info[0].Split('/')[0];
            Subject = info[0].Split('/')[1];
            Coursecode = info[1];
            Credit = Convert.ToDouble(info[2]);
            CourseLink = $"https://w2prod.sis.yorku.ca/Apps/WebObjects/cdm.woa/wa/crsq?fa={Department}&sj={Subject}&cn={Coursecode}&cr={Credit}&ay=2018&ss={Term.ToUpper()}";

            if (CourseLink == "") throw new CourseNotFoundException();

            var pageDoc = web.Load(CourseLink).DocumentNode;

            var desc = pageDoc.SelectSingleNode("/html[1]/body[1]/table[1]/tr[2]/td[2]/table[1]/tr[2]/td[1]/table[1]/tr[1]/td[1]").ChildNodes[5].InnerText;
            Title = pageDoc.SelectSingleNode("/html[1]/body[1]/table[1]/tr[2]/td[2]/table[1]/tr[2]/td[1]/table[1]/tr[1]/td[1]/table[1]/tr[1]/td[1]").InnerText.Replace("&nbsp;", "");
            Description = desc.Replace("&quot;", "\"");
            var scheduleNode = pageDoc.SelectSingleNode("/html[1]/body[1]/table[1]/tr[2]/td[2]/table[1]/tr[2]/td[1]/table[1]/tr[1]/td[1]/p[7]/a[1]");
            ScheduleLink = "https://w2prod.sis.yorku.ca" + scheduleNode.Attributes[0].Value;
            Schedule = new CourseSchedule(this);
        }

        public string GetDepartment() { return Department; }

        public string GetSubject() { return Subject; }

        public string GetCode() { return Coursecode; }

        public double GetCredit() { return Credit; }

        public string GetCourseLink() { return CourseLink; }

        public string GetScheduleLink() { return ScheduleLink; }

        public string GetDescription() { return Description; }

        public string GetTitle() { return Title; }

        public string GetTerm() { return Term.ToUpper(); }

        public CourseSchedule GetSchedule() { return Schedule; }
        
    }
    

    class CourseNotFoundException : Exception { static new string Message = "Unable to find course on course list.Ensure course code is correct."; }
    class CourseNotLoadedException : Exception { static new string Message = "Course not loaded. Ensure course is loaded properly before using this function."; }

}
