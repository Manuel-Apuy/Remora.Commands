//
//  CommandNode.cs
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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Builders;
using Remora.Commands.Conditions;
using Remora.Commands.Signatures;
using Remora.Commands.Tokenization;
using Remora.Results;

namespace Remora.Commands.Trees.Nodes;

/// <summary>
/// Represents a delegate that executes a command.
/// </summary>
/// <param name="services">The service provider.</param>
/// <param name="parameters">The parameters to be passed to the command.</param>
/// <param name="cancellationToken">The cancellation token.</param>
/// <returns>The result of executing the command.</returns>
public delegate ValueTask<IResult> CommandInvocation(IServiceProvider services, object?[] parameters, CancellationToken cancellationToken);

/// <summary>
/// Represents a command in a command group.
/// </summary>
[PublicAPI]
public class CommandNode : IChildNode
{
    /// <summary>
    /// Gets the delegate that represents the command, or invokes it.
    /// </summary>
    public CommandInvocation Invoke { get; }

    /// <inheritdoc/>
    public IParentNode Parent { get; }

    /// <summary>
    /// Gets the general shape of the command.
    /// </summary>
    public CommandShape Shape { get; }

    /// <summary>
    /// Gets the attributes of the command.
    /// </summary>
    public IReadOnlyList<Attribute> Attributes { get; }

    /// <summary>
    /// Gets the conditions of the command.
    /// </summary>
    public IReadOnlyList<ConditionAttribute> Conditions { get; }

    /// <inheritdoc/>
    /// <remarks>
    /// This key value represents the name of the command, which terminates the command prefix.
    /// </remarks>
    public string Key { get; }

    /// <inheritdoc />
    public IReadOnlyList<string> Aliases { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandNode"/> class.
    /// </summary>
    /// <param name="parent">The parent node.</param>
    /// <param name="key">The key value of the command node.</param>
    /// <param name="invoke">The function to invoke the command.</param>
    /// <param name="shape">The shape of the command.</param>
    /// <param name="aliases">Additional key aliases, if any.</param>
    /// <param name="attributes">Applied attributes for the command, if any.</param>
    /// <param name="conditions">Applied conditions for the command, if any.</param>
    public CommandNode
    (
        IParentNode parent,
        string key,
        CommandInvocation invoke,
        CommandShape shape,
        IReadOnlyList<string>? aliases = null,
        IReadOnlyList<Attribute>? attributes = null,
        IReadOnlyList<ConditionAttribute>? conditions = null
    )
    {
        this.Invoke = invoke;
        this.Parent = parent;
        this.Key = key;
        this.Aliases = aliases ?? Array.Empty<string>();
        this.Shape = shape;
        this.Attributes = attributes ?? Array.Empty<Attribute>();
        this.Conditions = conditions ?? Array.Empty<ConditionAttribute>();
    }

    /// <summary>
    /// Attempts to bind the command shape to the given tokenizer, materializing values for its parameters.
    /// </summary>
    /// <param name="tokenizer">The token sequence.</param>
    /// <param name="boundCommandShape">The resulting shape, if any.</param>
    /// <param name="searchOptions">A set of search options.</param>
    /// <returns>true if the shape matches; otherwise, false.</returns>
    public bool TryBind
    (
        TokenizingEnumerator tokenizer,
        [NotNullWhen(true)] out BoundCommandNode? boundCommandShape,
        TreeSearchOptions? searchOptions = null
    )
    {
        searchOptions ??= new TreeSearchOptions();
        boundCommandShape = null;

        if (!tokenizer.MoveNext())
        {
            return false;
        }

        var parametersToCheck = new List<IParameterShape>(this.Shape.Parameters);

        var boundParameters = new List<BoundParameterShape>();
        while (parametersToCheck.Count > 0)
        {
            var matchedParameters = new List<IParameterShape>();
            foreach (var parameterToCheck in parametersToCheck)
            {
                if (!parameterToCheck.Matches(tokenizer, out var consumedTokens, searchOptions))
                {
                    continue;
                }

                // gobble up the tokens
                matchedParameters.Add(parameterToCheck);

                var boundTokens = new List<string>();
                for (ulong i = 0; i < consumedTokens; ++i)
                {
                    if (!tokenizer.MoveNext())
                    {
                        return false;
                    }

                    var type = tokenizer.Current.Type;
                    if (type != TokenType.Value)
                    {
                        // skip names, we've already checked them
                        continue;
                    }

                    var value = tokenizer.Current.Value.ToString();
                    boundTokens.Add(value);
                }

                boundParameters.Add(new BoundParameterShape(parameterToCheck, boundTokens));
            }

            if (matchedParameters.Count == 0)
            {
                // Check if all remaining parameters are optional
                if (!parametersToCheck.All(p => p.IsOmissible(searchOptions)))
                {
                    return false;
                }

                boundCommandShape = new BoundCommandNode(this, boundParameters);
                return true;
            }

            foreach (var matchedParameter in matchedParameters)
            {
                parametersToCheck.Remove(matchedParameter);
            }
        }

        // if there are more tokens to come, we don't match
        if (tokenizer.MoveNext())
        {
            return false;
        }

        boundCommandShape = new BoundCommandNode(this, boundParameters);
        return true;
    }

    /// <summary>
    /// Attempts to bind the command shape to the given named parameters, matching values to its parameters.
    /// </summary>
    /// <param name="namedParameters">The named parameters.</param>
    /// <param name="boundCommandShape">The resulting shape, if any.</param>
    /// <param name="searchOptions">A set of search options.</param>
    /// <returns>true if the shape matches; otherwise, false.</returns>
    public bool TryBind
    (
        IReadOnlyDictionary<string, IReadOnlyList<string>> namedParameters,
        [NotNullWhen(true)] out BoundCommandNode? boundCommandShape,
        TreeSearchOptions? searchOptions = null
    )
    {
        searchOptions ??= new TreeSearchOptions();
        boundCommandShape = null;

        using var enumerator = namedParameters.GetEnumerator();

        var parametersToCheck = new List<IParameterShape>(this.Shape.Parameters);

        var boundParameters = new List<BoundParameterShape>();
        while (parametersToCheck.Count > 0)
        {
            // The return value of MoveNext is ignored, because empty collections are allowed
            _ = enumerator.MoveNext();

            var matchedParameters = new List<IParameterShape>();
            foreach (var parameterToCheck in parametersToCheck)
            {
                // Because the current enumerator might be invalid or ended, we'll fix up the key-value pair here
                var current = enumerator.Current;
                if (current.Equals(default(KeyValuePair<string, IReadOnlyList<string>>)))
                {
                    current = new KeyValuePair<string, IReadOnlyList<string>>(string.Empty, Array.Empty<string>());
                }

                if (!parameterToCheck.Matches(current, out var isFatal, searchOptions))
                {
                    if (isFatal)
                    {
                        return false;
                    }

                    continue;
                }

                matchedParameters.Add(parameterToCheck);
                boundParameters.Add(new BoundParameterShape(parameterToCheck, current.Value ));
            }

            if (matchedParameters.Count == 0)
            {
                // Check if all remaining parameters are optional
                if (!parametersToCheck.All(p => p.IsOmissible(searchOptions)))
                {
                    return false;
                }

                boundCommandShape = new BoundCommandNode(this, boundParameters);
                return true;
            }

            foreach (var matchedParameter in matchedParameters)
            {
                parametersToCheck.Remove(matchedParameter);
            }
        }

        // if there are more tokens to come, we don't match
        if (enumerator.MoveNext())
        {
            return false;
        }

        boundCommandShape = new BoundCommandNode(this, boundParameters);
        return true;
    }
}
