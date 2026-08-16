using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using SchoolResultManagementSystem.Helpers;

namespace SchoolResultManagementSystem.Forms
{
    public class ManageStudentsForm : Form
    {
        private DataGridView _studentsGrid;

        private TextBox _rollNoTextBox;
        private TextBox _nameTextBox;
        private TextBox _classTextBox;
        private TextBox _sectionTextBox;
        private TextBox _usernameTextBox;
        private TextBox _passwordTextBox;

        private Button _addButton;
        private Button _updateButton;
        private Button _deleteButton;
        private Button _clearButton;

        private int? _selectedStudentId;
        private int? _selectedUserId;

        public ManageStudentsForm()
        {
            InitializeComponent();
            LoadStudents();
        }

        private void InitializeComponent()
        {
            Text = "Manage Students";
            Size = new Size(780, 560);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5F);

            _studentsGrid = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(730, 250),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            _studentsGrid.CellClick += StudentsGrid_CellClick;

            int formTop = 290;
            _rollNoTextBox = AddField("Roll No:", formTop, 0);
            _nameTextBox = AddField("Full Name:", formTop, 1);
            _classTextBox = AddField("Class:", formTop + 45, 0);
            _sectionTextBox = AddField("Section:", formTop + 45, 1);
            _usernameTextBox = AddField("Username:", formTop + 90, 0);
            _passwordTextBox = AddField("Password:", formTop + 90, 1);

            _addButton = MakeActionButton("Add", 20, formTop + 145, Color.FromArgb(40, 167, 69));
            _updateButton = MakeActionButton("Update", 200, formTop + 145, Color.FromArgb(41, 98, 255));
            _deleteButton = MakeActionButton("Delete", 380, formTop + 145, Color.FromArgb(220, 53, 69));
            _clearButton = MakeActionButton("Clear", 560, formTop + 145, Color.Gray);

            _addButton.Click += AddButton_Click;
            _updateButton.Click += UpdateButton_Click;
            _deleteButton.Click += DeleteButton_Click;
            _clearButton.Click += (s, e) => ClearForm();

            Controls.Add(_studentsGrid);
            Controls.AddRange(new Control[] { _addButton, _updateButton, _deleteButton, _clearButton });
        }

        private TextBox AddField(string labelText, int top, int column)
        {
            int left = column == 0 ? 20 : 400;
            var label = new Label { Text = labelText, Location = new Point(left, top), Size = new Size(90, 25) };
            var textBox = new TextBox { Location = new Point(left + 95, top - 2), Size = new Size(240, 25) };
            Controls.Add(label);
            Controls.Add(textBox);
            return textBox;
        }

        private Button MakeActionButton(string text, int left, int top, Color color)
        {
            return new Button
            {
                Text = text,
                Location = new Point(left, top),
                Size = new Size(160, 35),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
        }

        private void LoadStudents()
        {
            const string sql = @"
                SELECT st.student_id AS StudentId, u.user_id AS UserId, st.roll_no AS RollNo,
                       u.full_name AS FullName, st.class_name AS Class, st.section AS Section,
                       u.username AS Username
                FROM students st
                JOIN users u ON u.user_id = st.user_id
                ORDER BY st.roll_no";

            _studentsGrid.DataSource = DatabaseHelper.ExecuteQuery(sql);

            if (_studentsGrid.Columns["StudentId"] != null) _studentsGrid.Columns["StudentId"].Visible = false;
            if (_studentsGrid.Columns["UserId"] != null) _studentsGrid.Columns["UserId"].Visible = false;
        }

        private void StudentsGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = _studentsGrid.Rows[e.RowIndex];
            _selectedStudentId = Convert.ToInt32(row.Cells["StudentId"].Value);
            _selectedUserId = Convert.ToInt32(row.Cells["UserId"].Value);

            _rollNoTextBox.Text = row.Cells["RollNo"].Value.ToString();
            _nameTextBox.Text = row.Cells["FullName"].Value.ToString();
            _classTextBox.Text = row.Cells["Class"].Value.ToString();
            _sectionTextBox.Text = row.Cells["Section"].Value.ToString();
            _usernameTextBox.Text = row.Cells["Username"].Value.ToString();
            _passwordTextBox.Text = "";
            _passwordTextBox.PlaceholderText = "(leave blank to keep unchanged)";
        }

