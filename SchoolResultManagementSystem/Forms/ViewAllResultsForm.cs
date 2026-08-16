using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using SchoolResultManagementSystem.Helpers;

namespace SchoolResultManagementSystem.Forms
{
    public class ViewAllResultsForm : Form
    {
        private ComboBox _classFilterComboBox;
        private Button _refreshButton;
        private DataGridView _resultsGrid;

        private ComboBox _reportCardStudentComboBox;
        private Button _generateReportButton;

        public ViewAllResultsForm()
        {
            InitializeComponent();
            LoadClassFilter();
            LoadResults();
            LoadStudentsForReportCard();
        }

        private void InitializeComponent()
        {
            Text = "View All Results";
            Size = new Size(800, 620);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5F);

            var filterLabel = new Label { Text = "Filter by class:", Location = new Point(20, 20), Size = new Size(100, 25) };
            _classFilterComboBox = new ComboBox
            {
                Location = new Point(120, 18), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList
            };
            _classFilterComboBox.SelectedIndexChanged += (s, e) => LoadResults();

            _refreshButton = new Button
            {
                Text = "Refresh",
                Location = new Point(290, 16),
                Size = new Size(100, 28),
                BackColor = Color.FromArgb(41, 98, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _refreshButton.Click += (s, e) => LoadResults();

            _resultsGrid = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(740, 320),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var reportLabel = new Label
            {
                Text = "Generate a report card:",
                Location = new Point(20, 400),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };

            _reportCardStudentComboBox = new ComboBox
            {
                Location = new Point(20, 430), Size = new Size(350, 25), DropDownStyle = ComboBoxStyle.DropDownList
            };

            _generateReportButton = new Button
            {
                Text = "View Report Card",
                Location = new Point(390, 428),
                Size = new Size(180, 30),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _generateReportButton.Click += GenerateReportButton_Click;

            Controls.AddRange(new Control[]
            {
                filterLabel, _classFilterComboBox, _refreshButton, _resultsGrid,
                reportLabel, _reportCardStudentComboBox, _generateReportButton
            });
        }

        private void LoadClassFilter()
        {
            DataTable table = DatabaseHelper.ExecuteQuery(
                "SELECT DISTINCT class_name FROM students ORDER BY class_name");

            _classFilterComboBox.Items.Add("All Classes");
            foreach (DataRow row in table.Rows)
            {
                _classFilterComboBox.Items.Add(row["class_name"].ToString());
            }
            _classFilterComboBox.SelectedIndex = 0;
        }

        private void LoadResults()
        {
            string sql = @"
                SELECT u.full_name AS Student, st.roll_no AS RollNo, st.class_name AS Class,
                       sub.subject_name AS Subject, r.exam_term AS Term,
                       r.marks_obtained AS Marks, sub.max_marks AS 'Max Marks'
                FROM results r
                JOIN students st ON st.student_id = r.student_id
                JOIN users u ON u.user_id = st.user_id
                JOIN subjects sub ON sub.subject_id = r.subject_id";

            string selectedClass = _classFilterComboBox.SelectedItem?.ToString();
            bool filterByClass = !string.IsNullOrEmpty(selectedClass) && selectedClass != "All Classes";

            if (filterByClass)
            {
                sql += " WHERE st.class_name = @cls";
            }
            sql += " ORDER BY st.roll_no, sub.subject_name";

            _resultsGrid.DataSource = filterByClass
                ? DatabaseHelper.ExecuteQuery(sql, new MySqlParameter("@cls", selectedClass))
                : DatabaseHelper.ExecuteQuery(sql);
        }

        private void LoadStudentsForReportCard()
        {
            const string sql = @"
                SELECT st.student_id AS StudentId, u.full_name AS FullName, st.roll_no AS RollNo
                FROM students st JOIN users u ON u.user_id = st.user_id
                ORDER BY st.roll_no";

            DataTable table = DatabaseHelper.ExecuteQuery(sql);
            table.Columns.Add("Display", typeof(string), "RollNo + ' - ' + FullName");

            _reportCardStudentComboBox.DisplayMember = "Display";
            _reportCardStudentComboBox.ValueMember = "StudentId";
            _reportCardStudentComboBox.DataSource = table;
        }

        private void GenerateReportButton_Click(object sender, EventArgs e)
        {
            if (_reportCardStudentComboBox.SelectedValue == null)
            {
                MessageBox.Show("Select a student first.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int studentId = Convert.ToInt32(_reportCardStudentComboBox.SelectedValue);
            string studentName = ((DataRowView)_reportCardStudentComboBox.SelectedItem)["FullName"].ToString();
            string rollNo = ((DataRowView)_reportCardStudentComboBox.SelectedItem)["RollNo"].ToString();

            const string sql = @"
                SELECT sub.subject_name AS Subject, r.exam_term AS Term,
                       r.marks_obtained AS Marks, sub.max_marks AS MaxMarks
                FROM results r
                JOIN subjects sub ON sub.subject_id = r.subject_id
                WHERE r.student_id = @sid
                ORDER BY r.exam_term, sub.subject_name";

            DataTable table = DatabaseHelper.ExecuteQuery(sql, new MySqlParameter("@sid", studentId));

            if (table.Rows.Count == 0)
            {
                MessageBox.Show("This student has no recorded results yet.", "No Data",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Report Card — {studentName} (Roll No: {rollNo})");
            sb.AppendLine(new string('-', 45));

            decimal totalObtained = 0, totalMax = 0;
            foreach (DataRow row in table.Rows)
            {
                decimal marks = Convert.ToDecimal(row["Marks"]);
                decimal maxMarks = Convert.ToDecimal(row["MaxMarks"]);
                totalObtained += marks;
                totalMax += maxMarks;

                sb.AppendLine($"{row["Subject"],-20} [{row["Term"],-8}]  {marks}/{maxMarks}");
            }

            decimal percentage = totalMax == 0 ? 0 : (totalObtained / totalMax) * 100;
            string grade = GradeHelper.GetGrade(percentage);

            sb.AppendLine(new string('-', 45));
            sb.AppendLine($"Total: {totalObtained}/{totalMax}   Percentage: {percentage:F2}%   Grade: {grade}");

            MessageBox.Show(sb.ToString(), "Report Card", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
