//
//  TreeRegistrationBuilder.cs
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
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Remora.Commands.Builders;
using Remora.Commands.Groups;
using Remora.Commands.Trees;

namespace Remora.Commands.DependencyInjection;

/// <summary>
/// Handles fluent building of command tree registrations.
/// </summary>
[PublicAPI]
public class TreeRegistrationBuilder
{
    private readonly string? _treeName;
    private readonly IServiceCollection _serviceCollection;

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeRegistrationBuilder"/> class.
    /// </summary>
    /// <param name="treeName">The name of the tree we're building for.</param>
    /// <param name="serviceCollection">The service collection.</param>
    public TreeRegistrationBuilder(string? treeName, IServiceCollection serviceCollection)
    {
        _treeName = treeName ?? Constants.DefaultTreeName;
        _serviceCollection = serviceCollection;
    }

    /// <summary>
    /// Adds the given command group to this tree.
    /// </summary>
    /// <typeparam name="TCommandGroup">The type of the command group.</typeparam>
    /// <returns>The builder, with the group added.</returns>
    public TreeRegistrationBuilder WithCommandGroup
    <
        [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        TCommandGroup
    >() where TCommandGroup : CommandGroup
        => WithCommandGroup(typeof(TCommandGroup));

    /// <summary>
    /// Adds the given command group to this tree.
    /// </summary>
    /// <param name="commandGroup">The type of the command group.</param>
    /// <returns>The builder, with the group added.</returns>
    public TreeRegistrationBuilder WithCommandGroup(Type commandGroup)
    {
        void AddGroupsScoped(Type groupType)
        {
            foreach (var nestedType in groupType.GetNestedTypes())
            {
                if (!nestedType.IsSubclassOf(typeof(CommandGroup)))
                {
                    continue;
                }

                _serviceCollection.TryAddScoped(nestedType);
                AddGroupsScoped(nestedType);
            }
        }

        if (!commandGroup.IsSubclassOf(typeof(CommandGroup)))
        {
            throw new ArgumentException(
                $"{nameof(commandGroup)} should inherit from {nameof(CommandGroup)}.",
                nameof(commandGroup));
        }

        _serviceCollection.Configure<CommandTreeBuilder>
        (
            _treeName,
            builder => builder.RegisterModule(commandGroup)
        );

        _serviceCollection.Configure<CommandTreeBuilder>
        (
            Constants.AllTreeName,
            builder => builder.RegisterModule(commandGroup)
        );

        _serviceCollection.TryAddScoped(commandGroup);
        AddGroupsScoped(commandGroup);

        return this;
    }

    /// <summary>
    /// Creates a new <see cref="GroupBuilder"/> to add to the command tree.
    /// </summary>
    /// <returns>The created builder.</returns>
    public GroupBuilder CreateCommandGroup()
    {
        var builder = new GroupBuilder(this);

        _serviceCollection.Configure<CommandTreeBuilder>
        (
            _treeName,
            b => b.RegisterBuilder(builder)
        );

        return builder;
    }

    /// <summary>
    /// Creates a new <see cref="GroupBuilder"/> to add to the command tree.
    /// </summary>
    /// <returns>The created builder.</returns>
    public CommandBuilder CreateCommand()
    {
        var builder = new CommandBuilder(this);

        _serviceCollection.Configure<CommandTreeBuilder>
        (
            _treeName,
            b => b.RegisterBuilder(builder)
        );

        return builder;
    }

    /// <summary>
    /// Finishes configuring the tree, returning the service collection.
    /// </summary>
    /// <returns>The service collection.</returns>
    public IServiceCollection Finish() => _serviceCollection;
}
