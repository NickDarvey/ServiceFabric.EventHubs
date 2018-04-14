using System.Collections.Generic;

namespace NickDarvey.ServiceFabric.EventHubs
{
    internal static class Loops
    {
        public static IEnumerable<Unit> Infinite()
        {
            while (true)
            {
                yield return Unit.Default;
            }
        }
    }
}
