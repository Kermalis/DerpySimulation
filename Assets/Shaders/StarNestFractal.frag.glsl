// Modified version of "Star Nest" by Pablo Roman Andrioli
// Original content is under the MIT License
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

const float NO_AMT = -1;
const float STEP_R_AMOUNT = 2;
const float STEP_G_AMOUNT = NO_AMT;
const float STEP_B_AMOUNT = 0.5;

out vec4 out_color;

uniform uvec2 screenSize;
uniform float displayTime;


mat2 makeRotMatrix(float v)
{
    float c = cos(v);
    float s = sin(v);
    return mat2(c, s, -s, c);
}
float getStepColor(float v, float amt)
{
    return amt == NO_AMT ? 0 : pow(v, amt);
}
vec3 getStepColor(float v)
{
    return vec3(getStepColor(v, STEP_R_AMOUNT), getStepColor(v, STEP_G_AMOUNT), getStepColor(v, STEP_B_AMOUNT));
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
    rot = makeRotMatrix(0.95);
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
        v += getStepColor(s) * a * BRIGHTNESS * curFade;
        curFade *= DIST_FADING;
        s += VOLUMETRIC_STEP_SIZE;
    }
    v = mix(vec3(length(v)), v, SATURATION);
    out_color = vec4(v * 0.01, 1);
}