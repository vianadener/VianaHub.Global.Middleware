using RT.Comb;

namespace VianaHub.Global.Middleware.Lib.Extensions;

public static class MethodExtensions
{
    // Gera um Comb como Guid indexável (sequencial)
    public static Guid OrderGuid()
    {
        var provider = Provider.Sql;
        return provider.Create(DateTime.UtcNow);
    }
}
