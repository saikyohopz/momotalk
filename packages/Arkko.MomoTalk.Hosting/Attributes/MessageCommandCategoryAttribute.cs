namespace Arkko.MomoTalk.Hosting.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class MessageCommandCategoryAttribute : Attribute {
    public MessageCommandCategoryAttribute(string category = "default") {
        Category = category;
    }

    public string Category { get; set; }
}
