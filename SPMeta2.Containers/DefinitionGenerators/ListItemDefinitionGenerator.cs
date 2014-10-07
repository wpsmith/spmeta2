﻿using System;
using SPMeta2.Containers.Services.Base;
using SPMeta2.Definitions;

namespace SPMeta2.Containers.DefinitionGenerators
{
    public class ListItemDefinitionGenerator : TypedDefinitionGeneratorServiceBase<ListItemDefinition>
    {
        public override DefinitionBase GenerateRandomDefinition(Action<DefinitionBase> action)
        {
            return WithEmptyDefinition(def =>
            {
                def.Title = Rnd.String();
            });
        }
    }
}