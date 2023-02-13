using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator
{
    class PacketFormat
    {
        // {0} 패킷 등록
        public static string managerFormat =
@"
class PacketManager
{{
    #region Singleton
    static PacketManager _instance;
    public static PacketManager Instance
    {{
        get 
        {{ 
            if (_instance == null)
                _instance = new PacketManager();
            return _instance;
        }}
    }}
    #endregion

    // _OnRecv 딕셔너리: 1. 프로토콜ID로 구별. 2.어떤 행동을 할 것인지 받아서 저장.
    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
        
    // _handler 딕셔너리: MakePacket제너릭함수에서 패킷buffer를 읽어서 만든 action을 저장하는 딕셔너리.
    // PacketHandler클래스의 PlayerInfoReqHandler함수를 
    // 1. 프로토콜ID를 받고, 2. IPacket를 받아서 생성.
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    // 레지스터 함수: _OnRecv 딕셔너리에 패킷클래스(예:PlayerInfoReq)를 생성하고 넣어줌.
    public void Register()  
    {{
{0}
    }}


    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {{
        //역직렬화
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        switch ((PacketID)id)
        {{
            case PacketID.PlayerInfoReq:
                {{
                    PlayerInfoReq p = new PlayerInfoReq();
                    p.Read(buffer);

                }}
                break;
        }}

        // action 생성: PacketSession과 ArraySegment(패킷)를 인자로 받는 delegate타입.
        Action<PacketSession, ArraySegment<byte>> action = null;
        // _onRecv딕셔너리에서 id에 맞는 action을 받아올 수 있다면, 그 action에 session과 buffer를 넣어서 실행한다.
        if (_onRecv.TryGetValue(id, out action))
            action.Invoke(session, buffer);
            
    }}

    // MakePacket은 패킷클래스(예를 들어 PlayerInfoReq)를 받을수 있도록 제너릭타입<T>으로 만들고, 다음 두 조건을 가지도록 하자.
    // (1) 제너릭타입<T>는 IPacket인터페이스를 구현하는 클래스여야 하고,
    // (2) 제너릭타입<T>는 new()가 가능해야 한다.
    // <<다음 정리글은 내가 코드를 풀이해본 내용이고, 틀렸을 수도 있다.>>
    // MakePacket 제너릭함수: 타입<T>을 인자로(예: class PlayerInfoReq) 받으면서 실행되는 함수. <T>타입(예: <PlayerInfoReq>)
    // IPacket인터페이스를 상속받는 어떠한 클래스(예: class PlayerInfoReq)를 타입<T>인자로 받았다.
    // 인자로 받은 그 클래스는 패킷 '역직렬화'에 필요한 코드가 구현되어 있는 '패킷클래스'이다.
    // 그 역직렬화 '패킷클래스' 인스턴스(new T())를 생성하고나서,
    // 인자로 받는 두 가지(어떤 session이 지금 어떤 직렬화된 패킷들을 받아서 buffer에 가지고 있다는) 정보를 담아주는 껍데기인 action 변수도 생성한다.
    // MakePacket은 레지스터함수 Register()로 단 한번만 싱글톤으로 실행되는 함수이다.
    // 그 이후 OnRecvPacket(session, buffer)함수가 수시로 호출되면서 _OnRecv딕셔너리를 검사해서 그 안에 넣어놨던 MakePacket함수(특정 '패킷클래스' 타입을 주어서 만들었던 함수)를 수시로 호출한다.
    // 만일 가지고 있는 프로토콜ID를 key로 가지는 value를 _handler에서 꺼낼 수 있다면 action형식으로 꺼내고 session과 패킷클래스를 가지고 Invoke해준다.(다는 이해 못함. 특히 handler와 invoke)
    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T: IPacket, new()
    {{
        T pkt = new T();
        pkt.Read(buffer);//buffer를 역직렬화.

        Action<PacketSession, IPacket> action = null;
        //만약 _handler딕셔너리에 action이 이미 있었다면 그 action을 invoke.
        if (_handler.TryGetValue(pkt.Protocol, out action))
            action.Invoke(session, pkt);
    }}

}}
";

        // {0} 패킷 이름
        public static string managerRegisterFormat =
@"      // MakePacket제너릭함수에게 어떤 패킷클래스를 생성할지(PlayerInfoReq같은) 알려준 다음, 그래서 MakePacket제너릭함수가 생성한 패킷클래스를 인자로 받고,
        // PacketID도 인자로 받아서
        // _onRecv딕셔너리에다가 저장함.
        _onRecv.Add((ushort)PacketID.{0}, MakePacket<{0}>);
        // PlayerInfoReq에 짝이 맞는 PlayerInfoReqHandler를 담는 _handler딕셔너리
        _handler.Add((ushort)PacketID.{0}, PacketHandler.{0}Handler);
";

        // {0} 패킷 이름/번호 목록
        // {1} 패킷 목록
        public static string fileFormat =
@"using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public enum PacketID
{{
    {0}
}}

interface IPacket
{{
	ushort Protocol {{ get; }}
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}}

{1}
";

        // {0} 패킷 이름
        // {1} 패킷 번호
        public static string packetEnumFormat =
@"{0} = {1},";



        // {0] 패킷 이름
        // {1} 멤버 변수들
        // {2} 멤버 변수 Read
        // {3} 멤버 변수 Write
        public static string packetFormat =
@"
class {0} : IPacket
{{
    {1}
   
    public ushort Protocol {{ get {{ return (ushort)PacketID.{0};  }} }}

    public void Read(ArraySegment<byte> segment)
    {{
        //역직렬화
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        {2}
    }}

    public ArraySegment<byte> Write()
    {{
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); // 한번에 큰 덩어리. new byte[4096];  

        //직렬화
        ushort count = 0;
        bool success = true;

        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.{0});//slice은 그 자체를 변화시키는 함수가 아니라 계산값을 리턴만 해줌.
        count += sizeof(ushort);
        {3}
        success &= BitConverter.TryWriteBytes(s, count); // 보내려는 최종 사이즈는 누적된 count.
        if (success == false)
            return null;
        return SendBufferHelper.Close(count);
    }}
}}
";
        // {0} 변수 형식
        // {1} 변수 이름
        public static string memberFormat =
