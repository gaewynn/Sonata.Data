#region Namespace Sonata.Data.SqlServer.Entity
//	TODO
# endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Sonata.Core.Extensions;
using Sonata.Data.SqlServer.Core;
using Sonata.Data.SqlServer.Core.Objects;
using Sonata.Data.SqlServer.Entity.Infrastructure;

namespace Sonata.Data.SqlServer.Entity
{
	/// <inheritdoc />
	/// <summary>
	/// A <see cref="T:Sonata.Data.SqlServer.Entity.SqlServerContext" /> instance represents a combination of the Unit Of Work and Repository patterns such that it can be used to query from a database and group together changes that will then be written back to the store as a unit.
	/// </summary>
	public class SqlServerContext : DbContext
	{
		#region Members

		private const string LastEntitiesQuery = "SELECT TOP {0} {1} FROM {2} ORDER BY {3} DESC;";
		private const string TakeEntitiesQuery = "SELECT TOP {0} {1} FROM {2};";
		private const string AllEntitiesQuery = "SELECT {0} FROM {1};";
		private const string FindEntityQuery = "SELECT {0} FROM {1} WHERE {2};";
		private const string InsertEntityQuery = "INSERT INTO {0} ({1}) VALUES ({2});";
		private const string UpdateEntityQuery = "UPDATE {0} SET {1} WHERE {2};";
		private const string DeleteEntityQuery = "DELETE FROM {0} WHERE {1};";
		private const string SelectIdentity = "SELECT @@IDENTITY;";
		private const string DiscriminatorColumnName = "Discriminator";
		private readonly SqlServerConnectionProxy _connection;
		private static bool _isDescriptorConfigured;

		private readonly Dictionary<EntityKey, EntityEntry> _modifiedEntityStore = new Dictionary<EntityKey, EntityEntry>();
		private readonly Dictionary<EntityKey, EntityEntry> _deletedEntityStore = new Dictionary<EntityKey, EntityEntry>();
		private readonly Dictionary<EntityKey, EntityEntry> _addedEntityStore = new Dictionary<EntityKey, EntityEntry>();

		#endregion

		#region Properties

		/// <summary>
		/// Gets the <see cref="SqlConnection"/> used to executes command toward the database.
		/// </summary>
		public SqlConnection Connection => _connection.Connection;

		#endregion

		#region Constructors

		/// <inheritdoc />
		/// <summary>
		/// </summary>
		/// <remarks>Ne pas utiliser ce constructeur. Il n'est là que pour le Code First.</remarks>
		public SqlServerContext()
		{
			var connectionStringValue = SqlServerDataProvider.ConnectionString;
			if (String.IsNullOrWhiteSpace(connectionStringValue))
				throw new ArgumentException($"The argument '{nameof(connectionStringValue)}' cannot be null, empty or contain only white space.");

			var dbConnectionFactory = new SqlServerConnectionFactory(connectionStringValue);
			_connection = dbConnectionFactory.Create();

			if (!_isDescriptorConfigured)
			{
				_isDescriptorConfigured = true;
				DatabaseDescriptors.Instance.Initialize(Model);
			}
		}
		
		#endregion

		#region Methods

		#region DbContext Members

		/// <inheritdoc />
		/// <summary>
		/// Saves all changes made in this context to the underlying database.
		/// </summary>
		/// <returns>The number of objects written to the underlying database.</returns>
		public override int SaveChanges()
		{
			var rowsAffected = 0;

			try
			{
				using (var transactionScope = new TransactionScope())
				{
					rowsAffected += _addedEntityStore.Sum(newEntity => InsertNewEntity(newEntity.Value));
					rowsAffected += _modifiedEntityStore.Sum(modifiedEntity => UpdateEntity(modifiedEntity.Value));
					rowsAffected += _deletedEntityStore.Sum(deletedEntity => DeleteEntity(deletedEntity.Value));

					transactionScope.Complete();
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Exception when executing a request: {ex.GetFullMessage()}");
			}
			finally
			{
				_addedEntityStore.Clear();
				_modifiedEntityStore.Clear();
				_deletedEntityStore.Clear();
			}

			return rowsAffected;
		}

		#endregion

		#region IDisposable Members

		public override void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_connection?.Connection == null)
				return;

