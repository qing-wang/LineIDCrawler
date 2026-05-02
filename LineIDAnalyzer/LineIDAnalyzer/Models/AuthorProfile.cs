namespace LineIDAnalyzer.Models
{
    public enum ProfileSource { 自陳, 推斷, 無法分析 }

    public class ProfileField
    {
        public string?       Value  { get; set; }
        public ProfileSource Source { get; set; } = ProfileSource.無法分析;

        public bool   HasValue    => !string.IsNullOrWhiteSpace(Value) && Source != ProfileSource.無法分析;
        public string DisplayText => Source == ProfileSource.無法分析
            ? "無法分析"
            : $"{Value}（{(Source == ProfileSource.自陳 ? "自陳" : "推斷")}）";
    }

    public class AuthorProfile
    {
        public ProfileField Gender             { get; set; } = new();
        public ProfileField Age                { get; set; } = new();
        public ProfileField ResidentialArea    { get; set; } = new();
        public ProfileField Interests          { get; set; } = new();
        public ProfileField RelationshipStatus { get; set; } = new();
        public ProfileField Occupation         { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public bool    IsSuccess    => ErrorMessage == null;
        public string  RawResponse  { get; set; } = string.Empty;
    }
}
