using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Common.Utilities;

internal class QueryBuilder : Dictionary<string, string>
{
    public string Build()
    {
        if (Count == 0)
        {
            return "";
        }

        StringBuilder sb = new();
        bool hasAdded = false;

        sb.Append("?");
        foreach (var item in this)
        {
            if (!hasAdded)
            {
                hasAdded = true;
            }
            else
            {
                sb.Append("&");
            }
            sb.Append($"{item.Key}={item.Value}");
        }

        return sb.ToString();
    }
}
