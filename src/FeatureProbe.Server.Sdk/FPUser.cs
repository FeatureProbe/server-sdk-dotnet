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

using System.Diagnostics;
using System.Text.Json.Serialization;

namespace FeatureProbe.Server.Sdk;

/// <summary>
///     A collection of attributes that can affect toggle evaluation, usually corresponding to a user of your application.
/// </summary>
public class FPUser
{
    /// <summary>
    ///     Creates a new FPUser, whose Key is current timestamp.
    /// </summary>
    public FPUser()
    {
        var timestamp = Stopwatch.GetTimestamp();
        var nanoTime = 1_000_000_000.0 * timestamp / Stopwatch.Frequency;
        Key = Convert.ToInt64(nanoTime).ToString();

        Attributes = new Dictionary<string, string>();
    }

    /// <summary>
    ///     FPUser's unique identifier.
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; private set; }

    /// <summary>
    ///     FPUser's all attributes.
    /// </summary>
    [JsonPropertyName("attributes")]
    public Dictionary<string, string> Attributes { get; set; }

    public string? this[string key]
    {
        get
        {
            Attributes.TryGetValue(key, out var value);
            return value;
        }
        set
        {
            if (value is not null) { Attributes[key] = value; }
        }
    }

    /// <summary>
    ///     Sets a unique id for the user for percentage rollout.
    /// </summary>
    /// <param name="key">user unique id for percentage rollout</param>
    /// <returns>this user</returns>
    public FPUser StableRollout(string key)
    {
        Key = key;
        return this;
    }

    /// <summary>
    ///     Adds an attribute to the user.
    /// </summary>
    /// <param name="name">attribute name</param>
    /// <param name="value">attribute value</param>
    /// <returns>this user</returns>
    public FPUser With(string name, string value)
    {
        Attributes[name] = value;
        return this;
    }

    /// <summary>
    ///     Checks whether the user has the attribute.
    /// </summary>
    /// <param name="name">attribute name</param>
    /// <returns>whether the attribute exists</returns>
    public bool ContainAttr(string name)
    {
        return Attributes.ContainsKey(name);
    }
}
