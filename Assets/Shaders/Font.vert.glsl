#version 330 core

layout(location = 0) in ivec2 in_position;
layout(location = 1) in vec2 in_uv;

out vec2 pass_uv;

uniform vec2 relPos;
uniform uvec2 screenSize;


vec2 AbsToRel(ivec2 v, uvec2 res)
{
    return vec2(float(v.x) / res.x, float(v.y) / res.y);
}
vec2 RelToGL(vec2 v)
{
    return vec2(v.x * 2 - 1, v.y * -2 + 1);
}

void main()
{
    vec2 strPos = AbsToRel(in_position, screenSize);
    gl_Position = vec4(RelToGL(strPos + relPos), 0, 1);
    pass_uv = in_uv;
}