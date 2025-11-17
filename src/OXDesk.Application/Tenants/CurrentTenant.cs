using OXDesk.Core.Tenants;

namespace OXDesk.Application.Tenants
{
    /// <summary>
    /// Default implementation of ICurrentTenant stored in request scope.
    /// </summary>
    public sealed class CurrentTenant : ICurrentTenant
    {
        private Guid? _id;
        private string? _schema;

        public Guid? Id => _id;
        public string? Schema => _schema;

        public void Change(Guid? tenantId, string? schema = null)
        {
            _id = tenantId;
            _schema = schema;
        }

        public IDisposable ChangeScoped(Guid? tenantId, string? schema = null)
        {
            var previousId = _id;
            var previousSchema = _schema;
            _id = tenantId;
            _schema = schema;
            return new Restore(() =>
            {
                _id = previousId;
                _schema = previousSchema;
            });
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
