
class PlayerInfoReq
{
    public long playerId;
	public string name;
	
   
    public void Read(ArraySegment<byte> segment)
    {
        //역직렬화
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
		count += sizeof(long);
		ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
		count += nameLen;
		
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); // 한번에 큰 덩어리. new byte[4096];  

        //직렬화
        ushort count = 0;
        bool success = true;

        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);//slice은 그 자체를 변화시키는 함수가 아니라 계산값을 리턴만 해줌.
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
		count += sizeof(long);
		ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
		count += sizeof(ushort);
		count += nameLen;
		
        success &= BitConverter.TryWriteBytes(s, count); // 보내려는 최종 사이즈는 누적된 count.
        if (success == false)
            return null;
        return SendBufferHelper.Close(count);
    }
}
