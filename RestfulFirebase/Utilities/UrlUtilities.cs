using RestfulFirebase.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Utilities
{
    /// <summary>
    /// Provides convenient url utilities.
    /// </summary>
    public static class UrlUtilities
    {
        /// <summary>
        /// Combines url separated with the web url separator '/'.
        /// </summary>
        /// <param name="paths">
        /// The paths of the url.
        /// </param>
        /// <returns>
        /// The resulting url combined <paramref name="paths"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="paths"/> is a null reference.
        /// </exception>
        public static string Combine(params string[] paths)
        {
            return Combine(paths as IEnumerable<string>);
        }

        /// <summary>
        /// Combines url separated with the web url separator '/'.
        /// </summary>
        /// <param name="paths">
        /// The paths of the url.
        /// </param>
        /// <returns>
        /// The resulting url combined <paramref name="paths"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="paths"/> is a null reference.
        /// </exception>
        public static string Combine(params IEnumerable<string>[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }
            StringBuilder builder = new StringBuilder();
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

        /// <summary>
        /// Combines url separated with the web url separator '/'.
        /// </summary>
        /// <param name="baseUrl">
        /// The base path of the url.
        /// </param>
        /// <param name="paths">
        /// The paths of the url.
        /// </param>
        /// <returns>
        /// The resulting url combined <paramref name="paths"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="paths"/> is a null reference.
        /// </exception>
        public static string Combine(string baseUrl, params IEnumerable<string>[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }

            StringBuilder builder = new StringBuilder();
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

        /// <summary>
        /// Separates url to its ordered sub-paths.
        /// </summary>
        /// <param name="url">
        /// The url to separate.
        /// </param>
        /// <returns>
        /// The separated paths from the provided <paramref name="url"/> parameter.
        /// </returns>
        public static string[] Separate(string url)
        {
            var split = url.Split('/');
            if (!string.IsNullOrEmpty(split.Last())) return split;
            return split.Take(split.Length - 1).ToArray();
        }

        /// <summary>
        /// Checks urls if it leads to a same path.
        /// </summary>
        /// <param name="url1">
        /// The url to compare.
        /// </param>
        /// <param name="url2">
        /// The url to compare.
        /// </param>
        /// <returns>
        /// <c>true</c> if the provided urls are the same; otherwise <c>false</c>.
        /// </returns>
        public static bool Compare(string url1, string url2)
        {
            url1 = url1.Trim().Trim('/');
            url2 = url2.Trim().Trim('/');
            if (url1.Length != url2.Length) return false;
            return url1 == url2;
        }

        /// <summary>
        /// Checks if the provided base url is a sub url from the provided url.
        /// </summary>
        /// <param name="baseUrl">
        /// The base url to check.
        /// </param>
        /// <param name="url">
        /// The url to check.
        /// </param>
        /// <returns>
        /// <c>true</c> if the provided <paramref name="url"/> is base from <paramref name="baseUrl"/>; otherwise <c>false</c>.
        /// </returns>
        public static bool IsBaseFrom(string baseUrl, string url)
        {
            baseUrl = baseUrl.Trim().Trim('/');
            url = url.Trim().Trim('/');
            return url.StartsWith(baseUrl);
        }
    }
}
