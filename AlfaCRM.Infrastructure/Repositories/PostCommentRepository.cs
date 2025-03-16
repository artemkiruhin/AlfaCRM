using System.Linq.Expressions;
using AlfaCRM.Domain.Interfaces.Database.Repositories;
using AlfaCRM.Domain.Models.Entities;
using AlfaCRM.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace AlfaCRM.Infrastructure.Repositories;

public class PostCommentRepository(AppDbContext context) : BaseRepository<PostCommentEntity>(context), IPostCommentRepository
{
    public async Task<IEnumerable<PostCommentEntity>> GetDeletedCommentsAsync(Guid postId, CancellationToken ct)
    {
        return await DbSet.AsNoTracking().Where(postComment => postComment.PostId == postId).ToListAsync(ct);
    }
}