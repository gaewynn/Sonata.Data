#region Namespace Sonata.Data.SqlServer
//	TODO
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;

namespace Sonata.Data.SqlServer
{
	/// <summary>
	/// see https://msdn.microsoft.com/en-us/library/cc716729(v=vs.110).aspx
	/// </summary>
	public class SqlServerDataTypeMapping
	{
		#region Members

		private static List<SqlServerDataTypeMapping> _dataTypemappings;

		#endregion

		#region Properties

		public string SqlServerDatabaseEngineType { get; set; }

		public Type DotNetFrameworkType { get; set; }

		public Func<SqlDataReader, string, object> DotNetFrameworkSqlDbTypedAccessor { get; set; }

		public SqlDbType SqlDbType { get; set; }

		public DbType DbType { get; set; }

		private bool UseAsDefault { get; set; }

		#endregion

		#region Constructors

		static SqlServerDataTypeMapping()
		{
			InitializeDataTypeMappings();
		}

		#endregion

		#region Methods

		public static SqlServerDataTypeMapping GetBySqlServerDatabaseEngineType(string sqlServerDatabaseEngineType)
		{
			return _dataTypemappings.Single(e => e.SqlServerDatabaseEngineType == sqlServerDatabaseEngineType);
		}

		public static SqlServerDataTypeMapping GetByDotNetType(Type type)
		{
			return _dataTypemappings.Single(e => e.UseAsDefault && e.DotNetFrameworkType == type);
		}

