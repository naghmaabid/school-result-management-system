using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using SchoolResultManagementSystem.Helpers;

namespace SchoolResultManagementSystem.Forms
{
    public class ManageSubjectsForm : Form
    {
        private DataGridView _subjectsGrid;
        private TextBox _subjectNameTextBox;
        private NumericUpDown _maxMarksUpDown;
        private Button _addButton;
        private Button _updateButton;
        private Button _deleteButton;
        private Button _clearButton;

        private int? _selectedSubjectId;

        public ManageSubjectsForm()
        {
            InitializeComponent();
            LoadSubjects();
        }

        private void InitializeComponent()
        {
            Text = "Manage Subjects";
            Size = new Size(520, 480);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5F);

            _subjectsGrid = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(470, 250),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            _subjectsGrid.CellClick += SubjectsGrid_CellClick;

            var nameLabel = new Label { Text = "Subject Name:", Location = new Point(20, 290), Size = new Size(110, 25) };
            _subjectNameTextBox = new TextBox { Location = new Point(140, 288), Size = new Size(200, 25) };

            var maxMarksLabel = new Label { Text = "Max Marks:", Location = new Point(20, 330), Size = new Size(110, 25) };
            _maxMarksUpDown = new NumericUpDown
            {
                Location = new Point(140, 328),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 1000,
                Value = 100
            };

            _addButton = MakeActionButton("Add", 20, 375, Color.FromArgb(40, 167, 69));
            _updateButton = MakeActionButton("Update", 140, 375, Color.FromArgb(41, 98, 255));
            _deleteButton = MakeActionButton("Delete", 260, 375, Color.FromArgb(220, 53, 69));
            _clearButton = MakeActionButton("Clear", 380, 375, Color.Gray);

            _addButton.Click += AddButton_Click;
            _updateButton.Click += UpdateButton_Click;
            _deleteButton.Click += DeleteButton_Click;
            _clearButton.Click += (s, e) => ClearForm();

            Controls.AddRange(new Control[]
            {
                _subjectsGrid, nameLabel, _subjectNameTextBox, maxMarksLabel, _maxMarksUpDown,
                _addButton, _updateButton, _deleteButton, _clearButton
            });
        }

        private Button MakeActionButton(string text, int left, int top, Color color)
        {
            return new Button
            {
                Text = text,
                Location = new Point(left, top),
                Size = new Size(105, 35),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
        }

        private void LoadSubjects()
        {
            const string sql = "SELECT subject_id AS SubjectId, subject_name AS Subject, max_marks AS 'Max Marks' " +
                                "FROM subjects ORDER BY subject_name";
            _subjectsGrid.DataSource = DatabaseHelper.ExecuteQuery(sql);

            if (_subjectsGrid.Columns["SubjectId"] != null)
                _subjectsGrid.Columns["SubjectId"].Visible = false;
        }

        private void SubjectsGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = _subjectsGrid.Rows[e.RowIndex];
            _selectedSubjectId = Convert.ToInt32(row.Cells["SubjectId"].Value);
            _subjectNameTextBox.Text = row.Cells["Subject"].Value.ToString();
            _maxMarksUpDown.Value = Convert.ToDecimal(row.Cells["Max Marks"].Value);
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_subjectNameTextBox.Text))
            {
                MessageBox.Show("Enter a subject name.", "Missing Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO subjects (subject_name, max_marks) VALUES (@name, @max)",
                    new MySqlParameter("@name", _subjectNameTextBox.Text.Trim()),
                    new MySqlParameter("@max", _maxMarksUpDown.Value));

                ClearForm();
                LoadSubjects();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Could not add subject.\n\n{ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            if (_selectedSubjectId == null)
            {
                MessageBox.Show("Select a subject from the list first.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE subjects SET subject_name=@name, max_marks=@max WHERE subject_id=@id",
                    new MySqlParameter("@name", _subjectNameTextBox.Text.Trim()),
                    new MySqlParameter("@max", _maxMarksUpDown.Value),
                    new MySqlParameter("@id", _selectedSubjectId));

                ClearForm();
                LoadSubjects();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Could not update subject.\n\n{ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (_selectedSubjectId == null)
            {
                MessageBox.Show("Select a subject from the list first.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "This will also delete any results recorded for this subject. Continue?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "DELETE FROM subjects WHERE subject_id=@id",
                    new MySqlParameter("@id", _selectedSubjectId));

                ClearForm();
                LoadSubjects();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Could not delete subject.\n\n{ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            _selectedSubjectId = null;
            _subjectNameTextBox.Text = "";
            _maxMarksUpDown.Value = 100;
            _subjectsGrid.ClearSelection();
        }
    }
}
