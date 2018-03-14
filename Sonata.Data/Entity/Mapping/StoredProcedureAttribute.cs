#region Namespace Sonata.Data.Entity.Mapping
//	The Sonata.Data.Entity.Mapping namespace contains classes that are used to generate a LINQ to SQL object model that represents the structure and content of a database.
#endregion

using System;

namespace Sonata.Data.Entity.Mapping
{
	/// <inheritdoc />
	/// <summary>
	/// Represents the database stored procedure that a property is mapped to.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class StoredProcedureAttribute : Attribute
	{
		#region Properties

		/// <summary>
		/// Gets the name of the stored procedure the property is mapped to.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets or sets the <see cref="Type"/> returned by the stored procedure the property is mapped to.
		/// </summary>
		/// <remarks>Multiple <see cref="Type"/> can be provided, allowing the stored procedure to return multiple <see cref="ResultSets"/> of different types.</remarks>
		public Type[] ReturnTypes { get; set; }

		#endregion

		#region Constructors

		/// <inheritdoc />
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sonata.Data.Entity.Mapping.StoredProcedureAttribute" /> class.
		/// </summary>
		/// <param name="name">The name of the stored procedure the property is mapped to.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="name" /> is NULL.</exception>
		/// <exception cref="T:System.ArgumentException"><paramref name="name" /> is empty or whitespace.</exception>
		public StoredProcedureAttribute(string name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (String.IsNullOrWhiteSpace(name))
				throw new ArgumentException(@"name can not be Empty or Whitespace", "name");

			Name = name;
		}

		#endregion
	}
}