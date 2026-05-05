using System.Data;
using System.Data.SqlClient;
using AcademixPro.Helpers;

namespace AcademixPro.Forms
{
    public class EnrollmentsControl : UserControl
    {
        private DataGridView dgvEnrollments = null!;
        private ComboBox cmbStudent = null!, cmbCourse = null!;

        public EnrollmentsControl()
        {
            Dock = DockStyle.Fill; BackColor = AppColors.Background;
            DoubleBuffered = true; Load += (_, __) => BuildUI();
        }

        private void BuildUI()
        {
            Controls.Clear();

            var formPanel = new Panel { Dock = DockStyle.Top, Height = 160, BackColor = AppColors.Surface, Padding = new Padding(20) };
            Controls.Add(formPanel);

            formPanel.Controls.Add(new Label { Text = "Enroll Student in Course", Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold), ForeColor = AppColors.TextPrimary, AutoSize = true, Location = new Point(20, 12), BackColor = Color.Transparent });

            formPanel.Controls.Add(FL("Student", 20, 50));
            cmbStudent = new ComboBox { Location = new Point(20, 72), Size = new Size(340, 30) };
            UIHelper.StyleComboBox(cmbStudent);
            cmbStudent.SelectedIndexChanged += (_, __) => LoadAvailableCourses();
            formPanel.Controls.Add(cmbStudent);

            formPanel.Controls.Add(FL("Available Course", 380, 50));
            cmbCourse = new ComboBox { Location = new Point(380, 72), Size = new Size(340, 30) };
            UIHelper.StyleComboBox(cmbCourse);
            formPanel.Controls.Add(cmbCourse);

            var btnEnroll = UIHelper.CreateButton("✅ Enroll", AppColors.Primary, 140, 38);
            btnEnroll.Location = new Point(740, 72);
            btnEnroll.Click += BtnEnroll_Click;
            formPanel.Controls.Add(btnEnroll);

            var btnDrop = UIHelper.CreateButton("❌ Drop Selected", AppColors.Danger, 160, 38);
            btnDrop.Location = new Point(20, 112);
            btnDrop.Click += BtnDrop_Click;
            formPanel.Controls.Add(btnDrop);

            var btnRefresh = UIHelper.CreateButton("↻ Refresh", AppColors.SurfaceLight, 110, 38);
            btnRefresh.Location = new Point(190, 112); btnRefresh.ForeColor = AppColors.TextSecondary;
            btnRefresh.Click += (_, __) => LoadData();
            formPanel.Controls.Add(btnRefresh);

            // Grid
            dgvEnrollments = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGrid(dgvEnrollments);
            Controls.Add(dgvEnrollments);
            formPanel.BringToFront();

            LoadStudents(); LoadData();
        }

        private void LoadStudents()
        {
            var dt = DatabaseHelper.ExecuteQuery("SELECT StudentID, StudentCode + ' - ' + FullName AS Display FROM Students WHERE IsActive=1 ORDER BY FullName");
            cmbStudent.DataSource = dt; cmbStudent.DisplayMember = "Display"; cmbStudent.ValueMember = "StudentID";
        }

        private void LoadAvailableCourses()
        {
            if (cmbStudent.SelectedValue == null || cmbStudent.SelectedValue is DataRowView) return;
            var p = new[] { new SqlParameter("@StudentID", cmbStudent.SelectedValue) };
            var dt = DatabaseHelper.ExecuteQuery(SqlQueries.Enrollments.GetAvailableCourses, p);
            cmbCourse.DataSource = dt; cmbCourse.DisplayMember = "Display"; cmbCourse.ValueMember = "CourseID";
        }

        private void LoadData()
        {
            dgvEnrollments.DataSource = DatabaseHelper.ExecuteQuery(SqlQueries.Enrollments.GetAll);
            if (dgvEnrollments.Columns.Contains("EnrollmentID")) dgvEnrollments.Columns["EnrollmentID"].Visible = false;
            var map = new Dictionary<string, string>
            {
                ["StudentCode"] = "Student ID",
                ["StudentName"] = "Student Name",
                ["CourseCode"]  = "Course Code",
                ["CourseName"]  = "Course Name",
                ["EnrollDate"]  = "Enrolled On",
                ["Status"]      = "Status",
            };
            foreach (var kv in map)
                if (dgvEnrollments.Columns.Contains(kv.Key))
                    dgvEnrollments.Columns[kv.Key].HeaderText = kv.Value;
        }

        private void BtnEnroll_Click(object? s, EventArgs e)
        {
            if (cmbStudent.SelectedValue == null || cmbCourse.SelectedValue == null || cmbStudent.SelectedValue is DataRowView || cmbCourse.SelectedValue is DataRowView)
            { MessageBox.Show("Select both student and course."); return; }

            var p = new[] { new SqlParameter("@StudentID", cmbStudent.SelectedValue), new SqlParameter("@CourseID", cmbCourse.SelectedValue) };
            try
            {
                DatabaseHelper.ExecuteStoredProcedure(SqlQueries.Enrollments.EnrollSP, p);
                MessageBox.Show("Enrollment successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData(); LoadAvailableCourses();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnDrop_Click(object? s, EventArgs e)
        {
            if (dgvEnrollments.CurrentRow == null) { MessageBox.Show("Select an enrollment to drop."); return; }
            int id = Convert.ToInt32(dgvEnrollments.CurrentRow.Cells["EnrollmentID"].Value);
            if (MessageBox.Show("Drop this enrollment?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            DatabaseHelper.ExecuteNonQuery(SqlQueries.Enrollments.DropCourse, new[] { new SqlParameter("@EnrollmentID", id) });
            LoadData();
        }

        private void InitializeComponent()
        {

        }

        private Label FL(string t, int x, int y) => new() { Text = t, Font = new Font("Segoe UI", 9), ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true, BackColor = Color.Transparent };
    }
}
