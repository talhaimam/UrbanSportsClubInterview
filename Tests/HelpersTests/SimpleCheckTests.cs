using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using FitogramMQ;
using InterviewService.Helpers;
using InterviewService.Jobs.FitogramMQConsumers;
using InterviewService.Jobs.FitogramMQPublishers;
using InterviewService.Models;
using InterviewService.Models.External;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;
using Xunit;

namespace Tests.JobsTests.FitogramMQConsumersTests
{
    public class SimpleCheckTests
    {
        [Fact]
        public void TestSimpleCheck()
        {
            // [Given]
            var simpleCheck = new SimpleCheck();

            // [When]
            var result = simpleCheck.Check(true);

            // [Then]
            result.ShouldBe("YES");
        }
    }
}