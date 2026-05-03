namespace MediaApp2.Models;

public class TagModel
{
    public string Name { get; set; }
    public string Color { get; set; }

    public TagModel(string name, string color)
    {
        Name = name;
        Color = color;
    }
}