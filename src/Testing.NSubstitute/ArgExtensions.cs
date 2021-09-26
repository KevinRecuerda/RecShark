using System;
using System.Collections.Generic;
using FluentAssertions.Equivalency;
using NSubstitute;
using RecShark.Testing.FluentAssertions;

namespace RecShark.Testing.NSubstitute
{
    public static class ArgExtensions
    {
        // expected.AsArg(options => options.Excluding(x => x.Timestamp)
        public static T AsArg<T>(this T item, Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> config = null)
        {
            return Arg.Is<T>(x => x.IsEquivalentTo(item, config));
        }
        
        public static IEnumerable<T> AsArgEnumerable<T>(this IEnumerable<T> item, Func<EquivalencyAssertionOptions<IEnumerable<T>>, EquivalencyAssertionOptions<IEnumerable<T>>> config = null)
        {
            return item.AsArg(config);
        }
        
        public static ICollection<T> AsArgCollection<T>(this IEnumerable<T> item, Func<EquivalencyAssertionOptions<ICollection<T>>, EquivalencyAssertionOptions<ICollection<T>>> config = null)
        {
            return item.AsArgCast(config);
        }
        
        public static TCast AsArgCast<TCast>(this object item, Func<EquivalencyAssertionOptions<TCast>, EquivalencyAssertionOptions<TCast>> config = null)
        {
            return Arg.Is<TCast>(x => x.IsEquivalentTo((TCast)item, config));
        }
    }
}