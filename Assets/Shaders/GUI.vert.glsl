#version 330 core

in vec2 in_position;

uniform vec2 relPos;
uniform vec2 relSize;


vec2 RelToGL(vec2 v)
{
    return vec2(v.x * 2 - 1, v.y * -2 + 1);
}

void main()
{
    gl_Position = vec4(RelToGL(in_position * relSize + relPos), 0, 1);
}