#region Namespace Sonata.Data.Entity.Mapping
//	The Sonata.Data.Entity.Mapping namespace contains classes that are used to generate a LINQ to SQL object model that represents the structure and content of a database.
#endregion

using System;
using System.Data;

namespace Sonata.Data.Entity.Mapping
{
	/// <inheritdoc />
	/// <summary>
	/// Represents a stored procedure <see cref="T:System.Data.Common.DbParameter" /> that a property is mapped to.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class StoredProcedureParameterAttribute : Attribute
	{
		#region Constants

		private const string DefaultSchema = StoredProcedure.DefaultSchema;

		#endregion

		#region Members

		private string _schema = DefaultSchema;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets a value that indicates whether the <see cref="System.Data.Common.DbParameter"/> is input-only, output-only, bidirectional, or a stored procedure return value parameter.
		/// </summary>
		public ParameterDirection Direction { get; set; }

		/// <summary>
		/// Gets the name of the stored procedure <see cref="System.Data.Common.DbParameter"/>.
		/// </summary>
		public string ParameterName { get; }

		/// <summary>
		/// Gets or sets the maximum number of digits used to represent the <see cref="System.Data.Common.DbParameter.Value"/> property.
		/// </summary>
		public byte Precision { get; set; }

		/// <summary>
		/// Gets or sets the number of decimal places to which <see cref="System.Data.Common.DbParameter.Value"/> is resolved.
		/// </summary>
		public byte Scale { get; set; }

		/// <summary>
		/// Gets or sets a value indicating the schema of the <see cref="System.Data.Common.DbParameter"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">value is null.</exception>
		/// <exception cref="ArgumentException">value is empty or whitespace.</exception>
		/// <remarks>This value has to be used in case of User-Defined Table type in which the schema is mandatory to retrieve it.</remarks>
		public string Schema
		{
			get => _schema;
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				if (String.IsNullOrWhiteSpace(value))
					throw new ArgumentException(@"value can not be Empty or Whitespace", "value");

				_schema = value;
			}
		}

		/// <summary>
		/// Gets or sets the maximum size, in bytes, of the data within the column.
		/// </summary>
		/// <remarks>Set size to -1 to tell SQL to stream data.</remarks>
		public int Size { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="DbType"/> of the <see cref="System.Data.Common.DbParameter"/>.
		/// </summary>
		public SqlDbType Type { get; }

		/// <summary>
		/// Gets or sets the type name for a User-Defined Table type <see cref="System.Data.Common.DbParameter"/>.
		/// </summary>
		public string TypeName { get; set; }

		#endregion

		#region Constructors

		/// <inheritdoc />
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sonata.Data.Entity.Mapping.StoredProcedureParameterAttribute" /> class.
		/// </summary>
		/// <param name="name">The name of the stored procedure <see cref="!:DbParameter" />.</param>
		/// <param name="type">The <see cref="T:SqlDbType" /> of the <see cref="!:DbParameter" />.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="name" /> is NULL.</exception>
		/// <exception cref="T:System.ArgumentException"><paramref name="name" /> is empty or whitespace.</exception>
		public StoredProcedureParameterAttribute(string name, SqlDbType type)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (String.IsNullOrWhiteSpace(name))
				throw new ArgumentException(@"name can not be Empty or Whitespace", "name");

			ParameterName = name;
			Type = type;
		}

		#endregion
	}
}