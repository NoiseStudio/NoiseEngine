﻿using NoiseEngine.Components;
using NoiseEngine.Jobs;
using System;

namespace NoiseEngine.Physics.Internal;

internal sealed partial class RigidBodyInitializerSystem : EntitySystem {

    public RigidBodyInitializerSystem() {
        Filter = new EntityFilter(
            new Type[] { typeof(RigidBodyComponent) }, new Type[] { typeof(RigidBodyMiddleDataComponent) }
        );
    }

    private void OnUpdateEntity(
        Entity entity, SystemCommands commands, ref RigidBodyComponent rigidBody, TransformComponent transform
    ) {
        rigidBody.RecalculateProperties(entity);
        commands.GetEntity(entity).Insert(new RigidBodyMiddleDataComponent(transform.Position));
    }

}
