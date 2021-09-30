#version 330 core

const int MAX_LIGHTS = 4;
const float AMBIENT_LIGHTING = 0.1;

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec3 in_color;

flat out vec4 pass_color;

uniform mat4 model;
uniform mat4 projectionViewMatrix;
uniform uint numLights;
uniform vec3 lightPos[MAX_LIGHTS];
uniform vec3 lightColor[MAX_LIGHTS];
uniform vec3 lightAttenuation[MAX_LIGHTS];

vec4 calcLighting(vec4 worldPos)
{
    // Calculate light data
    vec3 vecToLight[MAX_LIGHTS];
    for (uint i = 0u; i < numLights; i++)
    {
        vecToLight[i] = lightPos[i] - worldPos.xyz;
    }

    // Apply lighting
    vec3 norm = normalize((model * vec4(in_normal, 0)).xyz); // Update normal location with model location

    vec3 totalDiffuse = vec3(0);
    for (uint i = 0u; i < numLights; i++)
    {
        vec3 lightVector = vecToLight[i];
        float dist = length(lightVector); // Get distance before it's normalized
        lightVector = normalize(lightVector);

        float brightness = max(dot(norm, lightVector), 0);

        vec3 atten = lightAttenuation[i];
        float attenFactor = atten.x + (atten.y * dist) + (atten.z * dist * dist);
        totalDiffuse += (brightness * lightColor[i]) / attenFactor;
    }
    totalDiffuse = max(totalDiffuse, AMBIENT_LIGHTING); // Ambient lighting

    // Final value
    return vec4(totalDiffuse, 1) * vec4(in_color, 1);
}

void main()
{
    // Calculate vertex pos
    vec4 worldPos = model * vec4(in_position, 1);
    gl_Position = projectionViewMatrix * worldPos;

    // Calculate lighting here instead of per-pixel so it appears completely flat
    pass_color = calcLighting(worldPos);
}