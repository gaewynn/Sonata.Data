#region Namespace Sonata.Data.Entity
//	The Sonata.Data.Entity namespace contains classes that provides access to the core functionalities related to EntityBase.
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Sonata.Core.Collections.Generic;
using Sonata.Diagnostics.Logs;

namespace Sonata.Data.Entity
{
	/// <summary>
	/// Represents the base class of all services.
	/// </summary>
	public class ServiceValidator
	{
		#region Members

		private static StringCiAiComparer _stringCiAiComparer;

		#endregion

		#region Properties

		/// <summary>
		/// Gets a <see cref="StringCiAiComparer"/> configured to trim the compared strings during comparison.
		/// </summary>
		protected static StringCiAiComparer StringCiAiComparer => _stringCiAiComparer ?? (_stringCiAiComparer = new StringCiAiComparer(true, false));

		#endregion

		#region Methods

		public bool RunValidation<T>(List<Log> logs, Type source, EntityBase<T> entity, CrudOperation? crudOperation = null, string baseMessage = null)
			where T : IValidatableObject, new()
		{
			if (crudOperation.HasValue)
			{
				switch (crudOperation.Value)
				{
					case CrudOperation.Create: entity.InitializeValidationContextValues(BaseValidator.CreationValidationRules); break;
					case CrudOperation.Read: entity.InitializeValidationContextValues(BaseValidator.ReadValidationRules); break;
					case CrudOperation.Update: entity.InitializeValidationContextValues(BaseValidator.UpdateValidationRules); break;
					case CrudOperation.Delete: entity.InitializeValidationContextValues(BaseValidator.DeleteValidationRules); break;
				}
			}

			if (!entity.Validate())
				logs.AddRange(entity.ValidationResults.Select(e => BuildAndWriteErrorLog(source, $"{(baseMessage ?? String.Empty)} {e.ErrorMessage}")));

			return !logs.Any();
		}

		public bool RunValidation<T>(ConcurrentStack<Log> logs, Type source, EntityBase<T> entity, CrudOperation? crudOperation = null, string baseMessage = null)
			where T : IValidatableObject, new()
		{
			if (crudOperation.HasValue)
			{
				switch (crudOperation.Value)
				{
					case CrudOperation.Create: entity.InitializeValidationContextValues(BaseValidator.CreationValidationRules); break;
					case CrudOperation.Read: entity.InitializeValidationContextValues(BaseValidator.ReadValidationRules); break;
					case CrudOperation.Update: entity.InitializeValidationContextValues(BaseValidator.UpdateValidationRules); break;
					case CrudOperation.Delete: entity.InitializeValidationContextValues(BaseValidator.DeleteValidationRules); break;
				}
			}

			if (!entity.Validate())
				logs.PushRange(entity.ValidationResults.Select(e => BuildAndWriteErrorLog(source, $"{(baseMessage ?? String.Empty)} {e.ErrorMessage}")).ToArray());

			return !logs.Any();
		}

		/// <summary>
		/// Builds a <see cref="TechnicalLog"/> and writes it to the configured log output.
		/// </summary>
		/// <param name="source">The source of the <see cref="TechnicalLog"/>.</param>
		/// <param name="message">An error message for the <see cref="TechnicalLog"/>.</param>
		/// <returns>The created <see cref="TechnicalLog"/></returns>
		protected Log BuildAndWriteErrorLog(Type source, string message)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (String.IsNullOrWhiteSpace(message))
				throw new ArgumentNullException(nameof(message));

			var log = new TechnicalLog(source, LogLevels.Error, message);
			log.Write();

			return log;
		}

		/// <summary>
		/// Builds a <see cref="TechnicalLog"/> and writes it to the configured log output.
		/// </summary>
		/// <param name="source">The source of the <see cref="TechnicalLog"/>.</param>
		/// <param name="message">A fatal message for the <see cref="TechnicalLog"/>.</param>
		/// <param name="exception">The exception that occured.</param>
		/// <returns>The created <see cref="TechnicalLog"/></returns>
		protected Log BuildAndWriteFatalLog(Type source, string message, Exception exception)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (String.IsNullOrWhiteSpace(message))
				throw new ArgumentNullException(nameof(message));
			if (exception == null)
				throw new ArgumentNullException(nameof(exception));

			var log = new TechnicalLog(source, LogLevels.Fatal, message, exception);
			log.Write();

			return log;
		}

		/// <summary>
		/// Builds a <see cref="TechnicalLog"/> and writes it to the configured log output.
		/// </summary>
		/// <param name="source">The source of the <see cref="TechnicalLog"/>.</param>
		/// <param name="message">A warning message for the <see cref="TechnicalLog"/>.</param>
		/// <returns>The created <see cref="TechnicalLog"/></returns>
		protected Log BuildAndWriteWarningLog(Type source, string message)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (String.IsNullOrWhiteSpace(message))
				throw new ArgumentNullException(nameof(message));

			var log = new TechnicalLog(source, LogLevels.Warning, message);
			log.Write();

			return log;
		}

		#endregion
	}
}
