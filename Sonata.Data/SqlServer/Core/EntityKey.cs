#region Namespace Sonata.Data.SqlServer.Core
//	The Sonata.Data.SqlServer.Core namespace provides facilities for querying and working with entity data types in an SQL Server environment.
# endregion

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sonata.Data.SqlServer.Entity.Core;

namespace Sonata.Data.SqlServer.Core
{
	public class EntityKey : IEquatable<EntityKey>
	{
		#region Members

		private static readonly ConcurrentDictionary<string, string> NameLookup = new ConcurrentDictionary<string, string>();
		private string _entitySetName;
		private string _entityContainerName;
		private object _singletonKeyValue;
		private object[] _compositeKeyValues;
		private string[] _keyNames;
		private readonly bool _isLocked;
		private bool _containsByteArray;
		private EntityKeyMember[] _deserializedMembers;
		private int _hashCode;

		#endregion

		#region Properties

		/// <summary>
		/// Gets a singleton EntityKey by which a read-only entity is identified.
		/// </summary>
		public static EntityKey NoEntitySetKey { get; } = new EntityKey("NoEntitySetKey.NoEntitySetKey");

		/// <summary>
		/// Gets a singleton EntityKey identifying an entity resulted from a failed TREAT.
		/// </summary>
		public static EntityKey EntityNotValidKey { get; } = new EntityKey("EntityNotValidKey.EntityNotValidKey");

		/// <summary>
		/// Gets or sets the name of the entity set.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> value that is the name of the entity set for the entity to which the <see cref="T:System.Data.Entity.Core.EntityKey"/> belongs.
		/// </returns>
		public string EntitySetName
		{
			get => _entitySetName;
			set
			{
				ValidateWritable(_entitySetName);
				_entitySetName = LookupSingletonName(value);
			}
		}

		/// <summary>
		/// Gets or sets the name of the entity container.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> value that is the name of the entity container for the entity to which the <see cref="T:System.Data.Entity.Core.EntityKey"/> belongs.
		/// </returns>
		public string EntityContainerName
		{
			get => _entityContainerName;
			set
			{
				ValidateWritable(_entityContainerName);
				_entityContainerName = LookupSingletonName(value);
			}
		}

		/// <summary>
		/// Gets or sets the key values associated with this <see cref="T:System.Data.Entity.Core.EntityKey"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/> of key values for this <see cref="T:System.Data.Entity.Core.EntityKey"/>.
		/// </returns>
		public EntityKeyMember[] EntityKeyValues
		{
			get
			{
				if (IsTemporary)
					return null;

				EntityKeyMember[] entityKeyMemberArray;
				if (_singletonKeyValue != null)
				{
					entityKeyMemberArray = new[]
					{
						new EntityKeyMember(_keyNames[0], _singletonKeyValue)
					};
				}
				else
				{
					entityKeyMemberArray = new EntityKeyMember[_compositeKeyValues.Length];
					for (var index = 0; index < _compositeKeyValues.Length; ++index)
						entityKeyMemberArray[index] = new EntityKeyMember(_keyNames[index], _compositeKeyValues[index]);
				}
				return entityKeyMemberArray;
			}
			set
			{
				ValidateWritable(_keyNames);
				if (value == null || InitializeKeyValues(new KeyValueReader(value), true, true))
					return;

				_deserializedMembers = value;
			}
		}

		/// <summary>
		/// Gets a value that indicates whether the <see cref="T:System.Data.Entity.Core.EntityKey"/> is temporary.
		/// </summary>
		/// <returns>
		/// true if the <see cref="T:System.Data.Entity.Core.EntityKey"/> is temporary; otherwise, false.
		/// </returns>
		public bool IsTemporary
		{
			get
			{
				if (SingletonKeyValue == null)
					return CompositeKeyValues == null;

				return false;
			}
		}

		private object SingletonKeyValue
		{
			get
			{
				if (RequiresDeserialization)
					DeserializeMembers();

				return _singletonKeyValue;
			}
		}

		private object[] CompositeKeyValues
		{
			get
			{
				if (RequiresDeserialization)
					DeserializeMembers();

				return _compositeKeyValues;
			}
		}

