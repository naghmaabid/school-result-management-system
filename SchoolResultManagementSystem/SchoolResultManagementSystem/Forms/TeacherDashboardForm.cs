using System;
using System.Drawing;
using System.Windows.Forms;
using SchoolResultManagementSystem.Models;

namespace SchoolResultManagementSystem.Forms
{
    public class TeacherDashboardForm : Form
    {
        private readonly User _teacher;

        private Label _welcomeLabel;
        private Button _manageStudentsButton;
        private Button _manageSubjectsButton;
        private Button _enterResultsButton;
        private Button _viewAllResultsButton;
        private Button _logoutButton;

        public TeacherDashboardForm(User teacher)
        {
            _teacher = teacher;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Teacher Dashboard — School Result Management System";
            Size = new Size(480, 420);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Font = new Font("Segoe UI", 10F);

            _welcomeLabel = new Label
            {
                Text = $"Welcome, {_teacher.FullName}",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 25),
                Size = new Size(420, 35)
            };

            _manageStudentsButton = MakeMenuButton("Manage Students", 90);
            _manageSubjectsButton = MakeMenuButton("Manage Subjects", 150);
            _enterResultsButton = MakeMenuButton("Enter / Update Results", 210);
            _viewAllResultsButton = MakeMenuButton("View All Results & Report Cards", 270);

            _logoutButton = new Button
            {
                Text = "Logout",
                Location = new Point(140, 335),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            _manageStudentsButton.Click += (s, e) => new ManageStudentsForm().ShowDialog();
            _manageSubjectsButton.Click += (s, e) => new ManageSubjectsForm().ShowDialog();
            _enterResultsButton.Click += (s, e) => new EnterResultsForm().ShowDialog();
            _viewAllResultsButton.Click += (s, e) => new ViewAllResultsForm().ShowDialog();
            _logoutButton.Click += (s, e) => Close();

            Controls.AddRange(new Control[]
            {
                _welcomeLabel,
                _manageStudentsButton, _manageSubjectsButton,
                _enterResultsButton, _viewAllResultsButton,
                _logoutButton
            });
        }

        private Button MakeMenuButton(string text, int top)
        {
            return new Button
            {
                Text = text,
                Location = new Point(70, top),
                Size = new Size(340, 45),
                BackColor = Color.FromArgb(41, 98, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F)
            };
        }
    }
}
