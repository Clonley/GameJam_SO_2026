StructuredBuffer<uint> _visibleIDs;
StructuredBuffer<float3> _positions;

void ReadBufferValue_float(
    uint InstanceID,
    out float3 Position,
    out float id_1)
{
    uint index = _visibleIDs[InstanceID];
    Position = _positions[index];
    id_1 = (float) index;
}

void ReadBufferValue_half(
    uint InstanceID,
    out half3 Position,
    out half id_1)
{
    uint index = _visibleIDs[InstanceID];
    Position = (half3) _positions[index];
    id_1 = (half) index;
}