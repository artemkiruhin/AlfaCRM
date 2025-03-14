using AlfaCRM.Domain.Interfaces.Database.Repositories;
using AlfaCRM.Domain.Models.Entities;
using AlfaCRM.Infrastructure.Repositories.Base;

namespace AlfaCRM.Infrastructure.Repositories;

public class PostReactionRepository(AppDbContext context) : BaseRepository<PostReactionEntity>(context), IPostReactionRepository;