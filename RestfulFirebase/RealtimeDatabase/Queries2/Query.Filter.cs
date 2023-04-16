using RestfulFirebase.Common.Abstractions;
using RestfulHelpers.Common;
using RestfulFirebase.RealtimeDatabase.References;
using System;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.RealtimeDatabase.Queries2;

public partial class FluentFilteredQuery<TQuery>
{
    internal TQuery FilterCore(string parameterName, Func<object?> valueFactory)
    {
        object query = new FilteredQuery(Reference, this, ct =>
        {
            HttpResponse<string> response = new();

            object? value = valueFactory();

            string parameter;

            if (value is string strValue)
            {
                parameter = $"\"{strValue}\"";
            }
            else if (value is double doubleValue)
            {
                parameter = doubleValue.ToString(CultureInfo.InvariantCulture);
            }
            else if (value is long longValue)
            {
                parameter = longValue.ToString();
            }
            else if (value is bool boolValue)
            {
                parameter = $"{boolValue.ToString().ToLower()}";
            }
            else
            {
                parameter = $"null";
            }

            response.Append($"{parameterName}={parameter}");

            return new(response);
        });

        return (TQuery)query;
    }
}
