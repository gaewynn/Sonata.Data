#region Namespace Sonata.Data.SqlServer.Entity.Infrastructure
//	TODO
#endregion

using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace Sonata.Data.SqlServer.Entity.Infrastructure
{
	/// <summary>
	/// Instances of this class are used to create OleDbConnection objects for Microsoft Access based on a given database name or connection string.
	/// </summary>
	internal class SqlServerConnectionFactory
	{
		#region Members

		private readonly string _connectionString;
		private readonly string _databaseName;

		#endregion

		#region Constructors

		public SqlServerConnectionFactory(string connectionStringValue)
		{
			if (connectionStringValue == null)
				throw new ArgumentNullException(nameof(connectionStringValue));
			if (String.IsNullOrWhiteSpace(connectionStringValue))
				throw new ArgumentException($"The argument '{nameof(connectionStringValue)}' cannot empty or contain only white space.");

			var connectionString = new ConnectionStringSettings("DbConnectionFactory.InternalConnectionString", connectionStringValue);

			_connectionString = connectionString.ConnectionString;
			_databaseName = GetDatabaseName();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Creates a connection for Microsoft SQL Server based on the given database name or connection string.
		/// </summary>
		/// <returns>An <see cref="SqlServerConnectionProxy"/> wrapping the <see cref="Microsoft.EntityFrameworkCore.Storage.Internal.SqlServerConnection"/> created and the name of the database.</returns>
		public SqlServerConnectionProxy Create()
		{
			try
			{
				SqlConnection connection;
				try
				{
					connection = new SqlConnection(_connectionString);
				}
				catch (Exception ex)
				{
					throw new NotSupportedException("Other connections than SqlServer are currently not supported", ex);
				}

				connection.ConnectionString = _connectionString;
				connection.Open();

				return new SqlServerConnectionProxy { Connection = connection, Database = _databaseName };
			}
			catch (Exception ex)
			{
				throw new Exception("Error building database connection", ex);
			}
		}

		/// <summary>
		/// Retrieve the name of the database in the current <see cref="_connectionString"/>.
		/// </summary>
		/// <returns>The name of the database.</returns>
		private string GetDatabaseName()
		{
			return _connectionString.Split(';').Single(e => e.ToLower().StartsWith("initial catalog=")).Substring("initial catalog=".Length);
		}

		#endregion
	}
}