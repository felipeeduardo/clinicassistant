using ClinicAssistant.Infrastructure.Identity;

var password = Environment.GetEnvironmentVariable("E2E_DEFAULT_PASSWORD");
if (string.IsNullOrWhiteSpace(password))
{
    Console.Error.WriteLine("E2E_DEFAULT_PASSWORD must be set before generating a test-data password hash.");
    return 2;
}

Console.WriteLine(PasswordHasher.Hash(password));
return 0;
