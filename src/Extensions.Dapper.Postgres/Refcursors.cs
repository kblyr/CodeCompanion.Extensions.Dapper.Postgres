using System.Collections;
using Dapper;
using Npgsql;

namespace CodeCompanion.Extensions.Dapper.Postgres
{
    public class Refcursors : IEnumerable<string>
    {
        private readonly NpgsqlConnection _connection;
        private readonly NpgsqlTransaction _transaction;
        private readonly IEnumerator<string> _enumerator;

        public Refcursors(NpgsqlConnection connection, NpgsqlTransaction transaction, IEnumerator<string> enumerator)
        {
            _connection = connection;
            _transaction = transaction;
            _enumerator = enumerator;
        }

        public IEnumerator<string> GetEnumerator() => _enumerator;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private bool MoveNext() => _enumerator.MoveNext();

        string GetCurrentScript() => GetFetchAllInScript(_enumerator.Current);

        public IEnumerable<T> Read<T>()
        {
            if (MoveNext())
                return _connection.Query<T>(GetCurrentScript(), transaction: _transaction);

            throw new NoRefcursorLeftException();
        }

        public IEnumerable<T> Read<T>(Type[] types, Func<object[], T> map)
        {
            if (MoveNext())
                return _connection.Query<T>(GetCurrentScript(), types, map, transaction: _transaction);

            throw new NoRefcursorLeftException();
        }

        public async Task<IEnumerable<T>> ReadAsync<T>()
        {
            if (MoveNext())
                return await _connection.QueryAsync<T>(GetCurrentScript(), transaction: _transaction);

            throw new NoRefcursorLeftException();
        }

        public async Task<IEnumerable<T>> ReadAsync<T>(Type[] types, Func<object[], T> map)
        {
            if (MoveNext())
                return await _connection.QueryAsync<T>(GetCurrentScript(), types, map, transaction: _transaction);

            throw new NoRefcursorLeftException();
        }

        public T ReadFirst<T>()
        {
            if (MoveNext())
                return _connection.QueryFirst<T>(GetCurrentScript(), transaction: _transaction);

            throw new NoRefcursorLeftException();
        }

        public T ReadFirst<T>(Type[] types, Func<object[], T> map) => Read<T>(types, map).First();

        public async Task<T> ReadFirstAsync<T>()
        {
            if (MoveNext())
                return await _connection.QueryFirstAsync<T>(GetCurrentScript(), transaction: _transaction);
            
            throw new NoRefcursorLeftException();
        }

        public async Task<T> ReadFirstAsync<T>(Type[] types, Func<object[], T> map) => (await ReadAsync<T>(types, map)).First();

        public T ReadSingle<T>()
        {
            if (MoveNext())
                return _connection.QuerySingle<T>(GetCurrentScript(), transaction: _transaction);

            throw new NoRefcursorLeftException();
        }

        public T ReadSingle<T>(Type[] types, Func<object[], T> map) => Read<T>(types, map).Single();

        public async Task<T> ReadSingleAsync<T>()
        {
            if (MoveNext())
                return await _connection.QuerySingleAsync<T>(GetCurrentScript(), transaction: _transaction);
            
            throw new NoRefcursorLeftException();
        }

        public async Task<T> ReadSingleAsync<T>(Type[] types, Func<object[], T> map) => (await ReadAsync<T>(types, map)).Single();

        public T ReadFirstOrDefault<T>()
        {
            if (MoveNext())
                return _connection.QueryFirstOrDefault<T>(GetCurrentScript(), transaction: _transaction);

            throw new NoRefcursorLeftException();
        }

        public T? ReadFirstOrDefault<T>(Type[] types, Func<object[], T> map) => Read<T>(types, map).FirstOrDefault();

        public async Task<T> ReadFirstOrDefaultAsync<T>()
        {
            if (MoveNext())
                return await _connection.QueryFirstOrDefaultAsync<T>(GetCurrentScript(), transaction: _transaction);
            
            throw new NoRefcursorLeftException();
        }

        public async Task<T?> ReadFirstOrDefaultAsync<T>(Type[] types, Func<object[], T> map) => (await ReadAsync<T>(types, map)).FirstOrDefault();

        public T? ReadSingleOrDefault<T>()
        {
            if (MoveNext())
                return _connection.QuerySingleOrDefault<T>(GetCurrentScript(), transaction: _transaction);

            throw new NoRefcursorLeftException();
        }

        public T? ReadSingleOrDefault<T>(Type[] types, Func<object[], T> map) => Read<T>(types, map).SingleOrDefault();

        public async Task<T?> ReadSingleOrDefaultAsync<T>()
        {
            if (MoveNext())
                return await _connection.QuerySingleOrDefaultAsync<T>(GetCurrentScript(), transaction: _transaction);
            
            throw new NoRefcursorLeftException();
        }

        public async Task<T?> ReadSingleOrDefaultAsync<T>(Type[] types, Func<object[], T> map) => (await ReadAsync<T>(types, map)).SingleOrDefault();

        static string GetFetchAllInScript(string refcursorName) => $"FETCH ALL IN \"{refcursorName}\"";
    }
}