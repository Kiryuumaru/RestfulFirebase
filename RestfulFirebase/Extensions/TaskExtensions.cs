using System;
using System.Threading.Tasks;

namespace RestfulFirebase.Extensions
{
    public static class TaskExtensions
    {
        public static async Task WithAggregateException(this Task source)
        {
            try
            {
                await source.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw source.Exception ?? ex;
            }
        }
    }
}
