using BenchmarkDotNet.Attributes;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using System.Collections.Generic;

[MemoryDiagnoser]
public class ValidationBenchmarks
{
    private List<UserDto> _samples;
    private UserDtoFluentValidator _fluentValidator;

    [GlobalSetup]
    public void Setup()
    {
        _samples = new();
        for (int i = 0; i < 10000; i++)
        {
            _samples.Add(new UserDto
            {
                Email = $"user{i}@example.com",
                Password = "123456"
            });
        }

        _fluentValidator = new UserDtoFluentValidator();
    }

    [Benchmark]
    public void DataAnnotations()
    {
        foreach (var dto in _samples)
        {
            var context = new ValidationContext(dto);
            Validator.TryValidateObject(dto, context, new List<ValidationResult>(), true);
        }
    }

    [Benchmark]
    public void FluentValidation()
    {
        foreach (var dto in _samples)
        {
            _fluentValidator.Validate(dto);
        }
    }
}
