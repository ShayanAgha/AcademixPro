using System.Data;
using System.Data.SqlClient;
using AcademixPro.Helpers;

namespace AcademixPro.Forms
{
    public class ResultsControl : UserControl
    {
        private DataGridView dgvResults = null!;
        private ComboBox cmbEnrollment = null!;
        private TextBox txtAssignments = null!, txtMidterm = null!, txtFinal = null!;

        public ResultsControl()
        {
            Dock = DockStyle.Fill; BackColor = AppColors.Background;
            DoubleBuffered = true; Load += (_, __) => BuildUI();
        }

        private void BuildUI()
        {
            Controls.Clear();

            var formPanel = new Panel { Dock = DockStyle.Top, Height = 180, BackColor = AppColors.Surface, Padding = new Padding(20) };
            Controls.Add(formPanel);
            formPanel.Controls.Add(new Label { Text = "Enter / Update Results", Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold), ForeColor = AppColors.TextPrimary, AutoSize = true, Location = new Point(20, 10), BackColor = Color.Transparent });

            formPanel.Controls.Add(FL("Enrollment (Student - Course)", 20, 48));
            cmbEnrollment = new ComboBox { Location = new Point(20, 70), Size = new Size(480, 30) };
            UIHelper.StyleComboBox(cmbEnrollment);
            formPanel.Controls.Add(cmbEnrollment);

            formPanel.Controls.Add(FL("Assignments (0-20)", 20, 108));
            txtAssignments = STB(20, 130, 140); formPanel.Controls.Add(txtAssignments);

            formPanel.Controls.Add(FL("Midterm (0-30)", 180, 108));
            txtMidterm = STB(180, 130, 140); formPanel.Controls.Add(txtMidterm);

            formPanel.Controls.Add(FL("Final Exam (0-50)", 340, 108));
            txtFinal = STB(340, 130, 140); formPanel.Controls.Add(txtFinal);

            var btnSave = UIHelper.CreateButton("💾 Save Result", AppColors.Primary, 150, 38);
            btnSave.Location = new Point(500, 130); btnSave.Click += BtnSave_Click;
            formPanel.Controls.Add(btnSave);

            var btnLock = UIHelper.CreateButton("🔒 Lock Selected", AppColors.CardAmber, 160, 38);
            btnLock.Location = new Point(660, 130); btnLock.Click += BtnLock_Click;
            formPanel.Controls.Add(btnLock);

            var btnRefresh = UIHelper.CreateButton("↻ Refresh", AppColors.SurfaceLight, 110, 38);
            btnRefresh.Location = new Point(830, 130); btnRefresh.ForeColor = AppColors.TextSecondary;
            btnRefresh.Click += (_, __) => LoadData();
            formPanel.Controls.Add(btnRefresh);

            // Grid
            dgvResults = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGrid(dgvResults);
            Controls.Add(dgvResults); formPanel.BringToFront();

            LoadEnrollments(); LoadData();
        }

        private void LoadEnrollments()
        {
            var dt = DatabaseHelper.ExecuteQuery(SqlQueries.Results.GetEnrollmentsWithoutResult);
            if (!dt.Columns.Contains("Display"))
            {
                dt.Columns.Add("Display", typeof(string));
                foreach (DataRow r in dt.Rows)
                    r["Display"] = r["StudentDisplay"].ToString() + " | " + r["CourseDisplay"].ToString();
            }
            cmbEnrollment.DataSource = dt; cmbEnrollment.DisplayMember = "Display"; cmbEnrollment.ValueMember = "EnrollmentID";
        }

        private void LoadData()
        {
            dgvResults.DataSource = DatabaseHelper.ExecuteQuery(SqlQueries.Results.GetAll);
            if (dgvResults.Columns.Contains("ResultID")) dgvResults.Columns["ResultID"].Visible = false;
            var map = new Dictionary<string, string>
            {
                ["StudentCode"]  = "Student ID",
                ["StudentName"]  = "Student Name",
                ["CourseCode"]   = "Course Code",
                ["CourseName"]   = "Course Name",
                ["Assignments"]  = "Assignments",
                ["Midterm"]      = "Midterm",
                ["FinalExam"]    = "Final Exam",
                ["TotalMarks"]   = "Total Marks",
                ["Grade"]        = "Grade",
                ["GradePoints"]  = "GPA Points",
                ["IsLocked"]     = "Locked",
            };
            foreach (var kv in map)
                if (dgvResults.Columns.Contains(kv.Key))
                    dgvResults.Columns[kv.Key].HeaderText = kv.Value;
        }

        private void BtnSave_Click(object? s, EventArgs e)
        {
            if (cmbEnrollment.SelectedValue == null || cmbEnrollment.SelectedValue is DataRowView) { MessageBox.Show("Select an enrollment."); return; }
            if (!decimal.TryParse(txtAssignments.Text, out decimal a) || a < 0 || a > 20) { MessageBox.Show("Assignments must be 0-20."); return; }
            if (!decimal.TryParse(txtMidterm.Text, out decimal m) || m < 0 || m > 30) { MessageBox.Show("Midterm must be 0-30."); return; }
            if (!decimal.TryParse(txtFinal.Text, out decimal f) || f < 0 || f > 50) { MessageBox.Show("Final must be 0-50."); return; }

            int enrollId = Convert.ToInt32(cmbEnrollment.SelectedValue);
            var p = new SqlParameter[] { new("@EnrollmentID", enrollId), new("@Assignments", a), new("@Midterm", m), new("@FinalExam", f), new("@EnteredBy", Session.UserID) };

            if (DatabaseHelper.ExecuteNonQuery(SqlQueries.Results.Insert, p) > 0)
            {
                var resultId = DatabaseHelper.ExecuteScalar("SELECT MAX(ResultID) FROM Results WHERE EnrollmentID=@EID", new[] { new SqlParameter("@EID", enrollId) });
                if (resultId != null)
                    DatabaseHelper.ExecuteSP(SqlQueries.Results.AssignGradeSP, new[] { new SqlParameter("@ResultID", Convert.ToInt32(resultId)) });

                MessageBox.Show("Result saved and grade assigned!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtAssignments.Clear(); txtMidterm.Clear(); txtFinal.Clear();
                LoadEnrollments(); LoadData();
            }
        }

        private void BtnLock_Click(object? s, EventArgs e)
        {
            if (dgvResults.CurrentRow == null) { MessageBox.Show("Select a result to lock."); return; }
            int id = Convert.ToInt32(dgvResults.CurrentRow.Cells["ResultID"].Value);
            if (MessageBox.Show("Lock this result? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            DatabaseHelper.ExecuteNonQuery(SqlQueries.Results.LockResult, new[] { new SqlParameter("@ResultID", id) });
            LoadData();
        }

        private Label FL(string t, int x, int y) => new() { Text = t, Font = new Font("Segoe UI", 9), ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true, BackColor = Color.Transparent };
        private TextBox STB(int x, int y, int w) { var tb = new TextBox { Location = new Point(x, y), Size = new Size(w, 30) }; UIHelper.StyleTextBox(tb); return tb; }
    }
}
