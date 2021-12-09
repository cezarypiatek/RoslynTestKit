﻿using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeActions;

namespace RoslynTestKit.CodeActionLocators
{
    public class ByIndexCodeActionSelector: ICodeActionSelector
    {

        private readonly int _index;

        public ByIndexCodeActionSelector(int index)
        {
            _index = index;
        }

        public CodeAction Find(IReadOnlyList<CodeAction> actions)
        {

            if (_index > actions.Count - 1)
            {
                return null;
            }
            return actions[_index];
        }

        public override string ToString()
        {
            return $"with index {_index}";
        }
    }
}
