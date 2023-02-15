
[20230206] ReaderWriterLock 구현 연습  
[20230206] Thread Local Storage(TLS) 구현 연습  
[20230207] Onblocking, 즉 Async비동기 방식 연습  
[20230208_1] Session작성, Receive를 비동기로.  
[20230208_2] Session작성, send를 비동기로.  
[20230209_1] Recv Arg 수정, _pendingList 생성.  
[20230209_2] EventHandler 추가, abstract class Session 추상화.  Session만들고 accept하는 부분을 Program -> Listener로(코어서버단으로) 옮김. Action<Socket>onAcceptHandler -> Func<Session> _sessionFactory. 
onAcceptHandler삭제(session으로 대신함.)  
[20230209_3] 서버코어에 Listener의 반대 역할을 하는 Connector 추가. 분산서버에서 다른 서버와 통신하기 위해서 필요함. 'ServerCore'-속성-출력유형(콘솔어플리케이션->클래스라이브러리)변경. 'Server'-추가-프로젝트참조(ServerCore)', 'DummyClient'-추가-프로젝트참조(ServerCore).
ServerCore Program -> Server Program 코드 이전. ServerCore Session클래스를 public화. Listener클래스를 public화. Connector클래스 public화. 솔루션 속성-여러개의시작프로젝트-(DummyClient,Server). DummyClient의 program스크립트에도 GameSession클래스 복사.  
[20230209_4] RecvBuffer 개선.  OnRecv: void -> int  
[20230209_5] SendBuffer 작성.  Session 외부에서 만듬. 내부에서 만들면 100명 유저가 각각 나머지 99명 유저에게 보내야 하므로 거의 만번을 보내지만, 외부에서 만들면 외부에서 100명 유저에게 총 100번만 보내면 되기 때문. SendBuffer의 크기는 어떻게 정할까? 가변 사이즈의 리스트라도 들어가면 어떻게 할까? 
아주 큰 덩어리를 만들고, 그걸 조금씩 잘라서 사용하면 효율적일 것. 
Server-Program-Session: knight class 추가. ServerCore-Session-Send인풋인자를 byte[]가 아니라 ArraySegment<byte>로 수정. _sendQueue도 ArraySegment<byte>로 수정.  
[20230209_6] ServerCore: Session-Session(Class)->PacketSession(Class)로 수정. Server: Program-GameSession(Class)가Session상속받던것을->PacketSession을 상속받도록 수정. kight클래스{hp, attack}을 Packet클래스{size, packetId}로 수정.   
[20230210_1] [Serialization] DummyClient: ServerSession(Class)추가. 기존 Programs(Class)내 패킷통신 부분을 이동함. Server: ClientSession(Class)추가. 기존 Programs(Class)내 패킷통신 부분을 이동함.  
[20230210_2] [Serialization] DummyClient-ServerSession: BitConverter.GetBytes+Array.Copy대신에 BitConverter.TryWriteBytes사용함. success여부를 리턴값으로 알 수 있고 빠름.
[20230210_3] [Serialization] Serialization의 packet write, read함수화. 패킷 변조 방지용 ReadOnlySpan<byte>함수.  
[20230210_4] [Serialization] string 가변적인 정보를 위한 작업. sizeof(ushort). span.Slice(). Encoding.Unicode.GetByteCount(), GetBytes(), GetString().  
[20230210_5] [Serialization] Instance buffer를 만들어서 거기에 add를 해서 패킷을 보냈음. flatbuffer를 이용하면 데이터를 배열에 바로 넣도록 하지만, 우리의 방법은 중간의 Instance를 만들어서 거기에 넣는 방법임. 코딩과 관리에 편함.  
[20230210_6] [Packet Generator] 자동화.   Tools폴더 내에 PacketGenerator 프로젝트를 생성. PDL.xml 새항목 추가.(Packet Definition List라고 하자.). Program에 ParsePacket(XmlReader r), ParseMembers(XmlReader r) 작성. PacketFormat새항목 추가, 작성중.  
[20230210_7] [Packet Generator] ServerSession Read Write 함수 템플릿화, 변수 이름 자동화 중. at PacketFormat.cs
[20230210_8] [Packet Generator] PacketGenerator - Program: File.WriteAllText(genPackets)로 파싱한 내용을 추가함. 지금 에러상태임. GenPackets.cs의 내용이 자동생성 안됨.  
[20230212_1] [Packet Generator] GenPacket.cs가 생성 안되는 에러. Error메시지: "Unhandled exception. System.FormatException: Input string was not in a correct format.". Error슈팅: PacketFormat.cs에서 변수를 지정해줄 때 {}중괄호를 {]로 오타. Error슈팅결과: GenPackets.cs의 내용이 자동생성 됨.  
[20230212_2] [Packet Generator] Tab정렬.  
[20230212_3] [Packet Generator] List parsing작성, 패킷제너레이터 사용, 테스트.  
[20230213_1] [Packet Generator] byte, sbyte타입 추가함.  
[20230213_2] [Packet Generator] PDL.xml: list안에 list가 들어가는 경우(예를 들어 캐릭터 속성)를 추가. ERROR 발생.
[20230213_3] [Packet Generator] ERROR 해결. ThrowFormatError()로부터 원인을 추측함. PacketFormat.cs에서 //주석부분에 {}중괄호를 넣었는데 이걸 인식해버려서 에러났음.. 해결법: 주석에서 { 뺐음.  
[20230213_4] [Packet Generator] 이중리스트도 자동으로 코드 생성이 가능함. struct List를 class List로 변경함.   
[20230213_5] [Packet Generator] 1. PacketGenerator-Program.cs: PDL.xml의 경로를 string인자로 받도록 수정. 경로는 ../../../PDL.xml.  
2.Commons-GenPackets.bat: 배치파일을 만들어서 exe를 클릭하는 일을 시킴. START의 인자로 PDL.xml의 경로를 넣어줌.  
3. 생성된 GenPackets.cs를 DummyClient-Packet-GenPackets.cs, Server-Packet-GenPackets.cs으로 자동 복사 갱신되도록 함. XCOPY에 /Y 옵션으로 덮어쓰기 허락함.  
[20230213_6] [Packet Generator] OnRecvPacket()자동화를 위해서 IPacket을 공통 인수로 가지도록 하고 싶음. 그래서 PacketFormat.cs: interface IPacket 만들어서 GenPackets.cs의 모든 패킷은 IPacket로 구현되도록 함.  
Server-Packet-PacketHandler.cs추가, Server-Packet-PacketManager.cs추가. PacketManager는 싱글톤(static 인스턴스를 하나만 사용할 것을 상정하고 만듬).  
그리고 Server-Program.cs가 맨 처음 실행될 때(아직 멀티쓰레드가 아닐 때) 단 한번만 PacketManager Instance의 Register()함수를 실행해서 _onRecv딕셔너리(dictionary)에는 패킷클래스들을, _handler딕셔너리(dictionary)에는 그 패킷클래스 타입의 패킷을 invoke하기 위한 핸들러들을 담는다(add).  
추후에 OnRecvPacket(session, buffer)함수가 수시로 호출될 때 핸들러가 패킷프로토콜ID에 맞는 값을 참조해서 읽는다.  
[20230214_1] [Packet Generator] Automate PacketManager.  
[20230214_2] [Packet Generator] Server-ServerPacketManager: only get packets from Client with "C_". DummyClient-ClientPacketManager: only get packets from Server with "S_".  
[20230214_3] [Chatting Test]  
[20230214_4] [JobQueue]  
[20230214_5] [JobQueue] 락을 이용해서 일감을 할당하는 경우는 만일 쓰레드가 너무 많이 생성될 경우 병목현상이 생겨서 싱글쓰레드일때보다도 일을 못하게 될 수 있다. 그래서 Queue를 만들고 일감을 Queue에 넣어서 순차적으로 일감을 할당해주는 것이 더 좋고 중요하다.  그러나 심리스seamless 게임(와우, 리니지)의 경우는 map region에 따라 JobQueue를 만들 수는 없고, 몬스터, 오브젝 등에게 JobQueue를 만든다.  [TaskQueue]작성,테스트  
[20230215_1] [TaskQueue] 삭제, [패킷모아보내기][JobTimer]  
[20230215_2] Unity 연동  





