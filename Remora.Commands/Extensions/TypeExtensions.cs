//
//  TypeExtensions.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Parsers;
using Remora.Results;

namespace Remora.Commands.Extensions;

/// <summary>
/// Defines extensions to the <see cref="Type"/> class.
/// </summary>
internal static class TypeExtensions
{
    private static readonly Type _typeParserType = typeof(ITypeParser);
    private static readonly Type _genericTypeParserType = typeof(ITypeParser<>);

    /// <summary>
    /// Attempts to get an annotated group name from the given type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="groupName">The group name.</param>
    /// <returns>true if a group name was retrieved; otherwise, false.</returns>
    public static bool TryGetGroupName(this Type type, [NotNullWhen(true)] out string? groupName)
    {
        groupName = null;

        var groupNameAttribute = type.GetCustomAttribute<GroupAttribute>();
        if (groupNameAttribute is null)
        {
            return false;
        }

        // treat empty strings as having no name
        if (string.IsNullOrWhiteSpace(groupNameAttribute.Name))
        {
            return false;
        }

        groupName = groupNameAttribute.Name;
        return true;
    }

    /// <summary>
    /// Determines whether the given typ is a type parser.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>true if the type is a type parser; otherwise, false.</returns>
    public static bool IsTypeParser(this Type type)
    {
        var interfaces = type.GetInterfaces();
        return interfaces.Contains(_typeParserType) ||
               interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == _genericTypeParserType);
    }

    /// <summary>
    /// Determines whether the type is a supported enumerable type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>true if the type is an enumerable type; otherwise, false.</returns>
    public static bool IsSupportedCollection(this Type type)
    {
        if (type.IsGenericType)
        {
            type = type.GetGenericTypeDefinition();
        }

        switch (type)
        {
            case var _ when type == typeof(IEnumerable<>):
            case var _ when type == typeof(ICollection<>):
            case var _ when type == typeof(IList<>):
            case var _ when type == typeof(IReadOnlyCollection<>):
            case var _ when type == typeof(IReadOnlyList<>):
            case var _ when type == typeof(List<>):
            case var _ when type.IsArray:
            {
                return true;
            }
            default:
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Determines whether the type is a nullable struct.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>true if the type is a nullable struct; otherwise, false.</returns>
    public static bool IsNullableStruct(this Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        return type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// Gets the element type of the given type. The type is assumed to return true if
    /// <see cref="IsSupportedCollection"/> were to be called on it.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The element type.</returns>
    public static Type GetCollectionElementType(this Type type)
    {
        if (type.IsArray)
        {
            return type.GetElementType() ?? throw new InvalidOperationException();
        }

        return type.GetGenericArguments()[0];
    }

    /// <summary>
    /// Checks whether the given type is a supported return type for a command. Currently, supported return types
    /// are limited to <see cref="Task{TResult}"/> or <see cref="ValueTask{TResult}"/>, where TResult is any of
    /// <see cref="IResult"/>, <see cref="Result"/>, or <see cref="Result{TEntity}"/>, where TEntity is any type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>True if the type is supported; otherwise, false.</returns>
    public static bool IsSupportedCommandReturnType(this Type type)
    {
        if (!type.IsGenericType)
        {
            // The return type must be generic
            return false;
        }

        var genericTypeDefinition = type.GetGenericTypeDefinition();
        if (genericTypeDefinition != typeof(Task<>) && genericTypeDefinition != typeof(ValueTask<>))
        {
            // The return type must be a task
            return false;
        }

        var innerType = type.GetGenericArguments().Single();
        return typeof(IResult).IsAssignableFrom(innerType);
    }
}
