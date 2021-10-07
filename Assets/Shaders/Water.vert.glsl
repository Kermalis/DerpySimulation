#version 330 core

// Light constants
const int MAX_LIGHTS = 4;
const float AMBIENT_LIGHTING = 0.15;
const float SPECULAR_REFLECTIVITY = 0.2; // Does not control the reflection texture; only light reflection
const float SHINE_DAMPER = 2.0;
// Wave animation constants
const float PI = 22.0 / 7.0;
const float WAVE_LENGTH = 1000; // Creates line patterns in the distortion
const float WAVE_HEIGHT = 0.25;

layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_partnerVertex1;
layout(location = 2) in vec2 in_partnerVertex2;

out vec4 pass_clipSpaceOriginal;
out vec4 pass_clipSpaceDistorted;
out vec3 pass_normal;
out vec3 pass_toCameraVector;
out vec3 pass_specular;
out vec3 pass_diffuse;

uniform mat4 projectionView;
uniform float height;
uniform vec3 cameraPos;
uniform float waveTime;
uniform uint numLights;
uniform vec3 lightPos[MAX_LIGHTS];
uniform vec3 lightColor[MAX_LIGHTS];
uniform vec3 lightAttenuation[MAX_LIGHTS];

// Gets the distortion amount for this vertex based on the wave time
float generateOffset(float x, float z, float valX, float valZ)
{
    float radiansX = ((mod((x * z * valX) + x, WAVE_LENGTH) / WAVE_LENGTH) + waveTime * mod(x * 0.8 + z, 1.5)) * 2.0 * PI;
    float radiansZ = ((mod(((x * z) + (x * z)) * valZ, WAVE_LENGTH) / WAVE_LENGTH) + waveTime * 2.0 * mod(x , 2.0) ) * 2.0 * PI;
    return WAVE_HEIGHT * 0.5 * (sin(radiansZ) + cos(radiansX));
}
// Slightly bobs up and down the coordinates of this vertex
// Still looks good without x/z distortion so if we need performance we can skip those calculations
vec3 distort(vec3 v)
{
    float distortionX = generateOffset(v.x, v.z, 0.23, 0.16);
    float distortionY = generateOffset(v.x, v.z, 0.17, 0.31);
    float distortionZ = 0; // generateOffset(v.x, v.z, 0.21, 0.26);
    return v + vec3(distortionX, distortionY, distortionZ);
}

// Gets the normal of a triangle
vec3 getNormal(vec3 v0, vec3 v1, vec3 v2)
{
    return normalize(cross(v1 - v0, v2 - v0));
}

// Calculates the lighting for each light
vec3 diffuseLighting(vec3 toLightVector, vec3 lightColor, float attenuation, vec3 normal)
{
    float brightness = max(dot(toLightVector, normal), 0);
    return (brightness * lightColor) / attenuation;
}
vec3 specularLighting(vec3 toCamVector, vec3 toLightVector, vec3 lightColor, float attenuation, vec3 normal)
{
    float specularFactor = max(dot(reflect(-toLightVector, normal), toCamVector), 0);
    specularFactor = pow(specularFactor, SHINE_DAMPER);
    return (specularFactor * SPECULAR_REFLECTIVITY * lightColor) / attenuation;
}

void main()
{    
    // Get the grid position of all 3 vertices in the triangle
    vec3 currentVertex = vec3(in_position.x, height, in_position.y);
    vec3 vertex1 = currentVertex + vec3(in_partnerVertex1.x, 0, in_partnerVertex1.y);
    vec3 vertex2 = currentVertex + vec3(in_partnerVertex2.x, 0, in_partnerVertex2.y);

    // The base position from the vbo
    pass_clipSpaceOriginal = projectionView * vec4(currentVertex, 1);

    // Apply distortion to all 3 vertices
    currentVertex = distort(currentVertex);
    vertex1 = distort(vertex1);
    vertex2 = distort(vertex2);

    // Get the normal of this water piece after distortion
    pass_normal = getNormal(currentVertex, vertex1, vertex2);

    // The currentVertex is now distorted so we need another variable for it, which is used to get the water depth later
    pass_clipSpaceDistorted = projectionView * vec4(currentVertex, 1);
    gl_Position = pass_clipSpaceDistorted;

    // Calculate the vector of the camera direction
    pass_toCameraVector = normalize(cameraPos - currentVertex);

    // Calculate lighting
    pass_diffuse = vec3(0);
    pass_specular = vec3(0);
    for (uint i = 0u; i < numLights; i++)
    {
        vec3 toLightVector = lightPos[i] - currentVertex;
        // Calculate attenuation before toLightVector is normalized
        float dist = length(toLightVector);
        vec3 atten = lightAttenuation[i];
        float attenFactor = atten.x + (atten.y * dist) + (atten.z * dist * dist);
        
        toLightVector = normalize(toLightVector); // Normalize vector to light for diffuse/specular calculations
        vec3 lColor = lightColor[i];

        pass_diffuse += diffuseLighting(toLightVector, lColor, attenFactor, pass_normal);
        pass_specular += specularLighting(pass_toCameraVector, toLightVector, lColor, attenFactor, pass_normal);
    }
    pass_diffuse = max(pass_diffuse, AMBIENT_LIGHTING); // Ambient lighting
}