using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ACT.UndoRedo
{
    public interface ICommand<T>
    {
        void Do(T input);
        void Undo(T input);
    }
}