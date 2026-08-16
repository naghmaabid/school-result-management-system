namespace SchoolResultManagementSystem.Helpers
{
    /// <summary>Converts a percentage score into a letter grade for report cards.</summary>
    public static class GradeHelper
    {
        public static string GetGrade(decimal percentage)
        {
            if (percentage >= 90) return "A+";
            if (percentage >= 80) return "A";
            if (percentage >= 70) return "B";
            if (percentage >= 60) return "C";
            if (percentage >= 50) return "D";
            return "F";
        }
    }
}
