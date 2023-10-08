/*
 * Copyright 2023 FeatureProbe
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

﻿using System.Collections.Immutable;
using FeatureProbe.Server.Sdk.Models;
using Moq;
using Moq.Protected;

namespace FeatureProbe.Server.Sdk.UT;

public class ToggleTest
{
    private readonly FPUser _user = new();

    [Fact]
    private void TestIfToggleIsDisabledServeDisabledVariation()
    {
        var toggle = new Toggle
        {
            Enabled = false,
            Variations = new List<object> { 0, 1 },
            DisabledServe = new Serve { Select = 0 },
            DefaultServe = new Serve { Select = 1 }
        };
        var result = toggle.Eval(_user, ImmutableDictionary<string, Toggle>.Empty,
            ImmutableDictionary<string, Segment>.Empty, null, 1);
        Assert.Equal(0, result.VariationIndex);
    }

    [Fact]
    private void TestIfToggleIsEnabledServeDefaultVariation()
    {
        var toggle = new Toggle
        {
            Enabled = true,
            Variations = new List<object> { 0, 1 },
            DisabledServe = new Serve { Select = 0 },
            DefaultServe = new Serve { Select = 1 }
        };
        var result = toggle.Eval(_user, ImmutableDictionary<string, Toggle>.Empty,
            ImmutableDictionary<string, Segment>.Empty, null, 1);
        Assert.Equal(1, result.VariationIndex);
    }

    [Fact]
    private void TestIfToggleIsEnabledServeDisabledVariation()
    {
        var toggle = new Toggle
        {
            Enabled = false,
            Variations = new List<object> { 0, 1 },
            DisabledServe = new Serve { Select = 1 },
            DefaultServe = new Serve { Select = 1 }
        };
        var result = toggle.Eval(_user, ImmutableDictionary<string, Toggle>.Empty,
            ImmutableDictionary<string, Segment>.Empty, null, 1);
        Assert.Equal(1, result.VariationIndex);
    }

    [Fact]
    private void TestIfToggleIsEnabledServeDefaultVariationWhenMeetPrerequisiteReturnsFalse()
    {
        var toggle = new Mock<Toggle>();
        toggle.Setup(t => t.Enabled).Returns(true);
        toggle.Setup(t => t.Variations).Returns(new List<object> { 0, 1 });
        toggle.Setup(t => t.DisabledServe).Returns(new Serve { Select = 0 });
        toggle.Setup(t => t.DefaultServe).Returns(new Serve { Select = 1 });

        toggle.Protected().Setup<bool>("MeetPrerequisite",
            _user, ImmutableDictionary<string, Toggle>.Empty,
            ImmutableDictionary<string, Segment>.Empty, 1).Returns(false);

        var result = toggle.Object.Eval(_user, ImmutableDictionary<string, Toggle>.Empty,
            ImmutableDictionary<string, Segment>.Empty, null, 1);
        Assert.Equal(0, result.VariationIndex);
    }

    [Fact]
    private void TestIfToggleIsEnabledServeDefaultVariationWhenMeetPrerequisiteReturnsTrue()
    {
        var toggle = new Mock<Toggle>();
        toggle.Setup(t => t.Enabled).Returns(true);
        toggle.Setup(t => t.Variations).Returns(new List<object> { 0, 1 });
        toggle.Setup(t => t.DisabledServe).Returns(new Serve { Select = 0 });
        toggle.Setup(t => t.DefaultServe).Returns(new Serve { Select = 1 });

        toggle.Protected().Setup<bool>("MeetPrerequisite",
            _user, ImmutableDictionary<string, Toggle>.Empty,
            ImmutableDictionary<string, Segment>.Empty, 1).Returns(true);

        var result = toggle.Object.Eval(_user, ImmutableDictionary<string, Toggle>.Empty,
            ImmutableDictionary<string, Segment>.Empty, null, 1);
        Assert.Equal(1, result.VariationIndex);
    }
}
