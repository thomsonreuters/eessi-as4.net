using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Mappings.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.UnitTests.Model;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.FSharp.Core;
using Org.BouncyCastle.Crypto.Engines;

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

    public class NonWhiteSpaceString
    {
        internal NonWhiteSpaceString(NonEmptyString str)
        {
            Get = str.Get.Replace(" ", String.Empty);
        }

        public string Get { get; }
    }

    public static class Gens
    {
        public static Gen<T> Or<T>(this Gen<T> g1, Gen<T> g2)
        {
            return Gen.OneOf(g1, g2);
        }

        public static Gen<T> OrNull<T>(this Gen<T> g1) where T : class
        {
            return Gen.OneOf(g1, Gen.Constant((T) null));
        }

        public static Gen<T3> Zip<T1, T2, T3>(this Gen<T1> g1, Gen<T2> g2, Func<T1, T2, T3> f)
        {
            return Gen.zip(g1, g2).Select(t => f(t.Item1, t.Item2));
        }

        public static Gen<Tuple<T1, T2>> Zip<T1, T2>(this Gen<T1> g1, Gen<T2> g2)
        {
            return Gen.zip(g1, g2);
        }

        public static Arbitrary<NonWhiteSpaceString> NonWhiteSpaceString()
        {
            return Arb.Generate<NonEmptyString>()
                      .Select(str => new NonWhiteSpaceString(str))
                      .ToArbitrary();
        }

        public static Arbitrary<MessageUnit> MessageUnits()
        {
            return Gen.Elements<MessageUnit>(
                new UserMessage($"user-{Guid.NewGuid()}"),
                new Receipt(
                    $"receipt-{Guid.NewGuid()}", 
                    $"ref-to-user-{Guid.NewGuid()}"),
                new FilledNRReceipt(), 
                new Error(
                    $"error-{Guid.NewGuid()}", 
                    $"user-{Guid.NewGuid()}", 
                    AS4.Model.Core.ErrorLine.FromErrorResult(
                        new ErrorResult($"desc-{Guid.NewGuid()}", ErrorAlias.Other))))
                      .ToArbitrary();
        }

        public static Arbitrary<SignalMessage> SignalMessages()
        {
            return MessageUnits()
                   .Generator
                   .Where(u => u is SignalMessage)
                   .Select(u => u as SignalMessage)
                   .ToArbitrary();
        }

        public static Arbitrary<Receipt> Receipt()
        {
            return Gen.zip3(Arb.Generate<NonEmptyString>().Two(), GenNonRepudiation(), UserMessage().Generator)
                      .Select(t => new Receipt(
                          t.Item1.Item1.Get, 
                          t.Item1.Item2.Get, 
                          t.Item2, 
                          UserMessageMap.ConvertToRouting(t.Item3)))
                      .ToArbitrary();
        }

        private static Gen<NonRepudiationInformation> GenNonRepudiation()
        {
            var genUri = Arb.Generate<NonNull<string>>();
            var genDigestValue = Arb.Generate<byte[]>();

            var genDigestMethod =
                Arb.Generate<NonNull<string>>()
                   .Select(x => new ReferenceDigestMethod(x.Get));

            var genTransforms =
                Arb.Generate<NonNull<string>>()
                   .Select(x => new ReferenceTransform(x.Get))
                   .ListOf();

            return Gen.zip3(genUri, genTransforms, genDigestMethod.Zip(genDigestValue))
                      .Select(t => new Reference(t.Item1.Get, t.Item2, t.Item3.Item1, t.Item3.Item2))
                      .ListOf()
                      .Select(rs => new NonRepudiationInformation(rs));
        }

        public static Arbitrary<Error> Error()
        {
            return Gen.zip3(
                Arb.Generate<NonEmptyString>().Two(),
                UserMessage().Generator,
                ErrorLine().ListOf())
                      .Select(t => new Error(
                          t.Item1.Item1.Get, 
                          t.Item1.Item2.Get, 
                          DateTimeOffset.Now, 
                          t.Item3, 
                          UserMessageMap.ConvertToRouting(t.Item2)))
                      .ToArbitrary();
        }

        private static Gen<ErrorLine> ErrorLine()
        {
            return Gen.zip3(
                MaybeArbitrary<NonNull<string>>()
                    .Generator
                    .Four(),
                Gen.zip3(Arb.Generate<Severity>(), Arb.Generate<ErrorCode>(), Arb.Generate<ErrorAlias>()),
                MaybeArbitrary<Tuple<NonNull<string>, NonNull<string>>>().Generator)
                      .Select(t => new ErrorLine(
                          t.Item2.Item2,
                          t.Item2.Item1,
                          t.Item2.Item3,
                          t.Item1.Item1.Select(m => m.Get),
                          t.Item1.Item2.Select(m => m.Get),
                          t.Item1.Item3.Select(m => m.Get),
                          t.Item3.Select(m => new ErrorDescription(m.Item1.Get, m.Item2.Get)),
                          t.Item1.Item4.Select(m => m.Get)));
        }

        public static Arbitrary<UserMessage> UserMessage()
        {
            return Gen.zip3(
                Arb.Generate<NonEmptyString>().Zip(GenCollaborationInfo()),
                GenParty().Two(),
                GenPartInfos().Zip(GenMessageProperties()))
                      .Select(t => new UserMessage(
                          t.Item1.Item1.Get,
                          t.Item1.Item2,
                          t.Item2.Item1,
                          t.Item2.Item2,
                          t.Item3.Item1,
                          t.Item3.Item2))
                      .ToArbitrary();
        }

        private static Gen<CollaborationInfo> GenCollaborationInfo()
        {
            var genAgreementRef =
                Arb.Generate<NonNull<string>>()
                   .Select(x => new AgreementReference(x.Get).AsMaybe())
                   .Or(Gen.Constant(Maybe<AgreementReference>.Nothing));

            var genServiceWithoutType =
                Arb.Generate<NonNull<string>>()
                   .Select(x => new Service(x.Get));

            var genServiceWithType =
                Arb.Generate<NonNull<string>>()
                   .Two()
                   .Select(t => new Service(t.Item1.Get, t.Item2.Get));

            var genActionConversation =
                Arb.Generate<NonNull<string>>()
                   .Two();

            return genAgreementRef
                .Zip(genServiceWithoutType.Or(genServiceWithType))
                .Zip(genActionConversation)
                .Select(tt => new CollaborationInfo(
                    tt.Item1.Item1,
                    tt.Item1.Item2,
                    tt.Item2.Item1.Get,
                    tt.Item2.Item2.Get));
        }

        private static Gen<Party> GenParty()
        {
            return Arb.Generate<NonEmptyString>()
                      .Zip(Arb.Generate<NonEmptyString>()
                              .NonEmptyListOf(),
                           (role, ids) => new Party(
                               role.Get,
                               ids.Select(id => new PartyId(id.Get))));
        }

        private static Gen<PartInfo[]> GenPartInfos()
        {
            var genSchemas =
                Arb.Generate<NonNull<string>>()
                   .Zip(Gens.MaybeArbitrary<NonNull<string>>().Generator.Select(t => t.Select(m => m.Get)))
                   .Zip(Gens.MaybeArbitrary<NonNull<string>>().Generator.Select(t => t.Select(m => m.Get)))
                   .Select(t => new Schema(t.Item1.Item1.Get, t.Item1.Item2, t.Item2))
                   .ListOf();

            return Arb.Generate<NonNull<string>>()
                      .Zip(Arb.Generate<IDictionary<string, string>>())
                      .Zip(genSchemas)
                      .Select(t => new PartInfo(t.Item1.Item1.Get, t.Item1.Item2, t.Item2))
                      .ArrayOf();
        }

        private static Gen<MessageProperty[]> GenMessageProperties()
        {
            var genPropWithType =
                Arb.Generate<NonEmptyString>()
                   .Three()
                   .Select(kv => new MessageProperty(kv.Item1.Get, kv.Item2.Get, kv.Item3.Get))
                   .ArrayOf();

            var genPropWithoutType =
                Arb.Generate<NonEmptyString>()
                   .Two()
                   .Select(kv => new MessageProperty(kv.Item1.Get, kv.Item2.Get))
                   .ArrayOf();

            return genPropWithoutType.Or(genPropWithType);
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
