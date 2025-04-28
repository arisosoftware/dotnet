using Serilog;


namespace SerilogMask.WebApi
{
  
    public static class SerilogTester
    {
        public static void RunTests()
        {
            TestDirectPropertyMasking();
            TestStructuredPropertyMasking();
        }

        private static void TestDirectPropertyMasking()
        {
            Log.Information("Testing direct property masking: Email={Email}", "someone@domain.com");
        }

        private static void TestStructuredPropertyMasking()
        {
            var testUser = new UserDTO
            {
                Email = "user@example.com",
                Name = "John Doe",
                OtherInfo = "Non-sensitive data"
            };

            Log.Information("Testing structured property masking: {@UserDTO}", testUser);
        }
    }

    // Sample DTO class for testing
    public class UserDTO
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string OtherInfo { get; set; }
    }

}
