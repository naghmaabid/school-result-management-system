using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using SchoolResultManagementSystem.Helpers;

namespace SchoolResultManagementSystem.Forms
{
    public class EnterResultsForm : Form
    {
        private ComboBox _studentComboBox;
        private ComboBox _subjectComboBox;
        private ComboBox _examTermComboBox;
        private NumericUpDown _marksUpDown;
        private Button _saveButton;
        private DataGridView _recentResultsGrid;

        public EnterResultsForm()
        {
            InitializeComponent();
            LoadStudents();
            LoadSubjects();
            LoadRecentResults();
        }

        private void InitializeComponent()
        {
            Text = "Enter / Update Results";
            Size = new Size(680, 560);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5F);

            var studentLabel = new Label { Text = "Student:", Location = new Point(20, 20), Size = new Size(90, 25) };
            _studentComboBox = new ComboBox
            {
                Location = new Point(120, 18), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList
            };

            var subjectLabel = new Label { Text = "Subject:", Location = new Point(20, 60), Size = new Size(90, 25) };
            _subjectComboBox = new ComboBox
            {
                Location = new Point(120, 58), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList
            };

            var termLabel = new Label { Text = "Exam Term:", Location = new Point(20, 100), Size = new Size(90, 25) };
            _examTermComboBox = new ComboBox
            {
                Location = new Point(120, 98), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList
            };
            _examTermComboBox.Items.AddRange(new object[] { "Midterm", "Final" });
            _examTermComboBox.SelectedIndex = 0;

            var marksLabel = new Label { Text = "Marks Obtained:", Location = new Point(20, 140), Size = new Size(110, 25) };
            _marksUpDown = new NumericUpDown
            {
                Location = new Point(140, 138), Size = new Size(100, 25), Minimum = 0, Maximum = 1000, DecimalPlaces = 2
            };

            _saveButton = new Button
            {
                Text = "Save Result",
                Location = new Point(120, 180),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(41, 98, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _saveButton.Click += SaveButton_Click;

            var recentLabel = new Label
            {
                Text = "Recently recorded results:",
                Location = new Point(20, 235),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };

            _recentResultsGrid = new DataGridView
            {
                Location = new Point(20, 265),
                Size = new Size(620, 250),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            Controls.AddRange(new Control[]
            {
                studentLabel, _studentComboBox,
                subjectLabel, _subjectComboBox,
                termLabel, _examTermComboBox,
                marksLabel, _marksUpDown,
                _saveButton, recentLabel, _recentResultsGrid
            });
        }

        private void LoadStudents()
        {
            const string sql = @"
                SELECT st.student_id AS StudentId, u.full_name AS FullName, st.roll_no AS RollNo
                FROM students st JOIN users u ON u.user_id = st.user_id
                ORDER BY st.roll_no";

            DataTable table = DatabaseHelper.ExecuteQuery(sql);
            _studentComboBox.DisplayMember = "Display";
            _studentComboBox.ValueMember = "StudentId";

            table.Columns.Add("Display", typeof(string), "RollNo + ' - ' + FullName");
            _studentComboBox.DataSource = table;
        }

        private void LoadSubjects()
        {
            const string sql = "SELECT subject_id AS SubjectId, subject_name AS SubjectName FROM subjects ORDER BY subject_name";
            DataTable table = DatabaseHelper.ExecuteQuery(sql);
            _subjectComboBox.DisplayMember = "SubjectName";
            _subjectComboBox.ValueMember = "SubjectId";
            _subjectComboBox.DataSource = table;
        }

        private void LoadRecentResults()
        {
            const string sql = @"
                SELECT u.full_name AS Student, sub.subject_name AS Subject,
                       r.exam_term AS Term, r.marks_obtained AS Marks, r.recorded_at AS 'Recorded At'
                FROM results r
                JOIN students st ON st.student_id = r.student_id
                JOIN users u ON u.user_id = st.user_id
                JOIN subjects sub ON sub.subject_id = r.subject_id
                ORDER BY r.recorded_at DESC
                LIMIT 20";

            _recentResultsGrid.DataSource = DatabaseHelper.ExecuteQuery(sql);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (_studentComboBox.SelectedValue == null || _subjectComboBox.SelectedValue == null)
            {
                MessageBox.Show("Select a student and a subject.", "Missing Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int studentId = Convert.ToInt32(_studentComboBox.SelectedValue);
            int subjectId = Convert.ToInt32(_subjectComboBox.SelectedValue);
            string examTerm = _examTermComboBox.SelectedItem.ToString();
            decimal marks = _marksUpDown.Value;

            try
            {
                // Upsert: update if a result for this student+subject+term already exists, else insert.
                object existing = DatabaseHelper.ExecuteScalar(
                    "SELECT result_id FROM results WHERE student_id=@sid AND subject_id=@subid AND exam_term=@term",
                    new MySqlParameter("@sid", studentId),
                    new MySqlParameter("@subid", subjectId),
                    new MySqlParameter("@term", examTerm));

                if (existing != null)
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE results SET marks_obtained=@marks WHERE result_id=@rid",
                        new MySqlParameter("@marks", marks),
                        new MySqlParameter("@rid", existing));
                }
                else
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO results (student_id, subject_id, exam_term, marks_obtained) " +
                        "VALUES (@sid, @subid, @term, @marks)",
                        new MySqlParameter("@sid", studentId),
                        new MySqlParameter("@subid", subjectId),
                        new MySqlParameter("@term", examTerm),
                        new MySqlParameter("@marks", marks));
                }

                MessageBox.Show("Result saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadRecentResults();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Could not save result.\n\n{ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
