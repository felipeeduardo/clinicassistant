using ClinicAssistant.Application.Conversations;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ClinicAssistant.Infrastructure.Conversations;

public sealed class RedisConversationLock(IConnectionMultiplexer connectionMultiplexer, IOptions<ConversationOptions> options) : IConversationLockManager
{
    private readonly ConversationOptions _options = options.Value;

    public async Task<IConversationLockHandle?> TryAcquireAsync(Guid tenantId, Guid conversationId, CancellationToken cancellationToken)
    {
        var database = connectionMultiplexer.GetDatabase();
        var key = new RedisKey($"clinicassistant:conversation-lock:{tenantId:N}:{conversationId:N}");
        var token = Guid.NewGuid().ToString("N");
        var deadline = DateTimeOffset.UtcNow.AddSeconds(_options.LockTimeoutSeconds);
        while (DateTimeOffset.UtcNow < deadline)
        {
            var acquired = await database.StringSetAsync(key, token, TimeSpan.FromSeconds(_options.LockTtlSeconds), When.NotExists).WaitAsync(cancellationToken);
            if (acquired) return new RedisConversationLockHandle(database, key, token);
            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        }
        return null;
    }

    private sealed class RedisConversationLockHandle(IDatabase database, RedisKey key, RedisValue token) : IConversationLockHandle
    {
        private const string ReleaseScript = "if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('del', KEYS[1]) else return 0 end";

        public async ValueTask DisposeAsync() =>
            await database.ScriptEvaluateAsync(ReleaseScript, [key], [token]);
    }
}
