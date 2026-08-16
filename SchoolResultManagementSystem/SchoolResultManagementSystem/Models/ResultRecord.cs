namespace SchoolResultManagementSystem.Models
{
    public class ResultRecord
    {
        public int ResultId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string RollNo { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public int MaxMarks { get; set; }
        public string ExamTerm { get; set; }
        public decimal MarksObtained { get; set; }
    }
}
