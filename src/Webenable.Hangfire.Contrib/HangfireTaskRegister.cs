using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Webenable.Hangfire.Contrib
{
    public static class HangfireTaskRegister
    {
        private static readonly Dictionary<string, HangfireTaskDefinition> _tasks = new Dictionary<string, HangfireTaskDefinition>();

        public static IReadOnlyDictionary<string, HangfireTaskDefinition> Tasks => new ReadOnlyDictionary<string, HangfireTaskDefinition>(_tasks);

        /// <summary>
        /// Adds the specified method of the task to the Hangfire task register.
        /// </summary>
        /// <typeparam name="TTask">The type of the task.</typeparam>
        /// <param name="expression">An expression which represents the method call to invoke the task.</param>
        public static void AddTask<TTask>(Expression<Func<TTask, object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }
            if (!(expression.Body is MethodCallExpression methodCall))
            {
                throw new ArgumentException("Specified expression is not a valid method call expression.", nameof(expression));
            }

            // Create a Hangfire task instance from the method call
            var method = methodCall.Method;
            var task = new HangfireTaskDefinition
            {
                Type = typeof(TTask),
                Method = method,
                // Add each simple parameter to the task definition
                Parameters = method
                    .GetParameters()
                    .Where(p => IsSimple(p.ParameterType))
                    .Select(p => new HangfireTaskParameter { Name = p.Name, Type = p.ParameterType }).ToArray()
            };

            _tasks[task.Type.Name] = task;

            bool IsSimple(Type type)
            {
                type = Nullable.GetUnderlyingType(type) ?? type;
                return type.IsPrimitive
                  || type.IsEnum
                  || type.Equals(typeof(string))
                  || type.Equals(typeof(decimal));
            }
        }
    }

    public class HangfireTaskDefinition
    {
        public Type Type { get; set; }

        public MethodInfo Method { get; set; }

        public HangfireTaskParameter[] Parameters { get; set; }
    }

    public class HangfireTaskParameter
    {
        public string Name { get; set; }

        public Type Type { get; set; }
    }
}
