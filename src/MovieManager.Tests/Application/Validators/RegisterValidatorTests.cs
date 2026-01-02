using FluentAssertions;
using MovieManager.Application.DTOs.Auth;
using MovieManager.Application.Validators;

namespace MovieManager.Tests.Application.Validators;

public class RegisterValidatorTests
{
    private readonly RegisterValidator _validator;

    public RegisterValidatorTests()
    {
        _validator = new RegisterValidator();
    }

    [Fact]
    public void Validate_ValidRegistration_ReturnsNoErrors()
    {
        var register = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var result = _validator.Validate(register);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    public void Validate_InvalidEmail_ReturnsError(string email)
    {
        var register = new RegisterDto
        {
            Email = email,
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var result = _validator.Validate(register);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("nouppercase1!")]
    [InlineData("NOLOWERCASE1!")]
    [InlineData("NoNumbers!")]
    public void Validate_InvalidPassword_ReturnsError(string password)
    {
        var register = new RegisterDto
        {
            Email = "test@example.com",
            Password = password,
            FirstName = "John",
            LastName = "Doe"
        };

        var result = _validator.Validate(register);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_EmptyFirstName_ReturnsError()
    {
        var register = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "",
            LastName = "Doe"
        };

        var result = _validator.Validate(register);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }

    [Fact]
    public void Validate_EmptyLastName_ReturnsError()
    {
        var register = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = ""
        };

        var result = _validator.Validate(register);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LastName");
    }

    [Fact]
    public void Validate_FirstNameTooLong_ReturnsError()
    {
        var register = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = new string('A', 51),
            LastName = "Doe"
        };

        var result = _validator.Validate(register);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }
}
