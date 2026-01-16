GitHub Link: [https://github.com/ece-sen/SWEN3-BIF-5L4-GE.git](https://github.com/ece-sen/SWEN3-BIF-5L4-GE.git)



### Paperless – Critical Aspects Documentation



#### Architecture



* Layered architecture (Controller, Service, Repository)
* Clear separation of concerns
* Dependency Injection for loose coupling



#### Business Logic



* Validation and orchestration handled in service layer
* Controllers only handle HTTP concerns
* External systems accessed via interfaces



#### Persistence



* Entity Framework Core for data access
* In-memory database used for integration tests
* Database migrations only applied for relational databases



#### External Infrastructure



* Object storage, message queue, and search treated as infrastructure concerns
* Infrastructure execution controlled via environment configuration
* Disabled in integration test environment



#### Testing Strategy



* Unit tests with mocked dependencies
* Integration tests with full HTTP pipeline
* Custom test host using WebApplicationFactory
