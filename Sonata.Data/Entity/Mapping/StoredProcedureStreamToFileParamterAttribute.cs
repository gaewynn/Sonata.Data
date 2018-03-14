#region Namespace Sonata.Data.Entity.Mapping
//	The Sonata.Data.Entity.Mapping namespace contains classes that are used to generate a LINQ to SQL object model that represents the structure and content of a database.
#endregion

using System;
using System.IO;
using Sonata.Core.Extensions;

namespace Sonata.Data.Entity.Mapping
{
	/// <inheritdoc />
	/// <summary>
	/// Represents a class to manage stored procedure stream <see cref="T:System.Data.Common.DbParameter" /> that a property is mapped to by streaming data in a <see cref="T:System.IO.FileStream" />.
	/// </summary>
	/// <remarks>Use this type when the SQL column type is Binary, Image, Varbinary or UDT</remarks>
	[AttributeUsage(AttributeTargets.Property)]
	public class StoredProcedureStreamToFileParamterAttribute : StoredProcedureStreamOutputParamterAttribute
	{
		#region Members

		private string _fileName;
		private string _filePath;
		private string _fileNamePropertyName;
		private string _filePathPropertyName;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets a value indicating the name of the file where the <see cref="Stream"/> will be written.
		/// </summary>
		/// <exception cref="ArgumentNullException">value is NULL.</exception>
		/// <exception cref="ArgumentException">value is empty or whitespace.</exception>
		/// <remarks>If both <see cref="FileName"/> and <see cref="FileNamePropertyName"/> are set, only the value of <see cref="FileNamePropertyName"/> will be used.</remarks>
		public string FileName
		{
			get => _fileName;
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				if (String.IsNullOrWhiteSpace(value))
					throw new ArgumentException("value can not be empty or whitespace");

				_fileName = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating the path of the file where the <see cref="Stream"/> will be written.
		/// </summary>
		/// <exception cref="ArgumentNullException">value is NULL.</exception>
		/// <exception cref="ArgumentException">value is empty or whitespace.</exception>
		/// <remarks>If both <see cref="FilePath"/> and <see cref="FilePathPropertyName"/> are set, only the value of <see cref="FilePathPropertyName"/> will be used.</remarks>
		public string FilePath
		{
			get => _filePath;
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				if (String.IsNullOrWhiteSpace(value))
					throw new ArgumentException("value can not be empty or whitespace");

				_filePath = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating the name of a property which the value is considered as the file where the <see cref="Stream"/> will be written.
		/// </summary>
		/// <exception cref="ArgumentNullException">value is NULL.</exception>
		/// <exception cref="ArgumentException">value is empty or whitespace.</exception>
		/// <remarks>If both <see cref="FileName"/> and <see cref="FileNamePropertyName"/> are set, only the value of <see cref="FileNamePropertyName"/> will be used.</remarks>
		public string FileNamePropertyName
		{
			get => _fileNamePropertyName;
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				if (String.IsNullOrWhiteSpace(value))
					throw new ArgumentException("value can not be empty or whitespace");

				_fileNamePropertyName = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating the name of a property which the value is considered as the path where the <see cref="Stream"/> will be written.
		/// </summary>
		/// <exception cref="ArgumentNullException">value is NULL.</exception>
		/// <exception cref="ArgumentException">value is empty or whitespace.</exception>
		/// <remarks>If both <see cref="FilePath"/> and <see cref="FilePathPropertyName"/> are set, only the value of <see cref="FilePathPropertyName"/> will be used.</remarks>
		public string FilePathPropertyName
		{
			get => _filePathPropertyName;
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				if (String.IsNullOrWhiteSpace(value))
					throw new ArgumentException("value can not be empty or whitespace");

				_filePathPropertyName = value;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Create the <see cref="FileStream"/> using values set on properties.
		/// </summary>
		/// <param name="streamedParameter">The instance of the <see cref="Object"/> containing all properties used to describe the streamed parameter.</param>
		/// <returns></returns>
		internal Stream CreateStream(object streamedParameter)
		{
			if (streamedParameter == null)
				throw new ArgumentNullException(nameof(streamedParameter));

			var filePath = FilePath;
			var fileName = FileName;
			var propertyType = streamedParameter.GetType();

			if (!String.IsNullOrWhiteSpace(FilePathPropertyName))
			{
				var propertyValue = streamedParameter.GetPropertyValue(propertyType, FilePathPropertyName);
				if (propertyValue != null)
					filePath = propertyValue.ToString();

				if (String.IsNullOrWhiteSpace(filePath))
					throw new InvalidOperationException("Can not determine the path of the file where the stream has to be written. Either the FilePath or FilePathPropertyName property has to be set on the StoredProcedureStreamToFileParamter attribute.");
			}

			if (!String.IsNullOrWhiteSpace(FileNamePropertyName))
			{
				var propertyValue = streamedParameter.GetPropertyValue(propertyType, FileNamePropertyName);
				if (propertyValue != null)
					fileName = propertyValue.ToString();

				if (String.IsNullOrWhiteSpace(fileName))
					throw new InvalidOperationException("Can not determine the name of the file where the stream has to be written. Either the FileName or FileNamePropertyName property has to be set on the StoredProcedureStreamToFileParamter attribute.");
			}

			return new FileStream(Path.Combine(filePath, fileName), FileMode.OpenOrCreate, FileAccess.Write);
		}

		#endregion
	}
}