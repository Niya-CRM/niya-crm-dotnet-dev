using System;
using System.Collections.Generic;
using System.Linq;
using OXDesk.Core.Identity;

namespace OXDesk.Application.Identity
{
    /// <summary>
    /// Default implementation of <see cref="ICurrentUser"/> stored in request scope.
    /// </summary>
    public sealed class CurrentUser : ICurrentUser
    {
        private Guid? _id;
        private readonly HashSet<string> _roles = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);
        private string? _name;
        private string? _email;

        /// <inheritdoc />
        public Guid? Id => _id;

        /// <inheritdoc />
        public IReadOnlyCollection<string> Roles => _roles;

        /// <inheritdoc />
        public IReadOnlyCollection<string> Permissions => _permissions;

        /// <inheritdoc />
        public string? Name => _name;

        /// <inheritdoc />
        public string? Email => _email;

        /// <inheritdoc />
        public void Change(Guid? id, IEnumerable<string>? roles = null, IEnumerable<string>? permissions = null, string? name = null, string? email = null)
        {
            _id = id;
            _name = string.IsNullOrWhiteSpace(name) ? null : name;
            _email = string.IsNullOrWhiteSpace(email) ? null : email;

            _roles.Clear();
            if (roles != null)
            {
                foreach (var r in roles.Where(r => !string.IsNullOrWhiteSpace(r)))
                {
                    _roles.Add(r);
                }
            }

            _permissions.Clear();
            if (permissions != null)
            {
                foreach (var p in permissions.Where(p => !string.IsNullOrWhiteSpace(p)))
                {
                    _permissions.Add(p);
                }
            }
        }

        /// <inheritdoc />
        public IDisposable ChangeScoped(Guid? id, IEnumerable<string>? roles = null, IEnumerable<string>? permissions = null, string? name = null, string? email = null)
        {
            var prevId = _id;
            var prevRoles = _roles.ToArray();
            var prevPerms = _permissions.ToArray();
            var prevName = _name;
            var prevEmail = _email;

            Change(id, roles, permissions, name, email);

            return new Restore(() =>
            {
                _id = prevId;
                _name = prevName;
                _email = prevEmail;
                _roles.Clear();
                foreach (var r in prevRoles) _roles.Add(r);
                _permissions.Clear();
                foreach (var p in prevPerms) _permissions.Add(p);
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
