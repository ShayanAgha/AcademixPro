using System.Data;
using System.Data.SqlClient;
using AcademixPro.Helpers;

namespace AcademixPro.Forms
{
    public class CoursesControl : UserControl
    {
        private DataGridView dgvCourses = null!;
        private TextBox txtCode = null!, txtName = null!, txtCapacity = null!;
        private ComboBox cmbDept = null!, cmbInstructor = null!, cmbSemester = null!, cmbCredits = null!;
        private int _selectedId = 0;

        public CoursesControl()
        {
            Dock = DockStyle.Fill; BackColor = AppColors.Background;
            DoubleBuffered = true; Load += (_, __) => BuildUI();
        }

        private void BuildUI()
        {
            Controls.Clear();
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.Transparent };
            var btnRefresh = UIHelper.CreateButton("↻ Refresh", AppColors.SurfaceLight, 110, 36);
            btnRefresh.Location = new Point(0, 10); btnRefresh.ForeColor = AppColors.TextSecondary;
            btnRefresh.Click += (_, __) => LoadData(); topPanel.Controls.Add(btnRefresh);
            Controls.Add(topPanel);

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterWidth = 8 };
            split.Panel1.BackColor = AppColors.Background; split.Panel2.BackColor = AppColors.Background;
            Controls.Add(split); topPanel.BringToFront();
            split.SizeChanged += (_, __) => { int t = (int)(split.Width * 0.6); if (t > 0 && t < split.Width - 50) try { split.SplitterDistance = t; } catch { } };

            dgvCourses = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGrid(dgvCourses); dgvCourses.SelectionChanged += DgvSel;
            split.Panel1.Controls.Add(dgvCourses);

