#region Namespace Sonata.Data.Entity
//	The Sonata.Data.Entity namespace contains classes that provides access to the core functionalities related to EntityBase.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Sonata.Data.Entity
{
	/// <inheritdoc />
	/// <summary>
	/// The <see cref="T:System.Exception" /> that is thrown for an <see cref="T:Sonata.Data.Entity.EntityBase`1" /> on which the validation process failed.
	/// </summary>
	public class EntityValidationException : Exception
	{
		#region Properties

		/// <summary>
		/// Gets the <see cref="Type"/> from which the <see cref="EntityValidationException"/> has been thrown.
		/// </summary>
		public new Type Source { get; }

		/// <summary>
		/// Gets the result of the validation process on the <see cref="EntityBase{T}"/>.
		/// </summary>
		public IEnumerable<ValidationResult> ValidationResults { get; }

		/// <summary>
		/// Gets the <see cref="ValidationResults"/> converted into an <see cref="IEnumerable{string}"/>.
		/// </summary>
		public IEnumerable<string> Logs { get { return ValidationResults.Select(e => e.ErrorMessage); } }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityValidationException"/> class.
		/// </summary>
		/// <param name="source">The <see cref="Type"/> from which the <see cref="EntityValidationException"/> has been thrown.</param>
		/// <param name="validationResults">The result of the validation process on the <see cref="EntityBase{T}"/>.</param>
		public EntityValidationException(Type source, IEnumerable<ValidationResult> validationResults)
			: this(source, validationResults, null)
		{
		}

		/// <inheritdoc />
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sonata.Data.Entity.EntityValidationException" /> class.
		/// </summary>
		/// <param name="source">The <see cref="T:System.Type" /> from which the <see cref="T:Sonata.Data.Entity.EntityValidationException" /> has been thrown.</param>
		/// <param name="validationResults">The result of the validation process on the <see cref="T:Sonata.Data.Entity.EntityBase`1" />.</param>
		/// <param name="innerException">The <see cref="T:System.Exception" /> that is the cause of the current <see cref="T:Sonata.Data.Entity.EntityValidationException" />.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="validationResults" /> is NULL.</exception>
		public EntityValidationException(Type source, IEnumerable<ValidationResult> validationResults, Exception innerException)
			: base(String.Empty, innerException)
		{
			ValidationResults = validationResults ?? throw new ArgumentNullException(nameof(validationResults));
			Source = source ?? throw new ArgumentNullException(nameof(source));
		}

		#endregion
	}
}