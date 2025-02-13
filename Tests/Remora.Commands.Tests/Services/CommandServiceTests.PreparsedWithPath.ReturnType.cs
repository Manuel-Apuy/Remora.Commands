//
//  CommandServiceTests.PreparsedWithPath.ReturnType.cs
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
        /// Tests nonstandard return types.
        /// </summary>
        public class ReturnType
        {
            /// <summary>
            /// Tests whether a method that returns a ValueTask{IResult} can be executed.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            [Fact]
            public async Task CanExecuteValueTaskCommand()
            {
                var services = new ServiceCollection()
                    .AddCommands()
                    .AddCommandTree()
                    .WithCommandGroup<ReturnTypeCommandGroup>()
                    .Finish()
                    .BuildServiceProvider(true)
                    .CreateScope().ServiceProvider;

                var commandService = services.GetRequiredService<CommandService>();

                var values = new Dictionary<string, IReadOnlyList<string>>();
                var executionResult = await commandService.TryExecuteAsync
                (
                    ["a"],
                    values,
                    services
                );

                Assert.True(executionResult.IsSuccess);
            }
        }
    }
}
