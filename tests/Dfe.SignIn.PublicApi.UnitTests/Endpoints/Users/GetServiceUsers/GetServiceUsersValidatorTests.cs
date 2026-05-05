using System;
using Dfe.SignIn.PublicApi.Endpoints.Users.GetServiceUsers;
using FluentValidation.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dfe.SignIn.PublicApi.UnitTests.Endpoints.Users.GetServiceUsers;

[TestClass]
public class GetServiceUsersValidatorTests
{
    private static GetServiceUsersQueryValidator Validator => new();

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void Page_LessThanOne_IsInvalid(int page)
    {
        var query = new GetServiceUsersQuery(Page: page);
        var result = Validator.Validate(query);
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Exists(e => e.PropertyName == "Page"));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void PageSize_LessThanOne_IsInvalid(int pageSize)
    {
        var query = new GetServiceUsersQuery(PageSize: pageSize);
        var result = Validator.Validate(query);
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Exists(e => e.PropertyName == "PageSize"));
    }

    [TestMethod]
    [DataRow(2)]
    [DataRow(-1)]
    public void Status_NotZeroOrOne_IsInvalid(int status)
    {
        var query = new GetServiceUsersQuery(Status: status);
        var result = Validator.Validate(query);
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Exists(e => e.PropertyName == "Status"));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    public void Status_ZeroOrOne_IsValid(int status)
    {
        var query = new GetServiceUsersQuery(Status: status);
        var result = Validator.Validate(query);
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void FromDate_GreaterThanToDate_IsInvalid()
    {
        var from = new DateTimeOffset(2023, 1, 5, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var query = new GetServiceUsersQuery(From: from, To: to);
        var result = Validator.Validate(query);
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Exists(e => e.ErrorMessage.Contains("From date greater than to date")));
    }

    [TestMethod]
    public void DateRange_Exceeds90Days_IsInvalid()
    {
        var from = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = from.AddDays(91);
        var query = new GetServiceUsersQuery(From: from, To: to);
        var result = Validator.Validate(query);
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Exists(e => e.ErrorMessage.Contains("Only 90 days are allowed")));
    }

    [TestMethod]
    public void DateRange_Exactly90Days_IsValid()
    {
        var from = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = from.AddDays(90);
        var query = new GetServiceUsersQuery(From: from, To: to);
        var result = Validator.Validate(query);
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void FromDate_InFuture_IsInvalid()
    {
        var from = DateTimeOffset.UtcNow.AddDays(1);
        var query = new GetServiceUsersQuery(From: from);
        var result = Validator.Validate(query);
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Exists(e => e.PropertyName == "From" || e.ErrorMessage.Contains("future")));
    }

    [TestMethod]
    public void ToDate_InFuture_IsInvalid()
    {
        var to = DateTimeOffset.UtcNow.AddDays(1);
        var query = new GetServiceUsersQuery(To: to);
        var result = Validator.Validate(query);
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Exists(e => e.PropertyName == "To" || e.ErrorMessage.Contains("future")));
    }

    [TestMethod]
    public void BothDates_InFuture_IsInvalid()
    {
        var from = DateTimeOffset.UtcNow.AddDays(2);
        var to = from.AddDays(1);
        var query = new GetServiceUsersQuery(From: from, To: to);
        var result = Validator.Validate(query);
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Exists(e => e.ErrorMessage.Contains("future")));
    }

    [TestMethod]
    public void FromDate_Only_Past_IsValid()
    {
        var from = DateTimeOffset.UtcNow.AddDays(-10);
        var query = new GetServiceUsersQuery(From: from);
        var result = Validator.Validate(query);
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void ToDate_Only_Past_IsValid()
    {
        var to = DateTimeOffset.UtcNow.AddDays(-1);
        var query = new GetServiceUsersQuery(To: to);
        var result = Validator.Validate(query);
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void BothDates_Past_ValidRange_IsValid()
    {
        var from = DateTimeOffset.UtcNow.AddDays(-10);
        var to = DateTimeOffset.UtcNow.AddDays(-5);
        var query = new GetServiceUsersQuery(From: from, To: to);
        var result = Validator.Validate(query);
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void NoDates_IsValid()
    {
        var query = new GetServiceUsersQuery();
        var result = Validator.Validate(query);
        Assert.IsTrue(result.IsValid);
    }
}
