#version 330 core

flat in vec4 pass_color;

out vec4 out_color;


void main()
{
    out_color = pass_color;
}