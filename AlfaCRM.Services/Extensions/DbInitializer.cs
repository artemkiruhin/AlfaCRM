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
            context.Logs.RemoveRange(context.Logs);
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
                hiredAt: DateTime.UtcNow.AddYears(-3),
                birthday: new DateTime(1988, 5, 12).ToUniversalTime(),
                isMale: true,
                isAdmin: true,
                hasPublishedRights: true,
                departmentId: departments[0].Id
            ),
            UserEntity.Create(
                fullName: "Корочаев Дмитрий Сергеевич",
                email: "korochaev@example.com",
                username: "korochaev",
                passwordHash: hasher.ComputeHash("1"),
                hiredAt: DateTime.UtcNow.AddYears(-2),
                birthday: new DateTime(1990, 7, 21).ToUniversalTime(),
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
                hiredAt: DateTime.UtcNow.AddYears(-4),
                birthday: new DateTime(1985, 3, 15).ToUniversalTime(),
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
                hiredAt: DateTime.UtcNow.AddMonths(-8),
                birthday: new DateTime(1992, 11, 30).ToUniversalTime(),
                isMale: false,
                isAdmin: false,
                hasPublishedRights: false,
                departmentId: departments[2].Id
            ),
            UserEntity.Create(
                fullName: "Смирнова Анна Владимировна",
                email: "smirnova@example.com",
                username: "smirnova",
                passwordHash: hasher.ComputeHash("1"),
                hiredAt: DateTime.UtcNow.AddYears(-1),
                birthday: new DateTime(1991, 2, 14).ToUniversalTime(),
                isMale: false,
                isAdmin: false,
                hasPublishedRights: true,
                departmentId: departments[3].Id
            ),
            UserEntity.Create(
                fullName: "Кузнецов Алексей Дмитриевич",
                email: "kuznetsov@example.com",
                username: "kuznetsov",
                passwordHash: hasher.ComputeHash("1"),
                hiredAt: DateTime.UtcNow.AddMonths(-10),
                birthday: new DateTime(1987, 9, 5).ToUniversalTime(),
                isMale: true,
                isAdmin: false,
                hasPublishedRights: false,
                departmentId: departments[4].Id
            ),
            UserEntity.Create(
                fullName: "Новикова Екатерина Игоревна",
                email: "novikova@example.com",
                username: "novikova",
                passwordHash: hasher.ComputeHash("1"),
                hiredAt: DateTime.UtcNow.AddYears(-2),
                birthday: new DateTime(1993, 4, 18).ToUniversalTime(),
                isMale: false,
                isAdmin: true,
                hasPublishedRights: true,
                departmentId: departments[1].Id
            ),
            UserEntity.Create(
                fullName: "Васильев Михаил Олегович",
                email: "vasiliev@example.com",
                username: "vasiliev",
                passwordHash: hasher.ComputeHash("1"),
                hiredAt: DateTime.UtcNow.AddMonths(-6),
                birthday: new DateTime(1989, 12, 3).ToUniversalTime(),
                isMale: true,
                isAdmin: false,
                hasPublishedRights: true,
                departmentId: departments[0].Id
            ),
            UserEntity.Create(
                fullName: "Федорова Ольга Сергеевна",
                email: "fedorova@example.com",
                username: "fedorova",
                passwordHash: hasher.ComputeHash("1"),
                hiredAt: DateTime.UtcNow.AddYears(-1),
                birthday: new DateTime(1994, 6, 25).ToUniversalTime(),
                isMale: false,
                isAdmin: false,
                hasPublishedRights: false,
                departmentId: departments[2].Id
            ),
            UserEntity.Create(
                fullName: "Григорьев Артем Викторович",
                email: "grigoriev@example.com",
                username: "grigoriev",
                passwordHash: hasher.ComputeHash("1"),
                hiredAt: DateTime.UtcNow.AddMonths(-4),
                birthday: new DateTime(1995, 8, 8).ToUniversalTime(),
                isMale: true,
                isAdmin: false,
                hasPublishedRights: false,
                departmentId: departments[3].Id
            )
        };

        context.Users.AddRange(users);
        context.SaveChanges();
        
        var posts = new List<PostEntity>
        {
            PostEntity.Create(
                title: "Запуск нового проекта",
                subtitle: "Разработка CRM системы",
                content: "Мы начинаем разработку новой CRM системы под кодовым названием 'Альфа'. Проект обещает быть революционным в своей области.",
                isImportant: true,
                departmentId: departments[0].Id,
                publisherId: users[0].Id
            ),
            PostEntity.Create(
                title: "Результаты тестирования",
                subtitle: "Успешное завершение QA",
                content: "Новая версия продукта успешно прошла все этапы тестирования и готова к релизу.",
                isImportant: false,
                departmentId: departments[1].Id,
                publisherId: users[1].Id
            ),
            PostEntity.Create(
                title: "Новая маркетинговая стратегия",
                subtitle: "Повышение узнаваемости бренда",
                content: "Утверждена новая маркетинговая стратегия на следующий квартал. Основной фокус - повышение узнаваемости бренда.",
                isImportant: true,
                departmentId: departments[2].Id,
                publisherId: users[2].Id
            ),
            PostEntity.Create(
                title: "Обновление офисного оборудования",
                subtitle: "Новые компьютеры для разработчиков",
                content: "На следующей неделе будет произведена замена компьютеров в отделе разработки на более мощные модели.",
                isImportant: true,
                departmentId: departments[0].Id,
                publisherId: users[0].Id
            ),
            PostEntity.Create(
                title: "Корпоративное мероприятие",
                subtitle: "Встреча с руководством",
                content: "В пятницу состоится общая встреча с руководством компании, где будут обсуждаться планы на следующий год.",
                isImportant: false,
                departmentId: departments[3].Id,
                publisherId: users[1].Id
            ),
            PostEntity.Create(
                title: "Обновление политики безопасности",
                subtitle: "Новые требования к паролям",
                content: "С понедельника вводятся новые требования к сложности паролей. Минимальная длина - 12 символов.",
                isImportant: true,
                departmentId: departments[4].Id,
                publisherId: users[2].Id
            ),
            PostEntity.Create(
                title: "Успехи отдела продаж",
                subtitle: "Рекордные показатели",
                content: "Отдел продаж достиг рекордных показателей в этом месяце - 150% от плана!",
                isImportant: false,
                departmentId: departments[3].Id,
                publisherId: users[1].Id
            ),
            PostEntity.Create(
                title: "Обучение новым технологиям",
                subtitle: "Курсы по машинному обучению",
                content: "Для всех желающих сотрудников отдела разработки будут организованы курсы по основам машинного обучения.",
                isImportant: false,
                departmentId: departments[0].Id,
                publisherId: users[0].Id
            ),
            PostEntity.Create(
                title: "Обновление системы поддержки",
                subtitle: "Внедрение нового тикет-системы",
                content: "Следующей неделе мы переходим на новую систему обработки запросов пользователей. Все сотрудники пройдут обучение.",
                isImportant: true,
                departmentId: departments[4].Id,
                publisherId: users[2].Id
            ),
            PostEntity.Create(
                title: "Праздничные дни",
                subtitle: "График работы в праздники",
                content: "Напоминаем, что с 30 декабря по 8 января включительно офис будет закрыт. С 9 января работаем в обычном режиме.",
                isImportant: false,
                departmentId: departments[2].Id,
                publisherId: users[1].Id
            )
        };

        context.Posts.AddRange(posts);
        context.SaveChanges();

        var tickets = new List<TicketEntity>
{
    TicketEntity.Create(
        title: "Не работает принтер в 305 кабинете",
        text: "Здравствуйте, коллеги! У нас в кабинете 305 сломался принтер. Просто не печатает, хотя индикаторы горят. Проверьте пожалуйста!",
        status: TicketStatus.Created,
        departmentId: departments[0].Id,
        creatorId: users[0].Id,
        type: TicketType.ProblemCase,
        feedback: null,
        assigneeId: null
    ),
    TicketEntity.Create(
        title: "Предложение по улучшению системы отчетов",
        text: "Предлагаю добавить возможность экспорта отчетов в PDF формате прямо из интерфейса системы. Это сэкономит время сотрудников.",
        status: TicketStatus.InWork,
        departmentId: departments[1].Id,
        creatorId: users[2].Id,
        type: TicketType.Suggestion,
        feedback: "Идея интересная, передано на рассмотрение разработчикам",
        assigneeId: users[5].Id
    ),
    TicketEntity.Create(
        title: "Не работает интернет в западном крыле",
        text: "На втором этаже в западном крыле полностью отсутствует интернет-соединение уже второй день. Прошу устранить проблему.",
        status: TicketStatus.InWork,
        departmentId: departments[4].Id,
        creatorId: users[3].Id,
        type: TicketType.ProblemCase,
        feedback: "Принято в работу, специалист выедет сегодня",
        assigneeId: users[7].Id
    ),
    TicketEntity.Create(
        title: "Замена устаревших компьютеров",
        text: "Компьютеры в нашем отделе уже морально устарели и не справляются с текущими задачами. Прошу рассмотреть возможность их замены.",
        status: TicketStatus.Completed,
        departmentId: departments[0].Id,
        creatorId: users[1].Id,
        type: TicketType.ProblemCase,
        feedback: "Закупка новых компьютеров запланирована на следующий квартал",
        assigneeId: users[6].Id
    ),
    TicketEntity.Create(
        title: "Идея для корпоративного мероприятия",
        text: "Предлагаю организовать выездной тимбилдинг на природе с профессиональными тренерами. Это улучшит командный дух.",
        status: TicketStatus.Rejected,
        departmentId: departments[3].Id,
        creatorId: users[4].Id,
        type: TicketType.Suggestion,
        feedback: "В этом году бюджет на мероприятия уже исчерпан, предложение отклонено",
        assigneeId: users[8].Id
    ),
    TicketEntity.Create(
        title: "Проблема с кондиционером",
        text: "В кабинете 412 кондиционер работает на полную мощность и его невозможно выключить. Температура опустилась до 16 градусов.",
        status: TicketStatus.Created,
        departmentId: departments[2].Id,
        creatorId: users[9].Id,
        type: TicketType.ProblemCase,
        feedback: null,
        assigneeId: null
    ),
    TicketEntity.Create(
        title: "Предложение по оптимизации процессов",
        text: "Можно автоматизировать процесс согласования документов, внедрив электронную подпись. Это сократит время обработки на 30%.",
        status: TicketStatus.InWork,
        departmentId: departments[1].Id,
        creatorId: users[5].Id,
        type: TicketType.Suggestion,
        feedback: "Передано в отдел разработки для оценки",
        assigneeId: users[0].Id
    )
};
        
        context.Tickets.AddRange(tickets);
        return context.SaveChanges();
    }
}