using ApiCadastro.Model;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.ChangeTracking;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//public class StubDbSet<T> : DbSet<T>, IQueryable, IEnumerable<T> where T : class
//{
//    private readonly List<T> _data;

//    public StubDbSet(List<T> data)
//    {
//        _data = data;
//    }

//    public override Task<T> FindAsync(params object[] keyValues)
//    {
//        var item = _data.FirstOrDefault(e => e is User user && user.Id == (int)keyValues[0]);
//        return Task.FromResult(item);
//    }

//    public override ValueTask<T> AddAsync(T entity, CancellationToken cancellationToken = default)
//    {
//        _data.Add(entity);
//        return new ValueTask<T>(entity);
//    }

//    public override IQueryable<T> AsQueryable() => _data.AsQueryable();

//    // Outros métodos podem ser adicionados conforme necessário
//}