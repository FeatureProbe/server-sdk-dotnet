using System.Text.Json;
using FeatureProbe.Server.Sdk.Models;

namespace FeatureProbe.Server.Sdk.UT;

public class SplitTest
{
    private readonly Split _split =
        JsonSerializer.Deserialize<Split>("{\"distribution\":[[[0,5000]], [[5000,10000]]]}")!;

    [Fact]
    void TestGetUserGroup()
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
    void TestGetHashKey()
    {
        var hash = _split.Invoke<int>("Hash", "13", "tutorial_rollout", 10000);
        Assert.Equal(9558, hash);
    }

    [Fact]
    void TestUserHasNoKey()
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
