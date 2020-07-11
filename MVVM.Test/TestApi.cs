using Microsoft.VisualStudio.TestTools.UnitTesting;
using mcpdipData;
using System.Collections.Generic;
using System.Net.Http;
using JsonLib;

namespace MCPDIP.Test
{
    [TestClass]
    public class TestApi
    {
        [TestMethod]
        public void TestGrievanceApi()
        {
            TestJsonGrievance result = null;
            using (var httpClient = new HttpClient())
            {
                using (var response = httpClient.GetAsync("http://localhost/MCPDIP.Api/api/Grievances/1"))
                {
                    var apiResponse = response.Result.Content.ReadAsStringAsync();
                    result = JsonDeserialize.DeserializeGrievances(apiResponse.Result);
                }
            }
            Assert.IsTrue(result != null);
        }
        [TestMethod]
        public void TestListOfGrievanceApi()
        {
            List<TestJsonGrievance> result = null;
            using (var httpClient = new HttpClient())
            {
                using (var response = httpClient.GetAsync("http://localhost/MCPDIP.Api/api/Grievances/2020/02/IEHP"))
                {
                    var apiResponse = response.Result.Content.ReadAsStringAsync();
                    result = JsonDeserialize.DeserializeListOfGrievances(apiResponse.Result);
                }
            }
            Assert.IsTrue(result.Count > 0);
        }
        [TestMethod]
        public void TestAppealApi()
        {
            TestJsonAppeal result = null;
            using (var httpClient = new HttpClient())
            {
                using (var response = httpClient.GetAsync("http://localhost/MCPDIP.Api/api/Appeals/1"))
                {
                    var apiResponse = response.Result.Content.ReadAsStringAsync();
                    result = JsonDeserialize.DeserializeAppeals(apiResponse.Result);
                }
            }
            Assert.IsTrue(result != null);
        }
        [TestMethod]
        public void TestListOfAppealApi()
        {
            List<TestJsonAppeal> result = null;
            using (var httpClient = new HttpClient())
            {
                using (var response = httpClient.GetAsync("http://localhost/MCPDIP.Api/api/Appeals/2020/02/IEHP"))
                {
                    var apiResponse = response.Result.Content.ReadAsStringAsync();
                    result = JsonDeserialize.DeserializeListOfAppeals(apiResponse.Result);
                }
            }
            Assert.IsTrue(result.Count > 0);
        }
        [TestMethod]
        public void TestCocApi()
        {
        }
        [TestMethod]
        public void TestOonApi()
        {
        }
        [TestMethod]
        public void TestPcpaApi()
        {
            TestJsonPcpa result = null;
            using (var httpClient = new HttpClient())
            {
                using (var response = httpClient.GetAsync("http://localhost/MCPDIP.Api/api/Pcpa/1"))
                {
                    var apiResponse = response.Result.Content.ReadAsStringAsync();
                    result = JsonDeserialize.DeserializePcpa(apiResponse.Result);
                }
            }
            Assert.IsTrue(result != null);
        }
        [TestMethod]
        public void TestListOfPcpaApi()
        {
            List<TestJsonPcpa> result = null;
            using (var httpClient = new HttpClient())
            {
                using (var response = httpClient.GetAsync("http://localhost/MCPDIP.Api/api/Pcpa/2020/02/IEHP"))
                {
                    var apiResponse = response.Result.Content.ReadAsStringAsync();
                    result = JsonDeserialize.DeserializeListOfPcpa(apiResponse.Result);
                }
            }
            Assert.IsTrue(result.Count > 0);
        }
    }
}

