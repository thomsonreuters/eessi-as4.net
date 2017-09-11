using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using Eu.EDelivery.AS4.UnitTests.Utilities;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Xml
{
    public class XsdValidationFacts : IClassFixture<XsdValidationFixture>
    {
        public class SamplesXsdValidationFacts : XsdValidationFacts
        {
            [Fact]
            public void SendingPModesValidateAgainstXsd()
            {
                var samplePModes = Computer.GetFilesInDirectory(".\\samples\\pmodes", "*send-pmode.xml", true);

                foreach (var sendingPMode in samplePModes)
                {
                    var violations = VerifySendingPMode(sendingPMode);

                    Assert.False(violations.Any(), $"Sending PMode {sendingPMode} invalid: {String.Join(Environment.NewLine, violations)}");
                }
            }

            [Fact]
            public void ReceivingPModesValidateAgainstXsd()
            {
                var samplePModes = Computer.GetFilesInDirectory(".\\samples\\pmodes", "*receive-pmode.xml", true);

                foreach (var receivingPMode in samplePModes)
                {
                    var violations = VerifyReceivingPMode(receivingPMode);

                    Assert.False(violations.Any(), $"Receiving PMode {receivingPMode} invalid: {String.Join(Environment.NewLine, violations)}");
                }
            }

            [Fact]
            public void SubmitMessagesValidateAgainstXsd()
            {
                var sampleSubmitMessages = Computer.GetFilesInDirectory(".\\samples\\messages", "*.xml", true);

                foreach (var submitMessage in sampleSubmitMessages)
                {
                    var violations = VerifyReceivingPMode(submitMessage);

                    Assert.False(violations.Any(), $"Submit Message {submitMessage} invalid: {String.Join(Environment.NewLine, violations)}");
                }
            }
        }

        public class ConformanceTestXsdValidationFacts : XsdValidationFacts
        {
            [Fact]
            public void SendingPModesValidateAgainstXsd()
            {
                var pmodes = Computer.GetFilesInDirectory(".\\config\\conformance-settings\\conftesting-pmodes\\C2\\send-pmodes", "*.xml", true).ToList();
                pmodes.AddRange(Computer.GetFilesInDirectory(".\\config\\conformance-settings\\conftesting-pmodes\\C3\\send-pmodes", "*.xml", true));

                foreach (var sendingPMode in pmodes)
                {
                    var violations = VerifySendingPMode(sendingPMode);

                    Assert.False(violations.Any(), $"Sending PMode {sendingPMode} invalid: {String.Join(Environment.NewLine, violations)}");
                }
            }
        }

        public class EessiConformanceXsdValidationFacts : XsdValidationFacts
        {
            [Fact]
            public void SendingPModesValidateAgainstXsd()
            {
                var pmodes = Computer.GetFilesInDirectory(".\\config\\eessi-conformancetest-settings\\C2\\send-pmodes", "*.xml", true).ToList();
                pmodes.AddRange(Computer.GetFilesInDirectory(".\\config\\eessi-conformancetest-settings\\C3\\send-pmodes", "*.xml", true));

                foreach (var sendingPMode in pmodes)
                {
                    var violations = VerifySendingPMode(sendingPMode);

                    Assert.False(violations.Any(), $"Sending PMode {sendingPMode} invalid: {String.Join(Environment.NewLine, violations)}");
                }
            }

            [Fact]
            public void ReceivingPModesValidateAgainstXsd()
            {
                var pmodes = Computer.GetFilesInDirectory(".\\config\\eessi-conformancetest-settings\\C2\\receive-pmodes", "*.xml", true).ToList();
                pmodes.AddRange(Computer.GetFilesInDirectory(".\\config\\eessi-conformancetest-settings\\C3\\receive-pmodes", "*.xml", true));

                foreach (var receivingPModes in pmodes)
                {
                    var violations = VerifyReceivingPMode(receivingPModes);

                    Assert.False(violations.Any(), $"Sending PMode {receivingPModes} invalid: {String.Join(Environment.NewLine, violations)}");
                }
            }
        }

        protected IEnumerable<string> VerifySendingPMode(string sendingPModeFile)
        {
            using (var schemaReader = XmlReader.Create(".\\doc\\schemas\\send-pmode-schema.xsd"))
            {
                var schemaSet = new XmlSchemaSet();
                schemaSet.Add(Constants.Namespaces.ProcessingMode, schemaReader);

                using (var xmlStream = File.OpenRead(sendingPModeFile))
                {
                    return GetXsdViolations(xmlStream, schemaSet);
                }
            }
        }

        protected IEnumerable<string> VerifyReceivingPMode(string receivingPModeFile)
        {
            using (var schemaReader = XmlReader.Create(".\\doc\\schemas\\receive-pmode-schema.xsd"))
            {
                var schemaSet = new XmlSchemaSet();
                schemaSet.Add(Constants.Namespaces.ProcessingMode, schemaReader);

                using (var xmlStream = File.OpenRead(receivingPModeFile))
                {
                    return GetXsdViolations(xmlStream, schemaSet);
                }
            }
        }

        protected IEnumerable<string> VerifySubmitMessage(string submitMessage)
        {
            using (var schemaReader = XmlReader.Create(".\\doc\\schemas\\submitmessage-schema.xsd"))
            {
                var schemaSet = new XmlSchemaSet();
                schemaSet.Add("cef:edelivery:eu:as4:messages", schemaReader);

                using (var xmlStream = File.OpenRead(submitMessage))
                {
                    return GetXsdViolations(xmlStream, schemaSet);
                }
            }
        }

        private static IEnumerable<string> GetXsdViolations(Stream xmlStream, XmlSchemaSet xsdSchemaSet)
        {
            var settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema
            };

            settings.Schemas = xsdSchemaSet;

            List<string> validationErrors = new List<string>();

            settings.ValidationEventHandler += delegate (object sender, ValidationEventArgs args)
            {
                validationErrors.Add(args.Message);
            };

            using (XmlReader reader = XmlReader.Create(xmlStream, settings))
            {
                while (reader.Read())
                {
                    // Read the stream and validate while reading.
                }
            }

            return validationErrors;
        }
    }

    public class XsdValidationFixture
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XsdValidationFixture"/> class.
        /// </summary>
        public XsdValidationFixture()
        {
            Computer.RunPowershellScript("&..\\scripts\\GenerateXsd.ps1 -binDirectory . -outputDirectory ./doc/schemas");
            // Pragmatic way to make sure that the XSD's are ready
            System.Threading.Thread.Sleep(2500);
        }
    }
}
