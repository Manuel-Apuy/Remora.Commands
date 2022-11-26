//
//  NamedCollectionParameterShape.cs
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
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Remora.Commands.Conditions;
using Remora.Commands.Extensions;
using Remora.Commands.Tokenization;
using Remora.Commands.Trees;

namespace Remora.Commands.Signatures;

/// <summary>
/// Represents a named parameter with a single value.
/// </summary>
[PublicAPI]
public class NamedCollectionParameterShape : NamedParameterShape, ICollectionParameterShape
{
    private static readonly MethodInfo _emptyArrayMethod;
    private readonly object _emptyCollection;

    /// <inheritdoc />
    public ulong? Min { get; }

    /// <inheritdoc />
    public ulong? Max { get; }

    /// <inheritdoc/>
    public override object? DefaultValue { get; }

    static NamedCollectionParameterShape()
    {
        var emptyArrayMethod = typeof(Array).GetMethod(nameof(Array.Empty));
        _emptyArrayMethod = emptyArrayMethod ?? throw new MissingMethodException();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedCollectionParameterShape"/> class.
    /// </summary>
    /// <param name="parameter">The underlying parameter.</param>
    /// <param name="shortName">The short name.</param>
    /// <param name="longName">The long name.</param>
    /// <param name="min">The minimum number of items in the collection.</param>
    /// <param name="max">The maximum number of items in the collection.</param>
    /// <param name="description">The description of the parameter.</param>
    public NamedCollectionParameterShape
    (
        ParameterInfo parameter,
        char shortName,
        string longName,
        ulong? min,
        ulong? max,
        string? description = null
    )
        : base(parameter, shortName, longName, description)
    {
        this.Min = min;
        this.Max = max;

        var elementType = parameter.ParameterType.GetCollectionElementType();

        var emptyArrayMethod = _emptyArrayMethod.MakeGenericMethod(elementType);
        _emptyCollection = emptyArrayMethod.Invoke(null, null)!;

        DefaultValue = parameter.IsOptional ? parameter.DefaultValue :
            this.Min is null or 0 ? _emptyCollection : throw new InvalidOperationException();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedCollectionParameterShape"/> class.
    /// </summary>
    /// <param name="parameter">The underlying parameter.</param>
    /// <param name="shortName">The short name.</param>
    /// <param name="min">The minimum number of items in the collection.</param>
    /// <param name="max">The maximum number of items in the collection.</param>
    /// <param name="description">The description of the parameter.</param>
    public NamedCollectionParameterShape
    (
        ParameterInfo parameter,
        char shortName,
        ulong? min,
        ulong? max,
        string? description = null
    )
        : base(parameter, shortName, description)
    {
        this.Min = min;
        this.Max = max;

        var elementType = parameter.ParameterType.GetCollectionElementType();

        var emptyArrayMethod = _emptyArrayMethod.MakeGenericMethod(elementType);
        _emptyCollection = emptyArrayMethod.Invoke(null, null)!;

        DefaultValue = parameter.IsOptional ? parameter.DefaultValue :
            this.Min is null or 0 ? _emptyCollection : throw new InvalidOperationException();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedCollectionParameterShape"/> class.
    /// </summary>
    /// <param name="shortName">The short name.</param>
    /// <param name="longName">The long name.</param>
    /// <param name="min">The minimum number of items in the collection.</param>
    /// <param name="max">The maximum number of items in the collection.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="parameterType">The type of the parameter.</param>
    /// <param name="isOptional">Whether the parameter is optional.</param>
    /// <param name="defaultValue">The default value of the parameter, if any.</param>
    /// <param name="attributes">The attributes of the parameter.</param>
    /// <param name="conditions">The conditions of the parameter.</param>
    /// <param name="description">The description of the paremeter.</param>
    public NamedCollectionParameterShape
    (
        char? shortName,
        string? longName,
        ulong? min,
        ulong? max,
        string parameterName,
        Type parameterType,
        bool isOptional,
        object? defaultValue,
        IReadOnlyList<Attribute> attributes,
        IReadOnlyList<ConditionAttribute> conditions,
        string description
    )
    : base(shortName, longName, parameterName, parameterType, isOptional, defaultValue, attributes, conditions, description)
    {
        this.Min = min;
        this.Max = max;

        var elementType = parameterType.GetCollectionElementType();

        var emptyArrayMethod = _emptyArrayMethod.MakeGenericMethod(elementType);
        _emptyCollection = emptyArrayMethod.Invoke(null, null)!;

        DefaultValue = isOptional ? defaultValue : _emptyCollection;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedCollectionParameterShape"/> class.
    /// </summary>
    /// <param name="parameter">The underlying parameter.</param>
    /// <param name="longName">The long name.</param>
    /// <param name="min">The minimum number of items in the collection.</param>
    /// <param name="max">The maximum number of items in the collection.</param>
    /// <param name="description">The description of the parameter.</param>
    public NamedCollectionParameterShape
    (
        ParameterInfo parameter,
        string longName,
        ulong? min,
        ulong? max,
        string? description = null
    )
        : base(parameter, longName, description)
    {
        this.Min = min;
        this.Max = max;

        var elementType = parameter.ParameterType.GetCollectionElementType();

        var emptyArrayMethod = _emptyArrayMethod.MakeGenericMethod(elementType);
        _emptyCollection = emptyArrayMethod.Invoke(null, null)!;
    }

    /// <inheritdoc/>
    public override bool Matches
    (
        TokenizingEnumerator tokenizer,
        out ulong consumedTokens,
        TreeSearchOptions? searchOptions = null
    )
    {
        searchOptions ??= new TreeSearchOptions();
        consumedTokens = 0;

        if (!tokenizer.MoveNext())
        {
            return false;
        }

        switch (tokenizer.Current.Type)
        {
            case TokenType.LongName:
            {
                if (this.LongName is null)
                {
                    return false;
                }

                if (!tokenizer.Current.Value.Equals(this.LongName, searchOptions.KeyComparison))
                {
                    return false;
                }

                break;
            }
            case TokenType.ShortName:
            {
                if (this.ShortName is null)
                {
                    return false;
                }

                if (tokenizer.Current.Value.Length != 1)
                {
                    return false;
                }

                if (tokenizer.Current.Value[0] != this.ShortName.Value)
                {
                    return false;
                }

                break;
            }
            case TokenType.Value:
            {
                return false;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        ulong itemCount = 0;
        while (this.Max is null || itemCount < this.Max.Value)
        {
            if (!tokenizer.MoveNext())
            {
                break;
            }

            if (tokenizer.Current.Type != TokenType.Value)
            {
                break;
            }

            ++itemCount;
        }

        if (this.Min is not null)
        {
            if (itemCount < this.Min.Value)
            {
                return false;
            }
        }

        consumedTokens = itemCount + 1;
        return true;
    }

    /// <inheritdoc/>
    public override bool Matches
    (
        KeyValuePair<string, IReadOnlyList<string>> namedValue,
        out bool isFatal,
        TreeSearchOptions? searchOptions = null
    )
    {
        searchOptions ??= new TreeSearchOptions();
        isFatal = false;

        var (name, value) = namedValue;
        var nameMatches = name.Equals(this.LongName, searchOptions.KeyComparison) ||
                          (this.ShortName is not null && name.Length == 1 && name[0] == this.ShortName);

        if (!nameMatches)
        {
            return false;
        }

        var count = (ulong)value.LongCount();
        if (count < this.Min)
        {
            isFatal = true;
            return false;
        }

        if (this.Max is null)
        {
            return true;
        }

        if (count <= this.Max)
        {
            return true;
        }

        isFatal = true;
        return false;
    }

    /// <inheritdoc/>
    public override bool IsOmissible(TreeSearchOptions? searchOptions = null)
    {
        if (this.IsOptional)
        {
            return true;
        }

        return this.Min is null or 0;
    }
}
