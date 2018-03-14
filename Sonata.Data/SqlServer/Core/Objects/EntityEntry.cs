#region Namespace Sonata.Data.SqlServer.Core.Objects
//	The Sonata.Data.SqlServer.Core.Objects namespace provides facilities for querying and working with entity data types in an SQL Server environment.
#endregion

using System;
using Microsoft.EntityFrameworkCore;

namespace Sonata.Data.SqlServer.Core.Objects
{
	/// <summary>
	/// Represents a wrapper around an Entity allowing to follow the entity state through an <see cref="Sonata.Data.SqlServer.Entity.SqlServerContext"/>.
	/// </summary>
	public class EntityEntry
	{
		#region Properties

		/// <summary>
		/// Gets or sets the wrapped entity.
		/// </summary>
		public object Entity { get; set; }

		/// <summary>
		/// Gets or sets the state of the current wrapped <see cref="Entity"/>.
		/// </summary>
		public EntityState State { get; set; }

		/// <summary>
		/// Gets or sets the base type of the current wrapped <see cref="Entity"/>.
		/// </summary>
		public Type BaseType { get; set; }

		#endregion
	}
}
