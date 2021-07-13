using System;
using Core.Ids;

namespace Core.Testing
{
    public class FakeIdGenerator : IIdGenerator
    {
        public Guid? LastGeneratedId { get; private set; }
        public Guid New() => (LastGeneratedId = Guid.NewGuid()).Value;
    }
}