            var fp = new Panel { Dock = DockStyle.Fill, BackColor = AppColors.Surface, AutoScroll = true, Padding = new Padding(20) };
            split.Panel2.Controls.Add(fp);
            fp.Controls.Add(new Label { Text = "Course Details", Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold), ForeColor = AppColors.TextPrimary, AutoSize = true, Location = new Point(20, 16), BackColor = Color.Transparent });

            int y = 52, x = 20, w = 280;
            fp.Controls.Add(FL("Course Code *", x, y)); txtCode = STB(x, y + 22, w); fp.Controls.Add(txtCode); y += 60;
            fp.Controls.Add(FL("Course Name *", x, y)); txtName = STB(x, y + 22, w); fp.Controls.Add(txtName); y += 60;
            fp.Controls.Add(FL("Credit Hours *", x, y));
            cmbCredits = new ComboBox { Location = new Point(x, y + 22), Size = new Size(w, 30) }; UIHelper.StyleComboBox(cmbCredits);
            for (int i = 1; i <= 6; i++) cmbCredits.Items.Add(i); cmbCredits.SelectedIndex = 2;
            fp.Controls.Add(cmbCredits); y += 60;

            fp.Controls.Add(FL("Department *", x, y));
            cmbDept = new ComboBox { Location = new Point(x, y + 22), Size = new Size(w, 30) }; UIHelper.StyleComboBox(cmbDept);
            fp.Controls.Add(cmbDept); y += 60;

            fp.Controls.Add(FL("Instructor", x, y));
            cmbInstructor = new ComboBox { Location = new Point(x, y + 22), Size = new Size(w, 30) }; UIHelper.StyleComboBox(cmbInstructor);
            fp.Controls.Add(cmbInstructor); y += 60;

            fp.Controls.Add(FL("Semester", x, y));
            cmbSemester = new ComboBox { Location = new Point(x, y + 22), Size = new Size(w, 30) }; UIHelper.StyleComboBox(cmbSemester);
            for (int i = 1; i <= 12; i++) cmbSemester.Items.Add(i); cmbSemester.SelectedIndex = 0;
            fp.Controls.Add(cmbSemester); y += 60;

            fp.Controls.Add(FL("Capacity", x, y)); txtCapacity = STB(x, y + 22, w); txtCapacity.Text = "40"; fp.Controls.Add(txtCapacity); y += 60;

            var btnAdd = UIHelper.CreateButton("➕ Add", AppColors.Primary, 130, 38); btnAdd.Location = new Point(20, y); btnAdd.Click += BtnAdd; fp.Controls.Add(btnAdd);
            var btnUpd = UIHelper.CreateButton("✏️ Update", AppColors.CardEmerald, 130, 38); btnUpd.Location = new Point(160, y); btnUpd.Click += BtnUpd; fp.Controls.Add(btnUpd);
            var btnDel = UIHelper.CreateButton("🗑️ Deactivate", AppColors.Danger, 130, 38); btnDel.Location = new Point(20, y + 48); btnDel.Click += BtnDel; fp.Controls.Add(btnDel);
            var btnClr = UIHelper.CreateButton("🔄 Clear", AppColors.SurfaceLight, 130, 38); btnClr.ForeColor = AppColors.TextSecondary; btnClr.Location = new Point(160, y + 48); btnClr.Click += (_, __) => ClearForm(); fp.Controls.Add(btnClr);

            LoadDepts(); LoadInstructors(); LoadData();
        }

        private void LoadData()
        {
            dgvCourses.DataSource = DatabaseHelper.ExecuteQuery(SqlQueries.Courses.GetAll);
            if (dgvCourses.Columns.Contains("CourseID")) dgvCourses.Columns["CourseID"].Visible = false;
            var map = new Dictionary<string, string>
            {
                ["CourseCode"]   = "Course Code",
                ["CourseName"]   = "Course Name",
                ["CreditHours"]  = "Credits",
                ["Department"]   = "Department",
                ["Instructor"]   = "Instructor",
                ["Semester"]     = "Semester",
                ["Capacity"]     = "Capacity",
                ["Enrolled"]     = "Enrolled",
                ["IsActive"]     = "Active",
            };
            foreach (var kv in map)
                if (dgvCourses.Columns.Contains(kv.Key))
                    dgvCourses.Columns[kv.Key].HeaderText = kv.Value;
        }
        private void LoadDepts() { var dt = DatabaseHelper.ExecuteQuery(SqlQueries.Departments.GetForDropdown); cmbDept.DataSource = dt; cmbDept.DisplayMember = "DeptName"; cmbDept.ValueMember = "DeptID"; }
        private void LoadInstructors() { var dt = DatabaseHelper.ExecuteQuery("SELECT InstructorID, FullName FROM Instructors WHERE IsActive=1 ORDER BY FullName"); cmbInstructor.DataSource = dt; cmbInstructor.DisplayMember = "FullName"; cmbInstructor.ValueMember = "InstructorID"; }

        private void DgvSel(object? s, EventArgs e)
        {
            if (dgvCourses.CurrentRow == null) return; var r = dgvCourses.CurrentRow;
            _selectedId = Convert.ToInt32(r.Cells["CourseID"].Value);
            txtCode.Text = r.Cells["CourseCode"].Value?.ToString() ?? "";
            txtName.Text = r.Cells["CourseName"].Value?.ToString() ?? "";
            txtCapacity.Text = r.Cells["Capacity"].Value?.ToString() ?? "40";
            if (r.Cells["CreditHours"].Value != DBNull.Value) cmbCredits.SelectedItem = Convert.ToInt32(r.Cells["CreditHours"].Value);
            if (r.Cells["Semester"].Value != DBNull.Value) cmbSemester.SelectedItem = Convert.ToInt32(r.Cells["Semester"].Value);
        }

        private void BtnAdd(object? s, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Code and Name required."); return; }
            var p = new SqlParameter[] { new("@CourseCode", txtCode.Text.Trim()), new("@CourseName", txtName.Text.Trim()), new("@CreditHours", cmbCredits.SelectedItem), new("@DeptID", cmbDept.SelectedValue), new("@InstructorID", cmbInstructor.SelectedValue ?? (object)DBNull.Value), new("@Semester", cmbSemester.SelectedItem), new("@Capacity", int.TryParse(txtCapacity.Text, out int c) ? c : 40) };
            if (DatabaseHelper.ExecuteNonQuery(SqlQueries.Courses.Insert, p) > 0) { MessageBox.Show("Course added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); ClearForm(); LoadData(); }
        }

        private void BtnUpd(object? s, EventArgs e)
        {
            if (_selectedId == 0) { MessageBox.Show("Select a course."); return; }
            var p = new SqlParameter[] { new("@CourseID", _selectedId), new("@CourseName", txtName.Text.Trim()), new("@CreditHours", cmbCredits.SelectedItem), new("@DeptID", cmbDept.SelectedValue), new("@InstructorID", cmbInstructor.SelectedValue ?? (object)DBNull.Value), new("@Semester", cmbSemester.SelectedItem), new("@Capacity", int.TryParse(txtCapacity.Text, out int c) ? c : 40) };
            if (DatabaseHelper.ExecuteNonQuery(SqlQueries.Courses.Update, p) > 0) { MessageBox.Show("Course updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); LoadData(); }
        }

        private void BtnDel(object? s, EventArgs e)
        {
            if (_selectedId == 0) return;
            if (MessageBox.Show("Deactivate?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            DatabaseHelper.ExecuteNonQuery(SqlQueries.Courses.Deactivate, new[] { new SqlParameter("@CourseID", _selectedId) });
            ClearForm(); LoadData();
        }

        private void ClearForm() { _selectedId = 0; txtCode.Clear(); txtName.Clear(); txtCapacity.Text = "40"; }
        private Label FL(string t, int x, int y) => new() { Text = t, Font = new Font("Segoe UI", 9), ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true, BackColor = Color.Transparent };
        private TextBox STB(int x, int y, int w) { var tb = new TextBox { Location = new Point(x, y), Size = new Size(w, 30) }; UIHelper.StyleTextBox(tb); return tb; }
    }
}
