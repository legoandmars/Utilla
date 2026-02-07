using System;

namespace Utilla.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ModdedBoardTextAttribute : Attribute
{
    public readonly string Title;

    public readonly string Text;

    public ModdedBoardTextAttribute()
    {
        Title = string.Empty;
        Text = string.Empty;
    }

    public ModdedBoardTextAttribute(string title, string text)
    {
        Title = title ?? string.Empty;
        Text = text ?? string.Empty;
    }
}
