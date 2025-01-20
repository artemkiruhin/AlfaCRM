namespace AlfaCRM.Domain.Interfaces.Services.Mapper;

public interface IMapper<TEntity, TDto> where TDto : class
{
    TDto ToDto(TEntity entity);
    TEntity ToEntity(TDto dto);
}