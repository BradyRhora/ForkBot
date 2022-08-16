using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SQLite;

namespace YorkU
{
    class Course
    {
        public string Faculty { get; }
        public string Department { get; }
        public int Level { get; }
        public string Subject { get; }
        public string Coursecode { get; }
        public double Credit { get; }
        public string CourseLink = "";
        public string ScheduleLink { get; internal set; }
        public string Description { get; internal set; }
        public string Title { get; }
        public string Term { get; internal set; }
        public string Language { get; }
        public string Type { get; }

        static int year = DateTime.Now.Year;
        public bool CourseNotFound = false;

        private CourseSchedule Schedule;
        static private HtmlWeb web = new HtmlWeb();

        public Course()
        {

        }
               

        /*public string TryFindCourse(string code)
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
                            Console.WriteLine($"Testing: {fac}/{code.ToUpper()} {credit}.00 Temp Name");
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
        }*/


        public CourseSchedule GetSchedule() { return Schedule; }

        public Course(string code, bool getInfo = true, string term = "")
        {
            using (var con = new SQLiteConnection(ForkBot.Constants.Values.YORK_DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "SELECT * FROM COURSES WHERE CODE = @code";
                using (var cmd = new SQLiteCommand(stm, con))
                {
                    cmd.Parameters.AddWithValue("@code", code.ToUpper());
                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        Faculty = reader.GetString(0);
                        Department = reader.GetString(1);
                        Level = reader.GetInt32(2);
                        Coursecode = reader.GetString(3);
                        Credit = reader.GetDouble(4);
                        Title = reader.GetString(5);
                        Language = reader.GetString(6);
                        Type = reader.GetString(7);
                    }
                }
            }
            if (getInfo) GetCourseInfo(term);
        }

        void GetCourseInfo(string term = "")
        {
            if (term != "") Term = term.ToUpper();
            else if (Term == null) Term = GetCurrentTerm();
            CourseLink = $"https://w2prod.sis.yorku.ca/Apps/WebObjects/cdm.woa/wa/crsq?fa={Faculty}&sj={Department}&cn={Level}&cr={Credit}&ay={year}&ss={Term.ToUpper()}";

            var pageDoc = web.Load(CourseLink).DocumentNode;

            var sError = pageDoc.SelectSingleNode("/html[1]/body[1]/table[1]/tr[2]/td[2]/table[1]/tr[2]/td[1]/table[1]/tr[1]/td[1]/table[1]/tr[1]/td[1]/p[1]"); //note to future self, don't include `tbody`
            if (sError != null && sError.InnerText == "Current Courses Search Results")
            {

                var listTable = pageDoc.SelectSingleNode("/html/body/table/tr[2]/td[2]/table/tr[2]/td/table/tr/td/table[2]");
                var newLink = listTable.ChildNodes.Last().ChildNodes[3].InnerText;
                pageDoc = web.Load(newLink).DocumentNode;
            }

            string desc;
            desc = pageDoc.SelectSingleNode("/html[1]/body[1]/table[1]/tr[2]/td[2]/table[1]/tr[2]/td[1]/table[1]/tr[1]/td[1]").ChildNodes[5].InnerText;
            
            Description = desc.Replace("&quot;", "\"");
            var scheduleNode = pageDoc.SelectSingleNode("/html[1]/body[1]/table[1]/tr[2]/td[2]/table[1]/tr[2]/td[1]/table[1]/tr[1]/td[1]/p[7]/a[1]");
            ScheduleLink = "https://w2prod.sis.yorku.ca" + scheduleNode.Attributes[0].Value;
            Schedule = new CourseSchedule(this);
        }

        public static string GetCurrentTerm()
        {
            DateTime FallStart = new DateTime(DateTime.Now.Year, 9, 4);
            DateTime FallEnd = new DateTime(DateTime.Now.Year, 12, 3);

            DateTime WinterStart = new DateTime(DateTime.Now.Year, 1, 6);
            DateTime WinterEnd = new DateTime(DateTime.Now.Year, 4, 5);

            DateTime SummerStart = new DateTime(DateTime.Now.Year, 5, 4);
            DateTime SummerEnd = new DateTime(DateTime.Now.Year, 8, 5);

            if (DateTime.Now > FallStart && DateTime.Now < WinterEnd) return "FW";
            if (DateTime.Now > SummerStart && DateTime.Now < SummerEnd) return "SU";
            return "FW";

        }

        public static Course[] GetCourses(Dictionary<string,string> parameters)
        {
            using (var con = new SQLiteConnection(ForkBot.Constants.Values.YORK_DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "SELECT * FROM COURSES WHERE ";
                foreach (var param in parameters)
                {
                    string val = param.Value.Replace("?", "_");
                    var comparison = "=";
                    if (param.Key == "level") comparison = "LIKE";
                    stm += $"{param.Key.ToUpper()} {comparison} '{val.ToUpper()}' ";

                    if (!param.Equals(parameters.Last())) stm += "AND ";
                }

                using (var cmd = new SQLiteCommand(stm, con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        List<Course> courses = new List<Course>();
                        while (reader.Read())
                            courses.Add(new Course(reader.GetString(3), false));

                        return courses.ToArray();
                    }
                }

            }

        }
    }
    
    

    class CourseNotFoundException : Exception { static new string Message = "Unable to find course on course list.Ensure course code is correct."; }
    class CourseNotLoadedException : Exception { static new string Message = "Course not loaded. Ensure course is loaded properly before using this function."; }

}
