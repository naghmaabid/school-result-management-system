using System;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace SchoolResultManagementSystem.Helpers
{
    /// <summary>
    /// Centralizes all MySQL connection and query logic so forms never
    /// build SQL strings or manage connections directly.
    /// </summary>
    public static class DatabaseHelper
    {
        private static readonly string ConnectionString =
            ConfigurationManager.ConnectionStrings["SchoolResultDb"].ConnectionString;

        private static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        /// <summary>Quick check used at startup to confirm the DB is reachable.</summary>
        public static bool TestConnection(out string errorMessage)
        {
            errorMessage = null;
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>For INSERT/UPDATE/DELETE. Returns number of affected rows.</summary>
        public static int ExecuteNonQuery(string sql, params MySqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new MySqlCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>For INSERT statements where you need the new row's auto-increment id.</summary>
        public static long ExecuteInsertAndGetId(string sql, params MySqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new MySqlCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                cmd.ExecuteNonQuery();
                return cmd.LastInsertedId;
            }
        }

        /// <summary>For SELECT statements returning a full result set.</summary>
        public static DataTable ExecuteQuery(string sql, params MySqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new MySqlCommand(sql, conn))
            using (var adapter = new MySqlDataAdapter(cmd))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                var table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        /// <summary>For SELECT statements that return a single value (COUNT, single column, etc).</summary>
        public static object ExecuteScalar(string sql, params MySqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new MySqlCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }
    }
}