		private bool RequiresDeserialization => _deserializedMembers != null;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityKey"/> class.
		/// </summary>
		public EntityKey()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityKey"/> class with an entity set name and a generic <see cref="T:System.Collections.Generic.KeyValuePair"/> collection.
		/// </summary>
		/// <param name="qualifiedEntitySetName">A <see cref="T:System.String"/> that is the entity set name qualified by the entity container name.</param>
		/// <param name="entityKeyValues">
		/// A generic <see cref="T:System.Collections.Generic.KeyValuePair"/> collection.Each key/value pair has a property name as the 
		/// key and the value of that property as the value. There should be one pair for each property that is part of the <see cref="T:System.Data.Entity.Core.EntityKey"/>.
		/// The order of the key/value pairs is not important, but each key property should be included. The property names are simple names that are not qualified with 
		/// an entity type name or the schema name.
		/// </param>
		public EntityKey(string qualifiedEntitySetName, IEnumerable<KeyValuePair<string, object>> entityKeyValues)
		{
			InitializeEntitySetName(qualifiedEntitySetName);
			InitializeKeyValues(entityKeyValues);
			_isLocked = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityKey"/> class with an entity set name and an 
		/// <see cref="T:System.Collections.Generic.IEnumerable`1"/> collection of <see cref="T:System.Data.Entity.Core.EntityKeyMember"/> objects.
		/// </summary>
		/// <param name="qualifiedEntitySetName">A <see cref="T:System.String"/> that is the entity set name qualified by the entity container name.</param>
		/// <param name="entityKeyValues">An <see cref="T:System.Collections.Generic.IEnumerable`1"/> collection of <see cref="T:System.Data.Entity.Core.EntityKeyMember"/> objects with which to initialize the key. </param>
		public EntityKey(string qualifiedEntitySetName, IEnumerable<EntityKeyMember> entityKeyValues)
		{
			InitializeEntitySetName(qualifiedEntitySetName);
			InitializeKeyValues(new KeyValueReader(entityKeyValues));
			_isLocked = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityKey"/> class with an entity set name and specific entity key pair.
		/// </summary>
		/// <param name="qualifiedEntitySetName">A <see cref="T:System.String"/> that is the entity set name qualified by the entity container name.</param>
		/// <param name="keyName">A <see cref="T:System.String"/> that is the name of the key.</param>
		/// <param name="keyValue">An <see cref="T:System.Object"/> that is the key value.</param>
		public EntityKey(string qualifiedEntitySetName, string keyName, object keyValue)
		{
			InitializeEntitySetName(qualifiedEntitySetName);
			_keyNames = new[]
			{
				keyName
			};
			_singletonKeyValue = keyValue;
			_isLocked = true;
		}

		internal EntityKey(string qualifiedEntitySetName)
		{
			InitializeEntitySetName(qualifiedEntitySetName);
			_isLocked = true;
		}

		#endregion

		#region Methods

		#region IEquatable<T> Members

		/// <summary>
		/// Returns a value that indicates whether this instance is equal to a specified <see cref="T:System.Data.Entity.Core.EntityKey"/>.
		/// </summary>
		/// <param name="other">An <see cref="T:System.Data.Entity.Core.EntityKey"/> object to compare with this instance.</param>
		/// <returns>true if this instance and  other  have equal values; otherwise, false.</returns>
		public bool Equals(EntityKey other)
		{
			return InternalEquals(this, other, true);
		}

		#endregion

		#region Object Members

		/// <summary>
		/// Returns a value that indicates whether this instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">An <see cref="T:System.Object"/> to compare with this instance.</param>
		/// <returns>true if this instance and  obj  have equal values; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return InternalEquals(this, obj as EntityKey, true);
		}

		/// <summary>
		/// Serves as a hash function for the current <see cref="T:System.Data.Entity.Core.EntityKey"/> object.
		/// <see cref="M:System.Data.Entity.Core.EntityKey.GetHashCode"/> is suitable for hashing algorithms and data structures such as a hash table.
		/// </summary>
		/// <returns>A hash code for the current <see cref="T:System.Data.Entity.Core.EntityKey"/>.</returns>
		public override int GetHashCode()
		{
			var hashCode = _hashCode;
			if (hashCode != 0)
				return hashCode;

			_containsByteArray = false;
			if (RequiresDeserialization)
				DeserializeMembers();

			if (_entitySetName != null)
				hashCode = _entitySetName.GetHashCode();

			if (_entityContainerName != null)
				hashCode ^= _entityContainerName.GetHashCode();

			if (_singletonKeyValue != null)
				hashCode = AddHashValue(hashCode, _singletonKeyValue);
			else if (_compositeKeyValues != null)
			{
				var index = 0;
				for (var length = _compositeKeyValues.Length; index < length; ++index)
					hashCode = AddHashValue(hashCode, _compositeKeyValues[index]);
			}
			else
				hashCode = base.GetHashCode();

			if (_isLocked || !string.IsNullOrEmpty(_entitySetName) && !string.IsNullOrEmpty(_entityContainerName) && (_singletonKeyValue != null || _compositeKeyValues != null))
				_hashCode = hashCode;

			return hashCode;
		}

		#endregion

		internal static bool InternalEquals(EntityKey key1, EntityKey key2, bool compareEntitySets)
		{
			if (ReferenceEquals(key1, key2))
				return true;

			if (ReferenceEquals(key1, null)
				|| ReferenceEquals(key2, null)
				|| (ReferenceEquals(NoEntitySetKey, key1)
				|| ReferenceEquals(EntityNotValidKey, key1))
				|| (ReferenceEquals(NoEntitySetKey, key2)
				|| ReferenceEquals(EntityNotValidKey, key2)
				|| key1.GetHashCode() != key2.GetHashCode() && compareEntitySets)
				|| key1._containsByteArray != key2._containsByteArray)
				return false;

			if (key1._singletonKeyValue != null)
			{
				if (key1._containsByteArray)
				{
					if (key2._singletonKeyValue == null || !ByValueEqualityComparer.CompareBinaryValues((byte[])key1._singletonKeyValue, (byte[])key2._singletonKeyValue))
						return false;
				}
				else if (!key1._singletonKeyValue.Equals(key2._singletonKeyValue))
					return false;

				if (!String.Equals(key1._keyNames[0], key2._keyNames[0]))
					return false;
			}
			else
			{
				if (key1._compositeKeyValues == null || key2._compositeKeyValues == null || key1._compositeKeyValues.Length != key2._compositeKeyValues.Length)
					return false;

				if (key1._containsByteArray)
				{
					if (!CompositeValuesWithBinaryEqual(key1, key2))
						return false;
				}
				else if (!CompositeValuesEqual(key1, key2))
					return false;
			}

			return !compareEntitySets || string.Equals(key1._entitySetName, key2._entitySetName) && string.Equals(key1._entityContainerName, key2._entityContainerName);
		}

		internal static bool CompositeValuesWithBinaryEqual(EntityKey key1, EntityKey key2)
		{
			for (var index = 0; index < key1._compositeKeyValues.Length; ++index)
			{
				if (key1._keyNames[index].Equals(key2._keyNames[index]))
				{
					if (!ByValueEqualityComparer.Default.Equals(key1._compositeKeyValues[index], key2._compositeKeyValues[index]))
						return false;
				}
				else if (!ValuesWithBinaryEqual(key1._keyNames[index], key1._compositeKeyValues[index], key2))
					return false;
			}

			return true;
		}

		internal string ConcatKeyValue()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append("EntitySet=").Append(_entitySetName);

			if (IsTemporary)
				return stringBuilder.ToString();

			foreach (var entityKeyMember in EntityKeyValues)
			{
				stringBuilder.Append(';');
				stringBuilder.Append(entityKeyMember.Key).Append("=").Append(entityKeyMember.Value);
			}

			return stringBuilder.ToString();
		}

