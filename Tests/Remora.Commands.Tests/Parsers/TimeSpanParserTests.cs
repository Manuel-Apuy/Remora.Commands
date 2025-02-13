//
//  TimeSpanParserTests.cs
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
using System.Threading.Tasks;
using Remora.Commands.Parsers;
using Xunit;

namespace Remora.Commands.Tests.Parsers;

/// <summary>
/// Tests the <see cref="TimeSpanParser"/> class.
/// </summary>
public class TimeSpanParserTests
{
    /// <summary>
    /// Gets a set of various test cases.
    /// </summary>
    public static IEnumerable<object[]> Cases =>
    [
        ["0", new Func<TimeSpan>(() => TimeSpan.Zero)],
        ["1", new Func<TimeSpan>(() => TimeSpan.FromDays(1))],
        ["1.1:00", new Func<TimeSpan>(() => TimeSpan.FromDays(1) + TimeSpan.FromHours(1))],
        ["1:1:1", new Func<TimeSpan>(() => TimeSpan.FromHours(1) + TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(1))],
        ["1s", new Func<TimeSpan>(() => TimeSpan.FromSeconds(1))],
        ["1m", new Func<TimeSpan>(() => TimeSpan.FromMinutes(1))],
        ["1h", new Func<TimeSpan>(() => TimeSpan.FromHours(1))],
        ["1d", new Func<TimeSpan>(() => TimeSpan.FromDays(1))],
        ["1w", new Func<TimeSpan>(() => TimeSpan.FromDays(7))],
        [
            "1mo",
            new Func<TimeSpan>(() =>
            {
                var now = DateTimeOffset.UtcNow;
                var then = now.AddMonths(1);

                return then - now;
            })
        ],
        [
            "1y",
            new Func<TimeSpan>(() =>
            {
                var now = DateTimeOffset.UtcNow;
                var then = now.AddYears(1);

                return then - now;
            })
        ],
        [
            "1mo3d",
            new Func<TimeSpan>(() =>
            {
                var now = DateTimeOffset.UtcNow;
                var then = now.AddMonths(1);

                return then - now + TimeSpan.FromDays(3);
            })
        ],
        [
            "1mo1m",
            new Func<TimeSpan>(() =>
            {
                var now = DateTimeOffset.UtcNow;
                var then = now.AddMonths(1);

                return then - now + TimeSpan.FromMinutes(1);
            })
        ]
    ];

    /// <summary>
    /// Tests whether the parser can successfully parse various inputs.
    /// </summary>
    /// <param name="value">The value to parse.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [MemberData(nameof(Cases))]
    public async Task CanParse(string value, Func<TimeSpan> expected)
    {
        var parser = new TimeSpanParser();

        var result = await parser.TryParseAsync(value);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected(), result.Entity);
    }
}
