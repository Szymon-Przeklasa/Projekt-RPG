shader_type canvas_item;

uniform vec4 outline_color : source_color = vec4(0.0, 0.5, 1.0, 1.0);
uniform float outline_width : hint_range(0, 10) = 1.0;

void fragment() {
    vec4 col = texture(TEXTURE, UV);
    
    if (col.a < 0.1) {
        float texel = outline_width / float(textureSize(TEXTURE, 0).x);
        float sum = texture(TEXTURE, UV + vec2(texel, 0)).a
                  + texture(TEXTURE, UV - vec2(texel, 0)).a
                  + texture(TEXTURE, UV + vec2(0, texel)).a
                  + texture(TEXTURE, UV - vec2(0, texel)).a;
        if (sum > 0.1)
            col = outline_color;
    }
    
    COLOR = col;
}