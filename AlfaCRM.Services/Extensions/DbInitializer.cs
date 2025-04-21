using AlfaCRM.Domain.Interfaces.Services.Security;
using AlfaCRM.Domain.Models.Entities;
using AlfaCRM.Infrastructure;

namespace AlfaCRM.Services.Extensions;

public static class DbInitializer
{
    public static int Initialize(AppDbContext context, IHashService hasher)
    {
        if (context.Users.Any() || context.Departments.Any() || context.Posts.Any())
        {
            context.Users.RemoveRange(context.Users);
            context.Posts.RemoveRange(context.Posts);
            context.PostReactions.RemoveRange(context.PostReactions);
            context.PostComments.RemoveRange(context.PostComments);
            context.Departments.RemoveRange(context.Departments);
            context.Tickets.RemoveRange(context.Tickets);
            context.Messages.RemoveRange(context.Messages);
            context.Chats.RemoveRange(context.Chats);
            context.SaveChanges();
        }
        
        var departments = new List<DepartmentEntity>
        {
            DepartmentEntity.Create("Отдел разработки", true),
            DepartmentEntity.Create("Отдел тестирования", true),
            DepartmentEntity.Create("Отдел маркетинга", false),
            DepartmentEntity.Create("Отдел продаж", false),
            DepartmentEntity.Create("Отдел поддержки", false)
        };

        context.Departments.AddRange(departments);
        context.SaveChanges();
        
        var users = new List<UserEntity>
        {
            UserEntity.Create(
                fullName: "Иванов Иван Иванович",
                email: "ivanov@example.com",
                username: "ivanov",
                passwordHash: hasher.ComputeHash("1"),
                hiredAt: DateTime.UtcNow,
                birthday: new DateTime(1990, 1, 1).ToUniversalTime(),
                isMale: true,
                isAdmin: true,
                hasPublishedRights: true,
                departmentId: departments[0].Id
            ),
            UserEntity.Create(
                fullName: "Петров Петр Петрович",
                email: "petrov@example.com",
                username: "petrov",
                passwordHash: hasher.ComputeHash("1"),
                hiredAt: DateTime.UtcNow,
                birthday: new DateTime(1985, 5, 15).ToUniversalTime(),
                isMale: true,
                isAdmin: false,
                hasPublishedRights: true,
                departmentId: departments[1].Id
            ),
            UserEntity.Create(
                fullName: "Сидорова Мария Серафимовна",
                email: "sidorova@example.com",
                username: "sidorova",
                passwordHash: hasher.ComputeHash("1"),
                hiredAt: DateTime.UtcNow,
                birthday: new DateTime(1992, 8, 20).ToUniversalTime(),
                isMale: false,
                isAdmin: false,
                hasPublishedRights: false,
                departmentId: departments[2].Id
            )
        };

        context.Users.AddRange(users);
        context.SaveChanges();
        
        var posts = new List<PostEntity>
        {
            PostEntity.Create(
                title: "Новый проект",
                subtitle: "Запуск нового проекта",
                content: "Мы начинаем новый проект, который изменит мир!",
                isImportant: true,
                departmentId: departments[0].Id,
                publisherId: users[0].Id
            ),
            PostEntity.Create(
                title: "Тестирование новой версии",
                subtitle: "Результаты тестирования",
                content: "Новая версия прошла все тесты успешно.",
                isImportant: false,
                departmentId: departments[1].Id,
                publisherId: users[1].Id
            ),
            PostEntity.Create(
                title: "Маркетинговая стратегия",
                subtitle: "Новая стратегия",
                content: "Мы разработали новую маркетинговую стратегию.",
                isImportant: true,
                departmentId: departments[2].Id,
                publisherId: users[2].Id
            )
        };

        context.Posts.AddRange(posts);
        return context.SaveChanges();
    }
}