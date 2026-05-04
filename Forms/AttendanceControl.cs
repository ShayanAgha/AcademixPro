using System.Data;
using System.Data.SqlClient;
using AcademixPro.Helpers;

namespace AcademixPro.Forms
{
    public class AttendanceControl : UserControl
    {
        private DataGridView dgvAttendance = null!;
        private ComboBox cmbCourse = null!;
        private DataGridView dgvMark = null!;
        private DateTimePicker dtpDate = null!;

        public AttendanceControl()
        {
            Dock = DockStyle.Fill; BackColor = AppColors.Background;
            DoubleBuffered = true; Load += (_, __) => BuildUI();
        }

        private void BuildUI()
        {
            Controls.Clear();

            // Top form panel
            var formPanel = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = AppColors.Surface, Padding = new Padding(20) };
            Controls.Add(formPanel);
            formPanel.Controls.Add(new Label { Text = "Mark / View Attendance", Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold), ForeColor = AppColors.TextPrimary, AutoSize = true, Location = new Point(20, 10), BackColor = Color.Transparent });

            formPanel.Controls.Add(FL("Select Course", 20, 48));
            cmbCourse = new ComboBox { Location = new Point(20, 70), Size = new Size(340, 30) };
            UIHelper.StyleComboBox(cmbCourse);
            cmbCourse.SelectedIndexChanged += (_, __) => LoadAttendance();
            formPanel.Controls.Add(cmbCourse);

            formPanel.Controls.Add(FL("Date", 380, 48));
            dtpDate = UIHelper.CreateDatePicker();
            dtpDate.Location = new Point(380, 70); dtpDate.Size = new Size(180, 30);
            formPanel.Controls.Add(dtpDate);

            var btnMark = UIHelper.CreateButton("✅ Mark Attendance", AppColors.Primary, 180, 38);
            btnMark.Location = new Point(580, 70); btnMark.Click += BtnMark_Click;
            formPanel.Controls.Add(btnMark);

            var btnSummary = UIHelper.CreateButton("📊 Summary", AppColors.CardPurple, 130, 38);
            btnSummary.Location = new Point(770, 70); btnSummary.Click += BtnSummary_Click;
            formPanel.Controls.Add(btnSummary);

            // Split for marking (left) and history (right)
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 450, SplitterWidth = 8 };
            split.Panel1.BackColor = AppColors.Background; split.Panel2.BackColor = AppColors.Background;
            Controls.Add(split); formPanel.BringToFront();

            // Mark grid (editable)
            dgvMark = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGrid(dgvMark);
            dgvMark.ReadOnly = false;
            dgvMark.AllowUserToAddRows = false;
            split.Panel1.Controls.Add(dgvMark);

            // Title for mark panel
            var lblMark = new Label { Text = "📋 Enrolled Students", Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold), ForeColor = AppColors.TextPrimary, Dock = DockStyle.Top, Height = 32, BackColor = AppColors.SurfaceLight, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0) };
            split.Panel1.Controls.Add(lblMark); lblMark.BringToFront();

            // History grid
            dgvAttendance = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGrid(dgvAttendance);
            split.Panel2.Controls.Add(dgvAttendance);
            var lblHist = new Label { Text = "📜 Attendance History", Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold), ForeColor = AppColors.TextPrimary, Dock = DockStyle.Top, Height = 32, BackColor = AppColors.SurfaceLight, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0) };
            split.Panel2.Controls.Add(lblHist); lblHist.BringToFront();

            LoadCourses();
        }

        private void LoadCourses()
        {
            var dt = DatabaseHelper.ExecuteQuery(SqlQueries.Courses.GetForDropdown);
            cmbCourse.DataSource = dt; cmbCourse.DisplayMember = "Display"; cmbCourse.ValueMember = "CourseID";
        }

        private void LoadAttendance()
        {
            if (cmbCourse.SelectedValue == null || cmbCourse.SelectedValue is DataRowView) return;
            int courseId = Convert.ToInt32(cmbCourse.SelectedValue);

            // Load enrolled students for marking
            var enrolled = DatabaseHelper.ExecuteQuery(SqlQueries.Attendance.GetEnrolledForCourse, new[] { new SqlParameter("@CourseID", courseId) });
            // Add Status column for marking
            if (!enrolled.Columns.Contains("Status"))
            {
                enrolled.Columns.Add("Status", typeof(string));
                foreach (DataRow r in enrolled.Rows) r["Status"] = "Present";
            }
            if (!enrolled.Columns.Contains("Remarks"))
            {
                enrolled.Columns.Add("Remarks", typeof(string));
            }
            dgvMark.DataSource = enrolled;
            if (dgvMark.Columns.Contains("EnrollmentID")) dgvMark.Columns["EnrollmentID"].Visible = false;
            if (dgvMark.Columns.Contains("StudentCode")) dgvMark.Columns["StudentCode"].ReadOnly = true;
            if (dgvMark.Columns.Contains("StudentName")) dgvMark.Columns["StudentName"].ReadOnly = true;

            // Load history
            var history = DatabaseHelper.ExecuteQuery(SqlQueries.Attendance.GetByCourse, new[] { new SqlParameter("@CourseID", courseId) });
            dgvAttendance.DataSource = history;
            if (dgvAttendance.Columns.Contains("AttendanceID")) dgvAttendance.Columns["AttendanceID"].Visible = false;
            if (dgvAttendance.Columns.Contains("EnrollmentID")) dgvAttendance.Columns["EnrollmentID"].Visible = false;
        }

        private void BtnMark_Click(object? s, EventArgs e)
        {
            if (dgvMark.Rows.Count == 0) { MessageBox.Show("No students to mark."); return; }

            int count = 0;
            foreach (DataGridViewRow row in dgvMark.Rows)
            {
                int enrollId = Convert.ToInt32(row.Cells["EnrollmentID"].Value);
                string status = row.Cells["Status"].Value?.ToString() ?? "Present";
                if (status != "Present" && status != "Absent" && status != "Late") status = "Present";
                string remarks = row.Cells["Remarks"].Value?.ToString() ?? "";

                var p = new SqlParameter[]
                {
                    new("@EnrollmentID", enrollId),
                    new("@Date", dtpDate.Value.Date),
                    new("@Status", status),
                    new("@MarkedBy", Session.UserID),
                    new("@Remarks", remarks),
                };
                if (DatabaseHelper.ExecuteNonQuery(SqlQueries.Attendance.Insert, p) > 0) count++;
            }

            MessageBox.Show($"Attendance marked for {count} students.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadAttendance();
        }

        private void BtnSummary_Click(object? s, EventArgs e)
        {
            var dt = DatabaseHelper.ExecuteQuery(SqlQueries.Attendance.GetSummary);
            dgvAttendance.DataSource = dt;
        }

        private Label FL(string t, int x, int y) => new() { Text = t, Font = new Font("Segoe UI", 9), ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true, BackColor = Color.Transparent };
    }
}