@"public {0} {1};";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        // {2} 멤버 변수들
        // {3} 멤버 변수 Read
        // {4} 멤버 변수 Write
        public static string memberListFormat =
@"
public class {0}
{{
    {2}

    public void Read(ReadOnlySpan<byte> s, ref ushort count)
    {{
        {3}
    }}

    public bool Write(Span<byte> s, ref ushort count)//Span: 전체 배열, count: 현재 작업한부분.
    {{
        bool success = true;
        {4}
        return success;
    }}
}}
public List<{0}> {1}s = new List<{0}>();
";

        // {0} 변수 이름
        // {1} To~ 변수 형식
        // {2} 변수 형식
        public static string readFormat =
@"this.{0} = BitConverter.{1}(s.Slice(count, s.Length - count));
count += sizeof({2});";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string readByteFormat =
@"this.{0} = ({1})segment.Array[segment.Offset + count];//sbyte일 경우 sbyte로 casting을 위해서 ()추가함.
count += sizeof({1});";



        // {0} 변수 이름
        public static string readStringFormat =
@"ushort {0}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(s.Slice(count, {0}Len));
count += {0}Len;";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        public static string readListFormat =
@"
// {1} list
this.{1}s.Clear();
ushort {1}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
for (int i = 0; i < {1}Len; i++)
{{
    {0} {1} = new {0}();
    {1}.Read(s, ref count);
    {1}s.Add({1});
}}
";

        // {0} 변수 이름
        // {1} 변수 형식

        public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.{0});
count += sizeof({1});";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string writeByteFormat =
@"segment.Array[segment.Offset + count] = (byte)this.{0};//sbyte일 경우 byte로 강제 casting을 위해서 (byte)추가함.
count += sizeof({1});";

        // {0} 변수 이름

        public static string writeStringFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + count + sizeof(ushort));
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {0}Len);
count += sizeof(ushort);
count += {0}Len;";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        public static string writeListFormat =
@"
// {1} list
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.{1}s.Count);
count += sizeof(ushort);
foreach ({0} {1} in this.{1}s)
    success &= {1}.Write(s, ref count);
";
    }
}
