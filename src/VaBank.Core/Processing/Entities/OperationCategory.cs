﻿using VaBank.Core.Common;

namespace VaBank.Core.Processing.Entities
{
    public class OperationCategory : Entity, IReferenceEntity
    {
    
        //TODO: children collection

        protected OperationCategory()
        {
        }

        public string Code { get; protected set; }

        public string Name { get; protected set; }

        public string Description { get; protected set; }
    }
}