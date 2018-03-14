#region Namespace Sonata.Data.Entity.Mapping
//	The Sonata.Data.Entity.Mapping namespace contains classes that are used to generate a LINQ to SQL object model that represents the structure and content of a database.
#endregion

using System;

namespace Sonata.Data.Entity.Mapping
{
	/// <inheritdoc />
	/// <summary>
	/// Represents a base class to manage stored procedure stream <see cref="T:System.Data.Common.DbParameter" /> that a property is mapped to.
	/// </summary>
	/// <remarks>Use this type when the SQL column type is Binary, Image, Varbinary or UDT</remarks>
	[AttributeUsage(AttributeTargets.Property)]
	public class StoredProcedureStreamOutputParamterAttribute : Attribute
	{
		#region Properties

		/// <summary>
		/// Gets or sets a value indicating if the underlying stream has to be buffered.
		/// </summary>
		public bool Buffered { get; set; }

		/// <summary>
		/// Gets or sets a value indicating if the underlying stream must stay open to further accesses.
		/// </summary>
		public bool LeaveStreamOpen { get; set; } 
		
		#endregion
	}
}