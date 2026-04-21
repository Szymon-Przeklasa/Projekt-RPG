using Godot;

public partial class EliteEnemy : Enemy
{
    public override void _Ready()
    {
        base._Ready();
        
        // 2x HP, 2x XP, 1.2x size
        MaxHealth = (int)(MaxHealth * 2f);
        _health = MaxHealth;
        XpDrop = (int)(XpDrop * 2.5f);
        Scale *= 1.2f;
        
        // Blue outline through shader
        var sprite = FindSprite(this);
        if (sprite != null)
        {
            var mat = new ShaderMaterial();
            mat.Shader = GD.Load<Shader>("res://Shaders/outline.gdshader");
            mat.SetShaderParameter("outline_color", new Color(0.2f, 0.4f, 1f, 1f));
            mat.SetShaderParameter("outline_width", 1f);
            sprite.Material = mat;
        }
    }

    private Sprite2D FindSprite(Node node)
    {
        if (node is Sprite2D sprite)
            return sprite;

        foreach (Node child in node.GetChildren())
        {
            var found = FindSprite(child);
            if (found != null)
                return found;
        }

        return null;
    }
}