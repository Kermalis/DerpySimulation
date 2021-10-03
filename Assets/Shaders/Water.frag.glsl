#version 330 core

// Color constants
const float FRESNEL_REFLECTIVENESS = 0.05; // Higher value makes it more reflective
const float EDGE_SOFTNESS = 5; // How soft the top of the water is (modifies alpha)
const vec3 BLUENESS_COLOR = vec3(0.2, 0.4, 0.9);
const float BLUENESS_MIN = 0.1;
const float BLUENESS_MAX = 0.7;
const float MURKY_DEPTH = 150; // The depth the water must be to be BLUENESS_MAX

in vec4 pass_clipSpaceOriginal;
in vec4 pass_clipSpaceDistorted;
in vec3 pass_normal;
in vec3 pass_toCameraVector;
in vec3 pass_specular;
in vec3 pass_diffuse;

out vec4 out_color;

uniform sampler2D reflectionTexture;
uniform sampler2D refractionTexture;
uniform sampler2D depthTexture;
uniform vec2 nearFarPlanes; // Near plane is stored in x, far plane is stored in y


// Applies perspective texturing to get a texture coordinate from the reflection/refraction textures
vec2 clipSpaceToTexCoords(vec4 clipSpace)
{
    vec2 ndc = clipSpace.xy / clipSpace.w;
    vec2 texCoords = ndc * 0.5 + 0.5;
    return clamp(texCoords, 0.002, 0.998); // This clamp fixes the wobble at the bottom of the screen
}

// Depth values in a depth texture aren't stored linearly, so this converts them
float toLinearDepth(float z)
{
    float near = nearFarPlanes.x;
    float far = nearFarPlanes.y;
    return 2.0 * near * far / (far + near - (2.0 * z - 1.0) * (far - near));
}
float getWaterDepth(vec2 texCoords)
{
    float depth = texture(depthTexture, texCoords).r; // Sample depth texture, all depth is in the r component
    float floorDistance = toLinearDepth(depth); // The distance from the floor to the camera
    depth = gl_FragCoord.z;
    float waterDistance = toLinearDepth(depth); // The distance from the water to the camera
    return floorDistance - waterDistance; // Now we can get the distance from the water to the floor since we know both
}

// Uses the water's depth to mix the refraction color with a blue water color
vec3 applyMurkiness(vec3 refractColor, float waterDepth)
{
    float murkyFactor = clamp(waterDepth / MURKY_DEPTH, 0, 1);
    float murkiness = BLUENESS_MIN + murkyFactor * (BLUENESS_MAX - BLUENESS_MIN);
    return mix(refractColor, BLUENESS_COLOR, murkiness);
}

// Makes the water more reflective depending on the angle
// More transparent if you're looking directly at the water's normal (above)
// More reflective if you're looking perpendicular to the water's normal (sideways)
float getFresnelFactor()
{
    vec3 toCam = normalize(pass_toCameraVector);
    vec3 normal = normalize(pass_normal);
    float refractiveFactor = pow(dot(toCam, normal), FRESNEL_REFLECTIVENESS);
    return clamp(refractiveFactor, 0, 1);
}

void main()
{
    vec2 texCoordsOriginal = clipSpaceToTexCoords(pass_clipSpaceOriginal);
    vec2 texCoordsDistorted = clipSpaceToTexCoords(pass_clipSpaceDistorted);
    
    vec2 reflectionTexCoords = vec2(texCoordsOriginal.x, 1 - texCoordsOriginal.y); // Flip reflection
    vec2 refractionTexCoords = texCoordsOriginal;
    float waterDepth = getWaterDepth(texCoordsDistorted);
    
    // Get colors from the rendered scene
    vec3 reflectColor = texture(reflectionTexture, reflectionTexCoords).rgb;
    vec3 refractColor = texture(refractionTexture, refractionTexCoords).rgb;
    reflectColor = mix(reflectColor, BLUENESS_COLOR, BLUENESS_MIN); // Mix some blueness into the reflection
    refractColor = applyMurkiness(refractColor, waterDepth); // Add blue murkiness to the refraction

    // Calculate the final color of this pixel
    vec3 finalColor = mix(reflectColor, refractColor, getFresnelFactor()); // Mix the reflection and refraction based on the fresnel effect
    finalColor = finalColor * pass_diffuse + pass_specular; // Add the lighting
    float finalAlpha = clamp(waterDepth / EDGE_SOFTNESS, 0, 1); // Apply soft edges to shallow water
    out_color = vec4(finalColor, finalAlpha);
}