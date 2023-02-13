using ServerCore;

class PacketManager
{
    #region Singleton
    static PacketManager _instance;
    public static PacketManager Instance
    {
        get 
        { 
            if (_instance == null)
                _instance = new PacketManager();
            return _instance;
        }
    }
    #endregion

    // _OnRecv 딕셔너리: 1. 프로토콜ID로 구별. 2.어떤 행동을 할 것인지 받아서 저장.
    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
        
    // _handler 딕셔너리: MakePacket제너릭함수에서 패킷buffer를 읽어서 만든 action을 저장하는 딕셔너리.
    // PacketHandler클래스의 PlayerInfoReqHandler함수를 
    // 1. 프로토콜ID를 받고, 2. IPacket를 받아서 생성.
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    // 레지스터 함수: _OnRecv 딕셔너리에 패킷클래스(예:PlayerInfoReq)를 생성하고 넣어줌.
    public void Register()  
    {
      // MakePacket제너릭함수에게 어떤 패킷클래스를 생성할지(PlayerInfoReq같은) 알려준 다음, 그래서 MakePacket제너릭함수가 생성한 패킷클래스를 인자로 받고,
        // PacketID도 인자로 받아서
        // _onRecv딕셔너리에다가 저장함.
        _onRecv.Add((ushort)PacketID.S_Test, MakePacket<S_Test>);
        // PlayerInfoReq에 짝이 맞는 PlayerInfoReqHandler를 담는 _handler딕셔너리
        _handler.Add((ushort)PacketID.S_Test, PacketHandler.S_TestHandler);


    }


    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        //역직렬화
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        // action 생성: PacketSession과 ArraySegment(패킷)를 인자로 받는 delegate타입.
        Action<PacketSession, ArraySegment<byte>> action = null;
        // _onRecv딕셔너리에서 id에 맞는 action을 받아올 수 있다면, 그 action에 session과 buffer를 넣어서 실행한다.
        if (_onRecv.TryGetValue(id, out action))
            action.Invoke(session, buffer);
            
    }

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
    {
        T pkt = new T();
        pkt.Read(buffer);//buffer를 역직렬화.

        Action<PacketSession, IPacket> action = null;
        //만약 _handler딕셔너리에 action이 이미 있었다면 그 action을 invoke.
        if (_handler.TryGetValue(pkt.Protocol, out action))
            action.Invoke(session, pkt);
    }

}
