#define INITIALIZE_RECOLOR_PROPERTY(name) sampler2D _SourceMask_##name##; fixed4 _SourceColor_##name##; fixed4 _SourceColorRange_##name##; fixed4 _ReplacementColor_##name##;

INITIALIZE_RECOLOR_PROPERTY(0)
INITIALIZE_RECOLOR_PROPERTY(1)
INITIALIZE_RECOLOR_PROPERTY(2)
INITIALIZE_RECOLOR_PROPERTY(3)
INITIALIZE_RECOLOR_PROPERTY(4)
INITIALIZE_RECOLOR_PROPERTY(5)
INITIALIZE_RECOLOR_PROPERTY(6)
INITIALIZE_RECOLOR_PROPERTY(7)
INITIALIZE_RECOLOR_PROPERTY(8)
INITIALIZE_RECOLOR_PROPERTY(9)

#define RECOLOR(origin, color, uv, name) lerp(color, lerp(color, _ReplacementColor_##name##, in_range(origin, _SourceColor_##name##, _SourceColorRange_##name##)), tex2D(_SourceMask_##name##, uv))

inline bool in_range(fixed4 a, fixed4 b, fixed4 range)
{
    return distance(a.r, b.r) <= range.r && distance(a.g, b.g) <= range.g && distance(a.b, b.b) <= range.b && distance(a.a, b.a) <= range.a;
}

fixed4 recolor(fixed4 color, float2 uv)
{
    fixed4 origin = color;
    color = RECOLOR(origin, color, uv, 9);
    color = RECOLOR(origin, color, uv, 8);
    color = RECOLOR(origin, color, uv, 7);
    color = RECOLOR(origin, color, uv, 6);
    color = RECOLOR(origin, color, uv, 5);
    color = RECOLOR(origin, color, uv, 4);
    color = RECOLOR(origin, color, uv, 3);
    color = RECOLOR(origin, color, uv, 2);
    color = RECOLOR(origin, color, uv, 1);
    color = RECOLOR(origin, color, uv, 0);
    return color;
}