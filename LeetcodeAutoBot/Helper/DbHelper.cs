using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace LeetcodeAutoBot.Helper;

public static class DbHelper
{
    /// <summary>
    /// 添加或更新
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey">按哪个字段更新</typeparam>
    /// <param name="dbSet"></param>
    /// <param name="keySelector">按哪个字段更新</param>
    /// <param name="entities"></param>
    /// <param name="ignoreNavigationProperty">是否忽略导航属性</param>
    public static void AddOrUpdate<T, TKey>(this DbSet<T> dbSet,
        Expression<Func<T, TKey>> keySelector,
        IEnumerable<T> entities,
        bool ignoreNavigationProperty = false)
        where T : class
        where TKey : notnull
    {
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));

        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        var collection = entities as ICollection<T> ?? entities.ToList();
        if (collection.Count == 0)
            return;

        var func = keySelector.Compile();
        var keyObjects = collection.Select(func).Distinct().ToList(); // Remove duplicates
        if (keyObjects.Count == 0)
            return;

        var parameter = keySelector.Parameters[0];

        Expression combinedCondition;

        // Handle primitive types differently than complex types
        if (typeof(TKey).IsPrimitive || typeof(TKey) == typeof(string) || !typeof(TKey).GetProperties().Any())
        {
            // For primitive types, directly create equality expressions
            var conditions = keyObjects.Select(keyObj =>
            {
                var memberAccess = keySelector.Body;
                var constant = Expression.Constant(keyObj, typeof(TKey));
                return Expression.Equal(memberAccess, constant);
            }).ToList();

            combinedCondition = conditions.Aggregate(Expression.OrElse);
        }
        else
        {
            // For complex types with properties, use the original approach
            var keyProperties = typeof(TKey).GetProperties();
            var conditions = new List<Expression>();

            foreach (var keyObj in keyObjects)
            {
                Expression? condition = null;
                foreach (var prop in keyProperties)
                {
                    object? keyValue = prop.GetValue(keyObj);
                    var memberAccess = Expression.Property(keySelector.Body, prop);
                    var constant = Expression.Constant(keyValue, prop.PropertyType);
                    var equality = Expression.Equal(memberAccess, constant);
                    condition = condition == null ? equality : Expression.AndAlso(condition, equality);
                }

                if (condition != null)
                {
                    conditions.Add(condition);
                }
            }

            combinedCondition = conditions.Aggregate((expr1, expr2) => Expression.OrElse(expr1, expr2));
        }

        var lambda = Expression.Lambda<Func<T, bool>>(combinedCondition, parameter);
        var items = dbSet.Where(lambda).ToDictionary(func);
        
        // Rest of the original code remains the same...
        foreach (var entity in collection)
        {
            var key = func(entity);
            if (items.TryGetValue(key, out var existingItem))
            {
                // Update existing item
                var keyIgnoreFields = typeof(T).GetProperties()
                    .Where(p => p.GetCustomAttribute<KeyAttribute>() != null)
                    .ToList();

                if (!keyIgnoreFields.Any())
                {
                    string idName = typeof(T).Name + "Id";
                    keyIgnoreFields.AddRange(typeof(T).GetProperties()
                        .Where(p => p.Name.Equals(
                                        "Id", StringComparison.OrdinalIgnoreCase) ||
                                    p.Name.Equals(
                                        idName, StringComparison.OrdinalIgnoreCase)));
                }

                if (ignoreNavigationProperty)
                {
                    keyIgnoreFields.AddRange(typeof(T).GetProperties()
                        .Where(p => p.PropertyType.Namespace ==
                                    "System.Collections.Generic"));
                }

                foreach (var p in typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite))
                {
                    if (keyIgnoreFields.Any(x => x.Name == p.Name))
                        continue;

                    var newValue = p.GetValue(entity);
                    var existingValue = p.GetValue(existingItem);
                    if (!Equals(newValue, existingValue))
                        p.SetValue(existingItem, newValue);
                }

                foreach (var idField in keyIgnoreFields.Where(p => p.CanRead && p.CanWrite))
                {
                    var existingValue = idField.GetValue(existingItem);
                    var newValue = idField.GetValue(entity);
                    if (!Equals(newValue, existingValue))
                        idField.SetValue(entity, existingValue);
                }
            }
            else
            {
                dbSet.Add(entity);
            }
        }
    }
}