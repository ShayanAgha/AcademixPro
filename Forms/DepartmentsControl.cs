using System.Data;
using System.Data.SqlClient;
using AcademixPro.Helpers;

namespace AcademixPro.Forms
{
    public class DepartmentsControl : UserControl
    {
        private DataGridView dgvDepts = null!;
        private TextBox txtName = null!, txtCode = null!, txtDesc = null!;
        private int _selectedId = 0;

        public DepartmentsControl()
        {
            Dock = DockStyle.Fill; BackColor = AppColors.Background;
            DoubleBuffered = true; Load += (_, __) => BuildUI();
        }

        private void BuildUI()
        {
            Controls.Clear();

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterWidth = 8 };
            split.Panel1.BackColor = AppColors.Background; split.Panel2.BackColor = AppColors.Background;
            Controls.Add(split);
            split.SizeChanged += (_, __) => { int t = (int)(split.Width * 0.6); if (t > 0 && t < split.Width - 50) try { split.SplitterDistance = t; } catch { } };

            dgvDepts = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGrid(dgvDepts);
            dgvDepts.SelectionChanged += (_, __) =>
            {
                if (dgvDepts.CurrentRow == null) return;
                _selectedId = Convert.ToInt32(dgvDepts.CurrentRow.Cells["DeptID"].Value);
                txtName.Text = dgvDepts.CurrentRow.Cells["DeptName"].Value?.ToString() ?? "";
                txtCode.Text = dgvDepts.CurrentRow.Cells["DeptCode"].Value?.ToString() ?? "";
                txtDesc.Text = dgvDepts.CurrentRow.Cells["Description"].Value?.ToString() ?? "";
            };
            split.Panel1.Controls.Add(dgvDepts);

            var fp = new Panel { Dock = DockStyle.Fill, BackColor = AppColors.Surface, AutoScroll = true, Padding = new Padding(20) };
            split.Panel2.Controls.Add(fp);
            fp.Controls.Add(new Label { Text = "Department Details", Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold), ForeColor = AppColors.TextPrimary, AutoSize = true, Location = new Point(20, 16), BackColor = Color.Transparent });

            int y = 52, x = 20, w = 280;
            fp.Controls.Add(FL("Department Name *", x, y)); txtName = STB(x, y + 22, w); fp.Controls.Add(txtName); y += 60;
            fp.Controls.Add(FL("Code *", x, y)); txtCode = STB(x, y + 22, w); fp.Controls.Add(txtCode); y += 60;
            fp.Controls.Add(FL("Description", x, y)); txtDesc = STB(x, y + 22, w); fp.Controls.Add(txtDesc); y += 60;

            var btnAdd = UIHelper.CreateButton("➕ Add", AppColors.Primary, 130, 38); btnAdd.Location = new Point(20, y);
            btnAdd.Click += (_, __) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtCode.Text)) { MessageBox.Show("Name and Code required."); return; }
                var p = new SqlParameter[] { new("@DeptName", txtName.Text.Trim()), new("@DeptCode", txtCode.Text.Trim()), new("@Description", txtDesc.Text.Trim()) };
                if (DatabaseHelper.ExecuteNonQuery(SqlQueries.Departments.Insert, p) > 0) { MessageBox.Show("Department added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); Clear(); LoadData(); }
            };
            fp.Controls.Add(btnAdd);

            var btnUpd = UIHelper.CreateButton("✏️ Update", AppColors.CardEmerald, 130, 38); btnUpd.Location = new Point(160, y);
            btnUpd.Click += (_, __) =>
            {
                if (_selectedId == 0) { MessageBox.Show("Select a department."); return; }
                var p = new SqlParameter[] { new("@DeptID", _selectedId), new("@DeptName", txtName.Text.Trim()), new("@DeptCode", txtCode.Text.Trim()), new("@Description", txtDesc.Text.Trim()) };
                if (DatabaseHelper.ExecuteNonQuery(SqlQueries.Departments.Update, p) > 0) { MessageBox.Show("Updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); LoadData(); }
            };
            fp.Controls.Add(btnUpd);

            var btnDel = UIHelper.CreateButton("🗑️ Delete", AppColors.Danger, 130, 38); btnDel.Location = new Point(20, y + 48);
            btnDel.Click += (_, __) =>
            {
                if (_selectedId == 0) return;
                if (MessageBox.Show("Delete this department?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                DatabaseHelper.ExecuteNonQuery(SqlQueries.Departments.Delete, new[] { new SqlParameter("@DeptID", _selectedId) });
                Clear(); LoadData();
            };
            fp.Controls.Add(btnDel);

            var btnClr = UIHelper.CreateButton("🔄 Clear", AppColors.SurfaceLight, 130, 38); btnClr.ForeColor = AppColors.TextSecondary; btnClr.Location = new Point(160, y + 48);
            btnClr.Click += (_, __) => Clear();
            fp.Controls.Add(btnClr);

            LoadData();
        }

        private void LoadData()
        {
            dgvDepts.DataSource = DatabaseHelper.ExecuteQuery(SqlQueries.Departments.GetAll);
            if (dgvDepts.Columns.Contains("DeptID")) dgvDepts.Columns["DeptID"].Visible = false;
        }

        private void Clear() { _selectedId = 0; txtName.Clear(); txtCode.Clear(); txtDesc.Clear(); }
        private Label FL(string t, int x, int y) => new() { Text = t, Font = new Font("Segoe UI", 9), ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true, BackColor = Color.Transparent };
        private TextBox STB(int x, int y, int w) { var tb = new TextBox { Location = new Point(x, y), Size = new Size(w, 30) }; UIHelper.StyleTextBox(tb); return tb; }
    }
}
