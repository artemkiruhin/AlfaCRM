﻿using AlfaCRM.Domain.Interfaces.Database.Repositories;
using AlfaCRM.Domain.Models.Entities;
using AlfaCRM.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace AlfaCRM.Infrastructure.Repositories;

public class PostRepository(AppDbContext context) : BaseRepository<PostEntity>(context), IPostRepository
{
    public async Task<IEnumerable<PostEntity>> GetImportantPostsAsync(
        CancellationToken ct,
        bool isImportant, 
        Guid? departmentId, 
        bool includeDeleted = false)
    {
        return await DbSet
            .AsNoTracking()
            .Where(post =>
                (post.IsImportant == isImportant) &&
                (departmentId == null || post.DepartmentId == departmentId) &&
                (includeDeleted || !post.IsActual)
            )
            .OrderByDescending(post => post.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<PostEntity>> GetActualPostsAsync(bool isActual, Guid? departmentId, CancellationToken ct)
    {
        return await DbSet.AsNoTracking().Where(post =>
            post.IsActual == isActual &&
            (departmentId == null || post.DepartmentId == departmentId)
        ).ToListAsync(ct);
    }

    public async Task<IEnumerable<PostEntity>> GetPostForDepartment(Guid departmentId, CancellationToken ct)
    {
        return await DbSet.AsNoTracking().Where(post => post.DepartmentId == departmentId).ToListAsync(ct);
    }

    public async Task<IEnumerable<PostEntity>> GetCommonPosts(CancellationToken ct)
    {
        return await DbSet.AsNoTracking().Where(post => post.DepartmentId == null).ToListAsync(ct);
    }
}