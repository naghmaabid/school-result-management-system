using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using SchoolResultManagementSystem.Helpers;
using SchoolResultManagementSystem.Models;

namespace SchoolResultManagementSystem.Forms
{
    public class LoginForm : Form
    {
        private Label _titleLabel;
        private Label _subtitleLabel;
        private Label _usernameLabel;
        private TextBox _usernameTextBox;
        private Label _passwordLabel;
        private TextBox _passwordTextBox;
        private Label _roleLabel;
        private ComboBox _roleComboBox;
        private Button _loginButton;
        private Label _statusLabel;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "School Result Management System — Login";
            Size = new Size(420, 380);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Font = new Font("Segoe UI", 10F);

            _titleLabel = new Label
            {
                Text = "School Result Management System",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 25),
                Size = new Size(360, 50)
            };

            _subtitleLabel = new Label
            {
                Text = "Sign in to continue",
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 75),
                Size = new Size(360, 25)
            };

            _usernameLabel = new Label { Text = "Username:", Location = new Point(40, 125), Size = new Size(100, 25) };
            _usernameTextBox = new TextBox { Location = new Point(150, 122), Size = new Size(220, 25) };

            _passwordLabel = new Label { Text = "Password:", Location = new Point(40, 160), Size = new Size(100, 25) };
            _passwordTextBox = new TextBox { Location = new Point(150, 157), Size = new Size(220, 25), UseSystemPasswordChar = true };

            _roleLabel = new Label { Text = "Login as:", Location = new Point(40, 195), Size = new Size(100, 25) };
            _roleComboBox = new ComboBox
            {
                Location = new Point(150, 192),
                Size = new Size(220, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _roleComboBox.Items.AddRange(new object[] { "Teacher", "Student" });
            _roleComboBox.SelectedIndex = 0;

            _loginButton = new Button
            {
                Text = "Login",
                Location = new Point(150, 235),
                Size = new Size(220, 35),
                BackColor = Color.FromArgb(41, 98, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _loginButton.Click += LoginButton_Click;

            _statusLabel = new Label
            {
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 285),
                Size = new Size(360, 40)
            };

            Controls.AddRange(new Control[]
            {
                _titleLabel, _subtitleLabel,
                _usernameLabel, _usernameTextBox,
                _passwordLabel, _passwordTextBox,
                _roleLabel, _roleComboBox,
                _loginButton, _statusLabel
            });

            AcceptButton = _loginButton;
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            string username = _usernameTextBox.Text.Trim();
            string password = _passwordTextBox.Text;
            string selectedRole = _roleComboBox.SelectedItem.ToString();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _statusLabel.Text = "Please enter both username and password.";
                return;
            }

            try
            {
                const string sql = "SELECT user_id, username, password, full_name, role " +
                                    "FROM users WHERE username = @username AND role = @role";

                DataTable table = DatabaseHelper.ExecuteQuery(
                    sql,
                    new MySqlParameter("@username", username),
                    new MySqlParameter("@role", selectedRole));

                if (table.Rows.Count == 0)
                {
                    _statusLabel.Text = $"No {selectedRole.ToLower()} account found with that username.";
                    return;
                }

                DataRow row = table.Rows[0];
                string storedHash = row["password"].ToString();

                if (!PasswordHelper.Verify(password, storedHash))
                {
                    _statusLabel.Text = "Incorrect password. Please try again.";
                    return;
                }

                var user = new User
                {
                    UserId = Convert.ToInt32(row["user_id"]),
                    Username = row["username"].ToString(),
                    FullName = row["full_name"].ToString(),
                    Role = row["role"].ToString()
                };

                Hide();

                Form dashboard = user.Role == "Teacher"
                    ? (Form)new TeacherDashboardForm(user)
                    : new StudentDashboardForm(user);

                dashboard.FormClosed += (s, args) => Close();
                dashboard.Show();
            }
            catch (MySqlException ex)
            {
                _statusLabel.Text = "Database connection failed. Is MySQL/XAMPP running?";
                MessageBox.Show(
                    $"Could not connect to the database.\n\nDetails: {ex.Message}\n\n" +
                    "Make sure XAMPP's MySQL service is running and the connection string " +
                    "in App.config matches your setup.",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                _statusLabel.Text = "An unexpected error occurred.";
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
