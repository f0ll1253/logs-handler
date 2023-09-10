namespace Core.View.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
    public UInt16 Index { get; set; }
}