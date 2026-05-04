using System.Drawing.Drawing2D;
using AcademixPro.Helpers;

namespace AcademixPro.Forms
{
    public class DashboardControl : UserControl
    {
        public DashboardControl()
        {
            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;
            AutoScroll = true;
            DoubleBuffered = true;
            Padding = new Padding(8);
            Load += (_, __) => LoadDashboard();
        }

        private void LoadDashboard()
        {
            Controls.Clear();

            // ── Welcome Banner ────────────────────────────────────────
            var bannerPanel = new Panel
            {
                Size = new Size(Width - 40, 80),
                Location = new Point(8, 8),
                BackColor = AppColors.Surface,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            var lblWelcome = new Label
            {
                Text = $"Welcome back, {Session.FullName}!",
                Font = new Font("Segoe UI Semibold", 16, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(24, 14),
                BackColor = Color.Transparent,
            };
            bannerPanel.Controls.Add(lblWelcome);
            var lblDate = new Label
            {
                Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy"),
                Font = new Font("Segoe UI", 10),
                ForeColor = AppColors.TextMuted,
                AutoSize = true,
                Location = new Point(24, 46),
                BackColor = Color.Transparent,
            };
            bannerPanel.Controls.Add(lblDate);
            Controls.Add(bannerPanel);

            // ── Stat Cards ────────────────────────────────────────────
            var stats = DatabaseHelper.ExecuteQuery(SqlQueries.Dashboard.GetStats);
            int students = 0, instructors = 0, courses = 0, enrollments = 0;
            double avgCgpa = 0;

            if (stats.Rows.Count > 0)
            {
                var r = stats.Rows[0];
                students    = Convert.ToInt32(r["TotalStudents"]);
                instructors = Convert.ToInt32(r["TotalInstructors"]);
                courses     = Convert.ToInt32(r["TotalCourses"]);
                enrollments = Convert.ToInt32(r["TotalEnrollments"]);
                avgCgpa     = r["AvgCGPA"] != DBNull.Value ? Convert.ToDouble(r["AvgCGPA"]) : 0;
            }

            var cardData = new (string title, string value, string icon, Color color)[]
            {
                ("Total Students",    students.ToString(),               "👨‍🎓", AppColors.CardBlue),
                ("Total Instructors", instructors.ToString(),            "🧑‍🏫", AppColors.CardPurple),
                ("Active Courses",    courses.ToString(),                "📚", AppColors.CardEmerald),
                ("Enrollments",       enrollments.ToString(),            "🔗", AppColors.CardAmber),
                ("Average CGPA",      avgCgpa.ToString("F2"),            "⭐", AppColors.CardRose),
            };

            int cardY = 104;
            int cardX = 8;
            int cardCount = cardData.Length;
            int gap = 14;
            int totalGap = gap * (cardCount - 1);
            int cardW = Math.Max(180, (Width - 40 - totalGap) / cardCount);
            int cardH = 150;

            for (int i = 0; i < cardData.Length; i++)
            {
                var card = CreateStatCard(cardData[i].title, cardData[i].value, cardData[i].icon, cardData[i].color);
                card.Location = new Point(cardX + i * (cardW + gap), cardY);
                card.Size = new Size(cardW, cardH);
                Controls.Add(card);
            }

            // ── Top Students ──────────────────────────────────────────
            int tableY = cardY + cardH + 28;
            var topStudents = DatabaseHelper.ExecuteQuery(SqlQueries.Dashboard.GetTopStudents);

            var panelTop = new Panel
            {
                Location = new Point(8, tableY),
                Size = new Size((Width - 56) / 2, 280),
                BackColor = AppColors.Surface,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
            };
            Controls.Add(panelTop);

            var lblTopTitle = new Label
            {
                Text = "🏆  Top Performing Students",
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Location = new Point(16, 12),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            panelTop.Controls.Add(lblTopTitle);

            var dgvTop = new DataGridView
            {
                Location = new Point(8, 44),
                Size = new Size(panelTop.Width - 16, 224),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            UIHelper.StyleDataGrid(dgvTop);
            dgvTop.DataSource = topStudents;
            panelTop.Controls.Add(dgvTop);

            // ── Recent Enrollments ────────────────────────────────────
            var recentEnroll = DatabaseHelper.ExecuteQuery(SqlQueries.Dashboard.GetRecentEnrollments);

            var panelRecent = new Panel
            {
                Location = new Point(panelTop.Right + 16, tableY),
                Size = new Size((Width - 56) / 2, 280),
                BackColor = AppColors.Surface,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            Controls.Add(panelRecent);

            var lblRecentTitle = new Label
            {
                Text = "🕐  Recent Enrollments",
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Location = new Point(16, 12),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            panelRecent.Controls.Add(lblRecentTitle);

            var dgvRecent = new DataGridView
            {
                Location = new Point(8, 44),
                Size = new Size(panelRecent.Width - 16, 224),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            UIHelper.StyleDataGrid(dgvRecent);
            dgvRecent.DataSource = recentEnroll;
            panelRecent.Controls.Add(dgvRecent);

            // ── Department Stats ──────────────────────────────────────
            int deptY = tableY + 296;
            var deptStats = DatabaseHelper.ExecuteQuery(SqlQueries.Dashboard.GetDeptEnrollmentStats);

            var panelDept = new Panel
            {
                Location = new Point(8, deptY),
                Size = new Size(Width - 40, 220),
                BackColor = AppColors.Surface,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            Controls.Add(panelDept);

            var lblDeptTitle = new Label
            {
                Text = "🏛️  Students per Department",
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Location = new Point(16, 12),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            panelDept.Controls.Add(lblDeptTitle);

            // Simple bar chart
            int barY = 50;
            int barX = 24;
            Color[] barColors = { AppColors.CardBlue, AppColors.CardPurple, AppColors.CardEmerald, AppColors.CardAmber, AppColors.CardRose };
            int maxVal = 1;
            foreach (System.Data.DataRow row in deptStats.Rows)
            {
                int cnt = Convert.ToInt32(row["StudentCount"]);
                if (cnt > maxVal) maxVal = cnt;
            }

            for (int i = 0; i < deptStats.Rows.Count; i++)
            {
                var row = deptStats.Rows[i];
                string deptName = row["DeptName"].ToString()!;
                int cnt = Convert.ToInt32(row["StudentCount"]);
                Color col = barColors[i % barColors.Length];

                var lblName = new Label
                {
                    Text = deptName,
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = AppColors.TextSecondary,
                    AutoSize = false,
                    Size = new Size(200, 28),
                    Location = new Point(barX, barY + i * 36),
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent,
                };
                panelDept.Controls.Add(lblName);

                int barWidth = (int)((double)cnt / maxVal * 500);
                var bar = new Panel
                {
                    Size = new Size(Math.Max(barWidth, 4), 22),
                    Location = new Point(barX + 210, barY + i * 36 + 3),
                    BackColor = col,
                };
                panelDept.Controls.Add(bar);

                var lblCount = new Label
                {
                    Text = cnt.ToString(),
                    Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold),
                    ForeColor = AppColors.TextPrimary,
                    AutoSize = true,
                    Location = new Point(barX + 220 + barWidth, barY + i * 36 + 3),
                    BackColor = Color.Transparent,
                };
                panelDept.Controls.Add(lblCount);
            }
        }

        private Panel CreateStatCard(string title, string value, string icon, Color accentColor)
        {
            var panel = new Panel { BackColor = AppColors.Surface };

            panel.Paint += (s, e) =>
            {
                // left accent bar (wider for visibility)
                using var brush = new SolidBrush(accentColor);
                e.Graphics.FillRectangle(brush, 0, 0, 5, panel.Height);

                // subtle bottom accent glow line
                using var bottomBrush = new SolidBrush(Color.FromArgb(40, accentColor));
                e.Graphics.FillRectangle(bottomBrush, 0, panel.Height - 3, panel.Width, 3);
            };

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 22),
                Location = new Point(18, 14),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            panel.Controls.Add(lblIcon);

            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI Semibold", 28, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(18, 50),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            panel.Controls.Add(lblValue);

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI Semibold", 10.5f),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(18, 104),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            panel.Controls.Add(lblTitle);

            return panel;
        }
    }
}