		internal object FindValueByName(string keyName)
		{
			if (SingletonKeyValue != null)
				return _singletonKeyValue;

			var compositeKeyValues = CompositeKeyValues;
			for (var index = 0; index < compositeKeyValues.Length; ++index)
			{
				if (keyName == _keyNames[index])
					return compositeKeyValues[index];
			}

			throw new ArgumentOutOfRangeException(nameof(keyName));
		}

		internal void InitializeEntitySetName(string qualifiedEntitySetName)
		{
			var strArray = qualifiedEntitySetName.Split('.');
			if (strArray.Length != 2 || String.IsNullOrWhiteSpace(strArray[0]) || String.IsNullOrWhiteSpace(strArray[1]))
				throw new ArgumentException("qualifiedEntitySetName");

			_entityContainerName = strArray[0];
			_entitySetName = strArray[1];
		}

		internal bool InitializeKeyValues(IEnumerable<KeyValuePair<string, object>> entityKeyValues, bool allowNullKeys = false, bool tokenizeStrings = false)
		{
			var keyValuePairs = entityKeyValues as KeyValuePair<string, object>[] ?? entityKeyValues.ToArray();
			var length = keyValuePairs.Length;

			if (length == 1)
			{
				_keyNames = new string[1];
				var keyValuePair = keyValuePairs.Single();
				InitializeKeyValue(keyValuePair, 0, tokenizeStrings);
				_singletonKeyValue = keyValuePair.Value;
			}
			else if (length > 1)
			{
				_keyNames = new string[length];
				_compositeKeyValues = new object[length];
				var i = 0;
				foreach (var keyValuePair in keyValuePairs)
				{
					InitializeKeyValue(keyValuePair, i, tokenizeStrings);
					_compositeKeyValues[i] = keyValuePair.Value;
					++i;
				}
			}
			else if (!allowNullKeys)
				throw new ArgumentException("EntityKeyMustHaveValues", nameof(entityKeyValues));

			return length > 0;
		}

