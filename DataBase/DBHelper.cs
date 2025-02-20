using System.Collections;
using System.Data;
using System.Linq.Expressions;
using AssetManagement.Helpers.Extensions;
using AssetManagement.Helpers.Logger;
using Npgsql;

namespace AssetManagement.Helpers.DB
{
    public class DBHelper
    {
        private readonly NpgsqlConnection _con;

        public DBHelper(NpgsqlConnection connection) => _con = connection;


        /// <summary>
        /// Method <c>GetTableAll</c> gives all columns from table.
        /// </summary>
        public async Task<DataTable> GetTableCustom(string query, NpgsqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable();
            try
            {
                await _con.CloseAsync();
                await _con.OpenAsync();
                using (NpgsqlCommand cm = new NpgsqlCommand(query, _con))
                {
                    if (parameters != null) cm.Parameters.AddRange(parameters);
                    NpgsqlDataReader reader = await cm.ExecuteReaderAsync();
                    dt.Load(reader);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.AppendLog("ERROR", ex.Message);
                throw new Exception("Exception from GetTableCustom :::> \n" + ex.Message + " <::::::>");
            }
            return dt;
        }

        /// <summary>
        /// Method <c>GetTableAll</c> gives all columns from table.
        /// </summary>
        public async Task<DataTable> GetTableAll(string tablename, string[] array = null)
        {
            DataTable dt = new DataTable();
            try
            {
                string columns = (array == null || array.Length == 0) ? "*" : string.Join(", ", array);
                string query = $"SELECT {columns} FROM {tablename}";
                await _con.CloseAsync();
                await _con.OpenAsync();
                using (NpgsqlCommand cm = new NpgsqlCommand(query, _con))
                {
                    NpgsqlDataReader reader = await cm.ExecuteReaderAsync();
                    dt.Load(reader);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.AppendLog("ERROR", ex.Message);
                throw new Exception("Exception from GetTableAll :::> \n" + ex.Message + " <::::::>");
            }
            return dt;
        }

        /// <summary>
        /// Method <c>GetTableOne</c> gives all columns from a table according to the primary key.
        /// </summary>
        public async Task<DataTable> GetTableOne(string tablename, string pk, string pk_val, string[]? array = null)
        {
            DataTable dt = new DataTable();

            try
            {
                string columns = (array == null || array.Length == 0) ? "*" : string.Join(", ", array);

                string query = $"SELECT {columns} FROM {tablename} WHERE {pk} = @pk_val";

                await _con.CloseAsync();
                await _con.OpenAsync();

                using (NpgsqlCommand cm = new NpgsqlCommand(query, _con))
                {
                    cm.Parameters.AddWithValue($"pk_val", pk_val.ToInt());
                    using (NpgsqlDataReader reader = await cm.ExecuteReaderAsync())
                    {
                        dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception from GetTableOne :::> \n" + ex.Message + " <::::::>");
            }

            return dt;
        }

        /// <summary>
        /// Method <c>GetTable</c> gives all columns from table by userid.
        /// </summary>
        public async Task<DataTable> GetTableUser(string tablename, string uid, string uid_val, string[] array = null)
        {
            DataTable dt = new DataTable();
            string columns = (array == null || array.Length == 0) ? "*" : string.Join(", ", array);
            string query = $"SELECT {columns} FROM {tablename} WHERE {uid} = @uid_val";
            await _con.CloseAsync();
            await _con.OpenAsync();
            using (NpgsqlCommand cm = new NpgsqlCommand(query, _con))
            {
                cm.Parameters.AddWithValue("uid_val", uid_val.ToInt());
                NpgsqlDataReader reader = await cm.ExecuteReaderAsync();
                dt.Load(reader);
            }
            return dt;
        }

        public async Task<DataTable> GetTableWithCondition(string tableName, Dictionary<string, object> conditions = null, string[] columns = null)
        {
            DataTable dt = new DataTable();
            try
            {
                // Determine selected columns
                string columnNames = (columns == null || columns.Length == 0) ? "*" : string.Join(", ", columns);

                // Build the base query
                string query = $"SELECT {columnNames} FROM {tableName}";

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                // Add WHERE conditions dynamically
                if (conditions != null && conditions.Count > 0)
                {
                    var whereClauses = new List<string>();
                    foreach (var condition in conditions)
                    {
                        string paramName = $"@{condition.Key}";
                        whereClauses.Add($"{condition.Key} = {paramName}");
                        parameters.Add(new NpgsqlParameter(paramName, condition.Value));
                    }
                    query += " WHERE " + string.Join(" AND ", whereClauses);
                }

                Console.WriteLine($"Generated Query: {query}");

                await _con.CloseAsync();
                await _con.OpenAsync();

                using (NpgsqlCommand cm = new NpgsqlCommand(query, _con))
                {
                    if (parameters.Count > 0)
                        cm.Parameters.AddRange(parameters.ToArray());

                    using (NpgsqlDataReader reader = await cm.ExecuteReaderAsync())
                    {
                        dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.AppendLog("ERROR", ex.Message);
                throw new Exception($"Exception from GetTableWithCondition :::> {ex.Message} <::::::>");
            }
            return dt;
        }


        public async Task<int> InsertOne(string tablename, string[] colnames, ArrayList values)
        {
            try
            {
                string columns = string.Join(", ", colnames);
                string PlaceHolders = "";
                PlaceHolders = string.Join(", ", colnames.Select(e => "@" + e));
                string query = @$"
                INSERT INTO {tablename}
                ({columns}) VALUES (
                    {PlaceHolders}
                )
            ";
                Console.WriteLine("Query==> \n" + query);
                Console.WriteLine("PlaceHolders==> \n" + PlaceHolders);
                using (NpgsqlCommand cm = new NpgsqlCommand(query, _con))
                {
                    int i = 0;
                    string[] pl = PlaceHolders.Split(", ");
                    foreach (Object value in values)
                    {
                        cm.Parameters.AddWithValue(pl[i], value);
                        Console.WriteLine("Value==> \n" + value);
                        i++;
                    }
                    cm.ExecuteNonQuery();
                    return 1;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.AppendLog("Error in InsertOne", ex.Message);
                return 0;
            }
        }


        public async Task<int> UpdateOne(string tablename, string[] colnames, ArrayList values, string pk, string pk_val)
        {
            try
            {
                string columns = string.Join(", ", colnames);
                string PlaceHolders = "";
                PlaceHolders = string.Join(", ", colnames.Select(e => "@" + e));
                string q = "";
                int j = 0;
                foreach (string val in colnames)
                {
                    q += $"{colnames[j]} = @{colnames[j]}";
                }
                string query = @$"
                UPDATE {tablename} SET {q} WHERE {pk} = @pk_val";
                Console.WriteLine("Query==> \n" + query);
                Console.WriteLine("PlaceHolders==> \n" + PlaceHolders);
                using (NpgsqlCommand cm = new NpgsqlCommand(query, _con))
                {
                    cm.Parameters.AddWithValue("@pk_val", pk_val.ToInt());
                    int i = 0;
                    string[] pl = PlaceHolders.Split(", ");
                    foreach (Object value in values)
                    {
                        cm.Parameters.AddWithValue(pl[i], value);
                        Console.WriteLine("Value==> \n" + value);
                        i++;
                    }
                    cm.ExecuteNonQuery();
                    return 1;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.AppendLog("Error in UpdateOne", ex.Message);
                return 0;
            }
        }
    }
}