			if (_connection.Connection.State == ConnectionState.Open)
				_connection.Connection.Close();

			_connection.Connection.Dispose();
		}

		#endregion

		/// <summary>
		/// Adds the given entity to the context underlying the set in the Added state such that it will be inserted into the database when SaveChanges is called.
		/// </summary>
		/// <param name="entity">The entity to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="entity"/> is NULL.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="entity.State"/> is different than <see cref="EntityState.Added"/>.</exception>
		/// <exception cref="InvalidDataException"><paramref name="entity"/> has an existing identifier.</exception>
		public void Add<TEntity>(EntityEntry entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));
			if (entity.State != EntityState.Added)
				throw new InvalidOperationException("Can not add an entity to the context which is not in an Added state.");

			var primaryKeys = GetPrimaryKeys<TEntity>();
			if (!primaryKeys.Any())
				throw new InvalidOperationException("Can not add an entity to the context which has no primary key defined.");

			if (primaryKeys.Count == 1)
			{
				var propertyInfo = typeof(TEntity).GetProperty(primaryKeys[0].Name);
				var primaryKeyValue = propertyInfo.GetValue(entity.Entity, null);

				if ((propertyInfo.PropertyType.IsValueType &&
					 primaryKeyValue.ToString() != Activator.CreateInstance(propertyInfo.PropertyType).ToString())
					|| (!propertyInfo.PropertyType.IsValueType && primaryKeyValue != null))
				{
					throw new InvalidDataException($"Can not add an entity with an existing identitfer: Property '{propertyInfo.Name}' with a value set to '{primaryKeyValue}'.");
				}
			}

			_addedEntityStore.Add(GetEntityKey<TEntity>(entity), entity);
		}

		internal List<TEntity> All<TEntity>()
		{
			var query = String.Format(AllEntitiesQuery,
				String.Join(", ", GetTableColumnNames<TEntity>()),
				$" {GetTableName<TEntity>()} ");

			List<TEntity> foundEntities;
			using (var command = _connection.Connection.CreateCommand())
			{
				command.CommandText = query;
				foundEntities = ToList<TEntity>(command);
			}

			return foundEntities ?? new List<TEntity>();
		}

		internal TEntity Find<TEntity>(params object[] keyValues)
		{
			if (keyValues == null)
				throw new ArgumentNullException(nameof(keyValues));

			var primaryKeys = GetPrimaryKeys<TEntity>();
			if (keyValues.Length == 0)
				throw new InvalidOperationException("Can not find entity: no primary key value provided.");

			if (keyValues.Length != primaryKeys.Count)
				throw new InvalidOperationException($"Can not find entity: the number of provided keys ({keyValues.Length}) does not match the number of keys on the table ({primaryKeys.Count}).");

			var whereClause = new List<string>();
			for (var i = 0; i < keyValues.Length; i++)
			{
				whereClause.Add(String.Format("{0}=@{0}", GetTableColumnName<TEntity>(primaryKeys[i])));
				whereClause.Add("AND");
			}
			whereClause.RemoveAt(whereClause.Count - 1);

			var query = String.Format(FindEntityQuery,
				String.Join(", ", GetTableColumnNames<TEntity>())      /*	Select fields list	*/,
				$" {GetTableName<TEntity>()} "                         /*	FROM clause	*/,
				String.Join(" ", whereClause)                          /*	WHERE clause*/);

			var foundEntity = default(TEntity);
			using (var command = _connection.Connection.CreateCommand())
			{
				command.CommandText = query;
				for (var i = 0; i < keyValues.Length; i++)
					command.Parameters.AddWithValue(GetTableColumnName<TEntity>(primaryKeys[i]), keyValues[i]);

				DumpQuery(command);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
						foundEntity = Map<TEntity>(reader);
				}
			}

			return foundEntity;
		}

		/// <summary>
		/// Marks the given entity as Deleted such that it will be deleted from the database when SaveChanges is called.
		/// </summary>
		/// <param name="entity">The entity to remove.</param>
		/// <exception cref="ArgumentNullException"><paramref name="entity"/> is NULL.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="entity.State"/> is different than <see cref="EntityState.Deleted"/>.</exception>
		internal void Remove<TEntity>(EntityEntry entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));
			if (entity.State != EntityState.Deleted)
				throw new InvalidOperationException("Can not remove an entity from the context which is not in an Deleted state.");

			_deletedEntityStore.Add(GetEntityKey<TEntity>(entity), entity);
		}

		internal List<TEntity> Last<TEntity>(int count)
		{
			var query = String.Format(LastEntitiesQuery,
				count,
				String.Join(", ", GetTableColumnNames<TEntity>()),
				$" {GetTableName<TEntity>()} ",
				GetTableColumnName<TEntity>(GetProperties<TEntity>().Single(e => e.Value.ColumnName.ToLower().EndsWith("_creationdate")).Key));

			List<TEntity> foundEntities;
			using (var command = _connection.Connection.CreateCommand())
			{
				command.CommandText = query;
				foundEntities = ToList<TEntity>(command);
			}

			return foundEntities ?? new List<TEntity>();
		}

		internal List<TEntity> Take<TEntity>(int count)
		{
			var query = String.Format(TakeEntitiesQuery,
				count,
				String.Join(", ", GetTableColumnNames<TEntity>()),
				$" {GetTableName<TEntity>()} ");

			List<TEntity> foundEntities;
			using (var command = _connection.Connection.CreateCommand())
			{
				command.CommandText = query;
				foundEntities = ToList<TEntity>(command);
			}

			return foundEntities ?? new List<TEntity>();
		}

        internal TEntity FirstOrDefault<TEntity>(Expression<Func<TEntity, bool>> query)
            where TEntity: class
        {
            return this.Set<TEntity>().FirstOrDefault(query);
        }

		internal DataTable ToDataTable(IDbCommand command)
		{
			DumpQuery(command);

			using (var sda = new SqlDataAdapter((SqlCommand)command))
			{
				var dataTable = new DataTable();

				sda.SelectCommand = (SqlCommand)command;
				sda.Fill(dataTable);

				return dataTable;
			}
		}

		private TEntity Map<TEntity>(SqlDataReader sqlDataReader)
		{
			var newEntity = (TEntity)Activator.CreateInstance(typeof(TEntity));
			var properties = GetProperties<TEntity>();
			var modificationDate = new KeyValuePair<string, object>();
			var modifiedBy = new KeyValuePair<string, object>();

			foreach (var property in properties.Where(property => Enumerable.Range(0, sqlDataReader.FieldCount).Any(e => sqlDataReader.GetName(e) == property.Value.ColumnName)))
			{
				var sqlServerDataTypeMapping = GetSqlServerDataTypeMapping(property.Key);
				var fieldValue = sqlServerDataTypeMapping.DotNetFrameworkSqlDbTypedAccessor(sqlDataReader, property.Value.ColumnName);
				newEntity.GetType().GetProperty(property.Key.Name)?.SetValue(newEntity, fieldValue, null);

				if (property.Key.Name == "ModificationDate")
					modificationDate = new KeyValuePair<string, object>(property.Key.Name, fieldValue);
				if (property.Key.Name == "ModifiedBy")
					modifiedBy = new KeyValuePair<string, object>(property.Key.Name, fieldValue);
			}

			if (modificationDate.Key != null)
				newEntity.GetType().GetProperty(modificationDate.Key)?.SetValue(newEntity, modificationDate.Value, null);
			if (modifiedBy.Key != null)
				newEntity.GetType().GetProperty(modifiedBy.Key)?.SetValue(newEntity, modifiedBy.Value, null);

			return newEntity;
		}

		private int MapForInt32(SqlDataReader sqlDataReader, out string error)
		{
			error = null;

			if (sqlDataReader.FieldCount > 1)
			{
				if (!String.IsNullOrWhiteSpace(sqlDataReader.GetValue(1)?.ToString()))
					error = sqlDataReader.GetValue(1).ToString();
			}

			return int.Parse(sqlDataReader.GetValue(0).ToString());
		}

		private bool MapForBoolean(SqlDataReader sqlDataReader, out string error)
		{
			error = null;

			if (sqlDataReader.FieldCount > 1)
			{
				if (!String.IsNullOrWhiteSpace(sqlDataReader.GetValue(1)?.ToString()))
					error = sqlDataReader.GetValue(1).ToString();
			}

			var value = sqlDataReader.GetValue(0).ToString();

			if (bool.TryParse(value, out var result))
				return result;

			return value == "1";
		}

		internal List<TEntity> ToList<TEntity>(IDbCommand command)
		{
			DumpQuery(command);

			if (command.Connection.State == ConnectionState.Closed)
				command.Connection.Open();

			using (var reader = command.ExecuteReader())
			{
				var entities = new List<TEntity>();
				while (reader.Read())
				{
					var entity = Map<TEntity>((SqlDataReader)reader);
					entities.Add(entity);
				}

				return entities;
			}
		}

		internal List<TEntity> RunTransaction<TEntity>(IDbCommand command, out string error)
		{
			DumpQuery(command);

			if (command.Connection.State == ConnectionState.Closed)
				command.Connection.Open();

			using (var reader = command.ExecuteReader())
			{
				error = null;
				if (reader.FieldCount == 2)
				{
					if (!String.IsNullOrWhiteSpace(reader.GetValue(1)?.ToString()))
						error = reader.GetValue(1).ToString();

					return null;
				}

				var entities = new List<TEntity>();
				while (reader.Read())
				{
					var entity = Map<TEntity>((SqlDataReader)reader);
					entities.Add(entity);
				}

				return entities;
			}
		}

		internal TReturnType RunTransactionFor<TReturnType>(IDbCommand command, out string error) 
			where TReturnType : struct
		{
			DumpQuery(command);

			if (command.Connection.State == ConnectionState.Closed)
				command.Connection.Open();

			using (var reader = command.ExecuteReader())
			{
				if (reader.Read())
				{
					if (typeof(TReturnType) == typeof(Int32))
						return (TReturnType)Convert.ChangeType(MapForInt32((SqlDataReader)reader, out error), typeof(Int32));
					if (typeof(TReturnType) == typeof(bool))
						return (TReturnType)Convert.ChangeType(MapForBoolean((SqlDataReader)reader, out error), typeof(bool));
					
					throw new NotSupportedException($"TReturnType can only be int or bool");
				}

				throw new InvalidOperationException("The given trasaction does not return any rows. Please insert a SELECT statment before COMMIT and ROLLBAK TRAN");
			}
		}

		internal void Update<TEntity>(EntityEntry entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));
			if (entity.State != EntityState.Modified)
				throw new InvalidOperationException("Can not add an entity to the context which is not in an Modified state.");

			_modifiedEntityStore.Add(GetEntityKey<TEntity>(entity), entity);
		}

		private int DeleteEntity(EntityEntry entity)
		{
			var tableName = GetTableName(entity.BaseType);
			var primaryKey = GetPrimaryKeys(entity.BaseType);

			var whereClause = new List<string>();
			foreach (var key in primaryKey)
			{
				whereClause.Add(String.Format("{0}=@{0}", GetTableColumnName(entity.BaseType, key)));
				whereClause.Add("AND");
			}
			whereClause.RemoveAt(whereClause.Count - 1);

			var query = String.Format(DeleteEntityQuery,
				tableName,
				String.Join(" ", whereClause));

			using (var command = _connection.Connection.CreateCommand())
			{
				command.CommandText = query;
				foreach (var sqlParameter in from property in primaryKey
											 let sqlServerDataTypeMapping = GetSqlServerDataTypeMapping(property)
											 let propertyValue = entity.BaseType.GetProperty(property.Name).GetValue(entity.Entity, null)
											 select new SqlParameter
											 {
												 ParameterName = GetTableColumnName(entity.BaseType, property),
												 Value = propertyValue ?? DBNull.Value,
												 SqlDbType = sqlServerDataTypeMapping.SqlDbType
											 })
				{
					command.Parameters.Add(sqlParameter);
				}

				DumpQuery(command);

				return command.ExecuteNonQuery();
			}
		}

		private EntityKey GetEntityKey<TEntity>(EntityEntry entityEntry)
		{
			var primaryKeys = GetPrimaryKeys<TEntity>();
			var entityKeyMembers = entityEntry.State == EntityState.Added ?
					primaryKeys.Select(primaryKeyProperty => new EntityKeyMember(primaryKeyProperty.Name, Guid.NewGuid())) :
					primaryKeys.Select(primaryKeyProperty => new EntityKeyMember(primaryKeyProperty.Name, entityEntry.BaseType.GetProperty(primaryKeyProperty.Name).GetValue(entityEntry.Entity, null)));
			var qualifiedEntitySetName = $"{_connection.Database}.{GetTableName<TEntity>()}";

			return new EntityKey(qualifiedEntitySetName, entityKeyMembers);
		}

		private IReadOnlyList<IProperty> GetPrimaryKeys<TEntity>()
		{
			return Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties;
		}

		private IReadOnlyList<IProperty> GetPrimaryKeys(Type entityType)
		{
			return Model.FindEntityType(entityType).FindPrimaryKey().Properties;
		}

		private Dictionary<IProperty, ISqlServerPropertyAnnotations> GetProperties<TEntity>()
		{
			return Model.FindEntityType(typeof(TEntity))
						.GetProperties()
						.ToDictionary(e => e, e => e.SqlServer());
		}

		private Dictionary<IProperty, ISqlServerPropertyAnnotations> GetProperties(Type entityType)
		{
			return Model.FindEntityType(entityType)
						.GetProperties()
						.ToDictionary(e => e, e => e.SqlServer());
		}

		private static SqlServerDataTypeMapping GetSqlServerDataTypeMapping(IProperty property)
		{
			return SqlServerDataTypeMapping.GetByDotNetType(property.ClrType);
		}

		private IEnumerable<string> GetTableColumnNames<TEntity>()
		{
			return GetProperties<TEntity>().Select(e => e.Value.ColumnName);
		}

		private string GetTableColumnName<TEntity>(IPropertyBase property)
		{
			return GetProperties<TEntity>().Single(e => e.Key.Name == property.Name).Value.ColumnName;
		}

		private string GetTableColumnName(Type entityType, IPropertyBase property)
		{
			return GetProperties(entityType).Single(e => e.Key.Name == property.Name).Value.ColumnName;
		}

		private string GetTableName<TEntity>()
		{
			return Model.FindEntityType(typeof(TEntity)).SqlServer().TableName;
		}

		private string GetTableName(Type entityType)
		{
			return Model.FindEntityType(entityType).SqlServer().TableName;
		}

		private int InsertNewEntity(EntityEntry entity)
		{
			var tableName = GetTableName(entity.BaseType);
			var primaryKey = GetPrimaryKeys(entity.BaseType);
			var properties = primaryKey.Count == 1 ?
				GetProperties(entity.BaseType).Where(e => !primaryKey.Contains(e.Key)).ToList() :
				GetProperties(entity.BaseType).ToList();
			properties = properties.Where(e => e.Key.ValueGenerated == ValueGenerated.Never || e.Key.ValueGenerated == ValueGenerated.OnAdd).ToList();

			var query = String.Format(InsertEntityQuery,
				tableName,
				String.Join(", ", properties.Select(e => e.Value.ColumnName)),
				String.Join(", ", properties.Select(e => e.Value.ColumnName).Select(e => $"@{e}")));

			using (var command = _connection.Connection.CreateCommand())
			{
				command.CommandText = query;
				foreach (var sqlParameter in from property in properties
											 let sqlServerDataTypeMapping = GetSqlServerDataTypeMapping(property.Key)
											 let propertyValue = entity.BaseType.GetProperty(property.Key.Name)?.GetValue(entity.Entity, null)
											 select new SqlParameter
											 {
												 ParameterName = property.Value.ColumnName,
												 Value = propertyValue ?? DBNull.Value,
												 SqlDbType = sqlServerDataTypeMapping.SqlDbType
											 })
				{
					if (sqlParameter.ParameterName == DiscriminatorColumnName && entity.BaseType.BaseType.IsAbstract)
						sqlParameter.Value = entity.BaseType.Name;

					command.Parameters.Add(sqlParameter);
				}

				DumpQuery(command);

				var affectedRows = command.ExecuteNonQuery();
				if (primaryKey.Count == 1)
				{
					command.CommandText = SelectIdentity;
					var entityId = command.ExecuteScalar();
					var entityIdVConverted = Convert.ChangeType(entityId, primaryKey[0].ClrType);

					entity.Entity.GetType().GetProperty(primaryKey[0].Name).SetValue(entity.Entity, entityIdVConverted, null);
				}

				return affectedRows;
			}
		}

		private int UpdateEntity(EntityEntry entity)
		{
			var tableName = GetTableName(entity.BaseType);
			var primaryKey = GetPrimaryKeys(entity.BaseType);
			var properties = primaryKey.Count == 1 ?
				GetProperties(entity.BaseType).Where(e => !primaryKey.Contains(e.Key)).ToList() :
				GetProperties(entity.BaseType).ToList();
			properties = properties.Where(e => e.Key.ValueGenerated == ValueGenerated.Never).ToList();

			var setClause = properties.Aggregate(String.Empty, (current, property) => current + $"{property.Value.ColumnName} = @{property.Value.ColumnName},").Trim(',');
			var whereClause = new List<string>();
			foreach (var key in primaryKey)
			{
				whereClause.Add(String.Format("{0}=@{0}", GetTableColumnName(entity.BaseType, key)));
				whereClause.Add("AND");
			}
			whereClause.RemoveAt(whereClause.Count - 1);

			var query = String.Format(UpdateEntityQuery,
				tableName,
				setClause,
				String.Join(" ", whereClause));

			using (var command = _connection.Connection.CreateCommand())
			{
				command.CommandText = query;
				foreach (var sqlParameter in from property in properties
											 let sqlServerDataTypeMapping = GetSqlServerDataTypeMapping(property.Key)
											 let propertyValue = entity.BaseType.GetProperty(property.Key.Name)?.GetValue(entity.Entity, null)
											 select new SqlParameter
											 {
												 ParameterName = property.Value.ColumnName,
												 Value = propertyValue ?? DBNull.Value,
												 SqlDbType = sqlServerDataTypeMapping.SqlDbType
											 })
				{
					if (sqlParameter.ParameterName == DiscriminatorColumnName && entity.BaseType.BaseType.IsAbstract)
						sqlParameter.Value = entity.BaseType.Name;

					command.Parameters.Add(sqlParameter);
				}

				if (primaryKey.Count == 1)
				{
					command.Parameters.Add(new SqlParameter
					{
						ParameterName = GetTableColumnName(entity.BaseType, primaryKey[0]),
						Value = entity.BaseType.GetProperty(primaryKey[0].Name).GetValue(entity.Entity, null) ?? DBNull.Value,
						SqlDbType = GetSqlServerDataTypeMapping(primaryKey[0]).SqlDbType
					});
				}

				DumpQuery(command);

				return command.ExecuteNonQuery();
			}
		}

		private static void DumpQuery(IDbCommand command)
		{
#if DEBUG
			try
			{
				var query = command.CommandText;
				foreach (var parameter in command.Parameters.Cast<SqlParameter>().OrderByDescending(e => e.ParameterName.Length))
				{
					string value;
					if (parameter.SqlDbType == SqlDbType.Char
						|| parameter.SqlDbType == SqlDbType.Date
						|| parameter.SqlDbType == SqlDbType.DateTime
						|| parameter.SqlDbType == SqlDbType.DateTime2
						|| parameter.SqlDbType == SqlDbType.NChar
						|| parameter.SqlDbType == SqlDbType.NText
						|| parameter.SqlDbType == SqlDbType.NVarChar
						|| parameter.SqlDbType == SqlDbType.Text
						|| parameter.SqlDbType == SqlDbType.Time
						|| parameter.SqlDbType == SqlDbType.VarChar
						|| parameter.SqlDbType == SqlDbType.Xml)
					{
						value = parameter.Value == DBNull.Value ? "NULL" : $"'{parameter.Value.ToString().Replace("'", "''")}'";
					}
					else if (parameter.SqlDbType == SqlDbType.Bit)
					{
						value = parameter.Value == DBNull.Value ? "NULL" : ((bool)parameter.Value ? "1" : "0");
					}
					else
					{
						value = parameter.Value == DBNull.Value ? "NULL" : parameter.Value.ToString();
					}

					query = query.Replace($"@{parameter.ParameterName}", value);
				}

				Debug.WriteLine("Executing query: " + query);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Unable to trace query: " + ex.GetFullMessage());
			}
#endif
		}

		#endregion
	}
}