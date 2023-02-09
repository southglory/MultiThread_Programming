
[20230206] ReaderWriterLock 구현 연습  
[20230206] Thread Local Storage(TLS) 구현 연습  
[20230207] Onblocking, 즉 Async비동기 방식 연습  
[20230208] Session작성, Receive를 비동기로.  
[20230208] Session작성, send를 비동기로.  
[20230209] Recv Arg 수정, _pendingList 생성.  
[20230209] EventHandler 추가, abstract class Session 추상화.  Session만들고 accept하는 부분을 Program -> Listener로(코어서버단으로) 옮김. Action<Socket>onAcceptHandler -> Func<Session> _sessionFactor.   
onAcceptHandler삭제(session으로 대신함.)