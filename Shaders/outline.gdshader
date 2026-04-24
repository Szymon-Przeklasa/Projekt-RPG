// outline_solid.gdshader — attach to a duplicate Sprite2D scaled up by (1 + width/texture_size)
shader_type canvas_item;

uniform vec4 outline_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);

void fragment() {
    float alpha = texture(TEXTURE, UV).a;
    COLOR = alpha > 0.1 ? outline_color : vec4(0.0);
}
