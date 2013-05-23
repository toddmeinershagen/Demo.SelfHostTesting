using System;
using System.Collections.Generic;
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
        private const string Uri = "http://localhost:1234";
        private readonly NancyHost _host = new NancyHost(new Uri(Uri));
        private IRestClient _client;

        [TestFixtureSetUp]
        public void Init()
        {
            _host.Start();
            _client = new RestClient { BaseUrl = Uri };
        }

        [TestFixtureTearDown]
        public void Dispose()
        {
            _host.Stop();
        }

        [Test]
        public void given_facility_1234_when_providing_procedure_code_should_return_message_with_params()
        { 
            var request = new RestRequest("api/v/1/facilities/1234/cdms");
            var result = _client.Get(request);

            result.Content.Should().Be("Hello World from Version:  1, Facility:  1234");
        }

        [Test]
        public void given_facility_2345_when_providing_procedure_code_should_return_matching_cdm()
        {
            var request = new RestRequest("api/v/1/facilities/2345/cdms?ProcedureCodes[]=ABC123");
            var result = _client.Get(request);

            result.Content.Should().Be("[{\"ProcedureCode\":\"ABC123\",\"ChargeCode\":\"C123\",\"RevenueCode\":\"R1234\",\"PerUnitCharge\":1.25}]");
        }

        [Test]
        public void given_facility_does_not_exist_when_providing_procedure_code_should_return_404_status_code()
        {
            var request = new RestRequest("api/v/1/facilities/12/cdms");
            var result = _client.Get(request);

            result.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
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

                    if (parameters.facilityId == "2345")
                        return
                            Negotiate.WithModel(new List<ChargeDescriptionMaster>
                                {
                                    new ChargeDescriptionMaster
                                        {
                                            ChargeCode = "C123",
                                            PerUnitCharge = 1.25m,
                                            ProcedureCode = Context.Request.Query["ProcedureCodes[]"],
                                            RevenueCode = "R1234"
                                        }
                                });

                    return HttpStatusCode.NotFound;
                };
        }

        public class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public class ChargeDescriptionMaster
        {
            public string ProcedureCode { get; set; }
            public string ChargeCode { get; set; }
            public string RevenueCode { get; set; }
            public decimal PerUnitCharge { get; set; }
        }
    }
}
