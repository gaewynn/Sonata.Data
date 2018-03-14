#region Namespace Sonata.Data.Entity.Mapping
//	The Sonata.Data.Entity.Mapping namespace contains classes that are used to generate a LINQ to SQL object model that represents the structure and content of a database.
#endregion

using System;

namespace Sonata.Data.Entity.Mapping
{
	/// <inheritdoc />
	/// <summary>
	/// Represents a User-Defined table type that a property is mapped to.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class UserDefinedTableTypeAttribute : Attribute
	{
		#region Constants

		private const string DefaultSchema = StoredProcedure.DefaultSchema;

		#endregion

		#region Members

		private string _schema = DefaultSchema;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the name of the User-Defined table type.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets or sets a value indicating the schema of the User-Defined table type.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is NULL.</exception>
		/// <exception cref="ArgumentException"><paramref name="value"/> is empty or whitespace.</exception>
		public string Schema
		{
			get => _schema;
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				if (String.IsNullOrWhiteSpace(value))
					throw new ArgumentException("value can not be Empty or Whitespace", "value");

				_schema = value;
			}
		}

		#endregion

		#region Constructors

		/// <inheritdoc />
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sonata.Data.Entity.Mapping.UserDefinedTableTypeAttribute" /> class.
		/// </summary>
		/// <param name="name">The name of the User-Defined table type.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="name" /> is NULL.</exception>
		/// <exception cref="T:System.ArgumentException"><paramref name="name" /> is empty or whitespace.</exception>
		public UserDefinedTableTypeAttribute(string name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (String.IsNullOrWhiteSpace(name))
				throw new ArgumentException("name can not be Empty or Whitespace", "name");

			Name = name;
		}

		#endregion
	}
}