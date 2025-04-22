using FluentValidation;

public class UserDtoFluentValidator : AbstractValidator<UserDto>
{
    public UserDtoFluentValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}
