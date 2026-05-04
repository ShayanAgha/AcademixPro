using System.Drawing.Drawing2D;

namespace AcademixPro.Helpers
{
    /// <summary>
    /// Reusable UI helper methods for styling controls across all forms.
    /// </summary>
    public static class UIHelper
    {
        // ── Rounded-rectangle path ────────────────────────────────────
        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            var arc = new Rectangle(bounds.Location, new Size(d, d));

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - d;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - d;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ── Style a TextBox ───────────────────────────────────────────
        public static void StyleTextBox(TextBox tb)
        {
            tb.BackColor = AppColors.SurfaceLight;
            tb.ForeColor = AppColors.TextPrimary;
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.Font = new Font("Segoe UI", 10);
        }

        // ── Style a ComboBox ──────────────────────────────────────────
        public static void StyleComboBox(ComboBox cb)
        {
            cb.BackColor = AppColors.SurfaceLight;
            cb.ForeColor = AppColors.TextPrimary;
            cb.FlatStyle = FlatStyle.Flat;
            cb.Font = new Font("Segoe UI", 10);
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        // ── Style a DataGridView ──────────────────────────────────────
        public static void StyleDataGrid(DataGridView dgv)
        {
            dgv.BackgroundColor = AppColors.Surface;
            dgv.GridColor = AppColors.Border;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.EnableHeadersVisualStyles = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.RowHeadersVisible = false;
            dgv.Font = new Font("Segoe UI", 9.5f);

            dgv.DefaultCellStyle.BackColor = AppColors.Surface;
            dgv.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
            dgv.DefaultCellStyle.SelectionBackColor = AppColors.Primary;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Padding = new Padding(8, 6, 8, 6);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = AppColors.SurfaceLight;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = AppColors.SurfaceLight;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TextSecondary;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 8, 8, 8);
            dgv.ColumnHeadersHeight = 44;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgv.RowTemplate.Height = 40;
        }

        // ── Create a styled Button ────────────────────────────────────
        public static Button CreateButton(string text, Color backColor, int width = 140, int height = 40)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(width, height),
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(backColor, 0.15f);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(backColor, 0.1f);
            return btn;
        }

        // ── Create a styled Label ─────────────────────────────────────
        public static Label CreateLabel(string text, float fontSize = 10f, Color? color = null)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", fontSize),
                ForeColor = color ?? AppColors.TextSecondary,
                AutoSize = true,
                BackColor = Color.Transparent,
            };
        }

        // ── Create a Section Header ───────────────────────────────────
        public static Label CreateSectionHeader(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI Semibold", 16, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                BackColor = Color.Transparent,
            };
        }

        // ── Create a DateTimePicker ───────────────────────────────────
        public static DateTimePicker CreateDatePicker()
        {
            return new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10),
                CalendarForeColor = AppColors.TextPrimary,
                CalendarMonthBackground = AppColors.SurfaceLight,
            };
        }
    }
}
