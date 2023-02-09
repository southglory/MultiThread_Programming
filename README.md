
[20230206] ReaderWriterLock 구현 연습  
[20230206] Thread Local Storage(TLS) 구현 연습  
[20230207] Onblocking, 즉 Async비동기 방식 연습  
[20230208] Session작성, Receive를 비동기로.  
[20230208] Session작성, send를 비동기로.  
[20230209] Recv Arg 수정, _pendingList 생성.  
[20230209] EventHandler 추가, abstract class Session 추상화.  Session만들고 accept하는 부분을 Program -> Listener로(코어서버단으로) 옮김. Action<Socket>onAcceptHandler -> Func<Session> _sessionFactory. 
onAcceptHandler삭제(session으로 대신함.)  
[20230209] 서버코어에 Listener의 반대 역할을 하는 Connector 추가. 분산서버에서 다른 서버와 통신하기 위해서 필요함. 'ServerCore'-속성-출력유형(콘솔어플리케이션->클래스라이브러리)변경. 'Server'-추가-프로젝트참조(ServerCore)', 'DummyClient'-추가-프로젝트참조(ServerCore). ''DummyClient'-추가-프로젝트참조(ServerCore)'
ServerCore Program -> Server Program 코드 이전. ServerCore Session클래스를 public화. Listener클래스를 public화. Connector클래스 public화. 솔루션 속성-여러개의시작프로젝트-(DummyClient,Server). DummyClient의 program스크립트에도 GameSession클래스 복사.