using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Model.PMode;
using FsCheck;

namespace Eu.EDelivery.AS4.UnitTests.Validators
{
    public static class ValidationInputGenerators
    {
        public static Gen<Tuple<PrivateKeyCertificateChoiceType, object>> CreatePrivateKeyCertificateGen(
            string findValue,
            string certificate,
            string password)
        {
            var genCertFindCriteria = Gen.OneOf(
                Gen.Constant(Tuple.Create(
                    PrivateKeyCertificateChoiceType.CertificateFindCriteria,
                    (object) null)),
                Gen.Fresh(() => Tuple.Create(
                    PrivateKeyCertificateChoiceType.CertificateFindCriteria,
                    (object) new CertificateFindCriteria { CertificateFindValue = findValue })));

            var genPrivateKeyCert = Gen.OneOf(
                Gen.Constant(Tuple.Create(
                    PrivateKeyCertificateChoiceType.PrivateKeyCertificate,
                    (object) null)),
                Gen.Fresh(() => Tuple.Create(
                    PrivateKeyCertificateChoiceType.PrivateKeyCertificate,
                    (object) new PrivateKeyCertificate { Certificate = certificate, Password = password })));

            return Gen.OneOf(genCertFindCriteria, genPrivateKeyCert);
        }

        public static Gen<Method> CreateMethodGen()
        {
            var parameterGen =
                Arb.Generate<string>()
                   .Two()
                   .SelectMany(t => Gen.OneOf(
                       Gen.Constant((Parameter)null),
                       Gen.Constant(new Parameter { Name = t.Item1, Value = t.Item2 })));

            var parametersGen =
                Arb.Generate<string>()
                   .Two()
                   .ListOf()
                   .SelectMany(xs => Gen.OneOf(
                       Gen.Constant((IList<Parameter>)null),
                       parameterGen.ListOf()));

            return Gen.OneOf(
                Gen.Constant((Method)null),
                Arb.Generate<string>()
                   .SelectMany(s => parametersGen.Select(
                       p => new Method { Type = s, Parameters = p?.ToList() })));
        }
    }
}
