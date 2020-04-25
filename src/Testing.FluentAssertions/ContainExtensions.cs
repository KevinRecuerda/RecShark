using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Equivalency;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RecShark.Extensions.Testing.FluentAssertions
{
    public static class ContainExtensions
    {
        public static AndConstraint<GenericCollectionAssertions<T>> ContainEquivalentOf<T>(
            this GenericCollectionAssertions<T> assert,
            params T[] expectation)
        {
            return assert.ContainEquivalentOf<T>(expectation, "");
        }

        public static AndConstraint<GenericCollectionAssertions<T>> ContainEquivalentOf<T>(
            this GenericCollectionAssertions<T> assert,
            IEnumerable<T> expectation,
            string because = "",
            params object[] becauseArgs)
        {
            return assert.ContainEquivalentOf<T>(expectation, config => config, because, becauseArgs);
        }

        public static AndConstraint<GenericCollectionAssertions<T>> ContainEquivalentOf<T>(
            this GenericCollectionAssertions<T> assert,
            IEnumerable<T> expectation,
            Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> config,
            string because = "",
            params object[] becauseArgs)
        {
            return expectation.Aggregate(
                new AndConstraint<GenericCollectionAssertions<T>>(assert),
                (current, expected) => current.And.ContainEquivalentOf(expected, config, because, becauseArgs));
        }

        public static AndConstraint<GenericCollectionAssertions<T>> NotContainEquivalentOf<T>(
            this GenericCollectionAssertions<T> assert,
            params T[] expectation)
        {
            return assert.NotContainEquivalentOf(expectation, "");
        }

        public static AndConstraint<GenericCollectionAssertions<T>> NotContainEquivalentOf<T>(
            this GenericCollectionAssertions<T> assert,
            IEnumerable<T> expectation,
            string because = "",
            params object[] becauseArgs)
        {
            return assert.NotContainEquivalentOf(expectation, config => config, because, becauseArgs);
        }

        public static AndConstraint<GenericCollectionAssertions<T>> NotContainEquivalentOf<T>(
            this GenericCollectionAssertions<T> assert,
            IEnumerable<T> expectation,
            Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> config,
            string because = "",
            params object[] becauseArgs)
        {
            return expectation.Aggregate(
                new AndConstraint<GenericCollectionAssertions<T>>(assert),
                (current, expected) => current.And.NotContainEquivalentOf(expected, config, because, becauseArgs));
        }

        public static AndConstraint<GenericCollectionAssertions<T>> NotContainEquivalentOf<T>(
            this GenericCollectionAssertions<T> assert,
            T expectation,
            Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> config,
            string because = "",
            params object[] becauseArgs)
        {
            if (ReferenceEquals(assert.Subject, null))
            {
                return new AndConstraint<GenericCollectionAssertions<T>>(assert);
            }

            IEquivalencyAssertionOptions options = config(AssertionOptions.CloneDefaults<T>());
            IEnumerable<object> actualItems = assert.Subject.Cast<object>();

            using (var scope = new AssertionScope())
            {
                scope.AddReportable("configuration", options.ToString());

                foreach (var actualItem in actualItems)
                {
                    var context = new EquivalencyValidationContext
                    {
                        Subject = actualItem,
                        Expectation = expectation,
                        CompileTimeType = typeof(T),
                        Because = because,
                        BecauseArgs = becauseArgs,
                        Tracer = options.TraceWriter,
                    };

                    var equivalencyValidator = new EquivalencyValidator(options);
                    equivalencyValidator.AssertEquality(context);

                    var failures = scope.Discard();
                    if (!failures.Any())
                    {
                        Execute.Assertion
                            .BecauseOf(because, becauseArgs)
                            .FailWith("Expected {context:collection} {0} to not contain equivalent of {1}.", assert.Subject, expectation);
                        break;
                    }
                }
            }

            return new AndConstraint<GenericCollectionAssertions<T>>(assert);
        }
    }
}