		private static void InitializeDataTypeMappings()
		{
			_dataTypemappings = new List<SqlServerDataTypeMapping>
			{
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "bigint",
					DotNetFrameworkType = typeof(Int64),
					SqlDbType = SqlDbType.BigInt,
					DbType = DbType.Int64,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => Int64.Parse(reader.GetValue(reader.GetOrdinal(fieldName)).ToString())
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "bigint",
					DotNetFrameworkType = typeof(Nullable<Int64>),
					SqlDbType = SqlDbType.BigInt,
					DbType = DbType.Int64,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : (Nullable<Int64>)Int64.Parse(reader.GetValue(reader.GetOrdinal(fieldName)).ToString())
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "binary",
					DotNetFrameworkType = typeof(byte[]),
					SqlDbType = SqlDbType.VarBinary,
					DbType = DbType.Binary,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => (Byte[])reader[fieldName]
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "bit",
					DotNetFrameworkType = typeof(Boolean),
					SqlDbType = SqlDbType.Bit,
					DbType = DbType.Boolean,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => Boolean.Parse(reader.GetValue(reader.GetOrdinal(fieldName)).ToString())
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "bit",
					DotNetFrameworkType = typeof(Nullable<Boolean>),
					SqlDbType = SqlDbType.Bit,
					DbType = DbType.Boolean,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : (Nullable<Boolean>)Boolean.Parse(reader.GetValue(reader.GetOrdinal(fieldName)).ToString())
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "char",
					DotNetFrameworkType = typeof(String),
					SqlDbType = SqlDbType.Char,
					DbType = DbType.AnsiStringFixedLength,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader.GetValue(reader.GetOrdinal(fieldName)).ToString()
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "char",
					DotNetFrameworkType = typeof(char[]),
					SqlDbType = SqlDbType.Char,
					DbType = DbType.String,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader.GetValue(reader.GetOrdinal(fieldName)).ToString().ToCharArray()
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "date",
					DotNetFrameworkType = typeof(DateTime),
					SqlDbType = SqlDbType.Date ,
					DbType = DbType.Date ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => DateTime.Parse(reader.GetValue(reader.GetOrdinal(fieldName)).ToString())
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "date",
					DotNetFrameworkType = typeof(Nullable<DateTime>),
					SqlDbType = SqlDbType.Date ,
					DbType = DbType.Date ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : (Nullable<DateTime>)DateTime.Parse(reader.GetValue(reader.GetOrdinal(fieldName)).ToString())
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "datetime",
					DotNetFrameworkType = typeof(DateTime),
					SqlDbType = SqlDbType.DateTime  ,
					DbType = DbType.DateTime  ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => DateTime.Parse(reader.GetValue(reader.GetOrdinal(fieldName)).ToString())
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "datetime",
					DotNetFrameworkType = typeof(Nullable<DateTime>),
					SqlDbType = SqlDbType.DateTime  ,
					DbType = DbType.DateTime  ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : (Nullable<DateTime>)DateTime.Parse(reader.GetValue(reader.GetOrdinal(fieldName)).ToString())
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "datetime2",
					DotNetFrameworkType = typeof(DateTime),
					SqlDbType = SqlDbType.DateTime  ,
					DbType = DbType.DateTime  ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => DateTime.Parse(reader.GetValue(reader.GetOrdinal(fieldName)).ToString())
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "datetime2",
					DotNetFrameworkType = typeof(Nullable<DateTime>),
					SqlDbType = SqlDbType.DateTime2   ,
					DbType = DbType.DateTime2   ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : (Nullable<DateTime>)DateTime.Parse(reader.GetValue(reader.GetOrdinal(fieldName)).ToString())
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "datetimeoffset",
					DotNetFrameworkType = typeof(DateTime),
					SqlDbType = SqlDbType.DateTimeOffset  ,
					DbType = DbType.DateTimeOffset  ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => DateTime.Parse(reader.GetValue(reader.GetOrdinal(fieldName)).ToString())
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "datetimeoffset",
					DotNetFrameworkType = typeof(Nullable<DateTime>),
					SqlDbType = SqlDbType.DateTimeOffset   ,
					DbType = DbType.DateTimeOffset   ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : (Nullable<DateTime>)DateTime.Parse(reader.GetValue(reader.GetOrdinal(fieldName)).ToString())
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "decimal",
					DotNetFrameworkType = typeof(Decimal),
					SqlDbType = SqlDbType.Decimal   ,
					DbType = DbType.Decimal   ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => Decimal.Parse(reader.GetValue(reader.GetOrdinal(fieldName)).ToString())
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "decimal",
					DotNetFrameworkType = typeof(Nullable<Decimal>),
					SqlDbType = SqlDbType.Decimal    ,
					DbType = DbType.Decimal    ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : (Nullable<Decimal>)Decimal.Parse(reader.GetValue(reader.GetOrdinal(fieldName)).ToString())
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "varbinary",
					DotNetFrameworkType = typeof(Byte[]),
					SqlDbType = SqlDbType.VarBinary    ,
					DbType = DbType.Binary    ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.GetSqlBytes(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "float",
					DotNetFrameworkType = typeof(Double),
					SqlDbType = SqlDbType.Float    ,
					DbType = DbType.Double    ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => Double.Parse(reader.GetValue(reader.GetOrdinal(fieldName)).ToString())
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "float",
					DotNetFrameworkType = typeof(Nullable<Double>),
					SqlDbType = SqlDbType.Float    ,
					DbType = DbType.Double    ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : (Nullable<Double>)Double.Parse(reader.GetValue(reader.GetOrdinal(fieldName)).ToString())
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "image",
					DotNetFrameworkType = typeof(Byte[]),
					SqlDbType = SqlDbType.Binary     ,
					DbType = DbType.Binary    ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.GetSqlBinary(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "int",
					DotNetFrameworkType = typeof(Int32),
					SqlDbType = SqlDbType.Int    ,
					DbType = DbType.Int32    ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => Int32.Parse(reader.GetValue(reader.GetOrdinal(fieldName)).ToString())
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "int",
					DotNetFrameworkType = typeof(Nullable<Int32>),
					SqlDbType = SqlDbType.Int    ,
					DbType = DbType.Int32    ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : (Nullable<Int32>)Int32.Parse(reader.GetValue(reader.GetOrdinal(fieldName)).ToString())
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "money",
					DotNetFrameworkType = typeof(Decimal),
					SqlDbType = SqlDbType.Money     ,
					DbType = DbType.Decimal     ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.GetSqlMoney(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "money",
					DotNetFrameworkType = typeof(Nullable<Decimal>),
					SqlDbType = SqlDbType.Money     ,
					DbType = DbType.Decimal     ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : (Nullable<Decimal>)reader.GetSqlMoney(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "nchar",
					DotNetFrameworkType = typeof(String),
					SqlDbType = SqlDbType.NChar      ,
					DbType = DbType.StringFixedLength      ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader.GetSqlString(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "nchar",
					DotNetFrameworkType = typeof(Char[]),
					SqlDbType = SqlDbType.NChar      ,
					DbType = DbType.StringFixedLength      ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader.GetSqlString(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "ntext",
					DotNetFrameworkType = typeof(String),
					SqlDbType = SqlDbType.NText       ,
					DbType = DbType.String       ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader.GetSqlString(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "ntext",
					DotNetFrameworkType = typeof(Char[]),
					SqlDbType = SqlDbType.NText       ,
					DbType = DbType.String       ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader.GetSqlString(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "numeric",
					DotNetFrameworkType = typeof(Decimal),
					SqlDbType = SqlDbType.Money     ,
					DbType = DbType.Decimal     ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.GetSqlDecimal(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "numeric",
					DotNetFrameworkType = typeof(Nullable<Decimal>),
					SqlDbType = SqlDbType.Money     ,
					DbType = DbType.Decimal     ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : (Nullable<Decimal>)reader.GetSqlDecimal(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "nvarchar",
					DotNetFrameworkType = typeof(String),
					SqlDbType = SqlDbType.NVarChar        ,
					DbType = DbType.String       ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader.GetSqlString(reader.GetOrdinal(fieldName)).ToString()
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "nvarchar",
					DotNetFrameworkType = typeof(Char[]),
					SqlDbType = SqlDbType.NVarChar       ,
					DbType = DbType.String       ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader.GetSqlString(reader.GetOrdinal(fieldName)).ToString()
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "real",
					DotNetFrameworkType = typeof(Single),
					SqlDbType = SqlDbType.Real      ,
					DbType = DbType.Single      ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.GetSqlSingle(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "real",
					DotNetFrameworkType = typeof(Nullable<Single>),
					SqlDbType = SqlDbType.Real      ,
					DbType = DbType.Single      ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : (Nullable<Single>)reader.GetSqlSingle(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "rowversion",
					DotNetFrameworkType = typeof(Byte[]),
					SqlDbType = SqlDbType.Timestamp       ,
					DbType = DbType.Binary       ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader.GetSqlBinary(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "smalldatetime",
					DotNetFrameworkType = typeof(DateTime),
					SqlDbType = SqlDbType.DateTime       ,
					DbType = DbType.DateTime       ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.GetSqlDateTime (reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "smalldatetime",
					DotNetFrameworkType = typeof(Nullable<DateTime>),
					SqlDbType = SqlDbType.DateTime       ,
					DbType = DbType.DateTime       ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : (Nullable<DateTime>)reader.GetSqlDateTime(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "smallint",
					DotNetFrameworkType = typeof(Int16),
					SqlDbType = SqlDbType.SmallInt        ,
					DbType = DbType.Int16        ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.GetSqlInt16(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "smallint",
					DotNetFrameworkType = typeof(Nullable<Int16>),
					SqlDbType = SqlDbType.SmallInt        ,
					DbType = DbType.Int16        ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : (Nullable<Int16>)reader.GetSqlInt16(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "smallmoney",
					DotNetFrameworkType = typeof(Decimal),
					SqlDbType = SqlDbType.SmallMoney         ,
					DbType = DbType.Decimal         ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.GetSqlMoney(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "smallmoney",
					DotNetFrameworkType = typeof(Nullable<Decimal>),
					SqlDbType = SqlDbType.SmallMoney         ,
					DbType = DbType.Decimal         ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : (Nullable<Decimal>)reader.GetSqlMoney(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "sql_variant",
					DotNetFrameworkType = typeof(Object),
					SqlDbType = SqlDbType.Variant          ,
					DbType = DbType.Object          ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader.GetSqlValue(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "text",
					DotNetFrameworkType = typeof(String),
					SqlDbType = SqlDbType.Text         ,
					DbType = DbType.String       ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader.GetSqlString(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "text",
					DotNetFrameworkType = typeof(Char[]),
					SqlDbType = SqlDbType.Text        ,
					DbType = DbType.String       ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader.GetSqlString(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "time",
					DotNetFrameworkType = typeof(TimeSpan),
					SqlDbType = SqlDbType.Time          ,
					DbType = DbType.Time          ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => { throw new NotSupportedException("time has no getter in SqlServer"); }
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "timestamp",
					DotNetFrameworkType = typeof(Byte[]),
					SqlDbType = SqlDbType.Timestamp           ,
					DbType = DbType.Binary           ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader.GetSqlBinary(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "tinyint",
					DotNetFrameworkType = typeof(Byte),
					SqlDbType = SqlDbType.TinyInt         ,
					DbType = DbType.Byte         ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.GetSqlByte(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "tinyint",
					DotNetFrameworkType = typeof(Nullable<Byte>),
					SqlDbType = SqlDbType.TinyInt         ,
					DbType = DbType.Byte         ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : (Nullable<Byte>)reader.GetSqlByte(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "uniqueidentifier",
					DotNetFrameworkType = typeof(Guid),
					SqlDbType = SqlDbType.UniqueIdentifier          ,
					DbType = DbType.Guid          ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.GetSqlGuid(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "uniqueidentifier",
					DotNetFrameworkType = typeof(Nullable<Guid>),
					SqlDbType = SqlDbType.UniqueIdentifier          ,
					DbType = DbType.Guid          ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : (Nullable<Guid>)reader.GetSqlGuid(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "varbinary",
					DotNetFrameworkType = typeof(Byte[]),
					SqlDbType = SqlDbType.VarBinary            ,
					DbType = DbType.Binary           ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader.GetSqlBinary(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "varchar",
					DotNetFrameworkType = typeof(String),
					SqlDbType = SqlDbType.VarChar          ,
					DbType = DbType.AnsiString       ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader.GetSqlString(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "varchar",
					DotNetFrameworkType = typeof(Char[]),
					SqlDbType = SqlDbType.VarChar         ,
					DbType = DbType.AnsiString       ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader.GetSqlString(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "varchar",
					DotNetFrameworkType = typeof(String),
					SqlDbType = SqlDbType.VarChar          ,
					DbType = DbType.String        ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader.GetSqlString(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = false,
					SqlServerDatabaseEngineType = "varchar",
					DotNetFrameworkType = typeof(Char[]),
					SqlDbType = SqlDbType.VarChar         ,
					DbType = DbType.String        ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.IsDBNull(reader.GetOrdinal(fieldName)) ? null : reader.GetSqlString(reader.GetOrdinal(fieldName))
				},
				new SqlServerDataTypeMapping
				{
					UseAsDefault = true,
					SqlServerDatabaseEngineType = "xml",
					DotNetFrameworkType = typeof(SqlXml),
					SqlDbType = SqlDbType.Xml           ,
					DbType = DbType.Xml           ,
					DotNetFrameworkSqlDbTypedAccessor = (reader, fieldName) => reader.GetSqlXml(reader.GetOrdinal(fieldName))
				}
			};
		}

		#endregion
	}
}