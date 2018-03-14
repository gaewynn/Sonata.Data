#region Namespace Sonata.Data.SqlServer
//	TODO
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Sonata.Data.SqlServer.Entity;

namespace Sonata.Data.SqlServer
{
	public class DatabaseDescriptors
	{
		private static DatabaseDescriptors _instance;
		private Dictionary<Type, DatabaseDescriptor> _cache; 

		public static DatabaseDescriptors Instance => _instance ?? (_instance = new DatabaseDescriptors());

		private DatabaseDescriptors()
		{
			_cache = new Dictionary<Type, DatabaseDescriptor>();
		}

		public void Initialize(IModel dbContext)
		{
			var dbContexts = Assembly.GetAssembly(typeof(SqlServerContext)).ExportedTypes.Where(e => e.BaseType == typeof(SqlServerContext));
			var sets = dbContexts.ElementAt(0).GetProperties()
				.Where(e => e.PropertyType == typeof(SqlServerSet<>)).ToList();



			//currentAssembly.Where(e => e == typeof(SqlServerSet<>))
			//var types = currentAssembly.GetTypes();

			//foreach (var type in types)
			//{
			//	var sqlserverSets = type.GetProperties().Where(e => e.PropertyType == typeof (SqlServerSet<>) || e.PropertyType == typeof (SqlServerSimpleSet<>)).ToList();
			//	foreach (var sqlserverSet in sqlserverSets)
			//	{
			//		var sqlServerType = sqlserverSet.PropertyType.GetGenericArguments()[0];

			//		if (!_cache.ContainsKey(sqlServerType))
			//			_cache.Add(sqlServerType, new DatabaseDescriptor());

			//		_cache[sqlServerType].PrimaryKey = dbContext.FindEntityType(sqlServerType).FindPrimaryKey().Properties;
			//		_cache[sqlServerType].Properties = dbContext.FindEntityType(sqlServerType).GetProperties().ToDictionary(e => e, e => e.SqlServer());
			//		_cache[sqlServerType].TableColumnNames = _cache[sqlServerType].Properties.Select(e => e.Value.ColumnName);
			//		_cache[sqlServerType].TableName = dbContext.FindEntityType(sqlServerType).SqlServer().TableName;
			//	}
			//}
		}

		private class DatabaseDescriptor
		{
			public bool IsCompositeKey => PrimaryKey != null && PrimaryKey.Count > 1;
			
			public IReadOnlyList<IProperty> PrimaryKey { get; set; }

			public Dictionary<IProperty, ISqlServerPropertyAnnotations> Properties { get; set; }

			public IEnumerable<string> TableColumnNames { get; set; }

			public string TableName { get; set; }
		}
	}
}
