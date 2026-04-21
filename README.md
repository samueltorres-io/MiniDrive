# MiniDrive
Dotnet project of a Mini Drive


---

Rotas:

Users
    POST /api/users — criar
    GET  /api/users/{id} — obter

Folders
    POST /api/folders — criar (body: userId, name, parentId?)
    GET  /api/folders?userId=&parentId= — listar (serve pra navegar pastas)
    DELETE /api/folders/{id} — soft delete

Files (3)
    POST /api/files — upload (multipart: userId, folderId?, arquivo)
    GET  /api/files?userId=&folderId= — listar
    GET  /api/files/{id}/download — baixar o conteúdo
    DELETE /api/files/{id} — soft delete