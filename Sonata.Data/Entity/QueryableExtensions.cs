#region Namespace Sonata.Data.Entity
//	The Sonata.Data.Entity namespace contains classes that provides access to the core functionalities related to EntityBase.
#endregion

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Sonata.Data.Entity
{
	public static class QueryableExtensions
	{
		public static IQueryable<TEntity> Include<TEntity, TProperty>(this IQueryable<TEntity> source,
			int levelIndex, Expression<Func<TEntity, ICollection<TEntity>>> expression, Expression<Func<TEntity, TProperty>> path) where TEntity : class
		{
			if (levelIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(levelIndex));

			var queryables = new List<string>();

			var member = (MemberExpression)expression.Body;
			var property = member.Member.Name;

			var sb = new StringBuilder();
			for (var i = 0; i < levelIndex; i++)
			{
				if (i > 0)
					sb.Append(Type.Delimiter);

				sb.Append(property);

				queryables.Add(sb.ToString());
			}

			foreach (var queryable in queryables)
			{
				if (!TryParsePath(path.Body, out var path1))
					throw new ArgumentException("path");
				
				path1 = String.Join(separator: Type.Delimiter.ToString(), values: path1.IndexOf(Type.Delimiter) >= 0 ? path1.Split(Type.Delimiter).Skip(1) : path1.Split(Type.Delimiter));
				source = source.Include(queryable + Type.Delimiter + path1);
			}

			return source;
		}

		public static IQueryable<TEntity> Include<TEntity>(this IQueryable<TEntity> source,
			int levelIndex, Expression<Func<TEntity, ICollection<TEntity>>> expression) where TEntity : class
		{
			if (levelIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(levelIndex));

			var member = (MemberExpression)expression.Body;
			var property = member.Member.Name;

			var sb = new StringBuilder();
			for (var i = 0; i < levelIndex; i++)
			{
				if (i > 0)
					sb.Append(Type.Delimiter);

				sb.Append(property);
			}

			source = source.Include(sb.ToString());

			return source;
		}

		public static bool TryParsePath(Expression expression, out string path)
		{
			path = null;
			var expression1 = expression.RemoveConvert();
			var memberExpression = expression1 as MemberExpression;
			var methodCallExpression = expression1 as MethodCallExpression;
			if (memberExpression != null)
			{
				var name = memberExpression.Member.Name;
				if (!TryParsePath(memberExpression.Expression, out var path1))
					return false;
				path = path1 == null ? name : path1 + "." + name;
			}
			else if (methodCallExpression != null)
			{
				if (methodCallExpression.Method.Name == "Select" && methodCallExpression.Arguments.Count == 2 && (TryParsePath(methodCallExpression.Arguments[0], out var path1) && path1 != null))
				{
					if (methodCallExpression.Arguments[1] is LambdaExpression lambdaExpression 
					    && TryParsePath(lambdaExpression.Body, out var path2) && path2 != null)
					{
						path = path1 + "." + path2;
						return true;
					}
				}
				return false;
			}
			return true;
		}
	}

	public static class ExpressionExtensions
	{
		public static Expression RemoveConvert(this Expression expression)
		{
			while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
				expression = ((UnaryExpression)expression).Operand;
			return expression;
		}
	}
}