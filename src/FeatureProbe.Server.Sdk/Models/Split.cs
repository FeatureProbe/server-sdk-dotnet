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

using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using FeatureProbe.Server.Sdk.Results;

namespace FeatureProbe.Server.Sdk.Models;

public class Split
{
    private const int BucketSize = 10000;

    [JsonPropertyName("distribution")] public List<List<List<int>>> Distribution { get; set; }

    [JsonPropertyName("bucketBy")] public string BucketBy { get; set; }

    [JsonPropertyName("salt")] public string Salt { get; set; }

    public HitResult FindIndex(FPUser user, string toggleKey)
    {
        var hashKey = user.Key;
        if (!string.IsNullOrWhiteSpace(BucketBy))
        {
            if (user.ContainAttr(BucketBy))
            {
                hashKey = user[BucketBy];
            }
            else
            {
                return new HitResult(
                    false,
                    Reason: $"Warning: User with key {user.Key} does not have attribute name {BucketBy}"
                );
            }
        }

        var groupIndex = GetGroup(Hash(hashKey, GetHashSalt(toggleKey), BucketSize));
        return new HitResult(
            true,
            groupIndex,
            $"Selected {groupIndex} percentage group"
        );
    }

    private int? GetGroup(int hashValue)
    {
        for (var i = 0; i < Distribution.Count; i++)
        {
            var groups = Distribution[i];
            if (groups.Any(range => hashValue >= range[0] && hashValue < range[1]))
            {
                return i;
            }
        }

        return null;
    }

    private int Hash(string hashKey, string hashSalt, int bucketSize)
    {
        var value = hashKey + hashSalt;

        // modifications are made to adapt low .NET versions
        // var hashValue = SHA1.HashData(Encoding.UTF8.GetBytes(value));
        byte[] hashValue;
        using (var sha1 = SHA1.Create())
        {
            hashValue = sha1.ComputeHash(Encoding.UTF8.GetBytes(value));
        }

        var bytes = new byte[5];
        Array.Copy(hashValue, hashValue.Length - 4, bytes, 1, 4);
        Array.Reverse(bytes);
        bytes[4] = 0;
        var res = new BigInteger(bytes);
        // created from a byte[] is unsigned if you append a 00 byte to the end of the array before calling the ctor
        // isUnsigned and isBigEndian flags are not supported in low .NET versions
        return (int)(res % bucketSize);
    }

    private string GetHashSalt(string toggleKey)
    {
        return !string.IsNullOrWhiteSpace(Salt) ? Salt : toggleKey;
    }
}
