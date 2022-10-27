using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Common.Utilities;

internal static class UrlUtilities
{
    public static string Combine(params string[] paths)
    {
        return Combine(paths as IEnumerable<string>);
    }

    public static string Combine(params IEnumerable<string>[] paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentException.ThrowIfHasNullOrEmpty(paths);

        StringBuilder builder = new();
        foreach (var path in paths)
        {
            foreach (var subPath in path)
            {
                if (string.IsNullOrEmpty(subPath))
                {
                    builder.Append("/");
                }
                else
                {
                    builder.Append(subPath);
                    if (!subPath.EndsWith("/"))
                    {
                        builder.Append("/");
                    }
                }
            }
        }
        if (builder.Length == 0)
        {
            builder.Append("/");
        }
        return builder.ToString();
    }

    public static string Combine(string baseUrl, params IEnumerable<string>[] paths)
    {
        ArgumentNullException.ThrowIfNull(baseUrl);
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentException.ThrowIfEmpty(baseUrl);
        ArgumentException.ThrowIfHasNullOrEmpty(paths);

        StringBuilder builder = new();
        void append(string pathToAppend)
        {
            if (string.IsNullOrEmpty(pathToAppend))
            {
                builder.Append("/");
            }
            else
            {
                builder.Append(pathToAppend);
                if (!pathToAppend.EndsWith("/"))
                {
                    builder.Append("/");
                }
            }
        }
        append(baseUrl);
        foreach (var path in paths)
        {
            foreach (var subPath in path)
            {
                append(subPath);
            }
        }
        if (builder.Length == 0)
        {
            builder.Append("/");
        }
        return builder.ToString();
    }

    public static string[] Separate(string url)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentException.ThrowIfEmpty(url);

        var split = url.Split('/');
        if (!string.IsNullOrEmpty(split.Last())) return split;
        return split.Take(split.Length - 1).ToArray();
    }

    public static bool Compare(string url1, string url2)
    {
        ArgumentNullException.ThrowIfNull(url1);
        ArgumentNullException.ThrowIfNull(url2);
        ArgumentException.ThrowIfEmpty(url1);
        ArgumentException.ThrowIfEmpty(url2);

        url1 = url1.Trim().Trim('/');
        url2 = url2.Trim().Trim('/');
        if (url1.Length != url2.Length) return false;
        return url1 == url2;
    }

    public static bool IsBaseFrom(string baseUrl, string url)
    {
        ArgumentNullException.ThrowIfNull(baseUrl);
        ArgumentNullException.ThrowIfNull(url);
        ArgumentException.ThrowIfEmpty(baseUrl);
        ArgumentException.ThrowIfEmpty(url);

        baseUrl = baseUrl.Trim().Trim('/');
        url = url.Trim().Trim('/');
        return url.StartsWith(baseUrl);
    }
}
