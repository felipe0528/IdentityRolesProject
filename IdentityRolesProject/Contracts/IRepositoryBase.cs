using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityRolesProject.Contracts
{
    public interface IRepositoryBase<T> where T : class
    {
        Task<ICollection<T>> FindAll();
        Task<T> FindById(string id);
        Task<bool> isExists(string id);
        Task<string> Create(T entity);
        Task<string> Update(T entity);
        Task<bool> Delete(T entity);
        Task<bool> Save();
    }
}
