using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace middlerApp.Auth.ExtensionMethods
{
    public static class Extensions
    {
        internal static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IQueryable<T> source, CancellationToken cancellationToken)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return ExecuteAsync(source, cancellationToken);

            static async IAsyncEnumerable<T> ExecuteAsync(IQueryable<T> source, [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
                {
                    yield return element;
                }
            }
        }
    }
}