		internal static string LookupSingletonName(string name)
		{
			return !string.IsNullOrEmpty(name) ? NameLookup.GetOrAdd(name, (n => n)) : null;
		}

		private int AddHashValue(int hashCode, object keyValue)
		{
			if (!(keyValue is byte[] bytes))
				return hashCode ^ keyValue.GetHashCode();

			hashCode ^= ByValueEqualityComparer.ComputeBinaryHashCode(bytes);
			_containsByteArray = true;

			return hashCode;
		}

		private static bool ValuesWithBinaryEqual(string keyName, object keyValue, EntityKey key2)
		{
			for (var index = 0; index < key2._keyNames.Length; ++index)
			{
				if (String.Equals(keyName, key2._keyNames[index]))
					return ByValueEqualityComparer.Default.Equals(keyValue, key2._compositeKeyValues[index]);
			}

			return false;
		}

		private static bool CompositeValuesEqual(EntityKey key1, EntityKey key2)
		{
			for (var index = 0; index < key1._compositeKeyValues.Length; ++index)
			{
				if (key1._keyNames[index].Equals(key2._keyNames[index]))
				{
					if (!Equals(key1._compositeKeyValues[index], key2._compositeKeyValues[index]))
						return false;
				}
				else if (!ValuesEqual(key1._keyNames[index], key1._compositeKeyValues[index], key2))
					return false;
			}

			return true;
		}

		private static bool ValuesEqual(string keyName, object keyValue, EntityKey key2)
		{
			for (var index = 0; index < key2._keyNames.Length; ++index)
			{
				if (string.Equals(keyName, key2._keyNames[index]))
					return Equals(keyValue, key2._compositeKeyValues[index]);
			}

			return false;
		}

		private void InitializeKeyValue(KeyValuePair<string, object> keyValuePair, int i, bool tokenizeStrings)
		{
			if (EntityUtil.IsNull(keyValuePair.Value) || string.IsNullOrWhiteSpace(keyValuePair.Key))
				throw new ArgumentException("NoNullsAllowedInKeyValuePairs", nameof(keyValuePair));

			_keyNames[i] = tokenizeStrings ? LookupSingletonName(keyValuePair.Key) : keyValuePair.Key;
		}

		private void ValidateWritable(object instance)
		{
			if (_isLocked || instance != null)
				throw new InvalidOperationException("CannotChangeKey");
		}

		private void DeserializeMembers()
		{
			if (!InitializeKeyValues(new KeyValueReader(_deserializedMembers), true, true))
				return;

			_deserializedMembers = null;
		}

		#endregion

		#region Operators

		/// <summary>
		/// Compares two <see cref="T:System.Data.Entity.Core.EntityKey"/> objects.
		/// </summary>
		/// <param name="key1">A <see cref="T:System.Data.Entity.Core.EntityKey"/> to compare.</param>
		/// <param name="key2">A <see cref="T:System.Data.Entity.Core.EntityKey"/> to compare.</param>
		/// <returns>true if the  key1  and  key2  values are equal; otherwise, false.</returns>
		public static bool operator ==(EntityKey key1, EntityKey key2)
		{
			return InternalEquals(key1, key2, true);
		}

		/// <summary>
		/// Compares two <see cref="T:System.Data.Entity.Core.EntityKey"/> objects.
		/// </summary>
		/// <param name="key1">A <see cref="T:System.Data.Entity.Core.EntityKey"/> to compare.</param>
		/// <param name="key2">A <see cref="T:System.Data.Entity.Core.EntityKey"/> to compare.</param>
		/// <returns>true if the  key1  and  key2  values are not equal; otherwise, false.</returns>
		public static bool operator !=(EntityKey key1, EntityKey key2)
		{
			return !InternalEquals(key1, key2, true);
		}

		#endregion

		#region Nested Classes

		private class KeyValueReader : IEnumerable<KeyValuePair<string, object>>
		{
			private readonly IEnumerable<EntityKeyMember> _enumerator;

			public KeyValueReader(IEnumerable<EntityKeyMember> enumerator)
			{
				_enumerator = enumerator;
			}

			public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
			{
				return (from entityKeyMember in _enumerator
						where entityKeyMember != null
						select new KeyValuePair<string, object>(entityKeyMember.Key, entityKeyMember.Value)).GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		#endregion
	}

	public class EntityUtil
	{
		#region Methods

		public static bool IsNull(object value)
		{
			return value == null || DBNull.Value == value;
		}

		#endregion
	}
}