        private bool ValidateRequiredFields(bool requirePassword)
        {
            if (string.IsNullOrWhiteSpace(_rollNoTextBox.Text) ||
                string.IsNullOrWhiteSpace(_nameTextBox.Text) ||
                string.IsNullOrWhiteSpace(_classTextBox.Text) ||
                string.IsNullOrWhiteSpace(_sectionTextBox.Text) ||
                string.IsNullOrWhiteSpace(_usernameTextBox.Text) ||
                (requirePassword && string.IsNullOrWhiteSpace(_passwordTextBox.Text)))
            {
                MessageBox.Show("Please fill in all fields.", "Missing Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (!ValidateRequiredFields(requirePassword: true)) return;

            try
            {
                long newUserId = DatabaseHelper.ExecuteInsertAndGetId(
                    "INSERT INTO users (username, password, full_name, role) VALUES (@u, @p, @n, 'Student')",
                    new MySqlParameter("@u", _usernameTextBox.Text.Trim()),
                    new MySqlParameter("@p", PasswordHelper.Hash(_passwordTextBox.Text)),
                    new MySqlParameter("@n", _nameTextBox.Text.Trim()));

                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO students (user_id, roll_no, class_name, section) VALUES (@uid, @roll, @cls, @sec)",
                    new MySqlParameter("@uid", newUserId),
                    new MySqlParameter("@roll", _rollNoTextBox.Text.Trim()),
                    new MySqlParameter("@cls", _classTextBox.Text.Trim()),
                    new MySqlParameter("@sec", _sectionTextBox.Text.Trim()));

                MessageBox.Show("Student added successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadStudents();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Could not add student.\n\n{ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            if (_selectedStudentId == null)
            {
                MessageBox.Show("Select a student from the list first.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!ValidateRequiredFields(requirePassword: false)) return;

            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE students SET roll_no=@roll, class_name=@cls, section=@sec WHERE student_id=@sid",
                    new MySqlParameter("@roll", _rollNoTextBox.Text.Trim()),
                    new MySqlParameter("@cls", _classTextBox.Text.Trim()),
                    new MySqlParameter("@sec", _sectionTextBox.Text.Trim()),
                    new MySqlParameter("@sid", _selectedStudentId));

                if (string.IsNullOrWhiteSpace(_passwordTextBox.Text))
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE users SET username=@u, full_name=@n WHERE user_id=@uid",
                        new MySqlParameter("@u", _usernameTextBox.Text.Trim()),
                        new MySqlParameter("@n", _nameTextBox.Text.Trim()),
                        new MySqlParameter("@uid", _selectedUserId));
                }
                else
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE users SET username=@u, full_name=@n, password=@p WHERE user_id=@uid",
                        new MySqlParameter("@u", _usernameTextBox.Text.Trim()),
                        new MySqlParameter("@n", _nameTextBox.Text.Trim()),
                        new MySqlParameter("@p", PasswordHelper.Hash(_passwordTextBox.Text)),
                        new MySqlParameter("@uid", _selectedUserId));
                }

                MessageBox.Show("Student updated successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadStudents();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Could not update student.\n\n{ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (_selectedStudentId == null)
            {
                MessageBox.Show("Select a student from the list first.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "This will permanently delete the student, their login, and all their results. Continue?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {

                DatabaseHelper.ExecuteNonQuery(
                    "DELETE FROM users WHERE user_id=@uid",
                    new MySqlParameter("@uid", _selectedUserId));

                MessageBox.Show("Student deleted.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadStudents();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Could not delete student.\n\n{ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            _selectedStudentId = null;
            _selectedUserId = null;
            _rollNoTextBox.Text = "";
            _nameTextBox.Text = "";
            _classTextBox.Text = "";
            _sectionTextBox.Text = "";
            _usernameTextBox.Text = "";
            _passwordTextBox.Text = "";
            _passwordTextBox.PlaceholderText = "";
            _studentsGrid.ClearSelection();
        }
    }
}
