// <copyright company="RealDimensions Software, LLC" file="NullExtensions.cs">
//   Copyright 2015 - Present RealDimensions Software, LLC
// </copyright>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// 
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace chocolatey.package.verifier
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    ///   implement null-safe dereferencing 'operator' (safe navigation operator)
    /// </summary>
    public static class NullExtensions
    {
        public static TValue ValueOrDefault<TSource, TValue>(this TSource instance, Expression<Func<TSource, TValue>> expression)
        {
            return ValueOrDefault(instance, expression, true);
        }

        internal static TProperty EvaluateExpression<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> expression)
        {
            var method = expression.Body as MethodCallExpression;

            if (method != null)
            {
                return ValueOrDefault(source, expression, false);
            }

            var body = expression.Body as MemberExpression;

            if (body == null)
            {
                const string Format = "Expression '{0}' must refer to a property.";
                string message = string.Format(Format, expression);
                throw new ArgumentException(message);
            }

            object value = EvaluateMemberExpression(source, body);

            if (ReferenceEquals(value, null))
            {
                return default(TProperty);
            }

            return (TProperty)value;
        }

        private static object EvaluateMemberExpression(object instance, MemberExpression memberExpression)
        {
            if (memberExpression == null)
            {
                return instance;
            }

            instance = EvaluateMemberExpression(instance, memberExpression.Expression as MemberExpression);
            var propertyInfo = memberExpression.Member as PropertyInfo;
            instance = ValueOrDefault(instance, item => propertyInfo.GetValue(item, null), false);
            return instance;
        }

        private static TValue ValueOrDefault<TSource, TValue>(this TSource instance, Expression<Func<TSource, TValue>> expression, bool nested)
        {
            return ReferenceEquals(instance, default(TSource))
                       ? default(TValue)
                       : nested
                             ? EvaluateExpression(instance, expression)
                             : expression.Compile()(instance);
        }
    }
}