#region Namespace Sonata.Data.Entity.Mapping
//	The Sonata.Data.Entity.Mapping namespace contains classes that are used to generate a LINQ to SQL object model that represents the structure and content of a database.
#endregion

using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Sonata.Data.Entity.Mapping
{
	/// <inheritdoc />
	/// <summary>
	/// Represents a class to manage stored procedure stream <see cref="T:System.Data.Common.SqlParameter" /> that a property is mapped to by streaming data in a <see cref="T:System.IO.MemoryStream" />.
	/// </summary>
	/// <remarks>Use this type when the SQL column type is Binary, Image, Varbinary or UDT</remarks>
	[AttributeUsage(AttributeTargets.Property)]
	public class StoredProcedureStreamToMemoryParamterAttribute : StoredProcedureStreamOutputParamterAttribute
	{
		#region Members

		private string _encoding = "Default";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets a value indicating the <see cref="Encoding"/> used to write the <see cref="MemoryStream"/> into the property.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is NULL.</exception>
		/// <exception cref="ArgumentException"><paramref name="value"/> is empty or whitespace.</exception>
		/// <remarks>By default, this property is set to "Default"</remarks>
		public string Encoding
		{
			get => _encoding;
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				if (String.IsNullOrWhiteSpace(value))
					throw new ArgumentException("value can not be empty or whitespace");

				_encoding = value;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Create Memory Stream 
		/// </summary>
		/// <returns></returns>
		internal Stream CreateStream()
		{
			return new MemoryStream();
		}

		/// <summary>
		/// Resolve Encoding for conversion of MemoryStream to String
		/// </summary>
		/// <returns></returns>
		internal Encoding GetEncoding()
		{
			return (Encoding)typeof(Encoding).InvokeMember(
				Encoding,
				BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.IgnoreCase,
				null,
				null,
				null);
		}

		#endregion
	}
}