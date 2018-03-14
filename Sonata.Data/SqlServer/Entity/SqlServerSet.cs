#region Namespace Sonata.Data.SqlServer.Entity
//	TODO
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Sonata.Data.SqlServer.Core.Objects;

namespace Sonata.Data.SqlServer.Entity
{
	public class SqlServerSet<TEntity> : DbSet<TEntity>, ISqlServerSet<TEntity> where TEntity : class
	{
		#region Properties

		public SqlServerContext Context { get; }

		#endregion

		#region Constructors

		public SqlServerSet(SqlServerContext context)
		{
			Context = context;
		}

		#endregion

		#region Methods

		#region Create

		public new TEntity Add(TEntity entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			AddWrappedEntity(entity, EntityState.Added);
			return entity;
		}

		#endregion

		#region Read

		public IEnumerable<TEntity> All()
		{
			return Context.All<TEntity>();
		}

		public IEnumerable<TEntity> Last(int count)
		{
			return Context.Last<TEntity>(count);
		}

		public IEnumerable<TEntity> Take(int count)
		{
			return Context.Take<TEntity>(count);
		}

		public TEntity Find(params object[] keyValues)
		{
			if (keyValues == null)
				throw new ArgumentNullException(nameof(keyValues));

			return Context.Find<TEntity>(keyValues);
		}

		public IEnumerable<TEntity> ToList(SqlCommand command)
		{
			return Context.ToList<TEntity>(command);
		}

		public IEnumerable<TEntity> ToList(string query, IEnumerable<SqlParameter> parameters = null)
		{
			if (String.IsNullOrWhiteSpace(query))
				throw new ArgumentNullException(nameof(query));

			using (var dbCommand = Context.Connection.CreateCommand())
			{
				dbCommand.CommandText = query;

				if (parameters != null)
				{
					var sqlParameters = parameters as SqlParameter[] ?? parameters.ToArray();
					foreach (var sqlParameter in sqlParameters)
						dbCommand.Parameters.Add(sqlParameter);
				}

				return ToList(dbCommand);
			}
		}

		public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> query)
		{
			return Context.FirstOrDefault(query);
		}

		public IEnumerable<TEntity> RunTransaction(string query, out string error, IEnumerable<SqlParameter> parameters = null)
		{
			if (String.IsNullOrWhiteSpace(query))
				throw new ArgumentNullException(nameof(query));

			using (var dbCommand = Context.Connection.CreateCommand())
			{
				dbCommand.CommandText = query;

				if (parameters != null)
				{
					var sqlParameters = parameters as SqlParameter[] ?? parameters.ToArray();
					foreach (var sqlParameter in sqlParameters)
						dbCommand.Parameters.Add(sqlParameter);
				}

				return Context.RunTransaction<TEntity>(dbCommand, out error);
			}
		}

		public TReturnType RunTransactionFor<TReturnType>(string query, out string error, IEnumerable<SqlParameter> parameters = null)
			where TReturnType : struct
		{
			if (String.IsNullOrWhiteSpace(query))
				throw new ArgumentNullException(nameof(query));

			using (var dbCommand = Context.Connection.CreateCommand())
			{
				dbCommand.CommandText = query;

				if (parameters != null)
				{
					var sqlParameters = parameters as SqlParameter[] ?? parameters.ToArray();
					foreach (var sqlParameter in sqlParameters)
						dbCommand.Parameters.Add(sqlParameter);
				}

				return Context.RunTransactionFor<TReturnType>(dbCommand, out error);
			}
		}

		public DataTable ToDataTable(SqlCommand command)
		{
			return Context.ToDataTable(command);
		}

		public DataTable ToDataTable(string query, IEnumerable<SqlParameter> parameters = null)
		{
			if (String.IsNullOrWhiteSpace(query))
				throw new ArgumentNullException(nameof(query));

			using (var dbCommand = Context.Connection.CreateCommand())
			{
				dbCommand.CommandText = query;

				if (parameters != null)
				{
					var sqlParameters = parameters as SqlParameter[] ?? parameters.ToArray();
					foreach (var sqlParameter in sqlParameters)
						dbCommand.Parameters.Add(sqlParameter);
				}

				return ToDataTable(dbCommand);
			}
		}

		#endregion

		#region Update

		public new TEntity Update(TEntity entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			AddWrappedEntity(entity, EntityState.Modified);
			return entity;
		}

		#endregion

		#region Delete

		public new TEntity Remove(TEntity entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			AddWrappedEntity(entity, EntityState.Deleted);
			return entity;
		}

		#endregion

		public SqlParameter BuildParameter(string name, SqlDbType type, object value)
		{
			return new SqlParameter(name, value ?? DBNull.Value)
			{
				SqlDbType = type
			};
		}

		private void AddWrappedEntity(TEntity entity, EntityState state)
		{
			if (state == EntityState.Added)
			{
				Context.Add<TEntity>(new EntityEntry
				{
					BaseType = entity.GetType(),
					Entity = entity,
					State = state
				});
			}

			if (state == EntityState.Modified)
			{
				Context.Update<TEntity>(new EntityEntry
				{
					BaseType = entity.GetType(),
					Entity = entity,
					State = state
				});
			}

			if (state == EntityState.Deleted)
			{
				Context.Remove<TEntity>(new EntityEntry
				{
					BaseType = entity.GetType(),
					Entity = entity,
					State = state
				});
			}
		}

		#endregion
	}
}