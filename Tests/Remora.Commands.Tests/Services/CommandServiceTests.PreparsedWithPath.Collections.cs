//
//  CommandServiceTests.PreparsedWithPath.Collections.cs
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

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Commands.Services;
using Remora.Commands.Tests.Data.Modules;
using Xunit;

namespace Remora.Commands.Tests.Services;

public static partial class CommandServiceTests
{
    public static partial class PreparsedWithPath
    {
        /// <summary>
        /// Tests collection arguments.
        /// </summary>
        public class Collections
        {
            /// <summary>
            /// Tests whether the command service can execute a command with a positional collection.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecutePositionalCollectionCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<CollectionCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "values", ["ra", "ra", "rasputin"] }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    ["test", "positional-collection"],
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a named collection.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteNamedCollectionCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<CollectionCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "values", ["ra", "ra", "rasputin"] }
                };
                var executionResult = await commandService.TryExecuteAsync
                (
                    ["test", "named-collection"],
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a positional collection and named value.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteNamedValueAndPositionalCollectionCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<CollectionCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "values", ["ra", "rasputin"] },
                    { "named", ["ra"] }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    ["test", "positional-collection-and-named-value"],
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a positional collection and named value.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteOutOfOrderNamedValueAndPositionalCollectionCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<CollectionCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "named", ["ra"] },
                    { "values", ["ra", "rasputin"] }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    ["test", "positional-collection-and-named-value"],
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a min-constrained collection.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteCollectionWithMinCountCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<CollectionCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "values", ["ra"] }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    ["test", "collection-with-min-count"],
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);

                values = new Dictionary<string, IReadOnlyList<string>>();
                executionResult = await commandService.TryExecuteAsync
                (
                    ["test", "collection-with-min-count"],
                    values,
                    services
                );

                Assert.False(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a max-constrained collection.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteCollectionWithMaxCountCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<CollectionCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();
                var executionResult = await commandService.TryExecuteAsync
                (
                    ["test", "collection-with-max-count"],
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);

                values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "values", ["ra", "ra", "rasputin"] }
                };

                executionResult = await commandService.TryExecuteAsync
                (
                    ["test", "collection-with-max-count"],
                    values,
                    services
                );

                Assert.False(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a min and max-constrained collection.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteCollectionWithMinAndMaxCountCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<CollectionCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "values", ["ra"] }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    ["test", "collection-with-min-and-max-count"],
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);

                values = new Dictionary<string, IReadOnlyList<string>>();
                executionResult = await commandService.TryExecuteAsync
                (
                    ["test", "collection-with-min-and-max-count"],
                    values,
                    services
                );

                Assert.False(executionResult.IsSuccess);

                values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "values", ["ra", "ra", "rasputin"] }
                };

                executionResult = await commandService.TryExecuteAsync
                (
                    ["test", "collection-with-min-and-max-count"],
                    values,
                    services
                );

                Assert.False(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a max-constrained collection and a
            /// following positional argument.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteConstrainedCollectionWithPositionalValue()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<CollectionCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "values", ["ra", "ra"] },
                    { "value", ["rasputin"] }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    ["test", "constrained-collection-with-positional-value"],
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);

                values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "values", ["ra", "ra"] }
                };

                executionResult = await commandService.TryExecuteAsync
                (
                    ["test", "constrained-collection-with-positional-value"],
                    values,
                    services
                );

                Assert.False(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a positional array.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteArrayCollectionCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<CollectionCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "values", ["ra", "ra", "rasputin"] }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    ["test", "array-collection"],
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }

            /// <summary>
            /// Tests whether the command service can execute a command with a positional params array.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteParamsCollectionCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<CollectionCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>
                {
                    { "values", ["ra", "ra", "rasputin"] }
                };

                var executionResult = await commandService.TryExecuteAsync
                (
                    ["test", "params-collection"],
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }
        }
    }
}
