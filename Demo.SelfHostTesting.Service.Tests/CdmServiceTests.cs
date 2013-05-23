using System;
using FluentAssertions;
using NUnit.Framework;
using Nancy;
using Nancy.Hosting.Self;
using RestSharp;

namespace Demo.SelfHostTesting.Service.Tests
{
    [TestFixture]
    public class CdmServiceTests
    {
        private readonly NancyHost _host = new NancyHost(new Uri("http://localhost:1234"));

        [TestFixtureSetUp]
        public void Init()
        {
            _host.Start();
        }

        [Test]
        public void given_single_cdm_exists_when_providing_procedure_code_should_return_cdm()
        {
            var client = new RestClient {BaseUrl = "http://localhost:1234"};

            var request = new RestRequest("api/v/1/facilities/1234/cdms");
            var result = client.Get(request);

            result.Content.Should().Be("Hello World from Version:  1, Facility:  1234");
        }

        [TestFixtureTearDown]
        public void Dispose()
        {
            _host.Stop();
        }
    }

    public class HelloModule : NancyModule
    {
        public HelloModule()
        {
            Get["api/v/{version}/facilities/{facilityId}/cdms"] = parameters =>
                {
                    if (parameters.facilityId == "1234")
                        return string.Format("Hello World from Version:  {0}, Facility:  {1}", parameters.version,
                                             parameters.facilityId);
                    else
                        return HttpStatusCode.NotFound;
                };
        }
    }
}
