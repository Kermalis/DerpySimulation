#version 330 core

const int MAX_LIGHTS = 4;
const float AMBIENT_LIGHTING = 0.15;

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec3 in_color;

flat out vec4 pass_color;

uniform mat4 model;
uniform vec4 clippingPlane;
uniform mat4 projectionViewMatrix;
uniform uint numLights;
uniform vec3 lightPos[MAX_LIGHTS];
uniform vec3 lightColor[MAX_LIGHTS];
uniform vec3 lightAttenuation[MAX_LIGHTS];


vec4 calcLighting(vec4 worldPos)
{
    // Update normal location with model location
    vec3 norm = normalize((model * vec4(in_normal, 0)).xyz);

    // Calculate diffuse lighting for each light
    vec3 totalDiffuse = vec3(0);
    for (uint i = 0u; i < numLights; i++)
    {
        vec3 toLightVector = lightPos[i] - worldPos.xyz;
        float dist = length(toLightVector); // Get distance before toLightVector is normalized
        toLightVector = normalize(toLightVector);

        float brightness = max(dot(norm, toLightVector), 0);

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
    // This makes sure parts below the water aren't in the reflection, and parts above the water aren't in the refraction
    // The clipping plane isn't enabled when rendering the whole terrain as normal
    gl_ClipDistance[0] = dot(worldPos, clippingPlane);

    // Calculate lighting here instead of per-pixel so it appears completely flat
    pass_color = calcLighting(worldPos);
}