using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBrowser
{
    /// <summary>
    /// Helper class for sql queries
    /// </summary>
    /// <remarks>
    /// This helper class is meant for a sql browser, thus lacking sql injection checks
    /// </remarks>
    public class SqlHelper
    {
        /// <summary>
        /// Gets a list of existing databases
        /// </summary>
        /// <returns>A list containing the database names</returns>
        public static IEnumerable<string> GetDatabases()
        {
            using (SqlConnection connection = new SqlConnection(Settings.ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT name FROM Sys.Databases ORDER BY name", connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    yield return reader.GetValue(0) as string;
            }
        }

        /// <summary>
        /// Gets a list of tables per database
        /// </summary>
        /// <param name="database">Database to get its tables from</param>
        /// <returns>A list containing the table names</returns>
        public static IEnumerable<string> GetTablesForDatabase(string database)
        {
            using (SqlConnection connection = new SqlConnection(Settings.ConnectionString))
            {
                connection.Open();
                string query = String.Format("SELECT name FROM {0}.Sys.Tables ORDER BY name", database);
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    yield return reader.GetValue(0) as string;
            }
        }

        /// <summary>
        /// Gets a list of views per database
        /// </summary>
        /// <param name="database">Database to get its views from</param>
        /// <returns>A list containing the view names</returns>
        public static IEnumerable<string> GetViewsForDatabase(string database)
        {
            using (SqlConnection connection = new SqlConnection(Settings.ConnectionString))
            {
                connection.Open();
                string query = String.Format("SELECT name FROM {0}.Sys.Views ORDER BY name", database);
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                    yield return reader.GetValue(0) as string;
            }
        }

        /// <summary>
        /// Async executes a query on the current database
        /// </summary>
        /// <param name="query">Query to execute</param>
        /// <returns>Returns a <see cref="QueryResult"/> containing the results of <paramref name="query"/> being executed</returns>
        public static async Task<QueryResult> ExecuteQuery(string query)
        {
            using (SqlConnection connection = new SqlConnection(Settings.ConnectionString))
            {
                await connection.OpenAsync();
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = await command.ExecuteReaderAsync();
                List<string> columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
                List<List<string>> rows = new List<List<string>>();

                while (await reader.ReadAsync())
                {
                    List<string> toAdd = new List<string>();
                    for (int i = 0; i < columns.Count; i++)
                        toAdd.Add(reader.GetValue(i).ToString());
                    rows.Add(toAdd);
                }

                return new QueryResult
                {
                    Columns = columns,
                    Rows = rows
                };
            }
        }

        /// <summary>
        /// Async gets the current structure of a table
        /// </summary>
        /// <param name="database">Database to get the structure on</param>
        /// <param name="table">Table to get the structure on</param>
        /// <returns>Returns a <see cref="QueryResult"/> containing the structure of a table</returns>
        public static async Task<QueryResult> GetTableStruct(string database, string table)
        {
            string query = String.Format(@"SELECT {0}.sys.columns.name AS Name, sys.types.name AS Type, sys.types.max_length AS Length
                            FROM (SELECT object_id, name
                            FROM {0}.sys.tables
                            UNION
                            SELECT object_id, name
                            FROM {0}.sys.views) AS u INNER JOIN
                            {0}.sys.columns ON u.object_id = {0}.sys.columns.object_id
                            INNER JOIN sys.types ON {0}.sys.columns.system_type_id = sys.types.system_type_id
                            WHERE u.name='{1}'", database, table);
            using (SqlConnection connection = new SqlConnection(Settings.ConnectionString))
            {
                await connection.OpenAsync();
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = await command.ExecuteReaderAsync();
                List<string> columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
                List<List<string>> rows = new List<List<string>>();

                while (await reader.ReadAsync())
                {
                    List<string> toAdd = new List<string>();
                    for (int i = 0; i < columns.Count; i++)
                        toAdd.Add(reader.GetValue(i).ToString());
                    rows.Add(toAdd);
                }

                return new QueryResult
                {
                    Columns = columns,
                    Rows = rows
                };
            }
        }
    }

    /// <summary>
    /// A struct containing the result of a query
    /// </summary>
    public struct QueryResult
    {
        public List<string> Columns { get; set; }
        public List<List<string>> Rows { get; set; }
    }
}
