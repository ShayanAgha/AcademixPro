using AcademixPro.Forms;

namespace AcademixPro
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            if (!DatabaseHelper.TestConnection())
            {
                MessageBox.Show(
                    "Cannot connect to database.\n\n" +
                    "Please ensure:\n" +
                    "1. SQL Server is running\n" +
                    "2. Database 'AcademixProDB' exists\n" +
                    "3. Run DatabaseSetup.sql in SSMS first\n\n" +
                    "Connection: localhost\\SQLEXPRESS",
                    "Database Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            Application.Run(new LoginForm());
        }
    }
}