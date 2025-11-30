namespace Backends.FChan.Models;

public readonly record struct PostId(long Value)
{
    public static implicit operator long(PostId id) => id.Value;
    public static explicit operator PostId(long value) => new(value);
    
    public override string ToString() => Value.ToString();
}

public readonly record struct AttachmentId(long Value)
{
    public static implicit operator long(AttachmentId id) => id.Value;
    public static explicit operator AttachmentId(long value) => new(value);
    
    public override string ToString() => Value.ToString();
}