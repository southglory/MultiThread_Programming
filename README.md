
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
[20230210_4] [Serialization] string 가변적인 정보를 위한 작업. sizeof(ushort). span.Slice(). Encoding.Unicode.GetByteCount(), GetBytes(), GetString()  
[20230210_5] [Serialization] Instance buffer를 만들어서 거기에 add를 해서 패킷을 보냈음. flatbuffer를 이용하면 데이터를 배열에 바로 넣도록 하지만, 우리의 방법은 중간의 Instance를 만들어서 거기에 넣는 방법임. 코딩과 관리에 편함.  
