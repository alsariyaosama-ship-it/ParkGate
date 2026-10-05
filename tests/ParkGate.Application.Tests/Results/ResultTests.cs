using ParkGate.Application.Common;

namespace ParkGate.Application.Tests.Results;

public sealed class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = Result.Success();

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBe(Error.None);
    }

    [Fact]
    public void Failure_WithCustomError_ShouldCreateFailedResult()
    {
        var error = new Error("Custom.Code", "Something went wrong");
        var result = Result.Failure(error);

        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Custom.Code");
        result.Error.Message.ShouldBe("Something went wrong");
    }

    [Fact]
    public void Failure_WithCodeAndMessage_ShouldCreateFailedResult()
    {
        var result = Result.Failure("Custom.Code", "Something went wrong");

        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Custom.Code");
        result.Error.Message.ShouldBe("Something went wrong");
    }

    [Fact]
    public void TypedSuccess_ShouldProvideValue()
    {
        var result = Result.Success(42);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
        result.Error.ShouldBe(Error.None);
    }

    [Fact]
    public void TypedFailure_AccessingValue_ShouldThrowInvalidOperationException()
    {
        var result = Result.Failure<string>(Errors.General.NotFound);

        result.IsFailure.ShouldBeTrue();
        var ex = Should.Throw<InvalidOperationException>(() => _ = result.Value);
        ex.Message.ShouldContain(Errors.General.NotFound.Code);
    }

    [Fact]
    public void ImplicitConversions_ShouldWorkAsExpected()
    {
        Result<string> successResult = "hello";
        successResult.IsSuccess.ShouldBeTrue();
        successResult.Value.ShouldBe("hello");

        Result<string> failureResult = Errors.General.NotFound;
        failureResult.IsFailure.ShouldBeTrue();
        failureResult.Error.ShouldBe(Errors.General.NotFound);
    }

    [Fact]
    public void ValidationFailed_ShouldContainFieldErrors()
    {
        var fieldErrors = new List<ValidationError>
        {
            new("CardNumber", "Required", "Card number is required"),
            new("Phone", "Invalid", "Phone format is invalid")
        };

        var error = Errors.General.ValidationFailed(fieldErrors);

        error.Code.ShouldBe("General.ValidationFailed");
        error.ValidationErrors.ShouldNotBeNull();
        error.ValidationErrors.Count.ShouldBe(2);
        error.ValidationErrors[0].PropertyName.ShouldBe("CardNumber");
    }
}
