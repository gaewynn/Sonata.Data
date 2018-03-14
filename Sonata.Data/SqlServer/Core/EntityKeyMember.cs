#region Namespace Sonata.Data.SqlServer.Core
//	The Sonata.Data.SqlServer.Core namespace provides facilities for querying and working with entity data types in an SQL Server environment.
# endregion

using System;
using System.Globalization;

namespace Sonata.Data.SqlServer.Core
{
	public class EntityKeyMember
	{
		#region Members

		private string _keyName;
		private object _keyValue;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the name of the entity key.
		/// </summary>
		/// <returns>
		/// The key name.
		/// </returns>
		public string Key
		{
			get => _keyName;
			set
			{
				ValidateWritable(_keyName);
				_keyName = value;
			}
		}

		/// <summary>
		/// Gets or sets the value of the entity key.
		/// </summary>
		/// <returns>
		/// The key value.
		/// </returns>
		public object Value
		{
			get => _keyValue;
			set
			{
				ValidateWritable(_keyValue);
				_keyValue = value;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityKeyMember"/> class.
		/// </summary>
		public EntityKeyMember()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityKeyMember"/> class with the specified entity key pair.
		/// </summary>
		/// <param name="keyName">The name of the key.</param><param name="keyValue">The key value.</param>
		public EntityKeyMember(string keyName, object keyValue)
		{
			_keyName = keyName;
			_keyValue = keyValue;
		}

		#endregion

		#region Methods

		#region Object Members

		/// <summary>
		/// Returns a string representation of the entity key.
		/// </summary>
		/// <returns>
		/// A string representation of the entity key.
		/// </returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "[{0}, {1}]", _keyName, _keyValue);
		}

		#endregion

		private static void ValidateWritable(object instance)
		{
			if (instance != null)
				throw new InvalidOperationException("Can not change key");
		}

		#endregion
	}
}
