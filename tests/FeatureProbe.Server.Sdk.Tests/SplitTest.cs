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

using System.Text.Json;
using FeatureProbe.Server.Sdk.Models;

namespace FeatureProbe.Server.Sdk.UT;

public class SplitTest
{
    private readonly Split _split =
        JsonSerializer.Deserialize<Split>("{\"distribution\":[[[0,5000]], [[5000,10000]]]}")!;

    [Fact]
    private void TestGetUserGroup()
    {
        var user = new FPUser().StableRollout("test_user_key");
        var commonIndex = _split.FindIndex(user, "test_toggle_key");
        Assert.Equal(0, commonIndex.Index);

        _split.BucketBy = "email";
        _split.Salt = "abcddeafasde";
        user["email"] = "test@gmail.com";
        var customIndex = _split.FindIndex(user, "test_toggle_key");
        Assert.Equal(1, customIndex.Index);
    }

    [Fact]
    private void TestGetHashKey()
    {
        var hash = _split.Invoke<int>("Hash", "13", "tutorial_rollout", 10000);
        Assert.Equal(9558, hash);
    }

    [Fact]
    private void TestUserHasNoKey()
    {
        var user = new FPUser();

        var res1 = _split.FindIndex(user, "test_toggle_key");
        var key1 = user.Key;

        var res2 = _split.FindIndex(user, "test_toggle_key");
        var key2 = user.Key;

        Assert.Equal(key1, key2);
        Assert.Equal(res1.Index, res2.Index);
    }
}
