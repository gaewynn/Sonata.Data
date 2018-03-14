#region Namespace Sonata.Data.Entity
//	The Sonata.Data.Entity namespace contains classes that provides access to the core functionalities related to EntityBase.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sonata.Data.Entity
{
	/// <inheritdoc />
	/// <summary>
	/// Represents a handler for all result sets returned from a Stored Procedure call. 
	/// </summary>
	public class ResultSets : IEnumerable
	{
		#region Members

		/// <summary>
		/// An internal list which handles the list of results lists.
		/// </summary>
		private readonly List<List<object>> _internalSet = new List<List<object>>();

		#endregion

		#region Properties

		/// <summary>
		/// Gets the nth results list item.
		/// </summary>
		/// <param name="index">The zero-based index of the results list item to get</param>
		/// <returns>The nth results list item.</returns>
		public List<object> this[int index] => _internalSet[index];

		/// <summary>
		/// Gets the count of result sets.
		/// </summary>
		public int Count => _internalSet.Count;

		#endregion

		#region Methods

		/// <summary>
		/// Adds a results list to the results set.
		/// </summary>
		/// <param name="list">A list to add to the current results set.</param>
		public void Add(List<object> list)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));

			_internalSet.Add(list);
		}

		/// <inheritdoc />
		/// <summary>
		/// Gets an <see cref="T:System.Collections.IEnumerator" /> over the internal list.
		/// </summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> over List{object} that make up the result sets.</returns>
		public IEnumerator GetEnumerator()
		{
			return _internalSet.GetEnumerator();
		}

		/// <summary>
		/// Returns the result set that contains a particular type and does a cast to that type.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> that was listed in <see cref="StoredProcedure"/> object as a possible return type for the stored procedure</typeparam>
		/// <returns>List of T; if no results match, returns an empty list.</returns>
		public IEnumerable<T> ToEnumerable<T>()
		{
			//	Search each non-empty results list by comparing types of the first element - this is why we filter for non-empty results
			foreach (var list in _internalSet.Where(e => e.Count > 0).Where(e => typeof(T) == e[0].GetType()))
				return list.Cast<T>();

			return new List<T>();
		}

		#endregion
	}
}