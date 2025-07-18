# Muhaberat Evrak Yönetim Sistemi Class Diagram

Below is a simplified MVC-focused class diagram for the application.

```mermaid
classDiagram
    direction TB

    class HomeController {
        +Index()
        +Privacy()
        +Error()
    }

    class ErrorViewModel {
        string RequestId
        bool ShowRequestId
    }

    class Document {
        int DocumentId
        string DocumentNumber
        string Title
        string Description
        int DocumentTypeId
        int SenderUserId
        int ReceiverUserId
        int SenderDepartmentId
        int ReceiverDepartmentId
    }

    class DocumentType {
        int DocumentTypeId
        string TypeName
        string TypeCode
    }

    class User {
        int UserId
        string Username
        string Email
        string PasswordHash
        int DepartmentId
        int RoleId
    }

    class Department {
        int DepartmentId
        string DepartmentName
        string DepartmentCode
    }

    class Role {
        int RoleId
        string RoleName
        string RoleCode
    }

    HomeController --> Document : uses
    HomeController --> ErrorViewModel : returns
    Document --> DocumentType
    Document --> User : Sender
    Document --> User : Receiver
    User --> Department
    User --> Role
```

This diagram focuses on the main classes and their relationships without introducing additional layers or services. It represents an MVC structure where the `HomeController` interacts directly with models and views.
