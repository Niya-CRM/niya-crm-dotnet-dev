using OXDesk.Core.Tenants;

namespace OXDesk.Application.Tenants
{
    /// <summary>
    /// Default implementation of ICurrentTenant stored in request scope.
    /// </summary>
    public sealed class CurrentTenant : ICurrentTenant
    {
        private int? _id;

        public int? Id => _id;

        public void Change(int? tenantId)
        {
            _id = tenantId;
        }

        public IDisposable ChangeScoped(int? tenantId)
        {
            var previous = _id;
            _id = tenantId;
            return new Restore(() => _id = previous);
        }

        private sealed class Restore : IDisposable
        {
            private Action? _restore;
            public Restore(Action restore) => _restore = restore;
            public void Dispose()
            {
                _restore?.Invoke();
                _restore = null;
            }
        }
    }
}
