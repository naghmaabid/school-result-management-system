using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using SchoolResultManagementSystem.Helpers;
using SchoolResultManagementSystem.Models;

namespace SchoolResultManagementSystem.Forms
{
    public class StudentDashboardForm : Form
    {
        private readonly User _student;

        private Label _welcomeLabel;
        private Label _infoLabel;
        private DataGridView _resultsGrid;
        private Label _summaryLabel;
        private Button _logoutButton;

        public StudentDashboardForm(User student)
        {
            _student = student;
            InitializeComponent();
            LoadResults();
        }

        private void InitializeComponent()
        {
            Text = "Student Dashboard — My Results";
            Size = new Size(650, 520);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Font = new Font("Segoe UI", 10F);

            _welcomeLabel = new Label
            {
                Text = $"Welcome, {_student.FullName}",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(500, 30)
            };

            _infoLabel = new Label
            {
                Text = "Your Report Card",
                ForeColor = Color.Gray,
                Location = new Point(20, 50),
                Size = new Size(500, 25)
            };

            _resultsGrid = new DataGridView
            {
                Location = new Point(20, 85),
                Size = new Size(590, 300),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            _summaryLabel = new Label
            {
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(20, 400),
                Size = new Size(590, 50),
                ForeColor = Color.FromArgb(41, 98, 255)
            };

            _logoutButton = new Button
            {
                Text = "Logout",
                Location = new Point(230, 450),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _logoutButton.Click += (s, e) => Close();

            Controls.AddRange(new Control[]
            {
                _welcomeLabel, _infoLabel, _resultsGrid, _summaryLabel, _logoutButton
            });
        }

        private void LoadResults()
        {
            try
            {
                const string sql = @"
                    SELECT sub.subject_name AS Subject,
                           r.exam_term      AS Term,
                           r.marks_obtained AS Marks,
                           sub.max_marks    AS 'Max Marks'
                    FROM results r
                    JOIN subjects sub ON sub.subject_id = r.subject_id
                    JOIN students st ON st.student_id = r.student_id
                    WHERE st.user_id = @userId
                    ORDER BY r.exam_term, sub.subject_name";

                DataTable table = DatabaseHelper.ExecuteQuery(sql, new MySqlParameter("@userId", _student.UserId));
                _resultsGrid.DataSource = table;

                if (table.Rows.Count == 0)
                {
                    _summaryLabel.Text = "No results have been recorded yet.";
                    return;
                }

                decimal totalObtained = 0;
                decimal totalMax = 0;
                foreach (DataRow row in table.Rows)
                {
                    totalObtained += Convert.ToDecimal(row["Marks"]);
                    totalMax += Convert.ToDecimal(row["Max Marks"]);
                }

                decimal percentage = totalMax == 0 ? 0 : (totalObtained / totalMax) * 100;
                string grade = GradeHelper.GetGrade(percentage);

                _summaryLabel.Text = $"Total: {totalObtained}/{totalMax}   |   " +
                                      $"Percentage: {percentage:F2}%   |   Grade: {grade}";
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    $"Could not load your results.\n\nDetails: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
