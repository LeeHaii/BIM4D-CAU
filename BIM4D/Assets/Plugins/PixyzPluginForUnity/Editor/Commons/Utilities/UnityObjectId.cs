using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pixyz.Commons.Utilities
{
    /// <summary>
    /// Maps Unity entity IDs to the 32-bit IDs required by the legacy Pixyz native API.
    /// IDs are shared by import and optimization and remain stable until domain reload.
    /// </summary>
    internal static class UnityObjectId
    {
        private static readonly Dictionary<EntityId, uint> NativeIds = new Dictionary<EntityId, uint>();
        private static uint nextId;

        internal static uint GetId(UnityEngine.Object obj)
        {
            var entityId = obj.GetEntityId();
            lock (NativeIds)
            {
                if (NativeIds.TryGetValue(entityId, out uint id))
                    return id;

                // Do not truncate or hash an EntityId: distinct objects must keep distinct IDs.
                // Zero is reserved by Pixyz for an unassigned native reference.
                if (nextId == uint.MaxValue)
                    throw new InvalidOperationException("The Pixyz object ID space is exhausted.");

                id = ++nextId;
                NativeIds.Add(entityId, id);
                return id;
            }
        }
    }
}
