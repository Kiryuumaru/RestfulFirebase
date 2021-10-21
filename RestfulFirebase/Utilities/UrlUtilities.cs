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
        public static string Combine(params string[] paths)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(path)) builder.Append("/");
                else if (path.EndsWith("/")) builder.Append(path);
                else builder.Append(path + "/");
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
        /// <param name="basePath">
        /// The base path of the url.
        /// </param>
        /// <param name="paths">
        /// The paths of the url.
        /// </param>
        /// <returns>
        /// The resulting url combined <paramref name="paths"/>.
        /// </returns>
        public static string Combine(string basePath, params string[] paths)
        {
            StringBuilder builder = new StringBuilder();
            if (string.IsNullOrEmpty(basePath)) builder.Append("/");
            else if (basePath.EndsWith("/")) builder.Append(basePath);
            else builder.Append(basePath + "/");
            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(path)) builder.Append("/");
                else if (path.EndsWith("/")) builder.Append(path);
                else builder.Append(path + "/");
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
        /// <param name="paths">
        /// The paths of the url.
        /// </param>
        /// <returns>
        /// The resulting url combined <paramref name="paths"/>.
        /// </returns>
        public static string Combine(params IEnumerable<string>[] paths)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var path in paths)
            {
                foreach (var subPath in path)
                {
                    if (string.IsNullOrEmpty(subPath)) builder.Append("/");
                    else if (subPath.EndsWith("/")) builder.Append(subPath);
                    else builder.Append(subPath + "/");
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
