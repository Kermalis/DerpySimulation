#version 330 core

const int MAX_LIGHTS = 4 - 1; // Sun does not affect

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec3 in_instancedColor;
layout(location = 3) in mat4 in_instancedTransform;

out vec3 pass_color;
out vec3 pass_normal;
out vec3 pass_vecToLight[MAX_LIGHTS];
out vec3 pass_vecToCamera;

uniform mat4 projectionView;
uniform vec4 clippingPlane;
uniform vec3 cameraPos;
uniform uint numLights;
uniform vec3 lightPos[MAX_LIGHTS];


void main()
{
    vec4 worldPos = in_instancedTransform * vec4(in_position, 1);
    gl_Position = projectionView * worldPos;
    // This makes sure parts below the water aren't in the reflection, and parts above the water aren't in the refraction
    // The clipping plane isn't enabled when rendering the whole terrain as normal
    gl_ClipDistance[0] = dot(worldPos, clippingPlane);

    pass_color = in_instancedColor;
    pass_normal = normalize((in_instancedTransform * vec4(in_normal, 0)).xyz); // Update normal
    
    for (uint i = 0u; i < numLights; i++)
    {
        pass_vecToLight[i] = lightPos[i] - worldPos.xyz;
    }
    pass_vecToCamera = normalize(cameraPos - worldPos.xyz);
}