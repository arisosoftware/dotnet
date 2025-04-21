public interface ISensitiveDataRule
{
    bool IsSensitive(string data);
    string MaskData(string data);
}
