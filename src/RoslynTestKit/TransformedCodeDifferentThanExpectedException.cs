namespace RoslynTestKit
{
    public class TransformedCodeDifferentThanExpectedException: RoslynTestKitException
    {
        public string TransformedCode { get; }
        public string ExpectedCode { get; }

        public TransformedCodeDifferentThanExpectedException(string transformedCode, string expectedCode):base("Transformed code is different than expected")
        {
            TransformedCode = transformedCode;
            ExpectedCode = expectedCode;
        }
    }
}