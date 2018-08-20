using System;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.UnitTests.Model;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.FSharp.Core;

namespace Eu.EDelivery.AS4.UnitTests
{
    public class CustomProperty : PropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomProperty"/> class.
        /// </summary>
        public CustomProperty()
        {
            Arbitrary = new[] {typeof(Gens)};
        }
    }

    public static class Gens
    {
        public static Arbitrary<SignalMessage> Signals()
        {
            return Gen.Elements<SignalMessage>(
                          new Receipt($"ref-to-user-{Guid.NewGuid()}"), 
                          new FilledNRReceipt(), 
                          new Error(
                              $"error-{Guid.NewGuid()}", 
                              $"user-{Guid.NewGuid()}", 
                              ErrorLine.FromErrorResult(
                                  new ErrorResult($"desc-{Guid.NewGuid()}", ErrorAlias.Other))))
                      .ToArbitrary();
        }

        public static Arbitrary<Maybe<T>> MaybeArbitrary<T>()
        {
            return Arb.Default
                .Option<T>()
                .Generator
                .Select(x => 
                    Equals(x, FSharpOption<T>.None) 
                        ? Maybe<T>.Nothing 
                        : Maybe.Just(x.Value))
                .ToArbitrary();
        }
    }
}
