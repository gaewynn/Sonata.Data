#region Namespace Sonata.Data.SqlServer.Entity
//	TODO
#endregion

using System.Collections.Generic;
using System.Data.SqlClient;

namespace Sonata.Data.SqlServer.Entity
{
	/// <summary>
	/// Represents the collection of all entities in the <see cref="SqlServerContext"/>, or that can be queried from the database, of a given type. <see cref="SqlServerSet{TEntity}"/> is a concrete implementation of <see cref="ISqlServerSet{TEntity}"/>.
	/// </summary>
	/// <typeparam name="TEntity">The type that defines the set.</typeparam>
	public interface ISqlServerSet<out TEntity>
	{
		#region Properties

		/// <summary>
		/// Gets the <see cref="SqlServerContext"/> toward which all the database call will be done.
		/// </summary>
		SqlServerContext Context { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Execute the specified <paramref name="command"/> and returns its result in an <see cref="IEnumerable{TEntity}"/>.
		/// </summary>
		/// <param name="command">The command to execute.</param>
		/// <returns>The result of the <paramref name="command"/> in an <see cref="IEnumerable{TEntity}"/>.</returns>
		IEnumerable<TEntity> ToList(SqlCommand command);

		#endregion
	}
}
