using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace AcademixPro
{
    public static class DatabaseHelper
    {
        private static readonly string ConnectionString =
            @"Server=(localdb)\MSSQLLocalDB;Database=AcademixProDB;Integrated Security=True;TrustServerCertificate=True;";
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
        public static bool TestConnection()
        {
            try
            {
                using (var con = GetConnection())
                {
                    con.Open();
                    return true;
                }
            }
            catch { return false; }
        }

        //Execute SELECT ing DataTable
        public static DataTable ExecuteQuery(string sql, SqlParameter[] parameters = null)
        {
            var dt = new DataTable();
            try
            {
                using (var con = GetConnection())
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.CommandTimeout = 30;
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    con.Open();
                    using (var adapter = new SqlDataAdapter(cmd))
                        adapter.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        public static int ExecuteNonQuery(string sql, SqlParameter[] parameters = null)
        {
            try
            {
                using (var con = GetConnection())
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.CommandTimeout = 30;
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    con.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        public static DataTable ExecuteStoredProcedure(string procName, SqlParameter[] parameters = null)
        {
            var dt = new DataTable();
            try
            {
                using (var con = GetConnection())
                using (var cmd = new SqlCommand(procName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 30;
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    con.Open();
                    using (var adapter = new SqlDataAdapter(cmd))
                        adapter.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Stored Procedure Error:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }
        public static int ExecuteSP(string procName, SqlParameter[] parameters = null)
        {
            try
            {
                using (var con = GetConnection())
                using (var cmd = new SqlCommand(procName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 30;
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    con.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:\n" + ex.Message, "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        public static object ExecuteScalar(string sql, SqlParameter[] parameters = null)
        {
            try
            {
                using (var con = GetConnection())
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.CommandTimeout = 30;
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    con.Open();
                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}