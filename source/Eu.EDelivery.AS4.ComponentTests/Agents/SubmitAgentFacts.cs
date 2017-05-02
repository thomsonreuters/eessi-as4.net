using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{

    public class SubmitAgentFacts : ComponentTestTemplate
    {

        private readonly AS4Component _as4Msh = new AS4Component();
        private readonly HttpClient _httpClient = new HttpClient();

        // It would be nice if this could be extracted from the configuration.
        private static readonly string HttpSubmitAgentUrl = "http://localhost:7070/msh/";

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitAgentFacts"/> class.
        /// </summary>
        public SubmitAgentFacts()
        {
            OverrideSettings(@".\config\componenttest-settings\submitagent_http_settings.xml");
            _as4Msh.Start();
        }

        public class GivenValidSubmitMessage : SubmitAgentFacts
        {
            [Fact]
            public async void ThenAgentRespondsWithHttpAccepted()
            {
                var request = CreateRequestMessage(HttpSubmitAgentUrl, HttpMethod.Post, GetValidSubmitMessage());

                using (var response = await _httpClient.SendAsync(request))
                {
                    Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
                    Assert.True(String.IsNullOrWhiteSpace(response.Content.Headers.ContentType?.ToString()));
                }
            }

            [Fact]
            public async void ThenAgentRespondsWithErrorWhenSubmitFails()
            {
                const string submitMessageFile = @".\samples\messages\01-sample-message.xml";

                Assert.True(File.Exists(submitMessageFile), "The SubmitMessage could not be found.");

                // Wait a little bit to make sure we do not delete the DB to early; otherwise it is recreated.
                await Task.Delay(1000);
                File.Delete(@".\database\messages.db");

                var request = CreateRequestMessage(HttpSubmitAgentUrl, HttpMethod.Post, File.ReadAllText(submitMessageFile));

                using (var response = await _httpClient.SendAsync(request))
                {
                    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                    Assert.False(String.IsNullOrWhiteSpace(await response.Content.ReadAsStringAsync()));
                }
            }

            private static string GetValidSubmitMessage()
            {
                return @"<?xml version=""1.0""?>
                            <SubmitMessage xmlns = ""urn:cef:edelivery:eu:as4:messages""> 
                                <Collaboration> 
                                    <AgreementRef>
                                        <PModeId>componentsubmittest-pmode</PModeId> 
                                    </AgreementRef> 
                                </Collaboration> 
                                <Payloads/>   
                            </SubmitMessage>";
            }
        }

        public class GivenInvalidSubmitMessage : SubmitAgentFacts
        {
            [Fact]
            public async void ThenAgentRespondsWithHttpBadRequest()
            {
                var request = CreateRequestMessage(HttpSubmitAgentUrl, HttpMethod.Post, "");

                using (var response = await _httpClient.SendAsync(request))
                {
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                }
            }
        }

        private static HttpRequestMessage CreateRequestMessage(string url, HttpMethod method, string requestContent)
        {
            var request = new HttpRequestMessage(method, url);

            request.Content = new StringContent(requestContent);

            return request;
        }

        protected override void Disposing(bool isDisposing)
        {
            _as4Msh.Dispose();
        }

    }
}
