# CK_Chess_Server
![Concept Screenshot 01](./ScreenShots/2025-06-18_19-51-58%20(3840x2160).png)

![Concept Screenshot 02](./ScreenShots/2025-06-18_19-52-28%20(3840x2160).png)
## 0. 프로젝트 개요
### 개요
청강문화산업대학교 2학년 1학기 게임네트워크프로그래밍 기말과제 (간단한 네트워크 게임 개발하기)

멀티플레이 체스 게임
### 링크
- Server: [hwahyang1/CK_Chess_Server](https://github.com/hwahyang1/CK_Chess_Server)
- Client: [hwahyang1/CK_Chess_Client](https://github.com/hwahyang1/CK_Chess_Client)
### Tech Stack & Libraries
- C# (.NET 8)
- MySql.Data
- StackExchange.Redis
### 1. 프로젝트 요구사항
1. Install MySQL, Redis
2. rename `Config.cs.SAMPLE` to `Config.cs`
3. edit `Config.cs`
4. Build & Run
## 2. 체스 게임을 채택한 이유
학기 초부터 기말과제에 대한 내용을 듣고, 여러 게임들을 생각했었다.

하지만 과제 제출 시한인 6월 중에 내가 시간이 없을 것 같았고, 상대적으로 구현하기 쉬운 '체스 게임'을 채택하였다.
## 3. 미완성?
예상한 것보다 더 바빴다.

과제에 필요한 시간을 확보하기 어려웠고, 결국 '서버라도 완성하자'라는 생각으로 서버만 마무리 지었다.
## 4. 동작 구조
모든 요청은 서버를 거친다.

게임 특성상 정적인 턴제 게임이기에, 서버에 모든 연산을 맡겨도 서버에 부하가 크지 않고 유저도 지연 시간이 있더라도 납득 가능할 것이라 생각하였다.
## 5. 면책
기본적인 체스 룰은 [나무위키 '체스' 문서](https://namu.wiki/w/%EC%B2%B4%EC%8A%A4)를 참고하였습니다.

특수 행마법에서 '캐슬링'과 '앙파상' 기법은 구현하지 않았습니다.
## 6. LICENSE
GPLv3
