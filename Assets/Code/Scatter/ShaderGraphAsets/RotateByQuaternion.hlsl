void RotateByQuaternion_float(float3 Position_IN, float4 Quaternion, out float3 Position_OUT)
{
    float3 t = 2.0 * cross(Quaternion.xyz, Position_IN);
    Position_OUT = Position_IN + Quaternion.w * t + cross(Quaternion.xyz, t);
}

void RotateByQuaternion_half(half3 Position_IN, half4 Quaternion, out half3 Position_OUT)
{
    half3 t = 2.0 * cross(Quaternion.xyz, Position_IN);
    Position_OUT = Position_IN + Quaternion.w * t + cross(Quaternion.xyz, t);
}