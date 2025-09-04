using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CarRental.Controllers.Shared
{
    public delegate T ConverterDelegate<T>(IDataReader reader);

    public static class Db
    {
        private static string databaseName;
        private static string connectionString;
        private static string providerName;
        private static DbProviderFactory providerFactory;
        private static bool initialized = false;
        private static readonly object lockObject = new object();
        
        // Static configuration for modern .NET support
        public static IConfiguration Configuration { get; set; }

        private static void Initialize()
        {
            if (initialized) return;
            
            lock (lockObject)
            {
                if (initialized) return;
                
                try
                {
                    // Try modern configuration first
                    if (Configuration != null)
                    {
                        databaseName = Configuration["AppSettings:databaseName"];
                        connectionString = Configuration.GetConnectionString(databaseName);
                        providerName = "System.Data.SqlClient"; // Default provider
                    }
                    else
                    {
                        // Fallback to legacy ConfigurationManager
                        databaseName = ConfigurationManager.AppSettings["databaseName"];
                        connectionString = ConfigurationManager.ConnectionStrings[databaseName].ConnectionString;
                        providerName = ConfigurationManager.ConnectionStrings[databaseName].ProviderName;
                    }

                    providerFactory = DbProviderFactories.GetFactory(providerName);
                    initialized = true;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to initialize database configuration. Make sure connection strings are properly configured.", ex);
                }
            }
        }

        public static int Insert(string sql, Dictionary<string, object> parameters)
        {
            Initialize();
            using (IDbConnection connection = providerFactory.CreateConnection())
            {
                connection.ConnectionString = connectionString;

                using (IDbCommand command = providerFactory.CreateCommand())
                {
                    command.CommandText = sql.AppendSelectIdentity();
                    command.Connection = connection;
                    command.SetParameters(parameters);

                    connection.Open();

                    int id = Convert.ToInt32(command.ExecuteScalar());

                    return id;
                }
            }
        }

        public static void Update(string sql, Dictionary<string, object> parameters = null)
        {
            Initialize();
            using (IDbConnection connection = providerFactory.CreateConnection())
            {
                connection.ConnectionString = connectionString;

                using (IDbCommand command = providerFactory.CreateCommand())
                {
                    command.CommandText = sql;

                    command.Connection = connection;

                    command.SetParameters(parameters);

                    connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }

        public static void Delete(string sql, Dictionary<string, object> parameters)
        {
            Update(sql, parameters);
        }

        public static List<T> GetAll<T>(string sql, ConverterDelegate<T> convert, Dictionary<string, object> parameters = null)
        {
            Initialize();
            using (IDbConnection connection = providerFactory.CreateConnection())
            {
                connection.ConnectionString = connectionString;

                using (IDbCommand command = providerFactory.CreateCommand())
                {
                    command.CommandText = sql;

                    command.Connection = connection;

                    command.SetParameters(parameters);

                    connection.Open();

                    var list = new List<T>();

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var obj = convert(reader);
                            list.Add(obj);
                        }

                        return list;
                    }
                }
            }
        }

        public static T Get<T>(string sql, ConverterDelegate<T> convert, Dictionary<string, object> parameters)
        {
            Initialize();
            using (IDbConnection connection = providerFactory.CreateConnection())
            {
                connection.ConnectionString = connectionString;

                using (IDbCommand command = providerFactory.CreateCommand())
                {
                    command.CommandText = sql;

                    command.Connection = connection;

                    command.SetParameters(parameters);

                    connection.Open();

                    T t = default;

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            t = convert(reader);

                        return t;
                    }
                }
            }
        }

        public static bool Exists(string sql, Dictionary<string, object> parameters)
        {
            Initialize();
            using (IDbConnection connection = providerFactory.CreateConnection())
            {
                connection.ConnectionString = connectionString;

                using (IDbCommand command = providerFactory.CreateCommand())
                {
                    command.CommandText = sql;

                    command.Connection = connection;

                    command.SetParameters(parameters);

                    connection.Open();

                    int numberRows = Convert.ToInt32(command.ExecuteScalar());

                    return numberRows > 0;
                }
            }
        }

        private static void SetParameters(this IDbCommand command, Dictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return;

            foreach (var parameter in parameters)
            {
                string name = parameter.Key;

                object value = parameter.Value.IsNullOrEmpty() ? DBNull.Value : parameter.Value;

                IDataParameter dbParameter = command.CreateParameter();

                dbParameter.ParameterName = name;
                dbParameter.Value = value;

                command.Parameters.Add(dbParameter);
            }
        }

        private static string AppendSelectIdentity(this string sql)
        {
            Initialize();
            switch (providerName)
            {
                case "System.Data.SqlClient": return sql + ";SELECT SCOPE_IDENTITY()";

                case "System.Data.SQLite": return sql + ";SELECT LAST_INSERT_ROWID()";

                default: return sql;
            }
        }

        public static bool IsNullOrEmpty(this object value)
        {
            return (value is string && string.IsNullOrEmpty((string)value)) ||
                    value == null;
        }

    }
}
