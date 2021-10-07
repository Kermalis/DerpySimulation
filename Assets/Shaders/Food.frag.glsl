#version 330 core

const int MAX_LIGHTS = 4 - 1; // Sun does not affect
const float AMBIENT_LIGHTING = 0.85;
const float SHINE_DAMPER = 1;
const float SPECULAR_REFLECTIVITY = 0.9;

in vec3 pass_color;
in vec3 pass_normal;
in vec3 pass_vecToLight[MAX_LIGHTS];
in vec3 pass_vecToCamera;

out vec4 out_color;

uniform uint numLights;
uniform vec3 lightColor[MAX_LIGHTS];
uniform vec3 lightAttenuation[MAX_LIGHTS];


// Calculates the lighting for each light
vec3 diffuseLighting(vec3 toLightVector, vec3 lightColor, float attenuation, vec3 normal)
{
    float brightness = max(dot(toLightVector, normal), 0);
    return brightness * lightColor / attenuation;
}
vec3 specularLighting(vec3 toCamVector, vec3 toLightVector, vec3 lightColor, float attenuation, vec3 normal)
{
    float specularFactor = max(dot(reflect(-toLightVector, normal), toCamVector), 0);
    specularFactor = pow(specularFactor, SHINE_DAMPER);
    return specularFactor * SPECULAR_REFLECTIVITY * lightColor / attenuation;
}
// Adds some specular lighting coming from the camera
vec3 specularLightingCam(vec3 toCamVector, vec3 normal)
{
    float specularFactor = max(dot(reflect(-toCamVector, normal), toCamVector), 0);
    specularFactor = pow(specularFactor, SHINE_DAMPER);
    return specularFactor * SPECULAR_REFLECTIVITY * vec3(0, 1, 1) * 0.35;
}
// Adds some specular lighting from the opposite side of the object, creating an edge glow effect
vec3 specularLightingCamGlow(vec3 toCamVector, vec3 normal)
{
    const float LENIENCY = 0.1;
    float specularFactor = min(max(dot(reflect(toCamVector, normal), toCamVector), -LENIENCY) + LENIENCY, 1);
    specularFactor = pow(specularFactor, SHINE_DAMPER);
    return specularFactor * SPECULAR_REFLECTIVITY * vec3(1, 1, 1) * 0.1;
}

void main()
{
    // Calculate lighting
    vec3 totalDiffuse = vec3(0);
    vec3 totalSpecular = vec3(0);
    for (uint i = 0u; i < numLights; i++)
    {
        vec3 toLightVector = pass_vecToLight[i];

        // Calculate attenuation before toLightVector is normalized
        float dist = length(toLightVector);
        vec3 atten = lightAttenuation[i];
        float attenFactor = atten.x + (atten.y * dist) + (atten.z * dist * dist);
        
        toLightVector = normalize(toLightVector); // Normalize vector to light for diffuse/specular calculations
        vec3 lColor = lightColor[i];

        totalDiffuse += diffuseLighting(toLightVector, lColor, attenFactor, pass_normal);
        totalSpecular += specularLighting(pass_vecToCamera, toLightVector, lColor, attenFactor, pass_normal);
    }
    totalDiffuse = max(totalDiffuse, AMBIENT_LIGHTING); // Ambient lighting
    totalSpecular += specularLightingCam(pass_vecToCamera, pass_normal);
    totalSpecular += specularLightingCamGlow(pass_vecToCamera, pass_normal);

    // Calculate the final color of this pixel
    out_color = vec4(totalDiffuse, 1) * vec4(pass_color, 1) + vec4(totalSpecular, 1);
}