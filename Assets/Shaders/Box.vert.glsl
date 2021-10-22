#version 330 core

const int MAX_LIGHTS = 4 - 1; // Sun does not affect

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec3 in_instancedColorEast;
layout(location = 3) in vec3 in_instancedColorWest;
layout(location = 4) in vec3 in_instancedColorUp;
layout(location = 5) in vec3 in_instancedColorDown;
layout(location = 6) in vec3 in_instancedColorSouth;
layout(location = 7) in vec3 in_instancedColorNorth;
layout(location = 8) in mat4 in_instancedTransform;

out vec3 pass_color;
out vec3 pass_normal;
out vec3 pass_vecToLight[MAX_LIGHTS];
out vec3 pass_vecToCamera;

uniform mat4 projectionView;
uniform vec4 clippingPlane;
uniform vec3 cameraPos;
uniform uint numLights;
uniform vec3 lightPos[MAX_LIGHTS];


vec3 getColor()
{
    if (in_normal.x ==  1)   return in_instancedColorEast;
    if (in_normal.x == -1)   return in_instancedColorWest;
    if (in_normal.y ==  1)   return in_instancedColorUp;
    if (in_normal.y == -1)   return in_instancedColorDown;
    if (in_normal.z ==  1)   return in_instancedColorSouth;
  /*if (in_normal.z == -1)*/ return in_instancedColorNorth;
}

void main()
{
    vec4 worldPos = in_instancedTransform * vec4(in_position, 1);
    gl_Position = projectionView * worldPos;
    // This makes sure parts below the water aren't in the reflection, and parts above the water aren't in the refraction
    // The clipping plane isn't enabled when rendering the whole terrain as normal
    gl_ClipDistance[0] = dot(worldPos, clippingPlane);

    pass_color = getColor();
    pass_normal = normalize((in_instancedTransform * vec4(in_normal, 0)).xyz); // Update normal
    
    for (uint i = 0u; i < numLights; i++)
    {
        pass_vecToLight[i] = lightPos[i] - worldPos.xyz;
    }
    pass_vecToCamera = normalize(cameraPos - worldPos.xyz);
}