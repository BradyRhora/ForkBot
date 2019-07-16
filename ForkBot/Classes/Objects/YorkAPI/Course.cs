using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.Text.RegularExpressions;

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
        int year = 2019;
        public bool CourseNotFound = false;

        private CourseSchedule Schedule;
        static private HtmlWeb web = new HtmlWeb();

        public Course()
        {

        }

        public Course(string code, string term = "", bool force = false)
        {
            var courses = File.ReadAllLines("Files/courselist.txt");
            string courseLine = "";
            foreach (string course in courses)
            {
                var line = Regex.Replace(course, "( {2,})", " ").ToLower();
                if (line.Contains(code.ToLower()))
                {
                    courseLine = course;
                    break;
                }
            }
            CourseNotFound = courseLine == ""; 

            if (CourseNotFound && force)
            {
                var course = TryFindCourse(code);
                if (course != null)
                {
                    CourseNotFound = false;
                    courseLine = course;
                }
            }



            if (!CourseNotFound) LoadCourse(courseLine, term);
        }

        public string TryFindCourse(string code)
        {
            string[] faculties = { "AP", "ED", "ES", "FA", "GL", "GS", "HH", "LE", "LW", "SB", "SC" };
            string[] terms = { "FW","SU" };
            int[] creditCount = { 3, 6, 9 };

            foreach (var fac in faculties)
            {
                foreach (var credit in creditCount)
                {

                    foreach (var term in terms)
                    {
                        Course course = new Course();
                        try
                        {
                            course.LoadCourse($"{fac}/{code.ToUpper()} {credit}.00 Temp Name", term);
                            File.AppendAllText("Files/courselist.txt", $"\n{course.Title}");
                            Console.WriteLine($"Added {code} to courselist");
                            return course.Title;
                        }
                        catch { }
                    }
                }
            }
            return null;
        }

        public void LoadCourse(string courseLine, string term = "")
        {
            courseLine = Regex.Replace(courseLine, "( {2,})", " ");
            if (term == "") Term = Var.term;
            else Term = term;
            var info = courseLine.Split(' ');
            Department = info[0].Split('/')[0];
            Subject = info[0].Split('/')[1];
            Coursecode = info[1];
            Credit = Convert.ToDouble(info[2]);
            CourseLink = $"https://w2prod.sis.yorku.ca/Apps/WebObjects/cdm.woa/wa/crsq?fa={Department}&sj={Subject}&cn={Coursecode}&cr={Credit}&ay={year}&ss={Term.ToUpper()}";
            var clback = $"https://w2prod.sis.yorku.ca/Apps/WebObjects/cdm.woa/wa/crsq?fa={Department}&sj={Subject}&cn={Coursecode}&cr={Credit}&ay={year-1}&ss={Term.ToUpper()}";
            if (CourseLink == "") throw new CourseNotFoundException();

            var pageDoc = web.Load(CourseLink).DocumentNode;

            try
            {
                var sError = pageDoc.SelectSingleNode("/html/body/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr/td/table[1]/tbody/tr/td[1]/p");
                if (sError.InnerText == "Current Courses Search Results")
                {
                    var listTable = pageDoc.SelectSingleNode("/html/body/table/tbody/tr[2]/td[2]/table/tbody/tr[2]/td/table/tbody/tr/td/table[2]/tbody");
                    var newLink = listTable.ChildNodes.Last().ChildNodes[3];
                }
            }
            catch { }


            string desc;
            try
            {
                desc = pageDoc.SelectSingleNode("/html[1]/body[1]/table[1]/tr[2]/td[2]/table[1]/tr[2]/td[1]/table[1]/tr[1]/td[1]").ChildNodes[5].InnerText;
            }
            catch
            {
                pageDoc = web.Load(clback).DocumentNode;
                desc = pageDoc.SelectSingleNode("/html[1]/body[1]/table[1]/tr[2]/td[2]/table[1]/tr[2]/td[1]/table[1]/tr[1]/td[1]").ChildNodes[5].InnerText;
            }
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
