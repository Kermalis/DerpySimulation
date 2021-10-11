// Star Nest by Pablo Roman Andrioli
// This content is under the MIT License
#version 330 core

const int NUM_ITERATIONS = 15;
const float MAGIC_PARAM = 0.53;

const int NUM_VOLUMETRIC_STEPS = 15;
const float VOLUMETRIC_STEP_SIZE = 0.15; // Higher makes more clusters

const float ZOOM = 0.8;
const float TILE = 0.525;
const float MOVE_SPEED = 0.015;

const float BRIGHTNESS = 0.0001;
const float DARKMATTER = 0.0001;
const float DIST_FADING = 0.825;
const float SATURATION = 0.95;

out vec4 out_color;

uniform uvec2 screenSize;
uniform float displayTime;


mat2 makeRotMatrix(float v)
{
    float c = cos(v);
    float s = sin(v);
    return mat2(c, s, -s, c);
}

void main()
{
    vec2 uv = gl_FragCoord.xy / screenSize - 0.5;
    uv.y *= float(screenSize.y) / screenSize.x;
    vec3 dir = vec3(uv * ZOOM, 1);
    float time = (displayTime * MOVE_SPEED) + 0.25;
    vec3 from = vec3((time * 2) + 1, time + 0.5, -1.5);

    // Rotate a bit to make it look less digital
    mat2 rot = makeRotMatrix(0.3);
    dir.xz *= rot;
    from.xz *= rot;
    rot = makeRotMatrix(0.8);
    dir.xy *= rot;
    from.xy *= rot;

    float s = 0.1;
    float curFade = 1;
    vec3 v = vec3(0);
    for (int r = 0; r < NUM_VOLUMETRIC_STEPS; r++)
    {
        vec3 p = (s * dir * 0.5) + from;
        p = abs(vec3(TILE) - mod(p, vec3(TILE * 2)));
        float pa = 0;
        float a = 0;
        for (int i = 0; i < NUM_ITERATIONS; i++)
        {
            p = (abs(p) / dot(p, p)) - MAGIC_PARAM;
            a += abs(length(p) - pa);
            pa = length(p);
        }
        float dm = max(0, DARKMATTER - (a * a * 0.001));
        a *= a * a;
        if (r > 6)
        {
            curFade *= 1 - dm;
        }
        v += curFade;
        v += vec3(s * s * s, 0, s) * a * BRIGHTNESS * curFade; // Create a red/blue color
        curFade *= DIST_FADING;
        s += VOLUMETRIC_STEP_SIZE;
    }
    v = mix(vec3(length(v)), v, SATURATION);
    out_color = vec4(v * 0.01, 1);
}